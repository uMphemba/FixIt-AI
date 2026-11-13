# FixIt AI

FixIt AI is a troubleshooting application in development. This foundation accepts and saves a device problem description; it does not provide diagnoses or repair instructions yet.

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

## Frontend

In a second terminal:

```powershell
cd frontend
npm install
Copy-Item .env.example .env.local
npm run dev
```

Open `http://localhost:5173`. `VITE_API_BASE_URL` sets the API origin and defaults to `http://localhost:5080`. The frontend configuration is public and must not contain secrets. Run `npm run build` for a production build and `dotnet build` in `backend` to build the API.

This milestone has no authentication or history interface. Do not use it to store sensitive personal information or expose it publicly until identity, access control, and privacy protections are implemented.
