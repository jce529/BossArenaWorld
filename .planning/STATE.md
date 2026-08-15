---
gsd_state_version: 1.0
milestone: v1.1
milestone_name: 아레나 서브월드 디자인 개선
status: Defining requirements
stopped_at: "Milestone v1.1 (아레나 서브월드 디자인 개선) started; requirements not yet defined"
last_updated: "2026-08-15T00:00:00.000Z"
last_activity: 2026-08-15
progress:
  total_phases: 0
  completed_phases: 0
  total_plans: 0
  completed_plans: 0
  percent: 0
---

# Project State

## Project Reference

See: .planning/PROJECT.md (updated 2026-08-15)

**Core value:** The generic boss-kill → carrier-item → main-world-apply mechanism (BossRegistry + BossCoreItem + GlobalNPC) must reliably reproduce a boss's full "downed" state — flags, netcode sync, and any WorldGen side effects — for any registered boss.
**Current focus:** v1.1 (아레나 서브월드 디자인 개선) started 2026-08-15 -- defining requirements and roadmap. v1.0 MVP history preserved in .planning/MILESTONES.md and .planning/milestones/v1.0-ROADMAP.md.

## Current Position

Phase: Not started (defining requirements)
Plan: —
Status: Defining requirements — Milestone v1.1 (아레나 서브월드 디자인 개선) started 2026-08-15

### Old Duke Descope -- RESOLVED (2026-08-15)

The debug session `old-duke-immediate-despawn-plain-arena` (investigating The Old Duke's immediate-despawn-after-spawn bug) reached a confirmed root cause (InfernumMode's own per-world "Infernum Mode" toggle resetting to false inside the throwaway arena subworld, weaponized by NoxusBoss's Old-Duke-hijack AI) and a general fix was implemented and kept (`ForceInfernumModeActiveInArena()`, forcing the toggle active on every arena entry via InfernumMode's sanctioned `Mod.Call`) -- see `.planning/debug/old-duke-immediate-despawn-plain-arena.md` Resolution section. Despite the fix existing and building cleanly, The Old Duke's own registration was still closed as wontfix/descoped rather than live re-verified and kept: user decision, The Old Duke stays out of v1 scope entirely (the Sulphurous Sea biome variant was already excluded per D-07, Phase 9 -- this closes the same loop). `Integrations/CalamityIntegration.cs`'s `RegisterOldDuke()`/`IsOldDukeDowned()`/`ApplyOldDukeDowned()` removed; the `InfernumMode` weak reference (build.txt/.csproj) and the new `ForceInfernumModeActiveInArena()` helper were kept, since Providence/Profaned Guardians (absence-gating) and Astrum Deus/Astrum Aureus (forced-night presence-gating) still depend on `InfernumMode`, and the new helper further benefits their gating correctness. v1's Calamity roster is now 11 bosses (was 12), total Phase 10 roster 17 (was 18). Both `10-06-SUMMARY.md` and `08-04-SUMMARY.md` close citing the already-user-confirmed live-verification results for the 17-boss roster. See quick task `.planning/quick/260815-024-the-old-duke-v1-despawn/`.

### Phase 10 Plan 01 -- COMPLETE (2026-08-14, this session)

`10-01-PLAN.md` (SummonItemRegistry polymorphic resolver + ForcedTimeSystem + Test1Tile wiring)
executed autonomously (no checkpoints), all 3 tasks committed, `dotnet build` passed with 0
warnings/0 errors after each task. See `.planning/phases/10-full-calamity-spirit-boss-roster-registration-and-biome-subworld-routing/10-01-SUMMARY.md`.
Next entry point for this track: `10-02-PLAN.md` (Calamity Tier 1: Devourer of Gods, Yharon,
Supreme Witch Calamitas, Dragonfolly). ARENA-01 stays open in REQUIREMENTS.md -- 10-01 only
built the shared foundation, not real boss registrations.

### Phase 10 Plans 02 & 03 -- COMPLETE (2026-08-14, this session, parallel worktrees)

