using System.Collections.Generic;
using SubworldLibrary;
using Terraria.WorldBuilding;

namespace BossArenaSubWorld.Subworlds
{
	// The dedicated boss-arena subworld: a single flat stone platform and nothing else.
	// ShouldSave = false is the actual mechanism that guarantees SUBW-05 never accumulates
	// placed content across visits -- the arena regenerates from Tasks every entry instead
	// of loading from a saved file. NoPlayerSaving must stay false (Pitfall 1) or the
	// player's live inventory (and, from Phase 3+, the carrier item) is discarded on exit.
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
	}
}
