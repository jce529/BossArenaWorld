using Microsoft.Xna.Framework;
using SubworldLibrary;
using Terraria;
using Terraria.ModLoader;
using Terraria.ObjectData;
using BossArenaSubWorld.Subworlds;
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

            if (!SummonItemRegistry.TryGetBoss(player.HeldItem.type, out int bossNpcType))
                return false; // not a registered summon item -- no interaction (SUBW-01 gate)

            Main.NewText("보스 아레나로 입장합니다. 도착하면 보스가 자동으로 소환됩니다.", 220, 180, 255); // D-11

            BossSummonPlayer.PendingBossNpcType = bossNpcType;
            SubworldSystem.Enter<BossArenaSubworld>();
            return true;
        }
    }
}
