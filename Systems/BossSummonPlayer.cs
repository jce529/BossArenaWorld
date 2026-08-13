using SubworldLibrary;
using Terraria;
using Terraria.ModLoader;
using BossArenaSubWorld.Subworlds;

namespace BossArenaSubWorld.Systems
{
    // SUBW-04: generic "replay the summon item's effect" mechanism. Vanilla summon items
    // (and the standard modded-item convention) all funnel into NPC.SpawnOnPlayer with a
    // hardcoded boss type -- calling that same primitive here, once, on arrival, is the
    // exact substitute for "replaying" the item's private, non-callable use-logic (see
    // 02-RESEARCH.md Pattern 2 for the full justification of this design choice).
    public class BossSummonPlayer : ModPlayer
    {
        // Set by Test1Tile.NewRightClick (Plan 02-02) right before SubworldSystem.Enter<>()
        // is called. Static + nulled-after-consume: this project is singleplayer-only
        // (Out of Scope: multiplayer per REQUIREMENTS.md), so a single static slot is safe.
        public static int? PendingBossNpcType;

        public override void OnEnterWorld()
        {
            if (!PendingBossNpcType.HasValue) return;
            if (!SubworldSystem.IsActive<BossArenaSubworld>()) return;

            NPC.SpawnOnPlayer(Player.whoAmI, PendingBossNpcType.Value);
            PendingBossNpcType = null; // consume once -- Pitfall 3 guard: prevents
                                        // re-summon on a later, unrelated subworld entry
        }
    }
}
