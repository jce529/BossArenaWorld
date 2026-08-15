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

			int surfaceY = 670; // strictly > UnderworldLayer (600) with 3 tiers (670, 642, 614) all staying > 600
			int thickness = 10;

			// Fill themed background wall behind the arena tiers (DECOR-01)
			ArenaBuilder.FillWall(0, Main.maxTilesX, surfaceY - 65, surfaceY + thickness, WallID.ObsidianBrick);

			for (int x = 0; x < Main.maxTilesX; x++)
			{
				for (int y = surfaceY; y < surfaceY + thickness; y++)
				{
					Tile tile = Main.tile[x, y];
					tile.HasTile = true;
					tile.TileType = (y == surfaceY) ? TileID.ObsidianBrick : ((y == surfaceY + 1) ? TileID.Ash : TileID.Hellstone);
				}
			}

			// Set spawn point above the platform surface explicitly
			Main.spawnTileX = Main.maxTilesX / 2;
			Main.spawnTileY = surfaceY - 3;

			// Place demon campfire for arena theming (DECOR-01, style 4 = Demon)
			WorldGen.PlaceTile(Main.spawnTileX - 4, surfaceY - 1, TileID.Campfire, mute: true, forced: true, style: 4);
		}
	}
}
