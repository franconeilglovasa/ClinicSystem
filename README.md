# ClinicSystem

ClinicSystem is a full-stack clinic and laboratory management system built with ASP.NET Core (.NET 10), Angular 17, SQLite, ASP.NET Identity + JWT, Tailwind CSS, and Ollama AI integration.

## Features

### Core Clinical Workflow
- Patient registration and profile management
- Visit queue management for daily patient flow
- Nurse vitals encoding (BP, HR, temperature, weight/height, SpO2, RR, BMI)
- Doctor visit view with consolidated patient context
- Medical history recording per visit
- Prescription creation with printable format
- Laboratory request creation and result entry
- Laboratory attachments upload and viewing (X-ray, ultrasound images, PDF, DICOM)

### Billing and Cashier
- Bill creation per visit
- Itemized billing (consultation, laboratory, procedure, medication, other)
- Payment recording and balance tracking
- Printable receipt support

### AI Clinical Suggestions (Ollama)
- AI suggestions generated from visit data, vitals, history, and lab findings
- Doctor-triggered AI assistance endpoint
- Configurable Ollama base URL and model via app settings

### Authentication and Authorization
- ASP.NET Identity user management
- JWT-based authentication
- Role-based access control for:
  - Admin
  - Doctor
  - Nurse
  - Laboratory
  - Cashier
- Admin user seeding on startup

### File Storage
- Local filesystem storage for lab attachments
- Organized path structure under `wwwroot/uploads/{year}/{month}`
- File validation with configurable size and allowed extensions

## Tech Stack
- Backend: ASP.NET Core .NET 10, EF Core, SQLite, ASP.NET Identity, JWT
- Frontend: Angular 17 (NgModule-based), Tailwind CSS
- AI: Ollama (`llama3` by default)

## Project Structure
- `ClinicSystem.Server`: ASP.NET Core Web API + Identity + EF Core
- `clinicsystem.client`: Angular frontend
- `ClinicSystem.slnx`: solution file

## Prerequisites
- .NET 10 SDK
- Node.js 18+ and npm
- Ollama installed locally (optional but required for AI suggestions)

## Configuration

### Backend configuration
Main config file: `ClinicSystem.Server/appsettings.json`

Key sections:
- `ConnectionStrings:DefaultConnection`: SQLite database file (`clinicsystem.db`)
- `JwtSettings`: token secret/issuer/audience/expiry
- `Ollama`: AI endpoint and model (default `http://localhost:11434`, `llama3`)
- `FileStorage`: upload path, file size limit, allowed extensions

## Default Seeded Admin Account
The backend ensures a default admin account exists at startup:
- Username/Email: `admin@clinicsystem.com`
- Password: `Admin@123`

The login page also displays these default credentials for convenience.

## Sample Seeder Data
On a fresh database, the backend also seeds realistic demo data so you can test all modules immediately.

### Seeded User Accounts
- Admin: `admin@clinicsystem.com` / `Admin@123`
- Doctor: `doctor@clinicsystem.com` / `Doctor@123`
- Nurse: `nurse@clinicsystem.com` / `Nurse@123`
- Laboratory: `lab@clinicsystem.com` / `Lab@123`
- Cashier: `cashier@clinicsystem.com` / `Cashier@123`

### Seeded Clinical Records
- 3 sample patients
- 2 sample visits:
  - 1 active workflow visit with full records (vitals, history, prescription, lab request + result, bill)
  - 1 waiting visit for queue testing
- 1 sample AI suggestion record

Seeder behavior:
- User accounts are ensured on every startup (idempotent role/account health checks).
- Clinical sample records are inserted only when there are no existing patients (prevents duplicate demo data).

## How to Run

### 1. Start Ollama (for AI features)
If you want AI suggestions enabled, run Ollama first.

```bash
ollama serve
```

In another terminal (first time only):

```bash
ollama pull llama3
```

### 2. Run Backend (API)
From the project root:

```bash
cd ClinicSystem.Server
dotnet restore
dotnet build
dotnet run
```

Notes:
- Database migrations are applied automatically on startup.
- Roles and the default admin user are seeded automatically.
- Backend URL in development is typically `https://localhost:7050`.

### 3. Run Frontend (Angular)
Open a new terminal from project root:

```bash
cd clinicsystem.client
npm install
npm start
```

Notes:
- Angular dev server uses the configured proxy to route `/api` requests to backend.
- Frontend URL in development is typically `https://localhost:56699`.

### 4. Login
Use the seeded credentials:
- Username: `admin@clinicsystem.com`
- Password: `Admin@123`

## Build Commands

### Backend
```bash
cd ClinicSystem.Server
dotnet build
```

### Frontend
```bash
cd clinicsystem.client
npm run build
```

## API and Development Notes
- OpenAPI is enabled in development.
- JWT is required for protected endpoints.
- Role guards are enforced both in backend APIs and frontend routing.

## Troubleshooting
- If AI suggestions fail, confirm Ollama is running and `llama3` is available.
- If frontend cannot reach API, ensure backend is running and proxy target matches backend URL.
- If login fails for seeded admin, restart backend to re-run seeding health checks.

## License
This project is free for non-commercial use.

Commercial use requires a separate license from Franco Neil Glovasa.

If you plan to use this system in any paid, revenue-generating, or business environment,
please contact the Franco Neil Glovasa to obtain a commercial license.

visit this website for more info:

https://franconeil.glovasa.org/
email: clinicsystem@glovasa.org
