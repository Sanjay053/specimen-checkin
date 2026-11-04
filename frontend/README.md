# Specimen Check-In Frontend

Vue 3 + Vite frontend for the Specimen Check-In LIS assignment.

## Requirements
- Node.js installed
- ASP.NET Core API running at `http://localhost:5000`

## Install and run
```powershell
npm install
npm run dev
```
Open http://localhost:5173.

The Vite development server proxies `/api` requests to `http://localhost:5000`. The API client sends `X-Lab-Id: 1` by default (Lab Alpha). You can override these with a local `.env` file:

```env
VITE_API_BASE_URL=/api
VITE_LAB_ID=1
```

Do not use a client-controlled tenant header as a production security boundary. Production tenant identity should be derived from authenticated server-side claims.

## Build
```powershell
npm run build
```

## Backend routes used
- `GET /api/manifests`
- `GET /api/manifests/{id}`
- `POST /api/manifests/{id}/specimens/{sid}/receive`
- `POST /api/manifests/{id}/specimens/{sid}/flag`
- `POST /api/manifests/{id}/discrepancies/{did}/resolve`
- `POST /api/manifests/{id}/close`

The detail response normalizer supports either a root-level manifest object or a response wrapped in `manifest`, with `specimens` and `discrepancies` arrays at either level.
