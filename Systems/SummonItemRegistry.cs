using System;
using System.Collections.Generic;
using Terraria.ID;
using Terraria.ModLoader;

namespace BossArenaSubWorld.Systems
{
    // SUBW-01: central, data-driven registry mapping a summon item's Item.type to the
    // boss NPC.type it should spawn. Scope limited to "simple use-to-summon" items only
    // (D-07) -- altar-thrown/bulb-break triggers are explicitly out of scope this phase.
    public class SummonItemRegistry : ModSystem
    {
        private static readonly Dictionary<int, int> _itemToBoss = new();

        // 06-02 user-requested scope addition: optional per-item eligibility delegate,
        // mirroring BossRegistry's BossDefinition.ApplyDowned/IsDowned named-delegate
        // convention. Only populated when a source mod's real item has a genuine
        // permanent-lockout condition (e.g. CatalystMod's AstralCommunicator becoming
        // unusable once Moon Lord is downed before Astrageldon) that this project's
        // portal-redirect pipeline would otherwise silently ignore since it bypasses the
        // item's own CanUseItem()/UseItem() entirely (Phase 2 D-09).
        private static readonly Dictionary<int, Func<bool>> _eligibility = new();

        public override void PostSetupContent()
        {
            Register(ItemID.SlimeCrown, NPCID.KingSlime); // D-08 proof entry
        }

        public static void Register(int itemType, int bossNpcType, Func<bool> canSummon = null)
        {
            _itemToBoss[itemType] = bossNpcType;
            if (canSummon != null)
                _eligibility[itemType] = canSummon;
        }

        public static bool TryGetBoss(int itemType, out int bossNpcType) =>
            _itemToBoss.TryGetValue(itemType, out bossNpcType);

        // Defaults to true/allowed when no eligibility delegate was registered for this
        // item -- preserves existing behavior for every other boss in the project (King
        // Slime, Hive Mind, Infernon, Thorn, etc. are completely unaffected).
        public static bool CanSummon(int itemType) =>
            !_eligibility.TryGetValue(itemType, out var check) || check();
    }
}
