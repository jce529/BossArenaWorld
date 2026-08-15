using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BossArenaSubWorld.Systems
{
    // D-03: namespaced string keys ("modprefix:boss_name"), decoupled from raw NPC.type --
    // a boss key maps to one or more NPC types, not the reverse (supports future multi-phase bosses).
    // RequiresInfernumToggle (default false): true only for bosses whose AI/behavior actually
    // depends on InfernumMode's per-world toggle being forced active inside the throwaway arena
    // subworld (see BossSummonPlayer.OnEnterWorld() and CalamityIntegration.ForceInfernumModeActiveInArena()).
    // Explicit per-boss flag, not a key-prefix inference -- replaces the prior
    // bossKey.StartsWith("calamity:") heuristic from quick task 260815-to6 (see quick task 260815-u7g).
    public record BossDefinition(int[] NpcTypes, Action ApplyDowned, Func<bool> IsDowned, bool RequiresInfernumToggle = false);

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

        // Exposes the full BossDefinition (not just its key) for callers that need to inspect
        // per-boss flags (e.g. RequiresInfernumToggle) without duplicating the key->definition
        // lookup Apply() already does internally.
        public static bool TryGetDefinitionForNpc(int npcType, out BossDefinition def)
        {
            def = null;
            return TryGetKeyForNpc(npcType, out string key) && _byKey.TryGetValue(key, out def);
        }

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
