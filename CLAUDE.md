# SkillForge

## Before writing code
Write the tests TDD-style (red/green)

## Before running backend
docker compose up -d

## Package manager
Use bun, not npm/yarn

## Before committing
bun run lint && bun run typecheck && bun run test
dotnet format && dotnet test

## Autonomy
Operate autonomously without asking for confirmation. When working on a story:
- Create the feature branch, implement, commit, push and close the GitHub issue without prompting
- Assign issues to JelleMaes automatically
- Pull latest before starting work
- Fix failing pre-existing tests if encountered
- Do not ask for confirmation on git operations (branch, commit, push)
