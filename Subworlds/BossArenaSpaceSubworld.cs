using System.Collections.Generic;
using SubworldLibrary;
using Terraria;
using Terraria.GameContent.Events;
using Terraria.WorldBuilding;

namespace BossArenaSubWorld.Subworlds
{
	// Height-only biome arena (ARENA-01, 09-RESEARCH.md Pattern A / Pitfall 1): player.ZoneSkyHeight
	// is `(double)val.Y <= Main.worldSurface * 0.35` (boundary tile row 84 at this mod's fixed
	// WorldHeight=800, since worldSurface=240) -- a pure vertical-position check with ZERO
	// tile-composition requirement, decompiled from Terraria.Player.UpdateBiomes(). This is
	// structurally different from the tile-weighted family (Corruption/Hallow/Jungle/Desert/Dungeon):
	// only SpacePlatformPass's surfaceY=50 (<= 84, the ZoneSkyHeight boundary) matters for the flag
	// itself. TileID.Stone is placed purely so the player has something solid to stand on, not
	// because the flag check reads tile type.
	//
	// Duplicates (does NOT inherit) BossArenaSubworld's OnEnter/OnExit vanilla-downed-flag
	// snapshot/restore guard verbatim -- required independently per Subworld subclass, since
	// SubworldLibrary's CopyDowned()/ReadCopiedDowned() applies per-subworld, not project-wide.
	// See .planning/debug/resolved/isolation-premise-flag-persistence.md.
	public class BossArenaSpaceSubworld : Subworld
	{
		public const int PlatformWidth = 10000;
		public const int WorldHeight = 800;

		public override int Width => PlatformWidth;
		public override int Height => WorldHeight;

		public override List<GenPass> Tasks => new()
		{
			new SpacePlatformPass("Space Boss Arena Platform", 1f),
			new ArenaPolishPass("Space Arena Polish", 1f, surfaceY: 70, thickness: 10, tierCount: 3, tierSpacing: 18, torchInterval: 30, torchStyle: 5, boundaryMargin: 60)
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
