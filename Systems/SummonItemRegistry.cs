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

        public override void PostSetupContent()
        {
            Register(ItemID.SlimeCrown, NPCID.KingSlime); // D-08 proof entry
        }

        public static void Register(int itemType, int bossNpcType) =>
            _itemToBoss[itemType] = bossNpcType;

        public static bool TryGetBoss(int itemType, out int bossNpcType) =>
            _itemToBoss.TryGetValue(itemType, out bossNpcType);
    }
}
