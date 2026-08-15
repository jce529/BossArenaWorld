# Project Retrospective

*A living document updated after each milestone. Lessons feed forward into future planning.*

## Milestone: v1.0 — MVP

**Shipped:** 2026-08-15
**Phases:** 10 | **Plans:** 36 | **Sessions:** ~3 (2026-08-12 → 2026-08-15)

### What Was Built
- Subworld skeleton (SubworldLibrary-based) proven content-free, with the "flags don't cross worlds" isolation premise empirically verified (and SubworldLibrary's own opposite-direction vanilla-flag sync discovered and neutralized)
- Portal-tile entry mechanism redirecting an existing boss-summon item into the arena, auto-summoning the boss on arrival
- Generic BossRegistry + BossCoreItem + GlobalNPC carrier-item pipeline, proven end-to-end with King Slime before content-mod integration
- 17 bosses across 5 content mods registered end-to-end (Calamity 11, Spirit 7 incl. Infernon, Redemption 1, CatalystMod 1, Homeward Journey 1), each reproducing its source mod's actual flag + netcode + WorldGen side effects
- 7 biome-variant arena subworlds (Hallow, Underworld, Jungle, Space, Desert, Astral, Briar) routing biome-dependent bosses to a matching Zone/Biome-satisfying arena
- Full InfernumMode-conditional gating matrix (presence/absence redirect, forced-night persistence, and — post-ship — an explicit per-boss `RequiresInfernumToggle` flag replacing two rounds of looser heuristics)
- Live Boss Checklist (tracker mod) recognition and per-mod-disabled JIT safety confirmed for every integration

### What Worked
- Decompiling the actual installed DLL (`ilspycmd`) before treating a plan's illustrative code as compilable caught multiple real bugs (wrong namespaces, `internal` visibility, non-existent field names) before they cost a build cycle — this became a standing precedent by Phase 6+
- Isolated-worktree parallel execution for independent-file plans (e.g. Phase 9 Wave 1, Phase 10 Plans 02/03) worked well once the team learned to cherry-pick `feat` commits onto master and hand-reconcile shared docs (STATE/ROADMAP/REQUIREMENTS) centrally, rather than `git merge`-ing worktree branches directly
- Root-causing bugs before deciding scope (Hive Mind/ZoneCorrupt in Phase 4, The Old Duke's despawn in the debug session) rather than working around them blind — even when the fix's boss was ultimately descoped anyway (The Old Duke), the general fix was correctly kept because it benefited other bosses
- Quick-task workflow (`/gsd:quick`) was effective for small, well-scoped post-ship refinements (the two-round InfernumMode gating fix) without full phase ceremony

### What Was Inefficient
- The InfernumMode gating condition went through three iterations in one day (unconditional → `calamity:` string-prefix → explicit flag) — the second iteration's naming-convention heuristic was flagged by code review almost immediately. A per-boss explicit flag should have been the first design considered, not the third.
- Several `gsd-tools` state-management commands (`state advance-plan`, `state update-progress`, `phase complete`) had recurring bugs against this project's STATE.md format (narrative "Plan:" line instead of "Current Plan: N/Total Plans: M"; case-insensitive regex matching frontmatter instead of body) — required manual workaround nearly every phase (Phases 05, 06, 07, 09)
- `gsd-executor` spawned with `isolation: worktree` failed when the plan file itself was uncommitted in the main working tree (fresh worktree checkout doesn't see uncommitted files) — cost one wasted executor round-trip during the post-ship quick tasks

### Patterns Established
- Delegates/lambdas passed into `[JITWhenModsEnabled]`-guarded registration calls must be named, separately-tagged methods, never inline lambdas (inline lambdas hoist into a `<>c` cache class that doesn't inherit the attribute)
- Every method touching a weak-referenced mod's types needs its own `[JITWhenModsEnabled]` attribute regardless of containing-class laziness (caught live in Phase 9's Astral/Briar `GenPass.ApplyPass`)
- Biome-gated bosses get a dedicated per-biome arena subworld routed via `BossArenaRoutingRegistry`, not an every-tick `Zone*` override in a `ModPlayer` — a real tile-based biome survives vanilla's per-tick recompute
- When a content mod's downed-progress field has no public setter, write via cached `FieldInfo` reflection wrapped in try/catch + `Mod.Logger.Warn`, replicating the mod's own `OnKill()` write exactly rather than skipping it
- Cross-cutting per-boss behavior flags (like InfernumMode dependency) belong as an explicit field on the boss's own registration record, not inferred from a naming convention on its key

### Key Lessons
1. When a research pass's decompile conclusion is later contradicted by live behavior (Hive Mind Phase 4, The Old Duke Phase 10), re-decompile with a wider net (check AI overrides / other mods' hooks into the same boss) rather than trusting the first pass's scope.
2. A scope-cut decision (descoping a boss) and a root-cause fix are independent — fixing the underlying bug is still worth keeping even if the specific boss that surfaced it gets cut, when the fix benefits other in-scope bosses.
3. Prefer an explicit per-entity flag over a string-prefix/naming-convention check from the first draft — the naming convention will eventually diverge from the real condition it's proxying for (concretely: `catalyst:astrageldon` vs. `calamity:*` bosses both being CalamityMod-dependent).

### Cost Observations
- Sessions: ~3 working sessions across 2026-08-12 to 2026-08-15
- Notable: heavy use of parallel isolated-worktree executors for independent-file plan waves (Phase 9, Phase 10) kept wall-clock time down despite 36 total plans

---

## Cross-Milestone Trends

### Process Evolution

| Milestone | Sessions | Phases | Key Change |
|-----------|----------|--------|------------|
| v1.0 | ~3 | 10 | Established decompile-before-trust-illustrative-code and worktree-cherry-pick-merge patterns |

### Cumulative Quality

| Milestone | Tests | Coverage | Zero-Dep Additions |
|-----------|-------|----------|---------------------|
| v1.0 | 0 automated (live in-game verification only) | N/A (Terraria mod, no unit-test harness) | 0 (SubworldLibrary was the only hard dependency; all content-mod integrations use weakReferences) |

### Top Lessons (Verified Across Milestones)

1. Decompile the actual installed DLL before treating a plan's illustrative cross-mod code as compilable — verified repeatedly (Phases 6, 9, 10) as the single highest-value bug-catching step in this codebase.
