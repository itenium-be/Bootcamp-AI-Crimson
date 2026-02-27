---
stepsCompleted: [1, 2, 3]
inputDocuments: ['Bootcamp-AI.pptx']
session_topic: 'AI Bootcamp Preparation - Logistics, Experience & Risk Mitigation'
session_goals: 'Ensure a successful bootcamp day: smooth logistics, engaged participants, and contingency plans for what could go wrong'
selected_approach: 'ai-recommended'
techniques_used: ['Reverse Brainstorming', 'Role Playing', 'Constraint Mapping']
ideas_generated: ['Risk #1-10', 'Persona insights x8', 'Constraint map', 'Action checklist']
context_file: ''
session_continued: true
continuation_date: '2026-02-25'
session_status: 'complete'
---

# Brainstorming Session Results

**Facilitator:** Wouter
**Date:** 2026-02-06

## Session Overview

**Topic:** AI-assisted development quality process -- strategies, guardrails, and workflows to ensure AI-generated code in SkillForge meets quality standards
**Goals:** Brainstorm approaches beyond linting and testing to maintain code quality when AI is doing the heavy lifting during bootcamp development

### Session Setup

_Focus narrowed from full SkillForge product ideation to the AI code quality process dimension. Existing infrastructure includes .NET backend with layered architecture (Entities, Services, Data, WebApi + tests) and React frontend with established tooling. Linting and testing already in place as first line of defense._

## Technique Selection

**Approach:** AI-Recommended Techniques
**Analysis Context:** AI-assisted development quality process with focus on practical guardrails and workflows

**Recommended Techniques:**

- **Five Whys:** Root cause analysis of why AI-generated code fails quality standards -- uncover the underlying drivers before designing solutions
- **Chaos Engineering:** Stress-test the quality process by imagining deliberate failures -- find blind spots in existing linting+testing safety net
- **SCAMPER Method:** Systematically improve existing quality guardrails through 7 lenses (Substitute, Combine, Adapt, Modify, Put to other uses, Eliminate, Reverse)

**AI Rationale:** This sequence progresses from understanding (why does AI code fail?) through stress-testing (where are the gaps?) to systematic improvement (how do we upgrade the process?). Mixes deep analytical and structured approaches for comprehensive coverage.

---

## Session Pivot (2026-02-25)

**New Focus:** AI Bootcamp Preparation — Logistics, Participant Experience & Risk Mitigation

**Context Loaded:** Bootcamp-AI.pptx (22 slides)

**Bootcamp Summary:**
- **Date:** March 13, 2026
- **Schedule:** 9h start → 9h30 dev → 12h dinner → 13h demos → 13h30 dev → 16h final demos → 16h45 winners → 17h drinks
- **Product:** SkillForge (itenium knowledge matrix / CC growth paths)
- **Tech:** .NET 10 + React, BMAD optional, Docker/Postgres
- **Teams:** Self-organizing, own codebase + backlog, POs: Olivier/Michael/Bert
- **Winning:** Decided by POs, likely deployed as real itenium SkillForge

**Updated Techniques:**
1. **Reverse Brainstorming** — "How could we make this bootcamp fail?" → surfaces hidden risks
2. **Role Playing** — Embody different participant types to stress-test experience
3. **Constraint Mapping** — Map all logistics constraints and find pathways

---

## Technique 1: Reverse Brainstorming

**Prompt:** "How could we make this AI Bootcamp fail spectacularly?"

### Risks Identified

