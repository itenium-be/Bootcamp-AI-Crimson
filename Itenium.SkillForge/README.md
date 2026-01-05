Itenium.SkillForge
==================

A learning management system built with .NET 10 and React.

## Project Structure

```
Itenium.SkillForge/
├── backend/         # .NET 10.0 WebApi
└── frontend/        # React + Vite + TypeScript
```

## Getting Started

### Backend

```bash
cd backend
dotnet restore
dotnet run --project Itenium.SkillForge.WebApi
```

The API will be available at http://localhost:5000

### Frontend

```bash
cd frontend
npm install
npm run dev
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
