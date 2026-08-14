---
phase: 09-biome-dependent-subworld-coverage
plan: 05
subsystem: infra
tags: [tmodloader, subworldlibrary, calamitymod, spiritmod, jitwhenmodsenabled, debug-tooling, reflection]

# Dependency graph
requires:
  - phase: 09-01/02/03/04
    provides: 7 biome-variant Subworld/GenPass pairs (Hallow, Underworld, Jungle, Space, Desert, Astral, Briar)
provides:
  - Temporary chat command to enter any of the 7 new biome boss-arena subworlds by name
  - Temporary chat command to print the current player's vanilla/Calamity/Spirit biome Zone flags
affects: [09-06, 09-07]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "Extension methods on weak-referenced mod types must be called via their fully-qualified static class (e.g. CalamityMod.CalamityUtils.Calamity(player)) when avoiding a project-wide `using` directive"
    - "Reading a public static property declared on an `internal` mod type requires reflection (Type.GetType + PropertyInfo.GetValue), mirroring the existing FieldInfo-write pattern in Integrations/SpiritIntegration.cs, applied here to a read instead of a write"

key-files:
  created: [Debug/BiomeArenaDebugCommands.cs]
  modified: []

key-decisions:
  - "Fixed two compile-blocking bugs in the plan's illustrative code (Rule 3): player.Calamity() needed the CalamityMod namespace in scope, resolved by calling CalamityMod.CalamityUtils.Calamity(player) directly instead of adding a using directive; SpiritMod.Biomes.BiomeTileCounts is internal (confirmed via ilspycmd decompile), so its public InBriar property is read via cached reflection with try/catch, matching Integrations/SpiritIntegration.cs's established pattern for internal Spirit types"

patterns-established:
  - "When a plan's illustrative interface code references a mod API, verify it against the actual installed DLL via ilspycmd before treating it as ground truth -- research documents can describe a real member (InBriar exists and is correct) while the accessing syntax shown is still not directly compilable (BiomeTileCounts is internal)"

requirements-completed: [ARENA-01]

# Metrics
duration: ~15min
completed: 2026-08-14
---

# Phase 09 Plan 05: Biome-Arena Debug Commands Summary

**Temporary chat-command debug tool (`/bossarena-enterbiome`, `/bossarena-checkbiomeflags`) letting the tester enter any of the 7 new biome boss-arena subworlds and inspect vanilla/Calamity/Spirit Zone flags, without adding any permanent player-facing entry point.**

## Performance

- **Duration:** ~15 min
- **Completed:** 2026-08-14T03:11:11Z
- **Tasks:** 2
- **Files modified:** 1 (created)

## Accomplishments
- `BiomeArenaEnterCommand` routes `/bossarena-enterbiome <name>` to any of the 7 kept biome subworlds (hallow, underworld, jungle, space, desert, astral, briar) via `SubworldSystem.Enter<T>()`
- `BiomeArenaCheckFlagsCommand` prints vanilla Zone flags unconditionally and Calamity/Spirit flags only when the respective mod is installed, via named `[JITWhenModsEnabled]`-tagged methods (never inline lambdas), matching `Integrations/CalamityIntegration.cs`'s established discipline
- Confirmed, via `ilspycmd` decompile of the installed `Libs/CalamityMod.dll`/`Libs/SpiritMod.dll`, the exact real accessors needed for `ZoneAstral`/`ZoneSulphur` (public `CalamityPlayer` properties via the `CalamityMod.CalamityUtils.Calamity(this Player)` extension) and `InBriar` (a public static property on an `internal` `SpiritMod.Biomes.BiomeTileCounts` class, requiring reflection)

## Task Commits

Each task was committed atomically:

1. **Task 1: Create the biome-enter debug command** - `ae58968` (feat)
2. **Task 2: Add the biome-flag-check debug command to the same file** - `5a03230` (feat)

_Note: no separate plan-metadata commit was requested as part of these task commits; STATE.md/ROADMAP.md/SUMMARY.md are captured in the final docs commit below._

## Files Created/Modified
- `Debug/BiomeArenaDebugCommands.cs` - Two temporary `ModCommand` classes: `BiomeArenaEnterCommand` (enter any of the 7 new biome subworlds by name) and `BiomeArenaCheckFlagsCommand` (print vanilla/Calamity/Spirit Zone flags to chat). Explicitly documented in a file-header comment as temporary, to be deleted in Plan 07 (D-02).

