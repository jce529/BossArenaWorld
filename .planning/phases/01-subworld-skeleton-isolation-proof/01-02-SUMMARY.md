---
phase: 01-subworld-skeleton-isolation-proof
plan: 02
subsystem: worldgen
tags: [tmodloader, subworldlibrary, genpass, worldgen, subworld]

# Dependency graph
requires:
  - phase: 01-01
    provides: global.json SDK pin, build.txt modReferences=SubworldLibrary declaration, VERIFY-02 world-backup guidance doc
provides:
  - "Subworlds/FlatStonePlatformPass.cs -- the single GenPass that fills a flat stone platform and sets spawn, satisfying SUBW-05 by construction"
  - "Subworlds/BossArenaSubworld.cs -- the Subworld subclass wiring Width/Height/Tasks/ShouldSave/NoPlayerSaving"
  - "A working compile-time reference path (Libs/SubworldLibrary.dll, gitignored) that lets plain dotnet build/msbuild resolve SubworldLibrary types, alongside the runtime modReferences declaration"
affects: [01-03, 01-04]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "GenPass subclasses live under Subworlds/ and use the 1.4.4 data-oriented tile accessor (Tile.HasTile/TileType), not the deprecated active()/type API"
    - "Subworld subclasses set ShouldSave=false as the actual isolation mechanism (regenerate-from-Tasks every entry) and must never override NoPlayerSaving=true"
    - "Compile-time cross-mod references: extract the referenced mod's assembly from its installed .tmod into a gitignored Libs/ folder and add a conditional csproj Reference (Private=false); build.txt's modReferences/weakReferences remains the authoritative runtime declaration"

key-files:
  created:
    - Subworlds/FlatStonePlatformPass.cs
    - Subworlds/BossArenaSubworld.cs
  modified:
    - BossArenaSubWorld.csproj
    - .gitignore

key-decisions:
  - "GameConfiguration (GenPass.ApplyPass's second parameter) lives in namespace Terraria.IO, not Terraria.WorldBuilding as the research doc's code example implied -- confirmed via MetadataLoadContext reflection against the installed tModLoader.dll"
  - "Plain dotnet build/dotnet msbuild does not resolve build.txt's modReferences -- that mechanism is runtime-only (tModLoader's mod loader extracts referenced mods' assemblies from their .tmod at load time). Added a local compile-time-only Reference to a Libs/SubworldLibrary.dll extracted from the installed Workshop .tmod via TmodFile reflection, gitignored since it's not redistributable source"

patterns-established:
  - "Pattern 1: GenPass subclasses use Tile.HasTile/TileType (1.4.4 data-oriented API), never the 1.3-era active()/type API"
  - "Pattern 2: Extract-and-gitignore local reference DLLs for cross-mod compile-time type resolution when modReferences alone doesn't satisfy the outer dotnet build/msbuild compile step"

requirements-completed: [SUBW-05]

# Metrics
duration: 55min
completed: 2026-08-13
---

# Phase 1 Plan 02: Subworld Skeleton (GenPass + Subworld subclass) Summary

**Single-GenPass flat stone platform (10,000 wide x 800 tall subworld, 15-block-thick stone layer) wired into a `Subworld` subclass with `ShouldSave=false`/`NoPlayerSaving=false`, proving SUBW-05's zero-placed-content invariant by construction rather than convention.**

## Performance

