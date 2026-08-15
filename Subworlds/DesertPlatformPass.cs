using Terraria;
using Terraria.ID;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace BossArenaSubWorld.Subworlds
{
	// Source: decompiled Terraria.Player.UpdateBiomes() / Terraria.SceneMetrics --
	// ZoneDesert = SceneMetrics.SandTileCount >= SceneMetrics.DesertTileThreshold (1500), a per-tick
	// weighted sum over TileID.Sets.SandBiome (weight 1 each: Sand=53, Ebonsand=112, Pearlsand=116,
	// Crimsand=234, plus the Hardened/Sandstone tile families -- Sandstone counts EQUALLY to Sand,
	// confirmed via 09-RESEARCH.md's decompiled weight table). 09-RESEARCH.md Pitfall 5: Desert's
	// threshold (1500) is 5-12x higher than every other tile-weighted biome in this project
	// (Corruption=300, Hallow=125, Jungle=140, Dungeon=250) -- this pass uses thickness=20 (not the
	// usual 15) for extra safety margin, since the default 15-tile convention clears the threshold
	// with only ~2x margin instead of Corruption's ~10x.
	//
	// BUGFIX (live checkpoint, 2026-08-14): the first version of this pass filled all 20 rows with
	// TileID.Sand. Sand is a member of TileID.Sets.Falling (gravity tile), and a 10000-tile-wide strip
	// of Sand with nothing solid underneath it triggers vanilla WorldGen's fall-check machinery
	// (WorldGen.SquareTileFrame -> WorldGen.TileFrame -> WorldGen.SpawnFallingBlockProjectile) to
	// recurse across every unsupported tile in the strip, mutually re-triggering itself with no
	// terminating base case and blowing the call stack (confirmed via Natives.log: "Stack overflow."
	// inside exactly this SquareTileFrame/TileFrame/SpawnFallingBlockProjectile cycle). Underworld's
	// UnderworldPlatformPass avoided this by keeping its own falling-adjacent cosmetic tile (Ash) to a
	// single top row sitting on solid Hellstone; the same fix is applied here: only the top layer is
	// real Sand (falling, but fully supported by solid Sandstone directly beneath), and the remaining
	// depth is TileID.Sandstone -- solid/non-falling, and per the weight table above it counts equally
	// toward SceneMetrics.SandTileCount, so this changes nothing about clearing the 1500 threshold.
	public class DesertPlatformPass : GenPass
	{
		public DesertPlatformPass(string name, float loadWeight) : base(name, loadWeight)
		{
		}

		protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
		{
			progress.Message = "Generating desert boss arena platform";

			int surfaceY = Main.maxTilesY / 2; // mid-height (400)
			int thickness = 20; // 20-thick solid Sandstone/HardenedSand (>> 1500 threshold)
			int sandSurfaceRows = 3;

			// Fill themed background wall behind all 7 arena tiers (DECOR-01)
			ArenaBuilder.FillWall(0, Main.maxTilesX, surfaceY - 120, surfaceY + 80, WallID.SandstoneBrick);

			// 1. Solid desert floor slab below lower platform tiers (provides ~4000 Sandstone tiles >> 1500)
			int floorY = surfaceY + 60;
			for (int x = 0; x < Main.maxTilesX; x++)
			{
				for (int y = floorY; y < floorY + thickness; y++)
				{
					Tile tile = Main.tile[x, y];
					tile.HasTile = true;
					tile.TileType = (y < floorY + sandSurfaceRows) ? TileID.Sand : ((y == floorY + sandSurfaceRows) ? TileID.SandstoneBrick : TileID.Sandstone);
				}
			}

			// 2. Solid sandstone ceiling slab above top platform tier
			int ceilingY = surfaceY - 120;
			for (int x = 0; x < Main.maxTilesX; x++)
			{
				for (int y = ceilingY; y < ceilingY + 12; y++)
				{
					Tile tile = Main.tile[x, y];
					tile.HasTile = true;
					tile.TileType = TileID.Sandstone;
				}
			}

			// 3. Spawn pad on center tier for campfire and portal
			Main.spawnTileX = Main.maxTilesX / 2;
			Main.spawnTileY = surfaceY - 3;
			for (int x = Main.spawnTileX - 8; x <= Main.spawnTileX + 14; x++)
			{
				Tile tile = Main.tile[x, surfaceY];
				tile.HasTile = true;
				tile.TileType = TileID.SandstoneBrick;
			}

			// Place desert campfire for arena theming (DECOR-01, style 7 = Desert)
			WorldGen.PlaceTile(Main.spawnTileX - 4, surfaceY - 1, TileID.Campfire, mute: true, forced: true, style: 7);
		}
	}
}
