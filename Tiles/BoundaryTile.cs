using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace BossArenaSubWorld.Tiles
{
    // D-02 (11-CONTEXT.md): custom invisible solid tile for boundary and fall containment (BOUND-03).
    // Main.tileSolid[Type] = true provides collision containment to prevent player fall-out or drift,
    // Main.tileBlockLight[Type] = false prevents shadow casting, and PreDraw returning false ensures
    // the barrier remains completely invisible in-game.
    public class BoundaryTile : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = false;
            Main.tileNoAttach[Type] = true;
            Main.tileFrameImportant[Type] = false;
            Main.tileLavaDeath[Type] = false;
        }

        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        {
            return false; // completely invisible
        }
    }
}
