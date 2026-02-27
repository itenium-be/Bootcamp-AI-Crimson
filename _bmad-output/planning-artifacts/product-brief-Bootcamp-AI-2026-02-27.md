---
stepsCompleted: [1, 2, 3, 4, 5, 6]
inputDocuments:
  - _bmad-output/brainstorming/brainstorming-session-2026-02-06.md
  - _bmad-output/brainstorming/brainstorming-session-2026-02-09.md
date: 2026-02-27
author: Olivier
---

# Product Brief: Bootcamp-AI (SkillForge)

## Executive Summary

SkillForge is a consultant growth platform for itenium — a shared, living
map that gives every consultant and their coach a common language for where
they are, where they're going, and what to do next.

It is not a performance measurement tool. It is a career companion: one
that centralizes the coaching relationship, captures collective knowledge,
and makes the path forward visible and actionable for every consultant at
itenium.

For coaches, it amplifies their effectiveness across a team of consultants
they know personally. For sales and management, it provides a live,
validated skill snapshot — a byproduct of growth, not the purpose of it.

---

## Core Vision

### Problem Statement

itenium consultants today have no structured, visible way to understand
their current skill level relative to their role and career aspirations.
Competence coaches manage growth through informal conversations and personal
spreadsheets — with no shared tool to track progress, record decisions, or
capture the learning material they recommend. The collective knowledge of
coaches and senior consultants is scattered across emails, Slack messages,
and individual memory — never captured, never scored, never reusable.

### Problem Impact

- Consultants lack career clarity, leading to disengagement and missed
  growth opportunities
- Coaches spend disproportionate time on administrative recall rather than
  meaningful coaching conversations
- Valuable institutional knowledge is lost when coaches or consultants leave
- Sales proposals rely on outdated or self-reported skill data
- Salary and promotion decisions lack objective, structured input

### Why Existing Solutions Fall Short

Generic LMS platforms provide course catalogues but no skill tracking tied
to a consultant's role profile. LinkedIn Skills provides self-reported
badges without coaching validation or growth path guidance. itenium's
existing Excel matrices capture skill intent but are static, disconnected
from learning material, and impossible to maintain at scale. No existing
tool makes the coaching relationship itself the unit of value.

### Proposed Solution

SkillForge gives every stakeholder what they need:

- **Consultants:** A progressive roadmap — current state, prioritized next
  steps, and exactly what to learn to get there — co-maintained with their
  competence coach. Always answers: "what do I do next, and why?"
- **Coaches:** An always-on dashboard to guide and validate consultant
  growth, with a shared, community-rated resource library that captures
  institutional knowledge
- **Sales & Management:** A live, coach-validated skill snapshot — a
  byproduct of growth, not a primary data collection exercise

### Key Differentiators

**The coach relationship is the moat.** LinkedIn can track self-reported
skills. Pluralsight can serve courses. A spreadsheet can store a matrix.
None of them can replicate a real human who knows you, sets your goals, and
validates your progress. SkillForge makes that relationship ten times more
effective — and captures everything it produces.

Supporting this:
- The skill matrix is a shared language between coach and consultant,
  not a performance report card
- Progressive disclosure: the roadmap shows what's next, not everything
  at once
- Knowledge economy: resources contributed and rated by everyone —
  self-curating institutional memory
- Two-layer architecture: universal Itenium Skills + competence-center
  owned profiles — no duplication, clear ownership

---

## Target Users

> **MVP Focus:** The consultant + coach pair is the core user relationship.
> All other user groups are supported but can be deprioritised in early
> epics without losing the product's core value.
>
> **Scale note:** Designed for itenium's current ~40 consultants and 4
> coaches. Built to scale to 200+ without architectural shortcuts.

---

### Primary Users

---

#### The Consultant

**Who they are:**
itenium consultants span all seniority levels — from a .NET developer
6 months into their first role to a senior Java architect with 12 years
of experience. They belong to one of four competence centers (.NET, Java,
Functional/Business Analysis, QA) and work at client sites.

**How they experience the problem today:**
No structured view of their career trajectory. Growth conversations happen
in infrequent, unrecorded coaching sessions. Between sessions, there is
no shared artifact to refer back to. A junior doesn't know what to learn
this month. A senior doesn't know how close they are to the next level.

