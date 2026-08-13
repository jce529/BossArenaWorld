# Phase 4: Calamity Integration & Cross-Mod Side-Effect Reproduction - Context

**Gathered:** 2026-08-13
**Status:** Ready for planning

<domain>
## Phase Boundary

Calamity bosses' full downed state (flag, netcode sync, and WorldGen side effects) is faithfully reproduced in the main world, and the safe cross-mod access pattern used by every later integration (Phase 5+: Spirit, Redemption, CatalystMod, NoxusBoss, ContinentOfJourney/Daybreak) is established here. This phase does NOT attempt broad Calamity boss coverage — it proves the pattern with exactly one worked example, the same POC-first discipline Phase 3 used for the vanilla pipeline.

</domain>

<decisions>
## Implementation Decisions

### Cross-mod access strategy
- **D-01:** Use `weakReferences` (in `build.txt`) + `[JITWhenModsEnabled]` for all Calamity-type-touching code (`CalamityMod.DownedBossSystem`, `CalamityNetcode`, `CalamityGlobalNPC`) — the tModLoader-official documented pattern, already the project's stated intended approach per `CLAUDE.md`'s Tech Stack "What NOT to Use" table. User explicitly chose this over `research/PITFALLS.md`'s suggested safer alternative (pure runtime reflection), after being shown the trade-off (official pattern + strict method-isolation discipline required, vs. reflection's structurally-safer-but-slower/fragile approach). Every method touching a Calamity type must be fully isolated in its own method/class and tagged `[JITWhenModsEnabled("CalamityMod")]` — no partial isolation, per `research/PITFALLS.md` Pitfall 2.

### Worked-example boss selection
- **D-02:** The phase's single worked-example boss is **the earliest Calamity boss (in progression order) that triggers a WorldGen side effect** (ore generation, dungeon activation, biome unlock, etc.) — not simply the easiest/lowest-risk boss (e.g. Desert Scourge, which has no WorldGen effect) and not a boss the user currently fights for real (e.g. a post-Moon Lord flagship boss). This single boss must prove BOTH success criterion 2 (netcode/messaging side effects) AND success criterion 3 (WorldGen side effects) at once. **Exact boss name is NOT yet determined** — user explicitly deferred this to research-phase, which must inspect the actual installed `CalamityMod.tmod` (confirmed present locally: `Mods/2026.6CalamityMod.tmod`) to identify which boss's `DownedBossSystem` entry is earliest-in-progression among those with a confirmed WorldGen hook.

### Scope (boss count)
- **D-03:** Register exactly **one** Calamity boss this phase (the boss identified per D-02). Do not attempt broader Calamity coverage now — mirrors Phase 3's "prove the generic pattern with one worked example" discipline. Registering additional Calamity bosses beyond this one is explicitly deferred (see Deferred Ideas below), not part of this phase's success criteria.

### Live verification approach
- **D-04:** The WorldGen-triggering test (confirming ore generation/dungeon activation/etc. actually fires) runs against a **freshly created, dedicated test world** — NOT the backed-up main save Phase 3 used. Rationale (user-confirmed): WorldGen effects permanently alter terrain, unlike Phase 3's flag-only changes, so isolating this test from the main save removes any risk of unwanted permanent changes to the player's real world.
- **D-05:** Success criterion 4 ("mod continues to load and run safely with CalamityMod disabled") is verified via a **real in-game checkpoint** — disable CalamityMod, launch, confirm no JIT crash — not just a code review of `[JITWhenModsEnabled]` boundaries. User explicitly chose the live test over code-review-only confidence, reasoning that JIT crashes aren't reliably caught by code review alone. This checkpoint should be structured similarly to Phase 3's `03-03` live-verification checkpoint plan.

