<!-- GSD:project-start source:PROJECT.md -->
## Project

**BossArenaSubWorld**

A Terraria tModLoader mod that lets a player fight lag-heavy bosses (Moon Lord, Infernum/Wrath-reworked bosses, and other bosses from large content mods) inside a dedicated subworld that has never had any mod content placed in it, then carries the boss-kill progress back to the main world. Solves severe FPS crashes (40-50 → 1-2) caused by running multiple large content mods (Calamity, Spirit, Redemption, etc.) simultaneously during heavy boss fights.

**Core Value:** The generic boss-kill → carrier-item → main-world-apply mechanism (BossRegistry + BossCoreItem + GlobalNPC) must reliably reproduce a boss's full "downed" state in the main world — flags, netcode sync, and any WorldGen side effects — for any registered boss. If this pipeline doesn't work end-to-end, nothing else matters.

### Constraints

- **Tech stack**: tModLoader mod in C#, .NET 8.0 SDK, developed in VS Code with C# Dev Kit, built via `dotnet msbuild`
- **Dependency**: SubworldLibrary for subworld creation/management
- **Compatibility**: must reproduce each source mod's actual `OnKill` side effects (flag + netcode sync + any WorldGen calls) rather than just setting a boolean — under-reproducing breaks vanilla systems that key off those flags (e.g. Lantern Night event triggers)
- **API variance**: each content mod exposes downed-progress differently (Calamity: wrapper properties with hooks; Spirit: raw static fields) — registration code must be written per-mod after per-mod research, no generic shortcut
<!-- GSD:project-end -->

<!-- GSD:stack-start source:research/STACK.md -->
## Technology Stack

