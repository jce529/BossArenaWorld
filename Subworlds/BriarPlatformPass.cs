using Terraria;
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

		protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
		{
			progress.Message = "Generating briar boss arena platform";

			int surfaceY = 150; // <= worldSurface (240) -- satisfies the Surface variant's ZoneOverworldHeight requirement
			int thickness = 15; // ~200(scan window)*15 = 3000 tiles >> 80 threshold, ample margin

			ushort briarGrass = (ushort)ModContent.TileType<BriarGrass>();

			for (int x = 0; x < Main.maxTilesX; x++)
			{
				for (int y = surfaceY; y < surfaceY + thickness; y++)
				{
					Tile tile = Main.tile[x, y];
					tile.HasTile = true;
					tile.TileType = briarGrass;
				}
			}

			Main.spawnTileX = Main.maxTilesX / 2;
			Main.spawnTileY = surfaceY - 3;
		}
	}
}
