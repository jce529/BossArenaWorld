using Terraria;
using Terraria.IO;
using Terraria.WorldBuilding;
using Terraria.ModLoader;
using CalamityMod.Tiles.Astral;

namespace BossArenaSubWorld.Subworlds
{
	// Source: CalamityMod.BiomeManagers.AstralInfectionBiome.IsBiomeActive (decompiled):
	//   return !player.ZoneDungeon && BiomeTileCounterSystem.AstralTiles > 950;
	// CalamityMod.Systems.BiomeTileCounterSystem.TileCountsAvailable (decompiled) sums, weight 1 each:
	// AstralSand/AstralSandstone/HardenedAstralSand/CelestialRemains/AstralIce/AstralSnow/AstralDirt/
	// AstralStone/AstralGrass/AstralOre/NovaeSlag/AstralClay. Calamity does NOT extend vanilla's
	// TileID.Sets/SceneMetrics weighted-count system for its own biomes -- it uses the modern
	// ModBiome.IsBiomeActive + ModSystem.TileCountsAvailable hook (09-RESEARCH.md, structurally
	// different family from vanilla's Hallow/Jungle/Desert/Dungeon).
	//
	// JIT-safety discipline (09-RESEARCH.md Pitfall 4): Calamity type references live ONLY inside this
	// class's using directives and ApplyPass() method body -- this class is NOT a ModType (unlike
	// Subworld), so it is never autoloaded/instantiated until BossArenaAstralSubworld.Tasks actually
	// constructs it, which itself only happens inside SubworldSystem.LoadSubworld().
	public class AstralPlatformPass : GenPass
	{
		public AstralPlatformPass(string name, float loadWeight) : base(name, loadWeight)
		{
		}

		protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
		{
			progress.Message = "Generating astral infection boss arena platform";

			int surfaceY = Main.maxTilesY / 2; // no height constraint for ZoneAstral itself
			int thickness = 15; // ~200(scan window)*15 = 3000 tiles >> 950 threshold, ample margin

			ushort astralStone = (ushort)ModContent.TileType<AstralStone>();
			ushort astralGrass = (ushort)ModContent.TileType<AstralGrass>();

			for (int x = 0; x < Main.maxTilesX; x++)
			{
				for (int y = surfaceY; y < surfaceY + thickness; y++)
				{
					Tile tile = Main.tile[x, y];
					tile.HasTile = true;
					tile.TileType = (y == surfaceY) ? astralGrass : astralStone;
					// No dungeon wall/tiles placed anywhere in this arena -- required so
					// !player.ZoneDungeon holds (IsBiomeActive's AND-condition).
				}
			}

			Main.spawnTileX = Main.maxTilesX / 2;
			Main.spawnTileY = surfaceY - 3;
		}
	}
}
