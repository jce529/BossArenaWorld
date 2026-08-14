using Terraria;
using Terraria.ID;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace BossArenaSubWorld.Subworlds
{
	// Source: decompiled Terraria.Player.UpdateBiomes() / Terraria.SceneMetrics --
	// ZoneHallow = SceneMetrics.HolyTileCount >= SceneMetrics.HallowTileThreshold (125), a per-tick
	// weighted sum over TileID.Sets.HallowBiome (weight 1 each: Pearlstone=117, HallowedGrass=109,
	// HallowedPlants=110, HallowedPlants2=113, Pearlsand=116, HallowedIce=164, HallowHardenedSand=402,
	// HallowSandstone=403). Filling the ENTIRE platform width with Pearlstone/HallowedGrass guarantees
	// the flag stays true regardless of where the player/boss drifts mid-fight, matching
	// CorruptionPlatformPass's full-width-fill precedent (04-RESEARCH.md).
	public class HallowPlatformPass : GenPass
	{
		public HallowPlatformPass(string name, float loadWeight) : base(name, loadWeight)
		{
		}

		protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
		{
			progress.Message = "Generating hallow boss arena platform";

			int surfaceY = Main.maxTilesY / 2; // mid-height, matches CorruptionPlatformPass convention
			int thickness = 15;

			for (int x = 0; x < Main.maxTilesX; x++)
			{
				for (int y = surfaceY; y < surfaceY + thickness; y++)
				{
					Tile tile = Main.tile[x, y];
					tile.HasTile = true;
					tile.TileType = (y == surfaceY) ? TileID.HallowedGrass : TileID.Pearlstone;
				}
			}

			Main.spawnTileX = Main.maxTilesX / 2;
			Main.spawnTileY = surfaceY - 3;
		}
	}
}
