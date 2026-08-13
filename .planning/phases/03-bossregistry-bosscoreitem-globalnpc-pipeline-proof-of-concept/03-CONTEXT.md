# Phase 3: BossRegistry + BossCoreItem + GlobalNPC Pipeline (Proof of Concept) - Context

**Gathered:** 2026-08-13
**Status:** Ready for planning

<domain>
## Phase Boundary

Killing a registered boss inside the boss-arena subworld reliably carries a boss-kill credential (`BossCoreItem`) back to the main world and applies it exactly once, proven end-to-end with one low-risk vanilla boss (King Slime, continuing from Phase 1/2) before content-mod complexity is introduced. This phase does NOT reproduce Calamity/Spirit/etc.-specific netcode sync or WorldGen side effects (APPLY-02/APPLY-03 are Phase 4) — it proves the generic pipeline mechanism (registry lookup, drop, carry, apply, idempotency) using vanilla's own downed-flag pattern as the one worked example.

</domain>

<decisions>
## Implementation Decisions

### Idempotency (APPLY-04)
- **D-01:** `BossRegistry.Apply(key)` checks the boss's current downed state via a per-boss "already downed" getter (part of each `BossDefinition`) *before* calling the apply/side-effect logic. If already downed, `Apply()` is a no-op — no error, no re-application. No separate applied-tracking set is stored in `BossRegistry`'s own world data; checking the live flag is the single source of truth and generalizes cleanly to every future mod integration (Phase 4+), since each mod's registration only needs to supply one extra read-only getter alongside its existing apply delegate.