**[Risk #1] Claude token/rate limits exhausted**
- Individual or group hits rate limits mid-sprint
- Parallel worktrees = 2-3x token burn
- **Mitigation:** Max subscriptions ($100-200/mo), fallback pairing

**[Risk #2] Team paralysis — don't know where to start**
- Analysis paralysis on BMAD vs YOLO
- No one takes initiative
- Junior-heavy team intimidated

**[Risk #3] Self-organization fails**
- Everyone vibe-codes same feature
- No ownership emerges
- One voice dominates, others disengage

**[Risk #4] Merge conflict hell**
- No branching strategy, git chaos at demo time
- Force-push destroys work
- "I can't push!" at 12h55

**[Risk #5] Laptop failure**
- **Mitigated:** Pair programming fallback

**[Risk #6] Docker/infra gremlins**
- Port conflicts, WSL memory, disk space, old versions
- **Mitigated:** Prep.md verified days before bootcamp

**[Risk #7] People aren't having fun**
- Frustration loops fighting AI hallucinations
- Boredom waiting for generation
- Impostor syndrome ("everyone else is shipping")
- Social isolation (headphones, no interaction)
- Competition anxiety kills experimentation

**[Risk #8] Waiting for AI — dead time**
- Git worktree helps but requires context-switch
- **Ideas:** Parallel Claude windows, pair rotation, smaller prompts, background agents
- **Skill idea:** `/worktree` scaffold skill

**[Risk #9] BMAD prep level wrong**
- Too little = chaos; too much = no discovery
- **Decision:** PRD + Architecture visible, Epics on hidden branch as escape hatch
- Time-boxed discovery: first commit by 10h30

**[Risk #10] Discovery takes too long**
- Team debates until 11h30, demos nothing
- **Mitigations:** Hard timebox, PO check-in at 10h, "one button = valid demo"

---

## Technique 2: Role Playing

**Prompt:** Embody different participant types. What does the bootcamp feel like for each persona?

### Persona Walkthrough

**The Junior (1-2 yrs)**
- Risk: Passenger syndrome, impostor spiral
- Needs: Explicit first task, pair with senior, permission to ask, early small win

**The Senior Architect (15 yrs)**
- Risk: Quality frustration, bites tongue on AI slop
- Needs: Reviewer/architect role, permission to NOT vibe-code, "ship clean" challenge

**The AI Enthusiast**
- Risk: Alienates team by hogging keyboard
- Needs: Coach role (teach, don't show off), reminder that bonding > shipping

**The AI Skeptic**
- Risk: Disengaged cynic, "told you so" mode
- Needs: QA/breaker role, valued doubt, let them be right sometimes

**The Introvert**
- Risk: Invisible contributor, unrecognized work
- Needs: Async contribution paths, quiet space, pair > mob

**The Competitor**
- Risk: Cuts corners, no tests, toxic if loses
- Needs: Clear judging criteria upfront, quality counts for win

**The Stack Outsider (Java/PO)**
- Risk: Lost tourist, can't debug
- Needs: Domain expert role, pair with stack expert, non-code tasks

**The Socializer**
- Risk: Low technical output
- Needs: Morale/glue role is valid, demo presenter, snack coordinator

### Summary Table

| Persona | Biggest Risk | Key Mitigation |
|---------|--------------|----------------|
| Junior | Passenger syndrome | Assigned first task, pair up |
| Senior | Quality frustration | Reviewer role, architecture ownership |
| Enthusiast | Alienates team | Coach role, teach not show |
| Skeptic | Disengaged cynic | QA/breaker role, valued doubt |
| Introvert | Invisible | Async paths, quiet wins |
| Competitor | Cuts corners | Clear criteria, quality counts |
| Outsider | Lost tourist | Domain expert role, pair up |
| Socializer | Low output | Morale role, that's valid |

---

## Technique 3: Constraint Mapping

**Prompt:** Map all constraints and find pathways through or around them.

### Constraints Mapped

**Fixed Walls:**
| Category | Constraint | Value |
|----------|------------|-------|
| Time | Start | 9h00 |
| Time | Lunch | 12h00 |
| Time | Demo 1 | 13h00 |
| Time | Demo 2 | 16h00 |
| Time | End | 17h00 |
| Space | Venue | Decided, teams sit together |
| Space | Demo setup | Central screen, facilitator laptop |
| People | Total | ~36 participants |
| People | Teams | 5-6 teams (~6-7 per team) |
| People | Composition | Pre-assigned |
| People | Remote | None |
| People | POs | Olivier, Michael, Bert |
| Tech | Stack | .NET 10 + React + Docker/Postgres |
| Tech | Codebase | Per-team repos |
| Prep | PRD | Ready |
| Prep | Architecture | Ready |

**Flexible Doors:**
| Element | Flexibility |
|---------|-------------|
| Dev blocks / breaks | Team decides |
| BMAD vs YOLO | Team choice |
| Roles within team | Self-organize |
| Hidden epics branch | Escape hatch available |

**Decision Made:**
| Constraint | Decision |
|------------|----------|
| Claude subscription | Max 5x ($100/mo per person) |
| Paid by | itenium |
| Total cost | ~$3,600 for 36 participants |

---

## Session Outputs

**Artifacts generated:**
- 10 risks identified with mitigations
- 8 personas stress-tested
- Full constraint map
- Action checklist: `_bmad-output/bootcamp-prep-checklist.md`

**Key decisions:**
- PRD + Architecture visible, Epics on hidden branch
- Max 5x Claude subscriptions, itenium expenses
- Pre-assigned teams, balanced composition
- Per-team repos

---

*Session complete. Continued from 2026-02-06, finalized 2026-02-25.*