`10-02-PLAN.md` (Calamity Tier 1: Devourer of Gods, Yharon, Supreme Witch Calamitas plain-arena
registrations + Dragonfolly Jungle-routed registration) executed autonomously (no checkpoints)
in an isolated worktree parallel to Plan 10-03 (`Integrations/SpiritIntegration.cs`, different
file, no conflict). Both tasks committed (`56bc0bc`, `21d145d`), `dotnet build` passed with 0
warnings/0 errors after each task. One Rule-3 auto-fix: plan's illustrative code referenced
`CalamityMod.CalamityGlobalTownNPC` (wrong namespace) instead of the real
`CalamityMod.NPCs.CalamityGlobalTownNPC`, confirmed via `ilspycmd` decompile and corrected before
Task 1 would compile. See
`.planning/phases/10-full-calamity-spirit-boss-roster-registration-and-biome-subworld-routing/10-02-SUMMARY.md`.
`Integrations/CalamityIntegration.cs` now registers 5 Calamity bosses total (Hive Mind + this
plan's 4: `calamity:devourer_of_gods`, `calamity:yharon`, `calamity:supreme_calamitas`,
`calamity:dragonfolly`).

`10-03-PLAN.md` (Spirit full roster: Ancient Avian, Scarabeus, Vinewrath Bane, Moon Jelly
Wizard, Dusking, Atlas) executed autonomously (no checkpoints), all 3 tasks committed
(`baeb553`, `7e2c7cb`, `5f8ea8b`), `dotnet build` passed with 0 warnings/0 errors after each
task. Ran as a parallel executor in an isolated worktree alongside sibling plan 10-02
(`Integrations/CalamityIntegration.cs`, disjoint file) -- all commits used `--no-verify` per
orchestrator instructions to avoid pre-commit hook contention; orchestrator validated hooks
once after both completed. See
`.planning/phases/10-full-calamity-spirit-boss-roster-registration-and-biome-subworld-routing/10-03-SUMMARY.md`.
Spirit's full 7-boss roster (Infernon from Phase 5 + these 6) is now code-complete.

ARENA-01 stays open in REQUIREMENTS.md until the full roster (10-04..10-06) lands and Plan
10-06's live in-game verification confirms behavior. Next entry point for this track:
`10-04-PLAN.md` (Infernum-gated/polymorphic Calamity tier).

### Phase 10 Plan 04 -- COMPLETE (2026-08-14, this session)

`10-04-PLAN.md` (Infernum-gated + polymorphic Calamity tier: Providence, Profaned Guardians,
Astrum Deus, Astrum Aureus, Ceaseless Void, Signus, Storm Weaver) executed autonomously (no
checkpoints), all 3 tasks committed (`95567b9`, `cfc0342`, `add81c6`), `dotnet build` passed
with 0 warnings/0 errors after each task. No deviations -- plan's illustrative code compiled
against `Libs/CalamityMod.dll` v2.2.4 exactly as written. See
`.planning/phases/10-full-calamity-spirit-boss-roster-registration-and-biome-subworld-routing/10-04-SUMMARY.md`.
`Integrations/CalamityIntegration.cs` now registers 12 of 12 Calamity bosses this phase covers
directly (Hive Mind + 4 from 10-02 + these 7). Only The Old Duke (Plan 10-05, needs a new
external DLL) remains to finish Calamity's full roster. Providence/Profaned Guardians registered
only when InfernumMode is absent (D-02); Astrum Deus/Astrum Aureus force night only when
InfernumMode is loaded; MarkofProvidence resolved polymorphically via Plan 10-01's
`SummonItemRegistry.RegisterPolymorphic` -- first real exercise of both Plan 10-01 capabilities.

ARENA-01 stays open in REQUIREMENTS.md until 10-05/10-06 land and Plan 10-06's live
in-game verification confirms behavior. Next entry point: `10-05-PLAN.md` (The Old Duke,
needs a new external DLL).

### Phase 10 Plan 05 -- COMPLETE (2026-08-14, this session)

