using Terraria;
using Terraria.ID;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace BossArenaSubWorld.Subworlds
{
	// A single-purpose GenPass: fills a flat, thin stone platform and nothing else.
	// This absence-by-construction (no ore/structure/biome placement of any kind) is what
	// satisfies SUBW-05 (zero placed mod/vanilla content) -- see 01-RESEARCH.md Pattern 2.
	public class FlatStonePlatformPass : GenPass
	{
		public FlatStonePlatformPass(string name, float loadWeight) : base(name, loadWeight)
		{
		}

		protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
		{
			progress.Message = "Generating boss arena platform";

			int surfaceY = Main.maxTilesY / 2; // mid-height placement
			int thickness = 15; // thin stone layer (D-07 guideline: ~10-20 blocks)

			// Fill themed background wall behind the arena tiers (DECOR-01)
			ArenaBuilder.FillWall(0, Main.maxTilesX, surfaceY - 60, surfaceY + thickness, WallID.Stone);

			for (int x = 0; x < Main.maxTilesX; x++)
			{
				for (int y = surfaceY; y < surfaceY + thickness; y++)
				{
					Tile tile = Main.tile[x, y];
					tile.HasTile = true;
					tile.TileType = (y <= surfaceY + 1) ? TileID.GrayBrick : TileID.Stone;
				}
			}

			// Set spawn point above the platform surface explicitly
			Main.spawnTileX = Main.maxTilesX / 2;
			Main.spawnTileY = surfaceY - 3;

			// Place campfire for arena theming and player convenience (DECOR-01)
			WorldGen.PlaceTile(Main.spawnTileX - 4, surfaceY - 1, TileID.Campfire, mute: true, forced: true);
		}
	}
}
