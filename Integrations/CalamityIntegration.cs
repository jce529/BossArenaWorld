using System.Threading;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using BossArenaSubWorld.Subworlds;
using BossArenaSubWorld.Systems;

namespace BossArenaSubWorld.Integrations
{
    public class CalamityIntegration : ModSystem
    {
        public override void PostSetupContent()
        {
            if (!ModLoader.HasMod("CalamityMod")) return;
            RegisterHiveMind();
            RegisterDevourerOfGods();
            RegisterYharon();
            RegisterSupremeCalamitas();
            RegisterDragonfolly();
            RegisterProvidence();
            RegisterProfanedGuardians();
            RegisterAstrumDeus();
            RegisterAstrumAureus();
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

            // Route Hive Mind into the Corruption-biome arena, not the plain BossArenaSubworld --
            // see .planning/debug/resolved/hivemind-zonecorrupt-despawn-corruption-subworld.md.
            // Hive Mind's AI despawns it almost instantly without a real player.ZoneCorrupt,
            // which only a genuine Corruption-tile biome (recomputed every tick by vanilla)
            // can satisfy.
            BossArenaRoutingRegistry.Register<BossArenaCorruptionSubworld>(npcType);

            BossRegistry.Register("calamity:hive_mind", new BossDefinition(
                NpcTypes: new[] { npcType },
                ApplyDowned: ApplyHiveMindDowned,
                IsDowned: IsHiveMindDowned));
        }

        // D-01/Pitfall 2 continued: the C# compiler hoists an inline lambda referencing a
        // Calamity type into a compiler-generated method on a <>c cache class, which does
        // NOT inherit [JITWhenModsEnabled] from its enclosing method -- tModLoader's JIT
        // prefilter still eagerly touches that generated method even though RegisterHiveMind()
        // itself is never called when CalamityMod is absent, producing a real JITException.
        // (Confirmed live: CalamityMod-disabled load crashed with a JITException naming
        // <RegisterHiveMind>b__1_0 until this lambda was extracted into its own tagged
        // method below.) Every delegate passed into a [JITWhenModsEnabled]-guarded
        // registration call must be a named, separately-tagged method -- never an inline
        // lambda -- for this same reason.
        [JITWhenModsEnabled("CalamityMod")]
        private static bool IsHiveMindDowned() => CalamityMod.DownedBossSystem.downedHiveMind;

        // KNOWN COSMETIC LIMITATION (investigated, not a state-corruption bug -- see
        // .planning/debug/hivemind-zonecorrupt-despawn-corruption-subworld.md, session
        // 2026-08-13 continuation): the player sees TWO "sky is glittering" messages --
        // once inside BossArenaCorruptionSubworld at the moment Hive Mind actually dies
        // (Calamity's own compiled HiveMind.OnKill() genuinely fires there, since
        // tModLoader has no concept of "kill happened in a subworld" -- it's a real kill
        // event), and once for real here when the carrier item is used. This is EXPECTED
        // and architecturally correct, not a double-apply bug:
        //   1. HiveMind.OnKill()'s in-subworld call to AerialiteOreGen.Enchant() operates
        //      on whatever tile array is CURRENTLY LOADED (Main.tile) -- decompiled and
        //      confirmed it has no cross-world state/marker. Inside the arena, this scans
        //      BossArenaCorruptionSubworld's own generated platform (Ebonstone/CorruptGrass
        //      only, no AerialiteOreDisenchanted tiles ever placed there), so it converts
        //      ZERO tiles -- a genuine no-op broadcast, not even a wasted mutation.
        //   2. CalamityMod.DownedBossSystem.downedHiveMind is set true in-memory during
        //      that in-subworld OnKill(), but it is NOT part of SubworldLibrary's
        //      hardcoded vanilla NPC/DD2Event "downed" flag whitelist (CopyDowned() /
        //      ReadCopiedDowned() -- see isolation-premise-flag-persistence.md), so it
        //      gets no special leak/sync treatment. Instead it naturally resets to
        //      whatever's on the REAL main world's .wld file (false) when
        //      SubworldSystem's exit flow reloads the main world: decompiled
        //      SubworldSystem.LoadSubworld() confirms Main.ActiveWorldFileData (and the
        //      computed Main.worldPathName) points at the arena's OWN throwaway file path
        //      for the entire subworld visit, so the exit-flow's WorldFile.SaveWorld()
        //      call (which fires while still "in" the subworld, before the real main
        //      world reloads) writes the true value to the arena's own discarded file,
        //      never to the real main .wld. The subsequent real main-world reload then
        //      correctly restores downedHiveMind = false via DownedBossSystem's own
        //      LoadWorldData(TagCompound).
        //   3. Therefore BossRegistry.Apply()'s D-01 idempotency check (IsDowned() below)
        //      correctly reads false in the main world, and this method runs for REAL,
        //      exactly once, against the real main world's actual tiles/flag/save file --
        //      matching the project's "kill in throwaway subworld, apply for real via
        //      carrier item in main world" architecture (Pitfall 1).
        // Suppressing the first (in-subworld) message cleanly is NOT possible without
        // hooking/patching CalamityMod's compiled DLL: decompiled
        // Terraria.ModLoader.NPCLoader.OnKill() confirms `npc.ModNPC?.OnKill()` (Calamity's
        // own HiveMind.OnKill()) always runs BEFORE any GlobalNPC.OnKill hook this mod
        // could use to pre-empt it. Treated as an accepted, documented cosmetic-only UX
        // quirk rather than forcing an invasive IL-hook fix for a chat message.
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

