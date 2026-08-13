using System.Collections.Generic;
using SubworldLibrary;
using Terraria;
using Terraria.GameContent.Events;
using Terraria.WorldBuilding;

namespace BossArenaSubWorld.Subworlds
{
	// A SECOND dedicated boss-arena subworld, identical in purpose/mechanics to
	// Subworlds/BossArenaSubworld.cs except its platform is Corruption-biome-flavored
	// (Ebonstone + Corrupt Grass, via CorruptionPlatformPass) instead of plain Stone.
	//
	// WHY THIS EXISTS (see
	// .planning/debug/resolved/hivemind-zonecorrupt-despawn-corruption-subworld.md):
	// CalamityMod's HiveMind.AI() (decompiled from the installed CalamityMod.dll) re-caps
	// NPC.timeLeft to ~1 second on every tick its target is invalid, and one of its
	// target-validity conditions is `!player.ZoneCorrupt && !BossRushEvent.BossRushActive`. The
	// plain BossArenaSubworld's bare-stone platform never sets player.ZoneCorrupt, so Hive Mind
	// despawns almost immediately after NPC.SpawnOnPlayer(). Any future Corruption-gated boss
	// should route here too (see Systems/BossArenaRoutingRegistry.cs), rather than forcing
	// ZoneCorrupt via a ModPlayer override -- that alternative was explicitly considered and
	// rejected by the user: a real biome is a more faithful reproduction of the boss's actual
	// intended arena, and avoids an ever-growing pile of per-boss Zone* overrides in
	// BiomeOverridePlayer for every future biome-gated boss.
	//
	// BossArenaSubworld.cs itself is intentionally left untouched -- it remains the correct (and
	// default) arena for bosses that need no biome (e.g. King Slime, Phase 3). This class
	// duplicates (does NOT inherit) BossArenaSubworld's OnEnter/OnExit vanilla-downed-flag
	// snapshot/restore guard, specifically so BossArenaSubworld.cs does not need to change shape.
	// See .planning/debug/resolved/isolation-premise-flag-persistence.md for the full root-cause
	// explanation of why this guard is required for EVERY Subworld subclass in this mod
	// independently (SubworldLibrary's CopyDowned()/ReadCopiedDowned() bidirectional vanilla-flag
	// sync applies to any subworld, not just the first one written).
	public class BossArenaCorruptionSubworld : Subworld
	{
		public const int PlatformWidth = 10000;
		public const int WorldHeight = 800;

		public override int Width => PlatformWidth;
		public override int Height => WorldHeight;

		public override List<GenPass> Tasks => new()
		{
			new CorruptionPlatformPass("Corruption Boss Arena Platform", 1f)
		};

		public override bool ShouldSave => false;
		public override bool NoPlayerSaving => false;

		// Duplicated verbatim from BossArenaSubworld.cs -- see class-level comment above for why.
		private bool _downedSlimeKing;
		private bool _downedBoss1;
		private bool _downedBoss2;
		private bool _downedBoss3;
		private bool _downedQueenBee;
		private bool _downedDeerclops;
		private bool _downedQueenSlime;
		private bool _downedMechBoss1;
		private bool _downedMechBoss2;
		private bool _downedMechBoss3;
		private bool _downedMechBossAny;
		private bool _downedPlantBoss;
		private bool _downedGolemBoss;
		private bool _downedFishron;
		private bool _downedEmpressOfLight;
		private bool _downedAncientCultist;
		private bool _downedTowerSolar;
		private bool _downedTowerVortex;
		private bool _downedTowerNebula;
		private bool _downedTowerStardust;
		private bool _downedMoonlord;
		private bool _downedGoblins;
		private bool _downedClown;
		private bool _downedFrost;
		private bool _downedPirates;
		private bool _downedMartians;
		private bool _downedHalloweenTree;
		private bool _downedHalloweenKing;
		private bool _downedChristmasTree;
		private bool _downedChristmasSantank;
		private bool _downedChristmasIceQueen;
		private bool _downedInvasionT1;
		private bool _downedInvasionT2;
		private bool _downedInvasionT3;

