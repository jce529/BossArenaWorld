using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using BossArenaSubWorld.Systems;
using BossArenaSubWorld.Subworlds;

namespace BossArenaSubWorld.Integrations
{
    public class SpiritIntegration : ModSystem
    {
        // Cached once at PostSetupContent -- avoids re-reflecting on every BossCoreItem use
        // (rare event, but caching is nearly free and improves failure diagnostics per
        // 05-RESEARCH.md's "Anti-Patterns to Avoid" -- un-cached reflection per Apply() call).
        private static FieldInfo _downedField;

        public override void PostSetupContent()
        {
            if (!ModLoader.HasMod("SpiritMod")) return;
            RegisterInfernon();
            RegisterAncientAvian();
            RegisterScarabeus();
        }

        // D-01/Pitfall 2 (carried from Phase 4): every method below this point may reference
        // SpiritMod types; PostSetupContent() above never touches a Spirit type directly, so
        // it JITs safely regardless of whether SpiritMod is installed.
        [JITWhenModsEnabled("SpiritMod")]
        private void RegisterInfernon()
        {
            int infernonType = ModContent.NPCType<SpiritMod.NPCs.Boss.Infernon.Infernon>();
            int infernoSkullType = ModContent.NPCType<SpiritMod.NPCs.Boss.Infernon.InfernoSkull>();

            _downedField = ModLoader.GetMod("SpiritMod").Code
                .GetType("SpiritMod.NPCs.BossDownedTracker")
                .GetField("Downed", BindingFlags.NonPublic | BindingFlags.Static);

            // SummonItemRegistry/BossRegistry are boss-agnostic (int/string + delegates) --
            // zero changes needed to either existing file (Phase 2/3/4 code untouched).
            SummonItemRegistry.Register(
                ModContent.ItemType<SpiritMod.Items.Consumable.CursedCloth>(), infernonType);

            // No BossArenaRoutingRegistry.Register<T>() call needed -- unlike Hive Mind,
            // Infernon's AI has no Zone*/biome despawn dependency (verified: full PreAI/AI
            // read for both Infernon and InfernoSkull, 05-RESEARCH.md Pitfall D). Falls back
            // to the default BossArenaSubworld automatically.

            // Pitfall B: register BOTH NPC types under one BossDefinition. In Normal Mode,
            // Infernon.PreKill() returns true and Infernon can die "for real", firing ITS
            // OWN OnKill(). In Expert Mode, Infernon.PreKill() returns false (undying) and it
            // instead fades out via alpha/active=false WITHOUT ever calling its own OnKill --
            // the actual downed-trigger becomes whichever InfernoSkull add is later killed.
            // Registering both guarantees a BossCoreItem drops regardless of which entity
            // lands the fight-ending kill (BossRegistry.NpcTypes already supports multi-type
            // bosses by design).
            BossRegistry.Register("spirit:infernon", new BossDefinition(
                NpcTypes: new[] { infernonType, infernoSkullType },
                ApplyDowned: ApplyInfernonDowned,
                IsDowned: IsInfernonDowned));
        }

        // Read path: no reflection needed. MyWorld is a PUBLIC class, DownedInfernon is a
        // PUBLIC static get-only property (=> BossDownedTracker.IsBossDowned<InfernoSkull>()).
        // This is the exact same value SpiritMod's own BossChecklist integration
        // (Infernon.RegisterToChecklist) reads -- matches what MOD-02's idempotency check
        // must observe. (Mod.Call("downed","Infernon") is an equally valid alternative per
        // 05-RESEARCH.md's Alternatives Considered -- both return the identical value.)
        [JITWhenModsEnabled("SpiritMod")]
        private static bool IsInfernonDowned() => SpiritMod.MyWorld.DownedInfernon;