        // Plan 10-02, Task 1: Devourer of Gods -- plain arena (no biome/routing dependency
        // per 09-ALTAR-BIOME-REFERENCE.md Section 3), same RegisterX/IsXDowned/ApplyXDowned
        // triplet shape as Hive Mind above.
        [JITWhenModsEnabled("CalamityMod")]
        private void RegisterDevourerOfGods()
        {
            int itemType = ModContent.ItemType<CalamityMod.Items.SummonItems.CosmicWorm>();
            int npcType = ModContent.NPCType<CalamityMod.NPCs.DevourerofGods.DevourerofGodsHead>();

            SummonItemRegistry.Register(itemType, npcType);
            // No BossArenaRoutingRegistry call -- plain arena per 09-ALTAR-BIOME-REFERENCE.md
            // Section 3 ("Plain arena, no altar needed at all"). Falls back to the default
            // BossArenaSubworld automatically.

            BossRegistry.Register("calamity:devourer_of_gods", new BossDefinition(
                NpcTypes: new[] { npcType },
                ApplyDowned: ApplyDevourerOfGodsDowned,
                IsDowned: IsDevourerOfGodsDowned));
        }

        [JITWhenModsEnabled("CalamityMod")]
        private static bool IsDevourerOfGodsDowned() => CalamityMod.DownedBossSystem.downedDoG;

        [JITWhenModsEnabled("CalamityMod")]
        private static void ApplyDevourerOfGodsDowned()
        {
            // Faithful replay of DevourerofGodsHead.OnKill(). SetNewShopVariable is called
            // BEFORE the flag flips -- replicate that exact order (passes the OLD,
            // pre-update value). Excludes SetNewBossJustDowned() (player-scoped, Pitfall 5).
            CalamityMod.NPCs.CalamityGlobalTownNPC.SetNewShopVariable(
                new[] { ModContent.NPCType<CalamityMod.NPCs.TownNPCs.Bandit>() },
                CalamityMod.DownedBossSystem.downedDoG);
            if (!CalamityMod.DownedBossSystem.downedDoG)
            {
                CalamityMod.CalamityUtils.BroadcastLocalizedText("Mods.CalamityMod.Status.Progression.DoGBossText", Color.Cyan);
                CalamityMod.CalamityUtils.BroadcastLocalizedText("Mods.CalamityMod.Status.Progression.DoGBossText2", Color.Orange);
                CalamityMod.CalamityUtils.BroadcastLocalizedText("Mods.CalamityMod.Status.Progression.DargonBossText", Color.Yellow);
            }
            CalamityMod.DownedBossSystem.downedDoG = true;
            CalamityMod.CalamityNetcode.SyncWorld();
        }

