using Microsoft.Xna.Framework;
using SubworldLibrary;
using Terraria;
using Terraria.ModLoader;
using Terraria.ObjectData;
using BossArenaSubWorld.Systems;

namespace BossArenaSubWorld.Tiles
{
    // ENTRY-02: in-world return portal tile placed in boss arena subworlds.
    // Right-clicking triggers SubworldSystem.Exit() to safely return the player
    // to the main world, ensuring the OnExit vanilla downed-flag restore guard executes.
    public class ReturnPortalTile : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileLavaDeath[Type] = false;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(100, 200, 255), CreateMapEntryName());
        }

        public override bool RightClick(int i, int j)
        {
            if (BossArenaRoutingRegistry.IsAnyArenaActive())
            {
                Main.NewText("메인 월드로 귀환합니다.", 180, 220, 255);
                SubworldSystem.Exit();
                return true;
            }
            return false;
        }
    }
}
