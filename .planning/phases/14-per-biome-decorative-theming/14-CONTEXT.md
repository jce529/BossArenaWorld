# Phase 14 Context: Per-Biome Decorative Theming

**Milestone:** v1.1 아레나 서브월드 디자인 개선
**Phase Goal:** Enrich all 9 arena subworlds with immersive, biome-legible decorative theming, backgrounds, and accents while preserving Zone-flag tile budgets and JIT safety.
**Requirements Covered:** DECOR-01, DECOR-02, DECOR-03

---

## 1. Scope & Invariants

1. **Aesthetic & Legibility (DECOR-01)**:
   - Each of the 9 arenas feels distinct, polished, and biome-appropriate (background walls, campfires, themed block accents).
2. **Additive Tile Budget (DECOR-02)**:
   - Decorative additions do not reduce the volume of Zone-qualifying tiles below required thresholds.
3. **Strict JIT Isolation (DECOR-03)**:
   - Calamity/Spirit modded decorative assets reside exclusively inside `[JITWhenModsEnabled]` methods in `AstralPlatformPass.cs` and `BriarPlatformPass.cs`.

---

## 2. Wave Structure

- **Wave 1**: Plain Arena + 6 Vanilla Biome Arenas (Corruption, Hallow, Underworld, Space, Jungle, Desert).
- **Wave 2**: 2 Modded Biome Arenas (Astral, Briar) + JIT Verification + Milestone Wrap-up.
