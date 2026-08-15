using Terraria;
using Terraria.ModLoader;
using BossArenaSubWorld.Integrations;

namespace BossArenaSubWorld.Systems
{
    // SUBW-04: generic "replay the summon item's effect" mechanism. Vanilla summon items
    // (and the standard modded-item convention) all funnel into NPC.SpawnOnPlayer with a
    // hardcoded boss type -- calling that same primitive here, once, on arrival, is the
    // exact substitute for "replaying" the item's private, non-callable use-logic (see
    // 02-RESEARCH.md Pattern 2 for the full justification of this design choice).
    public class BossSummonPlayer : ModPlayer
    {
        // Set by Test1Tile.NewRightClick (Plan 02-02) right before SubworldSystem.Enter<>()
        // is called. Static + nulled-after-consume: this project is singleplayer-only
        // (Out of Scope: multiplayer per REQUIREMENTS.md), so a single static slot is safe.
        public static int? PendingBossNpcType;

        public override void OnEnterWorld()
        {
            if (!PendingBossNpcType.HasValue) return;
            // Boss-aware guard (generalized from the original SubworldSystem.IsActive<
            // BossArenaSubworld>() check -- see Systems/BossArenaRoutingRegistry.cs and
            // .planning/debug/resolved/hivemind-zonecorrupt-despawn-corruption-subworld.md):
            // accepts ANY registered boss-arena subworld type, not just the original plain one,
            // while still preserving Pitfall 3's intent (prevents re-summon on a later,
            // unrelated subworld entry belonging to some other mod).
            if (!BossArenaRoutingRegistry.IsAnyArenaActive()) return;

            // See .planning/debug/old-duke-immediate-despawn-plain-arena.md: InfernumMode's own
            // per-world "Infernum Mode" toggle resets to false inside this throwaway subworld --
            // force it true (via InfernumMode's sanctioned Mod.Call) before spawning, so Infernum's
            // boss AI overrides (and cross-mod compatibility checks other mods key off of) behave
            // correctly for every arena boss that actually depends on it.
            //
            // Only bosses explicitly flagged BossDefinition.RequiresInfernumToggle = true need
            // InfernumMode's toggle forced active (currently Providence/Profaned Guardians
            // absence-gating, Astrum Deus/Astrum Aureus forced-night presence-gating -- all set in
            // Integrations/CalamityIntegration.cs). This replaces the prior
            // bossKey.StartsWith("calamity:") string-prefix heuristic (quick task 260815-to6): a code
            // review flagged the prefix as a naming-convention proxy for the real condition, and
            // found a concrete gap -- catalyst:astrageldon (CalamityMod-dependent, but keyed
            // "catalyst:") was silently excluded from the fix despite being CalamityMod-adjacent.
            // Investigated and resolved (quick task 260815-u7g, decompiled Libs/InfernumMode.dll):
            // Astrageldon has NO InfernumMode dependency (zero references to it or CatalystMod
            // anywhere across InfernumMode's 2097 types), so its BossDefinition correctly leaves
            // RequiresInfernumToggle at its default false -- but the flag is now the explicit,
            // per-boss source of truth instead of an implicit key-prefix guess, so any future boss's
            // actual dependency is stated directly on its own BossDefinition.
            if (ModLoader.HasMod("InfernumMode")
                && BossRegistry.TryGetDefinitionForNpc(PendingBossNpcType.Value, out BossDefinition def)
                && def.RequiresInfernumToggle)
            {
                CalamityIntegration.ForceInfernumModeActiveInArena();
            }

            NPC.SpawnOnPlayer(Player.whoAmI, PendingBossNpcType.Value);
            PendingBossNpcType = null; // consume once -- Pitfall 3 guard: prevents
                                        // re-summon on a later, unrelated subworld entry
        }
    }
}
