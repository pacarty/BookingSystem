export default function ServiceStep({ services, loading, error, onSelect }) {
  return (
    <section className="step-panel">
      <h2>What do you need booked?</h2>
      <p className="step-hint">Pick a service to see who offers it and when they're free.</p>

      {error && <div className="error-banner">{error}</div>}

      {loading ? (
        <div className="empty-state">Loading services…</div>
      ) : services.length === 0 ? (
        <div className="empty-state">No services are available to book right now.</div>
      ) : (
        <div className="option-list">
          {services.map((service) => (
            <button key={service.id} className="option-card" onClick={() => onSelect(service)}>
              <div>
                <div className="name">{service.name}</div>
                {service.description && <div className="meta">{service.description}</div>}
                <div className="meta">{service.durationMinutes} min</div>
              </div>
              <div className="price">${service.price.toFixed(2)}</div>
            </button>
          ))}
        </div>
      )}
    </section>
  );
}
