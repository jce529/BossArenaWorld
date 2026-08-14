# Phase 10: Full Calamity/Spirit Boss Roster Registration & Biome Subworld Routing - Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions are captured in CONTEXT.md — this log preserves the alternatives considered.

**Date:** 2026-08-14
**Phase:** 10-full-calamity-spirit-boss-roster-registration-and-biome-subworld-routing
**Areas discussed:** Biome assignment principle, Infernum conditional gating, roster scope, time-gated bosses

---

## Biome assignment principle

| Option | Description | Selected |
|--------|-------------|----------|
| 유지 | Phase 9에서 이미 명시적으로 결정된 사항 — 위키가 말하는 테마 아레나로 전부 배정, AI 필요 여부와 무관 | ✓ |
| AI 필요 시에만 배정 | 실제 기능(디스폰 방지)에 영향 주는 보스만 바이옴 라우팅, 나머지는 기본 아레나 — 작업량 대폭 감소하지만 Phase 9 결정 번복 | |

**User's choice:** 유지 (Recommended)
**Notes:** Claude이 이 프로젝트의 SummonItemRegistry 파이프라인이 원본 아이템의 CanUseItem()/UseItem()을 건너뛰기 때문에 위키 기준 요구사항 대부분이 기능적으로 무관하다는 점을 짚었으나, 사용자는 Phase 9에서 이미 내린 결정(테마 일치가 목적, AI 필요성이 아님)을 재확인하고 유지하기로 함.

---

## Infernum 조건부 등록 로직

| Option | Description | Selected |
|--------|-------------|----------|
| 전부 구현 | 09-ALTAR-BIOME-REFERENCE.md Section 1의 조건부 게이팅 테이블(Providence/Profaned Guardians/Ceaseless Void는 Infernum 시 제외, The Old Duke는 Infernum 시에만 등록, Astrum Deus/Aureus는 Infernum 시 강제 밤) 전체 구현 | ✓ |
| Calamity 단독 구성만 | Infernum 조건부 로직은 후속 페이즈로 미루고 Calamity 단독 기준으로만 등록 | |

**User's choice:** 전부 구현 (Recommended)
**Notes:** 복잡도가 높지만 정확도를 위해 전체 구현하기로 함.

---

## 로스터 범위

| Option | Description | Selected |
|--------|-------------|----------|
| 전부 한 번에 | 09-ALTAR-BIOME-REFERENCE.md의 전체 로스터(Calamity 11개 + Spirit 7개, Hive Mind/Infernon 제외)를 이번 한 페이즈에서 전부 등록, 계획 단계에서 여러 Plan/Wave로 분할 | ✓ |
| 일부만 우선 | 리스크 높거나 복잡한 보스(Infernum 조건부, 문 로드류 락아웃 등) 제외하고 단순한 보스만 먼저 | |

**User's choice:** 전부 한 번에 (Recommended)
**Notes:** PROJECT.md의 "보스 우선순위 없음" 원칙과 일치.

---

## 낮/밤 게이팅 보스

| Option | Description | Selected |
|--------|-------------|----------|
| 제외 | 강제 낮/밤 유틸리티 미구현 상태 유지, Moon Jelly Wizard/Dusking은 계속 이월 | |
| 포함 — 유틸리티도 같이 구현 | 이번 페이즈에서 강제 낮/밤 유틸리티를 새로 구현하고 Moon Jelly Wizard/Dusking(Spirit)까지 포함 | ✓ |

**User's choice:** 포함 — 유틸리티도 같이 구현
**Notes:** Phase 9 D-05에서 범위 제외됐던 강제 낮/밤 메커니즘을 이번 페이즈에서 신규 구현하기로 함. Astrum Deus/Aureus의 Infernum 조건부 강제 밤 요구사항과 동일 메커니즘 재사용. Redemption의 낮/밤 게이팅 보스(Fowl Emperor, King Slayer III 등)는 모드 범위 밖이라 제외.

---

## Claude's Discretion

- 보스별 `OnKill()` 디컴파일 검증(부작용, player/world-scope 분류, 실제 Zone 의존성) — 리서치 단계에서 수행, 매 모드 통합 페이즈의 기존 원칙 그대로
- The Old Duke의 Sulphurous Sea 요구사항이 아이템 게이트인지 AI 게이트인지 (Open Item 3) — 디컴파일 확인 필요
- 강제 낮/밤 유틸리티의 정확한 구현 방식 (서브월드 OnEnter 시점 설정 등)
- Providence/Profaned Guardians의 Hallow vs Underworld 중 어느 쪽을 기본 배정할지
- 통합 파일을 기존 CalamityIntegration.cs/SpiritIntegration.cs에 계속 추가할지, 파일을 분리할지

## Deferred Ideas

- Redemption 전체 로스터 확장 (Thorn 외) — 대부분 구조 고정형이라 이미 제외 확인됨, 이번 페이즈 모드 범위 밖
- CatalystMod 전체 로스터 확장 (Astrageldon 외) — 미리서치, 범위 밖
- NoxusBoss / ContinentOfJourney / Daybreak — Phase 7 몫
- 다른 Calamity 리워크 모드(Fargo's Mod 등)의 구조 게이팅 영향 — 미조사, 향후 마일스톤 후보
- Dungeon / Sulphurous Sea 서브월드 재구축 여부 — Open Item 3 해결에 따라 결정
