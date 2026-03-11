# 🛠️ AI Bootcamp — Voorbereiding

Opzet SkillForge **vóór de workshop**. Duurt ongeveer **30 minuten**.
Problemen? Neem contact op met je coach.

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

## BMAD installeren

BMAD is het AI agent framework dat je tijdens de workshop zou kunnen gebruiken. Installeer het in de repository:

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
cd Itenium.SkillForge
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