        // Plan 10-02, Task 1: Yharon -- plain arena, same rationale as Devourer of Gods above.
        [JITWhenModsEnabled("CalamityMod")]
        private void RegisterYharon()
        {
            int itemType = ModContent.ItemType<CalamityMod.Items.SummonItems.YharonEgg>();
            int npcType = ModContent.NPCType<CalamityMod.NPCs.Yharon.Yharon>();

            SummonItemRegistry.Register(itemType, npcType);
            // Plain arena, same rationale as Devourer of Gods above.

            BossRegistry.Register("calamity:yharon", new BossDefinition(
                NpcTypes: new[] { npcType },
                ApplyDowned: ApplyYharonDowned,
                IsDowned: IsYharonDowned));
        }

        [JITWhenModsEnabled("CalamityMod")]
        private static bool IsYharonDowned() => CalamityMod.DownedBossSystem.downedYharon;

        [JITWhenModsEnabled("CalamityMod")]
        private static void ApplyYharonDowned()
        {
            // Faithful replay of Yharon.OnKill(). Note SetNewShopVariable fires BEFORE
            // SetNewBossJustDowned() here (reverse order vs. Devourer of Gods) -- source order
            // doesn't matter for us since SetNewBossJustDowned is excluded (Pitfall 5), but
            // SetNewShopVariable's BEFORE-the-flag-flip timing must still be preserved.
            CalamityMod.NPCs.CalamityGlobalTownNPC.SetNewShopVariable(
                new[] { ModContent.NPCType<CalamityMod.NPCs.TownNPCs.Bandit>() },
                CalamityMod.DownedBossSystem.downedYharon);
            if (!CalamityMod.DownedBossSystem.downedYharon)
            {
                CalamityMod.CalamityUtils.SpawnOre(ModContent.TileType<CalamityMod.Tiles.Ores.AuricOre>(), 2E-05, 0.75f, 0.9f, 10, 20);
                CalamityMod.CalamityUtils.BroadcastLocalizedText("Mods.CalamityMod.Status.Progression.AuricOreText", Color.Gold);
            }
            CalamityMod.DownedBossSystem.downedYharon = true;
            CalamityMod.CalamityNetcode.SyncWorld();
        }

        // Plan 10-02, Task 2: Supreme Witch, Calamitas -- plain arena. Calamity's own
        // "Altar of the Accursed" furniture just needs to be placeable in
        // BossArenaSubworld, no biome routing required (09-ALTAR-BIOME-REFERENCE.md
        // Section 3). Real vanilla flow is hold-item, right-click a placed SCalAltar
        // tile -- Test1Tile bypasses that entirely (Architecture Pattern 2,
        // 10-RESEARCH.md); register CeremonialUrn directly.
        [JITWhenModsEnabled("CalamityMod")]
        private void RegisterSupremeCalamitas()
        {
            int itemType = ModContent.ItemType<CalamityMod.Items.SummonItems.CeremonialUrn>();
            int npcType = ModContent.NPCType<CalamityMod.NPCs.SupremeCalamitas.SupremeCalamitas>();

            SummonItemRegistry.Register(itemType, npcType);

            BossRegistry.Register("calamity:supreme_calamitas", new BossDefinition(
                NpcTypes: new[] { npcType },
                ApplyDowned: ApplySupremeCalamitasDowned,
                IsDowned: IsSupremeCalamitasDowned));
        }

        [JITWhenModsEnabled("CalamityMod")]
        private static bool IsSupremeCalamitasDowned() => CalamityMod.DownedBossSystem.downedCalamitas;