**Engagement pattern:**
Weekly for engaged consultants; monthly for those less invested. The
platform rewards regular check-ins without penalising lower frequency.

**First login — moment zero:**
The consultant's first experience is already personalised — the coach
sets their profile and first goals before they log in for the first time.
Their opening screen reads: *"Welcome, [Name]. Your coach has set 3 goals
for your first 6 weeks."* Not a task list. A starting point with intent.

**What success looks like:**
*"I open SkillForge on Monday and I know exactly what to focus on this
week and why my coach thinks it matters for my growth."*

**Their journey:**
1. **Onboarding:** Coach assigns profile and sets first 3 goals before
   first login — consultant arrives to a personalised roadmap
2. **Regular use:** Checks active goals, browses linked resources,
   marks resources as completed
3. **Readiness signal:** Raises a readiness flag when they feel ready
   for a skill to be validated
4. **Coaching session:** Reviews progress with coach live; skill levels
   updated, new goals set
5. **Long-term:** Roadmap evolves — nodes turning green, next steps
   expanding, seniority threshold getting closer

---

#### The Competence Coach

**Who they are:**
A dedicated role at itenium — currently 4 coaches, each owning a
competence center:
- **Coach .NET** — guides .NET developers across all seniority levels
- **Coach Java** — guides Java developers
- **Coach FA/BA** — guides Functional and Business Analysts
- **Coach QA / Other** — guides QA engineers and smaller role profiles

Each coach manages roughly 8–15 consultants. They are domain experts
and bring both technical depth and personal investment in growth.

**How they experience the problem today:**
Consultants' progress lives in their heads and personal spreadsheets.
Before a coaching session they reconstruct context from memory and
scattered notes. Recommended resources live in emails and Slack —
never captured, never reusable. When a consultant changes coach,
all context is lost.

**What success looks like:**
*"I walked into every coaching session prepared — I already knew what
had happened since last time. And I walked out with everything validated
and recorded in under 5 minutes."*

**Their journey:**
1. **Between sessions:** Dashboard scan — readiness flags raised, who
   has had no activity in 3+ weeks, which goals are overdue
2. **Pre-session:** Auto-generated talking points surface changes
   since last session
3. **During session (live mode):** Minimal UI — validates skill levels
   with 2 taps, adds notes, adjusts goals in real time
4. **Post-session:** Finalises session record; sets new SMART goals
5. **Ongoing:** Adds resources to shared library, monitors cohort gaps,
   proposes new skills to the global catalogue

---

### Secondary Users

---

#### Sales / Account Management

**Who they are:**
The people responsible for matching consultants to client missions.
They think in terms of skill requirements and availability — not growth
trajectories.

**What they need:**
Fast, reliable answers to: *"Do we have a Senior Java developer with
Kubernetes experience available in March?"* They need matchable,
trustworthy snapshots — not coaching notes or resource lists.

**Critical trust requirement:**
The snapshot must expose *quality of signal*, not just presence:
- Validated skill niveau (1–7), not just "has skill"
- Validation date — how recent is this data?
- Evidence type — self-assessed or coach-validated?

A sales person who gets one client mismatch from stale data stops
trusting the platform. Freshness and specificity are non-negotiable.

**Engagement pattern:**
Low frequency, high intent. Logs in when a mission needs staffing.
Experience must be frictionless: search → filter → snapshot → export.

**What success looks like:**
*"I found the right consultant for this Java + Kubernetes mission in
2 minutes and sent the client a credible profile."*

---

#### HR / Backoffice

**Who they are:**
Responsible for platform governance, the global Itenium Skills
catalogue, and cross-company reporting. Consumers of skill data for
salary review cycles (which happen outside the platform).

**What they need:**
Aggregate views: skill distribution by profile, seniority threshold
attainment rates, cohort-level gaps. Salary review report exportable
per coach's consultant group.

**What success looks like:**
*"I pulled the salary review report in 10 minutes — validated skill
snapshots for every consultant, grouped by coach."*

---

#### Platform Admin

**Who they are:**
Full system access — user management, role assignment, global
configuration. May be an HR lead or designated itenium ops role.

**What they need:**
User lifecycle management (onboarding, coach assignment, offboarding)
and system-level configuration across all competence centers.

