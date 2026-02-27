---
stepsCompleted: [1, 2, 3]
inputDocuments: ['docs/skillmatrices/Skill_Matrix_Itenium.xlsx', 'docs/skillmatrices/Developer_Skill_Experience_Matrix.xlsx']
session_topic: 'SkillForge Competency Framework - Functional Requirements'
session_goals: 'Flesh out, challenge, and expand functional requirements for profiles, skills, employee tracking, learning material, reviews, and skill matrix visualization'
selected_approach: 'ai-recommended'
techniques_used: ['Morphological Analysis', 'Role Playing', 'Reverse Brainstorming']
ideas_generated: ['Profiles #1-10', 'Skills #1-5', 'Coach #1-3', 'Visualization #1-3', 'Model #1-3', 'RP-A1', 'RP-A2', 'RP-B1', 'RP-B2', 'RP-B3', 'RP-C1', 'RP-D1', 'RB-1 through RB-8']
context_file: ''
session_status: 'complete'
completion_date: '2026-02-27'
---

# Brainstorming Session Results

**Facilitator:** Wouter
**Date:** 2026-02-09

## Session Overview

**Topic:** SkillForge Competency Framework — Functional Requirements for a consultant L&D platform

**Goals:** Generate innovative and comprehensive functional requirements covering profiles (with seniority tiers), skills, employee skill tracking, learning material management, reviews & comments, and skill matrix visualization with gap analysis.

### Context Guidance

_Project is an active LMS (SkillForge) built with .NET 10 + React, with 4 dev teams. Current backlog covers user management, course catalog, enrollment/learning experience, and assessments. This session focuses on the competency/skill framework layer._

### Session Setup

- **Approach:** AI-Recommended Techniques
- **Domain:** Functional requirements for a competency-based L&D platform
- **Key entities:** Profiles, Skills, Seniority Tiers, Learning Material, Reviews, Skill Matrix
- **Inspiration:** roadmap.sh-style visual skill paths for consultants

## Technique Selection

**Approach:** AI-Recommended Techniques
**Analysis Context:** SkillForge Competency Framework with focus on comprehensive functional requirements

**Recommended Techniques:**

- **Morphological Analysis:** Systematically map all entity combinations (Profiles x Skills x Tiers x Materials x Reviews x Visualization) to uncover hidden requirement intersections
- **Role Playing:** Embody key stakeholders (junior consultant, senior analyst, team manager, backoffice) to stress-test requirements from real user perspectives
- **Reverse Brainstorming:** Flip the script — "How could we make this platform useless?" — to surface edge cases, assumptions, and failure modes

**AI Rationale:** Multi-entity domain with complex relationships benefits from systematic mapping first (Morphological), then human-centered validation (Role Playing), then adversarial stress-testing (Reverse Brainstorming). This sequence builds from structure through empathy to resilience.

## Technique Execution — Phase 1: Morphological Analysis (In Progress)

### Morphological Grid Dimensions

| Dimension | Values |
|-----------|--------|
| **Profiles** | .NET Developer, Java Developer, Functional Analyst, Product Owner, Business Architect, Integration Architect |
| **Career Paths (Tech)** | Full Stack, Backend, Frontend, DevOps/Cloud, Tech Lead, Team Lead, Architect |
| **Seniority Tiers** | Junior, Medior, Senior (BA/IA: limited tiers, already senior-level roles) |
| **Growth Models** | I-Shape (deep specialization), T-Shape (broad + depth) |
| **Skills** | Technical skills, Soft skills, Methodologies, Tools |
| **Learning Material** | PDF, Video, Books, Course links, Blog posts, Conference talks, YouTube |
| **User Roles** | Learner/Consultant, Team Manager, Backoffice, Competence Coach |
| **Actions** | View, Create, Assign, Track, Review, Visualize, Coach |

### Ideas Generated

#### Profiles

