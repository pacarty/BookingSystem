import { useEffect, useState } from 'react';
import { api } from './api/client';
import Stepper from './components/Stepper';
import ServiceStep from './components/ServiceStep';
import ProviderStep from './components/ProviderStep';
import TimeStep from './components/TimeStep';
import DetailsStep from './components/DetailsStep';
import ConfirmationStep from './components/ConfirmationStep';

const STEP_INDEX = { service: 0, provider: 1, time: 2, details: 3, confirmed: 4 };

function todayIso() {
  return new Date().toISOString().slice(0, 10);
}

export default function App() {
  const [step, setStep] = useState('service');

  const [services, setServices] = useState([]);
  const [servicesLoading, setServicesLoading] = useState(true);
  const [servicesError, setServicesError] = useState(null);

  const [selectedService, setSelectedService] = useState(null);
  const [providers, setProviders] = useState([]);
  const [providersLoading, setProvidersLoading] = useState(false);
  const [providersError, setProvidersError] = useState(null);

  const [selectedProvider, setSelectedProvider] = useState(null);
  const [date, setDate] = useState(todayIso());
  const [slots, setSlots] = useState([]);
  const [slotsLoading, setSlotsLoading] = useState(false);
  const [slotsError, setSlotsError] = useState(null);
  const [selectedSlot, setSelectedSlot] = useState(null);

  const [submitting, setSubmitting] = useState(false);
  const [submitError, setSubmitError] = useState(null);
  const [appointment, setAppointment] = useState(null);

  // Load the service catalog once, on mount.
  useEffect(() => {
    api
      .getServices()
      .then(setServices)
      .catch((err) => setServicesError(err.message))
      .finally(() => setServicesLoading(false));
  }, []);

  // Load providers whenever a service is selected.
  useEffect(() => {
    if (!selectedService) return;
    setProvidersLoading(true);
    setProvidersError(null);
    api
      .getProvidersForService(selectedService.id)
      .then(setProviders)
      .catch((err) => setProvidersError(err.message))
      .finally(() => setProvidersLoading(false));
  }, [selectedService]);

  // Load slots whenever provider, service, or date changes.
  useEffect(() => {
    if (!selectedProvider || !selectedService || step !== 'time') return;
    setSlotsLoading(true);
    setSlotsError(null);
    setSelectedSlot(null);
    api
      .getAvailableSlots(selectedProvider.id, selectedService.id, date)
      .catch((err) => {
        setSlotsError(err.message);
        return [];
      })
      .then(setSlots)
      .finally(() => setSlotsLoading(false));
  }, [selectedProvider, selectedService, date, step]);

  const handleServiceSelect = (service) => {
    setSelectedService(service);
    setSelectedProvider(null);
    setStep('provider');
  };

  const handleProviderSelect = (provider) => {
    setSelectedProvider(provider);
    setStep('time');
  };

  const handleSubmitDetails = async (form) => {
    setSubmitting(true);
    setSubmitError(null);
    try {
      const result = await api.createAppointment({
        providerId: selectedProvider.id,
        serviceId: selectedService.id,
        clientFirstName: form.firstName,
        clientLastName: form.lastName,
        clientEmail: form.email,
        clientPhone: form.phone,
        startUtc: selectedSlot.startUtc,
        notes: form.notes || null,
      });
      setAppointment(result);
      setStep('confirmed');
    } catch (err) {
      // A 409 means someone else took the slot between viewing and submitting -
      // send the client back to the time step to pick again.
      if (err.status === 409) {
        setSubmitError(err.message);
        setStep('time');
      } else {
        setSubmitError(err.message);
      }
    } finally {
      setSubmitting(false);
    }
  };

  const handleBookAnother = () => {
    setSelectedService(null);
    setSelectedProvider(null);
    setSelectedSlot(null);
    setAppointment(null);
    setSubmitError(null);
    setDate(todayIso());
    setStep('service');
  };

  return (
    <div className="app-shell">
      <header className="app-header">
        <p className="eyebrow">Booking</p>
        <h1>Reserve your time</h1>
      </header>

      <div className="app-body">
        <Stepper currentIndex={STEP_INDEX[step]} />

        {step === 'service' && (
          <ServiceStep
            services={services}
            loading={servicesLoading}
            error={servicesError}
            onSelect={handleServiceSelect}
          />
        )}

        {step === 'provider' && selectedService && (
          <ProviderStep
            service={selectedService}
            providers={providers}
            loading={providersLoading}
            error={providersError}
            onSelect={handleProviderSelect}
            onBack={() => setStep('service')}
          />
        )}

        {step === 'time' && selectedService && selectedProvider && (
          <TimeStep
            service={selectedService}
            provider={selectedProvider}
            date={date}
            onDateChange={setDate}
            slots={slots}
            loading={slotsLoading}
            error={slotsError || submitError}
            selectedSlot={selectedSlot}
            onSelectSlot={setSelectedSlot}
            onBack={() => setStep('provider')}
            onContinue={() => setStep('details')}
          />
        )}

        {step === 'details' && selectedService && selectedProvider && selectedSlot && (
          <DetailsStep
            service={selectedService}
            provider={selectedProvider}
            slot={selectedSlot}
            submitting={submitting}
            error={submitError}
            onBack={() => setStep('time')}
            onSubmit={handleSubmitDetails}
          />
        )}

        {step === 'confirmed' && appointment && (
          <ConfirmationStep appointment={appointment} onBookAnother={handleBookAnother} />
        )}
      </div>
    </div>
  );
}
