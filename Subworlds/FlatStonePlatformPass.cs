using Terraria;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace BossArenaSubWorld.Subworlds
{
	// A single-purpose GenPass: fills a flat, thin stone platform and nothing else.
	// This absence-by-construction (no ore/structure/biome placement of any kind) is what
	// satisfies SUBW-05 (zero placed mod/vanilla content) -- see 01-RESEARCH.md Pattern 2.
	public class FlatStonePlatformPass : GenPass
	{
		public FlatStonePlatformPass(string name, float loadWeight) : base(name, loadWeight)
		{
		}

		protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
		{
			progress.Message = "Generating boss arena platform";

			int surfaceY = Main.maxTilesY / 2; // mid-height placement (400)
			int thickness = 15;

			// Fill themed background wall behind all 7 arena tiers (DECOR-01)
			ArenaBuilder.FillWall(0, Main.maxTilesX, surfaceY - 120, surfaceY + 80, WallID.Stone);

			// Solid floor foundation below the lower platform tiers
			int floorY = surfaceY + 60;
			for (int x = 0; x < Main.maxTilesX; x++)
			{
				for (int y = floorY; y < floorY + thickness; y++)
				{
					Tile tile = Main.tile[x, y];
					tile.HasTile = true;
					tile.TileType = (y <= floorY + 1) ? TileID.GrayBrick : TileID.Stone;
				}
			}

			// Spawn pad on center tier for campfire, portal, and altar
			Main.spawnTileX = Main.maxTilesX / 2;
			Main.spawnTileY = surfaceY - 3;
			for (int x = Main.spawnTileX - 8; x <= Main.spawnTileX + 14; x++)
			{
				Tile tile = Main.tile[x, surfaceY];
				tile.HasTile = true;
				tile.TileType = TileID.GrayBrick;
			}

			// Place campfire for arena theming and player convenience (DECOR-01)
			WorldGen.PlaceTile(Main.spawnTileX - 4, surfaceY - 1, TileID.Campfire, mute: true, forced: true);

			// Place SCal Altar (Altar of the Accursed) for Supreme Calamitas summoning
			if (ModLoader.HasMod("CalamityMod") && ModContent.TryFind<ModTile>("CalamityMod", "SCalAltar", out ModTile sCalAltarTile))
			{
				WorldGen.PlaceObject(Main.spawnTileX + 8, surfaceY - 1, sCalAltarTile.Type);
			}
		}
	}
}
