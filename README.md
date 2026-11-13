# FixIt AI

FixIt AI is a troubleshooting application in development. It saves a device problem description, can ask AI-generated follow-up questions and produce an uncertain assessment, and presents suggested basic checks one at a time. Google sign-in and private history are in development; component-level repair is not implemented.

## Requirements

- .NET 10 SDK
- Node.js and npm (developed with Node 24)
- PostgreSQL and a database/user with permission to create and modify tables
- `dotnet-ef` 10 for migrations

## Database and API

Create a PostgreSQL database and a dedicated user using your local PostgreSQL administration tools. Keep the credentials outside the repository. From `backend`, set a connection string in .NET user secrets (replace the values with your own):

```powershell
dotnet user-secrets set "ConnectionStrings:FixItAI" "Host=localhost;Port=5432;Database=<database>;Username=<user>;Password=<password>"
dotnet tool install --global dotnet-ef --version 10.0.0
dotnet ef database update
dotnet run
```

Alternatively, supply `ConnectionStrings__FixItAI` as an environment variable. The API listens at `http://localhost:5080` in the development launch profile; Swagger is at `/swagger`. The frontend origin allowed by CORS defaults to `http://localhost:5173`; set `FrontendOrigin` if it differs. The API can start without a database connection, but submissions return 503 until the connection and migration are configured.

### Neon

The repository includes `neon.ts`. After authenticating and linking a Neon branch, `neon deploy` writes the branch credentials to the ignored root `.env.local` (or `.env` if already present). In Development, the API reads `DATABASE_URL`, `NEON_AI_GATEWAY_BASE_URL`, and `NEON_AI_GATEWAY_TOKEN` from that file, unless the same variables are already in the environment. The PostgreSQL URL takes precedence over `ConnectionStrings:FixItAI`; it is converted for Npgsql with TLS certificate and hostname verification. For non-development environments, supply these variables through the hosting environment's secret manager; the API does not load the file there. Never commit these files or publish the credentials.

## Diagnostic provider

When Neon gateway settings are present, the backend uses its OpenAI-compatible Responses endpoint with `gpt-5-mini` by default. Neon reports that gateway model calls require prepaid credits, including on the free plan. Otherwise, set `OPENAI_API_KEY` in the backend environment or .NET user secrets to use OpenAI directly with `gpt-4o-mini`. The browser never receives either credential. `OpenAI__Model` optionally selects a compatible model. The user is notified before device details and answers are sent to the AI service. Requests can incur provider charges.

The backend protects a short-lived conversation ticket with ASP.NET Core Data Protection. Questions and answers are not saved to the database in this milestone, so an interrupted conversation must be restarted. Configure shared, durable Data Protection keys securely before using multiple instances or relying on ticket continuity across restarts.

The app uses conservative prompts and basic hazard checks, but automated screening is not comprehensive and AI output can be wrong or unsafe. Do not rely on it for hazardous repair advice. The in-process rate limit is a development safeguard, not a substitute for authentication and production abuse controls.

## Frontend

In a second terminal:

```powershell
cd frontend
npm install
Copy-Item .env.example .env.local
npm run dev
```

Open the local URL printed by Vite (usually `http://localhost:5173`). The development server proxies `/api` to `http://localhost:5080`, including when Vite selects another port. Keep the backend running in the first terminal. If you previously set `VITE_API_BASE_URL` in `frontend/.env.local`, remove it or leave it blank to use the proxy. For separate deployments, set it to the API origin and configure the backend's `FrontendOrigin`. Frontend configuration is public and must not contain secrets. Run `npm run build` for a production build and `dotnet build` in `backend` to build the API. On Windows PowerShell, use `npm.cmd` in place of `npm` if script execution is blocked.

## Google sign-in and history

Create a Google OAuth **web application** client and register `http://localhost:5080/signin-google` as an authorized redirect URI for local development. Store `Google:ClientId` and `Google:ClientSecret` in backend user secrets, or provide `Google__ClientId` and `Google__ClientSecret` in the backend environment. Never commit the client secret. Set `FrontendOrigin` for the actual frontend origin in production and register the production API's HTTPS `/signin-google` callback with Google. Apply the `AccountsAndHistory` migration before enabling account flows. The app stores a local Google account identifier and email for ownership, but no Google password. Existing anonymous sessions remain unowned and do not appear in a user's history.

Diagnostic results and guided-check progress are saved to a signed-in user's session. Follow-up questions and answers are not yet persisted, so an interrupted conversation before assessment must be restarted. No AI assessment is guaranteed, and generated checks require careful review; stop and seek professional help whenever a situation is hazardous or uncertain.

Account setup, access isolation, and Google sign-in must be verified end-to-end against a migrated database and configured Google OAuth client before public use. Do not store sensitive personal information while verification remains incomplete.
