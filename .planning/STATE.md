---
gsd_state_version: 1.0
milestone: v1.0
milestone_name: milestone
status: All 7 kept biome variants live-verified across all 3 mechanism families (Wave 3 / 09-06 complete). Wave 4 (09-07 -- CalamityMod/SpiritMod-disabled JIT-safety checkpoint + debug-tool cleanup) remains, the last plan in Phase 9.
stopped_at: Completed 09-06-PLAN.md
last_updated: "2026-08-14T04:16:49.769Z"
last_activity: "2026-08-14 -- Phase 09 Wave 3 (09-06) complete: all 7 biome subworlds live-confirmed to satisfy their target Zone/Biome flag; Desert falling-Sand crash fixed mid-checkpoint"
progress:
  total_phases: 9
  completed_phases: 5
  total_plans: 21
  completed_plans: 20
  percent: 95
---

# Project State

## Project Reference

See: .planning/PROJECT.md (updated 2026-08-12)

**Core value:** The generic boss-kill → carrier-item → main-world-apply mechanism (BossRegistry + BossCoreItem + GlobalNPC) must reliably reproduce a boss's full "downed" state — flags, netcode sync, and any WorldGen side effects — for any registered boss.
**Current focus:** Phase 09 — biome-dependent-subworld-coverage (Wave 4 next: 09-07 mod-disabled safety checkpoint + debug-tool cleanup)

## Current Position

Phase: 09 (biome-dependent-subworld-coverage) — EXECUTING (out of ROADMAP order, per D-01 — ahead of Phases 6-8)
Plan: Wave 1 complete (09-01, 09-02, 09-03, 09-04). Wave 2 complete (09-05). Wave 3 complete (09-06). Wave 4 (09-07) next -- last plan in Phase 9.
Status: All 7 kept biome-variant subworlds (Hallow, Underworld, Jungle, Space, Desert, Astral, Briar) are now live-confirmed, across all 3 checkpoints of 09-06, to generate correctly and satisfy their target Zone/Biome flag on entry. One mid-checkpoint fix: Desert's platform crashed via falling-Sand recursion, fixed to a thin real-Sand-over-Sandstone layer (same SceneMetrics weight), re-verified live. ARENA-01 remains open pending Phases 6-8's boss-classification-and-routing half. 09-07 (Wave 4) remains: CalamityMod/SpiritMod-disabled JIT-safety checkpoint for Astral/Briar, then deletion of Debug/BiomeArenaDebugCommands.cs per D-02.
Last activity: 2026-08-14 -- Phase 09 Wave 3 (09-06) complete: all 7 biome subworlds live-confirmed to satisfy their target Zone/Biome flag; Desert falling-Sand crash fixed mid-checkpoint

Progress: [██████████] 95% (20 of 21 currently-planned plans across Phases 1-5 + Phase 9; Phases 6-8 plans not yet created, executed out of order per D-01)

## Performance Metrics

**Velocity:**

- Total plans completed: 0
- Average duration: - min
- Total execution time: 0 hours

**By Phase:**

| Phase | Plans | Total | Avg/Plan |
|-------|-------|-------|----------|
| - | - | - | - |

**Recent Trend:**

- Last 5 plans: none yet
- Trend: -

*Updated after each plan completion*
| Phase 01 P01 | 8 | 3 tasks | 3 files |
| Phase 01 P02 | 55 | 2 tasks | 4 files |
| Phase 01 P03 | 3min | 2 tasks | 2 files |
| Phase 01 P04 | 20min | 2 tasks | 0 files |
| Phase 02 P01 | 8min | 2 tasks | 2 files |
| Phase 02 P02 | 25min | 3 tasks | 5 files |
| Phase 02 P03 | 10min | 2 tasks | 1 files |
| Phase 03 P01 | 6min | 2 tasks | 3 files |
| Phase 03 P02 | 4min | 2 tasks | 2 files |
| Phase 03 P03 | verification-only | 1 tasks | 0 files |
| Phase 04 P01 | 5min | 2 tasks | 4 files |
| Phase 04 P02 | 25min | 2 tasks | 1 files |
| Phase 05 P01 | 7min | 2 tasks | 3 files |
| Phase 05 P02 | verification-only | 3 tasks | 0 files |
| Phase 09 P01 | 10min | 2 tasks | 4 files |
| Phase 09 P02 | 12min | 2 tasks | 4 files |
| Phase 09 P03 | n/a | 1 task kept (of 2 built) | 2 files (Dungeon discarded) |
| Phase 09 P04 | n/a | 2 tasks kept (of 3 built) | 4 files (Sulphurous discarded) |
| Phase 09 P05 | 15min | 2 tasks | 1 files |
| Phase 09 P06 | verification-only | 3 tasks | 1 files |

