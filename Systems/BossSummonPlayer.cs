using Terraria;
using Terraria.Audio;
using Terraria.ID;
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
        private bool _justUsedSummon;

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

        public override void PostUpdate()
        {
            if (!BossArenaRoutingRegistry.IsAnyArenaActive())
                return;

            if (Player.whoAmI != Main.myPlayer)
                return;

            Item held = Player.HeldItem;
            if (held == null || held.IsAir)
            {
                _justUsedSummon = false;
                return;
            }

            if (!SummonItemRegistry.TryGetBoss(Player, held.type, out int bossNpcType))
            {
                _justUsedSummon = false;
                return;
            }

            if (NPC.AnyNPCs(bossNpcType))
            {
                _justUsedSummon = false;
                return;
            }

            // When player uses held summon item in the arena subworld, trigger spawn
            if (Player.controlUseItem)
            {
                if (!_justUsedSummon)
                {
                    _justUsedSummon = true;
                    SoundEngine.PlaySound(SoundID.Roar, Player.position);
                    NPC.SpawnOnPlayer(Player.whoAmI, bossNpcType);
                }
            }
            else
            {
                _justUsedSummon = false;
            }
        }
    }
}
