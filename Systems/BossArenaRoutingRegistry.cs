using System;
using System.Collections.Generic;
using SubworldLibrary;
using BossArenaSubWorld.Subworlds;

namespace BossArenaSubWorld.Systems
{
    // Boss-agnostic routing: maps a boss NPC type to the specific SubworldLibrary arena it
    // needs (e.g. the Corruption-biome BossArenaCorruptionSubworld for Corruption-gated bosses
    // like Hive Mind, vs the plain content-free BossArenaSubworld for bosses like King Slime
    // that need no biome). Bosses with no explicit registration fall back to the original plain
    // arena, so existing Phase 2/3 behavior (King Slime -> BossArenaSubworld) is unchanged by
    // construction.
    //
    // See .planning/debug/resolved/hivemind-zonecorrupt-despawn-corruption-subworld.md for why
    // this exists: Hive Mind's AI despawns it almost instantly on the plain arena's bare-stone
    // platform because it requires player.ZoneCorrupt, which only a real biome (a different
    // subworld) can satisfy.
    public static class BossArenaRoutingRegistry
    {
        private static readonly Dictionary<int, Func<bool>> _enterByBoss = new();

        // Every arena Subworld type ever routed to (including the default). Lets boss-agnostic
        // callers (BossSummonPlayer.OnEnterWorld, BossCoreDropRule.CanDrop) ask "is the CURRENT
        // subworld one of THIS MOD's boss arenas" without hardcoding a per-type OR-chain that
        // would need editing every time a new arena subworld is added.
        private static readonly HashSet<Type> _knownArenaTypes = new() { typeof(BossArenaSubworld) };

        public static void Register<T>(int bossNpcType) where T : Subworld
        {
            _enterByBoss[bossNpcType] = SubworldSystem.Enter<T>;
            _knownArenaTypes.Add(typeof(T));
        }

        public static bool Enter(int bossNpcType) =>
            _enterByBoss.TryGetValue(bossNpcType, out var enter)
                ? enter()
                : SubworldSystem.Enter<BossArenaSubworld>();

        public static bool IsAnyArenaActive()
        {
            Subworld current = SubworldSystem.Current;
            return current != null && _knownArenaTypes.Contains(current.GetType());
        }
    }
}