        // Write path (Pitfall A): BossDownedTracker (SpiritMod.NPCs.BossDownedTracker) is
        // declared `internal`. Even though Downed/IsBossDowned<T>()/GetBossKey<T>() are
        // individually `public static`, C# caps effective member visibility at the
        // containing type's visibility -- direct `BossDownedTracker.Downed[...] = true;` is
        // a CS0122 compile error, weakReferences or not. No public setter and no Mod.Call
        // "set downed" context exist either (confirmed: SpiritMod.Call.cs's Downed context
        // is read-only). Reflection into the internal Dictionary<string,bool> field is the
        // ONLY path -- and it replicates EXACTLY what BossDownedTracker.OnKill(npc) itself
        // does (Downed[GetBossKey(npc)] = true), satisfying CLAUDE.md's "call the mod's
        // actual setter, don't bypass it" intent since no other path is exposed.
        //
        // Pitfall C: NEVER call BossDownedTrackingIO.HandleBossSyncing(BitsByte) as a
        // shortcut here -- it is the network-RECEIVE deserializer for MyWorld.NetReceive,
        // and it writes Downed[GetBossKey<Infernon>()] (Infernon's own key), NOT
        // Downed[GetBossKey<InfernoSkull>()] (the key MyWorld.DownedInfernon actually reads).
        // Calling it would silently set the WRONG dictionary entry.
        //
        // D-03: Spirit's Infernon downed-tracking path is fully world-scoped (dictionary
        // write + singleplayer-no-op netcode + world-scoped tile mutation) -- no
        // player-scoped side effect exists in BossDownedTracker.OnKill()/Infernon.OnKill()/
        // InfernoSkull.OnKill(), confirmed via full source read. No exclusion logic is
        // needed here, unlike Calamity's SetNewBossJustDowned() case (see
        // Integrations/CalamityIntegration.cs's ApplyHiveMindDowned, Phase 4) -- this
        // satisfies Phase 5 Success Criterion 2 by explicit documentation rather than by
        // building exclusion logic for a risk that was investigated and does not exist.
        [JITWhenModsEnabled("SpiritMod")]
        private static void ApplyInfernonDowned()
        {
            try
            {
                var downed = (Dictionary<string, bool>)_downedField.GetValue(null);

                // Key format replicated from BossDownedTracker.GetBossKey(NPC) via PUBLIC
                // APIs only (ModContent.GetInstance<T>()) -- avoids reflecting into
                // GetBossKey itself, which is stable tModLoader-wide convention, not
                // Spirit-specific logic likely to change.
                var infernoSkull = ModContent.GetInstance<SpiritMod.NPCs.Boss.Infernon.InfernoSkull>();
                string key = infernoSkull.Mod.Name + "/" + infernoSkull.Name; // "SpiritMod/InfernoSkull"

                downed[key] = true;

                // Matches BossDownedTracker.OnKill()'s own netcode replay exactly;
                // singleplayer no-op (same category as Phase 4's CalamityNetcode.SyncWorld()).
                if (Main.netMode != NetmodeID.SinglePlayer)
                    NetMessage.SendData(MessageID.WorldData);
            }
            catch (Exception e)
            {
                // Open Question 1 (05-RESEARCH.md): swallow-and-log for this phase's scope --
                // a broken reflection lookup (e.g. a future SpiritMod update renaming the
                // field) must never crash BossCoreItem.UseItem. IsInfernonDowned() would then
                // simply keep reporting "not yet downed", leaving the item retryable.
                ModContent.GetInstance<BossArenaSubWorld>().Logger.Warn(
                    "SpiritIntegration: failed to set Infernon downed flag via reflection: " + e);
            }

            // WorldGen side effect replay (D-02): Infernon.OnKill()/InfernoSkull.OnKill()
            // both place a ring of TileID.HellstoneBrick + drain liquid to Lava around the
            // boss's death position. No live NPC exists at BossCoreItem-use time in the main
            // world -- anchor on the player's current position instead (confirmed
            // cosmetic/non-blocking by the user, D-02 nuance).
            ReplayInfernonTileRing(Main.LocalPlayer.Center);
        }

        [JITWhenModsEnabled("SpiritMod")]
        private static void ReplayInfernonTileRing(Vector2 center)
        {
            int centerX = (int)center.X / 16;
            int centerY = (int)center.Y / 16;
            int halfLength = 160 / 2 / 16 + 1; // Infernon.NPC.width == 160 (source-confirmed)
            for (int x = centerX - halfLength; x <= centerX + halfLength; x++)
            {
                for (int y = centerY - halfLength; y <= centerY + halfLength; y++)
                {
                    Tile tile = Main.tile[x, y];
                    bool onRingEdge = x == centerX - halfLength || x == centerX + halfLength
                                    || y == centerY - halfLength || y == centerY + halfLength;
                    if (onRingEdge && !tile.HasTile)
                    {
                        tile.TileType = TileID.HellstoneBrick;
                        tile.HasTile = true;
                    }
                    tile.LiquidType = LiquidID.Lava;
                    tile.LiquidAmount = 0;
                    if (Main.netMode == NetmodeID.Server)
                        NetMessage.SendTileSquare(-1, x, y, 1);
                    else
                        WorldGen.SquareTileFrame(x, y, true);
                }
            }
        }

