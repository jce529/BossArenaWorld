using Terraria.ModLoader;
using BossArenaSubWorld.Tiles;

namespace BossArenaSubWorld.Items
{
    // D-05: no crafting recipe -- obtained for this phase via the temporary
    // /bossarena-givetestitems debug command (Debug/SubworldDebugCommands.cs).
    // Final itemization/acquisition path is explicitly deferred past this phase.
    public class Test1Item : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<Test1Tile>());
            Item.width = 16;
            Item.height = 16;
            Item.value = 0;
        }
    }
}