        [JITWhenModsEnabled("CalamityMod")]
        private static void ApplySupremeCalamitasDowned()
        {
            // Faithful replay of SupremeCalamitas.OnKill(). Deliberately EXCLUDES
            // SetNewBossJustDowned() (player-scoped, Pitfall 5), the player.Calamity().
            // sCalKillCount++ increment (also player-scoped -- already ran for real on the
            // live player during the actual subworld kill), and the follow-up
            // Archmage/BrimstoneWitch NPC.NewNPC() spawn (a live-in-subworld encounter
            // continuation -- no live NPC/position exists at BossCoreItem-use time in the
            // main world).
            CalamityMod.DownedBossSystem.downedCalamitas = true;
            CalamityMod.CalamityNetcode.SyncWorld();
        }

        // Plan 10-02, Task 2: Dragonfolly -- FUNCTIONAL Zone dependency (10-RESEARCH.md,
        // confirmed via decompile): Dragonfolly's AI() increments a leave-Jungle timer
        // whenever !player.ZoneJungle, capping at 300 -- a genuine grace-period-then-enrage
        // mechanic, not cosmetic. Must route to the real Jungle biome, not just thematically.
        [JITWhenModsEnabled("CalamityMod")]
        private void RegisterDragonfolly()
        {
            int itemType = ModContent.ItemType<CalamityMod.Items.SummonItems.ExoticPheromones>();
            int npcType = ModContent.NPCType<CalamityMod.NPCs.Bumblebirb.Dragonfolly>();

            SummonItemRegistry.Register(itemType, npcType);
            BossArenaRoutingRegistry.Register<BossArenaJungleSubworld>(npcType);

            BossRegistry.Register("calamity:dragonfolly", new BossDefinition(
                NpcTypes: new[] { npcType },
                ApplyDowned: ApplyDragonfollyDowned,
                IsDowned: IsDragonfollyDowned));
        }

        [JITWhenModsEnabled("CalamityMod")]
        private static bool IsDragonfollyDowned() => CalamityMod.DownedBossSystem.downedDragonfolly;

        [JITWhenModsEnabled("CalamityMod")]
        private static void ApplyDragonfollyDowned()
        {
            // Faithful replay of Dragonfolly.OnKill()'s downed-state effects. Excludes
            // SetNewBossJustDowned() (player-scoped, Pitfall 5) and the
            // Main.zenithWorld-gated lightning-projectile effect (seed-specific cosmetic,
            // not part of the downed-state contract -- matches Hive Mind's precedent of
            // skipping seed-specific effects).
            CalamityMod.DownedBossSystem.downedDragonfolly = true;
            CalamityMod.CalamityNetcode.SyncWorld();
        }

        // Plan 10-04, Task 1: Providence -- D-02: register ONLY when InfernumMode is
        // absent. With InfernumMode present, this item must fall through untouched to
        // Infernum's own structure-gated CanUseItem/UseItem (registering anyway would
        // produce a silent soft-lock).
        [JITWhenModsEnabled("CalamityMod")]
        private void RegisterProvidence()
        {
            if (ModLoader.HasMod("InfernumMode")) return;

            int itemType = ModContent.ItemType<CalamityMod.Items.SummonItems.ProfanedCore>();
            int npcType = ModContent.NPCType<CalamityMod.NPCs.Providence.Providence>();

            SummonItemRegistry.Register(itemType, npcType);
            // D-02 discretion: Providence is valid under either Hallow or Underworld per
            // vanilla Calamity (its own biomeType branch is a purely cosmetic effect-theme
            // choice, no functional Zone dependency -- confirmed via full OnKill/AI decompile,
            // 10-RESEARCH.md). Hallow chosen for both Providence and Profaned Guardians to
            // spread registered bosses across arenas more evenly (Underworld already hosts
            // Signus) -- zero functional cost either way.
            BossArenaRoutingRegistry.Register<BossArenaHallowSubworld>(npcType);

            BossRegistry.Register("calamity:providence", new BossDefinition(
                NpcTypes: new[] { npcType },
                ApplyDowned: ApplyProvidenceDowned,
                IsDowned: IsProvidenceDowned));
        }
        [JITWhenModsEnabled("CalamityMod")]
        private static bool IsProvidenceDowned() => CalamityMod.DownedBossSystem.downedProvidence;
        [JITWhenModsEnabled("CalamityMod")]
        private static void ApplyProvidenceDowned()
        {
            // Faithful replay of Providence.OnKill()'s side effects. Excludes
            // SetNewBossJustDowned() (player-scoped, Pitfall 5), the particle/screenshake
            // visuals (cosmetic, live-kill-only), and the Main.netMode==0 "challenge" chat line
            // (challenge-mode-specific, unrelated to downed state).
            if (!CalamityMod.DownedBossSystem.downedProvidence)
            {
                CalamityMod.CalamityUtils.SpawnOre(ModContent.TileType<CalamityMod.Tiles.Ores.UelibloomOre>(), 0.00017, 0.55f, 0.9f, 8, 14, 59);
                CalamityMod.CalamityUtils.BroadcastLocalizedText("Mods.CalamityMod.Status.Progression.ProfanedBossText3", Color.Orange);
                CalamityMod.CalamityUtils.BroadcastLocalizedText("Mods.CalamityMod.Status.Progression.TreeOreText", Color.LightGreen);
            }
            CalamityMod.DownedBossSystem.downedProvidence = true;
            CalamityMod.CalamityNetcode.SyncWorld();
        }