		public override void OnEnter()
		{
			_downedSlimeKing = NPC.downedSlimeKing;
			_downedBoss1 = NPC.downedBoss1;
			_downedBoss2 = NPC.downedBoss2;
			_downedBoss3 = NPC.downedBoss3;
			_downedQueenBee = NPC.downedQueenBee;
			_downedDeerclops = NPC.downedDeerclops;
			_downedQueenSlime = NPC.downedQueenSlime;
			_downedMechBoss1 = NPC.downedMechBoss1;
			_downedMechBoss2 = NPC.downedMechBoss2;
			_downedMechBoss3 = NPC.downedMechBoss3;
			_downedMechBossAny = NPC.downedMechBossAny;
			_downedPlantBoss = NPC.downedPlantBoss;
			_downedGolemBoss = NPC.downedGolemBoss;
			_downedFishron = NPC.downedFishron;
			_downedEmpressOfLight = NPC.downedEmpressOfLight;
			_downedAncientCultist = NPC.downedAncientCultist;
			_downedTowerSolar = NPC.downedTowerSolar;
			_downedTowerVortex = NPC.downedTowerVortex;
			_downedTowerNebula = NPC.downedTowerNebula;
			_downedTowerStardust = NPC.downedTowerStardust;
			_downedMoonlord = NPC.downedMoonlord;
			_downedGoblins = NPC.downedGoblins;
			_downedClown = NPC.downedClown;
			_downedFrost = NPC.downedFrost;
			_downedPirates = NPC.downedPirates;
			_downedMartians = NPC.downedMartians;
			_downedHalloweenTree = NPC.downedHalloweenTree;
			_downedHalloweenKing = NPC.downedHalloweenKing;
			_downedChristmasTree = NPC.downedChristmasTree;
			_downedChristmasSantank = NPC.downedChristmasSantank;
			_downedChristmasIceQueen = NPC.downedChristmasIceQueen;
			_downedInvasionT1 = DD2Event.DownedInvasionT1;
			_downedInvasionT2 = DD2Event.DownedInvasionT2;
			_downedInvasionT3 = DD2Event.DownedInvasionT3;
		}

		public override void OnExit()
		{
			NPC.downedSlimeKing = _downedSlimeKing;
			NPC.downedBoss1 = _downedBoss1;
			NPC.downedBoss2 = _downedBoss2;
			NPC.downedBoss3 = _downedBoss3;
			NPC.downedQueenBee = _downedQueenBee;
			NPC.downedDeerclops = _downedDeerclops;
			NPC.downedQueenSlime = _downedQueenSlime;
			NPC.downedMechBoss1 = _downedMechBoss1;
			NPC.downedMechBoss2 = _downedMechBoss2;
			NPC.downedMechBoss3 = _downedMechBoss3;
			NPC.downedMechBossAny = _downedMechBossAny;
			NPC.downedPlantBoss = _downedPlantBoss;
			NPC.downedGolemBoss = _downedGolemBoss;
			NPC.downedFishron = _downedFishron;
			NPC.downedEmpressOfLight = _downedEmpressOfLight;
			NPC.downedAncientCultist = _downedAncientCultist;
			NPC.downedTowerSolar = _downedTowerSolar;
			NPC.downedTowerVortex = _downedTowerVortex;
			NPC.downedTowerNebula = _downedTowerNebula;
			NPC.downedTowerStardust = _downedTowerStardust;
			NPC.downedMoonlord = _downedMoonlord;
			NPC.downedGoblins = _downedGoblins;
			NPC.downedClown = _downedClown;
			NPC.downedFrost = _downedFrost;
			NPC.downedPirates = _downedPirates;
			NPC.downedMartians = _downedMartians;
			NPC.downedHalloweenTree = _downedHalloweenTree;
			NPC.downedHalloweenKing = _downedHalloweenKing;
			NPC.downedChristmasTree = _downedChristmasTree;
			NPC.downedChristmasSantank = _downedChristmasSantank;
			NPC.downedChristmasIceQueen = _downedChristmasIceQueen;
			DD2Event.DownedInvasionT1 = _downedInvasionT1;
			DD2Event.DownedInvasionT2 = _downedInvasionT2;
			DD2Event.DownedInvasionT3 = _downedInvasionT3;
		}
	}
}
