const STEPS = ['Service', 'Provider', 'Time', 'Your details', 'Confirmed'];

export default function Stepper({ currentIndex }) {
  return (
    <nav className="stepper" aria-label="Booking progress">
      {STEPS.map((label, i) => (
        <div key={label} className={`stepper-item ${i === currentIndex ? 'active' : ''}`}>
          <span className="num">{String(i + 1).padStart(2, '0')}</span>
          <span>{label}</span>
        </div>
      ))}
    </nav>
  );
}
