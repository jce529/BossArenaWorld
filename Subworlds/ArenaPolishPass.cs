using Terraria;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace BossArenaSubWorld.Subworlds
{
    // Shared GenPass: applies boundary containment, multi-tier platform decking, and interval torches (BOUND-03, TIER-01, LIGHT-01).
    // Parameterized per arena so that each subworld can supply its own surfaceY, thickness, and biome torch style.
    public class ArenaPolishPass : GenPass
    {
        private readonly int _surfaceY;
        private readonly int _thickness;
        private readonly int _tierCount;
        private readonly int _tierSpacing;
        private readonly int _torchInterval;
        private readonly int _torchStyle;
        private readonly int _boundaryMargin;

        public ArenaPolishPass(
            string name,
            float loadWeight,
            int surfaceY,
            int thickness,
            int tierCount = 3,
            int tierSpacing = 28,
            int torchInterval = 30,
            int torchStyle = 0,
            int boundaryMargin = 120) : base(name, loadWeight)
        {
            _surfaceY = surfaceY;
            _thickness = thickness;
            _tierCount = tierCount;
            _tierSpacing = tierSpacing;
            _torchInterval = torchInterval;
            _torchStyle = torchStyle;
            _boundaryMargin = boundaryMargin;
        }

        protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = "Polishing boss arena";

            ushort boundaryTileType = (ushort)ModContent.TileType<Tiles.BoundaryTile>();

            // 1. Boundary containment (BOUND-03)
            ArenaBuilder.PlaceBoundaryContainment(0, Main.maxTilesX, _surfaceY, _thickness, _boundaryMargin, boundaryTileType);

            // 2. Multi-tier platforms (TIER-01)
            ArenaBuilder.BuildTierPlatforms(0, Main.maxTilesX, _surfaceY, _tierCount, _tierSpacing, TileID.Platforms, 0);

            // 3. Torches along base ground (LIGHT-01, LIGHT-02)
            ArenaBuilder.PlaceTorchInterval(0, Main.maxTilesX, _surfaceY, _torchInterval, _torchStyle);

            // 4. Torches along each upper tier (LIGHT-01, LIGHT-02)
            for (int t = 1; t < _tierCount; t++)
            {
                int tierY = _surfaceY - (t * _tierSpacing);
                if (tierY > 0)
                {
                    ArenaBuilder.PlaceTorchInterval(0, Main.maxTilesX, tierY, _torchInterval, _torchStyle);
                }
            }

            // 5. In-arena Return Portal near spawn point (ENTRY-02)
            ushort portalTileType = (ushort)ModContent.TileType<Tiles.ReturnPortalTile>();
            ArenaBuilder.PlaceReturnPortal((Main.maxTilesX / 2) + 4, _surfaceY - 1, portalTileType);
        }
    }
}
