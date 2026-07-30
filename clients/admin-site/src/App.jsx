import { useAuth } from './context/AuthContext';
import LoginForm from './components/LoginForm';
import Header from './components/Header';
import AppointmentBoard from './components/AppointmentBoard';

export default function App() {
  const { isAuthenticated } = useAuth();

  if (!isAuthenticated) {
    return <LoginForm />;
  }

  return (
    <div className="app-shell">
      <Header />
      <main className="main-content">
        <AppointmentBoard />
      </main>
    </div>
  );
}
