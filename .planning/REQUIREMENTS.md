# Requirements: BossArenaSubWorld v1.1

**Defined:** 2026-08-15
**Core Value (project-wide):** The generic boss-kill → carrier-item → main-world-apply mechanism must reliably reproduce a boss's full "downed" state (flags, netcode sync, WorldGen side effects) in the main world, for any registered boss.
**This milestone's focus:** Retrofit the 9 existing arena subworlds (1 plain + 8 biome variants: Corruption, Hallow, Underworld, Jungle, Space, Desert, Astral, Briar) with safety, layout, lighting, theming, and entry/exit-convenience improvements — the core carrier-item pipeline itself is out of scope (already shipped, v1.0).

## v1.1 Requirements

### Arena Boundary & Safety (BOUND)

- [ ] **BOUND-01**: Every one of the 9 arena subworlds has a fall/void boundary (solid containment below/around the platform) so a missed jump or knockback cannot send the player into the open void above/below the platform slab
- [ ] **BOUND-02**: For the 2 arenas whose Zone flag is a pure-Y check (Space: `y <= 84`, Underworld: `y > 600`), the boundary is placed strictly inside that Y-window, not just "near" the platform — going outside it would silently despawn the boss (same failure class as the already-fixed Hive Mind bug)
- [ ] **BOUND-03**: Boundary/containment logic is implemented as a shared, mod-agnostic helper (e.g. `ArenaBuilder` + a shared `ArenaPolishPass`) parameterized per-arena by that arena's own `surfaceY`/thickness — never a single hardcoded literal shared across all 9 arenas
- [ ] **BOUND-04**: The shared boundary/polish helper's public API never references a modded type directly (stays `ushort`/`int`-typed), so it can be safely called from both JIT-tagged (Astral/Briar) and untagged arena passes without introducing new JIT-unsafe surface

### Multi-Tier Platform Layout (TIER)

- [ ] **TIER-01**: Each of the 9 arenas gains a multi-tier (2-4 layer) platform structure, vertically spaced per vanilla jump-height convention, sized to accommodate that arena's registered bosses' attack/movement patterns (ground, flying, area-denial)
- [ ] **TIER-02**: Adding tiers does not drop any biome-gated arena (Corruption/Hallow/Underworld-height/Jungle/Space-height/Desert/Astral/Briar) below its existing Zone-flag qualifying threshold — each biome's documented tile-weight budget (e.g. Desert 1500, Astral 950, Jungle 140) is preserved
- [ ] **TIER-03**: A player who climbs to an upper tier does not drift the Zone-flag scan window (which is centered on the player, not the boss) off the base biome-qualifying tile strip mid-fight — verified live per biome, with Desert and Jungle (tightest tile-weight margins) checked most carefully

### Lighting (LIGHT)