### Claude's Discretion
- Exact Calamity boss name satisfying D-02 — resolved during research-phase, not user discretion but not yet known either.
- Exact shape/naming of the cross-mod access helper class (e.g. `Integrations/CalamityIntegration.cs` vs. other naming) — implementation detail for planning, following `research/ARCHITECTURE.md`'s "Recommended Project Structure".
- Exact `weakReferences` version pin syntax in `build.txt` for CalamityMod.
- Whether the WorldGen-test dedicated world is single-use-and-discarded or kept around for future phase reference — implementation/process detail, not a phase-blocking decision.

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Requirements this phase implements
- `.planning/REQUIREMENTS.md` §"Progress Application (APPLY)" — APPLY-02 (netcode/messaging side-effect reproduction), APPLY-03 (WorldGen side-effect reproduction)
- `.planning/REQUIREMENTS.md` §"Mod Coverage (MOD)" — MOD-01 (Calamity bosses registered via `DownedBossSystem` wrapper-property pattern)

### Cross-mod access pattern (locks D-01)
- `.planning/research/PITFALLS.md` §"Pitfall 2: JIT crashes from weak-reference code, even behind a correct null-check" — governs the strict per-method isolation discipline D-01 requires
- `.planning/research/SUMMARY.md` §"Research Flags" — the weak-reference vs. pure-reflection tension this phase resolves via D-01; also confirms the "Reflection/Weak-Reference Helper Layer" originally planned as its own phase was folded into Phase 4 (see `.planning/STATE.md` decisions log)
- `.planning/research/ARCHITECTURE.md` §"Phase 3: Reflection/Weak-Reference Helper Layer (Shared Infrastructure)" (as originally sketched, now folded into this phase) — the shared-helper shape: cached lookups via `ModLoader.TryGetMod` then `targetMod.Code.GetType(fullName)`, try/catch-and-log-per-boss failure isolation
- `https://github.com/tModLoader/tModLoader/wiki/Expert-Cross-Mod-Content` — official `weakReferences`/`[JITWhenModsEnabled]`/`Mod.Call` guidance (already cited in `research/SUMMARY.md` Sources)
- `./CLAUDE.md` §"What NOT to Use" — project-level mandate for `weakReferences` + `[JITWhenModsEnabled]` over `modReferences`, already aligned with D-01

