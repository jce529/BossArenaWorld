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

			int surfaceY = Main.maxTilesY / 2; // mid-height (400)
			int thickness = 15;

			// Fill themed background wall behind all 7 arena tiers (DECOR-01)
			ArenaBuilder.FillWall(0, Main.maxTilesX, surfaceY - 120, surfaceY + 80, WallID.HallowedGrassUnsafe);

			// 1. Solid hallow floor slab below lower platform tiers (provides ~3000 Pearlstone tiles >> 125)
			int floorY = surfaceY + 60;
			for (int x = 0; x < Main.maxTilesX; x++)
			{
				for (int y = floorY; y < floorY + thickness; y++)
				{
					Tile tile = Main.tile[x, y];
					tile.HasTile = true;
					tile.TileType = (y == floorY) ? TileID.HallowedGrass : ((y == floorY + 1) ? TileID.PearlstoneBrick : TileID.Pearlstone);
				}
			}

			// 2. Solid hallow ceiling slab above top platform tier
			int ceilingY = surfaceY - 120;
			for (int x = 0; x < Main.maxTilesX; x++)
			{
				for (int y = ceilingY; y < ceilingY + 10; y++)
				{
					Tile tile = Main.tile[x, y];
					tile.HasTile = true;
					tile.TileType = TileID.Pearlstone;
				}
			}

			// 3. Spawn pad on center tier for campfire and portal
			Main.spawnTileX = Main.maxTilesX / 2;
			Main.spawnTileY = surfaceY - 3;
			for (int x = Main.spawnTileX - 8; x <= Main.spawnTileX + 14; x++)
			{
				Tile tile = Main.tile[x, surfaceY];
				tile.HasTile = true;
				tile.TileType = TileID.PearlstoneBrick;
			}

			// Place hallowed campfire for arena theming (DECOR-01, style 2 = Hallowed)
			WorldGen.PlaceTile(Main.spawnTileX - 4, surfaceY - 1, TileID.Campfire, mute: true, forced: true, style: 2);
		}
	}
}
