using Terraria;
using Terraria.ID;
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

		// D-01/Pitfall 4 (live-confirmed 2026-08-14): being a non-ModType constructed
		// only inside BossArenaAstralSubworld.Tasks is NOT sufficient on its own --
		// tModLoader's AssemblyManager.JITAssembliesAsync JIT-prefilters every method in
		// the assembly regardless of call-reachability. Without this attribute,
		// CalamityMod-disabled load throws a real JITException naming
		// AstralPlatformPass.ApplyPass. Mirrors the exact discipline already applied to
		// Integrations/CalamityIntegration.cs's per-method tags.
		[JITWhenModsEnabled("CalamityMod")]
		protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
		{
			progress.Message = "Generating astral infection boss arena platform";

			int surfaceY = Main.maxTilesY / 2; // mid-height (400)
			int thickness = 15; // ~200(scan window)*15 = 3000 tiles >> 950 threshold, ample margin

			ushort astralStone = (ushort)ModContent.TileType<AstralStone>();
			ushort astralGrass = (ushort)ModContent.TileType<AstralGrass>();

			// Fill themed background wall behind all 7 arena tiers (DECOR-01, DECOR-03)
			ushort astralWall = ModContent.TryFind<ModWall>("CalamityMod", "AstralDirtWall", out ModWall wall) ? (ushort)wall.Type : (ushort)WallID.EbonstoneUnsafe;
			ArenaBuilder.FillWall(0, Main.maxTilesX, surfaceY - 120, surfaceY + 80, astralWall);

			// 1. Solid astral floor slab below lower platform tiers (provides ~3000 Astral tiles >> 950)
			int floorY = surfaceY + 60;
			for (int x = 0; x < Main.maxTilesX; x++)
			{
				for (int y = floorY; y < floorY + thickness; y++)
				{
					Tile tile = Main.tile[x, y];
					tile.HasTile = true;
					tile.TileType = (y == floorY) ? astralGrass : astralStone;
				}
			}

			// 2. Solid astral ceiling slab above top platform tier
			int ceilingY = surfaceY - 120;
			for (int x = 0; x < Main.maxTilesX; x++)
			{
				for (int y = ceilingY; y < ceilingY + 10; y++)
				{
					Tile tile = Main.tile[x, y];
					tile.HasTile = true;
					tile.TileType = astralStone;
				}
			}

			// 3. Spawn pad on center tier for campfire, portal, and beacon
			Main.spawnTileX = Main.maxTilesX / 2;
			Main.spawnTileY = surfaceY - 3;
			for (int x = Main.spawnTileX - 8; x <= Main.spawnTileX + 14; x++)
			{
				Tile tile = Main.tile[x, surfaceY];
				tile.HasTile = true;
				tile.TileType = astralGrass;
			}

			// Place campfire for arena theming (DECOR-01)
			WorldGen.PlaceTile(Main.spawnTileX - 4, surfaceY - 1, TileID.Campfire, mute: true, forced: true, style: 1);

			// Place Astral Beacon for Astrum Deus summoning
			if (ModContent.TryFind<ModTile>("CalamityMod", "AstralBeacon", out ModTile beaconTile))
			{
				WorldGen.PlaceObject(Main.spawnTileX + 8, surfaceY - 1, beaconTile.Type);
			}
		}
	}
}
