# Requirements: BossArenaSubWorld

**Defined:** 2026-08-12
**Core Value:** The generic boss-kill → carrier-item → main-world-apply mechanism must reliably reproduce a boss's full "downed" state (flags, netcode sync, WorldGen side effects) in the main world, for any registered boss.

## v1 Requirements

### Subworld & Entry (SUBW)

- [x] **SUBW-01**: A central mapping registers which existing boss-summon items (vanilla or modded, limited in v1 to simple "use item to summon" types — not altar-thrown or bulb-break style triggers) redirect into the subworld, keyed to the target boss
- [x] **SUBW-02**: A new placeable portal tile (`BossPortalTile`, working name "Test1" — visually benchmarked off the Corruption Altar sprite, a brand-new custom tile with no inherited vanilla altar behavior such as hammer-smash hardmode triggering) is the entry point; right-clicking it while holding a registered boss-summon item triggers the redirect, gated by the registry from SUBW-01
- [x] **SUBW-03**: The redirect sends the player into the boss-arena subworld as the altar interaction's next step
- [x] **SUBW-04**: The boss automatically summons inside the subworld once the player arrives, by reusing (replaying) the same held summon item's own use-effect there — no per-boss spawn logic needed — and the item is not consumed by the round trip
- [x] **SUBW-05**: The boss-arena subworld has zero placed mod content (custom `GenPass` list, not vanilla/modded worldgen) — this is the actual FPS-avoidance guarantee
- [x] **SUBW-06**: Player can reliably exit/return from the subworld to the main world

### Boss Detection & Carrier Item (DROP)

- [x] **DROP-01**: A central NPC.type → bossKey mapping registers which NPCs (across all covered mods) count as trackable bosses
- [x] **DROP-02**: Registered bosses drop a `BossCoreItem` via a conditional `ItemDropRule` added in `GlobalNPC.ModifyNPCLoot`, gated to only trigger when the kill happens inside the boss-arena subworld
- [x] **DROP-03**: `BossCoreItem` stores which boss it corresponds to (`BossKey`) as instance data (`Clone`/`SaveData`/`LoadData`), set at spawn time inside the custom drop rule

### Progress Application (APPLY)

- [x] **APPLY-01**: Using `BossCoreItem` in the main world calls `BossRegistry.Apply(key)`, which sets the corresponding boss's downed flag
- [x] **APPLY-02**: `BossRegistry.Apply` reproduces each source mod's netcode/messaging side effects (e.g. `CalamityNetcode.SyncWorld()`, `SetNewBossJustDowned()`)
- [x] **APPLY-03**: `BossRegistry.Apply` reproduces WorldGen side effects for world-altering bosses (ore generation, dungeon activation, etc.), not just the boolean flag
- [x] **APPLY-04**: Applying a boss's progress is idempotent — re-using the item after a partial failure does not double-apply rewards or duplicate netcode messages (world-scoped vs. player-scoped side effects classified explicitly to avoid double-granting anything that already survives the subworld round-trip via the live player object)

### Mod Coverage (MOD)

- [x] **MOD-01**: Calamity bosses registered via the `DownedBossSystem` wrapper-property pattern
- [x] **MOD-02**: Spirit bosses registered via the `MyWorld` static-field pattern
- [x] **MOD-03**: Redemption bosses researched (downed-progress API) and registered
- [x] **MOD-04**: CatalystMod bosses researched (downed-progress API) and registered
- [~] ~~**MOD-05**: NoxusBoss (Devourer of Universes) researched (downed-progress API) and registered~~ — **Removed** during Phase 7 discuss-phase (2026-08-14); see Out of Scope table
- [x] **MOD-06**: ContinentOfJourney / Daybreak (identified as Homeward Journey, GabeHasWon, Steam Workshop id 2930931197) bosses researched (downed-progress API) and registered

### Verification & Safety (VERIFY)

- [ ] **VERIFY-01**: Full pipeline (subworld kill → item drop → main-world apply) verified end-to-end in singleplayer for every registered boss across every integrated mod — expanded during Phase 8 discuss-phase (2026-08-14) from the original "at least one boss per mod" scope, since each mod-integration phase already proved its one-worked-example boss and Phase 9/10 have since expanded the total registered roster substantially
- [x] **VERIFY-02**: World-backup guidance is documented and followed before any live testing against a real save
- [ ] **VERIFY-03**: Applied flags are confirmed recognized by Boss Checklist (or equivalent tracker mod) after application — not just internally consistent. Scope now matches VERIFY-01's full-roster expansion; note only Infernon (Phase 5) has explicitly confirmed Boss Checklist recognition so far — King Slime (Phase 3) used an alternative confirmation method, Hive Mind/Thorn/Astrageldon confirmed their own side effects but not Boss Checklist specifically, so this still needs closing even for the original baseline set

### Arena Biome Coverage (ARENA)

