# Phase 5: Spirit Integration - Context

**Gathered:** 2026-08-13
**Status:** Ready for planning

<domain>
## Phase Boundary

Spirit bosses' downed state is registered and applied correctly using Spirit's actual downed-progress API, proving the generic `BossRegistry` pattern generalizes across a structurally different API shape than Calamity's wrapper-property pattern. This phase does NOT attempt broad Spirit boss coverage — it proves the pattern with exactly one worked example, the same POC-first discipline Phase 3/4 used.

</domain>

<decisions>
## Implementation Decisions

### API pattern correction (supersedes PROJECT.md)
- **D-01:** `PROJECT.md`'s existing note ("Spirit: `SpiritMod.MyWorld`, plain public static bool fields, no wrapper needed") is **OUTDATED** for the actually-installed copy and must be corrected before/during planning. Confirmed by reading the real decompiled source (`ModReader/SpiritMod/MyWorld.cs`, `ModReader/SpiritMod/NPCs/BossDownedTracker.cs`): real bosses (Scarabeus, AncientFlyer, SteamRaiderHead, Atlas, Infernon/InfernoSkull, MoonWizard, ReachBoss1, Dusking) are tracked via `SpiritMod.NPCs.BossDownedTracker`, a `GlobalNPC` with a static `Dictionary<string, bool> Downed` keyed by `"{npc.type}"` (vanilla) or `"{ModName}/{ModNPCName}"` (modded), auto-populated in `BossDownedTracker.OnKill(NPC npc)` when `npc.boss` is true. `MyWorld`'s plain static bool fields (`downedMechromancer`, `downedOccultist`, etc.) remain accurate ONLY for non-boss events/minibosses — explicitly commented in source as "These aren't bosses and so aren't tracked with BossDownedTracker". The "wrapper-property vs. raw-static-field" framing in `REQUIREMENTS.md`/`ROADMAP.md` (MOD-02, Phase 5 Success Criterion 1: "the `MyWorld` static-field pattern") needs research-phase to reconcile with this finding — the real access pattern is `BossDownedTracker.IsBossDowned<T>()` / `BossDownedTracker.Downed[key] = true`, not a `MyWorld` field write, for any boss in the worked-example candidate list.
- Persistence/netcode for this pattern: `BossDownedTrackingIO : ModSystem` handles `SaveWorldData`/`LoadWorldData` (world-scoped TagCompound, same shape as Calamity's `DownedBossSystem`) and `OnWorldUnload()` clears the in-memory dictionary. `BossDownedTracker.OnKill()` sends `NetMessage.SendData(MessageID.WorldData)` only when `Main.netMode != NetmodeID.SinglePlayer` — a singleplayer no-op, same category as `CalamityNetcode.SyncWorld()` in Phase 4.

### Worked-example boss selection
- **D-02:** The worked-example boss is **Infernon** (tracked via `BossDownedTracker.IsBossDowned<InfernoSkull>()`). Selection rationale (code-verified, not assumed): of the 8 Spirit-tracked bosses, Infernon is the ONLY one whose own `OnKill()` has any side effect beyond the generic `BossDownedTracker` flag write — it places a small ring of `TileID.HellstoneBrick` in the air tiles immediately surrounding its own death position. The other 7 bosses have no boss-level `OnKill` override at all (only their sub-projectiles do, which are irrelevant to downed-state reproduction). This mirrors Phase 4's D-02 selection discipline (earliest/richest boss for side-effect coverage) applied to Spirit's actual boss roster.
- **Known nuance (Claude's discretion, not blocking):** Infernon's tile-ring effect is anchored to `NPC.position` (the boss's own death location) — a live NPC that won't exist at `BossCoreItem`-use time in the main world. User confirmed (2026-08-13) this is inconsequential: the real ring already draws harmlessly inside the subworld's own throwaway platform during the actual kill (same "real effect fires once in the discarded subworld, doesn't matter" category as Phase 4's Sky Ore double-message finding), so the main-world replay should simply anchor the ring on the player's current position when `BossCoreItem` is used — no special design effort needed, treat as a minor cosmetic detail, not a correctness requirement.

### Player-scoped vs. world-scoped classification (Success Criterion 2)
- **D-03:** Explicitly investigated and **no player-scoped double-grant risk was found** in Spirit's tracked-boss `OnKill` paths (`BossDownedTracker.OnKill()`: pure world-scoped dictionary write + singleplayer-no-op netcode; `Infernon.OnKill()`: world-scoped tile mutation via `Main.tile`, no player-object writes). This is the **same category as Phase 3's King Slime** (no player-scoped reward to worry about), NOT the same as Phase 4's Hive Mind (which had `CalamityGlobalNPC.SetNewBossJustDowned()` as a confirmed player-scoped side effect deliberately excluded from replay). Success Criterion 2 is satisfied by **explicitly documenting this empirical finding** (classification = "fully world-scoped, no player-scoped effect exists") rather than by building exclusion logic for a risk that doesn't exist — mirrors Phase 3's precedent, not Phase 4's. Research-phase should re-confirm this holds for the final chosen boss by re-reading its full `OnKill` (and any `ModPlayer` hooks it might trigger) against the installed copy, not just trust this discussion's grep-level pass.

### Scope (boss count)
- **D-04:** Register exactly **one** Spirit boss this phase (Infernon, per D-02). Do not attempt broader Spirit coverage now — same "prove the generic pattern with one worked example" discipline as Phase 3 (King Slime) and Phase 4 (Hive Mind). Registering the remaining 7 Spirit bosses is explicitly deferred (see Deferred Ideas below).

### Live verification approach
- **D-05:** Following Phase 4's D-04 precedent: because Infernon has a real (if cosmetic) WorldGen tile-mutation side effect, the live verification checkpoint uses a **freshly created, dedicated test world** (SpiritMod + SubworldLibrary + BossArenaSubWorld enabled) — NOT the backed-up main save. A second checkpoint (mirroring Phase 4's D-05) verifies the mod loads and runs safely with SpiritMod disabled, satisfying this phase's Success Criterion 3.

### Claude's Discretion
- Exact `weakReferences` version pin syntax in `build.txt` for SpiritMod (installed copy: `ModReader/SpiritMod` decompiled source available locally — exact `.tmod` version string to be confirmed during research-phase the same way Phase 4 read CalamityMod's `.tmod` header).
- Exact shape/naming of the Spirit integration file (`Integrations/SpiritIntegration.cs`, following Phase 4's `Integrations/CalamityIntegration.cs` naming convention established in that phase's code_context).
- How exactly to anchor Infernon's tile-ring replay position in the main world (player position at `BossCoreItem` use time — confirmed low-stakes per D-02's nuance note, exact implementation detail deferred to planning).
- Whether to replay `NetMessage.SendData(MessageID.WorldData)` in the main-world apply path despite it being a singleplayer no-op — Phase 4 chose to replay the equivalent (`CalamityNetcode.SyncWorld()`) for fidelity/future-proofing; default to the same choice here unless research-phase finds a reason not to.
- **CRITICAL — carried forward from Phase 4's hard-won lesson:** Any delegate passed into a `[JITWhenModsEnabled("SpiritMod")]`-guarded registration call (e.g. `BossDefinition.IsDowned`, `ApplyDowned`) MUST be a named, separately-`[JITWhenModsEnabled]`-tagged method — never an inline lambda. Phase 4 hit a real live `JITException` (commit `0e19600`) from exactly this mistake; this is now a locked project-wide rule (see `PROJECT.md` Key Decisions), not optional for planning to reconsider.

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Requirements this phase implements
- `.planning/REQUIREMENTS.md` §"Mod Coverage (MOD)" — MOD-02 (Spirit bosses registered via the `MyWorld` static-field pattern — NOTE: this description is superseded by D-01's finding, research-phase must reconcile)
- `.planning/ROADMAP.md` §"Phase 5: Spirit Integration" — Goal, Success Criteria 1-3

### Spirit API specifics (locally available decompiled source — read these directly, no decompilation needed)
- `C:\Users\chang\Documents\My Games\Terraria\tModLoader\ModReader\SpiritMod\MyWorld.cs` — confirms `DownedScarabeus`/`DownedAncientAvian`/`DownedStarplate`/`DownedAtlas`/`DownedInfernon`/`DownedMoonWizard`/`DownedVinewrath`/`DownedDusking` are all computed properties wrapping `BossDownedTracker.IsBossDowned<T>()`, NOT plain static bools (D-01)
- `C:\Users\chang\Documents\My Games\Terraria\tModLoader\ModReader\SpiritMod\NPCs\BossDownedTracker.cs` — the actual registration/tracking mechanism: `Downed` dictionary, `IsBossDowned<T>()`, `GetBossKey<T>()`, `OnKill(NPC)`, and `BossDownedTrackingIO : ModSystem` for save/load/netcode (D-01, D-03)
- `C:\Users\chang\Documents\My Games\Terraria\tModLoader\ModReader\SpiritMod\NPCs\Boss\Infernon\Infernon.cs` — the worked-example boss's own `OnKill()` (tile-ring WorldGen effect) — research-phase should read this file in full, not just the `OnKill` excerpt seen during discussion (D-02)
- `C:\Users\chang\Documents\My Games\Terraria\tModLoader\ModReader\SpiritMod\NPCs\Boss\Infernon\InfernoSkull.cs` — the actual `ModNPC` type Infernon is tracked under in `BossDownedTracker` (`GetBossKey<InfernoSkull>()`) — confirm this is the correct type to register in `BossRegistry`, not `Infernon` itself, during research-phase
- Full `ModReader/SpiritMod/` tree — locally decompiled source for the entire installed SpiritMod copy; use this directly instead of re-decompiling `.tmod` files, matching Phase 4's "inspect the actual installed binary" discipline but via already-extracted source

### Locked patterns from Phase 3/4 (extend unchanged into this phase)
- `.planning/phases/04-calamity-integration-cross-mod-side-effect-reproduction/04-CONTEXT.md` — D-01 (weakReferences + `[JITWhenModsEnabled]` strict per-method isolation discipline), D-04/D-05 (live verification via fresh throwaway world + Calamity-disabled load-safety checkpoint) — both apply directly to Spirit (D-05 above)
- `.planning/PROJECT.md` §Key Decisions — the Phase 4 lambda/`[JITWhenModsEnabled]` JIT-crash lesson (commit `0e19600`) — MANDATORY discipline for this phase's `BossDefinition` registration (see Claude's Discretion above)
- `.planning/phases/03-bossregistry-bosscoreitem-globalnpc-pipeline-proof-of-concept/03-CONTEXT.md` — D-01 (idempotency via live-flag check), D-03 (namespaced `"modprefix:boss_name"` keys — anticipates `"spirit:infernon"` or similar), D-04 (vanilla-fidelity flag application via source mod's own setter/mechanism, not raw boolean)
- `.planning/research/PITFALLS.md` §"Pitfall 5: Player-scoped vs. world-scoped double-grants" — the discipline D-03 above applies; §"Pitfall 2/4" also apply (JIT isolation, reflective lookup fragility)

### World-safety / process references
- `docs/WORLD_BACKUP_GUIDANCE.md` — backup procedure for the SpiritMod-disabled load-safety checkpoint (D-05), which may touch the main save
- `.planning/debug/resolved/isolation-premise-flag-persistence.md` and `.planning/debug/resolved/hivemind-zonecorrupt-despawn-corruption-subworld.md` — the `OnEnter()`/`OnExit()` flag-isolation guard and the `WorldFile.SaveWorld()`/`Main.ActiveWorldFileData` exit-flow mechanism these sessions decompiled — directly explains WHY D-03's classification holds (the same "early SaveWorld() writes to the subworld's own discarded path" mechanism applies to `BossDownedTrackingIO`'s world data, not just Calamity's)

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `Systems/BossRegistry.cs` — `BossDefinition`/`ApplyResult`/`Apply`/`TryGetKeyForNpc` are boss-agnostic; this phase adds exactly one new `BossDefinition` entry (Infernon) with Spirit-specific `ApplyDowned`/`IsDowned` delegates (named methods, per the JIT lesson).
- `Items/BossCoreItem.cs`, `ItemDropRules/BossCoreDropRule.cs`, `GlobalNPCs/BossKillGlobalNPC.cs` — fully boss-agnostic already; zero changes needed.
- `Systems/BossArenaRoutingRegistry.cs` (built in Phase 4's debug session) — if Infernon needs a non-default arena biome (unconfirmed — research-phase should check whether Infernon has a `player.Zone*`-gated AI despawn requirement similar to Hive Mind's `ZoneCorrupt` dependency), this registry is already available for routing to a dedicated arena subworld.
- Namespaced key convention `"modprefix:boss_name"` (Phase 3 D-03, reused by Phase 4 as `"calamity:hive_mind"`) — this phase would use `"spirit:infernon"` or similar.

### Established Patterns
- `Integrations/CalamityIntegration.cs` — the direct template for this phase's `Integrations/SpiritIntegration.cs`: `PostSetupContent()` with a `ModLoader.HasMod("SpiritMod")` guard, `[JITWhenModsEnabled("SpiritMod")]`-tagged registration/apply methods, named (not lambda) delegate methods.
- `weakReferences` + `[JITWhenModsEnabled]` — confirmed working pattern from Phase 4, now including the lambda-avoidance lesson.

### Integration Points
- `build.txt` currently has `modReferences = SubworldLibrary` and `weakReferences = CalamityMod@2.2.4` — this phase adds a `weakReferences = SpiritMod@<version>` line.
- New file needed: `Integrations/SpiritIntegration.cs`, isolating all `[JITWhenModsEnabled("SpiritMod")]`-tagged methods.

</code_context>

<specifics>
## Specific Ideas

- The single most important finding from this discussion: **PROJECT.md's Spirit API description is stale.** The actual installed copy uses a `BossDownedTracker` generic dictionary system (structurally closer to this project's own `BossRegistry` than to Calamity's wrapper-property pattern), not plain `MyWorld` static bools for real bosses. Downstream research/planning must treat `REQUIREMENTS.md`'s "MyWorld static-field pattern" wording as directionally correct (Spirit's API shape IS structurally different from Calamity's, satisfying the phase's underlying goal of "proving the pattern generalizes across API shapes") but not literally accurate about which class/mechanism to touch.
- Full decompiled Spirit source is available locally at `ModReader/SpiritMod/` (and other mods at sibling `ModReader/<ModName>/` folders: CalamityMod, CatalystMod, ContinentOfJourney, Daybreak, InfernonMode [Infernum], NoxusBoss, Redemption, BossChecklist) — future phases (6, 7, 8) can use this same direct-source-read approach instead of decompiling `.tmod` files fresh each time.
- User confirmed the Infernon tile-ring position-anchoring question is low-stakes/cosmetic — don't over-design this; simplest implementation (anchor on player position) is acceptable.

</specifics>

<deferred>
## Deferred Ideas

- **Registering the remaining 7 Spirit bosses** (Scarabeus, AncientFlyer, SteamRaiderHead/Starplate, Atlas, MoonWizard, ReachBoss1/Vinewrath, Dusking) beyond Infernon — explicitly out of this phase's scope (D-04). Same reasoning as Phase 4's deferred Calamity bosses: near-zero marginal registration cost once the pattern is proven, no priority ordering needed per `PROJECT.md`.
- **Wrath of the Gods base mod** — user discovered during this session that only the Korean localization patch (`WrathoftheGodsKR.tmod`) is currently subscribed, not the base English mod. Not relevant to Phase 5-8 registration work (Wrath reworks NoxusBoss's Devourer of Universes AI only, no separate downed flag, per `PROJECT.md`'s existing note), but flagged in case the user wants it for actual gameplay later — not a roadmap item.

### Reviewed Todos (not folded)
None — `todo match-phase` returned 0 matches for Phase 5.

</deferred>

---

*Phase: 05-spirit-integration*
*Context gathered: 2026-08-13*
