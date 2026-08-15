# Roadmap: BossArenaSubWorld

## Milestones

- ✅ **v1.0 MVP** — Phases 1-10 (shipped 2026-08-15) — see [milestones/v1.0-ROADMAP.md](milestones/v1.0-ROADMAP.md)
- 🚧 **v1.1 아레나 서브월드 디자인 개선** — Phases 11-14 (in progress)

## Phases

<details>
<summary>✅ v1.0 MVP (Phases 1-10) — SHIPPED 2026-08-15</summary>

- [x] Phase 1: Subworld Skeleton & Isolation Proof (4/4 plans) — completed 2026-08-13
- [x] Phase 2: Summon-Item Redirect & Entry Registry (3/3 plans) — completed 2026-08-13
- [x] Phase 3: BossRegistry + BossCoreItem + GlobalNPC Pipeline (POC) (3/3 plans) — completed 2026-08-13
- [x] Phase 4: Calamity Integration & Cross-Mod Side-Effect Reproduction (2/2 plans) — completed 2026-08-13
- [x] Phase 5: Spirit Integration (2/2 plans) — completed 2026-08-13
- [x] Phase 6: Redemption & CatalystMod Integration (3/3 plans) — completed 2026-08-14
- [x] Phase 7: ContinentOfJourney/Daybreak (Homeward Journey) Integration (2/2 plans) — completed 2026-08-14
- [x] Phase 8: Full Pipeline Verification & Tracker Confirmation (4/4 plans) — completed 2026-08-15
- [x] Phase 9: Biome-Dependent Subworld Coverage (7/7 plans) — completed 2026-08-14
- [x] Phase 10: Full Calamity/Spirit Boss Roster Registration & Biome Subworld Routing (6/6 plans) — completed 2026-08-15

Full phase details, success criteria, and requirements: [milestones/v1.0-ROADMAP.md](milestones/v1.0-ROADMAP.md)

</details>

### 🚧 v1.1 아레나 서브월드 디자인 개선 (In Progress)

**Milestone Goal:** 기존 9개 아레나 서브월드(플레인 아레나 + Corruption/Hallow/Underworld/Jungle/Space/Desert/Astral/Briar)의 안전성, 공간 설계, 광원, 바이옴 테마 장식, 진입/퇴장 편의성을 개선한다. 핵심 carrier-item 파이프라인(BossRegistry/BossCoreItem/GlobalNPC)은 이미 v1.0에서 완료되어 이번 마일스톤 범위 밖이다.

**Revised 2026-08-15** (user feedback, pre-execution): compressed from 5 phases to 4 — Entry & Exit Convenience moved up to immediately follow the shared foundation phase (was last), and the vanilla-biome and modded-biome boundary/tier extension phases merged into one combined phase covering all 8 biome variants together.

- [x] **Phase 11: Shared Arena-Polish Foundation (Plain Arena)** (1/1 plans) — completed 2026-08-15
- [x] **Phase 12: Entry & Exit Convenience** (1/1 plans) — completed 2026-08-15
- [x] **Phase 13: Boundary & Tier Extension to All Biome Variants** (1/1 plans) — completed 2026-08-15
- [x] **Phase 14: Per-Biome Decorative Theming** (1/1 plans) — completed 2026-08-15

## Phase Details

### Phase 11: Shared Arena-Polish Foundation (Plain Arena)
**Goal**: A reusable, mod-agnostic arena-polish layer (boundary containment, torch lighting, multi-tier platforms) is built and proven on the plain arena, establishing the pattern the remaining 8 arenas will inherit mechanically.
**Depends on**: Nothing (first phase of v1.1)
**Requirements**: BOUND-03, LIGHT-01, LIGHT-02, TIER-01
**Success Criteria** (what must be TRUE):
  1. Entering the plain arena, the player cannot fall or be knocked off the platform into open void — a solid boundary blocks the fall in every direction.
  2. The plain arena has torches placed at regular intervals along its platform tiers, visibly lit without any additional placed light source.
  3. The plain arena has a 2-4 tier platform structure spaced at vanilla jump height, all tiers reachable by normal jumping (no gap requiring double-jump or wall-clip).
  4. Monster spawn rate in the plain arena is visibly unaffected by the new torches — confirms LIGHT-02's visual-only scoping holds in practice, not just in code comments.
