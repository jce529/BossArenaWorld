---
phase: 09-biome-dependent-subworld-coverage
plan: 06
subsystem: infra
tags: [tmodloader, subworldlibrary, calamitymod, spiritmod, live-verification, scenemetrics, modbiome]

# Dependency graph
requires:
  - phase: 09-01/02/03/04
    provides: 7 biome-variant Subworld/GenPass pairs (Hallow, Underworld, Jungle, Space, Desert, Astral, Briar)
  - phase: 09-05
    provides: Temporary /bossarena-enterbiome and /bossarena-checkbiomeflags debug chat commands
provides:
  - Live-confirmed proof that all 7 biome Subworld/GenPass pairs actually satisfy their target Zone/Biome flag when entered (not just structurally correct on paper)
affects: [09-07]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "A height-only Zone flag (ZoneUnderworldHeight, ZoneSkyHeight) can be satisfied purely by platform Y-position with no themed tile fill required, distinct from vanilla tile-weighted flags (ZoneHallow/ZoneJungle/ZoneDesert) which require SceneMetrics-counted tile types, and modded ModBiome flags (ZoneAstral, Briar.InBriar) which require a mod's own tile-count-hook system"
    - "A thin cosmetic tile layer (e.g. 3 rows of real Sand) over a solid structurally-inert base (Sandstone) contributes the same SceneMetrics weight as a full-depth themed fill while avoiding falling-block recursion crashes -- established precedent from UnderworldPlatformPass, now confirmed necessary for Desert too"

key-files:
  created: []
  modified:
    - Subworlds/DesertPlatformPass.cs

key-decisions:
  - "Fixed Desert's platform mid-checkpoint (Rule 1 - bug): filling all 20 platform rows with falling TileID.Sand caused a native stack overflow via infinite WorldGen.SquareTileFrame/TileFrame/SpawnFallingBlockProjectile recursion during generation. Changed to 3 real-Sand rows over a solid Sandstone base (same SceneMetrics weight-1 contribution per row per 09-RESEARCH.md's decompiled table), mirroring UnderworldPlatformPass's existing thin-cosmetic-layer pattern. User re-tested live after the fix and confirmed no crash with ZoneDesert=True."
  - "ARENA-01 requirement is intentionally NOT marked complete by this plan despite being listed in this plan's (and 09-07's) frontmatter requirements field -- both this plan's and 09-07's own <objective>/<success_criteria> text explicitly scope their result to only the 'arena-construction/JIT-safety half' of ARENA-01's claim (the 7 biome arenas genuinely satisfy their target flag, and load safely with their source mod disabled). The other half -- 'every v1-registered boss across all integrated mods is explicitly classified... and routed via BossArenaRoutingRegistry' -- spans Phases 6-8, which have not yet been planned or executed. Marking ARENA-01 complete now would be factually wrong. REQUIREMENTS.md's ARENA-01 checkbox is left unchecked; it should only be marked complete once Phase 6-8's boss-routing work closes the remaining half."

patterns-established: []

requirements-completed: []

# Metrics
duration: verification-only
completed: 2026-08-14
---

# Phase 09 Plan 06: Live Biome-Flag Verification (Wave 3) Summary

**Live in-game confirmation that all 7 biome boss-arena subworlds (Hallow, Underworld, Jungle, Space, Desert, Astral, Briar) actually satisfy their real per-tick Zone/Biome flag on entry, across all three underlying mechanism families (vanilla SceneMetrics tile-weighting, height-only Y-position, and modded ModBiome tile-count hooks) -- with one live-discovered Desert crash fixed mid-checkpoint.**

## Performance

- **Duration:** verification-only (no automated build/test loop; three live in-game checkpoints across separate user sessions)
- **Tasks:** 3
- **Files modified:** 1 (mid-checkpoint bug fix)

## Accomplishments
- Confirmed live that all 3 vanilla tile-weighted biomes (Hallow, Jungle, Desert) generate without crashing and each satisfies its target `SceneMetrics`-derived Zone flag (`ZoneHallow`, `ZoneJungle`, `ZoneDesert`) while standing on the platform
- Confirmed live that both height-only biomes (Underworld, Space) generate correctly and satisfy their target Zone flag (`ZoneUnderworldHeight`, `ZoneSkyHeight`) purely from platform Y-position, with no themed tile fill required
- Confirmed live, with CalamityMod and SpiritMod both enabled, that both modded `ModBiome` biomes (Astral, Briar) generate correctly and satisfy their target flag (`ZoneAstral` with `ZoneDungeon=False`, `Briar.InBriar=True`)
- Found and fixed a live-reproducible native stack-overflow crash in `DesertPlatformPass` (falling Sand recursion) mid-checkpoint, without needing to re-run the other two already-passed biomes in that same checkpoint

## Task Commits

Each task was committed atomically:

