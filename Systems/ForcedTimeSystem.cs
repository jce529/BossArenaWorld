using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace BossArenaSubWorld.Systems
{
    // D-04: forced day/night utility for bosses whose AI despawns outside a specific
    // time-of-day (Spirit's Moon Jelly Wizard, Dusking -- unconditional night; Calamity's
    // Astrum Deus/Astrum Aureus -- conditional night, ONLY when InfernumMode is loaded,
    // per D-02). No new player-facing UI/item -- purely an automatic subworld-setup step,
    // consistent with this project's existing "zero new player action beyond the portal
    // tile" design (matches CONTEXT.md D-04's explicit framing).
    //
    // Pitfall 6 (10-RESEARCH.md): a single Subworld.OnEnter() time-set is NOT confirmed
    // sufficient for a multi-minute fight -- whether Main.time advances normally inside a
    // SubworldLibrary subworld during an active fight was flagged as unconfirmed by
    // decompile. This re-asserts every tick via PreUpdateWorld, for the WHOLE arena
    // visit, not just once on entry, so it is safe regardless of the answer.
    public class ForcedTimeSystem : ModSystem
    {
        private static readonly HashSet<int> _forceNightBosses = new();

        // Set by Tiles/Test1Tile.cs alongside BossSummonPlayer.PendingBossNpcType, right
        // before BossArenaRoutingRegistry.Enter() is called (Plan 10-01 Task 3). Unlike
        // PendingBossNpcType, this is NOT nulled after the boss spawns -- it must persist
        // for the whole arena visit so PreUpdateWorld can keep re-forcing night every tick.
        // Singleplayer-only project (REQUIREMENTS.md Out of Scope: multiplayer), so a
        // single static slot is safe -- same justification as PendingBossNpcType.
        public static int? ActiveArenaBossNpcType;

        public static void RegisterForceNight(int bossNpcType) => _forceNightBosses.Add(bossNpcType);

        public override void PreUpdateWorld()
        {
            if (!ActiveArenaBossNpcType.HasValue) return;
            if (!_forceNightBosses.Contains(ActiveArenaBossNpcType.Value)) return;
            // Guard: only force time while genuinely inside a registered boss arena --
            // never touch the main world's real day/night cycle, even if
            // ActiveArenaBossNpcType is still set from a prior visit (it is intentionally
            // never cleared on exit -- this guard alone makes that safe).
            if (!BossArenaRoutingRegistry.IsAnyArenaActive()) return;

            Main.dayTime = false;
            Main.time = 0.0; // midnight -- maximizes buffer before Main.dayTime flips true
        }
    }
}