**Plans**: 1 plan
  - [x] 11-01-PLAN.md: Build BoundaryTile, ArenaBuilder static helper, ArenaPolishPass GenPass, and wire into BossArenaSubworld (plain arena) — completed 2026-08-15

### Phase 12: Entry & Exit Convenience
**Goal**: Players control their own prep timing before a boss fight starts, and have a clear, safe way back to the main world that doesn't compromise the existing vanilla-downed-flag snapshot/restore guard.
**Depends on**: Nothing (systems-layer work — touches `Systems/`/`Tiles/` runtime tick and interaction logic, not `Subworlds/` world-gen; per research/SUMMARY.md's Architecture and Phase Ordering Rationale sections, this work has no dependency on the shared arena-polish layer or any biome-extension phase. Re-verified during this revision: `ReturnPortalTile` mirrors `Test1Tile`'s existing interaction pattern and calls `SubworldSystem.Exit()` directly; the prep-time countdown lives in `BossSummonPlayer`/`ForcedTimeSystem`. Neither touches `ArenaBuilder`/`ArenaPolishPass` or any biome `*PlatformPass`. Sequenced immediately after Phase 11 per this revision, purely for delivery ordering — not a technical dependency.)
**Requirements**: ENTRY-01, ENTRY-02, ENTRY-03
**Success Criteria** (what must be TRUE):
  1. Entering any arena subworld no longer auto-summons the boss immediately — the player must use their held boss-summon item themselves whenever ready.
  2. Each arena has a visible return-point portal tile near spawn; using it exits the subworld back to the main world via `SubworldSystem.Exit()`, with the existing vanilla-downed-flag snapshot/restore guard still firing correctly (no flag regression on return).
  3. SubworldLibrary's built-in Return button still works exactly as before — the new portal is an additional convenience, not a replacement or a second exit mechanism.
**Plans**: 1 plan
  - [x] 12-01-PLAN.md: Entry convenience (player prep timing) & ReturnPortalTile placement (Wave 1: Entry, Wave 2: Exit) — completed 2026-08-15

### Phase 13: Boundary & Tier Extension to All Biome Variants
**Goal**: The proven shared arena-polish layer is extended to all 8 biome arenas (Corruption, Hallow, Underworld, Jungle, Space, Desert, Astral, Briar) in one combined pass, with boundary placement correctly parameterized per arena's own surfaceY/Y-window, multi-tier platforms verified not to break any biome's Zone/Biome-flag qualification, and Astral/Briar's JIT safety confirmed with each source mod disabled — closing out full 9-arena boundary/tier coverage.
**Depends on**: Phase 11
**Requirements**: BOUND-01, BOUND-02, BOUND-04, TIER-02, TIER-03
**Success Criteria** (what must be TRUE):
  1. Each of the 8 biome arenas has a fall/void boundary sized to that arena's own platform position, not a single shared literal — visibly correct across all 8, including Astral and Briar.
  2. In Space and Underworld, the boundary sits strictly inside the Y-window their Zone flag requires (y <= 84 for Space, y > 600 for Underworld) — confirmed live by standing at the boundary edge without the boss despawning.
  3. Climbing to the highest platform tier in Desert and Jungle (tightest vanilla tile-weight margins) does not flip ZoneDesert/ZoneJungle false mid-fight — confirmed live by checking the Zone flag from the top tier.
  4. Each of the 8 biome arenas' pre-existing Zone/Biome-flag qualification (including Desert's 1500-weight threshold and Astral/Briar's modded `IsBiomeActive` checks) is unchanged after adding tiers — the flag still reads true on entry exactly as it did pre-retrofit.
  5. Loading the mod with CalamityMod disabled succeeds without a JITException (Astral), and loading with SpiritMod disabled succeeds without a JITException (Briar) — confirming the shared `ArenaBuilder`/`ArenaPolishPass` public API stayed strictly primitive-typed and introduced no new JIT-unsafe surface reachable from either arena's untagged call path.
