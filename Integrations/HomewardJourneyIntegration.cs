using Terraria;
using Terraria.ModLoader;
using BossArenaSubWorld.Systems;

namespace BossArenaSubWorld.Integrations
{
    // File named after the mod's Workshop display name ("Homeward Journey"), matching how
    // CalamityIntegration.cs/SpiritIntegration.cs/etc. are named after what the mod is
    // *called* -- but every ModLoader.HasMod(...)/[JITWhenModsEnabled(...)] call below uses
    // the mod's actual internal name "ContinentOfJourney" (confirmed via decompile,
    // 07-RESEARCH.md Pitfall 1: "HomewardJourney" would silently no-op registration).
    public class HomewardJourneyIntegration : ModSystem
    {
        public override void PostSetupContent()
        {
            if (!ModLoader.HasMod("ContinentOfJourney")) return;
            RegisterGoblinChariot();
            RegisterBigDipper();
            RegisterDiverBoss();
            RegisterWallOfShadow();
        }

        // D-01/Pitfall 2 (carried from Phase 4/5/6): every method below this point may
        // reference ContinentOfJourney types; PostSetupContent() above never touches a
        // ContinentOfJourney type directly, so it JITs safely regardless of whether the mod
        // is installed.
        [JITWhenModsEnabled("ContinentOfJourney")]
        private void RegisterGoblinChariot()
        {
            int itemType = ModContent.ItemType<ContinentOfJourney.Items.PurpleFlareGun>();
            int npcType = ModContent.NPCType<ContinentOfJourney.NPCs.Boss_GoblinChariot.GoblinChariot>();

            // SummonItemRegistry/BossRegistry are boss-agnostic (int/string + delegates) --
            // zero changes needed to either existing file (Phase 2/3/4/5/6 code untouched).
            // No canSummon eligibility delegate -- PurpleFlareGun's real CanUseItem() only
            // checks !NPC.AnyNPCs(...) (no biome/location/structure gate, confirmed via
            // decompile), so this project's UseItem()-bypassing pipeline (Phase 2 D-09)
            // loses nothing by skipping it.
            SummonItemRegistry.Register(itemType, npcType);

            // No BossArenaRoutingRegistry.Register<T>() call -- confirmed no Zone*/CheckActive
            // override anywhere in Goblin Chariot's ~1940-line decompiled source, and no
            // wiki-stated biome exists to apply D-05's thematic-assignment principle to
            // ("summoned at any time" per terrariamods.wiki.gg, cross-checked against source).
            // Falls back to the default BossArenaSubworld automatically -- same as vanilla
            // King Slime.
            BossRegistry.Register("continentofjourney:goblin_chariot", new BossDefinition(
                NpcTypes: new[] { npcType },
                ApplyDowned: ApplyGoblinChariotDowned,
                IsDowned: IsGoblinChariotDowned));
        }

        [JITWhenModsEnabled("ContinentOfJourney")]
        private static bool IsGoblinChariotDowned() =>
            ContinentOfJourney.DownedBossSystem.downedGoblinChariot;

        // Goblin Chariot's downed-tracking path is fully world-scoped -- a plain flag write,
        // no chat broadcast, no netcode call beyond what SetEventFlagCleared itself does, no
        // WorldGen side effect, no player-scoped bookkeeping anywhere (confirmed via a
        // full-project grep for "downedGoblinChariot": this OnKill() is the ONLY write site
        // in the entire decompiled assembly; every other reference is a read -- BossChecklist
        // integration via CoJ_BossChecklist.cs, a Fishmen Free Market trading-post unlock
        // condition, and DownedBossSystem's own save/load/NetSend/NetReceive bookkeeping).
        // No exclusion logic needed (same discipline as Spirit's Infernon, Phase 5 D-03).
        [JITWhenModsEnabled("ContinentOfJourney")]
        private static void ApplyGoblinChariotDowned()
        {
            // Faithful replay of GoblinChariot.OnKill() -- the entire method body in the real
            // source is exactly this one line (07-RESEARCH.md Pattern 1). Matches this
            // project's established -1 convention (King Slime/Hive Mind/Infernon/Thorn).
            NPC.SetEventFlagCleared(ref ContinentOfJourney.DownedBossSystem.downedGoblinChariot, -1);
        }