- [ ] **ARENA-01**: Every v1-registered boss across all integrated mods is explicitly classified (via source research) as biome/Zone-flag-dependent or not, and every boss classified as dependent has a matching routed biome-variant subworld (via `BossArenaRoutingRegistry`) — generalizing the ad-hoc `BossArenaCorruptionSubworld` fix built for Calamity's Hive Mind in Phase 4. Phase 9 (09-01..09-07) closes the arena-construction/JIT-safety half: 7 biome-variant subworlds built, live-verified to satisfy their target Zone/Biome flag, and live-confirmed JIT-safe with their source mod disabled. Boss-classification-and-routing across Phase 7 (ContinentOfJourney/Daybreak, i.e. Homeward Journey) and Phase 10 (full Calamity/Spirit roster) remains outstanding before this requirement is fully complete. NoxusBoss dropped from scope entirely (Phase 7 discuss-phase), so it no longer contributes to this requirement. Phase 10 Plan 01 (2026-08-14) built the shared polymorphic-item/forced-night foundation Plans 10-02..10-06 register real bosses against -- requirement remains open until those plans complete. Phase 10 Plan 02 (2026-08-14) registered Calamity Tier 1 (Devourer of Gods, Yharon, Supreme Witch Calamitas -- plain arena; Dragonfolly -- routed to `BossArenaJungleSubworld`) -- requirement still remains open pending Plans 10-03..10-06 and Plan 10-06's live in-game verification.

## v2 Requirements

Deferred to future release. Tracked but not in current roadmap.

### Multiplayer

- **MP-01**: Dedicated-server / subserver-synced carrier-item pipeline

### UX Polish

- **UX-01**: Multi-boss/combo-encounter handling (paired fights like Infernum's Bereft Vassal + Great Sand Shark)
- **UX-02**: In-subworld boss selection/summon UI for multiple bosses per visit

## Out of Scope

Explicitly excluded. Documented to prevent scope creep.

| Feature | Reason |
|---------|--------|
| Multiplayer / dedicated-server support | Subserver sync adds real desync/duplicate-application risk on top of an already-fragile sync gap; ship singleplayer-only until the carrier-item pipeline is proven reliable |
| Automatic/implicit subworld entry (auto-detecting an imminent boss fight from game state, e.g. proximity/health/time-of-day heuristics) | Reliable detection is itself a hard, mod-specific problem across many content mods; removes player control. Note: this is distinct from SUBW-01/02 (redirecting on explicit summon-item use), which is still a deliberate player action, just reusing an existing item instead of a new dedicated portal item |
| Silent automatic resync-on-every-transition (mirroring SubworldLibrary's own vanilla-flag sync) | Not idempotent-safe for WorldGen side effects; explicit one-shot carrier-item use is easier to reason about and retry safely |
| Boss-priority ordering / phased mod rollout by "worst offender" | Marginal registration cost is uniform once the BossRegistry/BossCoreItem/GlobalNPC skeleton exists; no benefit to special-casing specific bosses first |
| Full arena-building/decoration toolkit (auto-platforms, campfires, aesthetics) | Out of scope for the progress-sync core value; duplicates existing dedicated arena-builder mods (e.g. Luiafk) |
| Generic full player-state mirroring (health/buffs/position) | SubworldLibrary already carries the live player object across the subworld boundary; a parallel mirroring layer would be redundant |
| NoxusBoss (Devourer of Universes and its other bosses) — was MOD-05 | Removed during Phase 7 discuss-phase (2026-08-14). Most NoxusBoss bosses are quest-triggered (Solyn's moon-event questline) or already run in their own dedicated subworld/arena mechanic, so they don't fit this project's plain-summon-item carrier-item redirect pattern. No plan to revisit |

## Traceability

Which phases cover which requirements. Updated during roadmap creation.

| Requirement | Phase | Status |
|-------------|-------|--------|
| SUBW-01 | Phase 2 | Complete |
| SUBW-02 | Phase 2 | Complete |
| SUBW-03 | Phase 2 | Complete |
| SUBW-04 | Phase 2 | Complete |
| SUBW-05 | Phase 1 | Complete |
| SUBW-06 | Phase 1 | Complete |
| DROP-01 | Phase 3 | Complete |
| DROP-02 | Phase 3 | Complete |
| DROP-03 | Phase 3 | Complete |
| APPLY-01 | Phase 3 | Complete |
| APPLY-02 | Phase 4 | Complete |
| APPLY-03 | Phase 4 | Complete |
| APPLY-04 | Phase 3 | Complete |
| MOD-01 | Phase 4 | Complete |
| MOD-02 | Phase 5 | Complete |
| MOD-03 | Phase 6 | Complete |
| MOD-04 | Phase 6 | Complete |
| MOD-05 | — | Removed (Phase 7 discuss-phase, 2026-08-14) |
| MOD-06 | Phase 7 | Complete |
| VERIFY-01 | Phase 8 | Pending |
| VERIFY-02 | Phase 1 | Complete |
| VERIFY-03 | Phase 8 | Pending |
| ARENA-01 | Phase 9 | Partial (arena-construction/JIT-safety half complete; boss-classification-and-routing awaits Phases 6-8) |

**Coverage:**
- v1 requirements: 23 total (22 active, 1 removed — MOD-05)
- Mapped to phases: 22 (100% of active requirements)
- Unmapped: 0

---
*Requirements defined: 2026-08-12*
*Last updated: 2026-08-14 — Phase 7 discuss-phase: MOD-05 (NoxusBoss) removed from scope, MOD-06 target identified as Homeward Journey*
