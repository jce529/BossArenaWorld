# Pitfalls Research

**Domain:** tModLoader mod — SubworldLibrary-based boss arena with soft cross-mod dependencies on Calamity/Spirit/Redemption/CatalystMod/NoxusBoss/Homeward-series, using reflection/Mod.Call to read-write boss "downed" progress
**Researched:** 2026-08-12
**Confidence:** MEDIUM-HIGH (SubworldLibrary source/wiki and tModLoader's own "Expert Cross Mod Content" wiki are authoritative and current; per-mod internal field names carry lower confidence and must be re-verified against installed DLLs at implementation time)

## Critical Pitfalls

### Pitfall 1: Boss "downed" flags are isolated per world file and do not survive the subworld round-trip

**What goes wrong:**
A boss killed inside the subworld appears "downed" only in that subworld's own `.wld`/`.twld` data. When the player returns to the main world, the main world's own (unchanged) flags are what's active — the kill is invisible there. This is the exact mechanism described in `PROJECT.md` and is why the carrier-item architecture was chosen in the first place; it is not a hypothetical risk, it's the documented reason this project exists.

**Why it happens:**
SubworldLibrary loads each subworld as an effectively separate `WorldFileData`/world-state context (its own `Main`-level flags, NPC downed booleans, bestiary tracker, etc.), not a shared overlay of the main world. `CopyMainWorldData()` in SubworldLibrary only copies a specific, hand-picked set of vanilla state into the subworld via reflection ("it's called reflection because the code is ugly like you" — literal source comment) and, historically, did not reliably copy changes back out. As of a 4 Jan 2025 SubworldLibrary update, the library gained a generic keyed `WorldData` store where "all data can be overwritten unless a key starts with `!`" and claims fixes for "changes to the main world not being transferred back in Singleplayer" and bestiary propagation — **but this generic store is opt-in infrastructure for subworld-aware mods, not a universal patch for arbitrary third-party mods' custom progress systems** (e.g. Calamity's `DownedBossSystem`, Spirit's `MyWorld` statics). Evidence this remains unsolved for large content mods: the third-party "Calamity Boss Resyncer" workaround mod was still being posted/used in 2025 specifically because Calamity boss progress "gets deleted after visiting subworlds."

**How to avoid:**
- Do not attempt to rely on SubworldLibrary's native world-data sync for the mods this project targets. Treat the carrier-item pattern (kill → `BossCoreItem` → apply-on-use in main world) as the only supported path, regardless of SubworldLibrary version.
- Set the arena `Subworld.ShouldSave = false` (the default) so the scratch world's local (fake) downed state is never persisted or trusted — the subworld's own flags are throwaway.
- Verify empirically per boss the first time it's registered: kill it in the subworld, return without using the carrier item, and confirm the main world's flag is still false. This should be a standing manual test, not an assumption.

**Warning signs:**
- A boss shows "downed" behavior (e.g. despawns future spawns) in the subworld session but the main world's bestiary/boss checklist still shows it undefeated after returning — expected, confirms the bug is present as designed-for.
- The *opposite* — main world flag flips true without the carrier item ever being used — means either SubworldLibrary's generic sync unexpectedly caught this mod's flags (re-test after every SubworldLibrary update) or there's a bug in this project's own Enter/Exit hooks.

**Phase to address:**
Foundational subworld-setup phase (before any per-mod boss registration work) — prove the isolation bug is real and reproducible for at least one boss (start with a vanilla boss, since vanilla `NPC.downedBoss*` fields are the simplest case) before building the BossRegistry abstraction on top of it.

---

### Pitfall 2: Weak-reference / reflection code crashes at JIT time even behind a null-check

**What goes wrong:**
Code like `var calamity = ModLoader.GetMod("CalamityMod"); if (calamity != null) { var x = new CalamityMod.SomeType(); }` can throw a `TypeLoadException`/crash the whole mod at startup **even when the guard correctly prevents that branch from running**, if Calamity is disabled/missing. The .NET JIT compiles a method's IL (and resolves the types it references) the first time that method is *called*, not lazily per-branch — so an unresolvable type anywhere in a method body can blow up the method even if the specific line is unreachable at runtime.

**Why it happens:**
This is a JIT compilation quirk, not a logic bug — confirmed directly by tModLoader's own "Expert Cross Mod Content" wiki page, which calls this out as *the* critical risk of weak references and prescribes wrapping any code that references another mod's types in a separate method/property annotated with `[JITWhenModsEnabled("ModName")]` so the JIT defers compiling it until that mod is confirmed present.

**How to avoid:**
- Preferred for this project: avoid compile-time weak references entirely for the boss-flag interop. Use pure runtime reflection (`Mod.Code` → `Assembly.GetType("Namespace.ClassName")` by string, `FieldInfo`/`PropertyInfo` via `BindingFlags`) so there is no compiled reference to the other mod's types at all — this sidesteps the JIT hazard by construction, at the cost of losing compile-time type safety (acceptable tradeoff here since these mods largely don't expose public APIs for this anyway).
- If any code path *does* take a compile-time `weakReferences` dependency (e.g. for a mod that does expose a stable public type), isolate every use of that mod's types into dedicated methods marked `[JITWhenModsEnabled("ModName")]`, and call those methods only from behind a `ModLoader.TryGetMod`/`HasMod` guard.
- Test explicitly with each soft-dependency mod disabled — this is the only way this class of bug reliably surfaces.

