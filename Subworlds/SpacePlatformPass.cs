using Terraria;
using Terraria.ID;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace BossArenaSubWorld.Subworlds
{
	// Source: decompiled Terraria.Player.UpdateBiomes() --
	// ZoneSkyHeight = (double)val.Y <= Main.worldSurface * 0.3499999940395355.
	// At this mod's fixed WorldHeight=800, worldSurface=240, so the boundary is tile row 84.
	// No tile composition is required for the flag itself (09-RESEARCH.md Pitfall 1) -- TileID.Stone
	// below is placed purely so the player has something solid to stand on, not because the Zone-flag
	// check reads tile type.
	public class SpacePlatformPass : GenPass
	{
		public SpacePlatformPass(string name, float loadWeight) : base(name, loadWeight)
		{
		}

		protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
		{
			progress.Message = "Generating space boss arena platform";

			int surfaceY = 90; // strictly inside ZoneSkyHeight (<= 168 at WorldHeight=1600)
			int thickness = 10;

			// Fill themed background wall behind all 7 arena tiers (DECOR-01)
			ArenaBuilder.FillWall(0, Main.maxTilesX, 15, 145, WallID.Glass);

			// 1. Solid floor slab below lower platform tiers (y in [128, 138] <= 168)
			int floorY = surfaceY + 38;
			for (int x = 0; x < Main.maxTilesX; x++)
			{
				for (int y = floorY; y < floorY + thickness; y++)
				{
					Tile tile = Main.tile[x, y];
					tile.HasTile = true;
					tile.TileType = (y == floorY) ? TileID.Sunplate : TileID.Stone;
				}
			}

			// 2. Solid ceiling slab above top platform tier (y in [20, 25])
			int ceilingY = surfaceY - 70;
			for (int x = 0; x < Main.maxTilesX; x++)
			{
				for (int y = ceilingY; y < ceilingY + 5; y++)
				{
					Tile tile = Main.tile[x, y];
					tile.HasTile = true;
					tile.TileType = TileID.Sunplate;
				}
			}

			// 3. Spawn pad on center tier for campfire and portal
			Main.spawnTileX = Main.maxTilesX / 2;
			Main.spawnTileY = surfaceY - 3;
			for (int x = Main.spawnTileX - 8; x <= Main.spawnTileX + 14; x++)
			{
				Tile tile = Main.tile[x, surfaceY];
				tile.HasTile = true;
				tile.TileType = TileID.Sunplate;
			}

			// Place ultrabright campfire for arena theming (DECOR-01, style 5 = Ultrabright)
			WorldGen.PlaceTile(Main.spawnTileX - 4, surfaceY - 1, TileID.Campfire, mute: true, forced: true, style: 5);
		}
	}
}
