using Terraria;
using Terraria.ModLoader;
using BossArenaSubWorld.Systems;

namespace BossArenaSubWorld.Integrations
{
    public class CatalystIntegration : ModSystem
    {
        public override void PostSetupContent()
        {
            if (!ModLoader.HasMod("CatalystMod")) return;
            RegisterAstrageldon();
        }

        // D-01/Pitfall 2 (carried from Phase 4/5): every method below this point may
        // reference CatalystMod types; PostSetupContent() above never touches a CatalystMod
        // type directly, so it JITs safely regardless of whether CatalystMod is installed.
        // Note: CatalystMod itself hard-depends on CalamityMod (modReferences = CalamityMod
        // in its own build.txt) -- this is expected and does not change our own
        // weakReferences/[JITWhenModsEnabled] requirements (CalamityMod is already a weak
        // reference from Phase 4).
        [JITWhenModsEnabled("CatalystMod")]
        private void RegisterAstrageldon()
        {
            int itemType = ModContent.ItemType<CatalystMod.Items.SummonItems.AstralCommunicator>();
            int npcType = ModContent.NPCType<CatalystMod.NPCs.Boss.Astrageldon.Astrageldon>();

            // SummonItemRegistry/BossRegistry are boss-agnostic (int/string + delegates) --
            // zero changes needed to either existing file. Note: AstralCommunicator's REAL
            // UseItem() spawns an AstrageldonSpawner ritual projectile that later calls
            // NPC.NewNPC(...); our SUBW-04 pipeline bypasses UseItem() entirely and calls
            // NPC.SpawnOnPlayer directly (Phase 2 D-09), a purely cosmetic difference
            // (no ritual animation) -- already-precedented, not a new risk.
            //
            // User-requested scope addition (confirmed with user this session, decompiled
            // and verified directly against Libs/CatalystMod.dll via ilspycmd): the real
            // item's CanUseItem() becomes permanently unusable once NPC.downedMoonlord is
            // true and WorldDefeats.downedAstrageldon is still false (a Moon-Lord-lockout
            // gate; source mod shows a "broken" texture variant for this exact state). Our
            // portal-redirect pipeline bypasses CanUseItem()/UseItem() entirely by design
            // (Phase 2 D-09), so without this eligibility delegate the lockout would be
            // silently ignored. See CanSummonAstrageldon below.
            SummonItemRegistry.Register(itemType, npcType, CanSummonAstrageldon);

            // No BossArenaRoutingRegistry.Register<T>() call -- Astrageldon.CheckActive()
            // explicitly returns false (never auto-despawns), and no player.Zone* reference
            // exists in its AI (06-RESEARCH.md). Falls back to the default
            // BossArenaSubworld automatically.

            // Judgment call, investigated and resolved (quick task 260815-u7g): RequiresInfernumToggle
            // deliberately LEFT at its default (false) here. Astrageldon is a CatalystMod-native boss
            // (CatalystMod.NPCs.Boss.Astrageldon), not one of CalamityMod's own boss NPC types that
            // InfernumMode reworks. Confirmed via ilspycmd decompile of the installed
            // Libs/InfernumMode.dll (2097 types enumerated): zero references to "Astrageldon" or
            // "Catalyst" anywhere in the assembly -- InfernumMode has no AI override, hook, or type
            // reference targeting Astrageldon at all, unlike Providence/Profaned Guardians/Astrum
            // Deus/Astrum Aureus (all CalamityMod NPC types Infernum explicitly reworks). The Old
            // Duke precedent (NoxusBoss's FUCKYOUOLDDUKESystem hijacking Old Duke when InfernumMode's
            // toggle reads false) was also a NoxusBoss-specific special case hardcoded for Calamity's
            // own Old Duke NPC type, not a generic pattern -- no equivalent risk found for Astrageldon.
            BossRegistry.Register("catalyst:astrageldon", new BossDefinition(
                NpcTypes: new[] { npcType },
                ApplyDowned: ApplyAstrageldonDowned,
                IsDowned: IsAstrageldonDowned));
        }

        [JITWhenModsEnabled("CatalystMod")]
        private static bool IsAstrageldonDowned() => CatalystMod.WorldDefeats.downedAstrageldon;

        // User-requested scope addition (this session): replicates only the
        // Moon-Lord-lockout branch of the real AstralCommunicator.CanUseItem()
        // (decompiled and confirmed against Libs/CatalystMod.dll via ilspycmd this
        // session):
        //   if (Player.ZoneOverworldHeight && (calamityPlayer.ZoneAstral || catalystPlayer.ZoneBlight)
        //       && !NPC.AnyNPCs(ModContent.NPCType<Astrageldon>()) && <no active AstrageldonSpawner projectile>)
        //   {
        //       if (NPC.downedMoonlord) return WorldDefeats.downedAstrageldon;
        //       return true;
        //   }
        //   return false;
        // Once Moon Lord is downed, the real item becomes permanently unusable until
        // Astrageldon is actually defeated. Our portal-redirect pipeline
        // (Tiles/Test1Tile.cs) bypasses CanUseItem()/UseItem() entirely (Phase 2 D-09), so
        // this named delegate (registered via SummonItemRegistry.Register's optional
        // canSummon parameter above) is the only way to preserve that source-mod lockout
        // behavior. Deliberately NOT replicating the biome/location/anti-duplicate-spawn
        // checks -- those govern where/when the real item can be physically used in the
        // overworld, which has no equivalent in this project's portal-redirect flow.
        [JITWhenModsEnabled("CatalystMod")]
        private static bool CanSummonAstrageldon() =>
            !NPC.downedMoonlord || CatalystMod.WorldDefeats.downedAstrageldon;

        // D-03 (this phase): Astrageldon's downed-tracking path is world-scoped (ore-vein
        // WorldGen + vanilla's own SetEventFlagCleared helper), no player-scoped live-state
        // bookkeeping found in OnKill() (06-RESEARCH.md Pitfall 4). No exclusion logic
        // needed. Deliberately NOT replaying Main.BestiaryTracker.Kills.RegisterKill()/
        // SetKillCountDirectly() or the mid-fight downedAstrageldonPhase1 flag -- both
        // already fired live during the real subworld kill (same discipline as Hive Mind's
        // SetNewBossJustDowned(), Phase 4).
        [JITWhenModsEnabled("CatalystMod")]
        private static void ApplyAstrageldonDowned()
        {
            // Deviation from plan (Rule 3 - blocking compile error): 06-02-PLAN.md's
            // illustrative code referenced CatalystMod.MetanovaGenerator, but the plan's own
            // Interfaces block flagged this namespace as unverified. Decompiled directly via
            // ilspycmd against Libs/CatalystMod.dll: the real fully-qualified path is
            // CatalystMod.Common.World.MetanovaGenerator (confirmed public static
            // Generate(object state = null)).
            if (!CatalystMod.WorldDefeats.downedAstrageldon)
                CatalystMod.Common.World.MetanovaGenerator.Generate(); // WorldGen ore-vein generation (APPLY-03)

            // NOTE: gameEventId is -Type here, NOT -1 like every other boss registered so
            // far in this project -- replicate exactly, do not simplify to -1
            // (06-RESEARCH.md Pitfall 4 / Anti-Patterns to Avoid).
            NPC.SetEventFlagCleared(ref CatalystMod.WorldDefeats.downedAstrageldon,
                -ModContent.NPCType<CatalystMod.NPCs.Boss.Astrageldon.Astrageldon>());
        }
    }
}
