using Terraria;
using Terraria.ID;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace BossArenaSubWorld.Subworlds
{
	// Source: decompiled Terraria.Player.UpdateBiomes() / Terraria.SceneMetrics --
	// ZoneDesert = SceneMetrics.SandTileCount >= SceneMetrics.DesertTileThreshold (1500), a per-tick
	// weighted sum over TileID.Sets.SandBiome (weight 1 each: Sand=53, Ebonsand=112, Pearlsand=116,
	// Crimsand=234, plus the Hardened/Sandstone tile families -- Sandstone counts EQUALLY to Sand,
	// confirmed via 09-RESEARCH.md's decompiled weight table). 09-RESEARCH.md Pitfall 5: Desert's
	// threshold (1500) is 5-12x higher than every other tile-weighted biome in this project
	// (Corruption=300, Hallow=125, Jungle=140, Dungeon=250) -- this pass uses thickness=20 (not the
	// usual 15) for extra safety margin, since the default 15-tile convention clears the threshold
	// with only ~2x margin instead of Corruption's ~10x.
	//
	// BUGFIX (live checkpoint, 2026-08-14): the first version of this pass filled all 20 rows with
	// TileID.Sand. Sand is a member of TileID.Sets.Falling (gravity tile), and a 10000-tile-wide strip
	// of Sand with nothing solid underneath it triggers vanilla WorldGen's fall-check machinery
	// (WorldGen.SquareTileFrame -> WorldGen.TileFrame -> WorldGen.SpawnFallingBlockProjectile) to
	// recurse across every unsupported tile in the strip, mutually re-triggering itself with no
	// terminating base case and blowing the call stack (confirmed via Natives.log: "Stack overflow."
	// inside exactly this SquareTileFrame/TileFrame/SpawnFallingBlockProjectile cycle). Underworld's
	// UnderworldPlatformPass avoided this by keeping its own falling-adjacent cosmetic tile (Ash) to a
	// single top row sitting on solid Hellstone; the same fix is applied here: only the top layer is
	// real Sand (falling, but fully supported by solid Sandstone directly beneath), and the remaining
	// depth is TileID.Sandstone -- solid/non-falling, and per the weight table above it counts equally
	// toward SceneMetrics.SandTileCount, so this changes nothing about clearing the 1500 threshold.
	public class DesertPlatformPass : GenPass
	{
		public DesertPlatformPass(string name, float loadWeight) : base(name, loadWeight)
		{
		}

		protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
		{
			progress.Message = "Generating desert boss arena platform";

			int surfaceY = Main.maxTilesY / 2; // mid-height, matches CorruptionPlatformPass convention
			int thickness = 20; // wider margin than the usual 15 -- see class comment (Pitfall 5)
			int sandSurfaceRows = 3; // top rows only -- see BUGFIX comment above; rest is solid Sandstone

			for (int x = 0; x < Main.maxTilesX; x++)
			{
				for (int y = surfaceY; y < surfaceY + thickness; y++)
				{
					Tile tile = Main.tile[x, y];
					tile.HasTile = true;
					tile.TileType = (y < surfaceY + sandSurfaceRows) ? TileID.Sand : TileID.Sandstone;
				}
			}

			Main.spawnTileX = Main.maxTilesX / 2;
			Main.spawnTileY = surfaceY - 3;
		}
	}
}
