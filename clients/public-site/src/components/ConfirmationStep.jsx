function formatSlot(startUtc, endUtc) {
  const start = new Date(startUtc);
  const end = new Date(endUtc);
  return `${start.toLocaleString([], { weekday: 'long', month: 'long', day: 'numeric' })}, ${start.toLocaleTimeString(
    [],
    { hour: '2-digit', minute: '2-digit' },
  )}–${end.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}`;
}

export default function ConfirmationStep({ appointment, onBookAnother }) {
  return (
    <section className="step-panel">
      <span className="confirmation-stamp">Confirmed</span>
      <h2>You're booked in</h2>
      <p className="step-hint">A confirmation has been sent to your email.</p>

      <div className="summary-card">
        <div className="row">
          <span className="label">Service</span>
          <span className="value">{appointment.serviceName}</span>
        </div>
        <div className="row">
          <span className="label">Provider</span>
          <span className="value">{appointment.providerName}</span>
        </div>
        <div className="row">
          <span className="label">When</span>
          <span className="value">{formatSlot(appointment.startUtc, appointment.endUtc)}</span>
        </div>
        <div className="row">
          <span className="label">Reference</span>
          <span className="value">{appointment.id.slice(0, 8)}</span>
        </div>
      </div>

      <button className="btn-secondary" onClick={onBookAnother}>
        Book another appointment
      </button>
    </section>
  );
}
