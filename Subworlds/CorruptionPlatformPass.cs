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

			int surfaceY = Main.maxTilesY / 2; // mid-height placement, matches FlatStonePlatformPass
			int thickness = 15; // same thickness guideline as FlatStonePlatformPass (D-07)

			// Fill themed background wall behind the arena tiers (DECOR-01)
			ArenaBuilder.FillWall(0, Main.maxTilesX, surfaceY - 60, surfaceY + thickness, WallID.EbonstoneUnsafe);

			for (int x = 0; x < Main.maxTilesX; x++)
			{
				for (int y = surfaceY; y < surfaceY + thickness; y++)
				{
					Tile tile = Main.tile[x, y];
					tile.HasTile = true;
					tile.TileType = (y == surfaceY) ? TileID.CorruptGrass : ((y == surfaceY + 1) ? TileID.EbonstoneBrick : TileID.Ebonstone);
				}
			}

			// Set spawn point above the platform surface explicitly
			Main.spawnTileX = Main.maxTilesX / 2;
			Main.spawnTileY = surfaceY - 3;

			// Place corrupt campfire for arena theming (DECOR-01, style 1 = Corrupt)
			WorldGen.PlaceTile(Main.spawnTileX - 4, surfaceY - 1, TileID.Campfire, mute: true, forced: true, style: 1);
		}
	}
}
