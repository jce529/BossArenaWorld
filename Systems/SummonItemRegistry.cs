using System;
using System.Collections.Generic;
using Terraria;
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

        // 10-01: polymorphic-item resolver, for summon items that spawn a DIFFERENT boss
        // depending on the player's live Zone state at use-time (e.g. Calamity's
        // MarkofProvidence -- Ceaseless Void / Signus / Storm Weaver). Keyed separately from
        // _itemToBoss so a single item can be EITHER single-boss OR polymorphic, never both.
        private static readonly Dictionary<int, Func<Player, int>> _polymorphicResolvers = new();

        public override void PostSetupContent()
        {
        }

        public static void Register(int itemType, int bossNpcType, Func<bool> canSummon = null)
        {
            _itemToBoss[itemType] = bossNpcType;
            if (canSummon != null)
                _eligibility[itemType] = canSummon;
        }

        // Registers an item whose target boss is resolved at click-time from the player's live
        // state, replicating the real source item's own UseItem() branch logic faithfully
        // (10-RESEARCH.md Architecture Pattern 1) instead of picking one boss arbitrarily.
        // resolveBossNpcType must return -1 when no valid boss applies for the player's current
        // state (mirrors the real item's CanUseItem() == false outcome -- no redirect, no
        // consumption). Reuses the existing _eligibility dictionary for canSummon, same as the
        // single-boss Register() overload -- CanSummon(itemType) already handles both cases.
        public static void RegisterPolymorphic(int itemType, Func<Player, int> resolveBossNpcType, Func<bool> canSummon = null)
        {
            _polymorphicResolvers[itemType] = resolveBossNpcType;
            if (canSummon != null)
                _eligibility[itemType] = canSummon;
        }

        public static bool TryGetBoss(int itemType, out int bossNpcType) =>
            _itemToBoss.TryGetValue(itemType, out bossNpcType);

        // Player-aware overload: checks the polymorphic resolver first (if this item was
        // registered via RegisterPolymorphic), falls back to the plain single-item dictionary
        // otherwise. This is the overload Tiles/Test1Tile.cs calls (Plan 10-01 Task 3) -- the
        // existing TryGetBoss(int, out int) stays available for any other future caller that
        // doesn't need polymorphic support.
        public static bool TryGetBoss(Player player, int itemType, out int bossNpcType)
        {
            if (_polymorphicResolvers.TryGetValue(itemType, out var resolve))
            {
                bossNpcType = resolve(player);
                return bossNpcType != -1;
            }
            return _itemToBoss.TryGetValue(itemType, out bossNpcType);
        }

        // Defaults to true/allowed when no eligibility delegate was registered for this
        // item -- preserves existing behavior for every other boss in the project (King
        // Slime, Hive Mind, Infernon, Thorn, etc. are completely unaffected).
        public static bool CanSummon(int itemType) =>
            !_eligibility.TryGetValue(itemType, out var check) || check();
    }
}