## Accumulated Context

### Decisions

Decisions are logged in PROJECT.md Key Decisions table.
Recent decisions affecting current work:

- Roadmap: Reflection/weak-reference cross-mod access helper is NOT a standalone phase — it's built as needed inside Phase 4 (Calamity), the first real content-mod integration, since it has no requirement of its own to anchor a phase to.
- Roadmap: Phase 3 (registry/item/GlobalNPC pipeline) proven with one vanilla boss before any content-mod integration, isolating pipeline-mechanism risk from per-mod API risk.
- Roadmap: Weak-references vs. pure-reflection tension (flagged in research/SUMMARY.md Gaps) is unresolved — needs explicit decision during Phase 4 planning, likely via `/gsd:research-phase`.
- [Phase 01]: GameConfiguration lives in Terraria.IO, not Terraria.WorldBuilding -- confirmed via reflection against installed tModLoader.dll
- [Phase 01]: Plain dotnet build/msbuild cannot resolve build.txt modReferences (runtime-only mechanism) -- added a gitignored, locally-extracted Libs/SubworldLibrary.dll compile-time Reference (Private=false) alongside the authoritative build.txt declaration
- [Phase 01]: Mod scaffold files (BossArenaSubWorld.cs, .csproj, build.txt, Properties/, Localization/, icons, description files) were untracked in git prior to this phase, causing isolated parallel worktrees to independently reconstruct them; scaffold is now committed to git as part of 01-01/01-02 merge, closing this gap for future worktree-isolated plans.
- [Phase 01]: BiomeOverridePlayer.cs references the Subworld type via using+unqualified name instead of fully-qualified path, avoiding CS0426 collision with the BossArenaSubWorld Mod class
- [Phase 01]: CRITICAL: Live King Slime isolation test shows NPC.downedSlimeKing=True after subworld round-trip (expected False) -- isolation premise NOT confirmed, contradicts 01-RESEARCH.md's source-traced expectation; Phase 2 planning blocked pending re-investigation
- [Phase 02]: Generalized D-09 as calling NPC.SpawnOnPlayer with the mapped boss type (vanilla summon items have no isolated, externally-callable use-effect method)
- [Phase 02]: Used SubworldSystem.IsActive<BossArenaSubworld>(), not AnyActive, correcting a non-compiling example in 02-RESEARCH.md (AnyActive requires where T : Mod)
- [Phase 02]: Used ModTile.RightClick, not NewRightClick -- the installed tModLoader.dll only exposes RightClick(int,int)->bool on ModTile, confirmed via MetadataLoadContext reflection against the real local binary
- [Phase 02]: Gitignored Libs/SubworldLibrary.dll compile-time reference must be manually copied into each fresh git worktree before dotnet build succeeds (not a code gap, a per-worktree setup step)
- [Phase 02]: User-confirmed live test proves SUBW-01..04 via Test1Tile redirect; treated '전부 통과했어' as the plan's exact resume-signal
- [Phase 02]: Deleted Debug/SubworldDebugCommands.cs in full now that Tiles/Test1Tile.cs redirect and SubworldLibrary's Return button fully supersede it (closes Phase 1 D-02)
- [Phase 03]: Fixed NpcTypes array literal (short->int[]) and CloneNewInstances access modifier (public->protected) to match installed tModLoader 1.4.4.9 API -- plan's illustrative code had two compile-blocking bugs
- [Phase 03]: Live King Slime pipeline test confirms full BossRegistry/BossCoreItem/GlobalNPC pipeline end-to-end (DROP-02, DROP-03, APPLY-01, APPLY-04); Test1Item's missing acquisition path is a carried-over Phase 2 gap (D-05), not a Phase 3 defect
- [Phase 04]: Confirmed installed CalamityMod version (2.2.4) by reading the .tmod's own header field during extraction, matching build.txt's weakReferences declaration exactly
- [Phase 04]: ApplyHiveMindDowned() deliberately omits CalamityGlobalNPC.SetNewBossJustDowned() per 04-RESEARCH.md correction -- it is player-scoped speedrun-timer bookkeeping already applied live during the subworld kill; replaying it would double-apply
- [Phase 04]: Resolved debug session `hivemind-zonecorrupt-despawn-corruption-subworld` (was blocking 04-02-PLAN.md's live verification checkpoint) -- Hive Mind despawn traced to missing Corruption biome tiles (player.ZoneCorrupt never true, no corruption tiles in the plain-stone arena); fixed via a new BossArenaCorruptionSubworld + CorruptionPlatformPass (full-width Ebonstone/CorruptGrass platform) and a boss-aware BossArenaRoutingRegistry replacing hardcoded BossArenaSubworld checks in Test1Tile/BossSummonPlayer/BossCoreDropRule. Also confirmed the follow-up "double Sky Ore message" symptom is expected, non-bug behavior (matches PITFALLS.md Pitfall 1's carrier-item architecture exactly) via decompilation, not a state-corruption bug. User confirmed fix live in-game (checklist items 1-5 passed); optional final spot-checks (flag persistence across save/relaunch, no third message on repeat use) explicitly skipped by user as redundant with the already-confirmed mechanism. See .planning/debug/resolved/hivemind-zonecorrupt-despawn-corruption-subworld.md.
- [Phase 04]: [Phase 04]: Delegates/lambdas passed into [JITWhenModsEnabled]-guarded registration calls (BossDefinition's ApplyDowned/IsDowned) must be named, separately-tagged methods -- never inline lambdas -- since the C# compiler hoists inline lambdas into a <>c cache-class method that does NOT inherit the enclosing method's JIT-guard attribute, causing a real JITException when the weak-referenced mod is disabled. Confirmed live: fixed in Integrations/CalamityIntegration.cs (commit 0e19600) after Task 2's CalamityMod-disabled load test crashed. Applies to every future per-boss registration (Spirit, Redemption, CatalystMod, NoxusBoss, ContinentOfJourney/Daybreak).
- [Phase 04]: [Phase 04]: Task 1's live WorldGen/netcode verification checkpoint was satisfied by evidence gathered during the immediately-preceding resolved debug session (hivemind-zonecorrupt-despawn-corruption-subworld), not a duplicate live test -- user explicitly confirmed the debug session used a fresh throwaway world with Evil Type = Corruption (not the real save), matching Task 1's exact acceptance criteria.
- [Phase 05]: Registered spirit:infernon in Integrations/SpiritIntegration.cs using cached reflection into SpiritMod's internal BossDownedTracker.Downed dictionary (no public setter exists) for the write path, and the public zero-reflection MyWorld.DownedInfernon property for the read path -- first genuine use of runtime reflection in this project, proving BossRegistry generalizes to a dictionary-tracking API shape distinct from Calamity's wrapper-property shape.
- [Phase 05]: Confirmed live that tModLoader's build.txt weakReferences syntax for multiple entries on one line is comma-separated (CalamityMod@2.2.4, SpiritMod@1.5.0.44), not space-separated -- the plan's illustrative space-separated syntax failed to parse (BuildProperties.ModReference.Parse threw "Invalid mod reference"); fixed inline (05-01, Rule 1).
- [Phase 05]: Both Infernon and InfernoSkull NPC types registered under one BossDefinition (Pitfall B) since either's OnKill can be the actual downed-trigger depending on Normal vs Expert Mode difficulty.
- [Phase 05]: D-03 confirmed and documented in-code: Infernon's downed-tracking path (BossDownedTracker.OnKill + Infernon/InfernoSkull.OnKill) is fully world-scoped with no player-scoped side effect, so no exclusion logic is needed (unlike Calamity's Hive Mind SetNewBossJustDowned() case in Phase 4).
- [Phase 05]: Tooling note (not a project code issue): the `state update-progress`/`state advance-plan` gsd-tools commands have a case-insensitive-regex bug that matches the YAML frontmatter's `progress:` key instead of the body's `Progress:` line, silently no-op-ing the update (the corrupted frontmatter copy gets discarded when syncStateFrontmatter rebuilds frontmatter from the unchanged body). Updated STATE.md's Current Position/Progress/frontmatter fields manually for 05-01 as a workaround.
- [Phase 05]: [Phase 05]: Task 2 (reflection-failure graceful-degradation checkpoint) was explicitly skipped by user decision -- it is not one of Phase 5's formal ROADMAP.md Success Criteria (only registration correctness, player/world-scope classification, and safe-disabled-load are), only an untested implementation-robustness path already confirmed via code review in 05-01. Precedent: future first-reflection-integration checkpoints (Phase 6/7) should be scoped strictly to formal Success Criteria unless the user asks for broader robustness coverage.
- [Phase 05]: [Phase 05]: Live-verification lesson -- a summon item's in-game display name can differ from its underlying ModItem class name (SpiritMod's CursedCloth displayed in-game as 'Pain Caller' during Task 1's checkpoint). Do not treat a display-name mismatch alone as a verification failure; cross-check by ModItem class/type for all future mods' live-verification checkpoints (Phases 6-9).
- [Phase 05]: Tooling note: `gsd-tools phase complete "05"` reported `roadmap_updated: true` but made zero changes to ROADMAP.md (verified via git diff), and set STATE.md's Current Position to "Phase: 09" instead of the actual next unplanned phase (Phase 6) -- likely picking the highest-numbered phase in the file rather than the next sequential incomplete one, now that Phase 9 exists past the still-unplanned Phases 6-8. Manually fixed ROADMAP.md's Phase 5 checkbox/Progress row and STATE.md's Current Position as a workaround.
- [Phase 09]: D-07 (2026-08-14) -- Mid-Wave-1 execution, after Plans 09-03 (Desert+Dungeon) and 09-04 (Astral+Sulphurous+Briar) had already fully built and committed all 9 originally-planned biome pairs in their isolated worktrees, the user instructed: do not build separate subworlds (or, looking ahead, altars) for Dungeon or Sulphurous Sea. The orchestrator stopped both affected executor agents, cherry-picked only the wanted commits (Underworld, Space, Hallow, Jungle, Desert, Astral, Briar -- 7 of 9) onto master, and left Dungeon's and Sulphurous Sea's commits unmerged in their now-deleted worktree branches (discarded, not recoverable from master's history). Downstream plans 09-05/09-06/09-07 were revised in place to reference only the 7 kept biomes. See 09-CONTEXT.md D-07 for full rationale, and ROADMAP.md Phase 9 Success Criterion 2 for the resulting explicit exception (Polterghast/Dungeon, The Old Duke/Sulphurous Sea blocked until a future phase reinstates coverage).
- [Phase 09]: Parallel-worktree merge pattern used for Wave 1: rather than `git merge`-ing each worktree branch (which would conflict on STATE.md/ROADMAP.md/REQUIREMENTS.md since all 4 parallel executors independently modified those shared files from the same base commit), the orchestrator cherry-picked only each plan's `feat` commits onto master and hand-wrote/reconciled STATE.md/ROADMAP.md/REQUIREMENTS.md/SUMMARY.md once, centrally, after all four worktrees' code was extracted. Precedent for any future phase using `isolation: worktree` parallel execution with >1 wave-1 plan. Re-verify `phase complete`'s output against `git diff` every time, don't trust its `*_updated` flags at face value.
- [Phase 09 P05]: Fixed two compile-blocking bugs (Rule 3) in 09-05-PLAN.md's illustrative Debug/BiomeArenaDebugCommands.cs code, both confirmed via `ilspycmd` decompile of the actual installed `Libs/CalamityMod.dll`/`Libs/SpiritMod.dll` rather than assumption: (1) `player.Calamity().ZoneAstral` needs the `CalamityMod` namespace in scope for extension-method resolution -- called `CalamityMod.CalamityUtils.Calamity(player)` directly instead of adding a `using` directive; (2) `SpiritMod.Biomes.BiomeTileCounts` is `internal class BiomeTileCounts : ModSystem`, so its `public static bool InBriar` is unreachable at compile time (CS0122) -- read via reflection (`Type.GetType` + `PropertyInfo.GetValue`, try/catch + `Mod.Logger.Warn`), mirroring `Integrations/SpiritIntegration.cs`'s established internal-Spirit-type reflection pattern (there for a write, here for a read). Precedent: verify a plan's illustrative mod-API interface code against the actual installed DLL via `ilspycmd` before treating it as directly compilable, even when the underlying member/value it describes is confirmed correct by research.
- [Phase 09 P05]: Tooling note: `gsd-tools state advance-plan` failed with `"Cannot parse Current Plan or Total Plans in Phase from STATE.md"` (this project's STATE.md uses a narrative "Plan:" line, not the `Current Plan: N/Total Plans: M` format the tool expects) -- `state update-progress`/`state record-metric`/`state record-session` all worked correctly, but `advance-plan`'s Current Position narrative update was done manually instead, consistent with the Phase 05 tooling-note precedent above.
- [Phase 09]: [Phase 09 P06] Fixed Desert platform mid-checkpoint (Rule 1): full-depth falling-Sand fill caused a native stack-overflow via infinite WorldGen.SquareTileFrame/TileFrame/SpawnFallingBlockProjectile recursion; changed to a 3-row real-Sand cosmetic layer over solid Sandstone base (same SceneMetrics weight-1 per tile), mirroring UnderworldPlatformPass's established pattern. User re-tested live and confirmed no crash, ZoneDesert=True.
- [Phase 09]: [Phase 09 P06] All 7 biome Subworld/GenPass pairs (Hallow, Underworld, Jungle, Space, Desert, Astral, Briar) live-confirmed to generate correctly and satisfy their target Zone/Biome flag across all three mechanism families (vanilla SceneMetrics, height-only, modded ModBiome). ARENA-01 deliberately left unmarked complete in REQUIREMENTS.md -- this plan (and 09-07) only close the arena-construction/JIT-safety half; boss-classification-and-routing across Phases 6-8 remains outstanding.

### Roadmap Evolution

- Phase 9 added: Biome-Dependent Subworld Coverage — user requested during Phase 5 execution (05-02 checkpoint) after confirming Infernon's registration works live end-to-end. Generalizes Phase 4's ad-hoc `BossArenaCorruptionSubworld`/`BossArenaRoutingRegistry` fix (built live for Calamity's Hive Mind ZoneCorrupt despawn bug) into a systematic per-boss audit across all v1 mods, placed after Phase 8 per user's explicit placement choice. New requirement: ARENA-01.
- Phase 9 scope reduced 9→7 biome variants (D-07, 2026-08-14): Dungeon and Sulphurous Sea removed from Phase 9's build scope mid-execution by user decision, after both had already been built once. See 09-CONTEXT.md D-07 and the Decisions entry above for full detail.

### Pending Todos

None yet.

### Blockers/Concerns

- Phase 4 planning should resolve the weak-reference+[JITWhenModsEnabled] vs. pure-reflection disagreement between research files before writing the first Integrations/*.cs file (see research/SUMMARY.md Gaps).
- Phases 6-7 (Redemption, CatalystMod, NoxusBoss, ContinentOfJourney/Daybreak) have entirely unresearched APIs — each will likely need a `/gsd:research-phase` pass before detailed planning.
- Isolation premise NOT empirically confirmed: live King Slime kill test shows NPC.downedSlimeKing=True in the main world after subworld round-trip (expected False per 01-RESEARCH.md/PITFALLS.md, which both explicitly predicted vanilla flags behave the same as modded ones for this bug). Do NOT proceed to Phase 2/3 planning until re-investigated -- see 01-04-SUMMARY.md hypotheses (in-memory-only leak vs. genuine on-disk persistence vs. vanilla-specific behavior difference). Also unconfirmed: inventory-intact check (SUBW-06) was skipped by tester during this run.
- Dungeon and Sulphurous Sea biome-variant subworlds are deferred, not built (D-07, 2026-08-14, "for now"/일단). Blocks a future biome-safe arena for Polterghast (Spirit, Dungeon, unconditionally assignable) and The Old Duke (Calamity+Infernum, Sulphurous Sea) until a future phase reinstates them. Do not silently resurrect the discarded Wave-1 code (never merged, not reachable from master) -- treat any future request to add these back as new scope requiring its own research/planning pass.

## Session Continuity

Last session: 2026-08-14T04:16:42.158Z
Stopped at: Completed 09-06-PLAN.md
Resume file: .planning/phases/09-biome-dependent-subworld-coverage/09-07-PLAN.md