**Warning signs:**
- Mod fails to load (crash on `Mod.Load`) only when a specific content mod is disabled, with a `TypeLoadException`/`MissingMethodException` in the log pointing at a method that "looks like" it correctly null-checks the mod first.

**Phase to address:**
Per-mod boss registration phase — apply as a hard rule from the first registered mod onward (Calamity, being the first per `PROJECT.md`'s Active requirements), and re-verify with a "disable this mod, does BossArenaSubWorld still load?" smoke test per mod added.

---

### Pitfall 3: Reflection into another mod's internals breaks silently (or crashes) after that mod updates

**What goes wrong:**
Content mods like Calamity refactor their internal boss-progress storage between updates (the `PROJECT.md` notes Spirit "may have moved from `ModWorld` to `ModSystem`" already). A field/property name change, type change, or class relocation breaks the reflection lookup. If unguarded, this throws a `NullReferenceException`/`TargetException` deep inside `GlobalNPC.OnKill` or item-use code — potentially on every boss kill, or worse, silently no-ops and the player's boss-downed state is never actually applied even though the carrier item is consumed.

**Why it happens:**
Reflection by string name has no compiler-enforced contract with the target mod. tModLoader's own wiki explicitly calls reflection a "bad approach" for this exact reason — "brittle code vulnerable to breaking when dependency mods update their internal structure" — while also acknowledging it's sometimes the only option when the target mod has no `Mod.Call` API (true for Calamity's/Spirit's boss-progress internals, per the project's own prior research).

**How to avoid:**
- Cache every `FieldInfo`/`PropertyInfo`/`Type` lookup once (e.g. in a per-boss registration record built during `Mod.PostSetupContent`), not per-kill — this also means a broken lookup fails fast at load time instead of mid-fight.
- Wrap every reflective lookup and every reflective get/set in `try/catch`, log a clear mod-load warning naming the specific boss/mod/member that failed, and **disable that specific boss's registration** rather than crashing the whole mod. A missing/broken registration for one boss should never take down the carrier-item pipeline for all other bosses.
- Record the exact mod `Version` this project was built/tested against per soft dependency (e.g. in a comment or a small compatibility table), and log a warning (not a crash) if the installed mod's `Version` differs from the known-good one at `Mod.PostSetupContent` time — this turns "silently wrong" into "loudly flagged."
- Prefer `weakReferences = ModName@X.Y` version pinning in `build.txt` where a compile-time reference is used at all, since tModLoader will refuse to load with an incompatible pinned version rather than silently misbehaving.

**Warning signs:**
- A boss kill drops the carrier item, the item is used, but the main world's checklist/bestiary still shows the boss undefeated — with no exception logged (silent no-op reflection failure is worse than a crash, because it looks like success).
- Client log shows `NullReferenceException`/`AmbiguousMatchException` originating from this mod's reflection helper immediately after updating a soft-dependency mod.

**Phase to address:**
Per-mod boss registration phase (design the reflection-access layer with try/catch + logging + version-check baked in from the very first mod registered, so it's a pattern, not a retrofit) and the full-pipeline verification phase (re-run the "kill → item → apply" smoke test after any soft-dependency mod update, before trusting a saved game to it).

---

### Pitfall 4: Setting the raw boolean flag replicates less than what actually happened on a "real" kill

**What goes wrong:**
Directly assigning `NPC.downedMoonlord = true` (or the modded equivalent) instead of going through the proper setter/helper reproduces *only* the flag, not the side effects vanilla/the source mod normally bundles with it: achievement progress notifications, bestiary "kill count" unlocks, the "a boss has been defeated!" chat/banner message, multiplayer packet sync (`NetMessage.SendData`), and — for world-altering bosses — WorldGen effects (hardmode ore selection on first mechanical boss kill, Temple/Plantera unlocks, altar-linked spawns, etc.). `PROJECT.md` already identifies this risk generally ("under-reproducing breaks vanilla systems that key off those flags, e.g. Lantern Night event triggers") — this pitfall is about *why* it's easy to under-reproduce even when you think you've covered it.

**Why it happens:**
Vanilla's own pattern is `NPC.SetEventFlagCleared(ref flag, eventType)`, which sets the flag **and** calls into `AchievementsHelper.NotifyProgressionEvent(eventType)` **and** syncs over the network when running as a server — three effects bundled in one call that's easy to reimplement incompletely if you reflect straight to the backing field instead. Content mods layer their own additional side effects on top (Calamity's pattern per prior project research: wrapper property setters that call `NPC.SetEventFlagCleared` internally, plus a separate `CalamityNetcode.SyncWorld()` and `CalamityGlobalNPC.SetNewBossJustDowned()` call expected alongside it). Skipping any one layer produces a boss that "looks" downed in one system (e.g. the checklist mod) but not another (e.g. achievements, or a later event that gates on the mod's own internal "just downed" banner state).

**How to avoid:**
- For each registered boss, replay the *setter*/helper method the source mod itself uses on kill (reflect to the property setter or call the mod's own helper method via reflection/`Mod.Call`), not the raw backing field, whenever a setter/helper exists.
- Where the source mod's setter is confirmed to have side-effecting siblings (Calamity's `SyncWorld()` + `SetNewBossJustDowned()`), call those explicitly as part of that boss's `BossRegistry.Apply()` entry — don't assume the flag setter alone triggers them.
- For world-altering bosses (mechanical bosses, Plantera, Golem, etc.), explicitly decide and document, per boss, whether WorldGen side effects (ore generation, Temple access, etc.) are being replayed — this is called out as an open Active requirement in `PROJECT.md` and is easy to silently skip since it "looks done" once the checklist shows the boss as downed.
- `CalamityNetcode.SyncWorld()`-style calls are cheap/harmless to call even in singleplayer (verified: it's gated on `Main.dedServ` and is a no-op when not a dedicated server) — so when in doubt, call the mod's full side-effect chain rather than trying to selectively skip "multiplayer-only" pieces.

**Warning signs:**
- Boss shows downed in a checklist/bestiary mod but a dependent vanilla system doesn't trigger (Lantern Night, Pumpkin Moon/Frost Moon unlocks, Old One's Army tier changes, hardmode ore never appears after "defeating" a mechanical boss via the carrier item).
- Achievements never unlock for bosses defeated only via the carrier item, even though the checklist shows them downed.

**Phase to address:**
Per-mod boss registration phase, specifically the WorldGen-side-effects requirement already listed in `PROJECT.md`'s Active requirements — treat "flag set" and "side effects replayed" as two separate, individually-verified checklist items per boss, not one.

---

### Pitfall 5: Boss rewards split between world-scoped and player-scoped data cause double-grants

**What goes wrong:**
Some "on boss kill" logic in content mods writes to **player-scoped** save data (a `ModPlayer` field, serialized with the character save) rather than (or in addition to) world-scoped data — for example, a "first kill of this boss" one-time reward, unlocked recipe, or per-character journal entry. Because `Subworld.NoPlayerSaving` defaults to `false`, ModPlayer-scoped changes made during the subworld fight **do survive** the return trip automatically, with no carrier item needed. If the carrier-item `Apply()` logic for that boss *also* replays the mod's full on-kill logic (because it was written assuming nothing survived the trip), the player-scoped reward gets granted twice.

**Why it happens:**
It's natural to assume "nothing carries over from the subworld" globally, based on the well-documented world-flag isolation bug (Pitfall 1) — but that isolation is specifically about *world*-scoped state. Player objects are not reset by SubworldLibrary by default; only enabling `NoPlayerSaving = true` on the `Subworld` reverts player changes on exit, and this project should not enable that flag (it would also revert legitimately-earned inventory/buffs, including the carrier item itself if granted before the transition completes).

**How to avoid:**
- Confirm `NoPlayerSaving` stays `false` (default) for the arena `Subworld` — this is required for the carrier item and any earned loot to survive at all, but it means player-scoped side effects of a kill are *not* isolated the way world-scoped ones are.
- When researching each mod's on-kill logic (per `PROJECT.md`'s per-mod research requirement), explicitly classify each side effect as world-scoped (needs replay via the carrier item) vs. player-scoped (already survived, must NOT be replayed) before writing that boss's `BossRegistry.Apply()` entry.
- When in doubt, test: kill the boss in the subworld, do *not* use the carrier item, return to the main world, and check whether the player-scoped reward already exists (e.g. recipe unlocked, journal entry present). If yes, exclude it from `Apply()`.

**Warning signs:**
- Duplicate currency/material grants, duplicate "you got X!" chat messages, or duplicate unlock notifications the first time a boss is defeated via the carrier item.

**Phase to address:**
Per-mod research phase (classify each mod's on-kill side effects by scope as part of the existing per-mod research work already planned) and BossRegistry design phase (the `Apply()` contract should have an explicit place to document "skip — already player-scoped" per side effect, not just a list of things to do).

---

### Pitfall 6: `Assembly.GetTypes()` throws when scanning a soft-dependency mod's assembly

**What goes wrong:**
If reflection code enumerates all types in a target mod's assembly (e.g. to search for a class by partial name instead of a known full name) via `mod.Code.GetTypes()`, this can throw `ReflectionTypeLoadException` and abort the whole enumeration — not just skip the problem type — if that mod itself uses `ExtendsFromModAttribute` to weakly extend another (possibly-absent) mod. This is a documented tModLoader gotcha, not a hypothetical.

**Why it happens:**
tModLoader's own `Mod.Code` documentation explicitly warns: "Do NOT call `Assembly.GetTypes` on this as it will error out if the mod uses the `ExtendsFromModAttribute` attribute... Use `AssemblyManager.GetLoadableTypes(Assembly)` instead." Large content mods like Calamity are exactly the kind of mod likely to weakly extend other mods (e.g. optional cross-mod content), making this a realistic risk here.

**How to avoid:**
- Never call `.GetTypes()` directly on another mod's `Mod.Code` assembly. Use `Terraria.ModLoader.Core.AssemblyManager.GetLoadableTypes(assembly)` (or, better, avoid full-assembly enumeration entirely and look up the specific known type by full name via `assembly.GetType("Namespace.ClassName")`, which does not have this failure mode).
- Prefer exact `GetType(fullName)` lookups over enumeration wherever the target class name is already known from prior research (which `PROJECT.md` indicates it will be, per-mod).

**Warning signs:**
- Mod registration for a boss throws `ReflectionTypeLoadException` at startup, specifically when scanning a mod known to have its own soft dependencies (Calamity is the most likely candidate here).

**Phase to address:**
Per-mod boss registration phase — bake this into the shared reflection-helper utility from the start so every per-mod registration inherits the safe pattern.

---

### Pitfall 7: `Type.GetType(string)` silently fails to find types in other mods' assemblies

**What goes wrong:**
`System.Type.GetType("SomeMod.SomeClass")` (the framework method, not going through the mod's own `Assembly`) typically returns `null` for types defined in another mod's assembly, because each tModLoader mod is loaded into its own assembly-load context rather than the default app domain's searchable set. Code that doesn't check for `null` here proceeds with a `NullReferenceException` on the next reflection call, and the failure mode looks identical to "the mod isn't installed" — easy to misdiagnose.

**Why it happens:**
tModLoader loads each mod's compiled code as a distinct `Assembly` obtained via that mod's own `Mod.Code` property, not merged into a globally-searchable type namespace. `Type.GetType` by default only searches the calling assembly and `mscorlib`/core assemblies unless given an assembly-qualified name.

**How to avoid:**
- Always resolve the target mod first via `ModLoader.TryGetMod("ModName", out Mod targetMod)`, then look up types via `targetMod.Code.GetType("Namespace.ClassName")` (or `targetMod.Code.GetType(name, throwOnError: false)`), never via bare `Type.GetType(name)`.
- Explicitly null-check every step of the reflection chain (mod found → type found → member found → value retrieved) and log which step failed, so "mod not installed" and "mod installed but internals changed" are distinguishable in logs.

**Warning signs:**
- Reflection-based lookups return null / throw even when the target mod is confirmed enabled and loaded in-game.

**Phase to address:**
Per-mod boss registration phase — part of the same shared reflection-helper utility as Pitfalls 3 and 6.

---

## Technical Debt Patterns

| Shortcut | Immediate Benefit | Long-term Cost | When Acceptable |
|----------|-------------------|-----------------|------------------|
| Direct `NPC.downedX = true` field assignment instead of the proper setter/helper | Faster to write per boss | Skips achievements/sync/WorldGen side effects (Pitfall 4) | Never for release; acceptable only as a temporary smoke-test stub while proving the carrier-item pipeline works end-to-end |
| Un-cached reflection lookups performed on every kill/item-use instead of once at registration | Less boilerplate initially | Repeated `try/catch` cost per kill, and load-time failures move to runtime failures (harder to diagnose) | Never — caching at `PostSetupContent` is nearly free to add up front |
| Hardcoding one mod's field/property names inline in `BossRegistry.Apply()` instead of a per-mod adapter class | Fewer files for a 1-boss prototype | Cannot cleanly disable/replace one mod's registration without touching shared code; harder to unit-test in isolation | Acceptable only for the very first proof-of-concept boss (e.g. a vanilla boss) before the per-mod adapter pattern is established |
| Skipping the "disable this mod, does BossArenaSubWorld still load" smoke test per soft dependency | Saves a few minutes per mod added | JIT crashes (Pitfall 2) go undetected until a player with a different mod list hits them | Never |
| Treating `ShouldSave = false` on the arena Subworld as "nothing persists, so I don't need to think about scope" | Simpler mental model | Misses that player-scoped data does persist regardless (Pitfall 5) | Never — always explicitly classify world-scoped vs. player-scoped per side effect |

## Integration Gotchas

| Integration | Common Mistake | Correct Approach |
|-------------|----------------|-------------------|
| SubworldLibrary | Assuming a recent SubworldLibrary version has "fixed" boss-flag sync generally, based on 2025 changelog entries about world-data sync improvements | Those fixes target SubworldLibrary's own generic `WorldData` store and vanilla bestiary sync — verify empirically per target mod; keep the carrier-item architecture regardless of SubworldLibrary version (Pitfall 1) |
| Calamity (`DownedBossSystem`) | Setting the wrapper property but forgetting the accompanying `CalamityNetcode.SyncWorld()` / `CalamityGlobalNPC.SetNewBossJustDowned()` calls the setter normally triggers alongside it | Replay the full side-effect chain the setter is documented (by prior project research) to trigger, not just the property assignment; re-verify field/method names against the actually-installed Calamity DLL version before shipping, since Calamity refactors frequently |
| Spirit Mod (`MyWorld` statics) | Assuming the `ModWorld`-based class location from older documentation still holds | `PROJECT.md` already flags this needs rechecking — Spirit "may have moved from `ModWorld` to `ModSystem`"; verify against the installed copy at registration time, not from memory/old guides |
| Mods without any public `Mod.Call`/API for boss progress (expected for most of these) | Trying weak `modReferences`/compile-time typed access first, then falling back to reflection only when that fails | Default straight to pure runtime reflection (no compile-time type reference) for boss-progress reads/writes on any mod that hasn't published a stable public API for it — avoids the JIT hazard (Pitfall 2) by construction |
| Any soft-dependency mod, in general | Building/testing only with all target mods enabled | Explicitly test with each soft dependency individually disabled, and with all of them disabled — the "works with everything installed" case is the easy case, not the risky one |

## Performance Traps

| Trap | Symptoms | Prevention | When It Breaks |
|------|----------|------------|-----------------|
| Re-running reflection lookups (Type/Field/Property resolution) on every `OnKill`/item-use call instead of caching | Small per-kill hitch, larger cumulative log/exception noise if a lookup fails repeatedly | Cache all `FieldInfo`/`PropertyInfo`/`MethodInfo` once per boss at `Mod.PostSetupContent`, keyed in the `BossRegistry` | Not a v1-scale concern given boss kills are rare events, but worth doing right from the start since it's nearly free and improves failure diagnostics |
| Calling WorldGen-side-effect replay logic (ore generation, structure placement) synchronously on the main game thread during item-use | Frame hitch/stutter the moment the carrier item is used, worse for bosses with heavier WorldGen effects (mechanical bosses' ore selection) | Mirror how vanilla/the source mod actually invokes that WorldGen code (thread-safe wrapper vs. direct call) rather than assuming it's safe to call inline; test for hitching specifically on item-use, not just correctness | Only bosses with real WorldGen side effects (mechanical bosses, Plantera, Golem) — flag-only bosses are unaffected |
| Leaving the arena subworld's `NormalUpdates` at its default (`false`, i.e. vanilla world update loop disabled) without realizing which systems that silently turns off | A boss or mod that depends on normal world ticking (spawn tables, day/night-gated behavior, other NPCs) behaves unexpectedly inside the arena | Confirm per-boss whether it depends on any normal world-update behavior beyond its own AI; `NormalUpdates=false` is actually desirable for this project's performance goal, but should be a deliberate choice, not an unexamined default | Bosses whose AI/mechanics reference world-update-driven systems (day/night phase changes, event-gated attacks) |

## Security Mistakes

| Mistake | Risk | Prevention |
|---------|------|------------|
| Trusting the `BossCoreItem`'s boss-key tag without validating it against the current `BossRegistry` before calling `Apply()` | A stale/edited/duplicated item (e.g. from an old mod version, or a save-edited item) references a boss key that no longer exists or was removed, causing an unhandled exception or applying stale reflection data | Validate the key exists in the current `BossRegistry` before doing anything else in `Apply()`; fail gracefully (chat message, item stays unused) rather than throwing |
| Reflection helper swallowing all exceptions silently (over-correcting for Pitfall 3) so a real, actionable bug never surfaces to the player/dev | Player uses the carrier item, nothing happens, no log entry — "looks done but isn't," undiagnosable without source access | Log every reflection failure at warning/error level with the specific mod/type/member name, even though execution should continue for other bosses (Pitfall 3's prevention and this one are two sides of the same coin — fail soft, but never fail silent) |

## UX Pitfalls

| Pitfall | User Impact | Better Approach |
|---------|-------------|-------------------|
| Carrier item silently does nothing when used against a boss whose registration failed (missing mod, broken reflection) | Player thinks the mod is broken or their kill "didn't count," possibly re-attempts the whole subworld fight unnecessarily | On `Apply()` failure, show a clear chat message identifying what went wrong (e.g. "CalamityMod boss data has changed and this boss could not be applied — please report this") instead of a silent no-op |
| No confirmation that the boss's downed state was actually applied (flags + side effects) after using the carrier item | Player can't tell if it worked without manually checking a checklist/bestiary mod | Emit a success chat message on `Apply()` mirroring the tone of the source mod's own "boss just downed" message, confirming both flag and side-effect replay succeeded |
| Testing directly on a real save file | A broken `Apply()` (e.g. partial WorldGen side effects, corrupted flags) can leave a save in a hard-to-diagnose or hard-to-revert state | `PROJECT.md` already requires a world backup before end-to-end testing — treat this as non-negotiable for every new boss registration, not just the final pipeline test |

## "Looks Done But Isn't" Checklist

- [ ] **Boss registered in `BossRegistry`:** Often missing the accompanying netcode-sync/"just downed" side-effect calls the source mod normally bundles with its flag setter — verify against Pitfall 4, not just that the flag reads `true` afterward.
- [ ] **Boss "downed" after using the carrier item:** Often missing WorldGen side effects (ore generation, Temple/altar unlocks) for world-altering bosses — verify the actual world-generation outcome, not just the flag/checklist state.
- [ ] **Soft dependency on a content mod "works":** Often only tested with that mod enabled — verify the mod also loads cleanly (no JIT crash) with it disabled (Pitfall 2), and that reflection failures degrade to a per-boss warning rather than a full mod-load crash (Pitfall 3).
- [ ] **Carrier item survives the subworld trip:** Often assumed safe by default — explicitly confirm `Subworld.NoPlayerSaving` is `false` for the arena subworld; a stray `true` here silently deletes the item along with everything else gained during the fight.
- [ ] **Boss reward correctness:** Often not checked for duplication — confirm player-scoped rewards (recipes, journal entries, currency) aren't double-granted between the automatic subworld-to-main-world carryover and the carrier item's `Apply()` (Pitfall 5).

## Recovery Strategies

| Pitfall | Recovery Cost | Recovery Steps |
|---------|----------------|------------------|
| Reflection breaks after a soft-dependency mod update (Pitfall 3) | LOW | Update the cached field/property names for that one boss's adapter; because failures are isolated per-boss (per prevention strategy), no other registrations are affected while this is fixed |
| JIT crash from unguarded weak-reference code (Pitfall 2) | LOW–MEDIUM | Wrap the offending method in `[JITWhenModsEnabled]` or convert it to pure reflection; requires a rebuild and retest with the mod disabled, but no save-data impact |
| Double-granted player-scoped reward (Pitfall 5) | MEDIUM | Identify and reclassify the specific side effect as player-scoped in that boss's `Apply()` entry; existing affected saves may need a one-time manual correction (e.g. a debug command to remove the duplicate), since the double-grant already happened |
| World-flag isolation surprises a boss registration that assumed sync "just works" (Pitfall 1) | MEDIUM | Convert that boss's registration to the carrier-item pattern like the others; no save corruption expected since the flag was simply never set in the main world to begin with |
| Corrupted/partial WorldGen side effects from a bad `Apply()` call on a real save | HIGH | This is exactly why `PROJECT.md` mandates a world backup before end-to-end testing — recovery is "restore the backup," which is why that requirement must never be skipped, especially for early world-altering boss registrations |

## Pitfall-to-Phase Mapping

| Pitfall | Prevention Phase | Verification |
|---------|-------------------|----------------|
| World-flag isolation between subworld and main world (Pitfall 1) | Foundational subworld-setup phase | Kill a vanilla boss in the subworld, return without using any carrier item, confirm main-world flag is still false |
| JIT crash from weak-reference code (Pitfall 2) | Per-mod boss registration phase (from the first mod onward) | Disable each soft-dependency mod one at a time and confirm BossArenaSubWorld still loads without error |
| Silent reflection breakage (Pitfall 3) | Per-mod boss registration phase + full-pipeline verification phase | Force a reflection failure (e.g. temporarily rename a field lookup) and confirm it logs clearly and disables only that boss, not the whole mod |
| Under-reproduced side effects on flag apply (Pitfall 4) | Per-mod boss registration phase (WorldGen-side-effects requirement) | Per boss, verify both "flag set" and "side effects replayed" as separate checklist items; check achievements/bestiary/checklist-mod agreement |
| Player-scoped vs. world-scoped double-grants (Pitfall 5) | Per-mod research phase + BossRegistry design phase | Kill boss in subworld, skip the carrier item, check whether any player-scoped reward already exists before writing that boss's `Apply()` entry |
| `Assembly.GetTypes()` failures on mods with their own soft dependencies (Pitfall 6) | Per-mod boss registration phase (shared reflection-helper utility) | Confirm the reflection helper never calls raw `.GetTypes()`; code review / static check |
| `Type.GetType` failing to find modded types (Pitfall 7) | Per-mod boss registration phase (shared reflection-helper utility) | Confirm all type lookups go through `Mod.Code.GetType(...)` after `ModLoader.TryGetMod`, never bare `Type.GetType` |

## Sources

- SubworldLibrary GitHub repository and source (`Subworld.cs`, `SubworldSystem.cs`): https://github.com/jjohnsnaill/SubworldLibrary — HIGH confidence (primary source)
- SubworldLibrary Steam Workshop changelog (world-data sync fixes, Jan 2025 and Aug 2025 entries): https://steamcommunity.com/sharedfiles/filedetails/changelog/2785100219 — MEDIUM-HIGH confidence (developer-authored changelog)
- SubworldLibrary GitHub issues (open/closed, multiplayer and load-timing bugs): https://github.com/jjohnsnaill/SubworldLibrary/issues — MEDIUM confidence (community-reported, useful for MP-landmine awareness even though v1 is singleplayer-only)
- "Calamity Boss Resyncer" mod listing (evidence the Calamity-specific flag-loss bug persisted into 2025 despite SubworldLibrary updates): https://steamcommunity.com/sharedfiles/filedetails/?id=3417899539 — MEDIUM confidence (third-party workaround mod description)
- tModLoader Wiki, "Expert Cross Mod Content" (weak references, `[JITWhenModsEnabled]`, Mod.Call, reflection guidance): https://github.com/tModLoader/tModLoader/wiki/Expert-Cross-Mod-Content — HIGH confidence (official documentation)
- tModLoader Wiki, `build.txt` reference (`weakReferences`, `sortAfter`, version pinning syntax): https://github.com/tModLoader/tModLoader/wiki/build.txt — HIGH confidence (official documentation)
- tModLoader API docs, `Mod` class reference (`Code`/Assembly property, `AssemblyManager.GetLoadableTypes` warning, `Version`, `Call`): https://docs.tmodloader.net/docs/1.4-stable/class_terraria_1_1_mod_loader_1_1_mod.html — HIGH confidence (official generated API docs)
- CalamityMod source, `CalamityNetcode.cs` (`SyncWorld()` gated on `Main.dedServ`, confirming it's a safe no-op in singleplayer): https://github.com/CalamityTeam/CalamityModPublic/blob/master/CalamityNetcode.cs — HIGH confidence (primary source, directly read)
- Project's own prior research (`DESIGN_1.md`, referenced in `PROJECT.md`) on Calamity's `DownedBossSystem` wrapper-property pattern and Spirit's `MyWorld` statics — MEDIUM confidence (secondhand via project context, needs re-verification against installed DLLs at implementation time, already flagged as such in `PROJECT.md`)

---
*Pitfalls research for: tModLoader subworld boss-arena mod with soft cross-mod dependencies*
*Researched: 2026-08-12*
