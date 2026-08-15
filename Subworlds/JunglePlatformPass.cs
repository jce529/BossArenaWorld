using Terraria;
using Terraria.ID;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace BossArenaSubWorld.Subworlds
{
	// Source: decompiled Terraria.Player.UpdateBiomes() / Terraria.SceneMetrics --
	// ZoneJungle = SceneMetrics.JungleTileCount >= SceneMetrics.JungleTileThreshold (140), a per-tick
	// weighted sum over TileID.Sets.JungleBiome = CreateIntSet(0, 60,1, 61,1, 62,1, 74,1, 226,1, 225,1)
	// -- JungleGrass(60), JunglePlants(61), JungleVines(62), JunglePlants2(74), LihzahrdBrick(226),
	// Hive(225). CRITICAL (09-RESEARCH.md Pitfall 2): Mud (TileID 59, the vanilla Jungle "body"/dirt
	// tile) carries ZERO weight in this table -- unlike Corruption where both Ebonstone (body) and
	// CorruptGrass (surface) carry weight 1. This pass fills the FULL platform thickness with
	// JungleGrass, not a thin surface veneer over Mud, so the weighted count is never silently zero
	// despite an apparently large fill.
	public class JunglePlatformPass : GenPass
	{
		public JunglePlatformPass(string name, float loadWeight) : base(name, loadWeight)
		{
		}

		protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
		{
			progress.Message = "Generating jungle boss arena platform";

			int surfaceY = Main.maxTilesY / 2; // mid-height, matches CorruptionPlatformPass convention
			int thickness = 15;

			// Fill themed background wall behind the arena tiers (DECOR-01)
			ArenaBuilder.FillWall(0, Main.maxTilesX, surfaceY - 60, surfaceY + thickness, WallID.JungleUnsafe);

			for (int x = 0; x < Main.maxTilesX; x++)
			{
				for (int y = surfaceY; y < surfaceY + thickness; y++)
				{
					Tile tile = Main.tile[x, y];
					tile.HasTile = true;
					tile.TileType = (y == surfaceY + thickness - 1) ? TileID.LihzahrdBrick : TileID.JungleGrass;
				}
			}

			Main.spawnTileX = Main.maxTilesX / 2;
			Main.spawnTileY = surfaceY - 3;

			// Place jungle campfire for arena theming (DECOR-01, style 6 = Jungle)
			WorldGen.PlaceTile(Main.spawnTileX - 4, surfaceY - 1, TileID.Campfire, mute: true, forced: true, style: 6);
		}
	}
}
