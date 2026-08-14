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

			int surfaceY = 50; // well under the ZoneSkyHeight boundary (84) with margin -- near the top of the 800-row world, NOT mid-height
			int thickness = 10;

			for (int x = 0; x < Main.maxTilesX; x++)
			{
				for (int y = surfaceY; y < surfaceY + thickness; y++)
				{
					Tile tile = Main.tile[x, y];
					tile.HasTile = true;
					tile.TileType = TileID.Stone;
				}
			}

			Main.spawnTileX = Main.maxTilesX / 2;
			Main.spawnTileY = surfaceY - 3;
		}
	}
}
