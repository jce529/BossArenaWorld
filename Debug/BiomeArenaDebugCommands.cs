// DEBUG-ONLY (Phase 9). Temporary tool for ARENA-01's live verification checkpoints (09-VALIDATION.md
// Wave 0 Gaps) -- D-02 forbids any new PERMANENT player-facing entry point, so this file must be
// deleted once Plan 07's checkpoints pass (mirrors Phase 1/2's now-deleted Debug/SubworldDebugCommands.cs).

using Terraria;
using Terraria.ModLoader;
using SubworldLibrary;
using BossArenaSubWorld.Subworlds;

namespace BossArenaSubWorld.Debug
{
	/// <summary>
	/// [DEBUG] Enters one of the 7 new biome boss-arena subworlds by name, for live Zone-flag
	/// verification. None of the 7 BossArenaXSubworld classes referenced here contain any direct
	/// CalamityMod/SpiritMod type reference (confined to their paired XPlatformPass classes, per
	/// 09-RESEARCH.md Pitfall 4) -- so this command's Action() is safe to call regardless of which
	/// weak-referenced mods are installed; entering "astral"/"briar" without the matching
	/// mod installed will surface a runtime error from inside that biome's own PlatformPass instead
	/// of a JIT-load crash, since Tasks/ApplyPass are the only place those types are ever touched.
	/// Dungeon and Sulphurous Sea are excluded per user decision 2026-08-14 (D-07, 09-CONTEXT.md).
	/// Delete this file once Plan 07's checkpoints pass (D-02).
	/// </summary>
	public class BiomeArenaEnterCommand : ModCommand
	{
		public override string Command => "bossarena-enterbiome";

		public override CommandType Type => CommandType.Chat;

		public override string Description =>
			"[DEBUG] Enter a named biome boss arena: hallow, underworld, jungle, space, desert, astral, briar.";

		public override void Action(CommandCaller caller, string input, string[] args)
		{
			if (args.Length < 1)
			{
				caller.Reply("Usage: /bossarena-enterbiome <hallow|underworld|jungle|space|desert|astral|briar>");
				return;
			}

			switch (args[0].ToLowerInvariant())
			{
				case "hallow": SubworldSystem.Enter<BossArenaHallowSubworld>(); break;
				case "underworld": SubworldSystem.Enter<BossArenaUnderworldSubworld>(); break;
				case "jungle": SubworldSystem.Enter<BossArenaJungleSubworld>(); break;
				case "space": SubworldSystem.Enter<BossArenaSpaceSubworld>(); break;
				case "desert": SubworldSystem.Enter<BossArenaDesertSubworld>(); break;
				case "astral": SubworldSystem.Enter<BossArenaAstralSubworld>(); break;
				case "briar": SubworldSystem.Enter<BossArenaBriarSubworld>(); break;
				default:
					caller.Reply("Unknown biome: " + args[0] + ". Valid: hallow, underworld, jungle, space, desert, astral, briar.");
					break;
			}
		}
	}
}
