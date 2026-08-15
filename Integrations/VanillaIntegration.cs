using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using BossArenaSubWorld.Subworlds;
using BossArenaSubWorld.Systems;

namespace BossArenaSubWorld.Integrations
{
    public class VanillaIntegration : ModSystem
    {
        public override void PostSetupContent()
        {
            RegisterKingSlime();
            RegisterEyeOfCthulhu();
            RegisterEaterOfWorlds();
            RegisterBrainOfCthulhu();
            RegisterQueenBee();
            RegisterDeerclops();
            RegisterWallOfFlesh();
            RegisterSkeletron();
            RegisterQueenSlime();
            RegisterTheTwins();
            RegisterTheDestroyer();
            RegisterSkeletronPrime();
            RegisterDukeFishron();
            RegisterEmpressOfLight();
            RegisterGolem();
            RegisterPlantera();
            RegisterCultist();
            RegisterMoonLord();
        }

        private static void RegisterKingSlime()
        {
            SummonItemRegistry.Register(ItemID.SlimeCrown, NPCID.KingSlime);
            BossRegistry.Register("vanilla:king_slime", new BossDefinition(
                NpcTypes: new int[] { NPCID.KingSlime },
                ApplyDowned: () => NPC.SetEventFlagCleared(ref NPC.downedSlimeKing, -1),
                IsDowned: () => NPC.downedSlimeKing));
        }

        private static void RegisterEyeOfCthulhu()
        {
            SummonItemRegistry.Register(ItemID.SuspiciousLookingEye, NPCID.EyeofCthulhu);
            ForcedTimeSystem.RegisterForceNight(NPCID.EyeofCthulhu);
            BossRegistry.Register("vanilla:eye_of_cthulhu", new BossDefinition(
                NpcTypes: new int[] { NPCID.EyeofCthulhu },
                ApplyDowned: () => NPC.SetEventFlagCleared(ref NPC.downedBoss1, -1),
                IsDowned: () => NPC.downedBoss1));
        }

        private static void RegisterEaterOfWorlds()
        {
            SummonItemRegistry.Register(ItemID.WormFood, NPCID.EaterofWorldsHead);
            BossArenaRoutingRegistry.Register<BossArenaCorruptionSubworld>(NPCID.EaterofWorldsHead);
            BossRegistry.Register("vanilla:eater_of_worlds", new BossDefinition(
                NpcTypes: new int[] { NPCID.EaterofWorldsHead, NPCID.EaterofWorldsBody, NPCID.EaterofWorldsTail },
                ApplyDowned: () => NPC.SetEventFlagCleared(ref NPC.downedBoss2, -1),
                IsDowned: () => NPC.downedBoss2));
        }

        private static void RegisterBrainOfCthulhu()
        {
            SummonItemRegistry.Register(ItemID.BloodySpine, NPCID.BrainofCthulhu);
            BossArenaRoutingRegistry.Register<BossArenaCorruptionSubworld>(NPCID.BrainofCthulhu);
            BossRegistry.Register("vanilla:brain_of_cthulhu", new BossDefinition(
                NpcTypes: new int[] { NPCID.BrainofCthulhu, NPCID.Creeper },
                ApplyDowned: () => NPC.SetEventFlagCleared(ref NPC.downedBoss2, -1),
                IsDowned: () => NPC.downedBoss2));
        }

        private static void RegisterQueenBee()
        {
            SummonItemRegistry.Register(ItemID.Abeemination, NPCID.QueenBee);
            BossArenaRoutingRegistry.Register<BossArenaJungleSubworld>(NPCID.QueenBee);
            BossRegistry.Register("vanilla:queen_bee", new BossDefinition(
                NpcTypes: new int[] { NPCID.QueenBee },
                ApplyDowned: () => NPC.SetEventFlagCleared(ref NPC.downedQueenBee, -1),
                IsDowned: () => NPC.downedQueenBee));
        }

        private static void RegisterDeerclops()
        {
            SummonItemRegistry.Register(ItemID.DeerThing, NPCID.Deerclops);
            BossRegistry.Register("vanilla:deerclops", new BossDefinition(
                NpcTypes: new int[] { NPCID.Deerclops },
                ApplyDowned: () => NPC.SetEventFlagCleared(ref NPC.downedDeerclops, -1),
                IsDowned: () => NPC.downedDeerclops));
        }

        private static void RegisterWallOfFlesh()
        {
            SummonItemRegistry.Register(ItemID.GuideVoodooDoll, NPCID.WallofFlesh);
            BossArenaRoutingRegistry.Register<BossArenaUnderworldSubworld>(NPCID.WallofFlesh);
            BossRegistry.Register("vanilla:wall_of_flesh", new BossDefinition(
                NpcTypes: new int[] { NPCID.WallofFlesh, NPCID.WallofFleshEye, NPCID.TheHungry, NPCID.TheHungryII },
                ApplyDowned: () => NPC.SetEventFlagCleared(ref Main.hardMode, -1),
                IsDowned: () => Main.hardMode));
        }

        private static void RegisterSkeletron()
        {
            SummonItemRegistry.Register(ItemID.ClothierVoodooDoll, NPCID.SkeletronHead);
            ForcedTimeSystem.RegisterForceNight(NPCID.SkeletronHead);
            BossRegistry.Register("vanilla:skeletron", new BossDefinition(
                NpcTypes: new int[] { NPCID.SkeletronHead, NPCID.SkeletronHand },
                ApplyDowned: () => NPC.SetEventFlagCleared(ref NPC.downedBoss3, -1),
                IsDowned: () => NPC.downedBoss3));
        }

