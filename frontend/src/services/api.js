import axios from 'axios';

// Lab 1 = Lab Alpha, Lab 2 = Lab Beta in the seeded development database.
// In production, tenant identity must come from authenticated server-side claims,
// not from a client-controlled header.
const api = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || '/api',
  headers: {
    'X-Lab-Id': import.meta.env.VITE_LAB_ID || '1',
    'Content-Type': 'application/json',
    Accept: 'application/json',
  },
  timeout: 15000,
});

export const getManifests = () => api.get('/manifests');
export const getManifestDetails = (manifestId) =>
  api.get(`/manifests/${manifestId}`);
export const receiveSpecimen = (manifestId, specimenId) =>
  api.post(`/manifests/${manifestId}/specimens/${specimenId}/receive`);
export const flagSpecimen = (manifestId, specimenId) =>
  api.post(`/manifests/${manifestId}/specimens/${specimenId}/flag`);
export const resolveDiscrepancy = (manifestId, discrepancyId) =>
  api.post(`/manifests/${manifestId}/discrepancies/${discrepancyId}/resolve`);
export const closeManifest = (manifestId) =>
  api.post(`/manifests/${manifestId}/close`);

export function getApiError(error) {
  const data = error?.response?.data;
  if (typeof data === 'string' && data.trim()) return data;
  return (
    data?.message ||
    data?.title ||
    error?.message ||
    'Something went wrong. Please try again.'
  );
}

export default api;
