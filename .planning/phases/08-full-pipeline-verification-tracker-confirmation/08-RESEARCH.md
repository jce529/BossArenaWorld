# Phase 8: Full Pipeline Verification & Tracker Confirmation - Research

**Researched:** 2026-08-14
**Domain:** Synthesis / verification-planning (no new mod-API research — this phase consumes the outputs of Phases 3-7, 9, 10) — cross-phase master roster compilation, execution-status audit, and Boss-Checklist-recognition gap closure
**Confidence:** HIGH — every roster/status claim below is sourced either directly from this session's own `git log`/file-existence checks, or from a re-read of the actual `SUMMARY.md`/`RESEARCH.md` files of the phase that produced it (not from CONTEXT.md's secondhand summaries)

## Project Constraints (from CLAUDE.md)

- Tech stack: tModLoader mod in C#, .NET 8.0 SDK, `dotnet msbuild` build — unchanged, this phase adds no new code
- Must reproduce each source mod's actual `OnKill` side effects (flag + netcode sync + WorldGen), not just a boolean — already the standard this phase verifies against, not something it changes
- Each content mod's downed-progress API varies (Calamity: wrapper properties; Spirit: raw/reflected fields; Redemption/CatalystMod/ContinentOfJourney: direct public-static fields) — registration code is already written per-mod; Phase 8 does not write new registration code, it verifies what exists
- GSD Workflow Enforcement: this phase's plan(s) must be produced/executed through `/gsd:plan-phase` / `/gsd:execute-phase`, not ad-hoc edits
- Communication with the user must be in Korean; code/commit-message/doc artifacts (including this file) stay in English, per established project convention

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions

