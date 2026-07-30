import { useCallback, useEffect, useState } from 'react';
import { api } from '../api/client';
import { useAuth } from '../context/AuthContext';
import StatusBadge from './StatusBadge';

const AppointmentStatus = { Requested: 0, Confirmed: 1, Attended: 2, NoShow: 3, Cancelled: 4 };

function formatDateTime(iso) {
  const d = new Date(iso);
  return {
    date: d.toLocaleDateString([], { weekday: 'short', month: 'short', day: 'numeric' }),
    time: d.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }),
  };
}

// What actions make sense from each status - keeps the UI from offering a
// transition the API would reject anyway (e.g. re-confirming something
// already marked Attended).
function actionsFor(status) {
  switch (status) {
    case AppointmentStatus.Requested:
      return [
        { label: 'Confirm', next: AppointmentStatus.Confirmed, primary: true },
        { label: 'Cancel', next: AppointmentStatus.Cancelled },
      ];
    case AppointmentStatus.Confirmed:
      return [
        { label: 'Mark attended', next: AppointmentStatus.Attended, primary: true },
        { label: 'No-show', next: AppointmentStatus.NoShow },
        { label: 'Cancel', next: AppointmentStatus.Cancelled },
      ];
    default:
      return [];
  }
}

export default function AppointmentBoard() {
  const { session, isAdmin } = useAuth();
  const [appointments, setAppointments] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [updatingId, setUpdatingId] = useState(null);

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const data = isAdmin
        ? await api.getAllAppointments(session.token)
        : await api.getMyAppointments(session.token);
      setAppointments(data.sort((a, b) => new Date(a.startUtc) - new Date(b.startUtc)));
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  }, [isAdmin, session.token]);

  useEffect(() => {
    load();
  }, [load]);

  const handleStatusChange = async (id, next) => {
    setUpdatingId(id);
    setError(null);
    try {
      await api.updateAppointmentStatus(id, next, session.token);
      await load(); // simplest correct approach here - re-fetch rather than guess the server's resulting state
    } catch (err) {
      setError(err.message);
    } finally {
      setUpdatingId(null);
    }
  };

  return (
    <div>
      <div className="dashboard-head">
        <h2>{isAdmin ? 'All appointments' : 'Your appointments'}</h2>
        <span className="count">next 30 days · {appointments.length}</span>
      </div>

      {error && <div className="error-banner">{error}</div>}

      {loading ? (
        <div className="empty-state">Loading…</div>
      ) : appointments.length === 0 ? (
        <div className="empty-state">Nothing on the board yet.</div>
      ) : (
        <div className="board">
          {appointments.map((appt) => {
            const { date, time } = formatDateTime(appt.startUtc);
            return (
              <div className="board-row" key={appt.id}>
                <div className="datetime">
                  <div className="date">{date}</div>
                  <div>{time}</div>
                </div>
                <div className="details">
                  <div className="client">{appt.clientName}</div>
                  <div className="service">
                    {appt.serviceName}
                    {isAdmin ? ` · ${appt.providerName}` : ''}
                  </div>
                </div>
                <div className="actions">
                  <StatusBadge status={appt.status} />
                  {actionsFor(appt.status).map((action) => (
                    <button
                      key={action.label}
                      className={`action-btn ${action.primary ? 'primary' : ''}`}
                      disabled={updatingId === appt.id}
                      onClick={() => handleStatusChange(appt.id, action.next)}
                    >
                      {action.label}
                    </button>
                  ))}
                </div>
              </div>
            );
          })}
        </div>
      )}
    </div>
  );
}
