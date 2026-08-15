# Phase 13 Context: Boundary & Tier Extension to All Biome Variants

**Milestone:** v1.1 아레나 서브월드 디자인 개선
**Phase Goal:** Extend the proven arena-polish foundation layer to all 8 biome arenas (Corruption, Hallow, Underworld, Jungle, Space, Desert, Astral, Briar) in one combined phase.
**Requirements Covered:** BOUND-01, BOUND-02, BOUND-04, TIER-02, TIER-03

---

## 1. Scope & Invariants

1. **Parameter Sizing (BOUND-01)**:
   - Each biome arena's boundary and platforms are sized according to its unique `surfaceY` and thickness.
2. **Y-Window Enforcement (BOUND-02)**:
   - Space sits at `surfaceY = 70` with ceiling at `y = 10` and floor at `y = 80` (strictly `< 84`).
   - Underworld sits at `surfaceY = 670` with ceiling at `y = 605` and floor at `y = 681` (strictly `> 600`).
3. **Biome Qualification Budget (TIER-02, TIER-03)**:
   - Multi-tier platforms in Desert, Jungle, Corruption, and Hallow maintain ample tile-weight budget in the SceneMetrics scan window from all tiers.
4. **JIT Safety (BOUND-04)**:
   - `ArenaPolishPass` is appended to `BossArenaAstralSubworld` and `BossArenaBriarSubworld` using primitive constructor arguments, preserving JIT safety when CalamityMod or SpiritMod is disabled.

---

## 2. Wave Plan

- **Wave 1**: 6 Vanilla Biome Arenas (Corruption, Hallow, Underworld, Space, Jungle, Desert).
- **Wave 2**: 2 Modded Biome Arenas (Astral, Briar) + JIT Safety Validation.
