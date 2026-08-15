using Terraria;
using Terraria.ID;
using Terraria.IO;
using Terraria.WorldBuilding;
using Terraria.ModLoader;
using SpiritMod.Tiles.Block;

namespace BossArenaSubWorld.Subworlds
{
	// Source: SpiritMod.Biomes.BriarSurfaceBiome.IsBiomeActive (already-decompiled local source,
	// ModReader/SpiritMod/Biomes/BriarSurfaceBiome.cs):
	//   return BiomeTileCounts.InBriar && (player.ZoneSkyHeight || player.ZoneOverworldHeight);
	// i.e. requires the Briar tile-weighted count (BiomeTileCounts.briarCount > 80, weight 1 for
	// SpiritMod.Tiles.Block.BriarGrass only) AND player.Y <= worldSurface (240 at this mod's fixed
	// WorldHeight=800). This mirrors Calamity's modern ModBiome.IsBiomeActive + tile-count-hook
	// mechanism (09-RESEARCH.md), one layer of abstraction below Calamity's own BiomeTileCounterSystem
	// (Spirit's own BiomeTileCounts class). Targets the Surface variant (not Underground) for closer
	// fidelity to Vinewrath Bane's own SpawnModBiomes = BriarSurfaceBiome declaration -- both variants
	// satisfy ARENA-01's literal wording equally, since no AI-level Zone dependency was found for
	// Vinewrath Bane on either (09-RESEARCH.md "No AI-level dependency confirmed").
	//
	// JIT-safety discipline (09-RESEARCH.md Pitfall 4): Spirit type references live ONLY inside this
	// class's using directives and ApplyPass() method body -- same rationale as AstralPlatformPass.cs,
	// applied to SpiritMod instead of CalamityMod.
	public class BriarPlatformPass : GenPass
	{
		public BriarPlatformPass(string name, float loadWeight) : base(name, loadWeight)
		{
		}

		// D-01/Pitfall 4 (proactive fix, same root cause live-confirmed for
		// AstralPlatformPass.cs 2026-08-14): being a non-ModType constructed only inside
		// BossArenaBriarSubworld.Tasks is NOT sufficient on its own -- tModLoader's
		// AssemblyManager.JITAssembliesAsync JIT-prefilters every method in the assembly
		// regardless of call-reachability. Without this attribute, SpiritMod-disabled
		// load would throw the same JITException naming BriarPlatformPass.ApplyPass.
		[JITWhenModsEnabled("SpiritMod")]
		protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
		{
			progress.Message = "Generating briar boss arena platform";

			int surfaceY = 130; // strictly <= worldSurface (240) -- satisfies BriarSurfaceBiome
			int thickness = 15;

			ushort briarGrass = (ushort)ModContent.TileType<BriarGrass>();

			// Fill themed background wall behind all 7 arena tiers (DECOR-01, DECOR-03)
			ushort briarWall = ModContent.TryFind<ModWall>("SpiritMod", "BriarWallUnsafe", out ModWall w) ? (ushort)w.Type : (ushort)WallID.JungleUnsafe;
			ArenaBuilder.FillWall(0, Main.maxTilesX, 40, 195, briarWall);

			// 1. Solid briar floor slab below lower platform tiers (provides ~3000 BriarGrass tiles >> 80)
			int floorY = surfaceY + 42;
			for (int x = 0; x < Main.maxTilesX; x++)
			{
				for (int y = floorY; y < floorY + thickness; y++)
				{
					Tile tile = Main.tile[x, y];
					tile.HasTile = true;
					tile.TileType = briarGrass;
				}
			}

			// 2. Solid briar ceiling slab above top platform tier
			int ceilingY = surfaceY - 75;
			for (int x = 0; x < Main.maxTilesX; x++)
			{
				for (int y = ceilingY; y < ceilingY + 10; y++)
				{
					Tile tile = Main.tile[x, y];
					tile.HasTile = true;
					tile.TileType = briarGrass;
				}
			}

			// 3. Spawn pad on center tier for campfire and portal
			Main.spawnTileX = Main.maxTilesX / 2;
			Main.spawnTileY = surfaceY - 3;
			for (int x = Main.spawnTileX - 8; x <= Main.spawnTileX + 14; x++)
			{
				Tile tile = Main.tile[x, surfaceY];
				tile.HasTile = true;
				tile.TileType = briarGrass;
			}

			// Place campfire for arena theming (DECOR-01, style 3 = Green/Briar)
			WorldGen.PlaceTile(Main.spawnTileX - 4, surfaceY - 1, TileID.Campfire, mute: true, forced: true, style: 3);
		}
	}
}