        [JITWhenModsEnabled("ContinentOfJourney")]
        private void RegisterBigDipper()
        {
            int itemType = ModContent.ItemType<ContinentOfJourney.Items.MaliciousPacket>();
            int npcType = ModContent.NPCType<ContinentOfJourney.NPCs.Boss_BigDipper.BigDipper>();

            SummonItemRegistry.Register(itemType, npcType);
            BossArenaRoutingRegistry.Register<Subworlds.BossArenaSpaceSubworld>(npcType);

            BossRegistry.Register("continentofjourney:big_dipper", new BossDefinition(
                NpcTypes: new[] { npcType },
                ApplyDowned: ApplyBigDipperDowned,
                IsDowned: IsBigDipperDowned));
        }
        [JITWhenModsEnabled("ContinentOfJourney")]
        private static bool IsBigDipperDowned() => ContinentOfJourney.DownedBossSystem.downedBigDipper;
        [JITWhenModsEnabled("ContinentOfJourney")]
        private static void ApplyBigDipperDowned()
        {
            NPC.SetEventFlagCleared(ref ContinentOfJourney.DownedBossSystem.downedBigDipper, -1);
        }

        [JITWhenModsEnabled("ContinentOfJourney")]
        private void RegisterDiverBoss()
        {
            int itemType = ModContent.ItemType<ContinentOfJourney.Items.CyanFlareGun>();
            int npcType = ModContent.NPCType<ContinentOfJourney.NPCs.Boss_Diver.Diver>();

            SummonItemRegistry.Register(itemType, npcType);

            BossRegistry.Register("continentofjourney:diver", new BossDefinition(
                NpcTypes: new[] { npcType },
                ApplyDowned: ApplyDiverBossDowned,
                IsDowned: IsDiverBossDowned));
        }
        [JITWhenModsEnabled("ContinentOfJourney")]
        private static bool IsDiverBossDowned() => ContinentOfJourney.DownedBossSystem.downedDiverBoss;
        [JITWhenModsEnabled("ContinentOfJourney")]
        private static void ApplyDiverBossDowned()
        {
            NPC.SetEventFlagCleared(ref ContinentOfJourney.DownedBossSystem.downedDiverBoss, -1);
        }

        [JITWhenModsEnabled("ContinentOfJourney")]
        private void RegisterWallOfShadow()
        {
            int itemType = ModContent.ItemType<ContinentOfJourney.Items.UltimateTorch>();
            int npcType = ModContent.NPCType<ContinentOfJourney.NPCs.Boss_WallofShadow.WallofShadow>();

            SummonItemRegistry.Register(itemType, npcType);

            BossRegistry.Register("continentofjourney:wall_of_shadow", new BossDefinition(
                NpcTypes: new[] { npcType },
                ApplyDowned: ApplyWallOfShadowDowned,
                IsDowned: IsWallOfShadowDowned));
        }
        [JITWhenModsEnabled("ContinentOfJourney")]
        private static bool IsWallOfShadowDowned() => ContinentOfJourney.DownedBossSystem.downedBarrier;
        [JITWhenModsEnabled("ContinentOfJourney")]
        private static void ApplyWallOfShadowDowned()
        {
            if (!ContinentOfJourney.DownedBossSystem.downedBarrier)
            {
                Main.NewText(Terraria.Localization.Language.GetTextValue("Mods.ContinentOfJourney.DownedBarrier"), 50, 255, 130);
            }
            NPC.SetEventFlagCleared(ref ContinentOfJourney.DownedBossSystem.downedBarrier, -1);
        }
    }
}
