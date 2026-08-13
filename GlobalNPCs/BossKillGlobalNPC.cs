using Terraria;
using Terraria.ModLoader;
using BossArenaSubWorld.ItemDropRules;
using BossArenaSubWorld.Systems;

namespace BossArenaSubWorld.GlobalNPCs
{
    // DROP-01/DROP-02: attaches BossCoreDropRule unconditionally at mod-load for every
    // registered boss NPC type -- the dynamic subworld gate lives inside the rule itself
    // (BossCoreDropRule.CanDrop), not here, since this hook only fires once per NPC type
    // at load time, not per kill (see 03-RESEARCH.md Pitfall 1).
    public class BossKillGlobalNPC : GlobalNPC
    {
        public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
        {
            if (BossRegistry.TryGetKeyForNpc(npc.type, out string key))
                npcLoot.Add(new BossCoreDropRule(key));
        }
    }
}
