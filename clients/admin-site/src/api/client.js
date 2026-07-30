const API_BASE = import.meta.env.VITE_API_BASE_URL || 'https://localhost:7100';

async function request(path, { token, ...options } = {}) {
  const headers = { 'Content-Type': 'application/json' };
  if (token) headers.Authorization = `Bearer ${token}`;

  const res = await fetch(`${API_BASE}${path}`, { headers, ...options });

  if (!res.ok) {
    const body = await res.json().catch(() => null);
    const error = new Error(body?.error || `Request to ${path} failed with ${res.status}`);
    error.status = res.status;
    throw error;
  }

  if (res.status === 204) return null;
  return res.json();
}

export const api = {
  login: (email, password) =>
    request('/api/auth/login', { method: 'POST', body: JSON.stringify({ email, password }) }),

  getMyAppointments: (token) => request('/api/appointments/mine', { token }),

  getAllAppointments: (token) => request('/api/appointments', { token }),

  updateAppointmentStatus: (id, status, token) =>
    request(`/api/appointments/${id}/status`, {
      method: 'PATCH',
      token,
      body: JSON.stringify({ status }),
    }),
};
