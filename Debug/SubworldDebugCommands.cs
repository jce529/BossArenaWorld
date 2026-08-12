// DEBUG-ONLY (Phase 1). Delete this entire file in Phase 2 once the real summon-item redirect lands (D-02).

using Terraria;
using Terraria.ModLoader;
using SubworldLibrary;
using BossArenaSubWorld.Subworlds;

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
}
