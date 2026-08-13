using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using BossArenaSubWorld.Systems;

namespace BossArenaSubWorld.Items
{
    // DROP-03: BossKey is this item's instance data, set at spawn time by BossCoreDropRule
    // (Plan 03-02). CloneNewInstances=true + Clone override are required because Item.Clone()
    // (e.g. inventory stack-split) does NOT run a save/load round-trip -- without this override,
    // BossKey would revert to the default constructor's empty string on any split/duplicate.
    public class BossCoreItem : ModItem
    {
        public string BossKey = string.Empty;

        protected override bool CloneNewInstances => true;

        public override ModItem Clone(Item newEntity)
        {
            BossCoreItem clone = (BossCoreItem)base.Clone(newEntity);
            clone.BossKey = BossKey;
            return clone;
        }

        public override void SaveData(TagCompound tag) => tag["BossKey"] = BossKey;

        public override void LoadData(TagCompound tag) => BossKey = tag.GetString("BossKey");

        public override void SetDefaults()
        {
            Item.maxStack = 1;
            Item.consumable = true;
            Item.value = 0;
            Item.width = 20;
            Item.height = 20;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = Item.useAnimation = 20;
        }

        // APPLY-01/APPLY-04/D-02: consume only on Applied/AlreadyDowned; retain + explain on UnknownKey.
        public override bool? UseItem(Player player)
        {
            switch (BossRegistry.Apply(BossKey))
            {
                case ApplyResult.Applied:
                    Main.NewText($"Boss credential applied: {BossKey}", Color.LimeGreen);
                    return true;
                case ApplyResult.AlreadyDowned:
                    Main.NewText($"This boss was already marked defeated ({BossKey}).", Color.Yellow);
                    return true;
                case ApplyResult.UnknownKey:
                default:
                    Main.NewText($"Could not apply boss credential '{BossKey}' -- registry lookup failed. Item was not consumed; please report this.", Color.Red);
                    return false;
            }
        }
    }
}
