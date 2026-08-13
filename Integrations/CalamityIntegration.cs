using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using BossArenaSubWorld.Systems;

namespace BossArenaSubWorld.Integrations
{
    public class CalamityIntegration : ModSystem
    {
        public override void PostSetupContent()
        {
            if (!ModLoader.HasMod("CalamityMod")) return;
            RegisterHiveMind();
        }

        // D-01/Pitfall 2: every method below this point may reference CalamityMod
        // types; PostSetupContent() above never touches a Calamity type directly, so
        // it JITs safely regardless of whether CalamityMod is installed.
        [JITWhenModsEnabled("CalamityMod")]
        private void RegisterHiveMind()
        {
            int itemType = ModContent.ItemType<CalamityMod.Items.SummonItems.Teratoma>();
            int npcType = ModContent.NPCType<CalamityMod.NPCs.HiveMind.HiveMind>();

            // SummonItemRegistry/BossRegistry are boss-agnostic (int/string + delegates) --
            // zero changes needed to either existing file (Phase 2/Phase 3 code untouched).
            SummonItemRegistry.Register(itemType, npcType);

            BossRegistry.Register("calamity:hive_mind", new BossDefinition(
                NpcTypes: new[] { npcType },
                ApplyDowned: ApplyHiveMindDowned,
                IsDowned: () => CalamityMod.DownedBossSystem.downedHiveMind));
        }

        [JITWhenModsEnabled("CalamityMod")]
        private static void ApplyHiveMindDowned()
        {
            // Faithful replay of HiveMind.OnKill()'s exact conditional (checks BOTH
            // downedHiveMind and downedPerforator, matching Calamity's own source --
            // do not simplify to a single-flag check, per Pitfall 4 discipline: this
            // guard prevents double-enchanting already-enchanted ore once Perforator
            // is registered in a future phase).
            if (!CalamityMod.DownedBossSystem.downedHiveMind && !CalamityMod.DownedBossSystem.downedPerforator)
            {
                CalamityMod.World.AerialiteOreGen.Enchant();
                CalamityMod.CalamityUtils.BroadcastLocalizedText(
                    "Mods.CalamityMod.Status.Progression.SkyOreText", Color.Cyan);
            }
            CalamityMod.DownedBossSystem.downedHiveMind = true; // wrapper setter calls NPC.SetEventFlagCleared internally
            CalamityMod.CalamityNetcode.SyncWorld();
            // Deliberately NOT calling CalamityGlobalNPC.SetNewBossJustDowned() -- it is
            // player-scoped speedrun-timer bookkeeping that already ran for real on the
            // live NPC/players during the actual subworld kill (Subworld.NoPlayerSaving
            // = false means those ModPlayer field changes already survived the exit).
            // Replaying it here would double-apply per Pitfall 5. See 04-RESEARCH.md
            // "Important Correction to Prior Project Research" for the full trace.
        }
    }
}