- **Duration:** ~55 min
- **Started:** 2026-08-13T07:45:00Z (approx)
- **Completed:** 2026-08-13T08:40:00Z (approx)
- **Tasks:** 2
- **Files modified:** 4 (2 created for the plan's own scope, 2 modified to unblock the build)

## Accomplishments
- `FlatStonePlatformPass` GenPass: the only generation logic in the subworld -- a thin stone-only tile-fill loop plus explicit spawn-point placement, with no ore/structure/biome placement of any kind
- `BossArenaSubworld` Subworld subclass: `Width`/`Height`/`Tasks` (exactly one `GenPass`), `ShouldSave => false`, `NoPlayerSaving => false`
- Confirmed via clean `dotnet build BossArenaSubWorld.csproj` (exit code 0) that both files compile and the SubworldLibrary type dependency resolves correctly

## Task Commits

Each task was committed atomically:

1. **Task 1: Create the flat stone platform GenPass** - `a883e9c` (feat)
2. **Task 2: Create the BossArenaSubworld class** - `3916f57` (feat, includes the csproj/.gitignore reference-wiring fix needed to build it)

**Prerequisite unblock commit:** `e0bdad8` (chore -- see Deviations)

**Plan metadata:** (this commit)

## Files Created/Modified
- `Subworlds/FlatStonePlatformPass.cs` - GenPass that fills a 15-block-thick stone layer across the full subworld width and sets `Main.spawnTileX/Y` above the platform surface
- `Subworlds/BossArenaSubworld.cs` - `Subworld` subclass: `PlatformWidth=10000`, `WorldHeight=800`, `Tasks` = `[new FlatStonePlatformPass(...)]`, `ShouldSave=false`, `NoPlayerSaving=false`
- `BossArenaSubWorld.csproj` - added a conditional, non-copying (`Private=false`) `Reference` to `Libs\SubworldLibrary.dll` so the outer `dotnet build`/`msbuild` compile step can resolve `SubworldLibrary` types (see Deviations)
- `.gitignore` - added `Libs/` (local, non-redistributable, machine-extracted compile-time reference DLLs)

## Decisions Made
- `GameConfiguration` is in `Terraria.IO`, not `Terraria.WorldBuilding` -- confirmed by loading the installed `tModLoader.dll` via `System.Reflection.MetadataLoadContext` and inspecting `GenPass.ApplyPass`'s actual signature, since the research doc's code example didn't specify the namespace and the type wasn't resolvable in `Terraria.WorldBuilding`.
- Used mid-height (`Main.maxTilesY / 2`) platform placement and 15-block thickness, both explicitly left to Claude's discretion per 01-CONTEXT.md (D-07).

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Recreated 01-01's prerequisite build artifacts in this isolated parallel worktree**
- **Found during:** Task 1 setup (before any code was written)
- **Issue:** This plan (wave 1, `depends_on: ["01-01"]`) runs in an isolated git worktree for parallel execution. Git worktrees do not carry over untracked files, and the entire pre-existing mod scaffold (`BossArenaSubWorld.csproj`, `BossArenaSubWorld.cs`, `build.txt`, `Localization/`, `Properties/`, icons, description files, `.gitignore`) was untracked in the shared repo -- none of it existed in this worktree. Additionally, plan `01-01`'s own deliverables (`global.json` SDK pin, `build.txt`'s `modReferences = SubworldLibrary` line) were not present either, since `01-01` had not been committed at execution time (it appears to have been running concurrently in a sibling worktree).
- **Fix:** Copied the untracked base scaffold from the main checkout into this worktree, then recreated `global.json` (SDK pinned to 8.0.424, `rollForward: latestFeature`) and added `modReferences = SubworldLibrary` to `build.txt`, matching 01-01-PLAN.md's exact specified deliverables.
- **Files modified:** `.gitignore`, `BossArenaSubWorld.cs`, `BossArenaSubWorld.csproj`, `build.txt`, `description.txt`, `description_workshop.txt`, `icon.png`, `icon_small.png`, `global.json`, `Localization/*`, `Properties/*`
- **Verification:** `dotnet --version` resolved to `8.0.424`; baseline `dotnet build BossArenaSubWorld.csproj` succeeded before any of this plan's own files were added.
- **Committed in:** `e0bdad8` (separate prerequisite commit, before Task 1)
- **Note for orchestrator:** This worktree's `e0bdad8` commit content should match (or be reconcilable with) whatever `01-01`'s own execution produces when merged. If `01-01` was executed independently in the sibling worktree, expect a merge conflict on `global.json`/`build.txt`/scaffold files that should resolve cleanly since the content is derived from the same plan spec.

**2. [Rule 3 - Blocking] `modReferences` in build.txt is a runtime-only mechanism; added a compile-time Reference**
- **Found during:** Task 2 (`BossArenaSubworld.cs`, first line using `SubworldLibrary` types)
- **Issue:** `dotnet build`/`dotnet msbuild` failed with `CS0246: 'SubworldLibrary' type or namespace not found`. Investigation (reading `tMLMod.targets`) confirmed `modReferences` in `build.txt` is read only by tModLoader's own runtime mod loader/packaging step (`AfterTargets="Build"` `Exec` calling `dotnet tModLoader.dll -build ... -eac $(TargetPath)`), which in this case just packages the *already-compiled* dll rather than recompiling with its own reference resolution -- it never runs at all if the outer `CoreCompile` fails first. The outer `dotnet build`/`csc` step, which is what actually needs to resolve `SubworldLibrary` types, has no built-in mechanism to read `build.txt`.
- **Fix:** Used `System.Reflection` (`Assembly.LoadFrom` + reflection into `Terraria.ModLoader.Core.TmodFile`) to programmatically read the installed SubworldLibrary Workshop `.tmod` (`...\2785100219\2025.9\SubworldLibrary.tmod`) and extract its compiled `SubworldLibrary.dll` (62,976 bytes) to a local `Libs/SubworldLibrary.dll`. Added a conditional (`Exists(...)`), non-copying (`Private=false`) `<Reference>` in `BossArenaSubWorld.csproj` pointing at it. Added `Libs/` to `.gitignore` since the extracted DLL is third-party compiled content, not redistributable source -- each dev machine must extract its own copy from its installed SubworldLibrary Workshop mod. `build.txt`'s `modReferences = SubworldLibrary` remains the authoritative runtime dependency declaration; this Reference is purely a local compile-time convenience.
- **Files modified:** `BossArenaSubWorld.csproj`, `.gitignore`
- **Verification:** Clean `dotnet build BossArenaSubWorld.csproj` (after `rm -rf obj bin`) succeeded with exit code 0, `SubworldLibrary`/`GenPass` types resolved.
- **Committed in:** `3916f57` (part of Task 2 commit)

