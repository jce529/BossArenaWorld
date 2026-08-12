using System;
using Terraria;
using Terraria.ModLoader;
using SubworldLibrary;
using BossArenaSubWorld.Subworlds;

namespace BossArenaSubWorld.Systems
{
	/// <summary>
	/// Generic, reusable infrastructure for force-setting Player.Zone* biome flags while
	/// inside the boss arena subworld. Phase 1 scope (D-09): the hook and guard exist, but
	/// no boss-to-biome mapping is wired up yet -- that lands in Phase 3+ as per-boss content
	/// is registered.
	///
	/// PostUpdate() runs after vanilla's own per-tick Zone* flag computation (confirmed via
	/// source read, see 01-RESEARCH.md), so overrides applied here reliably win for that tick.
	/// Because vanilla recomputes Zone* flags every tick regardless of the previous tick's
	/// override, any future per-boss override must be re-applied every tick (via this
	/// PostUpdate hook) rather than being a one-shot patch.
	/// </summary>
	public class BiomeOverridePlayer : ModPlayer
	{
		/// <summary>
		/// Generic, reusable entry point for later phases to force any Player.Zone* flag(s)
		/// via a lambda, without this phase needing to know which flags any specific boss
		/// requires.
		/// </summary>
		public static void ForceZone(Player player, Action<Player> apply) => apply(player);

		public override void PostUpdate()
		{
			if (!SubworldSystem.IsActive<BossArenaSubworld>())
				return;

			// Phase 1: infrastructure only, no active override applied.
			// Future example (Phase 3+, per-boss): Player.ZoneJungle = true;
		}
	}
}
