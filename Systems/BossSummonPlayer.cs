using Terraria;
using Terraria.ModLoader;
using BossArenaSubWorld.Integrations;

namespace BossArenaSubWorld.Systems
{
    // ENTRY-01: player-controlled summon timing. Entering the arena subworld no longer
    // auto-summons the boss immediately -- the player controls prep timing and uses the held
    // summon item directly whenever ready.
    //
    // OnEnterWorld retains the environment priming logic (e.g. InfernumMode per-world toggle
    // forcing via CalamityIntegration.ForceInfernumModeActiveInArena()) so Infernum-dependent
    // boss AI overrides are active when the player chooses to summon.
    public class BossSummonPlayer : ModPlayer
    {
        public override void OnEnterWorld()
        {
            if (!BossArenaRoutingRegistry.IsAnyArenaActive())
                return;

            if (ModLoader.HasMod("InfernumMode")
                && ForcedTimeSystem.ActiveArenaBossNpcType.HasValue
                && BossRegistry.TryGetDefinitionForNpc(ForcedTimeSystem.ActiveArenaBossNpcType.Value, out BossDefinition def)
                && def.RequiresInfernumToggle)
            {
                CalamityIntegration.ForceInfernumModeActiveInArena();
            }
        }
    }
}
