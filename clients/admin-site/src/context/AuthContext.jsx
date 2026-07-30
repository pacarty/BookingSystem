import { createContext, useContext, useEffect, useState } from 'react';
import { api } from '../api/client';

const STORAGE_KEY = 'bookingSystemAdminSession';

// Storing the JWT in localStorage is a deliberate, common trade-off: it
// survives a page refresh, at the cost of being readable by any script
// that runs on this origin (XSS risk). An httpOnly cookie avoids that risk
// but requires same-site cookie plumbing between the API and this SPA. For
// a portfolio admin tool this is the right trade; worth being able to name
// the alternative and why it wasn't chosen here.
const AuthContext = createContext(null);

function loadStoredSession() {
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    if (!raw) return null;
    const session = JSON.parse(raw);
    if (new Date(session.expiresUtc) <= new Date()) return null; // expired
    return session;
  } catch {
    return null;
  }
}

export function AuthProvider({ children }) {
  const [session, setSession] = useState(loadStoredSession);

  useEffect(() => {
    if (session) {
      localStorage.setItem(STORAGE_KEY, JSON.stringify(session));
    } else {
      localStorage.removeItem(STORAGE_KEY);
    }
  }, [session]);

  const login = async (email, password) => {
    const result = await api.login(email, password);
    setSession(result);
    return result;
  };

  const logout = () => setSession(null);

  const value = {
    session,
    isAuthenticated: !!session,
    isAdmin: !!session?.roles?.includes('Admin'),
    isProvider: !!session?.roles?.includes('Provider'),
    login,
    logout,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth must be used within an AuthProvider');
  return ctx;
}
