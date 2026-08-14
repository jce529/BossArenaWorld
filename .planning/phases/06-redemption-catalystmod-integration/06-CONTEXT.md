# Phase 6: Redemption & CatalystMod Integration - Context

**Gathered:** 2026-08-14
**Status:** Ready for planning

<domain>
## Phase Boundary

Redemption's and CatalystMod's downed-progress APIs are researched, and exactly one worked-example boss per mod is registered and applied correctly in the main world — extending the generic `BossRegistry`/`BossCoreItem`/`GlobalNPC` pipeline to two more content mods, proving it generalizes to a third and fourth API shape beyond Calamity's wrapper-properties (Phase 4) and Spirit's internal dictionary (Phase 5). This phase does NOT attempt full boss-roster coverage for either mod — same "prove the pattern with one worked example" discipline as Phase 3/4/5.

</domain>

<decisions>
## Implementation Decisions

### CatalystMod source-access approach
- **D-01:** CatalystMod's modder has explicitly hidden code, resources, and the `.dll` itself from tModReader's extraction (`extract.log`: "The modder has chosen to hide the code... has chosen to hide resources"; a `HelloDataminers.txt` file is present, signaling deliberate anti-datamining intent). User explicitly chose to proceed anyway: **decompile the installed `.tmod`'s embedded DLL directly via `ilspycmd`**, the same tool/approach already used in Phase 4/5 (Calamity/Spirit) and Phase 9 (`ilspycmd` against `Libs/CalamityMod.dll`/`Libs/SpiritMod.dll`). This bypasses tModReader's respect for the modder's stated preference — noted here explicitly as a deliberate, informed choice, not an oversight. Framing: personal/individual use against the user's own installed copy, no redistribution or publishing of decompiled output planned.
- Research-phase should manually locate and extract `CatalystMod.dll` from the installed `.tmod` (same extraction approach as Phase 9's `Libs/` DLL copies) since tModReader's own extraction pipeline will not produce it.

### Redemption worked-example boss selection
- **D-02:** No specific boss requested by the user. Research-phase applies the same selection discipline as Phase 4 (Hive Mind, D-02) and Phase 5 (Infernon, D-02): decompile all 10 Redemption bosses' (`ADD`, `Cleaver`, `Erhan`, `Gigapora`, `Keeper`, `KSIII`, `Neb`, `Obliterator`, `PatientZero`, `SeedOfInfection`, `Thorn`) `OnKill()` methods and pick the one with the richest side effects beyond a plain downed-flag write (netcode sync, WorldGen triggers, etc.) as the worked example — mirroring the exact reasoning Phase 4/5 already used.

### CatalystMod worked-example boss selection
- **D-03:** User specified **Astrageldon** as the worked-example boss, based on its prominence in CatalystMod's asset tree (dedicated loading-screen images `Assets/UI/LoadMenu/Astrageldon_2/3/4.png`, a dedicated background `Assets/Backgrounds/Astrageldon/Background.png`, and a pet projectile `Projectiles/Pets/AstrageldonPet.png` — asset-density signals suggest it is CatalystMod's headline/final boss). Research-phase (after the D-01 decompile) must still confirm Astrageldon actually has a boss-level `OnKill()` with reproducible side effects before finalizing it as the registration target — if research finds Astrageldon has no non-trivial `OnKill` side effects (unlikely given its apparent prominence, but not yet code-confirmed), fall back to Redemption's D-02 selection discipline (richest-side-effect heuristic) applied to whatever other CatalystMod bosses are discovered during decompilation.

### Scope (boss count) — carried forward, not re-discussed
- Register exactly **one** boss per mod (Redemption + CatalystMod = 2 total) this phase. Same discipline as every prior mod-integration phase (Phase 3: King Slime, Phase 4: Hive Mind, Phase 5: Infernon). Full-roster registration for any mod remains explicitly out of v1 scope across all mod-integration phases.

