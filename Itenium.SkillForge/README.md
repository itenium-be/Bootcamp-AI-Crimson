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

### Backend

```bash
cd backend
dotnet restore
dotnet run --project Itenium.SkillForge.WebApi

# Or watch changes and rebuild+restart:
dotnet watch run --project Itenium.SkillForge.WebApi
```

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
| java       | UserPassword123!  | local      | Java            |
| dotnet     | UserPassword123!  | local      | .NET            |
| multi      | UserPassword123!  | local      | Java + .NET     |

## Teams

- Java
- .NET
- PO & Analysis
- QA
