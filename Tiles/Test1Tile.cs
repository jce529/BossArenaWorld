using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ObjectData;
using BossArenaSubWorld.Systems;

namespace BossArenaSubWorld.Tiles
{
    // D-02/D-03: brand-new custom tile, visually benchmarked off the Corruption Altar
    // (placeholder solid-color texture, see Tiles/Test1Tile.png) but NOT TileID.Altars --
    // no hammer-smash hardmode trigger, no altar chat message, no altar recipe unlocks,
    // because none of that vanilla behavior is tied to this tile type (it only exists on
    // vanilla's own hardcoded TileID.Altars checks elsewhere in the game).
    public class Test1Tile : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileLavaDeath[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(120, 60, 200), CreateMapEntryName());
        }

        // SUBW-02/SUBW-03/SUBW-04 redirect trigger (D-04). Returning true claims the
        // interaction, so the held item's own UseItem/AltFunctionUse pipeline is never
        // reached -- this is what satisfies "main-world summon never happens" and "item
        // not consumed" (SUBW-04) by construction, not via an extra guard (02-RESEARCH.md
        // Pattern 3 / Pitfall 4).
        //
        // Deviation from plan (Rule 3 - blocking compile error): 02-02-PLAN.md's interface
        // section specified overriding "NewRightClick", sourced from tModLoader's GitHub
        // patch files. The actually-installed local tModLoader.dll (this project's real
        // compile-time reference, confirmed via MetadataLoadContext reflection) only declares
        // "RightClick(int i, int j) -> bool" on ModTile -- no "NewRightClick" member exists.
        // Same signature/semantics (bool return, i/j tile-coordinate params, virtual), so the
        // override target was swapped to match what actually compiles against this binary.
        public override bool RightClick(int i, int j)
        {
            Player player = Main.LocalPlayer; // singleplayer-only project

            // 10-01: player-aware overload -- resolves polymorphic items (e.g. MarkofProvidence) via
            // their registered resolver delegate, falls back to the plain single-item lookup for
            // every other boss unchanged. Returns false (no interaction) if the item isn't
            // registered at all, OR if a polymorphic item's resolver returns -1 for the player's
            // current Zone state (mirrors the real item's CanUseItem() == false outcome).
            if (!SummonItemRegistry.TryGetBoss(player, player.HeldItem.type, out int bossNpcType))
                return false; // not a registered summon item -- no interaction (SUBW-01 gate)

            // 06-02 user-requested scope addition: mirrors the real source item's
            // CanUseItem() lockout (e.g. CatalystMod's AstralCommunicator becoming
            // permanently unusable once Moon Lord is downed before Astrageldon). Silent
            // no-op (no chat message), matching vanilla's own CanUseItem() == false
            // behavior -- item is not consumed, no subworld entry happens.
            if (!SummonItemRegistry.CanSummon(player.HeldItem.type))
                return false;

            Main.NewText("보스 아레나로 입장합니다. 도착하면 보스가 자동으로 소환됩니다.", 220, 180, 255); // D-11

            BossSummonPlayer.PendingBossNpcType = bossNpcType;
            // D-04 (10-CONTEXT.md): tracks which boss this arena visit is for, for the WHOLE visit
            // (unlike BossSummonPlayer.PendingBossNpcType, which is consumed once the boss spawns).
            // Lets ForcedTimeSystem.PreUpdateWorld keep re-asserting forced night every tick for
            // bosses that need it (Moon Jelly Wizard, Dusking unconditionally; Astrum Deus/Astrum
            // Aureus only when InfernumMode is loaded). No-op for every boss not registered via
            // ForcedTimeSystem.RegisterForceNight (i.e. every boss before Plan 10-03/10-04).
            ForcedTimeSystem.ActiveArenaBossNpcType = bossNpcType;
            // SUBW-boss-aware: routes to the correct arena subworld for this boss (plain
            // BossArenaSubworld by default, or a boss-specific biome arena if one was
            // registered -- see Systems/BossArenaRoutingRegistry.cs and
            // .planning/debug/resolved/hivemind-zonecorrupt-despawn-corruption-subworld.md).
            BossArenaRoutingRegistry.Enter(bossNpcType);
            return true;
        }
    }
}
