export default function ProviderStep({ service, providers, loading, error, onSelect, onBack }) {
  return (
    <section className="step-panel">
      <h2>Who would you like to see?</h2>
      <p className="step-hint">Providers offering {service.name}.</p>

      {error && <div className="error-banner">{error}</div>}

      {loading ? (
        <div className="empty-state">Loading providers…</div>
      ) : providers.length === 0 ? (
        <div className="empty-state">No one currently offers this service. Try another one.</div>
      ) : (
        <div className="option-list">
          {providers.map((provider) => (
            <button key={provider.id} className="option-card" onClick={() => onSelect(provider)}>
              <div>
                <div className="name">{provider.name}</div>
                {provider.bio && <div className="meta">{provider.bio}</div>}
              </div>
            </button>
          ))}
        </div>
      )}

      <div className="actions-row">
        <button className="btn-secondary" onClick={onBack}>
          Back
        </button>
      </div>
    </section>
  );
}
