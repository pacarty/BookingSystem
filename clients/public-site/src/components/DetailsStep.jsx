import { useState } from 'react';

function formatSlot(slot) {
  const start = new Date(slot.startUtc);
  return start.toLocaleString([], {
    weekday: 'short',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  });
}

export default function DetailsStep({ service, provider, slot, submitting, error, onBack, onSubmit }) {
  const [form, setForm] = useState({ firstName: '', lastName: '', email: '', phone: '', notes: '' });

  const update = (field) => (e) => setForm((f) => ({ ...f, [field]: e.target.value }));

  const handleSubmit = (e) => {
    e.preventDefault();
    onSubmit(form);
  };

  return (
    <section className="step-panel">
      <h2>Your details</h2>
      <p className="step-hint">We'll send your confirmation here.</p>

      <div className="summary-card">
        <div className="row">
          <span className="label">Service</span>
          <span className="value">{service.name}</span>
        </div>
        <div className="row">
          <span className="label">Provider</span>
          <span className="value">{provider.name}</span>
        </div>
        <div className="row">
          <span className="label">Time</span>
          <span className="value">{formatSlot(slot)}</span>
        </div>
      </div>

      {error && <div className="error-banner">{error}</div>}

      <form onSubmit={handleSubmit}>
        <div className="field-grid">
          <div className="field">
            <label htmlFor="firstName">First name</label>
            <input id="firstName" required value={form.firstName} onChange={update('firstName')} />
          </div>
          <div className="field">
            <label htmlFor="lastName">Last name</label>
            <input id="lastName" required value={form.lastName} onChange={update('lastName')} />
          </div>
        </div>

        <div className="field">
          <label htmlFor="email">Email</label>
          <input id="email" type="email" required value={form.email} onChange={update('email')} />
        </div>

        <div className="field">
          <label htmlFor="phone">Phone</label>
          <input id="phone" type="tel" required value={form.phone} onChange={update('phone')} />
        </div>

        <div className="field">
          <label htmlFor="notes">Notes (optional)</label>
          <textarea id="notes" rows={3} value={form.notes} onChange={update('notes')} />
        </div>

        <div className="actions-row">
          <button type="button" className="btn-secondary" onClick={onBack} disabled={submitting}>
            Back
          </button>
          <button type="submit" className="btn-primary" disabled={submitting}>
            {submitting ? 'Booking…' : 'Confirm booking'}
          </button>
        </div>
      </form>
    </section>
  );
}
