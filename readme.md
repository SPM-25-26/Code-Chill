# Chill Code App

ASP.NET Core Web API + React frontend for the Chill Code project. Use this repo to implement and validate authentication/login and related flows.

## Backend (API) Quick Start

```bash
cd src/backend/chill-code-backend
dotnet build
dotnet run
```

Run backend tests:

```bash
dotnet test src/backend/chill-code-backend.Tests
```

The API exposes auth endpoints at `/api/auth` (login/register/refresh). Update `appsettings.json` or `appsettings.Development.json` (or env vars/user-secrets) to set JWT/DB/email settings.

Email flows:
- `POST /api/auth/register` sends a verification email (requires SMTP config).
- `POST /api/auth/verify` with `{ "email": "...", "token": "..." }` marks the email verified.
- `POST /api/test-email` sends a test message using the configured SMTP settings.

## Frontend (dev/test)

```bash
cd src/frontend
npm install      # first time only
npm start        # launches React dev server at http://localhost:3000
npm test         # optional: run UI tests if/when added
```

Run the backend in parallel so the UI can hit the API. Adjust the frontend API base URL in `src/frontend/src/utilities/Utilities.js` if needed.

## Study Project Links

This playground lives inside our Software Project Management (Scrum) study project, so keep the following references nearby:

- API for data ingestion – https://apispm.eppoi.io/swagger/index.html (used by the classroom exercises).
- Google Classroom – https://classroom.google.com/c/ODA5NzkwMTMwMzUx for announcements and sprint reviews.
- GitHub backlog – https://github.com/suderco/SPM hosts the user stories/issues we estimate and pull into each sprint.
- ZenHub workspace – open https://app.zenhub.com/ with the GitHub repo above to view the Kanban board and burndown chart we report on in class.

## Configuring Email (SMTP)

We send emails via SMTP using settings from the `Email` section in configuration. Keep secrets out of git—use environment variables or `dotnet user-secrets`.

### Option 1: Environment variables

Set these before running the backend:

```bash
export Email__SmtpHost="smtp.example.com"
export Email__SmtpPort="587"
export Email__UseStartTls="true"      # or false
export Email__SmtpUser="smtp-user"
export Email__SmtpPassword="smtp-pass"
export Email__FromEmail="no-reply@example.com"
export Email__FromName="ChillCode"
export Email__VerificationBaseUrl="https://app.example.com/verify"


unset Email__SmtpHost # for removing
```

Then run the API:

```bash
dotnet run --project src/backend/chill-code-backend
```

### Option 2: dotnet user-secrets (local dev only)

From the backend project folder:

```bash
cd src/backend/chill-code-backend
dotnet user-secrets init
dotnet user-secrets set "Email:SmtpHost" "smtp.example.com"
dotnet user-secrets set "Email:SmtpPort" "587"
dotnet user-secrets set "Email:UseStartTls" "true"
dotnet user-secrets set "Email:SmtpUser" "smtp-user"
dotnet user-secrets set "Email:SmtpPassword" "smtp-pass"
dotnet user-secrets set "Email:FromEmail" "no-reply@example.com"
dotnet user-secrets set "Email:FromName" "ChillCode"
dotnet user-secrets set "Email:VerificationBaseUrl" "https://app.example.com/verify"
```

`dotnet run` will read these secrets automatically in Development. Do not check real credentials into `appsettings*.json`.
