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
            RegisterKeeper();
            RegisterFowlEmperor();
            RegisterErhan();
            RegisterNebuleus();
            RegisterKingSlayer();
            RegisterEaglecrestGolem();
            RegisterAncientDeities();
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

        [JITWhenModsEnabled("Redemption")]
        private void RegisterKeeper()
        {
            int itemType = ModContent.ItemType<Redemption.Items.Usable.Summons.WeddingRing>();
            int keeperType = ModContent.NPCType<Redemption.NPCs.Bosses.Keeper.Keeper>();
            int spiritType = ModContent.NPCType<Redemption.NPCs.Bosses.Keeper.KeeperSpirit>();

            SummonItemRegistry.Register(itemType, keeperType);
            ForcedTimeSystem.RegisterForceNight(keeperType);

            BossRegistry.Register("redemption:keeper", new BossDefinition(
                NpcTypes: new[] { keeperType, spiritType },
                ApplyDowned: ApplyKeeperDowned,
                IsDowned: IsKeeperDowned));
        }
        [JITWhenModsEnabled("Redemption")]
        private static bool IsKeeperDowned() => Redemption.Globals.RedeBossDowned.downedKeeper;
        [JITWhenModsEnabled("Redemption")]
        private static void ApplyKeeperDowned()
        {
            if (!Redemption.Globals.RedeBossDowned.downedKeeper)
            {
                Redemption.UI.ChaliceAlignmentUI.BroadcastDialogue(
                    NetworkText.FromKey("Mods.Redemption.UI.Chalice.WeddingRing"), 240, 30, 0f, Color.DarkGoldenrod);
            }
            NPC.SetEventFlagCleared(ref Redemption.Globals.RedeBossDowned.downedKeeper, -1);
        }

        [JITWhenModsEnabled("Redemption")]
        private void RegisterFowlEmperor()
        {
            int itemType = ModContent.ItemType<Redemption.Items.Usable.Summons.EggCrown>();
            int npcType = ModContent.NPCType<Redemption.NPCs.Minibosses.FowlEmperor.FowlEmperor>();

            SummonItemRegistry.Register(itemType, npcType);

            BossRegistry.Register("redemption:fowl_emperor", new BossDefinition(
                NpcTypes: new[] { npcType },
                ApplyDowned: ApplyFowlEmperorDowned,
                IsDowned: IsFowlEmperorDowned));
        }
        [JITWhenModsEnabled("Redemption")]
        private static bool IsFowlEmperorDowned() => Redemption.Globals.RedeBossDowned.downedFowlEmperor;
        [JITWhenModsEnabled("Redemption")]
        private static void ApplyFowlEmperorDowned()
        {
            NPC.SetEventFlagCleared(ref Redemption.Globals.RedeBossDowned.downedFowlEmperor, -1);
        }

        [JITWhenModsEnabled("Redemption")]
        private void RegisterErhan()
        {
            int itemType = ModContent.ItemType<Redemption.Items.Usable.Summons.DemonScroll>();
            int impType = ModContent.NPCType<Redemption.NPCs.Bosses.Erhan.PalebatImp>();
            int erhanType = ModContent.NPCType<Redemption.NPCs.Bosses.Erhan.Erhan>();
            int spiritType = ModContent.NPCType<Redemption.NPCs.Bosses.Erhan.ErhanSpirit>();

            SummonItemRegistry.Register(itemType, impType);

            BossRegistry.Register("redemption:erhan", new BossDefinition(
                NpcTypes: new[] { impType, erhanType, spiritType },
                ApplyDowned: ApplyErhanDowned,
                IsDowned: IsErhanDowned));
        }
        [JITWhenModsEnabled("Redemption")]
        private static bool IsErhanDowned() => Redemption.Globals.RedeBossDowned.downedErhan;
        [JITWhenModsEnabled("Redemption")]
        private static void ApplyErhanDowned()
        {
            NPC.SetEventFlagCleared(ref Redemption.Globals.RedeBossDowned.downedErhan, -1);
        }

        [JITWhenModsEnabled("Redemption")]
        private void RegisterNebuleus()
        {
            int itemType = ModContent.ItemType<Redemption.Items.Usable.Summons.NebSummon>();
            int startType = ModContent.NPCType<Redemption.NPCs.Bosses.Neb.Neb_Start>();
            int nebType = ModContent.NPCType<Redemption.NPCs.Bosses.Neb.Nebuleus>();
            int neb2Type = ModContent.NPCType<Redemption.NPCs.Bosses.Neb.Phase2.Nebuleus2>();

            SummonItemRegistry.Register(itemType, startType);
            ForcedTimeSystem.RegisterForceNight(startType);

            BossRegistry.Register("redemption:nebuleus", new BossDefinition(
                NpcTypes: new[] { startType, nebType, neb2Type },
                ApplyDowned: ApplyNebuleusDowned,
                IsDowned: IsNebuleusDowned));
        }
        [JITWhenModsEnabled("Redemption")]
        private static bool IsNebuleusDowned() => Redemption.Globals.RedeBossDowned.downedNebuleus;
        [JITWhenModsEnabled("Redemption")]
        private static void ApplyNebuleusDowned()
        {
            NPC.SetEventFlagCleared(ref Redemption.Globals.RedeBossDowned.downedNebuleus, -1);
        }

        [JITWhenModsEnabled("Redemption")]
        private void RegisterKingSlayer()
        {
            int itemType = ModContent.ItemType<Redemption.Items.Usable.Summons.CyberTech>();
            int startType = ModContent.NPCType<Redemption.NPCs.Bosses.KSIII.KS3_Start>();
            int ks3Type = ModContent.NPCType<Redemption.NPCs.Bosses.KSIII.KS3>();

            SummonItemRegistry.Register(itemType, startType);

            BossRegistry.Register("redemption:king_slayer", new BossDefinition(
                NpcTypes: new[] { startType, ks3Type },
                ApplyDowned: ApplyKingSlayerDowned,
                IsDowned: IsKingSlayerDowned));
        }
        [JITWhenModsEnabled("Redemption")]
        private static bool IsKingSlayerDowned() => Redemption.Globals.RedeBossDowned.downedSlayer;
        [JITWhenModsEnabled("Redemption")]
        private static void ApplyKingSlayerDowned()
        {
            NPC.SetEventFlagCleared(ref Redemption.Globals.RedeBossDowned.downedSlayer, -1);
        }

        [JITWhenModsEnabled("Redemption")]
        private void RegisterEaglecrestGolem()
        {
            int itemType = ModContent.ItemType<Redemption.Items.Usable.Summons.EaglecrestSpelltome>();
            int golemType = ModContent.NPCType<Redemption.NPCs.Bosses.ADD.EaglecrestGolem2>();

            SummonItemRegistry.Register(itemType, golemType);

            BossRegistry.Register("redemption:eaglecrest_golem", new BossDefinition(
                NpcTypes: new[] { golemType },
                ApplyDowned: ApplyEaglecrestGolemDowned,
                IsDowned: IsEaglecrestGolemDowned));
        }
        [JITWhenModsEnabled("Redemption")]
        private static bool IsEaglecrestGolemDowned() => Redemption.Globals.RedeBossDowned.downedEaglecrestGolem;
        [JITWhenModsEnabled("Redemption")]
        private static void ApplyEaglecrestGolemDowned()
        {
            NPC.SetEventFlagCleared(ref Redemption.Globals.RedeBossDowned.downedEaglecrestGolem, -1);
        }

        [JITWhenModsEnabled("Redemption")]
        private void RegisterAncientDeities()
        {
            int itemType = ModContent.ItemType<Redemption.Items.Usable.Summons.AncientSigil>();
            int akkaType = ModContent.NPCType<Redemption.NPCs.Bosses.ADD.Akka>();
            int ukkoType = ModContent.NPCType<Redemption.NPCs.Bosses.ADD.Ukko>();

            SummonItemRegistry.Register(itemType, akkaType);

            BossRegistry.Register("redemption:ancient_deities", new BossDefinition(
                NpcTypes: new[] { akkaType, ukkoType },
                ApplyDowned: ApplyAncientDeitiesDowned,
                IsDowned: IsAncientDeitiesDowned));
        }
        [JITWhenModsEnabled("Redemption")]
        private static bool IsAncientDeitiesDowned() => Redemption.Globals.RedeBossDowned.downedADD;
        [JITWhenModsEnabled("Redemption")]
        private static void ApplyAncientDeitiesDowned()
        {
            NPC.SetEventFlagCleared(ref Redemption.Globals.RedeBossDowned.downedADD, -1);
        }
    }
}
