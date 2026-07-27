function formatTime(iso) {
  return new Date(iso).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
}

function formatDuration(startIso, endIso) {
  const mins = Math.round((new Date(endIso) - new Date(startIso)) / 60000);
  return `${mins} min`;
}

export default function TimeStep({
  provider,
  service,
  date,
  onDateChange,
  slots,
  loading,
  error,
  selectedSlot,
  onSelectSlot,
  onBack,
  onContinue,
}) {
  return (
    <section className="step-panel">
      <h2>Choose a time</h2>
      <p className="step-hint">
        {service.name} with {provider.name}
      </p>

      <input
        type="date"
        className="date-input"
        value={date}
        min={new Date().toISOString().slice(0, 10)}
        onChange={(e) => onDateChange(e.target.value)}
      />

      {error && <div className="error-banner">{error}</div>}

      {loading ? (
        <div className="empty-state">Checking the board…</div>
      ) : slots.length === 0 ? (
        <div className="empty-state">Nothing open this day. Try another date.</div>
      ) : (
        <div className="board" role="list" aria-label="Available time slots">
          {slots.map((slot, i) => {
            const isSelected = selectedSlot?.startUtc === slot.startUtc;
            return (
              <button
                key={slot.startUtc}
                role="listitem"
                className={`board-row ${isSelected ? 'selected' : ''}`}
                style={{ animationDelay: `${Math.min(i, 12) * 25}ms` }}
                onClick={() => onSelectSlot(slot)}
              >
                <span className="time">{formatTime(slot.startUtc)}</span>
                <span className="duration">{formatDuration(slot.startUtc, slot.endUtc)}</span>
                <span className="status">{isSelected ? 'Selected' : 'Open'}</span>
              </button>
            );
          })}
        </div>
      )}

      <div className="actions-row" style={{ marginTop: 24 }}>
        <button className="btn-secondary" onClick={onBack}>
          Back
        </button>
        <button className="btn-primary" disabled={!selectedSlot} onClick={onContinue}>
          Continue
        </button>
      </div>
    </section>
  );
}