- **D-01 (Scope expansion — full roster, not "one per mod"):** Phase 8 was originally scoped to "at least one boss per registered mod." The user explicitly expanded this to cover **every registered boss across every integrated mod** — vanilla (King Slime), Calamity (Hive Mind + Phase 10's full ~12-boss roster), Spirit (Infernon + Phase 10's full ~6-boss roster), Redemption (Thorn), CatalystMod (Astrageldon), ContinentOfJourney/Daybreak i.e. Homeward Journey (Goblin Chariot).
  - Rationale (user's own words): "이미 각 페이즈에서 모드별로 한 보스씩 작동을 확인했고 문제가 없음을 확인 (BossCheckList까지 전부) 그래서 이제 모든 보스로 범위를 넓힐 생각" — each mod-integration phase already proved its one-worked-example boss works end-to-end, so Phase 8's remaining job is full-roster breadth confirmation, not re-proving the underlying mechanism.
  - **Correction surfaced during discussion (re-verified by this research pass against actual SUMMARY.md files, not just CONTEXT.md's summary — see "Boss Checklist Recognition Status Re-Verification" below):** Boss Checklist recognition specifically was only explicitly confirmed for **Infernon** (Phase 5). King Slime (Phase 3) used an alternative confirmation method (internal flag read + distinct success/idempotency chat messages — no tracker-mod UI check performed). Hive Mind (Phase 4) confirmed its own side effects (Sky Ore broadcast, `CalamityNetcode.SyncWorld()`, real WorldGen tile conversion) but never checked Boss Checklist's UI either. Thorn/Astrageldon (Phase 6) verification is still pending its own `06-03` live checkpoint (not executed as of this research pass — see Execution Status Audit below). **Phase 8 must close Boss Checklist recognition for the original baseline set too, not only the newly-added Phase 9/10 roster.**
  - `ROADMAP.md` Phase 8 Goal/Success Criteria and `REQUIREMENTS.md` VERIFY-01/VERIFY-03 have already been updated to reflect the expanded scope — treat as already-locked project state.

### Claude's Discretion

- Exact plan/task structure for verifying dozens of bosses (likely grouped by mod or by wave, mirroring `06-03-PLAN.md`'s `check.md` and Phase 9/10's `*-VALIDATION.md` manual-checklist patterns) — see "Recommended Plan Structure" below.
- Whether Phase 8 execution should logically wait until Phase 6 (`06-03`), Phase 7 (execution, not just research), and Phase 10 (execution) all actually complete before its own live checkpoints can run — obviously implied by the dependency chain. This research pass confirms, as of 2026-08-14, that **none of the three have executed** (see Execution Status Audit) — this is the single most important planning input in this document.
- Whether to fold Homeward Journey's own bundled `CoJ_BossChecklist.cs` integration confirmation (07-RESEARCH.md Open Question 2) into Phase 8's Goblin Chariot checkpoint — **recommended: yes**, fold it in, since Phase 8 is exactly the "confirmed recognized by Boss Checklist" phase.

### Deferred Ideas (OUT OF SCOPE)

None — discussion stayed within phase scope. Exo Mechs and Starplate Voyager are not "deferred" from Phase 8 — they are **permanently excluded from v1 entirely** (Phase 10 planning decision, `PROJECT.md` Out of Scope table: both use a non-item trigger mechanism SUBW-01 excludes from v1). They never had a row in the Phase 10 roster to begin with and do not appear in the master table below.
</user_constraints>

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|------------------|
| VERIFY-01 | Full pipeline (subworld kill → item drop → main-world apply) verified end-to-end in singleplayer for every registered boss across every integrated mod | Master Boss Roster table below enumerates all 24 in-scope bosses with exact registration source, execution status, and whether their pipeline has already been live-verified (vs. code-registered-only, vs. not-yet-registered). Blocked-boss list identifies exactly which are not yet executable. |
| VERIFY-03 | Applied flags confirmed recognized by Boss Checklist (or equivalent tracker mod) after application — not just internally consistent | Master table's "Boss-Checklist-Recognition Status" column, re-verified against actual Phase 3/4/5 SUMMARY.md files (not CONTEXT.md's secondhand claim). Local Boss Checklist install status documented below. |

</phase_requirements>

## Summary

Phase 8 requires no new mod-API research — every downed-flag API, side effect, and Zone dependency it needs to verify was already researched and (mostly) implemented in Phases 3-7, 9, and 10. This document's job is instead to **compile the single master roster** the planner needs, and to establish — via direct `git log` / file-existence checks performed this session, not assumption — which of Phase 8's prerequisite phases have actually executed as of 2026-08-14.

**The critical finding:** of Phase 8's stated dependencies (Phase 7 execution, effectively also Phase 6's `06-03` and Phase 10's execution), **none have executed yet**. Only research/context artifacts exist for Phase 7 and Phase 10 (no `PLAN.md` for either), and Phase 6's `06-02` code registration is complete but its `06-03` live-verification plan has not been run (checklist unchecked, no `06-03-SUMMARY.md`). This means **18 of the 24 bosses in Phase 8's expanded scope cannot be live-verified yet** — only 6 bosses (King Slime, Hive Mind, Infernon, plus Thorn/Astrageldon which are code-registered-but-unverified, plus Goblin Chariot which is not yet registered at all) are even reachable in the current codebase, and of those only 3 (King Slime, Hive Mind, Infernon) have a working, live-tested pipeline today.

**Primary recommendation:** Do not write Phase 8 as one flat checklist across all 24 bosses. Structure it in dependency-ordered waves (detailed in "Recommended Plan Structure" below): an immediately-executable wave that closes the Boss-Checklist gap for the 3 already-working bosses, then explicitly gated waves for Thorn/Astrageldon (blocked on `06-03`), Goblin Chariot (blocked on Phase 7 execution), and the full Phase 10 roster (blocked on Phase 10 execution — itself likely multi-wave given its ~18-boss scope and two new architecture pieces). The planner/orchestrator should treat Phase 8 planning as safe to do now (per CONTEXT.md's own instruction), but Phase 8 *execution* of the later waves cannot begin until those phases land.

## Master Boss Roster

One row per boss in v1 scope. "Registered In" cites the exact plan; "Execution Status" was verified this session via `git log --oneline` (no `06-03`/Phase 7/Phase 10 execution commits exist beyond `docs(...)` research/context commits) and via checking for the presence/absence of each phase's `*-SUMMARY.md` files (not assumed from `RESEARCH.md`/`CONTEXT.md` existence alone, per this task's explicit instruction).

| Boss | Source Mod | Registered In | Downed-Flag API | Arena/Subworld Routed To | Boss-Checklist Status | Execution Status |
|---|---|---|---|---|---|---|
| King Slime | vanilla | Phase 3 (03-01/03-02, live-verified 03-03) | `NPC.downedSlimeKing` via `NPC.SetEventFlagCleared` | Default `BossArenaSubworld` | **Not Yet Confirmed** — alternative method used (internal flag + distinct success/idempotency chat messages); `03-03-SUMMARY.md` contains no Boss Checklist UI check | **Complete** — pipeline live-verified end-to-end (03-03-SUMMARY.md, 2026-08-13) |
| Hive Mind | Calamity | Phase 4 (04-01/04-02) | `CalamityMod.DownedBossSystem.downedHiveMind` (wrapper property → `SetEventFlagCleared`) | `BossArenaCorruptionSubworld` (functional — `ZoneCorrupt` despawn dependency, Phase 4 debug session) | **Not Yet Confirmed** — own side effects (Sky Ore broadcast, `CalamityNetcode.SyncWorld()`, real Aerialite ore conversion) confirmed live; `04-02-SUMMARY.md` contains no Boss Checklist UI check | **Complete** — pipeline + WorldGen/netcode side effects + Calamity-disabled JIT safety all live-verified (04-02-SUMMARY.md, 2026-08-13) |
| Infernon / InfernoSkull | Spirit | Phase 5 (05-01/05-02) | `SpiritMod.MyWorld.DownedInfernon` (read) + cached-reflection write into internal `BossDownedTracker.Downed` dictionary | `BossArenaUnderworldSubworld` (wiki-thematic, Phase 9) | **CONFIRMED** — `05-02-SUMMARY.md`: "`MyWorld.DownedInfernon` reads `true` (confirmed via BossChecklist showing Infernon as downed)" — the only boss in the project with an explicit tracker-UI confirmation on record | **Complete** — pipeline + WorldGen tile-ring replay + SpiritMod-disabled JIT safety all live-verified (05-02-SUMMARY.md, 2026-08-13) |
| Thorn | Redemption | Phase 6 (06-02 code; 06-03 live-verify plan exists but **not executed**) | `Redemption.Globals.RedeBossDowned.downedThorn` (direct public-static-field write) | Default `BossArenaSubworld` (no Zone dependency found; not routed) | **Not Yet Confirmed** — `06-03-PLAN.md` Task 1 step 9 explicitly instructs checking via Boss Checklist tracker UI, but this checkpoint has not run | **Planned-not-executed** — `dotnet build` passes (06-02-SUMMARY.md), but no live subworld kill/carry/apply cycle has been run; `06-03-SUMMARY.md` does not exist, `check.md` checklist is entirely unchecked |
| Astrageldon | CatalystMod | Phase 6 (06-02 code; 06-03 live-verify plan exists but **not executed**) | `CatalystMod.WorldDefeats.downedAstrageldon` (direct public-static-field write, non-`-1` `gameEventId`) | Default `BossArenaSubworld` — **note:** `06-02-SUMMARY.md` explicitly states no `BossArenaRoutingRegistry.Register<T>()` call was added ("confirmed via full decompiled-source read to have no `player.Zone*`/`CheckActive`-override biome dependency"), even though `09-ALTAR-BIOME-REFERENCE.md` had wiki-thematically assigned Astrageldon to the Astral altar. This predates Phase 10's D-01 "wiki-thematic assignment always applies" principle — flag as a possible inconsistency the planner/user may want to reconcile, not a defect | **Not Yet Confirmed** — same `06-03-PLAN.md` Task 1 gap as Thorn | **Planned-not-executed** — same as Thorn. Additionally has an unverified Moon-Lord-lockout gate (`SummonItemRegistry.canSummon` delegate, Phase 6 Task 3) and a new-this-session `check.md` §3 item (lockout block/unblock case) that also has not been live-tested |
| Goblin Chariot | ContinentOfJourney (Homeward Journey) | Phase 7 — **researched only, not planned or executed** | `ContinentOfJourney.DownedBossSystem.downedGoblinChariot` (direct public-static-field write) | Default `BossArenaSubworld` (no wiki-stated biome, no Zone dependency) | **Unknown** — Homeward Journey ships its own `CoJ_BossChecklist.cs` integration reading the same flag, very likely auto-recognized, but explicitly flagged as unverified (07-RESEARCH.md Open Question 2) | **BLOCKED — Not yet planned.** Only `07-CONTEXT.md`/`07-RESEARCH.md`/`07-DISCUSSION-LOG.md`/`07-VALIDATION.md` exist; **no `07-01-PLAN.md` or any `PLAN.md` exists**, confirmed via directory listing and `git log` (no `feat(07-...)` commits, only `docs(07/phase-07)` research/validation commits). `Integrations/HomewardJourneyIntegration.cs` does not exist in the codebase yet — this boss is not registered at all |
| Providence | Calamity | Phase 10 — **researched only, not planned or executed** | `CalamityMod.DownedBossSystem.downedProvidence` | `BossArenaHallowSubworld` (recommended; thematic only — biome only affects AI's cosmetic `biomeType` branch) | Unknown (not registered) | **BLOCKED — Phase 10 not planned.** Register only when `!ModLoader.HasMod("InfernumMode")` (D-02) |
| Profaned Guardians | Calamity | Phase 10 (not planned) | `CalamityMod.DownedBossSystem.downedGuardians` (only `ProfanedGuardianCommander.OnKill()` sets it; `Defender`/`Healer` have no `OnKill` override) | `BossArenaHallowSubworld` (thematic only) | Unknown (not registered) | **BLOCKED.** Register only when `!ModLoader.HasMod("InfernumMode")` (D-02) |
| Ceaseless Void | Calamity | Phase 10 (not planned) | `CalamityMod.DownedBossSystem.downedCeaselessVoid` | Default `BossArenaSubworld` — 10-RESEARCH.md confirmed zero `ZoneDungeon` AI dependency, no Dungeon subworld rebuild needed | Unknown (not registered) | **BLOCKED.** Register only when `!ModLoader.HasMod("InfernumMode")` (D-02). Summon item (`MarkofProvidence`) is polymorphic — shared with Signus/Storm Weaver, requires the new `SummonItemRegistry.RegisterPolymorphic()` capability (not yet built) |
| The Old Duke | Calamity (+ InfernumMode-added item) | Phase 10 (not planned) | `CalamityMod.DownedBossSystem.downedBoomerDuke` — **not** `downedOldDuke` (naming trap, confirmed via decompile) | Default `BossArenaSubworld` — 10-RESEARCH.md confirmed zero AI-level Sulphurous Sea dependency (item-gate/wiki-flavor only), no Sulphurous subworld rebuild needed | Unknown (not registered) | **BLOCKED.** Register ONLY when `ModLoader.HasMod("InfernumMode")` (D-02) — without Infernum's `Bloodworm Platter` item, no summon item exists at all. `InfernumMode.dll` has not yet been extracted to `Libs/` (10-RESEARCH.md Environment Availability — a Wave 0 task for Phase 10, not Phase 8) |
| Signus | Calamity | Phase 10 (not planned) | `CalamityMod.DownedBossSystem.downedSignus` | `BossArenaUnderworldSubworld` (thematic only) | Unknown (not registered) | **BLOCKED.** Unconditional (unaffected by Infernum). Polymorphic `MarkofProvidence` item, same as Ceaseless Void |
| Storm Weaver | Calamity | Phase 10 (not planned) | `CalamityMod.DownedBossSystem.downedStormWeaver` | `BossArenaSpaceSubworld` (thematic only) | Unknown (not registered) | **BLOCKED.** Unconditional. Polymorphic `MarkofProvidence` item, same as Ceaseless Void/Signus |
| Astrum Deus | Calamity | Phase 10 (not planned) | `CalamityMod.DownedBossSystem.downedAstrumDeus` | `BossArenaAstralSubworld` (thematic only) | Unknown (not registered) | **BLOCKED.** Unconditional registration; when `HasMod("InfernumMode")`, additionally requires the not-yet-built forced-night utility (D-04) |
| Astrum Aureus | Calamity | Phase 10 (not planned) | `CalamityMod.DownedBossSystem.downedAstrumAureus` | `BossArenaAstralSubworld` (thematic only) | Unknown (not registered) | **BLOCKED.** Unconditional; same Infernum-conditional forced-night requirement as Astrum Deus. WorldGen side effect (`PlaceAstralMeteor()`) is dispatched on a background thread — replay pattern must match exactly (10-RESEARCH.md Pitfall 4) |
| Dragonfolly | Calamity | Phase 10 (not planned) | `CalamityMod.DownedBossSystem.downedDragonfolly` | `BossArenaJungleSubworld` — **functionally required**, not just thematic: confirmed grace-period-then-despawn timer on leaving `ZoneJungle` | Unknown (not registered) | **BLOCKED.** Unconditional |
| Devourer of Gods | Calamity | Phase 10 (not planned) | `CalamityMod.DownedBossSystem.downedDoG` | Default `BossArenaSubworld` (plain arena) | Unknown (not registered) | **BLOCKED.** Unconditional |
| Yharon | Calamity | Phase 10 (not planned) | `CalamityMod.DownedBossSystem.downedYharon` | Default `BossArenaSubworld` (plain arena) | Unknown (not registered) | **BLOCKED.** Unconditional |
| Supreme Witch, Calamitas | Calamity | Phase 10 (not planned) | `CalamityMod.DownedBossSystem.downedCalamitas` | Default `BossArenaSubworld` (plain arena — Altar of the Accursed furniture, no biome subworld) | Unknown (not registered) | **BLOCKED.** Unconditional (confirmed unchanged by Infernum) |
| Ancient Avian | Spirit | Phase 10 (not planned) | Generic `SpiritMod.NPCs.BossDownedTracker.OnKill` reflection write (Phase 5 pattern generalized, Architecture Pattern 3) | `BossArenaSpaceSubworld` (thematic only) | Unknown (not registered) | **BLOCKED** |
| Scarabeus | Spirit | Phase 10 (not planned) | Generic `BossDownedTracker` reflection write | `BossArenaDesertSubworld` — **functionally motivated**: 1/3 damage-scaling penalty both directions outside `ZoneDesert` (not a despawn, a balance issue) | Unknown (not registered) | **BLOCKED** |
| Vinewrath Bane | Spirit | Phase 10 (not planned) | Generic `BossDownedTracker` reflection write | `BossArenaBriarSubworld` (thematic only) | Unknown (not registered) | **BLOCKED** |
| Moon Jelly Wizard | Spirit | Phase 10 (not planned) | Generic `BossDownedTracker` reflection write | Default `BossArenaSubworld` + **forced night required** — confirmed AI-level despawn-on-`Main.dayTime` check, not just item-level | Unknown (not registered) | **BLOCKED.** Requires the not-yet-built forced-night utility (D-04); persistence-for-full-fight-duration is an explicit open question (10-RESEARCH.md Pitfall 6/Open Question 3) |
| Dusking | Spirit | Phase 10 (not planned) | Generic `BossDownedTracker` reflection write | Default `BossArenaSubworld` + **forced night required** — same despawn mechanism as Moon Jelly Wizard | Unknown (not registered) | **BLOCKED.** Same forced-night dependency and open persistence question as Moon Jelly Wizard |
| Atlas | Spirit | Phase 10 (not planned) | Generic `BossDownedTracker` reflection write | Default `BossArenaSubworld` (plain arena) | Unknown (not registered) | **BLOCKED** |

**Permanently excluded, not part of this roster (do not add to any Phase 8 checklist):** Exo Mechs (Calamity — no `Item.type` exists, real trigger is a placeable Codebreaker tile+UI) and Starplate Voyager (Spirit — real trigger is a scripted ambient-tile `Event`, not an item). Both excluded from v1 scope entirely per the Phase 10 planning decision recorded in `PROJECT.md` ("앞으로도 제외" — applies going forward, not just Phase 10). NoxusBoss (all bosses) similarly excluded entirely, not researched, not part of any phase's scope.

**Total: 24 bosses in v1 scope.** 3 fully complete (pipeline + own side effects live-verified; Boss-Checklist gap open on 2 of the 3). 2 code-registered but live-verification-pending (blocked on `06-03`). 1 not yet registered at all (blocked on Phase 7 planning+execution). 18 not yet registered at all (blocked on Phase 10 planning+execution).

## Boss Checklist Recognition Status Re-Verification

Per this task's explicit instruction, the two bosses CONTEXT.md flagged as uncertain were re-checked against their **actual `SUMMARY.md` files**, not CONTEXT.md's secondhand summary:

- **King Slime (`03-03-SUMMARY.md`):** Confirmed accurate. The 6-step live test log (Steps 1-6) checks the drop gate, cross-world `BossCoreItem` survival, the "Boss credential applied" chat message, `NPC.downedSlimeKing == true`, and idempotent re-use with a distinct "already defeated" message. No step mentions Boss Checklist, BossChecklist UI, or any external tracker mod at all. CONTEXT.md's characterization ("allowed an alternative confirmation method") is accurate — the alternative was internal-flag-plus-chat-message, not a tracker mod.
- **Hive Mind (`04-02-SUMMARY.md`):** Confirmed accurate. Task 1's live-verification evidence (reused from the `hivemind-zonecorrupt-despawn-corruption-subworld` debug session) confirms the "Boss credential applied" message, the Calamity Sky Ore broadcast, and a real Aerialite Ore tile conversion — again, no Boss Checklist UI check anywhere in this summary. CONTEXT.md's characterization ("confirmed its own side effects but not Boss Checklist recognition") is accurate.

Both re-verifications match CONTEXT.md's claims exactly — no correction needed, but confirmed independently rather than trusted at face value, per this task's instruction.

## Local Boss Checklist (JavidPack/BossChecklist) Installation Status

Checked this session two ways:

1. **`Mods\enabled.json`** (`C:\Users\chang\Documents\My Games\Terraria\tModLoader\Mods\enabled.json`, read directly this session): lists `["CalamityModMusic", "SubworldLibrary", "CheatSheet", "SpiritMod", "BossChecklist", "BossArenaSubWorld", "CalamityMod"]`. **BossChecklist is present and enabled.** This is consistent with three independent prior phases' research (`06-RESEARCH.md`, `07-RESEARCH.md`, `09-RESEARCH.md`), all of which read the same file and found the same list, and is corroborated by Infernon's own live Boss Checklist confirmation in Phase 5 (which required the mod to actually be running).
2. **Directory listing of the same `Mods\` folder** (`Get-ChildItem`, this session): only shows `2025.12CalamityModMusic.tmod`, `2026.6CalamityMod.tmod`, two `agent-*.tmod` files, and `BossArenaSubWorld.tmod` as physical `.tmod` files — **no `SpiritMod.tmod` or `BossChecklist.tmod` file is physically present in this listing**, despite both being listed as enabled.

**This is a real discrepancy, flagged honestly rather than resolved by assumption:** either (a) tModLoader's Workshop-mod storage/sync mechanism doesn't always leave a `.tmod` copy directly in this folder (possible, not confirmed), or (b) the environment this research session ran in does not have full/current access to the same `Mods\` folder the user actually plays from (the two unfamiliar `agent-*.tmod` entries are consistent with a distinct/sandboxed folder), or (c) the files were removed since Phase 5's successful live BossChecklist test and would need re-subscribing. Given Phase 5's own live, in-game confirmation ("confirmed via BossChecklist showing Infernon as downed") is real, first-party evidence that BossChecklist was running and working as recently as 2026-08-13, **treat BossChecklist's install status as MEDIUM confidence "should already be present," not HIGH confidence "physically verified this session."** Recommend the very first live checkpoint task in Phase 8's first plan include a trivial "confirm Boss Checklist is enabled and its tracker UI opens" sanity check before relying on it for every subsequent boss.

## Standard Stack

No new libraries or tooling. This phase performs zero new code writes beyond what Phases 6/7/10 already produce; its own artifacts are manual checklist documents (mirroring `06-redemption-catalystmod-integration/check.md`) and `SUMMARY.md` records of live-test results.

## Architecture Patterns

Not applicable in the usual sense — Phase 8 does not add new architecture. The one relevant pattern is procedural: this project's established live-verification-checklist format (`check.md`, Korean-language, numbered steps grouped by boss, ending in explicit pass/fail resume-signals matching a `PLAN.md`'s `<resume-signal>` blocks) is the correct template to reuse for every wave below, per Phase 6's precedent.

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---|---|---|---|
| Per-boss live-verification checklist format | A new checklist style/format for Phase 8 | `06-redemption-catalystmod-integration/check.md`'s exact format (Korean headers, numbered per-boss steps, explicit pass/fail resume-signal mapping back to plan tasks) | Already proven, already the format the user is used to reading and responding to |
| Tracking which bosses are "done" vs "blocked" | A new tracking mechanism/spreadsheet | This document's Master Boss Roster table, kept as the single source of truth and re-synced at Phase 8 planning/execution time if Phase 6/7/10's status changes | Avoids the planner re-deriving this table from scratch by re-reading 7+ phase directories, which is exactly what this research document exists to prevent |

**Key insight:** The actual risk in Phase 8 is not technical (no new mod-API work exists) — it's **sequencing**. Writing Phase 8 tasks against bosses that don't exist in the codebase yet (Goblin Chariot, all 18 Phase 10 bosses) would produce unexecutable plan tasks. The single most valuable output of this research is the explicit, session-verified (not assumed) execution-status column above.

## Common Pitfalls

### Pitfall 1: Assuming a phase's `RESEARCH.md`/`CONTEXT.md` existence means its code is registered
**What goes wrong:** Treating Phase 7 or Phase 10 as "ready to verify" because their research is thorough and detailed.
**Why it happens:** Both `07-RESEARCH.md` and `10-RESEARCH.md` are exhaustive, decompile-sourced documents that read as if implementation is imminent or complete — but neither phase has a single `PLAN.md`, let alone a `SUMMARY.md`.
**How to avoid:** Always check for `*-PLAN.md`/`*-SUMMARY.md` file existence and recent `git log` `feat(...)` commits, not just `RESEARCH.md`/`CONTEXT.md` presence, before scheduling a live-verification task against a specific boss. This document already did that check for all 24 bosses (see Master Boss Roster).
**Warning signs:** A phase directory with `RESEARCH.md`, `CONTEXT.md`, `DISCUSSION-LOG.md`, `VALIDATION.md` but no `01-PLAN.md`.

### Pitfall 2: Treating Thorn/Astrageldon as "the same situation" as King Slime/Hive Mind/Infernon
**What goes wrong:** Grouping all 5 pre-Phase-7 bosses into one "already done" wave.
**Why it happens:** All 5 have registration code merged and building cleanly.
**How to avoid:** Only King Slime, Hive Mind, and Infernon have completed live-verification `SUMMARY.md`s. Thorn and Astrageldon are code-complete but zero live checkpoints have run (`06-03-SUMMARY.md` does not exist; `check.md`'s checkboxes are unchecked). These belong in their own gated wave, not the "ready now" wave.
**Warning signs:** Planning a "close BossChecklist gap" task for Thorn/Astrageldon without first running `06-03`'s full 16-step Part A-D checkpoint.

### Pitfall 3: Missing the Astrageldon routing inconsistency
**What goes wrong:** Assuming Astrageldon is already routed to `BossArenaAstralSubworld` because `09-ALTAR-BIOME-REFERENCE.md` (Phase 9 reference research) assigned it there thematically.
**Why it happens:** The reference doc predates Phase 6's actual implementation. Phase 6's own decompiled-source research concluded Astrageldon has no Zone dependency and `06-02-SUMMARY.md` explicitly records that no `BossArenaRoutingRegistry.Register<T>()` call was added for it — it uses the plain default arena, not Astral.
**How to avoid:** When Phase 8 verifies Astrageldon, confirm it actually spawns/behaves correctly in the **default** arena (not Astral) — do not write a checkpoint step that expects an Astral-biome arena for this boss, and flag this discrepancy to the user/planner as worth a conscious decision (leave as-is, since Phase 10's "wiki-thematic assignment always applies" principle (D-01) came later and wasn't applied retroactively to Phase 6) rather than silently treating it as a bug to fix mid-Phase-8.
**Warning signs:** A Phase 8 task instructing the tester to enter the Astral subworld to test Astrageldon.

### Pitfall 4: Re-running Boss Checklist checks that are already satisfied
**What goes wrong:** Scheduling a full live re-verification of Infernon's Boss Checklist recognition, duplicating Phase 5's already-confirmed result.
**Why it happens:** VERIFY-03's Success Criterion 2 lists Infernon among bosses needing confirmation, but only because it's listing the full roster for completeness — Infernon's own row already says "only Infernon (Phase 5) did" confirm it.
**How to avoid:** Phase 8's first wave should **record/cite** Infernon's existing Phase 5 confirmation rather than re-test it from scratch, unless the user specifically wants a fresh spot-check (e.g. after other changes might have affected it). Spend the live-testing budget on the two bosses (King Slime, Hive Mind) that never got a Boss Checklist check at all.
**Warning signs:** A Phase 8 task asking the user to re-kill Infernon.

## Environment Availability

| Dependency | Required By | Available | Version | Fallback |
|---|---|---|---|---|
| Boss Checklist (JavidPack/BossChecklist) | VERIFY-03, every live checkpoint in every wave | **Config says yes** (`Mods\enabled.json` lists it, confirmed this session), but the `.tmod` file was not found in this session's `Mods\` directory listing — see "Local Boss Checklist Installation Status" above for the full discrepancy discussion | Unknown (not confirmed this session) | If missing at Phase 8 execution time: re-subscribe via Steam Workshop/Mod Browser, same low-effort fallback pattern already used for Redemption/CatalystMod/Homeward Journey in Phases 6/7 |
| Redemption + CatalystMod, enabled locally | Thorn/Astrageldon live checkpoints (Wave 2) | Not currently in `enabled.json` (only `CalamityModMusic, SubworldLibrary, CheatSheet, SpiritMod, BossChecklist, BossArenaSubWorld, CalamityMod` present) | — | Must be re-subscribed/enabled before Wave 2 can run — this is `06-03-PLAN.md`'s own Task 1 Part A, already documented there, not new to Phase 8 |
| Homeward Journey (`ContinentOfJourney`), enabled locally | Goblin Chariot checkpoint (Wave 3) | Not currently in `enabled.json` | — | Must be re-subscribed/enabled; also blocked on Phase 7 not being planned/executed at all yet (see Wave 3 gating) |
| InfernumMode | Providence/Guardians/Ceaseless-Void/Old-Duke conditional-gating checkpoints, forced-night-for-Astrum-bosses checkpoints (Wave 4+) | Confirmed present in `ModReader/InfernumMode/build.txt` per `10-RESEARCH.md` (source-extraction cache, not necessarily the same as being currently enabled in-game) | 2.0.1.35 | Verify actually enabled in `Mods\enabled.json` at Phase 8 execution time, and verify a **disabled** state too — Wave 4's polymorphic-item/conditional-gating checkpoints explicitly require testing both with and without InfernumMode active |

**Missing dependencies with no fallback:** None identified — every gap above has a documented, low-effort fallback (re-subscribe/enable) already precedented in Phases 6/7.

**Missing dependencies with fallback:** Boss Checklist's physical `.tmod` presence (unconfirmed this session, config says enabled); Redemption/CatalystMod/Homeward Journey/InfernumMod-enabled-state (all need confirming/toggling at the start of their respective waves).

## Recommended Plan Structure

Do not write one flat plan covering all 24 bosses — the dependency chain makes most of the roster unexecutable today. Recommend a wave structure keyed directly to the execution-status findings above, so later waves can simply be re-scheduled (not re-researched) once their blocking phase lands:

**Wave 1 — executable immediately, no phase dependency.**
Close the Boss-Checklist-recognition gap (VERIFY-03) for the 3 bosses whose pipeline is already fully live-verified:
- King Slime: live re-check via Boss Checklist's tracker UI specifically (internal flag already confirmed in Phase 3; only the tracker-recognition step is new)
- Hive Mind: same — live re-check via Boss Checklist UI specifically (side effects already confirmed in Phase 4)
- Infernon: **record/cite** Phase 5's existing confirmation; only re-test if the user wants a fresh spot-check (Pitfall 4)
- Include the one-time "confirm Boss Checklist is actually running and its UI opens" sanity check here first (Environment Availability discrepancy above), since every later wave depends on it

**Wave 2 — gated on Phase 6's `06-03` plan executing.**
Thorn and Astrageldon full live verification (pipeline + side effects + Boss Checklist recognition + the new-this-session Moon-Lord-lockout check in `check.md` §3). At Phase 8 execution time, re-check whether `06-03-SUMMARY.md` already exists — if Phase 6 finished it independently before Phase 8 reaches this wave, this becomes a citation/record step instead of a live-test step (same logic as Infernon in Wave 1). If not, this wave IS effectively `06-03`'s own checkpoint, executed under Phase 8's umbrella — coordinate with whoever owns closing out Phase 6 to avoid duplicate live-testing.

**Wave 3 — gated on Phase 7 planning AND execution.**
Goblin Chariot. Cannot be scheduled as an executable task until `/gsd:plan-phase 7` produces a `PLAN.md` and it executes (registers `Integrations/HomewardJourneyIntegration.cs`). Once it exists: verify pipeline + fold in the `CoJ_BossChecklist.cs` auto-recognition check (07-RESEARCH.md Open Question 2) in the same checkpoint, per CONTEXT.md's Claude's-Discretion recommendation.

**Wave 4+ — gated on Phase 10 planning AND execution (largest wave, itself likely needs sub-waves).**
Once Phase 10 registers its 18-boss roster, batch Phase 8's live checkpoints by **destination arena/subworld** rather than by mod — this minimizes repeated subworld entry/exit overhead for the tester and mirrors how the bosses will actually be reachable in-game:
- Hallow batch: Providence, Profaned Guardians (test with InfernumMode disabled — they don't register at all otherwise)
- Underworld batch: Signus (Infernon already closed in Wave 1)
- Astral batch: Astrum Deus, Astrum Aureus (each tested twice — with and without InfernumMode, to cover the conditional forced-night requirement)
- Jungle batch: Dragonfolly (functional despawn-prevention check — confirm no timer-based despawn in the routed arena)
- Space batch: Storm Weaver, Ancient Avian
- Desert batch: Scarabeus (functional damage-scaling check — confirm no 1/3 penalty inside the routed arena)
- Briar batch: Vinewrath Bane
- Default-arena batch: The Old Duke (InfernumMode-only), Devourer of Gods, Yharon, Supreme Witch Calamitas, Atlas
- Forced-night batch: Moon Jelly Wizard, Dusking — recommend at least one **long-duration** fight here specifically to test the open forced-night-persistence question (10-RESEARCH.md Pitfall 6/Open Question 3), not just a quick kill
- Polymorphic-item + Infernum-matrix batch: verify `MarkofProvidence` resolves to the correct boss depending on player Zone at click-time (all 3 reachable targets), and verify the full D-02 conditional-registration matrix (Providence/Guardians/Ceaseless-Void NOT registered with InfernumMode present; Old Duke ONLY registered with InfernumMode present) — this is as much an architecture-correctness check as a per-boss one, keep it as its own checkpoint rather than folding into individual boss batches
- Close with a combined Boss-Checklist-recognition sweep across all newly-registered bosses (can likely piggyback on each batch's own checkpoint rather than a separate pass, planner's discretion)

**Sequencing note for the orchestrator:** ROADMAP.md already states Phase 8 "Depends on: Phase 7 (and, per the expanded scope below, effectively also Phase 9/10...)". Waves 3 and 4 above are not schedulable as concrete dated tasks yet — write them as explicitly-gated plan stubs (or defer creating their `PLAN.md`s entirely until their blocking phase's `SUMMARY.md`s exist), consistent with CONTEXT.md's own instruction that "context-capture can proceed regardless" of this dependency chain, but execution cannot.

## Validation Architecture

### Test Framework
| Property | Value |
|---|---|
| Framework | None — tModLoader mod, no automated in-game test harness exists or is planned (established since Phase 1) |
| Config file | none |
| Quick run command | `dotnet build BossArenaSubWorld.csproj` — smoke-test gate only; Phase 8 writes no new code, so this simply re-confirms nothing was accidentally broken by whatever Phase 6/7/10 work landed most recently before each wave |
| Full suite command | N/A — "full verification" IS the manual live in-game checklist per wave, described above |

### Phase Requirements → Test Map
| Req ID | Behavior | Test Type | Automated Command | File Exists? |
|---|---|---|---|---|
| VERIFY-01 | Full subworld-kill → carrier-item → main-world-apply pipeline works for every registered boss | manual-only, live in-game, per-boss (see Recommended Plan Structure waves) | `dotnet build` (smoke gate only, does not exercise runtime behavior) | N/A — manual checklist per boss/wave, `check.md` format |
| VERIFY-03 | Applied flag recognized by Boss Checklist for every boss | manual-only, live in-game, per-boss | none | N/A — manual checklist per boss/wave |

### Sampling Rate
- **Per wave kickoff:** `dotnet build BossArenaSubWorld.csproj` (confirms the codebase Wave N depends on — e.g. Phase 6/7/10's registration code — actually compiles before spending live-test time on it)
- **Per boss checkpoint:** the manual live in-game steps described in each wave above, following the `check.md` numbered-checklist / explicit resume-signal format
- **Phase gate:** all 24 bosses' rows in the Master Boss Roster table show "Complete" execution status AND "Confirmed" Boss-Checklist status before `/gsd:verify-work` — given the current blocked state, this phase gate cannot close until Phase 6 (`06-03`), Phase 7, and Phase 10 have all executed

### Wave 0 Gaps
- None in the traditional "missing test file" sense — this project has never had automated tests and won't start here. The "Wave 0 gap" for Phase 8 specifically is **Phase 7 and Phase 10 not being planned yet**, which is a phase-sequencing gap, not a test-infrastructure gap. Recommend the orchestrator treat Waves 3-4 above as blocked-plan stubs rather than attempting to force Wave-0-style automated coverage where none can exist.

## Sources

### Primary (HIGH confidence — this session's own direct file reads / git log / live config read)
- `git log --oneline -15` (this session) — confirms no `feat`/execution commits exist for `06-03`, Phase 7, or Phase 10 beyond `docs(...)` research/context/validation commits
- Directory listings of `.planning/phases/06-*`, `07-*`, `10-*` (this session) — confirms `06-03-PLAN.md` exists with no `06-03-SUMMARY.md`; Phase 7 and Phase 10 have `CONTEXT.md`/`RESEARCH.md`/`VALIDATION.md`/`DISCUSSION-LOG.md` but no `PLAN.md` at all
- `.planning/phases/03-.../03-03-SUMMARY.md`, `04-.../04-02-SUMMARY.md`, `05-.../05-02-SUMMARY.md`, `06-.../06-01-SUMMARY.md`, `06-.../06-02-SUMMARY.md` — read in full this session, primary source for the Master Boss Roster's "Execution Status" and "Boss-Checklist Status" columns
- `.planning/phases/06-.../06-03-PLAN.md`, `.planning/phases/06-.../check.md` — read in full this session, confirms Thorn/Astrageldon's live-verification steps are specified but unexecuted (all checkboxes unchecked)
- `.planning/phases/07-.../07-RESEARCH.md`, `07-VALIDATION.md` — read in full this session, confirms Goblin Chariot's exact API/routing and its own open BossChecklist question
- `.planning/phases/09-.../09-ALTAR-BIOME-REFERENCE.md`, `.planning/phases/10-.../10-CONTEXT.md`, `10-RESEARCH.md` — read in full this session, primary source for the Phase 10 roster's 18 rows, Infernum-gating logic, and the two new architecture pieces (polymorphic `SummonItemRegistry`, `ForcedTimeUtility`)
- `C:\Users\chang\Documents\My Games\Terraria\tModLoader\Mods\enabled.json` — read directly this session, confirms BossChecklist/SpiritMod/CalamityMod/SubworldLibrary/CheatSheet/BossArenaSubWorld/CalamityModMusic currently listed as enabled
- `Get-ChildItem` directory listing of the same `Mods\` folder — read directly this session, surfaced the `.tmod`-file-vs-`enabled.json` discrepancy documented above
- `.planning/PROJECT.md`, `.planning/REQUIREMENTS.md`, `.planning/STATE.md`, `.planning/ROADMAP.md` (Phase 8 section) — read in full this session

### Secondary (MEDIUM confidence)
- None required — every claim in this document traces to a primary source read this session.

### Tertiary (LOW confidence / flagged for validation)
- Boss Checklist's physical `.tmod` presence — genuinely unresolved this session (config says enabled, file listing doesn't show it); flagged explicitly above, not asserted either way
- Whether `Main.time` persists/advances during a subworld fight (affects Moon Jelly Wizard/Dusking's forced-night requirement) — inherited as an open question from `10-RESEARCH.md`, not resolved here (Phase 10's own scope, Phase 8 just needs to test for it live once reachable)

## Metadata

**Confidence breakdown:**
- Master roster / execution status: HIGH — every row sourced from a direct file/git read this session, not inference
- Boss Checklist recognition re-verification: HIGH — both flagged bosses' actual SUMMARY.md files were re-read and confirm CONTEXT.md's characterization exactly
- Local Boss Checklist install status: MEDIUM — genuine discrepancy between config (`enabled.json`) and file listing, honestly flagged rather than resolved by assumption
- Recommended plan structure: HIGH — directly derived from the execution-status findings, not a judgment call

**Research date:** 2026-08-14
**Valid until:** Re-verify the Execution Status column (and Wave gating) immediately before Phase 8 planning/execution actually begins, since Phase 6 (`06-03`), Phase 7, and Phase 10 are all independently in-flight and any of them completing invalidates parts of this table (in the direction of "more bosses become executable," not a correctness risk — but stale gating could cause the planner to under-schedule waves that are actually ready).
