# Specimen Check-In (LIS)

A local full-stack prototype for receiving laboratory specimens against shipment manifests. The application has a Vue 3 frontend and an ASP.NET Core Web API backed by SQLite through Entity Framework Core.

> **Prototype scope:** This project uses synthetic seed data and a development-only `X-Lab-Id` request header to demonstrate tenant scoping. It is not a production clinical system and must not be used with real patient information.

## Contents

- [Features](#features)
- [Technology stack](#technology-stack)
- [Repository structure](#repository-structure)
- [Prerequisites](#prerequisites)
- [Run locally](#run-locally)
- [Tenant selection](#tenant-selection)
- [API endpoints](#api-endpoints)
- [Data model and workflow](#data-model-and-workflow)
- [Implementation decisions and assumptions](#implementation-decisions-and-assumptions)
- [Azure deployment topology (proposed)](#azure-deployment-topology-proposed)
- [Tenant isolation](#tenant-isolation)
- [HIPAA-aware data handling](#hipaa-aware-data-handling)
- [Testing and verification](#testing-and-verification)
- [Known limitations and next steps](#known-limitations-and-next-steps)

## Features

- List manifests belonging to the selected laboratory.
- Open a manifest and view its expected specimens and discrepancy records.
- Mark a pending specimen as received.
- Flag a pending specimen as missing and create a discrepancy.
- Resolve an open discrepancy.
- Close a manifest only when it has no pending specimens and no open discrepancies.
- Keep manifest reads and mutations scoped to the lab identified by the request context.
- Refresh the Vue view after successful actions and display loading, empty and error states.

## Technology stack

| Layer | Technology |
|---|---|
| Frontend | Vue 3, Vite, JavaScript, Axios, lucide-vue-next |
| Backend | ASP.NET Core Web API on .NET 8 |
| Data access | Entity Framework Core 8, code-first migrations |
| Database | SQLite (local development) |
| API format | JSON over REST |
| API documentation | Swagger / OpenAPI |

SQLite was selected to keep the prototype self-contained and quick to run locally. The assignment explicitly allows SQLite when it helps deliver the working slice quickly. The project is not deployed to Azure and does not require Azure resources or paid services.

## Repository structure

```text
specimen-checkin/
├── backend/
│   └── SpecimenCheckIn.Api/
│       ├── Controllers/
│       │   └── ManifestsController.cs
│       ├── Data/
│       │   ├── AppDbContext.cs
│       │   └── DbSeeder.cs
│       ├── Models/
│       │   ├── Lab.cs
│       │   ├── Manifest.cs
│       │   ├── Specimen.cs
│       │   └── Discrepancy.cs
│       ├── Migrations/
│       ├── appsettings.json
│       ├── Program.cs
│       └── SpecimenCheckIn.Api.csproj
├── frontend/
│   ├── src/
│   │   ├── components/
│   │   ├── services/
│   │   ├── App.vue
│   │   ├── main.js
│   │   └── style.css
│   ├── index.html
│   ├── package.json
│   └── vite.config.js
└── README.md
```

## Prerequisites

- .NET 8 SDK
- Node.js and npm
- Git

Check the installed tools:

```powershell
dotnet --version
node --version
npm --version
```

## Run locally

Run the backend and frontend in separate terminals.

### 1. Backend

```powershell
cd backend/SpecimenCheckIn.Api
dotnet restore
dotnet build
dotnet run --urls http://localhost:5000
```

At startup, the API applies the committed EF Core migrations and runs the seed routine. The SQLite connection is configured in `appsettings.json`; with the current local configuration, the database file is `specimen.db` in the backend application's working directory.

Swagger UI is available at:

- <http://localhost:5000/swagger>

OpenAPI document:

- <http://localhost:5000/swagger/v1/swagger.json>

### 2. Frontend

In a second terminal:

```powershell
cd frontend
npm install
npm run dev
```

Open the Vite URL printed in the terminal, normally:

- <http://localhost:5173>

The frontend's Axios service uses the Vite development proxy (configured in `vite.config.js`) to forward `/api` requests to `http://localhost:5000`. The API client sends the selected development tenant using the `X-Lab-Id` header.

### 3. Reset local seed data

The application persists changes in SQLite. `dotnet clean`, `dotnet build`, restarting the API, or refreshing the browser does **not** reset the database.

To recreate the local database from migrations and seed data:

1. Stop the backend with `Ctrl+C`.
2. From `backend/SpecimenCheckIn.Api`, remove the local SQLite database and any WAL/SHM sidecar files:

```powershell
Remove-Item .\specimen.db -Force -ErrorAction SilentlyContinue
Remove-Item .\specimen.db-shm -Force -ErrorAction SilentlyContinue
Remove-Item .\specimen.db-wal -Force -ErrorAction SilentlyContinue
```

3. Start the API again with `dotnet run --urls http://localhost:5000`.

The startup migration and seeding code should recreate the database. Do not delete the `Migrations` directory.

## Tenant selection

The seed data contains two labs. The current frontend is configured to send `X-Lab-Id: 1` (Lab Alpha). To test the other seeded lab, change the header value in `frontend/src/services/api.js` to `2`, save, and refresh the frontend.

The header is a **development tenant selector**, not authentication. A caller can change it, so it must not be trusted as an identity mechanism in production. The backend still applies tenant scoping to each manifest query and mutation so that a request for one lab cannot retrieve or mutate another lab's manifest through these endpoints.

## API endpoints

All endpoints require a valid positive `X-Lab-Id` header.

| Method | Route | Description |
|---|---|---|
| `GET` | `/api/manifests` | List manifests for the selected lab, including summary counts. |
| `GET` | `/api/manifests/{id}` | Return one lab-scoped manifest, specimens and discrepancy details. |
| `POST` | `/api/manifests/{id}/specimens/{sid}/receive` | Mark a pending specimen as received. Repeating the request for an already received specimen is idempotent. |
| `POST` | `/api/manifests/{id}/specimens/{sid}/flag` | Mark a pending specimen as flagged and create a missing-specimen discrepancy. Repeated flagging does not create another discrepancy. |
| `POST` | `/api/manifests/{id}/discrepancies/{did}/resolve` | Mark an open discrepancy as resolved. |
| `POST` | `/api/manifests/{id}/close` | Close the manifest only when there are no pending specimens and no open discrepancies. |

Swagger can be used to exercise these endpoints. Authorize with lab ID `1` or `2` using the `X-Lab-Id` API-key header definition.

## Data model and workflow

The core entities are:

- **Lab:** tenant boundary.
- **Manifest:** belongs to one lab and contains shipment specimens and discrepancies.
- **Specimen:** belongs to a manifest; its check-in status is `Pending`, `Received`, or `Flagged`.
- **Discrepancy:** belongs to a manifest and references the affected specimen; its status is `Open` or `Resolved`.

The receiving workflow is:

1. A technician selects a manifest and compares the expected list with the physical shipment.
2. The technician marks each physically present specimen as **Received**.
3. A specimen confirmed absent is marked **Flagged**, and the API creates a **Missing / Open** discrepancy.
4. The discrepancy is investigated and marked **Resolved** when the investigation is complete.
5. The manifest may be closed when no specimens remain `Pending` and no discrepancies remain `Open`.

A resolved discrepancy does not automatically turn a flagged specimen into a received specimen. The flagged state preserves the fact that it was reported missing. In a production workflow, the resolution outcome and notes should be captured as structured audit data.

## Implementation decisions and assumptions

- **SQLite:** Chosen over Azure SQL / SQL Server to make the prototype runnable locally without infrastructure or cost. EF Core migrations keep the schema reproducible.
- **Tenant context:** A positive `X-Lab-Id` header represents the current lab for this prototype. The API performs tenant checks server-side; the frontend does not implement isolation by merely hiding data.
- **Manifest close rule:** “Reconciled” is implemented as zero `Pending` specimens and zero `Open` discrepancies. A `Flagged` specimen can remain flagged after its discrepancy is resolved; this is intentional so that the missing history is not overwritten.
- **Discrepancy resolution:** The current endpoint changes the discrepancy status to `Resolved`. It does not capture a resolution category, notes, actor, or timestamp beyond any fields already present in the model.
- **Seed data:** The seeder creates two labs, manifests and synthetic specimens so that tenant isolation and the check-in workflow can be demonstrated. No real patient data is used.
- **API response shape:** Manifest detail returns summary counts separately from `specimens` and `discrepancyDetails`. The `discrepancies` summary property is a count; `discrepancyDetails` contains the records.
- **JSON responses:** Mutation endpoints return small response objects rather than EF Core entities, avoiding serialization cycles through bidirectional navigation properties.
- **No production login:** Login/OAuth was not implemented because the assignment permits a stubbed tenant context and explicitly says authentication infrastructure is not required.

## Azure deployment topology (proposed)

No Azure resources have been provisioned, in accordance with the assignment's instruction not to deploy or incur cloud costs. A production deployment could use:

```text
Technician's browser
      |
      | HTTPS
      v
Azure App Service (Vue static site or Static Web Apps)
      |
      | HTTPS / authenticated API call
      v
Azure App Service (ASP.NET Core Web API)
      |
      | Managed Identity + private network access
      v
Azure SQL Database

Optional, as product needs grow:
  Azure Blob Storage   -> controlled storage for permitted documents
  Azure Queue + Function -> asynchronous downstream processing
  Key Vault            -> secrets and key management
  Application Insights -> operational telemetry with PHI-safe settings
```

The browser would call the API over HTTPS. The API would authenticate the user, resolve the lab tenant from a trusted identity claim, enforce tenant filters on every read/write, and access Azure SQL using managed identity. Static frontend hosting and API hosting can be separated. Queues, Functions and Blob Storage should only be introduced for concrete asynchronous or file-storage needs; they are not required for this slice.

## Tenant isolation

Each manifest has a `LabId`. Every manifest read first scopes the query by both manifest ID and the current lab ID. Mutations first retrieve the manifest using the same tenant filter, then retrieve its specimen or discrepancy using the manifest relationship. Closing a manifest also loads it using the tenant-scoped query. This prevents a caller using Lab B's context from accessing Lab A's manifest through the implemented routes.

The isolation approach should be reinforced as the application grows by:

- Deriving the lab ID from a validated authentication claim rather than a caller-controlled header.
- Centralizing tenant context and query-filter conventions so new endpoints cannot omit the tenant predicate.
- Adding automated integration tests that create data for two labs and verify cross-tenant `GET`, `POST`, `resolve` and `close` requests cannot read or change the other lab's rows.
- Testing both successful same-tenant requests and cross-tenant negative cases.

## HIPAA-aware data handling

This prototype uses synthetic patient labels only. For a real deployment, I would:

- Enforce HTTPS/TLS for browser-to-API and service-to-database traffic; disable insecure production endpoints.
- Use Azure SQL encryption at rest (including backups), managed identities, least-privilege database access, and Key Vault for secrets and keys.
- Authenticate users and authorize actions by lab, role and least privilege; derive tenant context from trusted identity claims.
- Minimize patient data returned to the browser and never log patient names, specimen contents, access tokens, or other protected health information (PHI).
- Use structured audit events for sensitive reads and mutations, with actor, tenant, action, timestamp and record identifiers while avoiding PHI in event payloads.
- Apply appropriate retention, backup, monitoring, access review and incident-response controls, and validate the full system against applicable organizational and legal requirements before handling PHI.

This is a design discussion, not a claim that the prototype is HIPAA compliant or has undergone a compliance assessment.

## Testing and verification

The API can be manually exercised through Swagger or the frontend. Key manual checks include:

1. List manifests using Lab 1 and Lab 2; each response should contain only that lab's manifests.
2. Request a manifest belonging to Lab 1 while using Lab 2's header; expect `404 Not Found`.
3. Receive a pending specimen; repeat the same receive request and verify its status/count is not incremented twice.
4. Attempt to flag a received specimen; expect a structured client error.
5. Flag a pending specimen and verify a discrepancy is created.
6. Attempt to close with pending specimens or open discrepancies; expect a structured client error.
7. Resolve all open discrepancies, account for all pending specimens, and verify that the manifest can then be closed.

> **Automated tests still need to be added before submission.** The assignment explicitly asks for a couple of automated tests covering core reconciliation logic and tenant isolation. The next tests to add are (a) a manifest cannot close while pending specimens or open discrepancies remain, and can close once reconciled; and (b) Lab B cannot retrieve or mutate Lab A's manifest. Manual Swagger checks are useful but do not replace these automated tests.

## Known limitations and next steps

- Add automated backend tests for reconciliation rules, idempotent receiving and cross-tenant isolation.
- Replace the development `X-Lab-Id` header with a tenant context derived from authenticated identity claims in production.
- Add an explicit resolution outcome and investigation notes for discrepancies; preserve an audit trail of who resolved each issue and when.
- Consider handling off-manifest specimens as an optional discrepancy type if the workflow is extended.
- Add optimistic concurrency or transactional safeguards for simultaneous check-in actions.
- Improve accessibility and add a frontend component or end-to-end test if time allows.
- Use Azure SQL and the proposed Azure hosting topology for a production deployment; no Azure deployment is included in this assignment submission.

## Assignment scope note

The implementation focuses on the required vertical slice: manifest worklist and detail, receive, missing-specimen discrepancy, resolution, reconciliation-based closure, tenant scoping, EF Core migrations and synthetic seed data. The UI follows the supplied reference layout by eye rather than attempting a pixel-perfect reproduction. Optional stretch goals such as off-manifest specimens and an immutable audit log have not been implemented.