        private static void RegisterQueenSlime()
        {
            SummonItemRegistry.Register(ItemID.QueenSlimeCrystal, NPCID.QueenSlimeBoss);
            BossArenaRoutingRegistry.Register<BossArenaHallowSubworld>(NPCID.QueenSlimeBoss);
            BossRegistry.Register("vanilla:queen_slime", new BossDefinition(
                NpcTypes: new int[] { NPCID.QueenSlimeBoss },
                ApplyDowned: () => NPC.SetEventFlagCleared(ref NPC.downedQueenSlime, -1),
                IsDowned: () => NPC.downedQueenSlime));
        }

        private static void RegisterTheTwins()
        {
            SummonItemRegistry.Register(ItemID.MechanicalEye, NPCID.Retinazer);
            ForcedTimeSystem.RegisterForceNight(NPCID.Retinazer);
            BossRegistry.Register("vanilla:the_twins", new BossDefinition(
                NpcTypes: new int[] { NPCID.Retinazer, NPCID.Spazmatism },
                ApplyDowned: () => NPC.SetEventFlagCleared(ref NPC.downedMechBoss2, -1),
                IsDowned: () => NPC.downedMechBoss2));
        }

        private static void RegisterTheDestroyer()
        {
            SummonItemRegistry.Register(ItemID.MechanicalWorm, NPCID.TheDestroyer);
            ForcedTimeSystem.RegisterForceNight(NPCID.TheDestroyer);
            BossRegistry.Register("vanilla:the_destroyer", new BossDefinition(
                NpcTypes: new int[] { NPCID.TheDestroyer, NPCID.TheDestroyerBody, NPCID.TheDestroyerTail, NPCID.Probe },
                ApplyDowned: () => NPC.SetEventFlagCleared(ref NPC.downedMechBoss1, -1),
                IsDowned: () => NPC.downedMechBoss1));
        }

        private static void RegisterSkeletronPrime()
        {
            SummonItemRegistry.Register(ItemID.MechanicalSkull, NPCID.SkeletronPrime);
            ForcedTimeSystem.RegisterForceNight(NPCID.SkeletronPrime);
            BossRegistry.Register("vanilla:skeletron_prime", new BossDefinition(
                NpcTypes: new int[] { NPCID.SkeletronPrime, NPCID.PrimeCannon, NPCID.PrimeSaw, NPCID.PrimeVice, NPCID.PrimeLaser },
                ApplyDowned: () => NPC.SetEventFlagCleared(ref NPC.downedMechBoss3, -1),
                IsDowned: () => NPC.downedMechBoss3));
        }

        private static void RegisterDukeFishron()
        {
            SummonItemRegistry.Register(ItemID.TruffleWorm, NPCID.DukeFishron);
            BossRegistry.Register("vanilla:duke_fishron", new BossDefinition(
                NpcTypes: new int[] { NPCID.DukeFishron },
                ApplyDowned: () => NPC.SetEventFlagCleared(ref NPC.downedFishron, -1),
                IsDowned: () => NPC.downedFishron));
        }

        private static void RegisterEmpressOfLight()
        {
            SummonItemRegistry.Register(ItemID.EmpressButterfly, NPCID.HallowBoss);
            BossArenaRoutingRegistry.Register<BossArenaHallowSubworld>(NPCID.HallowBoss);
            ForcedTimeSystem.RegisterForceNight(NPCID.HallowBoss);
            BossRegistry.Register("vanilla:empress_of_light", new BossDefinition(
                NpcTypes: new int[] { NPCID.HallowBoss },
                ApplyDowned: () => NPC.SetEventFlagCleared(ref NPC.downedEmpressOfLight, -1),
                IsDowned: () => NPC.downedEmpressOfLight));
        }

        private static void RegisterGolem()
        {
            SummonItemRegistry.Register(ItemID.LihzahrdPowerCell, NPCID.Golem);
            BossArenaRoutingRegistry.Register<BossArenaJungleSubworld>(NPCID.Golem);
            BossRegistry.Register("vanilla:golem", new BossDefinition(
                NpcTypes: new int[] { NPCID.Golem, NPCID.GolemHead },
                ApplyDowned: () => NPC.SetEventFlagCleared(ref NPC.downedGolemBoss, -1),
                IsDowned: () => NPC.downedGolemBoss));
        }

        private static void RegisterPlantera()
        {
            BossArenaRoutingRegistry.Register<BossArenaJungleSubworld>(NPCID.Plantera);
            BossRegistry.Register("vanilla:plantera", new BossDefinition(
                NpcTypes: new int[] { NPCID.Plantera, NPCID.PlanterasHook, NPCID.PlanterasTentacle },
                ApplyDowned: () => NPC.SetEventFlagCleared(ref NPC.downedPlantBoss, -1),
                IsDowned: () => NPC.downedPlantBoss));
        }

        private static void RegisterCultist()
        {
            BossRegistry.Register("vanilla:lunatic_cultist", new BossDefinition(
                NpcTypes: new int[] { NPCID.CultistBoss },
                ApplyDowned: () => NPC.SetEventFlagCleared(ref NPC.downedAncientCultist, -1),
                IsDowned: () => NPC.downedAncientCultist));
        }

        private static void RegisterMoonLord()
        {
            SummonItemRegistry.Register(ItemID.CelestialSigil, NPCID.MoonLordCore);
            BossRegistry.Register("vanilla:moon_lord", new BossDefinition(
                NpcTypes: new int[] { NPCID.MoonLordCore, NPCID.MoonLordHead, NPCID.MoonLordHand, NPCID.MoonLordFreeEye },
                ApplyDowned: () => NPC.SetEventFlagCleared(ref NPC.downedMoonlord, -1),
                IsDowned: () => NPC.downedMoonlord));
        }
    }
}
