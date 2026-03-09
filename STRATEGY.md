# SkillForge – AI Agent Strategy

Practical guide for working with Claude Code on this project: workflows, skills, and MCP servers.

---

## Workflows

### 1. GitHub Issue → Implementation (daily default)

The standard loop for picking up work from the backlog.

```
1. Read the issue:          "implement issue #42"
2. Claude creates branch:   feature/42-short-description
3. Clarify acceptance criteria with Claude if needed
4. Invoke tdd-guide:        /tdd-guide implement <acceptance criteria>
5. Pre-commit checks run automatically (lint, typecheck, test)
6. Create PR:               /pr  (links issue automatically)
```

**Tip:** Tell Claude the issue number and it will read the issue via the GitHub MCP server (see below), extract acceptance criteria, and start the TDD cycle — no copy-pasting required.

---

### 2. New Feature (full BMAD cycle)

For larger features that need design before coding.

```
1. /bmad-bmm-quick-spec     → Quick tech spec from requirements
2. /bmad-bmm-create-story   → Story file with full context
3. /bmad-bmm-dev-story      → Implementation (invokes tdd-guide internally)
4. /pr                      → PR with AI code review
```

For complex features spanning multiple stories:
```
1. /bmad-bmm-create-prd         → Full PRD
2. /bmad-bmm-create-architecture → Architecture decisions
3. /bmad-bmm-create-epics-and-stories → Break into epics/stories
4. /bmad-bmm-sprint-planning    → Sprint plan
5. Per story: /bmad-bmm-dev-story → /pr
```

---

### 3. Bug Fix

```
1. Read issue / reproduce the bug
2. Write a failing test that captures the bug (red)
3. Fix the minimum code to make it pass (green)
4. /pr linking the issue
```

Do NOT reach for the debugger first — a failing test is both the reproduction and the regression guard.

---

### 4. Code Review

```
1. /pr                          → Creates PR and posts AI review automatically
2. /bmad-bmm-code-review        → Deeper adversarial review (optional, for critical paths)
3. /bmad-review-adversarial-general → Cynical review of architecture/design decisions
```

---

### 5. Sprint Ceremonies

```
/bmad-bmm-sprint-planning   → Generate sprint plan from epics
/bmad-bmm-sprint-status     → Current sprint health + risks
/bmad-bmm-retrospective     → Post-sprint retrospective
/bmad-bmm-correct-course    → Mid-sprint scope changes
```

---

## Skills

### Available (pre-installed)

| Skill | When to use |
|---|---|
| `tdd-guide` | Implementing any feature or fix — drives red-green-refactor |
| `pr` | Creating PRs with AI code review and line comments |
| `bmad-bmm-quick-spec` | Quick tech spec for small features |
| `bmad-bmm-create-story` | Create a story file from a spec |
| `bmad-bmm-dev-story` | Implement a story (uses tdd-guide) |
| `bmad-bmm-code-review` | Adversarial code review |
| `bmad-bmm-sprint-planning` | Sprint planning from epics |
| `bmad-bmm-sprint-status` | Sprint health check |
| `bmad-tea-testarch-atdd` | Write acceptance tests upfront |
| `bmad-tea-testarch-framework` | Initialize Playwright/Vitest |
| `bmad-bmm-create-architecture` | Architecture decisions (ADRs) |
| `bmad-brainstorming` | Ideation sessions |

---

### Custom Skills to Add

These fill gaps in the current workflow. Add them as files in `.claude/commands/`.

#### `github-issue.md` — Read issue and prepare implementation context

```markdown
# GitHub Issue → Implementation Context

Given an issue number:
1. Fetch the issue via GitHub MCP (title, description, labels, comments)
2. Extract acceptance criteria (or derive them from the description)
3. Suggest a branch name: feature/<number>-<slug> or fix/<number>-<slug>
4. Create the branch
5. Summarize: what needs to be built, what the definition of done is
6. Invoke tdd-guide with the acceptance criteria
```

Usage: `implement issue #42`

---

#### `done.md` — Definition of Done checklist before committing

```markdown
# Definition of Done

Before committing, verify:
1. Run: bun run lint && bun run typecheck && bun run test
2. Run: dotnet format && dotnet test
3. Check: no dead code left behind
4. Check: no secrets or PII in code or tests
5. Check: new behaviour has tests
6. Check: domain model has no infrastructure dependencies
7. If frontend: accessibility checked, Playwright test for the user journey
8. Summarize what was done and suggest a commit message
```

Usage: `/done` or "run done checklist"

---

#### `adr.md` — Create an Architecture Decision Record

```markdown
# Architecture Decision Record

Create an ADR for a significant design decision:
1. Ask: what is the decision, what alternatives were considered?
2. Document: context, decision, alternatives, consequences, date
3. Save to: docs/adr/NNNN-<slug>.md
```

Usage: `/adr we chose Zustand over Redux because...`

---

## MCP Servers

No MCP servers are currently configured. These are the highest-value additions for this project.

### 1. GitHub MCP ⭐ (highest priority)

Enables Claude to read issues, create PRs, post comments, and manage labels — without needing `gh` CLI installed.

```bash
# Install
npx -y @modelcontextprotocol/server-github

# Add to ~/.claude/settings.json:
{
  "mcpServers": {
    "github": {
      "command": "npx",
      "args": ["-y", "@modelcontextprotocol/server-github"],
      "env": {
        "GITHUB_PERSONAL_ACCESS_TOKEN": "<your-token>"
      }
    }
  }
}
```

Token needs: `repo`, `pull_requests`, `issues` scopes.

**Unlocks:** `implement issue #42` → Claude reads the issue, creates branch, drives TDD, creates PR — all in one flow.

---

### 2. Playwright MCP ⭐

Enables Claude to open a browser, interact with the running app, and take screenshots — useful for verifying UI behaviour during development and for writing Playwright tests against the real app.

```bash
# Add to ~/.claude/settings.json:
{
  "mcpServers": {
    "playwright": {
      "command": "npx",
      "args": ["-y", "@playwright/mcp@latest"]
    }
  }
}
```

**Unlocks:** "open the app and verify the login flow works" — Claude drives the browser and reports back. Also helps write accurate Playwright selectors by inspecting the live DOM.

---

### 3. PostgreSQL MCP

Enables Claude to query the database directly during development — useful for verifying migrations, inspecting data, and debugging.

```bash
# Add to ~/.claude/settings.json:
{
  "mcpServers": {
    "postgres": {
      "command": "npx",
      "args": ["-y", "@modelcontextprotocol/server-postgres", "postgresql://localhost/skillforge"]
    }
  }
}
```

**Unlocks:** "show me all courses with no lessons" or "check if the migration ran correctly" — without leaving the editor.

---

### Priority

| MCP | Impact | Effort |
|---|---|---|
| GitHub | High — closes the issue→PR loop | Low |
| Playwright | High — E2E test authoring & verification | Low |
| PostgreSQL | Medium — dev-time debugging | Low |