### BossCoreItem consumption policy
- **D-02:** `BossCoreItem` is consumed only when `Apply()` succeeds. On failure (e.g. registry lookup miss for the item's stored key, mod-specific data unavailable), the item is retained in the player's inventory and a chat message explains what happened, so the player can retry or report the issue. Matches `research/PITFALLS.md` UX Pitfalls guidance ("carrier item silently does nothing" is explicitly called out as the failure mode to avoid).

### BossRegistry key design
- **D-03:** Registry keys are namespaced strings, e.g. `"vanilla:king_slime"`, with future entries following the same `modprefix:boss_name` convention (e.g. `"calamity:desert_scourge"` in Phase 4). Keys are decoupled from raw `NPC.type` — a boss key maps to one or more NPC types (needed later for multi-phase bosses), not the reverse. Matches the `BossRegistry`/`BossDefinition` shape already sketched in `research/ARCHITECTURE.md` Pattern 2.

### King Slime downed-flag fidelity
- **D-04:** Applying King Slime's downed state replays the same helper vanilla itself uses on a real kill — `NPC.SetEventFlagCleared(ref NPC.downedSlimeKing, ...)` — not a raw boolean assignment. This reproduces vanilla's achievement-progression notification and its (singleplayer-no-op) multiplayer sync call, avoiding `research/PITFALLS.md` Pitfall 4 ("setting the raw boolean flag replicates less than what actually happened on a real kill") from the very first boss, and establishes the fidelity bar every Phase 4+ mod integration should match.

### Claude's Discretion
- `BossCoreItem` itemization (sprite/display name/rarity) for this POC — not discussed in depth; follow the `Test1Item` precedent (minimal/functional placeholder, no polish) since Phase 3 is proof-of-concept, not final itemization. It is obtained only via the kill drop (`ModifyNPCLoot` + `ItemDropRule`) — no debug give-command needed.
- Exact shape of the per-boss "already downed" getter on `BossDefinition` (e.g. `Func<bool> IsDowned` field vs. a method) — implementation detail for planning.
- Exact chat message wording for success/failure feedback (D-02).
- File/class naming within the `Systems/`, `GlobalNPCs/`, `Items/` structure already sketched in `research/ARCHITECTURE.md`.

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Requirements this phase implements
- `.planning/REQUIREMENTS.md` §"Boss Detection & Carrier Item (DROP)" — DROP-01 (NPC.type → bossKey registry), DROP-02 (`ModifyNPCLoot` + conditional `ItemDropRule`, subworld-gated), DROP-03 (`BossCoreItem` stores `BossKey` as instance data via `Clone`/`SaveData`/`LoadData`)
- `.planning/REQUIREMENTS.md` §"Progress Application (APPLY)" — APPLY-01 (`BossRegistry.Apply(key)` on item use), APPLY-04 (idempotent re-use, world-scoped vs. player-scoped classification)
- `.planning/PROJECT.md` §"Key Decisions" — the `ModifyNPCLoot` + `ItemDropRule` drop-mechanism decision is already locked; do not revisit as a gray area

### Architecture pattern
- `.planning/research/ARCHITECTURE.md` §"Pattern 2: Central registry as the only cross-cutting seam" — the `BossRegistry`/`BossDefinition` shape this phase implements (extend the sketched `BossDefinition` record with the "already downed" getter per D-01)
- `.planning/research/ARCHITECTURE.md` §"Recommended Project Structure" — file layout: `Systems/BossRegistry.cs`, `GlobalNPCs/BossKillGlobalNPC.cs`, `Items/BossCoreItem.cs`
- `.planning/research/ARCHITECTURE.md` §"Data Flow" → "Boss-kill-to-apply flow" — the exact sequence this phase must implement end-to-end

### Pitfalls governing this phase's design
- `.planning/research/PITFALLS.md` §"Pitfall 4: Setting the raw boolean flag replicates less than what actually happened on a real kill" — directly governs D-04
- `.planning/research/PITFALLS.md` §"Pitfall 5: Boss rewards split between world-scoped and player-scoped data cause double-grants" — relevant to APPLY-04; King Slime has no player-scoped reward, but the classification discipline this pitfall describes should be established now for Phase 4+ to reuse
- `.planning/research/PITFALLS.md` §"Security Mistakes" — validate the `BossCoreItem`'s stored key against the current `BossRegistry` before calling `Apply()`; fail gracefully rather than throwing
- `.planning/research/PITFALLS.md` §"UX Pitfalls" — governs D-02 (retain item + chat message on failure; confirm success via chat message on successful apply)

### Prior phase context (King Slime continuity)
- `.planning/phases/01-subworld-skeleton-isolation-proof/01-CONTEXT.md` — D-10/D-11/D-12: King Slime chosen as the isolation-proof test boss, real-kill methodology (not flag-toggling)
- `.planning/phases/02-summon-item-redirect-entry-registry/02-CONTEXT.md` — D-08: King Slime/Slime Crown reused for Phase 2's redirect proof; this phase continues the same boss for its own proof

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `Subworlds/BossArenaSubworld.cs` — `OnEnter()`/`OnExit()` now snapshot and force-restore ~30 vanilla downed/event flags (including `NPC.downedSlimeKing`) around every subworld visit, specifically to defend against SubworldLibrary's own `CopyDowned()`/`ReadCopiedDowned()` behavior. **This means a King Slime kill inside the subworld will NOT leak `NPC.downedSlimeKing = true` into the main world on its own** — the `BossCoreItem` pipeline built in this phase is now the sole legitimate path for that flag to change in the main world. This directly validates the phase's Core Value premise; no new isolation work is needed, only correct use of the existing guard.
- `Systems/SummonItemRegistry.cs` — existing `ModSystem`-based static `Dictionary<int,int>` registry pattern (`PostSetupContent` registration, static lookup methods). `BossRegistry` should follow the same structural convention (a `ModSystem` populated in `PostSetupContent`) for consistency, even though its value type is richer (`BossDefinition` with an apply delegate + downed-check getter, not a plain int).
- `Systems/BossSummonPlayer.cs` — established pattern for gating subworld-only behavior via `SubworldSystem.IsActive<BossArenaSubworld>()`; `BossKillGlobalNPC`'s `ModifyNPCLoot` gating should reuse this same check.

### Established Patterns
- `ModSystem.PostSetupContent()` is the established registration timing (used by `SummonItemRegistry`) — `BossRegistry`'s own boss registrations should register at the same lifecycle point.
- No existing `GlobalNPC` or `ModItem` exists yet in this codebase — this phase introduces both patterns for the first time (Phase 1/2 only used `ModTile`, `ModItem` for the placeable portal, and `ModPlayer`/`ModSystem`).

### Integration Points
- `NPC.SpawnOnPlayer(...)` in `BossSummonPlayer.OnEnterWorld()` is where King Slime is spawned inside the subworld — the new `GlobalNPC.ModifyNPCLoot` hook attaches to that same King Slime `NPC.type`, gated additionally by `SubworldSystem.IsActive<BossArenaSubworld>()`.
- `NPCID.KingSlime` is already referenced in `SummonItemRegistry.cs` (`Register(ItemID.SlimeCrown, NPCID.KingSlime)`) — the new `BossRegistry` registration for `"vanilla:king_slime"` reuses this same NPC type constant.

</code_context>

<specifics>
## Specific Ideas

- The four decisions in this phase (idempotency-by-flag-check, consume-on-success-only, namespaced string keys, vanilla-fidelity flag application) are explicitly meant to establish the *pattern* every Phase 4+ mod integration will reuse — not just solve King Slime's case narrowly. Each decision was chosen with "does this generalize to Calamity/Spirit/etc." as the deciding factor.

</specifics>

<deferred>
## Deferred Ideas

None — discussion stayed within phase scope. (APPLY-02/APPLY-03 mod-specific side-effect reproduction was raised only as a boundary clarification for D-04, not pulled into this phase's scope; it remains Phase 4's responsibility per `.planning/REQUIREMENTS.md`.)

</deferred>

---

*Phase: 03-bossregistry-bosscoreitem-globalnpc-pipeline-proof-of-concept*
*Context gathered: 2026-08-13*
