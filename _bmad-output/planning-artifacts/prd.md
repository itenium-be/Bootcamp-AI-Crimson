---
stepsCompleted: [step-01-init, step-02-discovery, step-02b-vision, step-02c-executive-summary, step-03-success, step-04-journeys, step-05-domain, step-06-innovation, step-07-project-type, step-08-scoping, step-09-functional, step-10-nonfunctional, step-11-polish]
inputDocuments:
  - _bmad-output/planning-artifacts/product-brief-Bootcamp-AI-2026-02-27.md
  - _bmad-output/brainstorming/brainstorming-session-2026-02-06.md
  - _bmad-output/brainstorming/brainstorming-session-2026-02-09.md
  - Itenium.SkillForge/README.md
workflowType: 'prd'
classification:
  projectType: saas_b2b
  domain: hrtech
  complexity: medium
  projectContext: brownfield
---

# Product Requirements Document - Bootcamp-AI (SkillForge)

**Author:** Olivier
**Date:** 2026-02-27

## Executive Summary

SkillForge is an internal consultant growth platform for itenium. It solves two connected problems: consultants without visible growth paths disengage and leave; consultants without validated skill profiles get placed in missions that don't fit — undervalued or overstretched. Both outcomes cost itenium in attrition and client trust.

SkillForge makes growth visible and actionable by giving every consultant a co-maintained skill roadmap — current state, prioritised next steps, and linked learning resources — co-owned with their competence coach. For coaches, it replaces informal spreadsheets and memory with a shared, always-current record of every consultant's progress, goals, and validated skill levels. For sales and management, validated skill snapshots are a byproduct of the coaching process, not a separate data collection exercise.

### What Makes This Special