**Plans**: 1 plan
  - [x] 13-01-PLAN.md: Boundary and multi-tier platform extension to all 8 biome arenas (Wave 1: Vanilla biomes, Wave 2: Modded biomes) — completed 2026-08-15

### Phase 14: Per-Biome Decorative Theming
**Goal**: All 9 arenas read as visually distinct, biome-legible spaces rather than minimal Zone-flag-satisfying platforms, without regressing any biome's tile-weight budget or JIT safety.
**Depends on**: Phase 13
**Requirements**: DECOR-01, DECOR-02, DECOR-03
**Success Criteria** (what must be TRUE):
  1. Each of the 9 arenas is visually identifiable as its intended biome at a glance (Corruption chasms/purple grass, Hallow crystal tones, Underworld ash/obsidian/lava glow, Jungle mud/vines, Desert dunes/sandstone, Astral/Briar mod-native palettes, plain arena neutral theme).
  2. After decoration, every biome-gated arena's Zone flag still reads true on entry — decoration tiles are additive to/drawn from the existing qualifying tile set, not a replacement.
  3. Astral and Briar's new decoration code lives entirely inside their existing JIT-tagged `ApplyPass()` methods — confirmed by code inspection plus the Phase 13 disable-mod smoke test still passing after decoration is added.
**Plans**: 1 plan
  - [x] 14-01-PLAN.md: Decorative theming for all 9 arenas (Wave 1: Plain & Vanilla biomes, Wave 2: Modded biomes) — completed 2026-08-15

## Progress

**Execution Order:**
Phases execute in numeric order: 11 → 12 → 13 → 14

| Phase | Milestone | Plans Complete | Status | Completed |
|-------|-----------|-----------------|--------|-----------|
| 1. Subworld Skeleton & Isolation Proof | v1.0 | 4/4 | Complete | 2026-08-13 |
| 2. Summon-Item Redirect & Entry Registry | v1.0 | 3/3 | Complete | 2026-08-13 |
| 3. BossRegistry + BossCoreItem + GlobalNPC Pipeline (POC) | v1.0 | 3/3 | Complete | 2026-08-13 |
| 4. Calamity Integration & Cross-Mod Side-Effect Reproduction | v1.0 | 2/2 | Complete | 2026-08-13 |
| 5. Spirit Integration | v1.0 | 2/2 | Complete | 2026-08-13 |
| 6. Redemption & CatalystMod Integration | v1.0 | 3/3 | Complete | 2026-08-14 |
| 7. ContinentOfJourney/Daybreak (Homeward Journey) Integration | v1.0 | 2/2 | Complete | 2026-08-14 |
| 8. Full Pipeline Verification & Tracker Confirmation | v1.0 | 4/4 | Complete | 2026-08-15 |
| 9. Biome-Dependent Subworld Coverage | v1.0 | 7/7 | Complete | 2026-08-14 |
| 10. Full Calamity/Spirit Boss Roster Registration & Biome Subworld Routing | v1.0 | 6/6 | Complete | 2026-08-15 |
| 11. Shared Arena-Polish Foundation (Plain Arena) | v1.1 | 1/1 | Complete | 2026-08-15 |
| 12. Entry & Exit Convenience | v1.1 | 1/1 | Complete | 2026-08-15 |
| 13. Boundary & Tier Extension to All Biome Variants | v1.1 | 1/1 | Complete | 2026-08-15 |
| 14. Per-Biome Decorative Theming | v1.1 | 1/1 | Complete | 2026-08-15 |
</content>
