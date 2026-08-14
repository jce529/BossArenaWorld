// DEBUG-ONLY (Phase 9). Temporary tool for ARENA-01's live verification checkpoints (09-VALIDATION.md
// Wave 0 Gaps) -- D-02 forbids any new PERMANENT player-facing entry point, so this file must be
// deleted once Plan 07's checkpoints pass (mirrors Phase 1/2's now-deleted Debug/SubworldDebugCommands.cs).

using System;
using System.Reflection;
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

	/// <summary>
	/// [DEBUG] Prints the current player's relevant Zone/Biome flags to chat -- vanilla flags always,
	/// Calamity/Spirit flags only if the respective mod is installed (JIT-guarded per 09-RESEARCH.md
	/// Pitfall 4 discipline, mirroring Integrations/CalamityIntegration.cs's named-method-not-lambda
	/// pattern). Delete this file once Plan 07's checkpoints pass (D-02).
	/// </summary>
	public class BiomeArenaCheckFlagsCommand : ModCommand
	{
		public override string Command => "bossarena-checkbiomeflags";

		public override CommandType Type => CommandType.Chat;

		public override string Description => "[DEBUG] Print the current player's biome Zone flags to chat.";

		public override void Action(CommandCaller caller, string input, string[] args)
		{
			Player player = Main.LocalPlayer;
			caller.Reply(
				$"ZoneHallow={player.ZoneHallow} ZoneUnderworldHeight={player.ZoneUnderworldHeight} " +
				$"ZoneJungle={player.ZoneJungle} ZoneSkyHeight={player.ZoneSkyHeight} " +
				$"ZoneDesert={player.ZoneDesert} ZoneDungeon={player.ZoneDungeon}");

			if (ModLoader.HasMod("CalamityMod"))
				PrintCalamityFlags(caller, player);

			if (ModLoader.HasMod("SpiritMod"))
				PrintSpiritFlags(caller);
		}

		// Action() above never touches a Calamity type directly, only this named, separately-tagged
		// method does -- see 09-RESEARCH.md Pitfall 4 / Integrations/CalamityIntegration.cs's
		// established discipline. Must stay a named method, never an inline lambda (Phase 4 Pitfall 2:
		// inline lambdas referencing weak-referenced mod types get hoisted into a <>c cache-class
		// method that does NOT inherit the enclosing method's JIT guard).
		//
		// Deviation from plan (Rule 3 - blocking compile error): the plan's illustrative
		// `player.Calamity()` requires the extension method's declaring namespace (`CalamityMod`,
		// on `CalamityMod.CalamityUtils.Calamity(this Player)`, confirmed via ilspycmd decompile
		// of the installed Libs/CalamityMod.dll) to be in scope. Calling the static class
		// directly (`CalamityMod.CalamityUtils.Calamity(player)`) avoids adding a project-wide
		// `using CalamityMod;` directive while keeping the exact same public extension method.
		[JITWhenModsEnabled("CalamityMod")]
		private static void PrintCalamityFlags(CommandCaller caller, Player player)
		{
			var calPlayer = CalamityMod.CalamityUtils.Calamity(player);
			caller.Reply($"ZoneAstral={calPlayer.ZoneAstral} ZoneSulphur={calPlayer.ZoneSulphur}");
		}

		// Deviation from plan (Rule 3 - blocking compile error, CS0122): decompiling
		// Libs/SpiritMod.dll (ilspycmd) confirms `SpiritMod.Biomes.BiomeTileCounts` is declared
		// `internal class BiomeTileCounts : ModSystem` -- its `public static bool InBriar`
		// property is therefore unreachable at compile time from this assembly, exactly the same
		// "public member on an internal type" shape as Integrations/SpiritIntegration.cs's
		// BossDownedTracker (Pitfall A precedent there). No public wrapper exists, so read via
		// reflection into the property getter -- mirrors that file's established
		// cached-reflection-with-try/catch discipline, applied to a read instead of a write.
		[JITWhenModsEnabled("SpiritMod")]
		private static void PrintSpiritFlags(CommandCaller caller)
		{
			try
			{
				Type biomeTileCountsType = ModLoader.GetMod("SpiritMod").Code.GetType("SpiritMod.Biomes.BiomeTileCounts");
				PropertyInfo inBriarProperty = biomeTileCountsType.GetProperty("InBriar", BindingFlags.Public | BindingFlags.Static);
				bool inBriar = (bool)inBriarProperty.GetValue(null);
				caller.Reply($"Briar.InBriar={inBriar}");
			}
			catch (Exception e)
			{
				ModContent.GetInstance<BossArenaSubWorld>().Logger.Warn(
					"BiomeArenaCheckFlagsCommand: failed to read SpiritMod.Biomes.BiomeTileCounts.InBriar via reflection: " + e);
				caller.Reply("Briar.InBriar=<reflection failed, see log>");
			}
		}
	}
}
