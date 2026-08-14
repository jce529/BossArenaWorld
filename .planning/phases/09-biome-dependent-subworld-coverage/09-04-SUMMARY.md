---
phase: 09-biome-dependent-subworld-coverage
plan: 04
subsystem: infra
tags: [subworldlibrary, genpass, zoneflag, modbiome, calamitymod, spiritmod, jit-safety]

requires:
  - phase: 04
    provides: BossArenaCorruptionSubworld/CorruptionPlatformPass template (vanilla-downed-flag guard, full-width platform fill), Integrations/CalamityIntegration.cs JIT-safety pattern
provides:
  - BossArenaAstralSubworld + AstralPlatformPass (Calamity ModBiome, ZoneAstral)
  - BossArenaBriarSubworld + BriarPlatformPass (Spirit ModBiome, InBriar)
affects: [09-05, 09-06, 09-07]

tech-stack:
  added: []
  patterns: ["Modded ModBiome.IsBiomeActive(Player) + ModSystem.TileCountsAvailable() detection, distinct from vanilla's TileID.Sets/SceneMetrics mechanism", "Subworld-as-autoloaded-ModType JIT-safety discipline: weak-referenced mod types confined exclusively to the paired GenPass class, never the Subworld subclass itself"]

key-files:
  created: [Subworlds/BossArenaAstralSubworld.cs, Subworlds/AstralPlatformPass.cs, Subworlds/BossArenaBriarSubworld.cs, Subworlds/BriarPlatformPass.cs]
  modified: []

key-decisions:
  - "Subworld subclasses (BossArenaAstralSubworld, BossArenaBriarSubworld) contain zero direct Calamity/Spirit type references; all such references confined to the paired XPlatformPass class's ApplyPass() body, since Subworld is itself an autoloaded ModType whose Register()/SetupContent() run unconditionally regardless of whether the mod is installed"
  - "Sulphurous Sea (originally also built in this plan) was descoped by user decision 2026-08-14 (09-CONTEXT.md D-07) and discarded before merging to master -- see Deviations below"

patterns-established:
  - "Modded ModBiome family: Astral (Calamity) and Briar (Spirit) use the modern IsBiomeActive hook, structurally different from the vanilla tile-weighted family (Plans 02-03)"
  - "JIT-safety discipline extended to a new autoload boundary (Subworld's Register()/SetupContent()), not just the previously-established PostSetupContent() guard pattern from Integrations/*.cs"

requirements-completed: [ARENA-01]

duration: n/a (see Issues Encountered)
completed: 2026-08-14
---

# Phase 9: Astral + Briar Biome Subworlds Summary

**Two modded-ModBiome boss-arena subworlds (Astral Infection, Briar) satisfying ZoneAstral/InBriar via Calamity/Spirit's IsBiomeActive hook, with Calamity/Spirit type references confined exclusively to their paired GenPass classes for JIT safety; the Sulphurous Sea pair originally built alongside them was descoped and discarded.**

## Performance

- **Tasks:** 2/3 planned tasks kept (Task 1: Astral, Task 3: Briar). Task 2 (Sulphurous Sea) was completed by the executor but subsequently discarded per user decision -- see Deviations.
- **Files modified:** 4 created (Astral + Briar only)