        // Plan 10-04, Task 1: Profaned Guardians -- D-02, same InfernumMode-absent gate as
        // Providence above.
        [JITWhenModsEnabled("CalamityMod")]
        private void RegisterProfanedGuardians()
        {
            if (ModLoader.HasMod("InfernumMode")) return; // D-02, same rationale as Providence

            int itemType = ModContent.ItemType<CalamityMod.Items.SummonItems.ProfanedShard>();
            // Only ProfanedGuardianCommander sets downedGuardians in OnKill() -- Defender/Healer
            // have no OnKill override at all (confirmed via decompile of all 3 classes,
            // 10-RESEARCH.md Open Question 5). NPC.SpawnOnPlayer(Commander) matches vanilla's
            // own SpawnBossUsingItem<ProfanedGuardianCommander>() call in ProfanedShard.
            // UseItem() -- same single-type spawn this project already does for every other boss.
            int npcType = ModContent.NPCType<CalamityMod.NPCs.ProfanedGuardians.ProfanedGuardianCommander>();

            SummonItemRegistry.Register(itemType, npcType);
            BossArenaRoutingRegistry.Register<BossArenaHallowSubworld>(npcType); // D-02 discretion, same as Providence

            BossRegistry.Register("calamity:profaned_guardians", new BossDefinition(
                NpcTypes: new[] { npcType },
                ApplyDowned: ApplyProfanedGuardiansDowned,
                IsDowned: IsProfanedGuardiansDowned));
        }
        [JITWhenModsEnabled("CalamityMod")]
        private static bool IsProfanedGuardiansDowned() => CalamityMod.DownedBossSystem.downedGuardians;
        [JITWhenModsEnabled("CalamityMod")]
        private static void ApplyProfanedGuardiansDowned()
        {
            // Faithful replay of ProfanedGuardianCommander.OnKill() -- excludes
            // SetNewBossJustDowned() (player-scoped, Pitfall 5).
            CalamityMod.DownedBossSystem.downedGuardians = true;
            CalamityMod.CalamityNetcode.SyncWorld();
        }

