# SkillForge

## Before running backend
docker compose up -d

## Package manager
Use bun, not npm/yarn

## Before committing
bun run lint && bun run typecheck && bun run test
dotnet format && dotnet test
