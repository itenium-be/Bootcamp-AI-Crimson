# 🛠️ AI Bootcamp — Voorbereiding

Installeer onderstaande tools **vóór de workshop**. Duurt ongeveer **30 minuten**.
Problemen? Neem contact op met je coach.

---

## Te installeren

| Tool           | Waarvoor                 | Controleer |
|----------------|--------------------------|------------|
| Git            | Versiebeheer             | `git --version` |
| Node.js LTS    | Frontend tooling         | `node --version` (v24+) |
| Bun            | Package manager frontend | `bun --version` |
| .NET 10 SDK    | Backend                  | `dotnet --version` (10.x) |
| Docker Desktop | PostgreSQL database      | `docker --version` |
| Claude Code    | AI coding agent          | `claude --version` |
| Github CLI     | Github Interactie        | `gh --version`     |
| IDE            | VS Code (aanbevolen) \| Cursor \| Antigravity (Google) | — |


Voer de commando's rechts uit in een terminal om te checken of iets al geïnstalleerd is.

---

## Installatie

Open **PowerShell als administrator** en voer onderstaande commando's uit:

```powershell
winget install Git.Git
winget install OpenJS.NodeJS.LTS  # of gebruik NVM!
winget install Oven-sh.Bun
winget install Microsoft.DotNet.SDK.10
winget install Docker.DockerDesktop
winget install GitHub.cli
gh auth login
```

**IDE (kies één):** (of skip)
```powershell
winget install Microsoft.VisualStudioCode
winget install Anysphere.Cursor
# Antigravity: https://ide.google.com
```

> Start Docker Desktop na installatie en wacht tot het groen is.
> Herstart je terminal na installatie zodat alle commando's beschikbaar zijn.


---

## Claude Code installeren

Je ontvangt later nog een mail met credentials met je subscription. Installeer Claude Code via:

```powershell
irm https://claude.ai/install.ps1 | iex
```

---

## Lokale Database

```powershell
docker image pull postgres:17
docker image pull mcr.microsoft.com/dotnet/sdk:10.0
docker image pull mcr.microsoft.com/dotnet/aspnet:10.0
```


---


## Repository klonen

```powershell
git clone xxx
cd SkillForge
```

## GitHub NuGet toegang instellen

De backend gebruikt GithHub NuGet packages. Je hebt een GitHub token nodig.

**Token aanmaken:**
1. Ga naar https://github.com/settings/tokens?type=beta
2. Genereer een nieuw token met **Packages: Read** permissie
3. Kopieer het token

**Token instellen** (vervang de placeholders):
```powershell
dotnet nuget update source itenium `
  --username JOUW_GITHUB_GEBRUIKERSNAAM `
  --password JOUW_TOKEN `
  --store-password-in-clear-text `
  --configfile backend/nuget.config
```

---

## Claude Code installeren

Je ontvangt een mail met credentials voor de team subscription. Installeer Claude Code via:

**Inloggen**:
```powershell
claude
```

Bij de eerste keer opstarten word je gevraagd om in te loggen. Kies **"Sign in with Claude.ai"** en gebruik de credentials uit de mail.

---

## BMAD installeren

BMAD is het AI agent framework dat we tijdens de workshop gebruiken. Installeer het in de repository:

```powershell
# Zorg dat je in de SkillForge map staat
cd SkillForge

bunx bmad-method install
```

**Doorloop de setup wizard:**
1. **MCP integration** → kies **Claude Code**
2. **Installation location** → kies **In this repository** (niet globaal)
3. **Modules** → selecteer enkel **Core** (deselect de rest)
4. Bevestig de installatie

Controleer of de installatie gelukt is:
```powershell
claude
```

BMAD-commando's zoals `/bmad` zouden nu beschikbaar moeten zijn in Claude Code.

---

## Alles testen

**Database starten:**
```powershell
docker compose up -d
```

**Backend starten:**
```powershell
cd backend
dotnet restore
dotnet run --project Itenium.SkillForge.WebApi
```
Controleer: http://localhost:5000/health/live → moet `Healthy` tonen.

**Frontend starten** (nieuw terminal venster):
```powershell
cd frontend
bun install
bun run dev
```
Controleer: http://localhost:5173 → je ziet de login pagina.

---

## Testgebruikers

| Gebruikersnaam | Wachtwoord | Rol |
|----------------|------------|-----|
| backoffice | AdminPassword123! | Admin |
| learner | UserPassword123! | Learner |

---

Alles werkt? Je bent klaar voor de workshop! 🎉
Loopt er iets mis? Neem dan **vóór de sessie** contact op met je coach.