The coach relationship is the moat. Self-reported skill tracking (LinkedIn), generic course catalogues (Pluralsight), and static spreadsheets (itenium's current state) all fail the same way: they remove the human. SkillForge makes the coaching relationship ten times more effective by giving it a shared language (the skill matrix), a shared record (the consultant roadmap), and a shared knowledge base (the community resource library). Every skill validation reflects a real human expert's judgement — not a checkbox or a course completion badge.

## Project Classification

- **Type:** Internal B2B SaaS platform — web application, role-based access, team-scoped data
- **Domain:** HRTech / internal talent platform — no regulatory compliance obligations; complexity lives in the domain model (skill dependency graph, variable-depth levels, seniority threshold computation)
- **Complexity:** Medium — non-trivial data model and coaching workflow state machine; standard web security and access control
- **Context:** Brownfield — existing .NET 10 + React scaffold with auth (OpenIddict/JWT), role model (backoffice/manager/learner), team structure (Java/.NET/PO&Analysis/QA), and PostgreSQL already operational

## Success Criteria

### User Success

**Consultant — Active Engagement**
A consultant is genuinely engaged when all three occur within any 3-month window:
- ≥1 resource completed linked to an active goal
- ≥1 readiness flag raised on a skill goal
- ≥1 skill level validated by their coach

Single signals are passive or coincidental. All three together confirm a complete growth cycle.

**Coach — Running Their Team**
A coach is actively using the platform when, per consultant per quarter:
- Every consultant has ≥1 active goal set by the coach
- ≥1 coaching session recorded
- ≥1 skill level validation made

**Bootcamp Demo Criterion**
Both flows demonstrated end-to-end in one session:
1. Consultant: logs in → sees personalised roadmap → finds linked resource → marks complete → raises readiness flag
2. Coach: sees flag on dashboard → opens live session mode → validates skill level → sets new goal

### Business Success

| Metric | Target | Timeframe |
|---|---|---|
| Consultants with active goals | ≥80% of total | 3 months post-launch |
| Active engagement cycles completed | ≥1 per consultant | Per quarter |
| Coaching sessions recorded | ≥1 per consultant | Per quarter |
| Client missions staffed via SkillForge | ≥1 | 12 months |
| Resources contributed by non-coaches | ≥10 | 12 months |

### Technical Success

See Non-Functional Requirements for measurable targets. Summary: <2s page load, <500ms API reads, zero data loss on coaching records, Chrome/Edge/Firefox support.

### Failure Signal

**The platform is considered a failure if it is not used.** Specific failure indicators:
- Coaches do not set goals for their consultants within 30 days of launch
- Fewer than 50% of consultants log in within the first month
- No coaching sessions recorded after the bootcamp demo

Adoption is the only metric that matters at launch. A technically perfect product that nobody uses has failed.

## Product Scope

### MVP — Minimum Viable Product (Bootcamp, March 13, 2026)

Enables the complete consultant–coach growth loop end-to-end for the bootcamp demo.

1. **Authentication & Role-Based Access** — Role-aware login for Consultant (`learner`), Coach (`manager`), Admin (`backoffice`). Extends existing OpenIddict auth infrastructure with SkillForge-specific role mapping.
2. **Global Skill Catalogue — Seeded** — Two-layer architecture seeded from itenium Excel matrices. Each skill node: name, category, description, levelCount (1=checkbox, 2–7=progression), level descriptors, prerequisite links.
3. **Skill Dependency Warnings** — Visible warning when prerequisites unmet. Skills are never locked — warned only.
4. **Skill Profile Assignment** — Coach assigns consultant to competence centre profile (.NET, Java, FA/BA, QA). Roadmap filtered to relevant skills only.
5. **Consultant Roadmap — Progressive Disclosure** — Default: current anchors + immediate next-tier skills (8–12 nodes). Full roadmap via "Show all."
6. **Active Goals View** — Coach-assigned goals on roadmap: skill, current niveau, target niveau, deadline, linked resources. First login shows pre-populated goals — not an empty screen.
7. **Resource Library — Browse, Link & Complete** — Add resource (title, URL, type, skill link, fromLevel→toLevel). Mark as completed (adds evidence). Rating: thumbs up/down.
8. **Readiness Flag** — Consultant signals "I think I'm ready." One active flag per goal. Dashboard indicator on coach view. Aging indicator shown. *(Push/email delivery: Post-MVP)*
9. **Coach Dashboard — Team Overview** — All consultants in one view: readiness flags (with age), inactive consultants (3+ weeks), active goals per consultant, quick entry to any profile.
10. **Live Session Mode — Skill Validation** — Focused view during coaching session: pending validations + active goals only. 2-tap validation (current→new niveau), inline notes, SMART goal setting.
11. **Seniority Threshold Ruleset** — Data model and computation for seniority thresholds (Junior/Medior/Senior) per profile: `{skillId, minLevel}` pairs. Consultant view shows progress: "You meet 14/18 Medior requirements."

### Growth Features (Post-MVP)

- Visual Profile Builder — drag-and-drop canvas for coaches to build/edit competence centre profiles
- Seniority Threshold Dashboard — visual gap view per consultant toward next seniority level
- Pre-session Talking Points — auto-generated context from activity data before coaching sessions
- Cohort Gap Heatmap — team-level skill gap visualisation for coaches
- Sales Snapshot & Profile Card Export — matchable, validated skill snapshot for client proposals
- HR Aggregate Reporting — salary review input; cohort-level skill distribution
- LearningEvents — group training sessions with multi-skill coverage and attendance tracking
- Resource Quality Lifecycle — flagged → stale → deprecated curation flow
- Skill Proposal Queue — coach-voting mechanism for promoting skills to global catalogue
- **Notifications** — push/email delivery for readiness flags and onboarding triggers

### Vision (Future)

- Cross-company skill benchmarking
- AI-assisted coaching suggestions
- Integration with external platforms (LinkedIn, certification bodies)
- Expansion to other competence centres or external clients

## User Journeys

### Journey 1: Lea — Consultant, First Login and First Growth Cycle (Success Path)

Lea is a .NET developer, 18 months into her second role at itenium. She's good at her job but has no idea where she stands relative to Medior. Her last coaching session was two months ago and she can't remember what was discussed.

**Opening Scene — First Login:**
Lea opens SkillForge for the first time. Instead of an empty dashboard asking her to fill something in, she sees a roadmap that already has her name on it. Three goals, set by her coach: "Clean Code niveau 3," "Entity Framework niveau 2," "REST API Design niveau 2." Linked resources are already attached to each one. She thinks: *"Someone has already thought about this for me."*

**Rising Action — Active Use:**
Over the next three weeks, Lea browses the resources linked to her Clean Code goal. She marks one as complete — a book chapter her coach recommended. She notices the skill node showing her current niveau (1) and target (3). She can see she's not there yet, but the path is clear. Her goal progress indicator updates — visible progress without needing her coach to be present.

**Climax — Readiness Signal:**
After completing two more resources and practicing in her current mission, Lea feels ready. She raises a readiness flag on "Clean Code niveau 3." The flag appears on her coach's dashboard immediately. The flag shows: *"Raised 0 days ago."*

**Resolution — Growth Confirmed:**
In their coaching session, her coach validates Clean Code niveau 2 (not 3 yet — but genuine progress). A new goal is set. Lea's roadmap updates in real time. The node turns green. She's 15/18 toward Medior. *"I know exactly where I am and what's next."*

**Capabilities revealed:** Personalised first-login experience (pre-set goals), progressive roadmap view, goal progress indicator, resource completion tracking, readiness flag (dashboard indicator to coach), coach validation flow, seniority progress indicator.

---

### Journey 2: Nathalie — Coach, Between Sessions to Session Close (Success Path)

Nathalie coaches 12 .NET consultants. It's Monday morning. She opens SkillForge and sees two readiness flags raised over the weekend.

**Opening Scene — Dashboard Scan:**
Nathalie's dashboard shows: Lea raised a readiness flag 2 days ago. Thomas has had no activity in 23 days. Two consultants have overdue goals. In 30 seconds, Nathalie knows who needs attention this week. The dashboard tells her what to do.

**Rising Action — Pre-Session Preparation:**
Before Lea's session, Nathalie opens Lea's profile. She sees everything since last time: resources completed, flag raised, current skill states. No archaeology through emails or Slack. She walks into the session prepared.

**Climax — Live Session Mode:**
During the coaching session, Nathalie taps "Start Session." The UI collapses to essentials: pending validations and active goals only. Two taps: Clean Code niveau 1 → niveau 2. She adds a short note: "Strong grasp of naming and functions. Not yet applying at architectural level." She sets a new SMART goal.

**Resolution — Session Closed:**
She exits live session mode. The session is recorded, the validation is timestamped, and the new goal is already visible on Lea's roadmap. Nathalie moves to the next consultant. *"I walked out with everything validated and recorded in under 5 minutes."*

**Capabilities revealed:** Coach dashboard with readiness flags and inactivity alerts, consultant profile with activity history, live session mode, 2-tap skill validation, inline session notes, SMART goal setting.

---

### Journey 3: Lea — Dependency Warning Edge Case

Lea is exploring her roadmap using "Show all." She sees an advanced skill: "Domain-Driven Design niveau 3." It looks interesting. She clicks it.

A visible warning appears: *"Clean Code niveau 3 not yet met — you can explore this skill, but your coach may ask you to address prerequisites first."*

The skill is not locked. Lea reads the description and level descriptors. She decides to focus on her current goals first but bookmarks the skill mentally. The warning has done its job: she's informed, not blocked.

**Capabilities revealed:** Prerequisite dependency graph, non-blocking dependency warning UI, full roadmap "show all" view.

---

### Journey 4: BackOffice Admin — Onboarding a New Consultant

itenium hires a new Java developer, Sander. The platform admin creates his account, assigns `learner` role and Java team claim. Two minutes of work.

Coach Java is notified via the dashboard. She opens Sander's profile, assigns the Java competence centre profile, and sets 3 onboarding goals before his first login.

Sander logs in. His opening screen: *"Welcome, Sander. Your coach has set 3 goals for your first 6 weeks."* Not an empty screen — a starting point with intent.

**Capabilities revealed:** User management (create, role + team assignment), competence centre profile assignment, pre-populated first-login experience.

---

### Journey Requirements Summary

| Capability Area | Journey | MVP/Post-MVP |
|---|---|---|
| Personalised first-login (pre-set goals) | 1, 4 | MVP |
| Progressive roadmap with current + next-tier nodes | 1 | MVP |
| Goal progress indicator (mid-cycle visibility) | 1 | MVP |
| Resource completion tracking as evidence | 1 | MVP |
| Readiness flag — dashboard indicator to coach | 1, 2 | MVP |
| Readiness flag — push/email notification to coach | 1, 2 | Post-MVP |
| Aging indicator on readiness flag | 1, 2 | MVP |
| Seniority progress indicator | 1 | MVP |
| Coach dashboard: flags, inactivity alerts, overdue goals | 2 | MVP |
| Consultant profile: full activity history for coach | 2 | MVP |
| Live session mode: focused 2-tap validation | 2 | MVP |
| Inline session notes + SMART goal setting | 2 | MVP |
| Skill dependency warning (non-blocking) | 3 | MVP |
| Full roadmap "show all" view | 3 | MVP |
| User management: role + team assignment | 4 | MVP |
| Competence centre profile assignment | 4 | MVP |
| Onboarding trigger — pre-populated first login | 4 | MVP |
| Onboarding trigger — push/email notification | 4 | Post-MVP |
| Retention signal (mid-cycle re-engagement) | — | Post-MVP |

## Domain-Specific Requirements

**Domain:** HRTech — Internal Talent & Coaching Platform
**Regulatory complexity:** Low (no external obligations)
**Technical complexity:** Medium (data model integrity, access scoping, lifecycle management)

### Compliance & Regulatory

- No external regulatory obligations. Data handling is governed by the itenium employment agreement — no GDPR data subject rights workflow, no data processing register, no DPA required.
- Access restrictions are organisational policy, not legal mandate, but enforced technically to maintain trust.

### Access Scoping

Role and team scoping enforced at API/repository layer, not UI only. See RBAC Matrix in SaaS B2B Requirements for full role definitions.

Key constraints:
- `learner` — own data only; cannot self-validate skill levels
- `manager` — own team only; cannot access other coaches' consultants
- `backoffice` — aggregate/administrative views; excluded from individual coaching session notes

### Validation Integrity

Coach-only validation is a business-critical constraint, not a UX preference:
- `POST /validations` restricted to `manager` role, enforced server-side
- Every validation records `validatedBy` (coach user ID) + `validatedAt` timestamp — immutable once written

### Data Lifecycle

**Consultant departure:**
- Account archived (not hard-deleted) on leaving employment
- Archived state: login disabled, invisible to active users, removed from coach dashboards
- All coaching history (session notes, validations, goal records) preserved for institutional knowledge
- Archived accounts recoverable by `backoffice` (re-hire scenario)

**Coach departure:**
- All coaching relationships remain intact, attributed to original coach
- Orphaned consultants (no active coach) remain visible to `backoffice` for reassignment

### Domain Risk Mitigations

| Risk | Mitigation |
|---|---|
| Coach validates wrong skill level | Immutable audit trail; `backoffice` admin override post-MVP |
| Consultant data visible cross-team | Team-scoped queries via `ISkillForgeUser.Teams` claim at repository layer |
| Coaching notes exposed to HR aggregate views | `backoffice` endpoints return summary statistics only, never raw notes |
| Archived user data leaks into active views | Archived filter applied at repository layer, not controller |

## SaaS B2B Specific Requirements

### Deployment Model

Single-tenant — one instance, one organisation (itenium), all data co-located. No billing, subscriptions, tenant isolation, or multi-org partitioning. Future multi-tenancy is Vision-tier only.

### RBAC Matrix

| Role | Label | Scope | Key Permissions |
|---|---|---|---|
| `backoffice` | Admin / HR | Platform-wide | User management, team assignment, aggregate reporting, archived account recovery |
| `manager` | Coach | Own team only | Skill validation (write), goal setting, session notes, consultant profile access |
| `learner` | Consultant | Own profile only | Roadmap view, resource library, readiness flag, goal progress tracking |

**Enforcement:** All role checks at API middleware level. Team scoping via `ISkillForgeUser.Teams` claim at repository/query layer. Validation writes restricted to `manager` server-side.

### Integrations

None at MVP. Only external dependency: existing OpenIddict/JWT auth infrastructure in the brownfield codebase.

**Potential future integrations (Vision, not committed):**
- External IdP federation (Azure AD / Google SSO)
- HR system sync for employee onboarding/offboarding
- Export to sales CRM for consultant profile cards

## Project Scoping & Phased Development

### MVP Strategy

**Approach:** Experience MVP — complete, demonstrable end-to-end growth loop (consultant → readiness → coach validation → new goal) in one bootcamp session on March 13, 2026.
**Team:** 4 AI-assisted developers, ~2-week sprint.
**Success:** Both demo flows run without workarounds. Platform usable by real coaches and consultants on demo day.

The 11 MVP capabilities are defined in the Product Scope section above.

**Key scope decision — Notifications deferred from MVP:**
- Readiness flag → coach: dashboard indicator only (no push/email)
- Onboarding trigger → consultant: pre-populated first-login (no notification delivery)
- Push/email notification delivery moves to Phase 2 (Growth)

### Risk Mitigation

| Risk | Mitigation |
|---|---|
| Skill dependency graph complexity | Simple prerequisite ID list per skill; warn-only, no recursive computation |
| Seniority threshold computation | Static ruleset per profile (`{skillId, minLevel}` pairs); computed at read time, no background jobs |
| Seeding Excel matrices | One-time import script; can be done manually if automation slips |
| Demo instability | Seed data covers full demo script; pre-populated state for demo users on day 1 |

## Functional Requirements

### Identity & Access Management

- FR1: A Consultant can authenticate and access only their own profile, roadmap, and goals
- FR2: A Coach can authenticate and access all consultants assigned to their team
- FR3: An Admin can authenticate and access platform-wide management views
- FR4: The system restricts skill validation writes to Coach role only
- FR5: An Admin can create user accounts and assign role and team membership

### Skill Catalogue Management

- FR6: The system maintains a global skill catalogue with skills organised by category
- FR7: Each skill has a name, description, variable level count (1=checkbox, 2–7=progression), level descriptors per niveau, and prerequisite links to other skills
- FR8: The system displays a non-blocking warning when a Consultant views a skill whose prerequisites are not yet met at the required niveau
- FR9: The system supports seeding the skill catalogue from imported data
- FR10: The system supports a two-layer skill architecture: universal itenium skills and competence-centre-specific profiles that filter the relevant subset

### Consultant Profile & Roadmap

- FR11: A Coach can assign a Consultant to a competence centre profile (Java, .NET, PO&Analysis, QA)
- FR12: A Consultant can view a personalised roadmap filtered to their assigned competence centre profile
- FR13: A Consultant's roadmap defaults to showing current skill anchors plus immediate next-tier skills (8–12 nodes)
- FR14: A Consultant can expand their roadmap to view all skills in their profile
- FR15: A Consultant's first login presents a pre-populated roadmap with coach-assigned goals (not an empty screen)

### Goal & Growth Management

- FR16: A Coach can assign a goal to a Consultant, specifying skill, current niveau, target niveau, deadline, and linked resources
- FR17: A Consultant can view their active goals including skill, current niveau, target niveau, deadline, and linked resources
- FR18: A Consultant can raise a readiness flag on an active goal (maximum one active flag per goal)
- FR19: The system tracks the age (days elapsed since raised) of each readiness flag
- FR20: A Coach can view all readiness flags across their team with the age of each flag displayed

### Resource Library

- FR21: Any authenticated user can browse the resource library
- FR22: Any authenticated user can contribute a resource to the library, specifying title, URL, type, linked skill, and applicable niveau range
- FR23: A Consultant can mark a resource as completed, recording it as evidence against a goal
- FR24: Any authenticated user can rate a resource

### Coach Dashboard & Team Management

- FR25: A Coach can view all consultants on their team in a single overview
- FR26: The Coach dashboard surfaces consultants with active readiness flags, with flag age visible
- FR27: The Coach dashboard surfaces consultants with no activity for 3 or more weeks
- FR28: The Coach dashboard shows the active goal count per consultant
- FR29: A Coach can navigate directly from the dashboard to any consultant's profile
- FR30: A Coach can view a consultant's full activity history (completed resources, goals set, validations received, readiness flags raised)

### Live Session & Skill Validation

- FR31: A Coach can enter a focused live session view for a specific consultant
- FR32: In live session mode, the Coach sees only pending validations and active goals for that consultant
- FR33: A Coach can validate a skill niveau for a Consultant in live session mode
- FR34: A Coach can add session notes during a live session
- FR35: A Coach can create a new goal for a Consultant during or after a live session
- FR36: The system records each skill validation with the validating coach's identity and a timestamp
- FR37: A completed live session is recorded and visible in the consultant's activity history

### Seniority & Progress Tracking

- FR38: The system maintains seniority threshold rulesets per competence centre profile, defined as skill + minimum niveau pairs
- FR39: A Consultant can view their progress toward their next seniority level as a count of met versus required criteria

### User & Account Lifecycle (Admin)

- FR40: An Admin can create a user account and assign role and team membership
- FR41: An Admin can archive a user account, disabling login while preserving all historical coaching data
- FR42: An Admin can restore an archived user account
- FR43: An Admin can view all consultants not currently assigned to an active coach

## Non-Functional Requirements

### Performance

- Page load (initial render): <2s on standard office network
- API response time: <500ms for read operations under normal load
- Write operations (validations, goal saves, session notes): <1s
- Concurrent user baseline: ~50–150 users; no load spike scenarios expected

### Security

- HTTPS required in all environments except localhost development
- JWT token validated on every API request; no unauthenticated endpoints except login
- Role and team scope enforced at API/repository layer, not UI only
- Validation records immutable once written (`validatedBy` + `validatedAt` fields non-updatable)
- Session timeout: inactive sessions expire after 1 hour (nice-to-have; does not block MVP demo)

### Reliability & Data Integrity

- Zero data loss tolerance for: coaching session notes, skill validations, goal records
- Soft-delete / archive model only — no hard deletion of user or coaching data
- Browser support: current stable versions of Chrome, Edge, and Firefox

### Out of Scope

- Accessibility (WCAG compliance) — not required for this internal tool
- Scalability beyond itenium team size — single-tenant, fixed user base
- External integration reliability — no third-party systems at MVP
