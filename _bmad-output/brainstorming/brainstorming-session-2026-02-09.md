---
stepsCompleted: [1, 2]
inputDocuments: []
session_topic: 'SkillForge Competency Framework - Functional Requirements'
session_goals: 'Flesh out, challenge, and expand functional requirements for profiles, skills, employee tracking, learning material, reviews, and skill matrix visualization'
selected_approach: 'ai-recommended'
techniques_used: ['Morphological Analysis', 'Role Playing', 'Reverse Brainstorming']
ideas_generated: ['Profiles #1-10', 'Skills #1-5', 'Coach #1-3', 'Visualization #1-3']
context_file: ''
session_status: 'paused - Phase 1 Morphological Analysis in progress'
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

## Session Pause Notes

**Status:** Paused mid-Phase 1 (Morphological Analysis) — 18 ideas generated
**Techniques remaining:** Role Playing (Phase 2), Reverse Brainstorming (Phase 3)
**Open question at pause:** Who curates and assigns learning material? Team manager (administrative), competence coach (developmental), or consultants themselves? Could this differ by material type (official course vs. blog post)?

### Unexplored Intersections (Resume Here)
- Learning Material x Reviews x Skills (how reviews surface best materials per skill)
- Competence Coach x Visualization (what does the coach's dashboard look like?)
- Seniority Tiers x Skill Evidence (does proof-of-competence requirements change by level?)
- PDP x Team Manager (does the manager see the PDP? Is it private between coach + consultant?)
- AI Transcription x Skill Extraction (NLP pipeline details, accuracy, human-in-the-loop)
- Existing backlog integration (how do courses/quizzes/feedback map to the competency layer?)
- Skill decay x PDP (do decaying skills auto-surface in the development plan?)

### Codebase Context for Resumption
- Current entities: Teams, Courses, Users (with roles). No skill/profile/PDP entities yet.
- Current roles: backoffice, manager, learner. Competence coach role needs to be added.
- Teams seed data: Java, .NET, PO & Analysis, QA — maps to profile categories.
- Course entity is flat (name, description, category, level) — needs skill linkage.
- Frontend has nav skeleton for My Learning, Catalog, Team, Admin, Reports — none implemented yet.
