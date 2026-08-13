// DEBUG-ONLY (Phase 1). Delete this entire file in Phase 2 once the real summon-item redirect lands (D-02).

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using SubworldLibrary;
using BossArenaSubWorld.Subworlds;
using BossArenaSubWorld.Items;

namespace BossArenaSubWorld.Debug
{
	/// <summary>
	/// [DEBUG] Enters the boss arena subworld. Temporary stand-in for Phase 2's real
	/// summon-item redirect (D-01). Delete in Phase 2 (D-02).
	/// </summary>
	public class BossArenaEnterCommand : ModCommand
	{
		public override string Command => "bossarena-enter";

		public override CommandType Type => CommandType.Chat;

		public override string Description => "[DEBUG] Enter the boss arena subworld.";

		public override void Action(CommandCaller caller, string input, string[] args)
		{
			SubworldSystem.Enter<BossArenaSubworld>();
		}
	}

	/// <summary>
	/// [DEBUG] Exits the boss arena subworld back to the main world. Temporary stand-in
	/// for Phase 2's real return flow (D-01). Delete in Phase 2 (D-02).
	/// </summary>
	public class BossArenaExitCommand : ModCommand
	{
		public override string Command => "bossarena-exit";

		public override CommandType Type => CommandType.Chat;

		public override string Description => "[DEBUG] Exit the boss arena subworld.";

		public override void Action(CommandCaller caller, string input, string[] args)
		{
			SubworldSystem.Exit();
		}
	}

	/// <summary>
	/// [DEBUG] Prints the current value of NPC.downedSlimeKing to chat. Read-only observation
	/// tool needed for Plan 04's isolation-proof checkpoint (D-12: observe the flag without any
	/// carrier-item action). Delete in Phase 2 (D-02).
	/// </summary>
	public class BossArenaCheckFlagCommand : ModCommand
	{
		public override string Command => "bossarena-checkflag";

		public override CommandType Type => CommandType.Chat;

		public override string Description => "[DEBUG] Print the current value of NPC.downedSlimeKing to chat.";

		public override void Action(CommandCaller caller, string input, string[] args)
		{
			caller.Reply($"NPC.downedSlimeKing = {Terraria.NPC.downedSlimeKing}");
		}
	}

	/// <summary>
	/// [DEBUG] Gives the player one Test1Item (portal tile) and one Slime Crown, for
	/// exercising the Phase 2 redirect without a real crafting recipe/drop source for
	/// either item yet (D-05). Delete alongside the rest of this file once the real
	/// redirect is verified working (see 02-03-PLAN.md).
	/// </summary>
	public class BossArenaGiveTestItemsCommand : ModCommand
	{
		public override string Command => "bossarena-givetestitems";

		public override CommandType Type => CommandType.Chat;

		public override string Description => "[DEBUG] Give Test1 portal item + Slime Crown for Phase 2 testing.";

		public override void Action(CommandCaller caller, string input, string[] args)
		{
			Player player = caller.Player;
			player.QuickSpawnItem(player.GetSource_Misc("bossarena_debug_give"), ModContent.ItemType<Test1Item>(), 1);
			player.QuickSpawnItem(player.GetSource_Misc("bossarena_debug_give"), ItemID.SlimeCrown, 1);
			caller.Reply("Test1 portal item + Slime Crown given.");
		}
	}
}
