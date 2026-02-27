# AI Bootcamp Prep Checklist

**Event:** AI Bootcamp
**Date:** March 13, 2026
**Participants:** ~36 people, 5-6 teams
**Product:** SkillForge

---

## BEFORE BOOTCAMP (Days/Weeks Ahead)

### Subscriptions & Tech
- [ ] Provision Max 5x Claude subscriptions for all 36 participants
- [ ] Verify everyone completed Prep.md setup (Docker, .NET 10, Bun, etc.)
- [ ] Pre-pull Docker images to avoid day-of network issues
- [ ] Create 5-6 team repos from template codebase
- [ ] Set up hidden `_backup-epics` branch in each repo

### Content & Docs
- [ ] Finalize PRD document
- [ ] Finalize Architecture document
- [ ] Prepare Epics on hidden branch (escape hatch)
- [ ] Complete slides 20-21 (Managing Context, Agent Orchestration) if needed
- [ ] Create "While Claude Thinks" task list (review PR, write test, sketch next feature)

### Team Composition
- [ ] Pre-assign 36 people into 5-6 teams
- [ ] Balance each team: mix of junior/senior, stack experts/outsiders
- [ ] Identify potential "AI coaches" (enthusiasts who can teach)
- [ ] Identify potential "quality gates" (seniors/skeptics who review)

### Communication
- [ ] Send prep reminder 1 week before (verify setup!)
- [ ] Share team assignments 2-3 days before
- [ ] Set expectations: learning + bonding + competing, not just shipping

---

## DAY-OF SETUP (9h00)

### Kickoff Additions
- [ ] Announce judging criteria clearly (quality counts, not just features)
- [ ] Announce escape hatch: "Hidden epics branch exists if you need it"
- [ ] Announce timebox: "First commit by 10h30"
- [ ] Remind: "13h demo can be ONE button that works — that's fine"

### Team Roles Prompt
- [ ] Suggest teams do 5-min "team contract" at 9h15:
  - Who's the git wrangler?
  - Who's the AI coach (if enthusiast present)?
  - Who's the quality reviewer?
  - Who's the demo presenter?
  - Juniors: claim your first task explicitly

---

## FALLBACK PLANS

| Risk | Trigger | Action |
|------|---------|--------|
| Rate limit hit | Claude errors/slowdowns | Pair up with teammate |
| Team paralyzed | No commits by 10h30 | PO check-in, point to hidden epics |
| Merge conflicts | Pre-demo panic | Git wrangler + senior help |
| Laptop dies | Hardware failure | Pair programming |
| Docker issues | Should be pre-mitigated | Shared cloud Postgres backup? |
| Not having fun | Visible frustration/disengagement | PO/facilitator check-in, reassign role |

---

## PERSONA-SPECIFIC PREP

| Persona | Prep Action |
|---------|-------------|
| Juniors | Tag "good first issue" stories in backlog |
| Seniors | Brief them on reviewer/architect role option |
| AI Enthusiasts | Ask them to coach, not dominate |
| Skeptics | Frame as "QA role — your doubt is valuable" |
| Introverts | Ensure pair option, not forced mob |
| Stack Outsiders | Assign domain expert / PO tasks |
| Socializers | Morale + demo presenter role is valid |

---

## KEY DECISIONS MADE

| Decision | Choice |
|----------|--------|
| Claude subscription | Max 5x ($100/mo), itenium expenses |
| Prep level | PRD + Architecture visible, Epics on hidden branch |
| Team repos | Per-team (not shared mono-repo) |
| Team composition | Pre-assigned, balanced |
| BMAD usage | Optional (team decides) |

---

*Generated from brainstorming session 2026-02-06, continued 2026-02-25*
