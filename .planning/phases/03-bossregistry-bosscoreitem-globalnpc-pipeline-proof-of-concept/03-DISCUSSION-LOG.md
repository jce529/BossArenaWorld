# Phase 3: BossRegistry + BossCoreItem + GlobalNPC Pipeline (POC) - Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions are captured in CONTEXT.md — this log preserves the alternatives considered.

**Date:** 2026-08-13
**Phase:** 03-bossregistry-bosscoreitem-globalnpc-pipeline-proof-of-concept
**Areas discussed:** 멱등성 처리 방식 (Idempotency), BossCoreItem 소모 정책 (Item consumption), 보스 키 설계 (Registry key design), King Slime Apply()의 충실도 (Flag fidelity)

---

## 멱등성(Idempotency) 처리 방식 — APPLY-04

| Option | Description | Selected |
|--------|-------------|----------|
| 적용 전 현재 플래그 확인 | Per-boss "IsDowned" getter checked before Apply(); no separate world-data tracking needed; generalizes to all future mods | ✓ |
| BossRegistry의 별도 적용-추적 세트 | HashSet<string> of applied keys stored in world data via SaveWorldData/LoadWorldData | |
| 둘 다 (이중 안전장치) | Both flag-check and tracking set | |

**User's choice:** 적용 전 현재 플래그 확인 (recommended option)
**Notes:** No further discussion requested — recommended option accepted directly.

---

## BossCoreItem 소모 정책

| Option | Description | Selected |
|--------|-------------|----------|
| 성공 시만 소모 | Item consumed only on Apply() success; retained + error chat message on failure, enabling retry | ✓ |
| 항상 소모 | Item consumed on use regardless of outcome | |

**User's choice:** 성공 시만 소모 (recommended option)
**Notes:** Matches PITFALLS.md UX guidance against silent no-op failures.

---

## 보스 키(Registry key) 설계

| Option | Description | Selected |
|--------|-------------|----------|
| 문자열 키, 모드 접두사 | e.g. "vanilla:king_slime", "calamity:desert_scourge"; decouples key from raw NPC.type, supports multi-phase bosses | ✓ |
| NPC.type 정수 그대로 사용 | Simpler but doesn't generalize to multi-phase bosses or cross-mod type collisions | |

**User's choice:** 문자열 키, 모드 접두사 (recommended option)
**Notes:** Matches research/ARCHITECTURE.md Pattern 2's sketched BossRegistry/BossDefinition shape.

---

## King Slime Apply()의 충실도

| Option | Description | Selected |
|--------|-------------|----------|
| NPC.SetEventFlagCleared 방식 | Replays vanilla's actual on-kill helper — flag + achievement notification + multiplayer sync (no-op in singleplayer) | ✓ |
| 단순 boolean 대입 | NPC.downedSlimeKing = true only; simpler but under-reproduces per Pitfall 4 | |

**User's choice:** NPC.SetEventFlagCleared 방식 (recommended option)
**Notes:** Establishes the fidelity bar (avoid Pitfall 4) from the first boss onward, before Phase 4's mod-specific integrations begin.

---

## Claude's Discretion

- BossCoreItem itemization (sprite/display name/rarity) — minimal placeholder per Test1Item precedent, no debug give-command needed (obtained via kill drop only)
- Exact shape of the per-boss "already downed" getter on BossDefinition (Func<bool> field vs. method)
- Exact chat message wording for success/failure feedback
- File/class naming within Systems/, GlobalNPCs/, Items/ structure

## Deferred Ideas

None — discussion stayed within phase scope. APPLY-02/APPLY-03 (mod-specific netcode/WorldGen side-effect reproduction) was referenced only as a boundary clarification for the King Slime fidelity discussion, not pulled into this phase's scope — remains Phase 4's responsibility.
