import { useAuth } from '../context/AuthContext';

export default function Header() {
  const { session, isAdmin, logout } = useAuth();

  return (
    <header className="app-header">
      <div className="brand">
        <p className="eyebrow">Booking · Staff</p>
        <h1>Dashboard</h1>
      </div>
      <div className="session">
        <span className="email">{session.email}</span>
        <span className="role-pill">{isAdmin ? 'Admin' : 'Provider'}</span>
        <button className="btn-ghost" onClick={logout}>
          Sign out
        </button>
      </div>
    </header>
  );
}