---

### The Core Growth Loop

The heartbeat of SkillForge — every other feature exists to support this:

```
Coach sets goals
      ↓
Consultant works, completes resources, signals readiness
      ↓
Coach validates skill level
      ↓
Goals updated, new goals set
      ↓
Repeat
```

---

## Success Metrics

### User Success Metrics

**Consultant — Active Engagement**
A consultant is considered genuinely engaged when all three of the
following occur within any 3-month window:
- At least 1 resource completed that is linked to an active goal
- At least 1 readiness flag raised on a skill goal
- At least 1 skill level validated by their coach

One signal alone can be passive or coincidental. All three together
indicate a real growth cycle has completed.

**Coach — Running Their Team**
A coach is considered actively using the platform when, per consultant
per quarter:
- Every consultant on their team has at least 1 active goal set by
  the coach
- At least 1 coaching session has been recorded
- At least 1 skill level validation has been made

---

### Business Objectives

**3-Month Adoption Target**
80%+ of itenium consultants have logged in and have at least 1 active
goal assigned by their coach. Coach adoption is implicit in this number
— if consultants have active goals, coaches have done their onboarding
work.

**12-Month Platform Health Indicators**
Two signals that SkillForge has become part of how itenium works:

1. **Sales trust:** The platform has been used to staff at least one
   client mission — a sales person found a consultant match through
   SkillForge and used the profile card in a proposal.

2. **Knowledge economy alive:** The resource library has grown
   organically — resources have been added by contributors other than
   the 4 coaches, indicating consultants are internalising the
   sharing culture.

---

### Key Performance Indicators

| KPI | Target | Timeframe | Measured by |
|---|---|---|---|
| Consultants with active goals | ≥ 80% of total | 3 months post-launch | Platform data |
| Active engagement cycles completed | ≥ 1 per consultant | Per quarter | Platform data |
| Coaching sessions recorded | ≥ 1 per consultant | Per quarter | Platform data |
| Sales missions staffed via SkillForge | ≥ 1 | 12 months | Sales confirmation |
| Resources added by non-coaches | ≥ 10 | 12 months | Platform data |

---

### Bootcamp Demo Success Criterion

**The winning demo shows both sides of the handshake:**

1. **Consultant view:** Logs in → sees personalised roadmap with active
   goals → finds a linked resource → marks it complete → raises a
   readiness flag
2. **Coach view:** Sees the flag on their dashboard → opens live session
   mode → validates the skill level with 2 taps → sets a new goal

Both flows, one demo, one story: *"I know what to do next — and my coach
just confirmed it."*

A team that can show this loop end-to-end has built the product.

---

## MVP Scope

> **Bootcamp context:** Teams have approximately 6 hours of development
> time on March 13, 2026. MVP scope is calibrated to what enables the
> core demo: the consultant-coach growth loop, end-to-end.

---

### Core Features (Must Have)

**1. Authentication & Role-Based Access**
Role-aware login for Consultant, Coach, and Admin. Auth infrastructure
already exists in the SkillForge repo — extend with the new roles.
Roles determine which views and actions are available.

**2. Global Skill Catalogue — Seeded**
The two-layer skill architecture seeded from the provided itenium Excel
matrices (Skill_Matrix_Itenium.xlsx and Developer_Skill_Experience_Matrix
.xlsx). Each skill node has:
- Name, category, description
- Variable level depth (levelCount: 1 = checkbox, up to 7 = progression)
- Level descriptors per niveau
- Prerequisite links to other skills (dependency graph)

**3. Skill Dependency Warnings**
When a consultant views a skill whose prerequisites are not yet met,
a visible warning is shown: *"Clean Code niveau 2 not yet met — you can
explore this skill, but your coach may ask you to address prerequisites
first."* Skills are never locked — only warned.

**4. Skill Profile Assignment**
Coaches can assign a consultant to a competence center profile (.NET,
Java, FA/BA, QA). The consultant's roadmap is filtered to show only
skills relevant to their profile + universal Itenium Skills.

**5. Consultant Roadmap — Progressive Disclosure View**
Default view shows: current skill states (anchors) + immediate next-tier
skills (active targets). Full roadmap available via "Show all." Goal is
8–12 relevant nodes visible at any time, not 45.

