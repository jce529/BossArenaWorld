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

			for (int x = 0; x < Main.maxTilesX; x++)
			{
				for (int y = surfaceY; y < surfaceY + thickness; y++)
				{
					Tile tile = Main.tile[x, y];
					tile.HasTile = true;
					tile.TileType = TileID.Stone;
				}
			}

			// Set spawn point above the platform surface explicitly -- LoadSubworld()'s blind
			// midpoint default would otherwise leave the player mid-air/underground (Pitfall 2).
			Main.spawnTileX = Main.maxTilesX / 2;
			Main.spawnTileY = surfaceY - 3;
		}
	}
}