## Accomplishments
- `BossArenaAstralSubworld`/`AstralPlatformPass` — satisfies `player.Calamity().ZoneAstral` via Calamity's `ModBiome.IsBiomeActive()` hook
- `BossArenaBriarSubworld`/`BriarPlatformPass` — satisfies `SpiritMod.Biomes.BiomeTileCounts.InBriar` via Spirit's equivalent hook
- Both `Subworld` subclasses contain zero direct Calamity/Spirit type references (confined to their paired `XPlatformPass` classes), satisfying the newly-identified JIT-safety discipline (09-RESEARCH.md Pitfall 4) by construction — required because `Subworld` is itself an autoloaded `ModType` whose `Register()`/`SetupContent()` run unconditionally at mod load, unlike the `PostSetupContent()`-guard pattern `Integrations/CalamityIntegration.cs`/`SpiritIntegration.cs` use
- Both `Subworld` classes duplicate the vanilla-downed-flag guard from `BossArenaCorruptionSubworld` (34-field count verified)
- `dotnet build BossArenaSubWorld.csproj` succeeds with 0 warnings/errors (verified post-merge with Sulphurous Sea's files absent)

## Task Commits

Merged onto `master` via `git cherry-pick -x` (original worktree commits `0fb7c68`/`c1057ec`):

1. **Task 1: Astral Infection subworld/platform pass** - `a721863` (feat)
2. **Task 3: Briar subworld/platform pass** - `d2e7d98` (feat)

Task 2 (Sulphurous Sea subworld/platform pass) was originally committed as `f2ff360` in the isolated worktree but was **not** cherry-picked to `master` — see Deviations below.

## Files Created/Modified
- `Subworlds/BossArenaAstralSubworld.cs` - Astral Infection biome arena Subworld subclass (zero Calamity type references)
- `Subworlds/AstralPlatformPass.cs` - GenPass calling Calamity's ModBiome hook, confines all Calamity type references
- `Subworlds/BossArenaBriarSubworld.cs` - Briar biome arena Subworld subclass (zero Spirit type references)
- `Subworlds/BriarPlatformPass.cs` - GenPass calling Spirit's ModBiome hook, confines all Spirit type references

## Decisions Made
- Confirmed and applied the JIT-safety discipline identified in `09-RESEARCH.md` Pitfall 4: since `Subworld` is an autoloaded `ModType`, any Calamity/Spirit type reference inside the `Subworld` subclass itself (not just inside a `PostSetupContent()`-guarded method) would JIT-crash on mod load when that mod is absent. All such references were kept exclusively in the paired `GenPass.ApplyPass()` body.

## Deviations from Plan

### Auto-fixed Issues

**1. [Scope change — user-directed] Sulphurous Sea descoped from Phase 9 mid-execution**
- **Found during:** Live execution, after Tasks 1 (Astral), 2 (Sulphurous Sea), and 3 (Briar) had all already completed and committed in the isolated worktree (`agent-a51275de945e244e7`)
- **Issue:** The user instructed, mid-Wave-1: "don't make separate subworlds (or, looking ahead, altars) for Dungeon or Sulphurous Sea." This is a scope change against the checker-verified `09-CONTEXT.md` D-06 decision ("all 9 biome variants, not a subset").
- **Fix:** The orchestrator stopped the executor agent (already past all three tasks, mid final self-check), inspected the worktree, cherry-picked only the Astral and Briar commits (`0fb7c68`/`c1057ec` → `a721863`/`d2e7d98` on `master`), and left the Sulphurous Sea commit (`f2ff360`, containing `BossArenaSulphurousSubworld.cs`/`SulphurousPlatformPass.cs`) unmerged. The worktree and its branch were subsequently deleted, so that code is discarded, not merely hidden. `09-CONTEXT.md` was updated with a new decision D-07 recording this change and its rationale. Sibling plans 09-05/09-06/09-07 were revised to reference only the 7 kept biomes.
- **Files affected:** `Subworlds/BossArenaSulphurousSubworld.cs`, `Subworlds/SulphurousPlatformPass.cs` — built, then discarded, never present on `master`.
- **Verification:** `ls Subworlds/ | grep -i sulphurous` returns nothing on `master`; `dotnet build BossArenaSubWorld.csproj` succeeds with Astral/Briar's files present and Sulphurous's absent.
- **Impact:** Reduces Phase 9's build scope from 9 to 7 biome variants. Sulphurous Sea was already the lowest-priority of the original 9 (zero assignable boss in a Calamity-only install; The Old Duke only becomes assignable when Infernum is also loaded, per `09-ALTAR-BIOME-REFERENCE.md`), so this deferral has minimal near-term impact. See `09-CONTEXT.md` D-07 for full rationale.

---

**Total deviations:** 1 (user-directed scope reduction, not a plan-execution defect)
**Impact on plan:** Tasks 1 and 3 (Astral, Briar) executed exactly as planned. Task 2 (Sulphurous Sea) was executed correctly but its output was discarded by explicit user decision, not due to any code defect.

## Issues Encountered
Duration was not recorded by name in the original executor's transcript before it was stopped mid-self-check; all three tasks' commits confirm completion occurred within the same session as sibling Wave-1 plans (~10-20 min range, consistent with 09-01/09-02/09-03).

## Next Phase Readiness
- `BossArenaAstralSubworld`/`BossArenaBriarSubworld` are structurally ready for a future `BossArenaRoutingRegistry.Register<T>()` call in Phase 6/7 (Astrum Deus/Aureus for Astral; Vinewrath Bane for Briar).
- No Sulphurous-Sea-based boss (The Old Duke, Infernum-only) can be routed to a biome-safe arena until Sulphurous Sea coverage is reinstated in a future phase — flagged in `09-CONTEXT.md` Deferred Ideas.
- No blockers for Plan 09-05 (debug tool) or Plan 09-07 (JIT-safety checkpoint), both revised to reference only Astral/Briar.

---
*Phase: 09-biome-dependent-subworld-coverage*
*Completed: 2026-08-14*
