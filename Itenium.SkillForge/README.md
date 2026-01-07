Itenium.SkillForge
==================

A learning management system built with .NET 10 and React.

## Project Structure

```
Itenium.SkillForge/
├── backend/         # .NET 10.0 WebApi
└── frontend/        # React + Vite + TypeScript
```

## Prerequisites

### GitHub NuGet Authentication

This project uses private NuGet packages from GitHub Packages. You need to authenticate before running `dotnet restore`.

#### Step 1: Create a Personal Access Token (PAT)

1. Go to https://github.com/settings/tokens?type=beta
2. Click **Generate new token**
3. Give it a name (e.g., "NuGet packages")
4. Set expiration (e.g., 90 days)
5. Under **Repository access**, select "Public Repositories (read-only)"
6. Under **Permissions** → **Account permissions** → **Packages**, select **Read**
7. Click **Generate token**
8. Copy the token (you won't see it again!)

#### Step 2: Configure NuGet

Run this command (replace `YOUR_GITHUB_USERNAME` and `YOUR_PAT`):

```bash
dotnet nuget update source itenium \
  --username YOUR_GITHUB_USERNAME \
  --password YOUR_PAT \
  --store-password-in-clear-text \
  --configfile backend/nuget.config
```

This only needs to be done once. The credentials are stored in your user-level NuGet config.

## Getting Started

### PostgreSQL Database

Start PostgreSQL using Docker:

```bash
docker compose up -d
```

This starts a PostgreSQL container with:
- **Host:** localhost
- **Port:** 5432
- **Database:** skillforge
- **Username:** skillforge
- **Password:** skillforge

### Backend

```bash
cd backend
dotnet restore
dotnet run --project Itenium.SkillForge.WebApi

# Or watch changes and rebuild+restart:
dotnet watch run --project Itenium.SkillForge.WebApi
```

Migrations run automatically at startup.

- [API at :5000](http://localhost:5000)
- [Swagger](http://localhost:5000/swagger)
  - Run `.\Get-Token.ps1` to create a JWT
- Health
  - [Live](http://localhost:5000/health/live)
  - [Ready](http://localhost:5000/health/ready)


### Frontend

```bash
cd frontend
bun install
bun run dev
```

The frontend will be available at http://localhost:5173

## Test Users

| Username   | Password          | Role       | Teams           |
|------------|-------------------|------------|-----------------|
| backoffice | AdminPassword123! | backoffice | All             |
| java       | UserPassword123!  | manager    | Java            |
| dotnet     | UserPassword123!  | manager    | .NET            |
| multi      | UserPassword123!  | manager    | Java + .NET     |
| learner    | UserPassword123!  | learner    | -               |


## Database Migrations

Migrations run automatically at startup. To create new migrations after modifying entities:

```bash
cd backend

# Add a new migration
dotnet ef migrations add <MigrationName> \
  --project Itenium.SkillForge.Data \
  --startup-project Itenium.SkillForge.WebApi \
  --output-dir Migrations

# Remove the last migration (if not yet applied)
dotnet ef migrations remove \
  --project Itenium.SkillForge.Data \
  --startup-project Itenium.SkillForge.WebApi

# Generate SQL script for all migrations
dotnet ef migrations script \
  --project Itenium.SkillForge.Data \
  --startup-project Itenium.SkillForge.WebApi \
  --output migrations.sql
```

## Running Tests

### Backend Tests

Tests use Testcontainers to spin up a PostgreSQL container automatically:

```bash
cd backend
dotnet test
```


### E2E Tests

E2E tests use Playwright and Testcontainers to spin up both PostgreSQL and the backend:

```bash
cd frontend

# Option 1: Use Docker (full e2e setup)
# Set environment variables for GitHub Packages authentication:
$env:NUGET_USER="your-github-username"
$env:NUGET_TOKEN="your-github-pat-with-read:packages"
bun run test:e2e

# Option 2: Use locally running backend (faster for development)
# Start the backend first, then:
bun run test:e2e:local

# Other test commands:
bun run test:e2e:ui        # Run with Playwright UI
bun run test:e2e:headed    # Run with visible browser
bun run test:e2e:debug     # Debug mode
```
