const API_BASE = import.meta.env.VITE_API_BASE_URL || 'https://localhost:7100';

async function request(path, options) {
  const res = await fetch(`${API_BASE}${path}`, {
    headers: { 'Content-Type': 'application/json' },
    ...options,
  });

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
  getServices: () => request('/api/services'),
  getProvidersForService: (serviceId) => request(`/api/services/${serviceId}/providers`),
  getAvailableSlots: (providerId, serviceId, date) =>
    request(`/api/availability?providerId=${providerId}&serviceId=${serviceId}&date=${date}`),
  createAppointment: (payload) =>
    request('/api/appointments', { method: 'POST', body: JSON.stringify(payload) }),
};