1. **Task 1: Live verification -- vanilla tile-weighted biomes (Hallow, Jungle, Desert)** - `047db7f` (fix, mid-checkpoint deviation) -- checkpoint itself produced no other commit (verification-only)
2. **Task 2: Live verification -- height-only biomes (Underworld, Space)** - no commit (verification-only, zero repo files modified)
3. **Task 3: Live verification -- modded ModBiome biomes (Astral, Briar)** - no commit (verification-only, zero repo files modified)

_Note: this plan's only code change (`047db7f`) was an auto-fixed deviation discovered during Task 1's live test, not a planned task deliverable -- Tasks 1-3 are pure verification gates over Plans 01-04's already-committed code._

## Files Created/Modified
- `Subworlds/DesertPlatformPass.cs` - Changed from a 20-row full-depth falling-Sand fill to a 3-row real-Sand cosmetic layer over a solid Sandstone base, fixing a native stack-overflow crash while preserving the same `SceneMetrics.SandTileCount` weight

## Decisions Made
- Desert's crash fix (Rule 1) mirrors `UnderworldPlatformPass`'s existing thin-cosmetic-layer-over-solid-base pattern rather than introducing a new technique, keeping the codebase's tile-fill approach consistent across biomes that need a falling/unstable tile type for their themed surface.
- ARENA-01 deliberately left unmarked in REQUIREMENTS.md (see key-decisions above) -- this plan and 09-07 together only close the arena-construction/JIT-safety half of the requirement; the boss-classification-and-routing half is Phase 6-8 scope.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] Desert platform crashed with a native stack overflow on generation**
- **Found during:** Task 1 (live entry into the Desert biome arena, third of the three vanilla tile-weighted biomes tested)
- **Issue:** `DesertPlatformPass` originally filled all 20 platform rows with `TileID.Sand`, a falling-block tile. During `Subworld` generation, placing a full-depth column of unsupported falling Sand triggers `WorldGen.SquareTileFrame` -> `TileFrame` -> `WorldGen.SpawnFallingBlockProjectile` in a mutually recursive loop across the whole unsupported column, exhausting the native call stack and crashing the process (not a managed exception, so no catchable .NET stack trace -- confirmed via live reproduction, not static analysis).
- **Fix:** Reduced the falling-Sand layer to the top 3 rows only, backed by 17 rows of solid, non-falling `TileID.Sandstone` beneath. Per 09-RESEARCH.md's decompiled `SceneMetrics` weight table, both Sand and Sandstone contribute weight 1 per tile to `SceneMetrics.SandTileCount`, so `ZoneDesert`'s threshold math is unaffected by the swap -- only the falling-block recursion risk is removed. This exactly mirrors `UnderworldPlatformPass.cs`'s pre-existing thin-cosmetic-layer-over-solid-base pattern (Lava/Ash surface over solid Obsidian/Ash base), now confirmed as the correct general technique for any biome requiring an unstable/falling themed tile.
- **Files modified:** `Subworlds/DesertPlatformPass.cs`
- **Verification:** User re-entered the Desert biome arena live after the fix; no crash occurred, player spawned standing on the platform surface, and `/bossarena-checkbiomeflags` confirmed `ZoneDesert=True` (alongside `ZoneHallow=True`/`ZoneJungle=True` from the same checkpoint's earlier passes).
- **Committed in:** `047db7f`

---

**Total deviations:** 1 auto-fixed (Rule 1 - bug, discovered via live reproduction during the checkpoint itself, not via static code review)
**Impact on plan:** Necessary for Task 1's acceptance criteria ("all 3 subworlds generated without a crash") to actually hold. No scope creep -- fix is confined to the one file whose tile-fill choice caused the crash, and preserves the exact `SceneMetrics` weight contribution the plan's design already relied on.

## Issues Encountered
None beyond the Desert crash documented above, which was resolved inline during Task 1 without needing to escalate to a checkpoint:decision (Rule 1 applied cleanly -- same weight-preserving pattern already established for Underworld).

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness
- All 7 biome-variant `Subworld`/`GenPass` pairs (Hallow, Underworld, Jungle, Space, Desert, Astral, Briar) are now empirically confirmed -- via live in-game testing across all three underlying mechanism families -- to generate without crashing and to genuinely satisfy their target Zone/Biome flag, closing ARENA-01's Success Criterion 2 for the arena-construction half of the claim.
- Plan 07 (Wave 4) remains: the CalamityMod-disabled / SpiritMod-disabled JIT-safety checkpoint for the Astral/Briar pairs, followed by deletion of the now-fully-exercised `Debug/BiomeArenaDebugCommands.cs` (D-02 compliance).
- ARENA-01 itself is NOT yet complete -- the boss-classification-and-routing half of the requirement (across Phases 6-8's still-unplanned mods) remains outstanding; do not mark ARENA-01 complete in REQUIREMENTS.md until that work closes it.

---
*Phase: 09-biome-dependent-subworld-coverage*
*Completed: 2026-08-14*

## Self-Check: PASSED

- FOUND: Subworlds/DesertPlatformPass.cs
- FOUND: 047db7f (Task 1 mid-checkpoint fix commit)