**6. Active Goals View**
Consultant sees their coach-assigned goals highlighted on the roadmap:
skill node, current niveau, target niveau, deadline, linked resources.
First login experience: personalised view with coach-set goals already
in place — not an empty screen.

**7. Resource Library — Browse, Link & Complete**
- Any user can add a learning resource (title, URL, type, skill link,
  level transition: fromLevel → toLevel)
- Resources are linked to specific skill nodes and level transitions
- Consultants can mark a resource as completed (adds as evidence on
  the skill state)
- Simple rating: thumbs up/down or 1–5 star score on completion

**8. Readiness Flag**
Consultant raises a "I think I'm ready" flag on an active skill goal.
One active flag per goal. Notifies the coach as a soft ping on their
dashboard. Visible aging indicator (raised X days ago).

**9. Coach Dashboard — Team Overview**
Coach sees all their consultants in a single view:
- Who has raised a readiness flag (and how long ago)
- Who has had no activity in 3+ weeks
- Active goals per consultant with status
- Quick-tap entry into any consultant's profile

**10. Live Session Mode — Skill Validation**
When a coach opens a consultant profile for a session:
- Focused view: pending validations and active goals only
- 2-tap skill level validation (current → new niveau)
- Add session notes inline
- Set or adjust SMART goals
- Minimal UI — designed for use during a live conversation

**11. Seniority Threshold Ruleset**
The data model and logic for seniority thresholds (Junior, Medior,
Senior) per profile: a named set of { skillId, minLevel } pairs.
The consultant view shows progress toward the next threshold:
*"You meet 14/18 Medior requirements."* The full threshold management
dashboard is a stretch goal, but the ruleset must be seeded and
computed at MVP.

---

### Nice-to-Have (Stretch Goals — Bootcamp)

These features add demo polish and real value but are not required for
the core loop. Teams should only attempt these if the MVP core is solid.

| Feature | Value |
|---|---|
| Visual Profile Builder | Coaches build/edit profiles via drag-and-drop canvas |
| Seniority Threshold Dashboard | Visual gap view per consultant toward next level |
| Pre-session Talking Points | Auto-generated context before coaching sessions |
| Cohort Gap Heatmap | Team-level skill gap visualisation for coaches |

---

### Out of Scope for Bootcamp MVP

Explicitly deferred — valuable for post-bootcamp v1, not needed for
the demo or core loop.

| Feature | Reason for deferral |
|---|---|
| Sales snapshot / export | Secondary user — loop works without it |
| HR salary report | Background output — not core to growth loop |
| LearningEvents (group sessions) | Real value, but complex model; defer |
| Skill proposal queue (coach voting) | Governance feature — not demo-critical |
| Slack bot integration | Nice-to-have, separate integration surface |
| Resource quality lifecycle (stale/deprecated flags) | Post-MVP curation feature |

---

### MVP Success Criteria

The MVP is considered successful when a demo can show, end-to-end:

1. A **consultant** logs in → sees their personalised roadmap with
   active goals → finds a linked resource → marks it complete →
   raises a readiness flag on their skill goal
2. A **coach** sees the flag on their dashboard → opens live session
   mode → validates the skill level → sets a new goal for the
   next cycle

Both flows in one demo. The handshake is the product.

---

### Future Vision (Post-Bootcamp)

If SkillForge is adopted post-bootcamp, the roadmap builds on the
MVP foundation:

**v1 — Consolidation (months 1–3 post-bootcamp)**
- Sales snapshot & profile card export
- HR aggregate reporting (salary review input)
- LearningEvents with multi-skill coverage and attendance tracking
- Resource quality lifecycle (flagged → stale → deprecated)
- Skill proposal queue with coach voting → global promotion

**v2 — Intelligence (months 4–9)**
- Cohort gap heatmap for strategic team planning
- Pre-session talking points (auto-generated from activity data)
- Contribution gamification (impact feed, contributor recognition)
- Visual profile builder with versioning

**v3 — Scale (year 2)**
- Cross-company skill benchmarking
- AI-assisted coaching suggestions
- Integration with external platforms (LinkedIn, certification bodies)
- Expansion to other competence centers or external clients
