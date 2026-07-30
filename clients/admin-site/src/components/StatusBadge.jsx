const STATUS = {
  0: { label: 'Requested', className: 'requested' },
  1: { label: 'Confirmed', className: 'confirmed' },
  2: { label: 'Attended', className: 'attended' },
  3: { label: 'No-show', className: 'noshow' },
  4: { label: 'Cancelled', className: 'cancelled' },
};

export function statusInfo(status) {
  return STATUS[status] ?? { label: 'Unknown', className: '' };
}

export default function StatusBadge({ status }) {
  const info = statusInfo(status);
  return <span className={`status-badge ${info.className}`}>{info.label}</span>;
}