        // Architecture Pattern 3 (10-RESEARCH.md): generalizes Infernon's write-path reflection
        // to every Spirit boss whose downed-tracking flows through the generic
        // BossDownedTracker.OnKill(NPC) hook (confirmed: none of the 6 bosses below override
        // OnKill() themselves, and none have any WorldGen/player-scoped side effect beyond the
        // flag write -- unlike Infernon's own Hellstone tile-ring, which stays
        // Infernon-specific). Reuses the SAME cached _downedField FieldInfo established in
        // RegisterInfernon() -- do not re-reflect per boss.
        [JITWhenModsEnabled("SpiritMod")]
        private static void ApplyGenericSpiritDowned<T>() where T : ModNPC
        {
            try
            {
                var downed = (Dictionary<string, bool>)_downedField.GetValue(null);
                var instance = ModContent.GetInstance<T>();
                string key = instance.Mod.Name + "/" + instance.Name;
                downed[key] = true;
                if (Main.netMode != NetmodeID.SinglePlayer)
                    NetMessage.SendData(MessageID.WorldData);
            }
            catch (Exception e)
            {
                // Same swallow-and-log discipline as ApplyInfernonDowned (05-RESEARCH.md Open
                // Question 1) -- a broken reflection lookup must never crash BossCoreItem.UseItem.
                ModContent.GetInstance<BossArenaSubWorld>().Logger.Warn(
                    "SpiritIntegration: failed to set " + typeof(T).Name + " downed flag via reflection: " + e);
            }
        }

        [JITWhenModsEnabled("SpiritMod")]
        private void RegisterAncientAvian()
        {
            int itemType = ModContent.ItemType<SpiritMod.Items.Consumable.JewelCrown>();
            int npcType = ModContent.NPCType<SpiritMod.NPCs.Boss.AncientFlyer>();

            SummonItemRegistry.Register(itemType, npcType);
            // Thematic only (no Zone check in AncientFlyer's AI, 10-RESEARCH.md) -- routed to
            // Space altar per 09-ALTAR-BIOME-REFERENCE.md wiki-thematic assignment (D-01).
            BossArenaRoutingRegistry.Register<BossArenaSpaceSubworld>(npcType);

            BossRegistry.Register("spirit:ancient_avian", new BossDefinition(
                NpcTypes: new[] { npcType },
                ApplyDowned: ApplyAncientAvianDowned,
                IsDowned: IsAncientAvianDowned));
        }
        [JITWhenModsEnabled("SpiritMod")]
        private static bool IsAncientAvianDowned() => SpiritMod.MyWorld.DownedAncientAvian;
        [JITWhenModsEnabled("SpiritMod")]
        private static void ApplyAncientAvianDowned() => ApplyGenericSpiritDowned<SpiritMod.NPCs.Boss.AncientFlyer>();

        [JITWhenModsEnabled("SpiritMod")]
        private void RegisterScarabeus()
        {
            int itemType = ModContent.ItemType<SpiritMod.Items.Consumable.ScarabIdol>();
            int npcType = ModContent.NPCType<SpiritMod.NPCs.Boss.Scarabeus.Scarabeus>();

            SummonItemRegistry.Register(itemType, npcType);
            // FUNCTIONAL (10-RESEARCH.md, confirmed via decompile): ModifyHitByItem/hit-modifier
            // code divides FinalDamage by 3 in both directions when !player.ZoneDesert -- fight
            // is technically completable outside Desert but heavily unbalanced. Route to the
            // real Desert biome for genuine balance reasons, not just theme.
            BossArenaRoutingRegistry.Register<BossArenaDesertSubworld>(npcType);

            BossRegistry.Register("spirit:scarabeus", new BossDefinition(
                NpcTypes: new[] { npcType },
                ApplyDowned: ApplyScarabeusDowned,
                IsDowned: IsScarabeusDowned));
        }
        [JITWhenModsEnabled("SpiritMod")]
        private static bool IsScarabeusDowned() => SpiritMod.MyWorld.DownedScarabeus;
        [JITWhenModsEnabled("SpiritMod")]
        private static void ApplyScarabeusDowned() => ApplyGenericSpiritDowned<SpiritMod.NPCs.Boss.Scarabeus.Scarabeus>();
    }
}
