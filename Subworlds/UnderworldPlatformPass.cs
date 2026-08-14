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

			int surfaceY = 650; // > UnderworldLayer (600) with margin -- near the bottom of the 800-row world, NOT mid-height
			int thickness = 10;

			for (int x = 0; x < Main.maxTilesX; x++)
			{
				for (int y = surfaceY; y < surfaceY + thickness; y++)
				{
					Tile tile = Main.tile[x, y];
					tile.HasTile = true;
					tile.TileType = (y == surfaceY) ? TileID.Ash : TileID.Hellstone; // cosmetic only, see class comment
				}
			}

			// Set spawn point above the platform surface explicitly -- LoadSubworld()'s blind
			// midpoint default would otherwise leave the player mid-air/underground (Pitfall 2,
			// mirrors FlatStonePlatformPass/CorruptionPlatformPass).
			Main.spawnTileX = Main.maxTilesX / 2;
			Main.spawnTileY = surfaceY - 3;
		}
	}
}
