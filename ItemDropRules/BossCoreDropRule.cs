using System.Collections.Generic;
using SubworldLibrary;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ModLoader;
using BossArenaSubWorld.Items;
using BossArenaSubWorld.Subworlds;

namespace BossArenaSubWorld.ItemDropRules
{
    // DROP-02: the subworld gate MUST live here (CanDrop, evaluated fresh per actual kill),
    // NOT as an if-check in GlobalNPC.ModifyNPCLoot -- that hook runs once per NPC type at
    // mod-load time, so any dynamic check written there would be frozen at whatever value was
    // true when the mod loaded (see 03-RESEARCH.md Pitfall 1).
    public class BossCoreDropRule : IItemDropRule
    {
        private readonly string _bossKey;

        public BossCoreDropRule(string bossKey) => _bossKey = bossKey;

        public List<IItemDropRuleChainAttempt> ChainedRules { get; } = new();

        public bool CanDrop(DropAttemptInfo info) =>
            SubworldSystem.IsActive<BossArenaSubworld>();

        // DROP-03: BossKey is set here, at spawn time, inside the drop rule -- immediately
        // after Item.NewItem returns the spawned item's Main.item[] index.
        public ItemDropAttemptResult TryDroppingItem(DropAttemptInfo info)
        {
            int index = Item.NewItem(
                info.npc.GetSource_Loot("BossArenaSubWorld:BossCoreDrop"),
                info.npc.getRect(),
                ModContent.ItemType<BossCoreItem>(),
                1);

            if (Main.item[index].ModItem is BossCoreItem coreItem)
                coreItem.BossKey = _bossKey;

            return new ItemDropAttemptResult { State = ItemDropAttemptResultState.Success };
        }

        public void ReportDroprates(List<DropRateInfo> drops, DropRateInfoChainFeed ratesInfo) { }
    }
}