### Claude's Discretion
- Exact `weakReferences` version pin syntax in `build.txt` for Redemption and CatalystMod (confirm installed `.tmod` version strings during research-phase, same as Phase 4/5's `.tmod` header read).
- Exact shape/naming of the two new integration files (`Integrations/RedemptionIntegration.cs`, `Integrations/CatalystIntegration.cs`), following the established `Integrations/CalamityIntegration.cs`/`Integrations/SpiritIntegration.cs` naming convention.
- Whether Astrageldon (or Redemption's selected boss) has any `player.Zone*`-gated AI despawn dependency requiring a `BossArenaRoutingRegistry` biome-arena entry — research-phase should check this explicitly, mirroring Phase 4's Hive Mind discovery. Note: Phase 9's biome-classification/routing work only covered Calamity/Spirit bosses that existed at the time — any biome dependency found for a Phase 6 boss is new territory this phase (or a follow-up) must handle directly, not something Phase 9 already resolved.
- **CRITICAL — carried forward from Phase 4's hard-won lesson:** Any delegate passed into a `[JITWhenModsEnabled(...)]`-guarded registration call (`BossDefinition.IsDowned`, `ApplyDowned`) MUST be a named, separately-tagged method — never an inline lambda. Locked project-wide rule (see `PROJECT.md` Key Decisions), not open for reconsideration.
- Whether Redemption's or CatalystMod's selected boss has any player-scoped side effect requiring exclusion logic (Phase 4's Hive Mind pattern) vs. being fully world-scoped (Phase 3/5's pattern) — research-phase determines per boss, same as every prior mod-integration phase.

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Requirements this phase implements
- `.planning/REQUIREMENTS.md` §"Mod Coverage (MOD)" — MOD-03 (Redemption), MOD-04 (CatalystMod)
- `.planning/ROADMAP.md` §"Phase 6: Redemption & CatalystMod Integration" — Goal, Success Criteria 1-3

### Redemption API specifics (locally available — resources + raw DLL/PDB extracted, no embedded source)
- `C:\Users\chang\Documents\My Games\Terraria\tModLoader\ModReader\Redemption\Redemption.dll` (+ `.pdb`) — modder did not include embedded source (`extract.log`: "The modder has not chosen to include their source code"); decompile via `ilspycmd` same as Phase 4/5's Calamity/Spirit DLLs, the `.pdb` should improve decompile fidelity
- `C:\Users\chang\Documents\My Games\Terraria\tModLoader\ModReader\Redemption\NPCs\Bosses\` — 10 boss asset folders (`ADD`, `Cleaver`, `Erhan`, `Gigapora`, `Keeper`, `KSIII`, `Neb`, `Obliterator`, `PatientZero`, `SeedOfInfection`, `Thorn`) confirming the full boss roster to evaluate for D-02's selection

### CatalystMod API specifics (source/resources/DLL explicitly hidden from tModReader — see D-01)
- Installed `CatalystMod.tmod` in the tModLoader `Mods` folder — must be manually located and its embedded `CatalystMod.dll` extracted (`.tmod` is a zip-like container), then decompiled via `ilspycmd`, since tModReader's own pipeline will not produce it (`ModReader/CatalystMod/extract.log` confirms full hide: code, resources, and the DLL itself all marked `[hidden]`)
- `C:\Users\chang\Documents\My Games\Terraria\tModLoader\ModReader\CatalystMod\extract.log` — confirms exactly what tModReader could NOT extract (useful for research-phase to know what's genuinely unavailable via the normal pipeline vs. what needs manual `.tmod` extraction)

### Locked patterns from Phase 3/4/5 (extend unchanged into this phase)
- `.planning/phases/04-calamity-integration-cross-mod-side-effect-reproduction/04-CONTEXT.md` — D-01 (weakReferences + `[JITWhenModsEnabled]` strict per-method isolation discipline), D-02 (richest-side-effect worked-example selection heuristic — directly reused for D-02 above), D-04/D-05 (live verification via fresh throwaway world + mod-disabled load-safety checkpoint)
- `.planning/phases/05-spirit-integration/05-CONTEXT.md` — D-01 (API-shape correction discipline: always verify against actually-installed/decompiled source, never trust `PROJECT.md`'s prior notes at face value), D-03 (player-scoped vs. world-scoped classification via explicit investigation, not assumed exclusion logic)
- `.planning/PROJECT.md` §Key Decisions — the Phase 4 lambda/`[JITWhenModsEnabled]` JIT-crash lesson (commit `0e19600`) and the Phase 9 lesson that lazy construction inside a containing class is NOT sufficient JIT protection on its own — every method touching a weak-referenced mod's types needs its own `[JITWhenModsEnabled]` tag regardless of call-reachability (Phase 09 P07, live-confirmed)
- `.planning/phases/03-bossregistry-bosscoreitem-globalnpc-pipeline-proof-of-concept/03-CONTEXT.md` — D-03 (namespaced `"modprefix:boss_name"` key convention — this phase would use `"redemption:<boss>"` / `"catalyst:astrageldon"` or similar)
- `.planning/research/PITFALLS.md` §"Pitfall 5: Player-scoped vs. world-scoped double-grants", §"Pitfall 2/4" (JIT isolation, reflective lookup fragility) — apply directly

### World-safety / process references
- `docs/WORLD_BACKUP_GUIDANCE.md` — backup procedure for both mod-disabled load-safety checkpoints, which may touch the main save
- `.planning/STATE.md` §Blockers/Concerns — "Phases 6-7 ... have entirely unresearched APIs — each will likely need a `/gsd:research-phase` pass before detailed planning" (confirmed true by this discussion; CatalystMod in particular needs the manual `.tmod` extraction step before research can even begin)

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `Systems/BossRegistry.cs` — `BossDefinition`/`ApplyResult`/`Apply`/`TryGetKeyForNpc` are boss-agnostic; this phase adds exactly two new `BossDefinition` entries (one Redemption, one CatalystMod) with mod-specific `ApplyDowned`/`IsDowned` delegates (named methods, per the JIT lesson).
- `Items/BossCoreItem.cs`, `ItemDropRules/BossCoreDropRule.cs`, `GlobalNPCs/BossKillGlobalNPC.cs` — fully boss-agnostic already; zero changes needed.
- `Systems/BossArenaRoutingRegistry.cs` (Phase 4) + the 7 biome-variant subworlds (Phase 9) — available if either new boss turns out to be biome/Zone-dependent; research-phase must check this explicitly (see Claude's Discretion above), since Phase 9 did not and could not cover bosses that didn't exist in the registry yet.
- Namespaced key convention `"modprefix:boss_name"` (Phase 3 D-03) — reused as `"redemption:<boss>"` / `"catalyst:astrageldon"`.

### Established Patterns
- `Integrations/CalamityIntegration.cs`, `Integrations/SpiritIntegration.cs` — direct templates for this phase's two new integration files: `PostSetupContent()` with a `ModLoader.HasMod("<ModName>")` guard, `[JITWhenModsEnabled("<ModName>")]`-tagged registration/apply methods, named (not lambda) delegate methods.
- `weakReferences` + `[JITWhenModsEnabled]` — confirmed working pattern from Phase 4/5/9, including the lambda-avoidance and per-method-tagging lessons.

### Integration Points
- `build.txt` currently has `modReferences = SubworldLibrary` and `weakReferences = CalamityMod@2.2.4, SpiritMod@1.5.0.44` — this phase adds `Redemption@<version>` and `CatalystMod@<version>` to that comma-separated line (confirmed comma-separated syntax from Phase 5 D-tooling-note, not space-separated).
- Two new files needed: `Integrations/RedemptionIntegration.cs`, `Integrations/CatalystIntegration.cs`.

</code_context>

<specifics>
## Specific Ideas

- CatalystMod's anti-datamining posture (`HelloDataminers.txt`) is a real, deliberate signal from the mod author. The user made an informed choice to decompile anyway for personal/individual use against their own installed copy — this is documented here (D-01) as a conscious decision, not something research/planning should silently work around or re-litigate.
- Astrageldon's asset footprint (dedicated loading-screen art, background, pet) is a strong but not yet code-confirmed signal that it's CatalystMod's headline boss — research-phase's first job after decompiling is to verify this assumption against the actual `OnKill()` before committing to it as the registration target (D-03's fallback clause).

</specifics>

<deferred>
## Deferred Ideas

- **Registering the remaining Redemption bosses** (9 of 10, beyond whichever is selected) and any other CatalystMod bosses beyond Astrageldon — explicitly out of this phase's scope. Same reasoning as every prior mod-integration phase: near-zero marginal registration cost once the pattern is proven, no priority ordering per `PROJECT.md`.
- **Retroactive biome-classification sweep for Phase 6/7 bosses** — surfaced during pre-discussion analysis (not a Phase 6 decision, but worth flagging): Phase 9's ARENA-01 work only classified/routed bosses that existed in the registry at the time (Calamity's Hive Mind, Spirit's Infernon). Any boss registered in Phase 6 or 7 that turns out to be biome/Zone-dependent will need its own classification + arena work that no currently-scoped phase owns. Flagged for whoever plans the phase after Phase 7 (or Phase 8/9 follow-up) to pick up explicitly rather than rediscovering live in-game.

### Reviewed Todos (not folded)
None — `todo match-phase` returned 0 matches for Phase 6.

</deferred>

---

*Phase: 06-redemption-catalystmod-integration*
*Context gathered: 2026-08-14*
