# Phase 9: Biome-Dependent Subworld Coverage - Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions are captured in CONTEXT.md — this log preserves the alternatives considered.

**Date:** 2026-08-13
**Phase:** 09-biome-dependent-subworld-coverage
**Areas discussed:** Phase sequencing, Portal/entry architecture, Mod/boss scope, Infernum gating ownership, Day/night forcing scope, Build scope (variant count)

---

## Phase Sequencing

| Option | Description | Selected |
|--------|-------------|----------|
| 선제 진행 (Proceed now) | This session's research already fully identifies which Calamity/Spirit bosses need biome coverage; build the Subworld variants now and connect them via `BossArenaRoutingRegistry.Register<T>()` when Phase 6/7 later registers those bosses | ✓ |
| Phase 6/7 사전자료로만 남기고 대기 | Keep this session's research as prep material only; defer actual Phase 9 planning/execution until Phase 8 completes, per ROADMAP's stated dependency ordering | |
| 로드맵 재구성 | Fold "register boss + build biome arena" together into Phase 6/7 directly; retire Phase 9 as a separate later phase | |

**User's choice:** 선제 진행 (Proceed now)
**Notes:** Raised because ROADMAP.md literally scopes Phase 9 to auditing "every boss registered in Phases 4-7," but Phases 6-8 haven't started and only 2 non-vanilla bosses are currently registered (Hive Mind — already covered; Infernon — confirmed biome-independent) — under a strict reading, Phase 9 would have zero work today. User chose to proceed anyway, building ahead of registration.

---

## Portal / Entry Architecture

| Option | Description | Selected |
|--------|-------------|----------|
| 기존 단일 포탈 유지 | Keep `Test1Tile` as the only entry point; routing to the correct biome subworld stays fully automatic via `BossArenaRoutingRegistry`, invisible to the player. No new player-facing items. | ✓ |
| 새 멀티 제단 시스템 도입 | Implement the "recolored altar item per biome" concept discussed earlier in this conversation as actual placeable items the player interacts with directly | |

**User's choice:** 기존 단일 포탈 유지 (Keep existing single portal)
**Notes:** This directly reverses the "제단 아이템을 구성해줘" (compose altar items) direction from earlier in this same conversation, once the already-validated Phase 2 architecture (single `Test1Tile`, auto-routing, no player-facing altar choice) was surfaced. The `09-ALTAR-BIOME-REFERENCE.md` altar naming table survives only as internal class-name reference, not as an actual item system.

---

## Mod / Boss Scope

| Option | Description | Selected |
|--------|-------------|----------|
| Calamity + Spirit만 | Cover only the two mods with already-registered/verified bosses | |
| Calamity + Spirit + CatalystMod | Add Astrageldon (Astral Infection) — low marginal cost since the Astral subworld is already needed for Astrum Deus/Aureus | ✓ |

**User's choice:** Calamity + Spirit + CatalystMod
**Notes:** Redemption and NoxusBoss/Wrath of the Gods were not offered as options — both were already confirmed fully excluded in prior conversation turns (research + explicit user decision), so they weren't re-litigated here.

---

## Infernum Gating Ownership

| Option | Description | Selected |
|--------|-------------|----------|
| Phase 6으로 전부 위임 | Phase 9 only builds the Subworld/GenPass infrastructure; the `ModLoader.HasMod("InfernumMode")`-conditional registration logic (Providence/Guardians/Ceaseless Void unassignable, Old Duke assignable, when Infernum loads) is implemented later in Phase 6 alongside actual Calamity boss registration | ✓ |
| Phase 9에서 헬퍼/패턴까지 준비 | Build the InfernumMode-detection helper/pattern now, even though no boss registration exists yet to use it | |

**User's choice:** Phase 6으로 전부 위임 (Delegate entirely to Phase 6)
**Notes:** Keeps Phase 9 scoped strictly to subworld/GenPass construction, not registration logic.

---

## Day/Night Forcing Scope

| Option | Description | Selected |
|--------|-------------|----------|
| Phase 9 범위에서 제외 | ARENA-01 covers biome/Zone-flag dependence only; day/night is a different concern. Note as a candidate future requirement instead of building now. | ✓ |
| Astral 서브월드에 함께 구현 | Add a "force world time to night" GenPass step to the Astral Infection subworld now, covering Astrum Deus/Aureus's Infernum-specific night requirement | |

**User's choice:** Phase 9 범위에서 제외 (Exclude from Phase 9)
**Notes:** The Astral Infection subworld built in this phase will NOT force night; that gap is inherited by whichever future phase picks up Astrum Deus/Aureus's Infernum delta.

---

## Build Scope (Variant Count)

| Option | Description | Selected |
|--------|-------------|----------|
| 9개 전부 한 번에 | Build all 9 identified biome variants (Hallow, Underworld, Astral, Jungle, Space, Dungeon, Desert, Briar, Sulphurous Sea) in this phase, matching the project's established "uniform marginal cost, no priority ordering" principle | ✓ |
| 우선순위 나눠서 일부만 | Build only the currently-certain-needed subset first, defer the rest | |

**User's choice:** 9개 전부 한 번에 (All 9 at once)
**Notes:** Sulphurous Sea is included despite having zero assignable boss without Infernum (Old Duke's assignability is conditional and deferred to Phase 6) — built now for the same uniform-cost reasoning.

---

## Claude's Discretion

- Exact vanilla/Calamity tile IDs and fill thickness satisfying each target biome's Zone-flag detection — follow the `CorruptionPlatformPass` decompilation-tracing methodology per biome, not memory.
- Exact class/file naming for the 9 new Subworld/GenPass pairs.
- Whether each new Subworld duplicates `BossArenaCorruptionSubworld`'s vanilla-downed-flag guard verbatim (expected yes, confirm during planning).
- Astral Infection tile/structure sourcing (a modded Calamity biome, not a vanilla one — needs Calamity-specific research, not just vanilla `TileID.Sets` lookup).

## Deferred Ideas

- Mod of Redemption bosses, NoxusBoss/Wrath of the Gods bosses — permanently excluded (not revisited in this session, already closed).
- Infernum-conditional registration logic — deferred to Phase 6.
- Forced day/night utility mechanism — deferred, not a tracked requirement yet.
- ContinentOfJourney / Daybreak mod identification — unresolved research gap, needs user-supplied Workshop ID/author name.
- Bloodworm Platter's Sulphurous Sea requirement (item gate vs. AI gate) — needs a decompiled-source check before Phase 6 finalizes The Old Duke's registration.
- Multi-altar player-facing UX — rejected in favor of the existing single-portal architecture.