        // Plan 10-04, Task 2: Astrum Deus -- routes to BossArenaAstralSubworld
        // unconditionally, forces night in its arena ONLY when InfernumMode is loaded
        // (D-02/D-04).
        [JITWhenModsEnabled("CalamityMod")]
        private void RegisterAstrumDeus()
        {
            // Recommended over TitanHeart (also used for unrelated armor crafting), 10-RESEARCH.md Discretion.
            int itemType = ModContent.ItemType<CalamityMod.Items.SummonItems.Starcore>();
            int npcType = ModContent.NPCType<CalamityMod.NPCs.AstrumDeus.AstrumDeusHead>();

            SummonItemRegistry.Register(itemType, npcType);
            BossArenaRoutingRegistry.Register<BossArenaAstralSubworld>(npcType); // thematic (no Zone dependency in AI, 10-RESEARCH.md)

            // D-02: Astrum Deus additionally needs forced night in its arena ONLY when
            // InfernumMode is loaded (Infernum reworks its AI to require night).
            // Unconditional (no forced night) under base Calamity.
            if (ModLoader.HasMod("InfernumMode"))
                ForcedTimeSystem.RegisterForceNight(npcType);

            BossRegistry.Register("calamity:astrum_deus", new BossDefinition(
                NpcTypes: new[] { npcType },
                ApplyDowned: ApplyAstrumDeusDowned,
                IsDowned: IsAstrumDeusDowned));
        }
        [JITWhenModsEnabled("CalamityMod")]
        private static bool IsAstrumDeusDowned() => CalamityMod.DownedBossSystem.downedAstrumDeus;
        [JITWhenModsEnabled("CalamityMod")]
        private static void ApplyAstrumDeusDowned()
        {
            // Faithful replay of AstrumDeusHead.OnKill()'s downed-state side effects. Excludes
            // the ShouldNotDropThings()-gated "kill all active NPCs of this type" cleanup
            // (live-fight multi-segment-worm bookkeeping) and SetNewBossJustDowned()
            // (player-scoped, Pitfall 5).
            if (!CalamityMod.DownedBossSystem.downedAstrumDeus)
                CalamityMod.CalamityUtils.BroadcastLocalizedText("Mods.CalamityMod.Status.Progression.AstralBossText", Color.Gold);
            CalamityMod.DownedBossSystem.downedAstrumDeus = true;
            CalamityMod.CalamityNetcode.SyncWorld();
        }

        // Plan 10-04, Task 2: Astrum Aureus -- same arena/night rules as Astrum Deus.
        [JITWhenModsEnabled("CalamityMod")]
        private void RegisterAstrumAureus()
        {
            int itemType = ModContent.ItemType<CalamityMod.Items.SummonItems.AstralChunk>();
            int npcType = ModContent.NPCType<CalamityMod.NPCs.AstrumAureus.AstrumAureus>();

            SummonItemRegistry.Register(itemType, npcType);
            BossArenaRoutingRegistry.Register<BossArenaAstralSubworld>(npcType);

            if (ModLoader.HasMod("InfernumMode"))
                ForcedTimeSystem.RegisterForceNight(npcType);

            BossRegistry.Register("calamity:astrum_aureus", new BossDefinition(
                NpcTypes: new[] { npcType },
                ApplyDowned: ApplyAstrumAureusDowned,
                IsDowned: IsAstrumAureusDowned));
        }
        [JITWhenModsEnabled("CalamityMod")]
        private static bool IsAstrumAureusDowned() => CalamityMod.DownedBossSystem.downedAstrumAureus;
        // Pitfall 4 (10-RESEARCH.md): PlaceAstralMeteor() is dispatched on a background thread
        // in the real OnKill() -- replicate that exact ThreadPool.QueueUserWorkItem dispatch,
        // do NOT simplify to a synchronous call.
        [JITWhenModsEnabled("CalamityMod")]
        private static void ApplyAstrumAureusDowned()
        {
            if (!CalamityMod.DownedBossSystem.downedAstrumAureus)
            {
                CalamityMod.CalamityUtils.BroadcastLocalizedText("Mods.CalamityMod.Status.Progression.AureusBossText", Color.Gold);
                CalamityMod.CalamityUtils.BroadcastLocalizedText("Mods.CalamityMod.Status.Progression.AureusBossText2", Color.Gold);
            }
            ThreadPool.QueueUserWorkItem(_ => CalamityMod.World.AstralBiome.PlaceAstralMeteor());
            CalamityMod.DownedBossSystem.downedAstrumAureus = true;
            CalamityMod.CalamityNetcode.SyncWorld();
        }
    }
}
