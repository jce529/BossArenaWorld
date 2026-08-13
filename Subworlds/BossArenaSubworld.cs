using System.Collections.Generic;
using SubworldLibrary;
using Terraria;
using Terraria.GameContent.Events;
using Terraria.WorldBuilding;

namespace BossArenaSubWorld.Subworlds
{
	// The dedicated boss-arena subworld: a single flat stone platform and nothing else.
	// ShouldSave = false is the actual mechanism that guarantees SUBW-05 never accumulates
	// placed content across visits -- the arena regenerates from Tasks every entry instead
	// of loading from a saved file. NoPlayerSaving must stay false (Pitfall 1) or the
	// player's live inventory (and, from Phase 3+, the carrier item) is discarded on exit.
	//
	// OnEnter/OnExit below defend against a confirmed SubworldLibrary v2.2.3.2 behavior (see
	// .planning/debug/resolved/isolation-premise-flag-persistence.md): SubworldSystem's
	// private CopyDowned()/ReadCopiedDowned() helpers bidirectionally sync a hardcoded
	// whitelist of ~30 vanilla NPC/DD2Event "downed" flags between the main world and any
	// subworld on EVERY entry/exit, independent of ShouldSave/NoPlayerSaving and independent
	// of any carrier-item mechanism. Confirmed call order (SubworldSystem.ExitWorldCallBack):
	// cache.OnExit() fires, THEN CopyMainWorldData() (-> CopyDowned()) captures whatever the
	// vanilla flags currently are, THEN (after the real main-world file is correctly reloaded
	// from disk) ReadCopiedMainWorldData() (-> ReadCopiedDowned()) unconditionally overwrites
	// the just-correctly-loaded flags with that captured snapshot. Without this guard, any
	// vanilla boss/event flag that changes while inside the arena (e.g. a real King Slime
	// kill) leaks back into the main world with none of the correctness safeguards
	// (achievements, netcode sync, WorldGen side effects) the carrier-item pipeline is meant
	// to provide. Snapshotting the true main-world values on OnEnter and force-restoring them
	// on OnExit (which runs BEFORE CopyMainWorldData() captures anything) makes SubworldLibrary's
	// later re-application of copiedData a no-op instead of a leak.
	public class BossArenaSubworld : Subworld
	{
		public const int PlatformWidth = 10000;
		public const int WorldHeight = 800;

		public override int Width => PlatformWidth;
		public override int Height => WorldHeight;

		public override List<GenPass> Tasks => new()
		{
			new FlatStonePlatformPass("Flat Stone Platform", 1f)
		};

		public override bool ShouldSave => false;
		public override bool NoPlayerSaving => false;

		// Snapshot of the main world's vanilla downed/event flags, captured in OnEnter and
		// restored in OnExit. Field list intentionally mirrors SubworldLibrary's own
		// CopyDowned()/ReadCopiedDowned() whitelist (SubworldSystem.cs) exactly, so nothing
		// on their sync list is missed.
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
