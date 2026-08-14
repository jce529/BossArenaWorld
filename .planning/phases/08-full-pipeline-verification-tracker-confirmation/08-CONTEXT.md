# Phase 8: Full Pipeline Verification & Tracker Confirmation - Context

**Gathered:** 2026-08-14
**Status:** Ready for planning

<domain>
## Phase Boundary

Verifies the complete subworld-kill-to-main-world-apply pipeline end-to-end, in singleplayer, for **every registered boss across every integrated mod** — not just one representative boss per mod, per the scope expansion decided in this session (see Decisions below). Confirms applied downed flags are recognized by Boss Checklist (or an equivalent tracker mod), not just internally consistent. All runs performed against a backed-up world save per Phase 1's established guidance.

</domain>

<decisions>
## Implementation Decisions

### Scope expansion — full roster, not "one per mod"
- **D-01:** Phase 8 was originally scoped (before Phase 9/10 existed) to "at least one boss per registered mod." The user explicitly expanded this to cover **every registered boss across every integrated mod** — vanilla (King Slime), Calamity (Hive Mind + Phase 10's full ~12-boss roster), Spirit (Infernon + Phase 10's full ~6-boss roster), Redemption (Thorn), CatalystMod (Astrageldon), ContinentOfJourney/Daybreak i.e. Homeward Journey (Goblin Chariot, per 07-RESEARCH.md).
  - **Rationale (user's own words):** "이미 각 페이즈에서 모드별로 한 보스씩 작동을 확인했고 문제가 없음을 확인 (BossCheckList까지 전부) 그래서 이제 모든 보스로 범위를 넓힐 생각" — each mod-integration phase already proved its one-worked-example boss works end-to-end, so Phase 8's remaining job is full-roster breadth confirmation, not re-proving the underlying mechanism.
  - **Important correction surfaced during this discussion, not just accepted at face value:** Boss Checklist recognition specifically (not just the internal flag/side-effect) was only explicitly confirmed for **Infernon** (Phase 5). King Slime (Phase 3) allowed an alternative confirmation method instead of requiring Boss Checklist specifically; Hive Mind (Phase 4) confirmed its own side effects (Sky Ore chat broadcast, netcode sync) but not Boss Checklist recognition; Thorn/Astrageldon (Phase 6) verification is still pending its own 06-03 live checkpoint. **Phase 8 must close Boss Checklist recognition for the original baseline set too, not only the newly-added Phase 9/10 roster.**
  - `ROADMAP.md` Phase 8 Goal/Success Criteria and `REQUIREMENTS.md` VERIFY-01/VERIFY-03 have already been updated in this session to reflect the expanded scope — researcher/planner should treat this as already-locked project state, not re-derive it.

### Claude's Discretion
- Exact plan/task structure for verifying dozens of bosses (likely grouped by mod or by wave, mirroring the per-mod/per-boss checklist granularity established in `06-03-PLAN.md`'s `check.md` and Phase 9/10's `*-VALIDATION.md` manual-checklist patterns) — this is a planning-shape decision, not a vision decision, left to the planner.
- Whether Phase 8 execution should logically wait until Phase 6 (06-03 live checkpoint), Phase 7 (execution, not just research), and Phase 10 (execution) all actually complete before its own live checkpoints can run — obviously implied by the dependency chain (you cannot live-verify a boss that hasn't been registered/executed yet), not a separate decision to make now. Planning/context-capture can proceed regardless.
- Whether to fold Homeward Journey's own bundled `CoJ_BossChecklist.cs` integration confirmation (flagged as Open Question 1 in `07-RESEARCH.md` — Homeward Journey ships its own Boss Checklist hook for `downedGoblinChariot`, very likely auto-recognized but not yet live-verified) into Phase 8's Goblin Chariot checkpoint specifically, rather than treating it as a separate Phase 7 concern — recommend folding it in here since Phase 8 is exactly the "confirmed recognized by Boss Checklist" phase.

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Requirements this phase implements
- `.planning/REQUIREMENTS.md` §"Verification & Safety (VERIFY)" — VERIFY-01, VERIFY-03 (both updated in this session to reflect the full-roster scope expansion)
- `.planning/ROADMAP.md` §"Phase 8" — Goal/Success Criteria/Scope note (updated in this session)

### What "every registered boss" actually means at planning time — read these for the authoritative rosters
- `.planning/phases/03-bossregistry-bosscoreitem-globalnpc-pipeline-proof-of-concept/` — King Slime (vanilla baseline)
- `.planning/phases/04-calamity-integration-cross-mod-side-effect-reproduction/` — Hive Mind
- `.planning/phases/05-spirit-integration/` — Infernon (only boss with confirmed Boss Checklist recognition so far)
- `.planning/phases/06-redemption-catalystmod-integration/06-CONTEXT.md`, `06-03-PLAN.md`, `check.md` — Thorn, Astrageldon (live verification still pending as of this session)
- `.planning/phases/07-noxusboss-continentofjourney-daybreak-integration/07-RESEARCH.md` — Goblin Chariot (Homeward Journey), including Open Question 1 re: its bundled `CoJ_BossChecklist.cs` integration
- `.planning/phases/09-biome-dependent-subworld-coverage/` — biome-routed bosses (Dragonfolly, Scarabeus, etc. once Phase 10 registers them) and the 7 biome-variant subworlds
- `.planning/phases/10-full-calamity-spirit-boss-roster-registration-and-biome-subworld-routing/10-RESEARCH.md`, `10-VALIDATION.md` — the full Calamity/Spirit roster this phase must also cover (once Phase 10 executes) — note Exo Mechs and Starplate Voyager are permanently excluded from all scope (see `PROJECT.md` Out of Scope), so they are NOT part of Phase 8's "every registered boss" either

### World-safety / process references
- `docs/WORLD_BACKUP_GUIDANCE.md` — backup procedure before any live verification touching the main save (Phase 8 Success Criterion 3)

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- No new code is expected for this phase — it is a verification-only phase (mirrors Phase 8's original design intent and the "manual-only, no automated in-game test harness exists" precedent established since Phase 1)
- `check.md`-style manual checklist files (Phase 6 precedent) and `*-VALIDATION.md` manual-checklist tables (Phase 9/10 precedent) are this project's established format for structuring a large batch of live in-game checkpoints

### Established Patterns
- Live in-game verification against a backed-up/throwaway world is the only verification method available in this project — no automated tModLoader test harness exists or is planned
- Boss Checklist (JavidPack/BossChecklist) is the reference tracker mod this project checks recognition against; Boss Checklist itself is a soft dependency the mod already interoperates with by design (no new integration code needed on this project's side — Boss Checklist reads whatever downed flags this project's `BossRegistry.Apply()` already sets)

### Integration Points
- None — this phase consumes the output of Phases 3-7, 9, and 10, it does not modify `Systems/`, `Integrations/`, or `Subworlds/`

</code_context>

<specifics>
## Specific Ideas

No visual/content specifics — this discussion was entirely about resolving a scope question (how much of the now-much-larger boss roster Phase 8 needs to cover) created by Phase 9/10 being added to the roadmap after Phase 8 was originally scoped.

</specifics>

<deferred>
## Deferred Ideas

None — discussion stayed within phase scope. (Exo Mechs and Starplate Voyager are not "deferred" from Phase 8 specifically — they are permanently out of v1 scope entirely, per the Phase 10 planning decision recorded in `PROJECT.md`, so they were never part of Phase 8's roster to begin with.)

### Reviewed Todos (not folded)
None — `todo match-phase` returned 0 matches for Phase 8.

</deferred>

---

*Phase: 08-full-pipeline-verification-tracker-confirmation*
*Context gathered: 2026-08-14*
