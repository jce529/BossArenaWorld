using Terraria;
using Terraria.ID;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace BossArenaSubWorld.Subworlds
{
	// Source: decompiled Terraria.Player.UpdateBiomes() --
	// ZoneUnderworldHeight = (double)val.Y > Main.UnderworldLayer, where
	// Main.UnderworldLayer => maxTilesY - 200 (a computed property, always live).
	// At this mod's fixed WorldHeight=800, UnderworldLayer=600, so surfaceY must be > 600.
	// No tile composition is required for the flag itself (09-RESEARCH.md Pitfall 1) --
	// TileID.Ash/Hellstone below are placed purely for cosmetic fidelity to vanilla's real
	// Underworld biome, not because the Zone-flag check reads them.
	public class UnderworldPlatformPass : GenPass
	{
		public UnderworldPlatformPass(string name, float loadWeight) : base(name, loadWeight)
		{
		}

		protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
		{
			progress.Message = "Generating underworld boss arena platform";

			int surfaceY = 1090; // strictly > UnderworldLayer (1000 at WorldHeight=1200)
			int thickness = 10;

			// Fill themed background wall behind all 7 arena tiers (DECOR-01)
			ArenaBuilder.FillWall(0, Main.maxTilesX, 1010, 1150, WallID.ObsidianBrick);

			// 1. Solid floor slab below lower platform tiers (y in [1128, 1138] > 1000)
			int floorY = surfaceY + 38;
			for (int x = 0; x < Main.maxTilesX; x++)
			{
				for (int y = floorY; y < floorY + thickness; y++)
				{
					Tile tile = Main.tile[x, y];
					tile.HasTile = true;
					tile.TileType = (y == floorY) ? TileID.ObsidianBrick : ((y == floorY + 1) ? TileID.Ash : TileID.Hellstone);
				}
			}

			// 2. Solid ceiling slab above top platform tier (y in [1020, 1025] > 1000)
			int ceilingY = surfaceY - 70;
			for (int x = 0; x < Main.maxTilesX; x++)
			{
				for (int y = ceilingY; y < ceilingY + 5; y++)
				{
					Tile tile = Main.tile[x, y];
					tile.HasTile = true;
					tile.TileType = TileID.ObsidianBrick;
				}
			}

			// 3. Spawn pad on center tier for campfire and portal
			Main.spawnTileX = Main.maxTilesX / 2;
			Main.spawnTileY = surfaceY - 3;
			for (int x = Main.spawnTileX - 8; x <= Main.spawnTileX + 14; x++)
			{
				Tile tile = Main.tile[x, surfaceY];
				tile.HasTile = true;
				tile.TileType = TileID.ObsidianBrick;
			}

			// Place demon campfire for arena theming (DECOR-01, style 4 = Demon)
			WorldGen.PlaceTile(Main.spawnTileX - 4, surfaceY - 1, TileID.Campfire, mute: true, forced: true, style: 4);
		}
	}
}
