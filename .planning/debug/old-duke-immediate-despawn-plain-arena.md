---
status: awaiting_human_verify
trigger: "Investigate issue: old-duke-immediate-despawn-plain-arena -- The Old Duke (Calamity + Infernum) despawns immediately after spawning in the default BossArenaSubworld (plain stone arena), during live verification of Phase 10 Plan 10-06."
created: 2026-08-14T15:00:00Z
updated: 2026-08-14T16:00:00Z
---

## Current Focus

hypothesis: CONFIRMED (see Resolution below). Root cause is NOT a Sulphurous-Sea Zone dependency at all (10-RESEARCH.md's narrow claim was technically correct) -- it's a cross-mod subworld-isolation bug: NoxusBoss ("Wrath of the Gods", installed+enabled but explicitly out of this project's integration scope) hijacks any OldDuke NPC's AI into a disappearing "Avatar of Emptiness" cutscene whenever InfernumMode's own per-world "Infernum Mode" toggle (`WorldSaveSystem.InfernumModeEnabled`) reads false -- and that flag resets to false inside BossArenaSubworld because it's a throwaway, unsaved subworld (same Pitfall-1 category as the resolved hivemind-zonecorrupt-despawn-corruption-subworld.md case).
test: N/A -- confirmed via decompile chain, fix implemented and build-verified.
expecting: N/A.
next_action: Awaiting live in-game re-verification from user (human-verify checkpoint).

## Symptoms

expected: The Old Duke should spawn via BloodwormPlatter -> Test1 redirect and remain active for a normal boss fight in the default plain-stone BossArenaSubworld, per 10-RESEARCH.md's decompile finding ("OldDuke.cs's full decompiled AI has zero references to any Sulphurous-Sea Zone flag" -> concluded safe on plain arena, no biome routing built, no BossArenaRoutingRegistry call for Old Duke in Integrations/CalamityIntegration.cs's RegisterOldDuke()).
actual: The Old Duke NPC does spawn (redirect + auto-summon succeed), but it despawns immediately right after appearing -- no fight is possible.
errors: None reported yet (no client.log excerpt captured) -- worth checking Logs/client.log for any despawn-related message during the investigation.
reproduction: With InfernumMode + CalamityMod enabled, hold BloodwormPlatter, right-click the Test1 portal tile -> enter BossArenaSubworld -> The Old Duke auto-summons -> despawns almost immediately after spawning.
started: First live test of The Old Duke ever (Plan 10-05 registered it this session, Plan 10-06 is the first live verification attempt) -- never worked, this is not a regression.

## Eliminated

- hypothesis: "Base CalamityMod.NPCs.OldDuke.OldDuke.AI() has an undocumented Sulphurous-Sea/liquid despawn check that 10-RESEARCH.md's grep-for-'Zone' pass missed."
  evidence: Full decompile of CalamityMod.dll's OldDuke.AI() (2941 lines) via ilspycmd -t. Only despawn-adjacent logic found: a target-invalid/far-away branch (`val.dead || Distance > 8800f`) that caps timeLeft to 10 -- requires the TARGET PLAYER to be dead or 550+ tiles away, not applicable when player summons the boss on themselves. The `flag10` world-bounds check (Y<300 or Y>worldSurface or X near map edges) only toggles an enrage/DR buff, never despawns. `SpawnModBiomes = SulphurousSeaBiome` (line 141) is bestiary-listing metadata only, confirmed not read anywhere else in the class. No `wet`/`InSulphur`/`ZoneSulphur` reference exists anywhere in the AI. 10-RESEARCH.md's narrow claim was technically correct -- it just didn't look at InfernumMode's override (its own research explicitly flagged InfernumMode.dll as "not yet decompiled this session").
  timestamp: 2026-08-14T15:15:00Z

- hypothesis: "InfernumMode's own OldDukeBehaviorOverride (which fully replaces Old Duke's AI when InfernumMode is loaded) has a despawn condition tied to Sulphurous Sea, liquid, or an Acid Rain event-active check."
  evidence: Full decompile of InfernumMode.dll's `OldDukeBehaviorOverride.PreAI` (1657 lines) via ilspycmd -t. Only despawn path found: a fade-out-then-`active=false` branch gated on `!val.active || val.dead || !WithinRange(npc.Center, 6800f)` (target player invalid/dead/550+ tiles away) -- doesn't fire when the player is standing next to the boss. No Zone/biome/wet/liquid check anywhere in the 1657-line override. `AcidRainEvent`-related code only appears in the base Calamity AI's far-away-target branch (irrelevant here). Not the cause.
  timestamp: 2026-08-14T15:25:00Z

## Evidence

- timestamp: 2026-08-14T15:35:00Z
  checked: D:\SteamLibrary\steamapps\common\tModLoader\tModLoader-Logs\client.log (the real, just-updated log from the user's actual test session) for exceptions/errors during the reported despawn.
  found: No exceptions or stack traces during gameplay (only 3 unrelated SpiritMod startup patch-failure warnings). But the log's mod-load section shows `NoxusBoss (Calamity: Wrath of the Gods) v1.2.31` is ENABLED in the current mod list, and logs `Hook CalamityMod.BiomeManagers.SulphurousSeaBiome::IsBiomeActive(Player) added by NoxusBoss` plus `ILHook Terraria.NPC::UpdateNPC_Inner(int) added by NoxusBoss`.
  implication: NoxusBoss -- explicitly removed from this project's v1 integration scope per STATE.md Roadmap Evolution ("NoxusBoss (Devourer of Universes) removed from v1 scope entirely") -- is nonetheless installed and ENABLED in the player's actual mod list, and it patches Sulphurous-Sea/NPC-update-related vanilla code. This is a real environment factor this project's own registration code cannot see or guard against via `[JITWhenModsEnabled]`, since NoxusBoss's hooks are global (IL-hook the vanilla NPC update loop directly), not scoped to a specific NPC type check in our code.

- timestamp: 2026-08-14T15:40:00Z
  checked: `ilspycmd -l c` against the real NoxusBoss DLL (`ModSources/ModAssemblies/NoxusBoss_v1.2.31.dll`, extracted from the actually-installed Workshop copy) for any Old-Duke-named type.
  found: `NoxusBoss.Core.World.GameScenes.OldDukeDeath.FUCKYOUOLDDUKESystem` -- a `ModSystem` that registers `GlobalNPCEventHandlers.PreAIEvent += KillOldDukeWrapper` in `OnModLoad()`.
  implication: NoxusBoss has a dedicated, global AI hijack specifically targeting Old Duke NPCs, independent of which mod spawns them.

- timestamp: 2026-08-14T15:42:00Z
  checked: Full decompile of `FUCKYOUOLDDUKESystem.KillOldDukeWrapper`/`DoBehavior_OldDukeAI` via ilspycmd -t.
  found: |
    ```
    private bool KillOldDukeWrapper(NPC npc) {
        if (InfernumCompatibilitySystem.InfernumModeIsActive) return true; // let real AI run
        if (npc.type == OldDukeID && !BossDownedSaveSystem.HasDefeated<AvatarOfEmptiness>() && !WorldSaveSystem.AvatarHasKilledOldDuke) {
            DoBehavior_OldDukeAI(npc);
            return false; // block real AI entirely
        }
        return true;
    }
    ```
    `DoBehavior_OldDukeAI` sets `npc.dontTakeDamage = true; npc.damage = 0;`, floats the boss upward, and after `RiftSummonDelay(19) + AttackDelay(218) = 237` ticks (~4 seconds) shrinks `npc.scale` by 0.12/tick until it reaches 0, then sets `npc.active = false` -- a scripted "Avatar of Emptiness" lore cutscene, not a real fight.
  implication: This exactly matches the reported symptom ("spawns, then despawns almost immediately" -- ~4 seconds is easily perceived as "almost immediate", and the boss is undamageable/harmless the whole time so it doesn't feel like a real encounter). The ONLY gate that skips this hijack is `InfernumCompatibilitySystem.InfernumModeIsActive`.

- timestamp: 2026-08-14T15:45:00Z
  checked: Decompiled `InfernumCompatibilitySystem.InfernumModeIsActive` (NoxusBoss.dll) -> `InfernumMode.CanUseCustomAIs` (InfernumMode.dll) -> `WorldSaveSystem.InfernumModeEnabled` (InfernumMode.dll), tracing the full call chain.
  found: |
    `InfernumModeIsActive => ModReferences.InfernumMod.Call("GetInfernumActive")` (NoxusBoss's sanctioned cross-mod compatibility check).
    `GetInfernumActiveModCall.SafeProcess => InfernumMode.CanUseCustomAIs`.
    `InfernumMode.CanUseCustomAIs => WorldSaveSystem.InfernumModeEnabled` (InfernumMode.InfernumMode.cs line 49).
    `WorldSaveSystem.InfernumModeEnabled` is a `ModSystem`-backed, per-world-saved bool (InfernumMode's own in-game "Infernum Mode" toggle, distinct from just having the mod installed) with `SaveWorldData`/`LoadWorldData` overrides: `LoadWorldData(TagCompound tag) { InfernumModeEnabled = list.Contains("InfernumModeActive"); ... }` where `list = tag.GetList<string>("downed")` -- an UNCONDITIONAL overwrite from whatever TagCompound is loaded, same structural shape as `CalamityMod.DownedBossSystem.LoadWorldData()` in the already-resolved Hive Mind case.
  implication: |
    Full mechanism confirmed. `BossArenaSubworld` is a freshly-generated, `ShouldSave = false` throwaway subworld -- its `LoadWorldData()` call fires against an empty/default `TagCompound` (no `"InfernumModeActive"` entry), so `WorldSaveSystem.InfernumModeEnabled` resets to `false` inside the arena REGARDLESS of the player's real main-world toggle state. This is the exact same "world-scoped modded flag does not survive the subworld round-trip" category already documented in `.planning/debug/resolved/hivemind-zonecorrupt-despawn-corruption-subworld.md` (Pitfall 1), just affecting a THIRD mod's (InfernumMode's) own state, further weaponized by a FOURTH mod (NoxusBoss) that isn't even integrated by this project. With `InfernumModeEnabled == false` inside the arena, `InfernumMode.CanUseCustomAIs` is false, so (a) Infernum's own `NPCBehaviorOverride` system for Old Duke doesn't apply (harmless on its own -- confirmed above base Calamity AI doesn't despawn either), but (b) NoxusBoss's `KillOldDukeWrapper` hijack DOES fire, producing the observed despawn.

- timestamp: 2026-08-14T15:48:00Z
  checked: `InfernumMode.Core.ModCalls.InfernumCalls.SetInfernumActiveModCall` (InfernumMode.dll) -- confirming a sanctioned, official cross-mod API exists for exactly this purpose.
  found: `SetInfernumActiveModCall.SafeProcess { WorldSaveSystem.InfernumModeEnabled = (bool)argsWithoutCommand[0]; }`, registered under call command `"SetInfernumActive"`. This is InfernumMode's own public `Mod.Call` API, explicitly designed for other mods (including NoxusBoss itself, which reads it) to set this flag -- not an unsupported reflection hack.
  implication: The correct, sanctioned fix is to call `ModLoader.GetMod("InfernumMode").Call("SetInfernumActive", true)` once per arena visit (on entry), not a raw reflection write to a private field. No restore-on-exit is needed: per the Hive Mind precedent's established mechanism, the real main world's own save data correctly reloads the real `InfernumModeEnabled` value when `SubworldSystem.Exit()` reloads the main `.wld` -- the arena's forced-true value is discarded along with the rest of the throwaway subworld's state.

## Resolution

root_cause: |
  `WorldSaveSystem.InfernumModeEnabled` (InfernumMode's own per-world-saved "Infernum Mode" toggle, read by `InfernumMode.CanUseCustomAIs`) resets to `false` inside `BossArenaSubworld` because it is a freshly-generated, `ShouldSave = false` throwaway subworld whose `LoadWorldData()` call receives an empty TagCompound -- the exact same "world-scoped modded flag does not survive the subworld round-trip" category already documented in `.planning/debug/resolved/hivemind-zonecorrupt-despawn-corruption-subworld.md` (Pitfall 1), here affecting a third mod's (InfernumMode's) state instead of Calamity's.

  With that flag false inside the arena, NoxusBoss ("Calamity: Wrath of the Gods" v1.2.31 -- installed and enabled in the player's mod list, but explicitly out of this project's own v1 integration scope per STATE.md) hijacks Old Duke's AI via its global `GlobalNPCEventHandlers.PreAIEvent` handler (`NoxusBoss.Core.World.GameScenes.OldDukeDeath.FUCKYOUOLDDUKESystem.KillOldDukeWrapper`), which only yields to the real AI when `InfernumCompatibilitySystem.InfernumModeIsActive` (== `InfernumMode.CanUseCustomAIs` via a sanctioned `Mod.Call("GetInfernumActive")`) is true. Since it reads false inside the arena, Old Duke is replaced with a scripted "Avatar of Emptiness" cutscene: undamageable, harmless, floats upward, and after ~237 ticks (~4 seconds) shrinks to nothing and sets `npc.active = false` -- exactly matching the reported "spawns, then despawns almost immediately" symptom.

  10-RESEARCH.md's original claim ("OldDuke.cs's AI has zero Sulphurous-Sea Zone references, safe on plain arena") was independently re-verified and is technically correct -- neither CalamityMod's base OldDuke AI nor InfernumMode's own OldDukeBehaviorOverride has any biome/Zone-based despawn. The real root cause is a cross-mod subworld-isolation interaction entirely outside Old Duke's own AI, involving a fourth mod (NoxusBoss) this project deliberately never integrated.
fix: |
  Force `WorldSaveSystem.InfernumModeEnabled` true for the duration of any BossArenaSubWorld arena visit, via InfernumMode's own sanctioned `Mod.Call("SetInfernumActive", true)` API (not a reflection hack), so both InfernumMode's real boss AI overrides and NoxusBoss's Old-Duke-hijack bypass apply correctly inside the arena, matching the player's real cross-mod setup intent. Added as a new `[JITWhenModsEnabled("InfernumMode")]`-tagged helper in `Integrations/CalamityIntegration.cs` (`ForceInfernumModeActiveInArena()`), called once from `Systems/BossSummonPlayer.cs`'s `OnEnterWorld()` (guarded by `ModLoader.HasMod("InfernumMode")`) right before `NPC.SpawnOnPlayer(...)`, so it applies to every boss arena entry, not just Old Duke's. No restore-on-exit needed -- the main world's own save data correctly reloads the real value when `SubworldSystem.Exit()` reloads the main `.wld` (same conclusion as the Hive Mind precedent).
verification: |
  Build verification: `dotnet build BossArenaSubWorld.csproj` -- PASS (0 warnings, 0 errors).
  Live verification: pending user re-test (human-verify checkpoint) -- re-enter the arena with BloodwormPlatter and confirm The Old Duke stays alive, damageable, and fightable for a normal fight duration (not just past the ~4-second cutscene window).
files_changed:
  - "Integrations/CalamityIntegration.cs"
  - "Systems/BossSummonPlayer.cs"