- [ ] **LIGHT-01**: Each arena has torches placed at regular intervals along its platform tiers for visibility, using a biome-appropriate `TorchID` style where one exists
- [ ] **LIGHT-02**: Torch placement is documented/scoped as visual-only — it does not suppress or reduce monster spawns (light level has no effect on spawn rate in Terraria; that is explicitly out of this milestone's scope, see Out of Scope)

### Biome-Themed Decoration (DECOR)

- [ ] **DECOR-01**: Each of the 9 arenas has biome-legible decorative tiles/background matching its theme (Corruption: chasms/purple grass; Hallow: crystal tones; Underworld: ash/obsidian/lava glow; Jungle: mud/vines; Desert: dunes/sandstone; Astral/Briar: mod-native palettes; plain arena: a neutral/generic theme)
- [ ] **DECOR-02**: Decoration is additive to (or drawn from) each biome's existing Zone-flag-qualifying tile ID set, not a wholesale replacement with non-qualifying cosmetic tiles — preserving the tile-weight budget each `*PlatformPass` already depends on
- [ ] **DECOR-03**: Astral and Briar's decoration code stays inside their existing `[JITWhenModsEnabled]`-guarded `ApplyPass()` methods (no decoration logic touching modded types moves into a shared/untagged helper)

### Entry & Exit Convenience (ENTRY)

- [ ] **ENTRY-01**: Entering the arena subworld no longer auto-summons the boss immediately; the player instead uses the held boss-summon item directly (its normal use-effect) whenever ready, giving the player control over prep timing
- [ ] **ENTRY-02**: Each arena has a visible return-point marker (a portal tile placed near arena spawn) that calls `SubworldSystem.Exit()` on use, without bypassing or reordering the existing vanilla-downed-flag snapshot/restore guard in `OnEnter()`/`OnExit()`
- [ ] **ENTRY-03**: SubworldLibrary's existing built-in Return button/flow remains intact and is not replaced or duplicated by the new return-point marker — the marker is an additional convenience, not a second exit mechanism

## v2 Requirements

Deferred to future release. Tracked but not in current roadmap.

### Spawn Control

- **SPWN-01**: Safe-wall-based unwanted-monster-spawn suppression layered onto the fall-prevention boundary walls — blocked on live-verifying whether a `GenPass`-placed wall inherits "safe" spawn-suppression status the same way a player-placed one does (LOW confidence per research)

### Combat Assist

- **COMBAT-01**: "Duck under" solid corner blocks at each arena's top platform tier, mirroring the vanilla Moon Lord-arena convention for dodging beam/laser attacks — natural follow-up once multi-tier platforms (TIER-01) exist

## Out of Scope

Explicitly excluded. Documented to prevent scope creep.

| Feature | Reason |
|---------|--------|
| Buff stations (Campfire / Heart Lantern / Honey pool) | Excluded per PROJECT.md's stated v1.1 scope ("리소스/버프 지점" explicitly out) |
| New biome-variant arenas (Dungeon, Sulphurous Sea) | Excluded per PROJECT.md's stated v1.1 scope — this milestone retrofits the existing 9 arenas only, does not add new ones |
| In-game UI notifications (entering arena / boss defeated banners) | Excluded per PROJECT.md's stated v1.1 scope |
| A second, custom exit/teleporter mechanism competing with SubworldLibrary's built-in Return button | Would risk bypassing the load-bearing vanilla-downed-flag snapshot/restore guard tightly coupled to SubworldLibrary's own exit call order; ENTRY-02/ENTRY-03 keep the new portal as an additional convenience routed through the same `SubworldSystem.Exit()` call, not a parallel mechanism |
| Safe-wall spawn suppression as a committed v1.1 deliverable | Mechanic not yet confirmed to apply to programmatically-placed (GenPass) walls the same way as player-placed ones — moved to v2 (SPWN-01) pending live verification |
| Torch-based spawn suppression | Not a real Terraria mechanic — light level does not affect spawn rate; explicitly corrected during research (see LIGHT-02) |
| Core carrier-item pipeline changes (BossRegistry/BossCoreItem/GlobalNPC) | Already shipped and validated in v1.0; this milestone only touches arena world-gen and entry/exit UX, not the downed-flag reproduction mechanism |

## Traceability

Which phases cover which requirements. Updated during roadmap creation.

| Requirement | Phase | Status |
|-------------|-------|--------|
| BOUND-01 | Phase 13 | Pending |
| BOUND-02 | Phase 13 | Pending |
| BOUND-03 | Phase 11 | Pending |
| BOUND-04 | Phase 13 | Pending |
| TIER-01 | Phase 11 | Pending |
| TIER-02 | Phase 13 | Pending |
| TIER-03 | Phase 13 | Pending |
| LIGHT-01 | Phase 11 | Pending |
| LIGHT-02 | Phase 11 | Pending |
| DECOR-01 | Phase 14 | Pending |
| DECOR-02 | Phase 14 | Pending |
| DECOR-03 | Phase 14 | Pending |
| ENTRY-01 | Phase 12 | Pending |
| ENTRY-02 | Phase 12 | Pending |
| ENTRY-03 | Phase 12 | Pending |

**Coverage:**
- v1.1 requirements: 15 total
- Mapped to phases: 15/15 ✓
- Unmapped: 0

---
*Requirements defined: 2026-08-15*
*Last updated: 2026-08-15 — roadmap revised per user feedback (Phases 11-14, was 11-15): Entry & Exit Convenience moved to Phase 12 (was Phase 15); vanilla-biome and modded-biome boundary/tier extension phases merged into one combined Phase 13. All 15 v1.1 requirements re-mapped, 100% coverage preserved.*
</content>
