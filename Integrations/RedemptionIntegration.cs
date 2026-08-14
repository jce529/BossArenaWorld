using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Chat;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using BossArenaSubWorld.Systems;

namespace BossArenaSubWorld.Integrations
{
    public class RedemptionIntegration : ModSystem
    {
        public override void PostSetupContent()
        {
            if (!ModLoader.HasMod("Redemption")) return;
            RegisterThorn();
        }

        // D-01/Pitfall 2 (carried from Phase 4/5): every method below this point may
        // reference Redemption types; PostSetupContent() above never touches a Redemption
        // type directly, so it JITs safely regardless of whether Redemption is installed.
        [JITWhenModsEnabled("Redemption")]
        private void RegisterThorn()
        {
            int itemType = ModContent.ItemType<Redemption.Items.Usable.Summons.HeartOfThorns>();
            int npcType = ModContent.NPCType<Redemption.NPCs.Bosses.Thorn.Thorn>();

            // SummonItemRegistry/BossRegistry are boss-agnostic (int/string + delegates) --
            // zero changes needed to either existing file (Phase 2/3/4/5 code untouched).
            // No eligibility delegate for Thorn -- no equivalent lockout confirmed in
            // research (unlike CatalystMod's Astrageldon, see Integrations/CatalystIntegration.cs).
            SummonItemRegistry.Register(itemType, npcType);

            // No BossArenaRoutingRegistry.Register<T>() call -- confirmed no Zone*/CheckActive
            // override anywhere in Thorn's ~2900-line decompiled source (06-RESEARCH.md).
            // Falls back to the default BossArenaSubworld automatically.

            BossRegistry.Register("redemption:thorn", new BossDefinition(
                NpcTypes: new[] { npcType },
                ApplyDowned: ApplyThornDowned,
                IsDowned: IsThornDowned));
        }

        [JITWhenModsEnabled("Redemption")]
        private static bool IsThornDowned() => Redemption.Globals.RedeBossDowned.downedThorn;

        // D-03 (this phase): Thorn's downed-tracking path is fully world-scoped -- a chat
        // broadcast, a world Alignment change (RedeWorld.Alignment's setter syncs
        // internally), and a dialogue broadcast, none of which are player-scoped live-state
        // bookkeeping (unlike Calamity's SetNewBossJustDowned() in Phase 4). No exclusion
        // logic needed.
        [JITWhenModsEnabled("Redemption")]
        private static void ApplyThornDowned()
        {
            // Faithful replay of Thorn.OnKill()'s exact conditional and both net-mode
            // branches (singleplayer-only reachable in this project per REQUIREMENTS.md,
            // but included for source-fidelity per 06-RESEARCH.md Open Question 1's
            // recommendation -- harmless no-op branch in singleplayer).
            if (!Redemption.Globals.RedeBossDowned.downedThorn)
            {
                string text = Language.GetTextValue("Mods.Redemption.StatusMessage.Progression.ThornDowned");
                if (Main.netMode == NetmodeID.Server)
                    ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(text), new Color(50, 255, 130));
                else if (Main.netMode == NetmodeID.SinglePlayer)
                    Main.NewText(text, new Color(50, 255, 130));

                Redemption.Globals.RedeWorld.Alignment += 2; // setter syncs internally
                Redemption.UI.ChaliceAlignmentUI.BroadcastDialogue(
                    NetworkText.FromKey("Mods.Redemption.UI.Chalice.HeartOfThorns2"), 300, 30, 0f, Color.DarkGoldenrod);
            }
            // Matches this project's established -1 convention (King Slime/Hive Mind/Infernon).
            NPC.SetEventFlagCleared(ref Redemption.Globals.RedeBossDowned.downedThorn, -1);
        }
    }
}
