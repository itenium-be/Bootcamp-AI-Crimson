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

## Docker Images

```powershell
docker image pull postgres:17
docker image pull mcr.microsoft.com/dotnet/sdk:10.0
docker image pull mcr.microsoft.com/dotnet/aspnet:10.0
```