using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BossArenaSubWorld.Systems
{
    // D-03: namespaced string keys ("modprefix:boss_name"), decoupled from raw NPC.type --
    // a boss key maps to one or more NPC types, not the reverse (supports future multi-phase bosses).
    public record BossDefinition(int[] NpcTypes, Action ApplyDowned, Func<bool> IsDowned);

    // APPLY-04: 3-state result drives BossCoreItem's D-02 consume-vs-retain policy.
    public enum ApplyResult { Applied, AlreadyDowned, UnknownKey }

    public class BossRegistry : ModSystem
    {
        private static readonly Dictionary<string, BossDefinition> _byKey = new();
        private static readonly Dictionary<int, string> _npcTypeToKey = new();

        public override void PostSetupContent()
        {
            // D-04: replay vanilla's own downed-flag helper (flag + achievement notify +
            // MessageID.WorldData netcode sync + Lantern Night trigger), not a raw assignment.
            // gameEventId = -1 per tModLoader Migration Guide's documented safe value (see
            // 03-RESEARCH.md Open Question 1).
            Register("vanilla:king_slime", new BossDefinition(
                NpcTypes: new int[] { NPCID.KingSlime },
                ApplyDowned: () => NPC.SetEventFlagCleared(ref NPC.downedSlimeKing, -1),
                IsDowned: () => NPC.downedSlimeKing));
        }

        public static void Register(string key, BossDefinition def)
        {
            _byKey[key] = def;
            foreach (int t in def.NpcTypes) _npcTypeToKey[t] = key;
        }

        public static bool TryGetKeyForNpc(int npcType, out string key) =>
            _npcTypeToKey.TryGetValue(npcType, out key);

        // D-01: idempotency via live-flag check, no separate applied-tracking set.
        public static ApplyResult Apply(string key)
        {
            if (!_byKey.TryGetValue(key, out var def))
                return ApplyResult.UnknownKey;

            if (def.IsDowned())
                return ApplyResult.AlreadyDowned;

            def.ApplyDowned();
            return ApplyResult.Applied;
        }
    }
}