**3. [Process note, no code change] Stray sibling reflection-probe project directories transiently broke the build**
- **Found during:** Investigation between Task 1 and Task 2
- **Issue:** Two throwaway reflection-probe `.csproj` projects (used to determine `GameConfiguration`'s namespace and to extract `SubworldLibrary.dll`) were created as sibling directories under the project root. After deleting them, `dotnet build BossArenaSubWorld.csproj` intermittently failed with `CS0579` duplicate-assembly-attribute errors, because a lingering MSBuild/VBCSCompiler build-server process held file locks on the probe projects' own `obj/` folders, causing `rm -rf` to silently leave those `obj` folders behind. `BossArenaSubWorld.csproj`'s default `**/*.cs` glob picked up the orphaned `probe*/obj/**/*.AssemblyInfo.cs` files (nested `obj` folders outside the main project's own `obj/` are not covered by the SDK's default item excludes), causing duplicate assembly-level attributes.
- **Fix:** `dotnet build-server shutdown` before deleting probe directories, confirmed full removal via `find`, then a clean `rm -rf obj bin` + rebuild.
- **Files modified:** None (no probe files were ever committed; this is documented for the orchestrator/future executors as a known parallel-worktree development-environment gotcha, not a code deviation).

---

**Total deviations:** 2 auto-fixed (both Rule 3 - blocking issues), 1 process note.
**Impact on plan:** Both fixes were required to satisfy this plan's own acceptance criteria (`dotnet build BossArenaSubWorld.csproj` exits with code 0). No scope creep beyond what was necessary to build the two files this plan specifies. The `Libs/SubworldLibrary.dll` extraction is a local, gitignored, machine-specific convenience and does not change the mod's actual runtime dependency contract (`build.txt`'s `modReferences = SubworldLibrary`).

## Issues Encountered
See Deviations above -- all issues were build-environment/tooling issues encountered while satisfying this plan's own verification requirement, not issues with the plan's design.

## User Setup Required
None - no external service configuration required. Note for other machines: `Libs/SubworldLibrary.dll` is gitignored and must be (re-)extracted locally if not present -- it is derived from the installed SubworldLibrary Workshop `.tmod` and is not committed to the repo.

## Next Phase Readiness
- `Subworlds/BossArenaSubworld.cs` and `Subworlds/FlatStonePlatformPass.cs` exist, compile, and are ready for Plan 01-03 (debug entry/exit commands + biome-override hook) to reference `BossArenaSubworld` via `SubworldSystem.Enter<BossArenaSubworld>()`.
- The `modReferences`-vs-compile-time-Reference gap discovered here will recur for every future weak-reference content-mod integration (Phase 4+, Calamity/Spirit/etc.) -- worth flagging explicitly when Phase 4 is planned, since those mods aren't installed via a simple single-file Workshop `.tmod` extraction in all cases and may need a different resolution strategy (e.g. `[JITWhenModsEnabled]` reflection-only access, which sidesteps compile-time References entirely for genuinely optional dependencies).
- Orchestrator should verify/reconcile this worktree's prerequisite commit (`e0bdad8`) against plan `01-01`'s actual execution output when merging waves -- see Deviation 1's note.

---
*Phase: 01-subworld-skeleton-isolation-proof*
*Completed: 2026-08-13*

## Self-Check: PASSED

- FOUND: Subworlds/FlatStonePlatformPass.cs
- FOUND: Subworlds/BossArenaSubworld.cs
- FOUND commit: a883e9c
- FOUND commit: 3916f57
- FOUND commit: e0bdad8