`10-05-PLAN.md` (InfernumMode weak reference wiring + The Old Duke Infernum-only
registration) executed autonomously (no checkpoints), both tasks committed (`05e8786`,
`00d2daa`), `dotnet build` passed with 0 warnings/0 errors after each task. One Rule-3
auto-fix: the plan's illustrative XML doc-comment text for the new `InfernumMode`
`<Reference>` block in `BossArenaSubWorld.csproj` contained literal `--` (MSB4025, same
class of bug as Phase 7's `ContinentOfJourney` fix) -- rephrased without double-dashes,
build succeeded after. `Libs/InfernumMode.dll` copied from `../ModAssemblies/InfernumMode_v2.0.1.35.dll`
(gitignored, per-worktree setup step, not a code deviation). See
`.planning/phases/10-full-calamity-spirit-boss-roster-registration-and-biome-subworld-routing/10-05-SUMMARY.md`.
`Integrations/CalamityIntegration.cs` now registers all 12 of 12 Calamity bosses this phase
covers, including The Old Duke (`calamity:old_duke`, `downedBoomerDuke`, gated to both
CalamityMod AND InfernumMode present -- internal guard correctly checks
`HasMod("InfernumMode")`, not `HasMod("CalamityMod")`, per the plan-checker fix already
applied to the plan file before this execution, commit `0c90ffa`).

ARENA-01 stays open in REQUIREMENTS.md until Plan 10-06 lands and its live in-game
verification confirms behavior (including Phase 7's still-pending Goblin Chariot checkpoint).
Next entry point: `10-06-PLAN.md` (live verification checkpoint + mod-disabled safety
checkpoint) -- the final plan in Phase 10.

### Phase 06/07/08/10 live-verification round -- RESOLVED (2026-08-14/15, this session)

In one round of live in-game testing, the user worked through `check.md`'s consolidated checklist (sections ①-④) covering four separate plans' checkpoints:

- **08-01** (Boss Checklist sanity + King Slime/Hive Mind tracker-UI recognition + Infernon citation): all items passed. See `08-01-SUMMARY.md`.
- **07-02** (Goblin Chariot pipeline + Boss Checklist recognition + ContinentOfJourney-disabled safety): all items passed. Closes Phase 7 (MOD-06 fully satisfied end-to-end). See `07-02-SUMMARY.md`.
- **08-02** (Thorn/Astrageldon pipeline + Boss Checklist + Moon-Lord-lockout + Redemption/CatalystMod-disabled safety): all items passed, including the two new-this-session Moon Lord lockout cases. Since `06-03-SUMMARY.md` did not exist prior to this session, this result also closes Phase 6's own outstanding `06-03` checkpoint by design (see `06-03-SUMMARY.md`, which cites this file). See `08-02-SUMMARY.md`.
- **08-03** (Goblin Chariot, Phase 8's own copy of the same checkpoint): closed by citation of `07-02-SUMMARY.md` (executed first this session) rather than a duplicate live test, per `08-03-PLAN.md`'s own "if not already done" design. See `08-03-SUMMARY.md`.
- **10-06** (Phase 10's full-roster live verification): all 17 in-scope bosses + gating matrix/idempotency/JIT-safety items passed live this session. The Old Duke was excluded from scope entirely (descoped 2026-08-15, quick task 260815-024) rather than remaining a failure. See 10-06-SUMMARY.md.

Net effect: Phase 6 complete (3/3), Phase 7 complete (2/2), Phase 8 complete (4/4), Phase 10 complete (6/6) -- all closed 2026-08-15 after The Old Duke was descoped from v1.

Last activity: 2026-08-15

### Phase 08 and Phase 10 plan-checker re-verification -- RESOLVED (2026-08-14, this session)

The prior session's two `gsd-plan-checker` background agents never reported back before it ended. Both were re-run fresh this session:

- **Phase 08 plans** (`08-01`..`08-04-PLAN.md`, committed `e1494a2`) -- `VERIFICATION PASSED`, zero issues. Ready to execute (`/gsd:execute-phase 8`); Wave 1 (`08-01`) has no dependency and is immediately executable, Waves 2-3 self-gate on Phase 6/7/10 live-verification status.
- **Phase 10 plans** (`10-01`..`10-06-PLAN.md`, committed `44e043f`) -- first pass found **1 real blocker**: `10-05-PLAN.md`'s `RegisterOldDuke()` guarded on `HasMod("CalamityMod")` instead of `HasMod("InfernumMode")`, which would have caused a live `TypeLoadException`/JIT crash the moment CalamityMod is present without InfernumMode (breaking Phase 10 Success Criterion 2). Also 1 warning: a stale `build.txt` snippet risked dropping Phase 7's `ContinentOfJourney@0.8.70.88` weakReferences entry on literal find/replace. Both fixed by a `gsd-planner` revision pass, committed `0c90ffa`. Re-verification after the fix: `VERIFICATION PASSED`, zero remaining issues. Ready to execute (`/gsd:execute-phase 10`).

Progress: [██████████] 100% (36 of 36 currently-planned plans across Phases 1-10 complete; The Old Duke removed from scope via quick task 260815-024, 2026-08-15)

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
| Phase 09 P07 | 25min | 2 tasks | 3 files |
| Phase 06 P01 | 8min | 2 tasks | 2 files |
| Phase 06 P02 | 10min | 3 tasks | 4 files |
| Phase 07 P01 | 12min | 2 tasks | 3 files |
| Phase 10 P01 | 12min | 3 tasks | 3 files |
| Phase 10 P02 | 15min | 2 tasks | 1 files |
| Phase 10 P03 | 4min | 3 tasks | 1 files |
| Phase 10 P04 | 8min | 3 tasks | 1 files |
| Phase 10 P05 | 5min | 2 tasks | 3 files |

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
- [Phase 09]: [Phase 09 P07]: Live CalamityMod-disabled checkpoint caught a real JITException in AstralPlatformPass.ApplyPass -- lazy construction inside Subworld.Tasks alone is NOT sufficient JIT protection; every method touching a weak-referenced mod's types needs its own [JITWhenModsEnabled] attribute regardless of containing-class laziness. Fixed both AstralPlatformPass.cs and BriarPlatformPass.cs; both mod-disabled checkpoints re-verified clean.
- [Phase 06]: Plan 06-01: CatalystMod.dll extracted from Steam Workshop content cache via scripts/extract_tmod.py (not in local Mods/ folder); fresh worktree required manually copying pre-existing Libs/SubworldLibrary.dll, CalamityMod.dll, SpiritMod.dll from main working tree to unblock dotnet build verification (known per-worktree setup gap, STATE.md Phase 02)
- [Phase 06]: Fixed CatalystMod.MetanovaGenerator namespace (Rule 3): real path is CatalystMod.Common.World.MetanovaGenerator, confirmed via ilspycmd decompile
- [Phase 06]: User-approved scope addition: SummonItemRegistry gained an optional named canSummon eligibility delegate to replicate CatalystMod AstralCommunicator real Moon-Lord-lockout CanUseItem() behavior, gated in Test1Tile.RightClick; Thorn (Redemption) has no equivalent lockout
- [Phase 06]: Tooling note: state advance-plan again failed with the same STATE.md narrative-format parse error documented in Phase 05/09 (Current Position uses a narrative Plan: line, not Current Plan: N/Total Plans: M); Current Position/frontmatter progress updated manually as workaround, consistent with prior precedent
- [Phase 07]: [Phase 07]: Fixed compile-blocking XML comment double-dash (Rule 3) in BossArenaSubWorld.csproj's new ContinentOfJourney Reference block doc comment (MSB4025) before dotnet restore/build could succeed
- [Phase 07]: [Phase 07]: Registered continentofjourney:goblin_chariot via Integrations/HomewardJourneyIntegration.cs, direct public-static-field write to ContinentOfJourney.DownedBossSystem.downedGoblinChariot, no reflection, no BossArenaRoutingRegistry needed (no biome dependency), closing MOD-06 code-level registration -- live verification deferred to Plan 02
- [Phase 07]: [Phase 07]: Tooling note: state update-progress silently wrote a stale percent (96 instead of the correct 75 for 24/32) into STATE.md frontmatter, matching the known case-insensitive-regex bug documented in Phase 05/09/06 notes (matches frontmatter progress: key instead of body Progress: line); fixed both the frontmatter percent and body Progress line manually as workaround
- [Phase 10]: Kept existing single-item TryGetBoss(int, out int) overload untouched; added a separate player-aware TryGetBoss(Player, int, out int) overload and RegisterPolymorphic for multi-boss summon items, zero regression to existing boss registrations
- [Phase 10]: ForcedTimeSystem.ActiveArenaBossNpcType intentionally never cleared on arena exit -- PreUpdateWorld's IsAnyArenaActive() guard alone makes this safe, avoiding a consume-once pattern that would break the every-tick re-assertion needed for multi-minute fights (10-RESEARCH.md Pitfall 6)
- [Phase 10]: [Phase 10]: Fixed compile-blocking namespace bug in 10-02-PLAN.md's illustrative code (Rule 3): CalamityGlobalTownNPC lives in CalamityMod.NPCs, not CalamityMod directly -- confirmed via ilspycmd decompile of installed CalamityMod.dll, corrected in Integrations/CalamityIntegration.cs before Task 1 would compile.
- [Phase 10]: Spirit full roster: ApplyGenericSpiritDowned<T>() shared reflection helper generalizes Infernon's write path across 6 bosses; Vinewrath Bane registers dual ReachBoss/ReachBoss1 NPC types (DownedVinewrath reads ReachBoss1 specifically)
- [Phase 10]: [Phase 10]: Plan 10-04 registered 7 more Calamity bosses (Providence, Profaned Guardians, Astrum Deus, Astrum Aureus, Ceaseless Void, Signus, Storm Weaver) -- Providence/Profaned Guardians gated to InfernumMode-absent, Astrum Deus/Aureus force night only when InfernumMode is loaded, MarkofProvidence resolved polymorphically (first real exercise of Plan 10-01's polymorphic resolver + forced-night capabilities). Integrations/CalamityIntegration.cs now registers 12 of 12 Calamity bosses this phase covers directly (The Old Duke remains, Plan 10-05).
- [Phase 10]: [Phase 10]: Plan 10-05 wired InfernumMode as a new weak reference and registered The Old Duke (Calamity+Infernum AND-gate). Internal guard checks HasMod("InfernumMode"), not HasMod("CalamityMod") -- avoids a no-op guard that would crash via lazy JIT in the CalamityMod-only configuration. Fixed a Rule 3 MSB4025 XML-comment double-dash bug in the new csproj Reference block (same class of bug as Phase 7's ContinentOfJourney fix). All 12 Calamity boss registrations this phase covers are now code-complete.
- [Phase 10]: Live 10-06 checkpoint (2026-08-14) confirmed 17 of 18 registered bosses correct end-to-end, including: Dragonfolly's no-despawn Jungle fight, Scarabeus's normal (non-scaled) damage in Desert, Ancient Avian's normal Space fight, MarkofProvidence's polymorphic resolution to Ceaseless Void/Signus/Storm Weaver across all 3 reachable Zones with no item consumption, the full Infernum-conditional gating matrix in BOTH configurations (Providence/Profaned Guardians/Ceaseless Void redirect only when InfernumMode absent, Astrum Deus/Aureus force night only when InfernumMode present), APPLY-04 re-use idempotency, Moon Jelly Wizard/Dusking full-duration forced-night persistence with no mid-fight daytime despawn (Pitfall 6 confirmed non-issue), and CalamityMod/SpiritMod-disabled JIT safety. The ONE failure: The Old Duke despawns immediately after spawning in the default plain-stone arena -- contradicts 10-RESEARCH.md's decompile conclusion that OldDuke.cs has no Sulphurous-Sea Zone-flag dependency. User hypothesizes the wiki's Sulphurous Sea requirement was correct and some other AI/biome-check mechanism (missed by the prior decompile pass) is the real cause. Debug session `old-duke-immediate-despawn-plain-arena` spawned via `/gsd:debug` to investigate via fresh decompile of the live `CalamityMod.NPCs.OldDuke.OldDuke` AI (and check InfernumMode's own AI override, since Infernum reworks many Calamity bosses) -- see PENDING BUG under Current Position. This closely mirrors the Phase 4 Hive Mind/ZoneCorrupt precedent: a research pass's decompile conclusion turned out incomplete once tested live.
- [Phase 10]: The Old Duke removed from v1 scope entirely (2026-08-15, quick task 260815-024). Its immediate-despawn bug (debug session old-duke-immediate-despawn-plain-arena) WAS root-caused (NoxusBoss's Old-Duke-hijack AI firing whenever InfernumMode's per-world toggle reads false, which it always does inside the throwaway arena subworld) and a general fix (`ForceInfernumModeActiveInArena()`) was implemented, build-verified, and kept in the codebase since it also benefits Providence/Profaned Guardians/Astrum Deus/Astrum Aureus's Infernum-conditional gating correctness. Despite the fix existing, The Old Duke's own registration was still removed by user decision rather than live re-verified and re-added -- a deliberate scope-closing choice, mirroring the Sulphurous Sea exclusion already made for the same boss under D-07 (Phase 9). Integrations/CalamityIntegration.cs's RegisterOldDuke/IsOldDukeDowned/ApplyOldDukeDowned removed; InfernumMode weak reference kept. Phase 10's roster is now 17 bosses (11 Calamity + 6 Spirit), down from 18.
- [Quick 260815-to6]: `BossSummonPlayer.OnEnterWorld()`'s call to `CalamityIntegration.ForceInfernumModeActiveInArena()` was gated to Calamity-sourced boss summons only (`BossRegistry.TryGetKeyForNpc(...)` + `bossKey.StartsWith("calamity:")`), replacing the prior unconditional "call whenever InfernumMode is installed" guard -- a Spirit-mod (or any other non-Calamity) boss summon no longer touches InfernumMode's per-world toggle. No behavioral change for any currently-registered Calamity boss (Providence/Profaned Guardians/Astrum Deus/Astrum Aureus gating unaffected).
- [Quick 260815-u7g]: Replaced quick task 260815-to6's `bossKey.StartsWith("calamity:")` string-prefix heuristic with an explicit `BossDefinition.RequiresInfernumToggle` flag (default `false`), added via a new optional 4th record parameter (source-compatible with all other mods' existing `new BossDefinition(...)` call sites) plus a new `BossRegistry.TryGetDefinitionForNpc(int, out BossDefinition)` accessor. Providence/Profaned Guardians/Astrum Deus/Astrum Aureus now set the flag `true`; `catalyst:astrageldon` is confirmed and documented (via `ilspycmd` decompile of `Libs/InfernumMode.dll`: zero references to Astrageldon or CatalystMod across 2097 types) to correctly stay `false`, closing the code-review-flagged silent-exclusion gap (Astrageldon is `catalyst:`-keyed, not `calamity:`-keyed, so it was invisible to the prior string-prefix check). Pure refactor, zero behavioral change for any currently-registered boss -- no live re-verification needed.

### Roadmap Evolution

- Phase 9 added: Biome-Dependent Subworld Coverage — user requested during Phase 5 execution (05-02 checkpoint) after confirming Infernon's registration works live end-to-end. Generalizes Phase 4's ad-hoc `BossArenaCorruptionSubworld`/`BossArenaRoutingRegistry` fix (built live for Calamity's Hive Mind ZoneCorrupt despawn bug) into a systematic per-boss audit across all v1 mods, placed after Phase 8 per user's explicit placement choice. New requirement: ARENA-01.
- Phase 9 scope reduced 9→7 biome variants (D-07, 2026-08-14): Dungeon and Sulphurous Sea removed from Phase 9's build scope mid-execution by user decision, after both had already been built once. See 09-CONTEXT.md D-07 and the Decisions entry above for full detail.
- Phase 10 added (2026-08-14): Full Calamity/Spirit boss roster registration and biome subworld routing — user requested during Phase 6 execution, after confirming only one worked-example boss per mod (Hive Mind/Calamity, Infernon/Spirit) is currently registered despite Phase 9's 09-ALTAR-BIOME-REFERENCE.md already having biome-classified the full researched roster (Astrum Deus/Aureus, Vinewrath Bane, etc.). This expands v1 scope beyond the "one boss per mod, prove the pattern" discipline used in Phases 3-6 — full-roster registration was previously deferred/unscheduled (see PROJECT.md Out of Scope: "no boss priority ordering"). Needs its own `/gsd:discuss-phase 10` and research/planning pass before execution.
- Phase 7 rescoped (2026-08-14, discuss-phase): NoxusBoss (Devourer of Universes) removed from v1 scope entirely — user decision, most NoxusBoss bosses are quest-triggered (Solyn's moon-event questline) or already run in their own dedicated subworld/arena mechanic, don't fit the carrier-item pattern; no plan to revisit. MOD-05 marked Removed in REQUIREMENTS.md, moved to PROJECT.md Out of Scope. Phase 7 renamed "ContinentOfJourney/Daybreak (Homeward Journey) Integration", ROADMAP.md Goal/Success Criteria/Requirements trimmed to MOD-06 only. Also resolved during the same discuss-phase: "ContinentOfJourney" identified as **Homeward Journey** (GabeHasWon, Steam Workshop id 2930931197) — user supplied the link directly, confirming the guess that the phase title's "(Homeward series)" parenthetical was the actual pointer (09-ALTAR-BIOME-REFERENCE.md Open Item 1, previously unresolved across two research passes). "Daybreak" reconfirmed as `gold-meridian/daybreak-mod`, a boss-less library dependency of Wrath of the Gods.

### Pending Todos

None yet.

### Blockers/Concerns

- Phase 4 planning should resolve the weak-reference+[JITWhenModsEnabled] vs. pure-reflection disagreement between research files before writing the first Integrations/*.cs file (see research/SUMMARY.md Gaps).
- Phase 7 is COMPLETE (2026-08-14) — Goblin Chariot registered and live-verified end-to-end, including Boss Checklist recognition and ContinentOfJourney-disabled safety. NoxusBoss removed from v1 scope (2026-08-14, see Roadmap Evolution).
- Phase 6 is COMPLETE (2026-08-14) — Thorn/Astrageldon registered and live-verified end-to-end (closed via 08-02's citation, see `06-03-SUMMARY.md`).
- Phase 8 is COMPLETE (4/4, 2026-08-15): 08-01/08-02/08-03 closed 2026-08-14; 08-04 closed 2026-08-15 citing 10-06-SUMMARY.md for the 17-boss roster (The Old Duke descoped, quick task 260815-024).
- Phase 10 is COMPLETE (6/6, 2026-08-15): all 17 in-scope bosses + the full Infernum-gating matrix + polymorphic item + forced-night persistence + mod-disabled JIT safety were user-confirmed passing live (2026-08-14). The Old Duke was removed from v1 scope entirely (2026-08-15, quick task 260815-024) rather than kept -- its despawn bug was investigated and root-caused, and a general fix was implemented and kept, but The Old Duke's own registration was still descoped by user decision; see .planning/debug/old-duke-immediate-despawn-plain-arena.md Resolution section.
- Isolation premise NOT empirically confirmed: live King Slime kill test shows NPC.downedSlimeKing=True in the main world after subworld round-trip (expected False per 01-RESEARCH.md/PITFALLS.md, which both explicitly predicted vanilla flags behave the same as modded ones for this bug). Do NOT proceed to Phase 2/3 planning until re-investigated -- see 01-04-SUMMARY.md hypotheses (in-memory-only leak vs. genuine on-disk persistence vs. vanilla-specific behavior difference). Also unconfirmed: inventory-intact check (SUBW-06) was skipped by tester during this run.
- Dungeon and Sulphurous Sea biome-variant subworlds are deferred, not built (D-07, 2026-08-14, "for now"/일단). Blocks a future biome-safe arena for Polterghast (Spirit, Dungeon, unconditionally assignable) and The Old Duke (Calamity+Infernum, Sulphurous Sea) until a future phase reinstates them. Do not silently resurrect the discarded Wave-1 code (never merged, not reachable from master) -- treat any future request to add these back as new scope requiring its own research/planning pass. **UPDATE (2026-08-15):** The Old Duke's despawn bug was investigated and root-caused -- NOT the Sulphurous Sea Zone dependency this entry predicted, but a cross-mod subworld-isolation interaction (InfernumMode's per-world toggle resetting inside the throwaway arena, weaponized by NoxusBoss's Old-Duke-hijack AI). A general fix was implemented and kept (benefits Providence/Profaned Guardians/Astrum Deus/Astrum Aureus too). Despite the fix, The Old Duke was removed from v1 scope entirely (quick task 260815-024) by deliberate user decision rather than live re-verified and kept. This closes that open question permanently for v1, not just defers it. Polterghast/Dungeon remains the only still-open item this entry describes.

### Quick Tasks Completed

| # | Description | Date | Commit | Directory |
|---|-------------|------|--------|-----------|
| 260815-to6 | Infernum 모드 강제 활성화를 칼라미티 소속 보스에만 적용 | 2026-08-15 | 8629799 | [260815-to6-infernum](./quick/260815-to6-infernum/) |
| 260815-u7g | BossDefinition에 명시적 RequiresInfernumToggle 플래그 추가 (문자열 접두사 추론 대체) | 2026-08-15 | 954ee88 | [260815-u7g-bossdefinition-requiresinfernumtoggle-ca](./quick/260815-u7g-bossdefinition-requiresinfernumtoggle-ca/) |

Last activity: 2026-08-15 - Completed quick task 260815-u7g: BossDefinition에 명시적 RequiresInfernumToggle 플래그 추가

## Session Continuity

Last session: 2026-08-15T00:00:00.000Z
Stopped at: Quick task 260815-u7g complete (BossDefinition.RequiresInfernumToggle explicit flag replaces bossKey.StartsWith("calamity:") heuristic); The Old Duke descoped from v1 (quick task 260815-024); Phase 10 (10-06) and Phase 8 (08-04) both closed
Resume file: none -- no open blockers

**What's in flight when this session ends:**

1. The Old Duke despawn bug was descoped, not fixed-and-kept-in-scope -- see .planning/debug/old-duke-immediate-despawn-plain-arena.md Resolution section and quick task 260815-024. No longer in flight.
2. Phases 6 and 7 are now fully COMPLETE (all live-verification checkpoints passed this session) -- see `06-03-SUMMARY.md`, `07-02-SUMMARY.md`.
3. Phase 8 is COMPLETE (4/4) -- 08-04 closed 2026-08-15.
4. Phase 10 is COMPLETE (6/6) -- 10-06 closed 2026-08-15 for the resulting 17-boss roster.
5. Worktree setup note (historical, from earlier this session's code-writing waves): gitignored `Libs/*.dll` compile-time references were missing in each fresh worktree and were copied from the main working tree before each first build; they remain gitignored, not committed. `Libs/InfernumMode.dll` was also copied into the main working tree's `Libs/` folder (from `../ModAssemblies/InfernumMode_v2.0.1.35.dll`) so master itself can build Plan 10-05's code.
6. Build-lock note: immediately after merging Plan 10-05, `dotnet build` failed with `TML003: Please close tModLoader or disable the mod in-game to build mods directly` (C# compiles clean, 0 errors -- only `.tmod` packaging was locked because tModLoader was running). The user was told to close tModLoader and rebuild before live-testing Wave 4/5 content; unclear whether a fresh successful build has been confirmed since (worth re-checking `dotnet build BossArenaSubWorld.csproj` at the start of the next session if any doubt exists about whether the currently-loaded `.tmod` matches the latest merged code).

All planning artifacts (CONTEXT/RESEARCH/VALIDATION/PLAN/ROADMAP/REQUIREMENTS/PROJECT/STATE) through this point are committed to git except this STATE.md update itself (about to be committed) -- nothing is lost.
