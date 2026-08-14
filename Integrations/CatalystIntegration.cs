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
            SummonItemRegistry.Register(itemType, npcType);

            // No BossArenaRoutingRegistry.Register<T>() call -- Astrageldon.CheckActive()
            // explicitly returns false (never auto-despawns), and no player.Zone* reference
            // exists in its AI (06-RESEARCH.md). Falls back to the default
            // BossArenaSubworld automatically.

            BossRegistry.Register("catalyst:astrageldon", new BossDefinition(
                NpcTypes: new[] { npcType },
                ApplyDowned: ApplyAstrageldonDowned,
                IsDowned: IsAstrageldonDowned));
        }

        [JITWhenModsEnabled("CatalystMod")]
        private static bool IsAstrageldonDowned() => CatalystMod.WorldDefeats.downedAstrageldon;

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