**[Profiles #1]**: Career Path Branching
_Concept_: A profile isn't just a single role — it's a career path with branches. A Junior .NET Developer might fork toward Full-Stack, Backend Specialist, or Tech Lead. The platform needs to model these branching paths, not just linear ladders.
_Novelty_: Most competency platforms model flat role lists. Branching paths let consultants visualize multiple futures and the skills that differentiate them.

**[Profiles #2]**: Cross-Path Skill Overlap
_Concept_: A .NET Backend Developer and a Java Backend Developer likely share many skills (design patterns, CI/CD, SQL, REST APIs). The platform needs to recognize shared skill pools across profiles so consultants switching paths get credit for what they already know.
_Novelty_: Prevents the frustration of "starting over" when exploring a related career path.

**[Profiles #3]**: T-Shape vs. I-Shape Growth Models
_Concept_: The platform supports two growth philosophies: I-shape (deep specialization along a career path) and T-shape (broad competence across many areas with depth in one or two). A consultant chooses their growth model, and the platform adapts its recommendations, gap analysis, and visualization accordingly.
_Novelty_: Most platforms assume everyone wants to climb a ladder. T-shape support validates the "craftsperson" who wants to master their craft broadly without title progression.

**[Profiles #4]**: Horizontal Growth Tracking
_Concept_: For T-shape consultants, "progress" isn't about moving up a tier — it's about widening the bar. The skill matrix visualization needs a different metaphor: not a ladder to climb, but a radar chart or skill web that expands outward.
_Novelty_: Redefines what "growth" means in the visualization layer, preventing T-shape people from feeling like they're "not progressing."

**[Profiles #5]**: Decoupled Skills from Career Ambition
_Concept_: Skills exist independently from career paths. A career path is a curated collection of skills with a recommended sequence — but a consultant can acquire any skill regardless of their chosen path. The path is a guide, not a gate.
_Novelty_: Prevents the platform from feeling restrictive. A backend developer who picks up frontend skills shouldn't have to "switch paths."

**[Profiles #6]**: Role vs. Ambition Separation
_Concept_: Separate "current role" from "growth direction." A Medior .NET Developer's current role is fixed, but their growth direction could be: deeper in backend, broader as T-shape, or pivoting toward Tech Lead. These are profile overlays, not profile changes.
_Novelty_: Avoids forcing consultants into a single identity. One person can explore multiple growth directions without commitment.

**[Profiles #7]**: Multi-Path Subscription
_Concept_: A consultant can "subscribe" to multiple career paths simultaneously. Their skill matrix becomes a union of all subscribed paths, with visual indicators showing which skills belong to which path (and which overlap).
_Novelty_: Eliminates the false choice between paths. Exploring a direction doesn't mean committing to it.

**[Profiles #8]**: Competence Coach Role
_Concept_: A new platform role — the Competence Coach — who collaborates with consultants on their growth direction. Unlike a Team Manager (who tracks progress), the coach is a career sparring partner who co-curates skill goals, suggests paths, and reviews growth periodically. The coach's role is to aide the consultant in personal development; the platform is the tool to assist both coach and consultant.
_Novelty_: Adds a human-guided dimension alongside algorithmic suggestions. The coach sees the consultant's full matrix and can make personalized recommendations.

**[Profiles #9]**: Peer-Powered Skill Suggestions ("Consultants Like You")
_Concept_: For T-shape consultants without a fixed path, the platform analyzes skill profiles of similar consultants and suggests skills that those peers have acquired. "Consultants with your .NET + SQL + Docker profile also learned: Kubernetes, Azure DevOps, Terraform."
_Novelty_: Collaborative filtering applied to career development. Surfaces organic learning patterns from the consultant population.

**[Profiles #10]**: Coach-Consultant Growth Plan
_Concept_: The competence coach and consultant co-create a growth plan — a time-bound selection of target skills with linked learning material. This plan lives on the platform, is trackable, and can be reviewed/adjusted periodically.
_Novelty_: Bridges the gap between "here's your skill gap" and "here's what to do about it." Makes the coach relationship actionable and visible.

#### Coach & Personal Development

**[Coach #1]**: Personal Development Plan (PDP) as First-Class Entity
_Concept_: A PDP is a collaborative document created by coach + consultant, containing target skills, timeline, selected learning materials, and milestones. It lives on the platform as a trackable, versioned artifact — not a Word doc in someone's mailbox. The coach and consultant can both edit it, and progress auto-updates as skills are checked off.
_Novelty_: Turns a traditionally offline HR process into a living, measurable platform feature. The PDP becomes the central navigation tool for the consultant's growth.

**[Coach #2]**: AI Transcription → PDP Generation
_Concept_: After a coaching interview, an AI-transcribed conversation is uploaded. The system parses the transcript to extract mentioned skills, goals, strengths, and gaps, then generates a draft PDP. The coach reviews, adjusts, and finalizes.
_Novelty_: Bridges the gap between unstructured human conversation and structured platform data. Coaching sessions become direct input to the system instead of lost context.

**[Coach #3]**: Coaching Session Log
_Concept_: Each coaching session (transcript or summary) is stored as a session log linked to the consultant's profile and PDP. Over time, this creates a longitudinal record: what was discussed, what goals were set, what changed.
_Novelty_: Creates institutional memory for coaching relationships. When a coach changes, the history transfers seamlessly.

#### Skills

**[Skills #1]**: Skill Granularity Levels
_Concept_: Not all skills are binary (have it / don't have it). Some skills have proficiency levels — e.g., "Docker: Awareness / Working Knowledge / Proficient / Expert." The platform needs to support both binary checkoff skills AND graduated proficiency scales.
_Novelty_: Prevents oversimplification. "Knows C#" means very different things at Junior vs. Senior level.

**[Skills #2]**: Skill Dependencies / Prerequisites
_Concept_: Some skills have natural prerequisites — you can't meaningfully learn Kubernetes without understanding containers. The platform could model skill dependency chains that guide learning order and prevent consultants from jumping to advanced topics prematurely.
_Novelty_: Creates natural learning sequences. The roadmap.sh inspiration comes alive here — visual dependency trees.

**[Skills #3]**: Skill Decay / Freshness
_Concept_: Skills aren't permanent. A consultant who used Angular 3 years ago but hasn't touched it since has a decaying skill. The platform could flag skills that haven't been "refreshed" within a configurable timeframe.
_Novelty_: Keeps the skill matrix honest and current. Prevents a false sense of competence based on outdated experience.

**[Skills #4]**: Skills vs. Courses — The Missing Link
_Concept_: The platform needs a many-to-many relationship: Learning Material <-> Skills. A single course might cover 5 skills. A single skill might have 10 different learning materials. This is fundamentally different from the existing Course -> Module -> Lesson hierarchy.
_Novelty_: Courses aren't the only learning material. The skill becomes the organizing principle, not the course.

**[Skills #5]**: Skill Evidence / Proof of Competence
_Concept_: Checking off a skill isn't just self-declaration. The platform could support multiple evidence types: self-assessment, quiz completion (linking to Team 4's work), manager validation, peer endorsement, certificate upload, or project experience. Different evidence types carry different weight.
_Novelty_: Adds credibility to the skill matrix. Integrates naturally with Team 4's assessment work.

#### Visualization

**[Visualization #1]**: Roadmap.sh-Style Skill Tree
_Concept_: Each career path renders as an interactive skill tree — a visual dependency graph where nodes are skills, edges show prerequisites, and color-coding shows the consultant's status (completed / in-progress / not started / decaying). Click a node to see linked learning materials.
_Novelty_: The core roadmap.sh inspiration brought to life. The tree adapts based on subscribed paths and seniority tier.

**[Visualization #2]**: Gap Heat Map
_Concept_: A team-level visualization where the manager or coach sees a heat map of skill gaps across their team. Rows = team members, Columns = required skills. Red = missing, Yellow = in progress, Green = completed. Instantly shows collective team weaknesses.
_Novelty_: Turns individual skill matrices into a strategic team planning tool.

**[Visualization #3]**: T-Shape Radar Chart
_Concept_: For T-shape consultants, a radar/spider chart showing breadth across skill categories with depth indicated by distance from center. The consultant sees their shape and can compare it to the "ideal T-shape" for their level.
_Novelty_: Gives T-shape consultants a visualization that celebrates breadth instead of penalizing lack of specialization.

---

---

## Phase 2 — Role Playing (completed 2026-02-27)

### Key Decisions Confirmed
- Consultant signals readiness (flag), coach validates and moves the skill level
- Skills are never locked — prerequisite warnings shown, never gates
- Learning material is global, linked to skills and level transitions, never consultant-specific
- One global skill set: Layer 1 (Itenium Skills, universal) + Layer 2 (profile-specific, per competence center)
- Coach is the lead curator of personal matrices, not the consultant
- SMART goals per skill target (specific, measurable, time-bound)
- Coaching session = structured event with typed outcomes; coach dashboard is always-on

### Ideas Generated

**[RP-A1] Readiness Flag**
Consultant raises a "I think I'm ready" flag on a skill goal. Notifies coach as a soft ping. One active flag per goal to prevent spam. Feels like a considered action, not a button.

**[RP-A2] Live Session Mode**
When a coaching session is opened, platform enters a minimal focused view: pending validations, level control, notes field. Two-tap validation. Everything else hidden during the session.

**[RP-B1] Pre-Session Talking Points**
Auto-generated from activity data before each coaching session: resources completed, readiness flags raised, goals with no activity. Context-setting, not prescriptive.

**[RP-B2] Skill Proposal Queue**
Coach adds a skill not in the global set → immediately usable as a local skill → goes into a pending queue → coach voting promotes it to global (e.g., 3 independent additions auto-promotes).

**[RP-B3] Cohort Learning Trigger**
When a systemic gap is detected (>50% of a profile group below minimum level on a skill), the platform surfaces a suggestion for a group learning intervention.

**[RP-C1] Share-from-Anywhere — Slack Bot (nice-to-have)**
A Slack bot that intercepts resource links shared in Slack and offers a one-confirm flow to add them to the SkillForge resource library, attributed to the sharer.

**[RP-D1] Visual Profile Builder**
HR/coaches get a canvas to drag skill nodes from the global catalogue into a profile, set minimum niveau requirements per skill with a slider, draw dependency edges, and preview the resulting roadmap as a consultant would see it. Changes are versioned.

---

## Phase 3 — Reverse Brainstorming (completed 2026-02-27)

### Design Constraints Derived

| Failure Mode | Design Constraint |
|---|---|
| Validation bottleneck kills motivation | Coach dashboard is always-on; readiness flags have aging indicators |
| Platform becomes salary data gathering | Growth framing first; HR/sales exports are background outputs, never in main flows |
| Resource library becomes a dump | Quality surfaced by usage + ratings; coaches pin "recommended" per level transition |
| Group training inflates levels | Attendance = evidence only, never auto-validates; coach decides at next review |
| Admin kills coaching conversation | Live session mode is 2-tap minimal; post-session detail added separately |
| Profile inconsistency across competence centers | Each competence center owns their profiles; Itenium Skills (Layer 1) are global and universal |
| Junior overwhelm on day one | Simplified onboarding view until first coaching session unlocks full roadmap |
| Sales report staleness | Data is inherently current (slow-changing, coach-validated); report shows validated level + date |
| Readiness flag spam | One active flag per goal; UI makes it a considered action |
| Cross-competence events invisible | LearningEvents support targetProfiles: ALL or multiple; any coach adds attendees for their consultants |

### Additional Model Refinements

**Resource quality lifecycle:**
- Status: `active | flagged | stale | deprecated`
- Any user can raise a "possibly outdated" flag (low friction)
- Accumulated negative reviews auto-flag as stale
- Coach explicitly marks stale or deprecated
- Author can update content → resets to active
- Deprecated resources hidden by default

**LearningEvent as multi-skill resource container:**
- Attendance = evidence on relevant skills, never a level-up trigger
- Session content (slides, recording, exercises) added to global resource library
- `skillCoverage: [{ skillId, fromLevel, toLevel }]` — one event covers multiple skills
- `targetProfiles: [profileId] | ALL | [multiple profiles]`
- Cross-competence events visible to all coaches; each coach manages attendance for their own consultants

**Progressive disclosure roadmap:**
- Default view: current niveau nodes (anchors) + immediate next tier (active targets)
- Nodes beyond collapsed into "future skills" summary
- Full tree opt-in via "Show full roadmap"
- Consultant with 45 nodes sees 8-12 at any time

**Two-layer skill architecture:**
- Layer 1: Itenium Skills — universal, HR-owned, inherited by every profile automatically
- Layer 2: Profile Skills — competence-center owned, no duplication across centers

---

## Complete Idea Register

| # | Idea | Phase |
|---|---|---|
| Profiles #1-10 | Career path branching, T/I-shape models, multi-path subscriptions, coach role, peer suggestions, PDP | Morphological |
| Coach #1-3 | PDP as first-class entity, AI transcript → PDP draft, coaching session log | Morphological |
| Skills #1-5 | Variable granularity levels, skill dependencies, skill decay/freshness, resource linkage, evidence types | Morphological |
| Visualization #1-3 | Roadmap.sh-style skill tree, gap heatmap, T-shape radar chart | Morphological |
| Model #1 | Variable-depth skills (levelCount:1 = checkbox, levelCount:N = progression) | Model proposition |
| Model #2 | Seniority as computed threshold (set of minLevel per skill), not a manually assigned label | Model proposition |
| Model #3 | Template fork model (consultant matrix inherits from versioned profile template) | Model proposition |
| RP-A1 | Readiness flag — considered action, one per goal, aging indicator for coach | Role Playing |
| RP-A2 | Live session mode — minimal 2-tap UI during coaching sessions | Role Playing |
| RP-B1 | Pre-session talking points — auto-generated from activity data | Role Playing |
| RP-B2 | Skill proposal queue — coach adds, vote promotes to global | Role Playing |
| RP-B3 | Cohort learning trigger — systemic gap surfaces group intervention suggestion | Role Playing |
| RP-C1 | Slack bot for resource sharing — nice-to-have, in spec | Role Playing |
| RP-D1 | Visual profile builder — drag-and-drop canvas, versioned | Role Playing |
| RB-1 | Coach dashboard always-on with aging indicators and activity signals | Reverse Brainstorm |
| RB-2 | Growth framing first; HR/sales as background exports | Reverse Brainstorm |
| RB-3 | Resource quality lifecycle (flagged → stale → deprecated, multi-signal) | Reverse Brainstorm |
| RB-4 | LearningEvent = multi-skill resource container; attendance ≠ level-up | Reverse Brainstorm |
| RB-5 | Readiness flag as considered action (one active per goal) | Reverse Brainstorm |
| RB-6 | Junior onboarding: simplified view until first coaching session | Reverse Brainstorm |
| RB-7 | Progressive disclosure roadmap (next steps only, full tree opt-in) | Reverse Brainstorm |
| RB-8 | Cross-competence LearningEvents global; targetProfiles: ALL or multiple | Reverse Brainstorm |

---

*Session complete. Resumed 2026-02-27, finalized 2026-02-27.*
