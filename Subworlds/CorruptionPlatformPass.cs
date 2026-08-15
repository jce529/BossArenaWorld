using Terraria;
using Terraria.ID;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace BossArenaSubWorld.Subworlds
{
	// A single-purpose GenPass: fills a flat, thin platform made of Corruption-biome tiles
	// (TileID.Ebonstone body, TileID.CorruptGrass surface row) instead of plain Stone -- the
	// Corruption-flavored counterpart to FlatStonePlatformPass, built to satisfy vanilla's real
	// ZoneCorrupt biome detection, not to fake it.
	//
	// WHY (see .planning/debug/resolved/hivemind-zonecorrupt-despawn-corruption-subworld.md):
	// decompiling the installed tModLoader.dll confirms Terraria.Player.UpdateBiomes() sets
	// `ZoneCorrupt = Main.SceneMetrics.EnoughTilesForCorruption`, i.e.
	// `EvilTileCount >= SceneMetrics.CorruptionTileThreshold` (300), recomputed EVERY TICK from a
	// live tile scan -- not a one-shot/sticky flag. `EvilTileCount` is a weighted sum
	// (Terraria.ModLoader.TileLoader.RecountTiles) over `TileID.Sets.CorruptBiome[tileType]`,
	// which gives weight 1 to Ebonstone (25), CorruptGrass (23), CorruptPlants (24),
	// CorruptThorns (32), Ebonsand (112), CorruptIce (163), CorruptHardenedSand (398),
	// CorruptSandstone (400), CorruptJungleGrass (661) -- and weight -10 to Sunflower (27),
	// which this pass never places. The scan area is a rectangle roughly
	// Main.buffScanAreaWidth x Main.buffScanAreaHeight (~200 x ~140 tiles at max supported
	// resolution) centered on the player, every tick.
	//
	// Filling the ENTIRE platform width (not just a small patch near spawn) with Ebonstone
	// guarantees ZoneCorrupt stays true no matter where the player/boss drifts mid-fight: any
	// ~20-column-wide slice of this platform's 15-tile-thick Ebonstone layer alone already
	// totals 300 (20 * 15), and the actual scan window is roughly 10x wider than that slice.
	public class CorruptionPlatformPass : GenPass
	{
		public CorruptionPlatformPass(string name, float loadWeight) : base(name, loadWeight)
		{
		}

		protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
		{
			progress.Message = "Generating corruption boss arena platform";

			int surfaceY = Main.maxTilesY / 2; // mid-height placement (400)
			int thickness = 15;

			// Fill themed background wall behind all 7 arena tiers (DECOR-01)
			ArenaBuilder.FillWall(0, Main.maxTilesX, surfaceY - 120, surfaceY + 80, WallID.EbonstoneUnsafe);

			// 1. Solid corruption floor slab below lower platform tiers (provides ~3000 Ebonstone tiles >> 300)
			int floorY = surfaceY + 60;
			for (int x = 0; x < Main.maxTilesX; x++)
			{
				for (int y = floorY; y < floorY + thickness; y++)
				{
					Tile tile = Main.tile[x, y];
					tile.HasTile = true;
					tile.TileType = (y == floorY) ? TileID.CorruptGrass : ((y == floorY + 1) ? TileID.EbonstoneBrick : TileID.Ebonstone);
				}
			}

			// 2. Solid corruption ceiling slab above top platform tier (guarantees ZoneCorrupt even when high up)
			int ceilingY = surfaceY - 120;
			for (int x = 0; x < Main.maxTilesX; x++)
			{
				for (int y = ceilingY; y < ceilingY + 10; y++)
				{
					Tile tile = Main.tile[x, y];
					tile.HasTile = true;
					tile.TileType = TileID.Ebonstone;
				}
			}

			// 3. Spawn pad on center tier for campfire and portal
			Main.spawnTileX = Main.maxTilesX / 2;
			Main.spawnTileY = surfaceY - 3;
			for (int x = Main.spawnTileX - 8; x <= Main.spawnTileX + 14; x++)
			{
				Tile tile = Main.tile[x, surfaceY];
				tile.HasTile = true;
				tile.TileType = TileID.EbonstoneBrick;
			}

			// Place corrupt campfire for arena theming (DECOR-01, style 1 = Corrupt)
			WorldGen.PlaceTile(Main.spawnTileX - 4, surfaceY - 1, TileID.Campfire, mute: true, forced: true, style: 1);
		}
	}
}