## Recommended Stack
### Core Technologies
| Technology | Version | Purpose | Why Recommended |
|------------|---------|---------|-----------------|
| tModLoader | 1.4.4.9 (current stable, targets Terraria 1.4.4.9) | Mod loader / runtime the project builds against | This is the platform the project already targets per `PROJECT.md`; all installed content mods (Calamity, Spirit, etc.) are built for this branch |
| .NET SDK | 8.0.x (any current 8.0 patch) | Compiles the mod's C# into the IL tModLoader loads | tModLoader 1.4 requires exactly .NET 8 — the official VS Code setup guide explicitly warns "avoid .NET 9.0 and .NET 10.0, as those will not work." This is a hard constraint, not a preference. |
| C# | Language version tied to `net8.0` TargetFramework (C# 12 features available) | Implementation language | Only language tModLoader's build pipeline supports; matches `PROJECT.md` constraint |
| MSBuild via `dotnet msbuild` | Bundled with .NET 8 SDK | Build command for the mod project | Official build command per tModLoader wiki; `dotnet build <ModName>.csproj` is the fallback when `dotnet msbuild` alone doesn't pick up new package/reference resolution (a known quirk — run `dotnet restore` first if you see `project.assets.json not found`) |
| VS Code + C# Dev Kit | Latest | IDE | Matches project's stated constraint (`PROJECT.md`: "developed in VS Code with C# Dev Kit"). Officially supported alternative to Visual Studio 2022 17.8+. Enables breakpoint debugging and hot reload against a running tModLoader instance when configured per the official wiki guide. |
### Supporting Libraries
| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| SubworldLibrary (jjohnsnaill fork) | Current Steam Workshop build, last updated Nov 24 2025, declares compatibility with tModLoader 1.4.3 and 1.4.4 | Subworld creation, entry/exit, generation-task pipeline, player-state handling across worlds | Required — this is the project's stated dependency for creating the dedicated no-content arena world. Reference via `modReferences = SubworldLibrary` in `build.txt`. |
| Terraria.ModLoader `TagCompound` (built-in, ships with tModLoader — not a separate package) | N/A (part of tModLoader API) | Serializing custom save data: world-level boss-registry state (`SaveWorldData`/`LoadWorldData` on `ModSystem`) and carrier-item payload (`SaveData`/`LoadData` on `ModItem`) | Use for both the `BossRegistry`'s per-world downed-state bookkeeping and any custom fields on `BossCoreItem` (e.g. which boss key it carries) |
| `[JITWhenModsEnabled]` + `weakReferences` (built-in tModLoader attributes/build.txt fields, not a package) | N/A | Safely referencing Calamity/Spirit/Redemption/CatalystMod/NoxusBoss/ContinentOfJourney/Daybreak types without hard-depending on all of them being installed | Required for the per-mod boss registration code — see "What NOT to Use" below |
| ExampleMod (bundled with the tModLoader GitHub repo, not installed at runtime) | Matches installed tModLoader version | Reference implementation only — not a runtime dependency | Pull up `ExampleMod/Common/Systems/DownedBossSystem.cs` and `ExampleMod/NPCs/ExampleGlobalNPC.cs` locally as the canonical pattern for `ModSystem`-based downed-flag storage and `GlobalNPC.OnKill` boss detection — this is exactly the shape `BossRegistry`/`BossCoreItem`/`GlobalNPC` should follow |
### Development Tools
| Tool | Purpose | Notes |
|------|---------|-------|
| tModLoader in-game "Workshop → Develop Mods → New Mod" wizard | Scaffolds a new mod's `.csproj`, `build.txt`, and folder layout | Use this instead of hand-writing the `.csproj` from scratch — it wires in tModLoader's custom MSBuild imports/targets correctly. `BossArenaSubWorld` already exists as a project under `ModSources`, so this only matters if regenerating scaffolding or creating companion test mods. |
| `dotnet restore` | Resolves NuGet/project references before first build or after adding `modReferences`/`weakReferences` | Run whenever `build.txt` dependency lines change, or on "project.assets.json not found" errors |
| tModLoader hot reload (VS Code debug config) | Iterate on mod code against a running tModLoader instance | Documented in the official "Developing with Visual Studio Code" wiki page; requires enabling hot reload settings in VS Code preferences and using Run and Debug (Ctrl+Shift+D) |
| Installed content mods as local references (Calamity, Spirit, etc., already present via Steam Workshop) | Source of the actual `DownedBossSystem`/`MyWorld` classes being targeted | tModLoader's build pipeline resolves `modReferences`/`weakReferences` against installed mods (Workshop-downloaded or `ModSources`) automatically — no manual DLL copying needed once `build.txt` lists them |
## Installation
# One-time: ensure .NET 8 SDK is installed (verify, do not install 9/10)
# From the BossArenaSubWorld project directory
# or, if msbuild alone doesn't pick up new references:
# build.txt — dependency declarations (not a package manager file, tModLoader-specific)
## Alternatives Considered
| Recommended | Alternative | When to Use Alternative |
|-------------|-------------|--------------------------|
| SubworldLibrary (jjohnsnaill/SubworldLibrary fork) | Mirsario/Terraria-SubworldLibrary (original repo) | Never for new work — the Mirsario repo is the predecessor codebase; the jjohnsnaill fork is the one actively distributed via Steam Workshop/Mod Browser and the one already listed as a dependency in `PROJECT.md`'s installed mod list. Confirm this is genuinely what's installed before writing code against it. |
| VS Code + C# Dev Kit | Visual Studio 2022 (17.8+) | If you want a more powerful visual debugger/refactoring tools and don't mind the heavier install; functionally equivalent for tModLoader dev, both officially documented |
| `weakReferences` + `[JITWhenModsEnabled]` for content-mod interop | `modReferences` (strong reference) to each content mod | Only if you intend `BossArenaSubWorld` to hard-require every single target mod to even load — explicitly not what this project wants, since a player may have some but not all of Calamity/Spirit/Redemption/etc. installed at a given time |
| Carrier-item + `TagCompound` world data (per `PROJECT.md`'s adopted design) | Attempting to rely on SubworldLibrary to auto-propagate downed flags between worlds | Never — this is a known ecosystem gap (downed flags are serialized per-world-file and overwritten on load), which is exactly why `PROJECT.md` already adopted the carrier-item workaround instead |
## What NOT to Use
| Avoid | Why | Use Instead |
|-------|-----|--------------|
| .NET 9.0 or .NET 10.0 SDK | Explicitly unsupported by tModLoader 1.4's build pipeline per the official VS Code setup wiki ("those will not work") | .NET 8.0 SDK, any current patch version |
| Hand-rolled `.csproj` written from a generic C# class-library template | Misses tModLoader-specific MSBuild imports/targets that resolve `modReferences`, copy the built `.tmod`, etc.; easy to get subtly wrong | Scaffold via tModLoader's "New Mod" in-game wizard, then edit `build.txt`/source from there |
| `modReferences` (strong references) to Calamity, Spirit, Redemption, CatalystMod, NoxusBoss, ContinentOfJourney, Daybreak | Forces all of those mods to be installed and enabled just for `BossArenaSubWorld` to load at all — contradicts the project's own goal of working across whichever subset of content mods a player has active | `weakReferences` in `build.txt`, with cross-mod-referencing code isolated into classes/methods tagged `[JITWhenModsEnabled("ModName")]` so the mod still loads fine when a given content mod is absent |
| Directly writing to vanilla `NPC.downedBoss*`-style static flags or a content mod's raw backing field without going through its intended setter/wrapper | Bypasses side-effect hooks (netcode sync broadcasts, "boss just downed" messages, WorldGen triggers) that other in-game systems key off of — `PROJECT.md` already flags this as a correctness requirement (e.g. Lantern Night event triggers depend on the *side effects*, not just the boolean) | Call each mod's actual downed-setter path (e.g. Calamity's wrapper property setters that call `NPC.SetEventFlagCleared`, followed by `CalamityNetcode.SyncWorld()` / `CalamityGlobalNPC.SetNewBossJustDowned()`) exactly as that mod's own `OnKill` code does |
| Relying on the `mirror.sgkoi.dev` Mod Browser mirror page's version metadata for SubworldLibrary (it currently shows a stale "v1.1.1 / tModLoader v0.11.8.9 / updated 4 years ago" record) | Contradicts the Steam Workshop page for the same mod, which shows an update on Nov 24, 2025 declaring compatibility with tModLoader 1.4.3/1.4.4 — the mirror appears to be indexing an outdated or mismatched entry | Treat the Steam Workshop page (or in-game Mod Browser) as the source of truth for SubworldLibrary's current version/compatibility, not third-party mirrors |
## Stack Patterns by Variant
- Use SubworldLibrary's `SubworldSystem.Enter<T>()` / `SubworldSystem.Exit()` directly; no need to touch its multiplayer-specific internals (`SyncDisconnect`, `MovePlayerToSubworld`, subserver linking)
- No custom `ModPacket` broadcasting needed for boss-registry state — everything happens in-process on the single client/server
- Will need to broadcast `BossRegistry` apply-events via `ModPacket`/`NetMessage` so all connected clients see the downed state change, since `GlobalNPC.OnKill` and item-use hooks are server-authoritative
- Will need to account for SubworldLibrary's dedicated-server subserver-link behavior (`SubserverLink.cs` in the library) rather than the simpler single-process `Enter`/`Exit` flow
## Version Compatibility
| Package A | Compatible With | Notes |
|-----------|------------------|-------|
| tModLoader 1.4.4.9 | .NET 8.0 SDK | Hard requirement; confirmed in official "Developing with Visual Studio Code" wiki page |
| tModLoader 1.4.4.9 | SubworldLibrary (Steam Workshop build, updated Nov 24 2025) | SubworldLibrary's own workshop listing declares support for tModLoader 1.4.3 and 1.4.4 |
| SubworldLibrary | Calamity / Spirit / Redemption / CatalystMod / NoxusBoss / ContinentOfJourney / Daybreak | All of these are independent content mods with no documented incompatibility with SubworldLibrary; compatibility for this project's actual boss-registration code must still be verified per-mod during implementation (per `PROJECT.md`'s "API variance" constraint) — this is a per-mod research task, not a stack-level concern |
## Sources
- https://github.com/tModLoader/tModLoader/wiki/Developing-with-Visual-Studio-Code — HIGH confidence, official wiki: confirms .NET 8 SDK requirement, explicitly excludes .NET 9/10, confirms `dotnet msbuild` build flow and C# Dev Kit setup
- https://tmodloader.app/docs/development-setup.html (referenced via search; direct fetch failed with DNS error, corroborated by wiki source above) — MEDIUM confidence
- https://github.com/jjohnsnaill/SubworldLibrary — MEDIUM confidence: repo structure confirmed (`Subworld.cs`, `SubworldSystem.cs`, `CrossModSubworld.cs`, `SubserverLink.cs`); exact release/version metadata not retrievable via WebFetch (dynamic GitHub page)
- https://github.com/jjohnsnaill/SubworldLibrary/wiki — MEDIUM confidence: confirms `Subworld` base class members (`Width`/`Height`/`Tasks`/`ShouldSave`/`NoPlayerSaving`) and `SubworldSystem.Enter<T>()`/`Enter(string)`/`Exit()` API shape
- https://steamcommunity.com/workshop/filedetails/?id=2785100219 — MEDIUM confidence, official distribution channel: current version last updated Nov 24 2025, declares tModLoader 1.4.3/1.4.4 compatibility (contradicts the stale mirror entry noted above)
- https://mirror.sgkoi.dev/Mods/Details/SubworldLibrary — LOW confidence / flagged as unreliable: shows stale metadata (v1.1.1, tModLoader v0.11.8.9) inconsistent with the Steam Workshop listing; do not trust for version decisions
- https://github.com/tModLoader/tModLoader/wiki/Expert-Cross-Mod-Content — HIGH confidence, official wiki: source for `modReferences`/`weakReferences`/`[JITWhenModsEnabled]` cross-mod interop pattern, directly applicable to this project's per-content-mod boss registration
- https://github.com/tModLoader/tModLoader/wiki/Saving-and-loading-using-TagCompound — HIGH confidence, official wiki: source for `SaveWorldData`/`LoadWorldData` (ModSystem) and `SaveData`/`LoadData` (ModItem) patterns used for `BossRegistry` and `BossCoreItem`
- https://github.com/tModLoader/tModLoader/blob/1.4/ExampleMod/Common/Systems/DownedBossSystem.cs (and `ExampleGlobalNPC.cs` in the same repo) — HIGH confidence, official example code: canonical reference pattern for downed-flag storage + `GlobalNPC.OnKill` detection
- https://github.com/tModLoader/tModLoader/wiki/build.txt — MEDIUM confidence: confirms `weakReferences = ModName@version` syntax for minimum-version soft dependencies
- https://github.com/GabeHasWon/HomewardSubworld — LOW confidence: repo exists and depends on both "Homeward Journey" and "Subworld Library" per its description, but WebFetch could not retrieve enough source detail to confirm a specific cross-world data-carry pattern; worth a manual read of `AbyssalSubworld.cs`/`HomewardSubworld.cs` during implementation research rather than relying on this summary
<!-- GSD:stack-end -->

<!-- GSD:conventions-start source:CONVENTIONS.md -->
## Conventions

Conventions not yet established. Will populate as patterns emerge during development.
<!-- GSD:conventions-end -->

<!-- GSD:architecture-start source:ARCHITECTURE.md -->
## Architecture

Architecture not yet mapped. Follow existing patterns found in the codebase.
<!-- GSD:architecture-end -->

<!-- GSD:workflow-start source:GSD defaults -->
## GSD Workflow Enforcement

Before using Edit, Write, or other file-changing tools, start work through a GSD command so planning artifacts and execution context stay in sync.

Use these entry points:
- `/gsd:quick` for small fixes, doc updates, and ad-hoc tasks
- `/gsd:debug` for investigation and bug fixing
- `/gsd:execute-phase` for planned phase work

Do not make direct repo edits outside a GSD workflow unless the user explicitly asks to bypass it.
<!-- GSD:workflow-end -->



<!-- GSD:profile-start -->
## Developer Profile

> Profile not yet configured. Run `/gsd:profile-user` to generate your developer profile.
> This section is managed by `generate-claude-profile` -- do not edit manually.
<!-- GSD:profile-end -->
