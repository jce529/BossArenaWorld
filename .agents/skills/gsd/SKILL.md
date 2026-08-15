---
name: gsd
description: >-
  Autonomous, spec-driven project execution framework using .planning/ artifacts (PROJECT.md, ROADMAP.md, STATE.md, REQUIREMENTS.md, phases/).
  Use this skill whenever the user requests GSD workflows, roadmap/phase planning, phase execution, progress checks, quick tasks, debug sessions, or milestone management.
---

# GSD (Get Shit Done) Workflow Skill

GSD is an autonomous, spec-driven project execution and planning framework designed for agentic pair-programming. It maintains persistent project memory and strictly tracks requirements and verification across phases.

---

## 1. Project Planning Architecture (`.planning/`)

```
.planning/
├── PROJECT.md          # Core value, constraints, vision, key architecture decisions
├── REQUIREMENTS.md     # Scoped v1/v1.x/v2 requirements with IDs (e.g. REQ-01) and traceability matrix
├── ROADMAP.md          # Milestones, sequential phases, success criteria, plan links
├── STATE.md            # Active project memory, current phase/plan position, progress metrics
├── config.json         # Workflow mode, model profiles, flags
├── phases/             # Phase-specific execution folders
│   └── XX-phase-name/
│       ├── XX-CONTEXT.md       # Scope, design decisions, invariants
│       ├── XX-YY-PLAN.md       # Executable task plans with verification criteria
│       └── XX-YY-SUMMARY.md    # Execution summaries and diffs
├── quick/              # Ad-hoc tasks (outside main roadmap)
└── debug/              # Persistent debug investigation sessions
```

---

## 2. Core Workflow Commands & Lifecycle

### 1) Progress & Status
- **Progress (`/gsd:progress`)**: Read `.planning/STATE.md` and `.planning/ROADMAP.md`, summarize completed work, and identify the exact next action.
- **Resume (`/gsd:resume-work`)**: Rehydrate session context from `STATE.md`.

### 2) Planning
- **Discuss Phase (`/gsd:discuss-phase <N>`)**: Solicit user intent, domain boundaries, and essential constraints; produce `XX-CONTEXT.md`.
- **Plan Phase (`/gsd:plan-phase <N>`)**: Create concrete, atomic `XX-YY-PLAN.md` files with verifiable `must_haves` and step-by-step tasks. Update `ROADMAP.md` and `STATE.md` to `Ready to execute`.

### 3) Execution
- **Execute Phase (`/gsd:execute-phase <N>`)**:
  1. Load `XX-YY-PLAN.md`.
  2. Implement all tasks in the plan.
  3. Verify against plan `verification` criteria (e.g. build tests, unit tests, code inspection).
  4. Write `XX-YY-SUMMARY.md`.
  5. Update `ROADMAP.md` and `STATE.md`.

### 4) Quick Tasks & Debugging
- **Quick (`/gsd:quick [task]`)**: Execute small isolated tasks with planner + executor into `.planning/quick/`.
- **Debug (`/gsd:debug [issue]`)**: Scientific method debugging (evidence → hypothesis → test) tracked in `.planning/debug/`.

---

## 3. Strict Operating Invariants

1. **Never Skip State Updates**: Every plan creation or completion MUST update `.planning/STATE.md` and `.planning/ROADMAP.md`.
2. **Preserve Integrity**: Do not overwrite or regress existing verified requirements in `REQUIREMENTS.md`.
3. **Continuous Verification**: Ensure project compiles cleanly (`dotnet build` or equivalent) after each task.
