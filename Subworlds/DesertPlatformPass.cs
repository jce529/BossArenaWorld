using Terraria;
using Terraria.ID;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace BossArenaSubWorld.Subworlds
{
	// Source: decompiled Terraria.Player.UpdateBiomes() / Terraria.SceneMetrics --
	// ZoneDesert = SceneMetrics.SandTileCount >= SceneMetrics.DesertTileThreshold (1500), a per-tick
	// weighted sum over TileID.Sets.SandBiome (weight 1 each: Sand=53, Ebonsand=112, Pearlsand=116,
	// Crimsand=234, plus the Hardened/Sandstone tile families). 09-RESEARCH.md Pitfall 5: Desert's
	// threshold (1500) is 5-12x higher than every other tile-weighted biome in this project
	// (Corruption=300, Hallow=125, Jungle=140, Dungeon=250) -- this pass uses thickness=20 (not the
	// usual 15) for extra safety margin, since the default 15-tile convention clears the threshold
	// with only ~2x margin instead of Corruption's ~10x.
	public class DesertPlatformPass : GenPass
	{
		public DesertPlatformPass(string name, float loadWeight) : base(name, loadWeight)
		{
		}

		protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
		{
			progress.Message = "Generating desert boss arena platform";

			int surfaceY = Main.maxTilesY / 2; // mid-height, matches CorruptionPlatformPass convention
			int thickness = 20; // wider margin than the usual 15 -- see class comment (Pitfall 5)

			for (int x = 0; x < Main.maxTilesX; x++)
			{
				for (int y = surfaceY; y < surfaceY + thickness; y++)
				{
					Tile tile = Main.tile[x, y];
					tile.HasTile = true;
					tile.TileType = TileID.Sand;
				}
			}

			Main.spawnTileX = Main.maxTilesX / 2;
			Main.spawnTileY = surfaceY - 3;
		}
	}
}
