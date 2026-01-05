# Itenium.SkillForge

A learning management system built with .NET 10 and React.

## Project Structure

```
Itenium.SkillForge/
├── backend/                 # .NET 10.0 WebApi
│   ├── Itenium.SkillForge.Entities/    # Domain entities
│   ├── Itenium.SkillForge.Data/        # Database context and seeding
│   ├── Itenium.SkillForge.Services/    # Business logic
│   └── Itenium.SkillForge.WebApi/      # API controllers
├── frontend/                # React + Vite + TypeScript
└── README.md
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

| Username   | Password           | Role    | Organizations    |
|------------|-------------------|---------|------------------|
| central    | AdminPassword123! | central | All              |
| acme       | UserPassword123!  | local   | Acme Corp        |
| techstart  | UserPassword123!  | local   | TechStart Inc    |
| regional   | UserPassword123!  | local   | Acme + TechStart |
