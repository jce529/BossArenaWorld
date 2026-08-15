using System;
using Terraria;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace BossArenaSubWorld.Subworlds
{
    // Shared GenPass: applies boundary containment, multi-tier platform decking (upper and lower), and interval torches (BOUND-03, TIER-01, LIGHT-01).
    // Parameterized per arena so that each subworld can supply its own surfaceY, thickness, tier counts, and biome torch style.
    public class ArenaPolishPass : GenPass
    {
        private readonly int _surfaceY;
        private readonly int _thickness;
        private readonly int _upperTierCount;
        private readonly int _lowerTierCount;
        private readonly int _tierSpacing;
        private readonly int _torchInterval;
        private readonly int _torchStyle;
        private readonly int _boundaryMargin;

        public ArenaPolishPass(
            string name,
            float loadWeight,
            int surfaceY,
            int thickness,
            int upperTierCount = 4,
            int lowerTierCount = 2,
            int tierSpacing = 26,
            int torchInterval = 30,
            int torchStyle = 0,
            int boundaryMargin = 60) : base(name, loadWeight)
        {
            _surfaceY = surfaceY;
            _thickness = thickness;
            _upperTierCount = upperTierCount;
            _lowerTierCount = lowerTierCount;
            _tierSpacing = tierSpacing;
            _torchInterval = torchInterval;
            _torchStyle = torchStyle;
            _boundaryMargin = boundaryMargin;
        }

        protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = "Polishing boss arena";

            ushort boundaryTileType = (ushort)ModContent.TileType<Tiles.BoundaryTile>();

            int topTierY = _surfaceY - (_upperTierCount * _tierSpacing);
            int bottomTierY = _surfaceY + (_lowerTierCount * _tierSpacing);
            int ceilingY = Math.Max(10, topTierY - _boundaryMargin);
            int floorY = Math.Min(Main.maxTilesY - 10, bottomTierY + _thickness + 10);

            // 1. Boundary containment (BOUND-03)
            ArenaBuilder.PlaceBoundaryContainment(0, Main.maxTilesX, ceilingY, floorY, boundaryTileType);

            // 2. Multi-tier platforms (TIER-01) - upper tiers, center tier, lower tiers
            ArenaBuilder.BuildTierPlatforms(0, Main.maxTilesX, _surfaceY, _upperTierCount, _lowerTierCount, _tierSpacing, TileID.Platforms, 0);

            // 3. Torches along center tier (LIGHT-01, LIGHT-02)
            ArenaBuilder.PlaceTorchInterval(0, Main.maxTilesX, _surfaceY, _torchInterval, _torchStyle);

            // 4. Torches along each upper tier (LIGHT-01, LIGHT-02)
            for (int t = 1; t <= _upperTierCount; t++)
            {
                int tierY = _surfaceY - (t * _tierSpacing);
                if (tierY > 0)
                {
                    ArenaBuilder.PlaceTorchInterval(0, Main.maxTilesX, tierY, _torchInterval, _torchStyle);
                }
            }

            // 5. Torches along each lower tier (LIGHT-01, LIGHT-02)
            for (int b = 1; b <= _lowerTierCount; b++)
            {
                int tierY = _surfaceY + (b * _tierSpacing);
                if (tierY < Main.maxTilesY)
                {
                    ArenaBuilder.PlaceTorchInterval(0, Main.maxTilesX, tierY, _torchInterval, _torchStyle);
                }
            }

            // 6. In-arena Return Portal near spawn point (ENTRY-02)
            ushort portalTileType = (ushort)ModContent.TileType<Tiles.ReturnPortalTile>();
            ArenaBuilder.PlaceReturnPortal((Main.maxTilesX / 2) + 4, _surfaceY - 1, portalTileType);
        }
    }
}
