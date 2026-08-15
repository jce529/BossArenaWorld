using System;
using Terraria;
using Terraria.ID;

namespace BossArenaSubWorld.Subworlds
{
    // D-01 (11-CONTEXT.md): Shared, mod-agnostic arena construction helper (BOUND-03, TIER-01, LIGHT-01).
    // All methods take strictly primitive parameters (int, ushort, etc.) and reference zero modded types,
    // ensuring complete JIT safety when called from both untagged and JIT-tagged subworld passes.
    public static class ArenaBuilder
    {
        /// <summary>
        /// Fills a solid rectangle of tiles directly into Main.tile.
        /// </summary>
        public static void FillRectangle(int startX, int endX, int startY, int endY, ushort tileType)
        {
            int minX = Math.Max(0, Math.Min(startX, endX));
            int maxX = Math.Min(Main.maxTilesX, Math.Max(startX, endX));
            int minY = Math.Max(0, Math.Min(startY, endY));
            int maxY = Math.Min(Main.maxTilesY, Math.Max(startY, endY));

            for (int x = minX; x < maxX; x++)
            {
                for (int y = minY; y < maxY; y++)
                {
                    Tile tile = Main.tile[x, y];
                    tile.HasTile = true;
                    tile.TileType = tileType;
                }
            }
        }

        /// <summary>
        /// Places an invisible solid boundary box around the arena platform to prevent void falls,
        /// edge knockback falls, and vertical containment breach (BOUND-03).
        /// </summary>
        public static void PlaceBoundaryContainment(int startX, int endX, int surfaceY, int thickness, int boundaryMargin, ushort boundaryTileType)
        {
            int minX = Math.Max(0, startX);
            int maxX = Math.Min(Main.maxTilesX, endX);

            int topY = Math.Max(0, surfaceY - boundaryMargin);
            int bottomY = Math.Min(Main.maxTilesY, surfaceY + thickness + 10);

            // 1. Bottom floor slab (catches player falling below the main platform)
            FillRectangle(minX, maxX, surfaceY + thickness + 1, surfaceY + thickness + 7, boundaryTileType);

            // 2. Ceiling barrier slab (prevents jumping/flying infinitely above the arena)
            FillRectangle(minX, maxX, Math.Max(0, topY - 5), topY, boundaryTileType);

            // 3. Left boundary column
            FillRectangle(minX, Math.Min(maxX, minX + 4), topY, bottomY, boundaryTileType);

            // 4. Right boundary column
            FillRectangle(Math.Max(minX, maxX - 4), maxX, topY, bottomY, boundaryTileType);
        }

        /// <summary>
        /// Builds multi-tier platforms spaced vertically per vanilla jump-height convention (TIER-01).
        /// Tier 0 is assumed to be the solid base at baseSurfaceY; tiers 1..(tierCount-1) are placed above it.
        /// </summary>
        public static void BuildTierPlatforms(int startX, int endX, int baseSurfaceY, int tierCount, int tierSpacing, ushort platformTileType, int platformStyle = 0)
        {
            int minX = Math.Max(0, startX);
            int maxX = Math.Min(Main.maxTilesX, endX);

            for (int t = 1; t < tierCount; t++)
            {
                int tierY = baseSurfaceY - (t * tierSpacing);
                if (tierY <= 0)
                    continue;

                for (int x = minX; x < maxX; x++)
                {
                    Tile tile = Main.tile[x, tierY];
                    tile.HasTile = true;
                    tile.TileType = platformTileType;
                    tile.TileFrameY = (short)(platformStyle * 18);
                }
            }
        }

        /// <summary>
        /// Places torches at regular intervals along a platform/ground tier for visibility (LIGHT-01).
        /// Note: Torch placement is strictly visual and does not alter enemy spawn rates in Terraria (LIGHT-02).
        /// </summary>
        public static void PlaceTorchInterval(int startX, int endX, int y, int interval, int torchStyle = 0)
        {
            if (interval <= 0 || y <= 1 || y >= Main.maxTilesY)
                return;

            int minX = Math.Max(0, startX);
            int maxX = Math.Min(Main.maxTilesX, endX);

            // Offset starting torch by half interval for balanced symmetry
            int startOffset = (interval / 2);

            for (int x = minX + startOffset; x < maxX; x += interval)
            {
                WorldGen.PlaceTile(x, y - 1, TileID.Torches, mute: true, forced: true, style: torchStyle);
            }
        }

        /// <summary>
        /// Places the in-arena return portal tile near the spawn point (ENTRY-02).
        /// </summary>
        public static void PlaceReturnPortal(int x, int y, ushort portalTileType)
        {
            if (x < 0 || x >= Main.maxTilesX || y < 0 || y >= Main.maxTilesY)
                return;

            WorldGen.PlaceTile(x, y, portalTileType, mute: true, forced: true);
        }
    }
}