## Decisions Made
- Copied `Libs/*.dll` (gitignored, per-worktree compile-time references) from the main repo checkout into this worktree before building -- known per-worktree setup step (see STATE.md Phase 02 decision), not a code gap.
- Kept the fully-qualified `CalamityMod.CalamityUtils.Calamity(player)` static-class-call form instead of adding a `using CalamityMod;` directive, to minimize the file's surface area and stay consistent with `Integrations/CalamityIntegration.cs`'s existing fully-qualified-reference style.
- Read `SpiritMod.Biomes.BiomeTileCounts.InBriar` via reflection (`Type.GetType` + `PropertyInfo.GetValue`, wrapped in try/catch + `Mod.Logger.Warn`) rather than skipping the Spirit flag print entirely, since the value itself (a public static property) is real and readable -- only direct compile-time access is blocked by the class's `internal` visibility.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] `player.Calamity()` did not compile as written**
- **Found during:** Task 2 (`dotnet build` after adding `BiomeArenaCheckFlagsCommand`)
- **Issue:** The plan's illustrative code called `player.Calamity().ZoneAstral` with no `using CalamityMod;` in scope. `CalamityMod.CalamityUtils.Calamity(this Player player)` (confirmed via `ilspycmd -t CalamityMod.CalamityUtils Libs/CalamityMod.dll`) is a real, public extension method, but C# extension-method resolution requires its declaring namespace (`CalamityMod`) to be imported via `using` for the fluent `player.Calamity()` syntax to resolve -- CS1061 otherwise.
- **Fix:** Called the static class directly: `CalamityMod.CalamityUtils.Calamity(player)`, avoiding the need for a new `using` directive while invoking the identical public extension method. Also confirmed `ZoneAstral`/`ZoneSulphur` are public properties on `CalamityMod.CalPlayer.CalamityPlayer : ModPlayer` (both public), so no further accessibility issue exists.
- **Files modified:** `Debug/BiomeArenaDebugCommands.cs`
- **Verification:** `dotnet build BossArenaSubWorld.csproj` exits 0
- **Committed in:** `5a03230` (Task 2 commit)

**2. [Rule 3 - Blocking] `SpiritMod.Biomes.BiomeTileCounts.InBriar` did not compile as written (CS0122)**
- **Found during:** Task 2 (`dotnet build` after adding `BiomeArenaCheckFlagsCommand`)
- **Issue:** Decompiling `Libs/SpiritMod.dll` (`ilspycmd -t SpiritMod.Biomes.BiomeTileCounts`) shows `internal class BiomeTileCounts : ModSystem` -- the class itself is `internal`, so even though `InBriar` is declared `public static`, C# caps effective external visibility at the containing type's accessibility, producing CS0122 at compile time. This is the identical "public member on an internal type" shape already documented for `SpiritMod.NPCs.BossDownedTracker` in `Integrations/SpiritIntegration.cs` (Phase 5, Pitfall A), just discovered independently for a different internal Spirit class.
- **Fix:** Read the property via reflection (`ModLoader.GetMod("SpiritMod").Code.GetType("SpiritMod.Biomes.BiomeTileCounts").GetProperty("InBriar", BindingFlags.Public | BindingFlags.Static).GetValue(null)`), wrapped in try/catch with `Mod.Logger.Warn` on failure and a chat-visible fallback message -- mirrors `SpiritIntegration.cs`'s established cached-reflection-with-try/catch discipline, applied to a read instead of a write.
- **Files modified:** `Debug/BiomeArenaDebugCommands.cs`
- **Verification:** `dotnet build BossArenaSubWorld.csproj` exits 0
- **Committed in:** `5a03230` (Task 2 commit)

---

**Total deviations:** 2 auto-fixed (both Rule 3 - blocking compile errors in the plan's illustrative code, discovered via `dotnet build` and resolved via `ilspycmd` decompile of the actual installed DLLs)
**Impact on plan:** Both fixes were required for the file to compile at all; no scope creep. The plan's two acceptance-criteria literal-text checks (`player.Calamity().ZoneAstral` and `SpiritMod.Biomes.BiomeTileCounts.InBriar` as exact substrings) no longer match verbatim, since the working code calls the same underlying members through compilable syntax (`CalamityMod.CalamityUtils.Calamity(player).ZoneAstral` and reflection into `BiomeTileCounts.InBriar`) -- the described chat-output behavior (printing `ZoneAstral`/`ZoneSulphur`/`Briar.InBriar` values) is fully preserved.

## Issues Encountered
- This worktree's `Libs/` directory (gitignored compile-time DLL references) was empty on first build attempt -- copied `CalamityMod.dll`/`SpiritMod.dll`/`SubworldLibrary.dll` from the main repo checkout, per the known per-worktree setup step already documented in STATE.md (Phase 02 decision), not a new gap.
- `ilspycmd` (already installed as a global dotnet tool, version 8.2.0.7535) was used to decompile the exact installed `Libs/CalamityMod.dll`/`Libs/SpiritMod.dll` and resolve both Rule 3 deviations above from ground truth rather than assumption.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness
- Both debug commands build cleanly and are ready for Plan 06's live in-game verification checkpoints (entering each of the 7 biomes and confirming the correct Zone flag reads true).
- Plan 07's Task 2 is responsible for deleting `Debug/BiomeArenaDebugCommands.cs` once those checkpoints pass, per D-02 (no new permanent player-facing entry point).
- No live in-game verification was performed as part of this plan (explicitly out of scope per the plan's own `<verification>` note -- "This plan does not require live in-game verification -- that is Plan 06's scope").

---
*Phase: 09-biome-dependent-subworld-coverage*
*Completed: 2026-08-14*

## Self-Check: PASSED

- FOUND: Debug/BiomeArenaDebugCommands.cs
- FOUND: ae58968 (Task 1 commit)
- FOUND: 5a03230 (Task 2 commit)