### Calamity API specifics
- `.planning/PROJECT.md` §"Context" (Mod-specific research completed so far) — `CalamityMod.DownedBossSystem` wrapper properties calling `NPC.SetEventFlagCleared`; requires `CalamityNetcode.SyncWorld()` and `CalamityGlobalNPC.SetNewBossJustDowned()` side effects for D-02's boss
- `https://github.com/CalamityTeam/CalamityModPublic/blob/master/CalamityNetcode.cs` — confirms `SyncWorld()` is a safe no-op in singleplayer (relevant since this phase's live tests are singleplayer)
- `https://github.com/JavidPack/BossChecklist/blob/1.4/BossChecklistIntegrationExample.cs` — real-world weak-reference cross-mod example (cited in `research/SUMMARY.md` Sources)
- Locally installed reference: `Mods/2026.6CalamityMod.tmod` — the actual binary to inspect/decompile during research-phase to resolve D-02's boss name and confirm exact `DownedBossSystem` member names against the installed version (not just documentation)

### Pitfalls governing this phase's design
- `.planning/research/PITFALLS.md` §"Pitfall 3: Setting the raw boolean flag instead of replaying the full side-effect chain" — directly governs why D-02's boss must exercise netcode+WorldGen replay, not just the flag
- `.planning/research/PITFALLS.md` §"Pitfall 4: Reflection into another mod's internals breaks silently after that mod updates" — cache all reflective/weak-ref lookups once at `PostSetupContent`, wrap in try/catch with warning-level logging, disable only the affected boss rather than crashing the mod
- `.planning/research/PITFALLS.md` §"Pitfall 5: Player-scoped vs. world-scoped double-grants" — classify D-02's boss's rewards before replaying `Apply()`, same discipline Phase 3 established for King Slime (which had no player-scoped reward to worry about; this phase's boss might)

### Locked pattern from Phase 3 (extends unchanged into this phase)
- `.planning/phases/03-bossregistry-bosscoreitem-globalnpc-pipeline-proof-of-concept/03-CONTEXT.md` — D-01 (idempotency via live-flag check), D-02 (consume-on-success-only), D-03 (namespaced `"modprefix:boss_name"` keys — already anticipates `"calamity:boss_name"`), D-04 (vanilla-fidelity flag application via source mod's own setter, not raw boolean) — all four generalize directly to this phase's `BossDefinition` entry for D-02's boss

### World-safety / process references
- `docs/WORLD_BACKUP_GUIDANCE.md` — backup procedure; this phase's WorldGen test uses a fresh dedicated world instead (D-04), but the Calamity-disabled load-safety checkpoint (D-05) may still touch the main save and should follow this guidance
- `.planning/debug/resolved/isolation-premise-flag-persistence.md` — the `Subworlds/BossArenaSubworld.cs` `OnEnter()`/`OnExit()` flag-isolation fix that must remain intact through this phase; do not modify this file without re-confirming the guard still covers all touched flags

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `Systems/BossRegistry.cs` — `BossDefinition`/`ApplyResult`/`Apply`/`TryGetKeyForNpc` are already boss-agnostic and require no changes; this phase adds exactly one new `BossDefinition` entry (D-02's boss) with Calamity-specific `ApplyDowned`/`IsDowned` delegates wrapped per D-01's access strategy.
- `Items/BossCoreItem.cs`, `ItemDropRules/BossCoreDropRule.cs`, `GlobalNPCs/BossKillGlobalNPC.cs` — fully boss-agnostic already; registering the new Calamity boss key in `BossRegistry` automatically flows through the existing drop/carry/apply pipeline with zero changes to these files.
- Namespaced key convention `"modprefix:boss_name"` (Phase 3 D-03) already anticipates `"calamity:boss_name"` — reuse directly.

### Established Patterns
- `ModSystem.PostSetupContent()` registration timing (used by `SummonItemRegistry`, `BossRegistry`) — the new Calamity `BossDefinition` registers at the same lifecycle point, guarded by the mod-presence check the cross-mod helper provides.
- `weakReferences` + `[JITWhenModsEnabled]` is the project's already-declared intended pattern (`CLAUDE.md`), now confirmed (D-01) rather than revisited.

### Integration Points
- `build.txt` currently has `modReferences = SubworldLibrary` only — this phase adds a `weakReferences = CalamityMod@<version>` line (exact version pin: Claude's discretion at planning time, based on installed `2026.6CalamityMod.tmod`).
- New folder needed (naming: Claude's discretion, `research/ARCHITECTURE.md` suggests `Integrations/`) for the Calamity-specific registration code, isolating all `[JITWhenModsEnabled]`-tagged methods away from `BossRegistry.cs` itself so `BossRegistry` stays free of any hard Calamity-type references.

</code_context>

<specifics>
## Specific Ideas

- The worked-example boss selection criterion (D-02) is deliberately narrow and non-obvious: "earliest Calamity boss with a WorldGen side effect," not "easiest boss" and not "a boss the user currently fights." This lets one boss prove two success criteria (netcode + WorldGen) at once while staying as low-risk as the progression order allows.
- Both live checkpoints this phase needs (WorldGen trigger test, Calamity-disabled load test) should be structured as checkpoint tasks similar in shape to Phase 3's `03-03-PLAN.md`, but the WorldGen one specifically must NOT reuse the backed-up main save — it needs a throwaway dedicated test world (D-04), a deliberate departure from Phase 3's approach.

</specifics>

<deferred>
## Deferred Ideas

- **Registering additional Calamity bosses beyond the one D-02 worked example** — explicitly out of this phase's scope (D-03). Belongs to either a future broader-coverage pass or is left implicit once the pattern is established (any boss can be added by following this phase's pattern with near-zero marginal registration cost, per `PROJECT.md`'s "No boss priority ordering in v1" decision).
- **Registering a specific late-game/flagship Calamity boss the user actually fights for FPS-relief purposes** — the user considered this as an option for the worked example but chose the WorldGen-first criterion instead (D-02). Not lost — once the pattern is proven, adding the user's actual target bosses is low-marginal-cost future work, same reasoning as the point above.

### Reviewed Todos (not folded)
None — no pending todos matched Phase 4 (`todo match-phase` returned 0 matches).

</deferred>

---

*Phase: 04-calamity-integration-cross-mod-side-effect-reproduction*
*Context gathered: 2026-08-13*
