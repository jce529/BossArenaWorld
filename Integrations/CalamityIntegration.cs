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
    }
}
