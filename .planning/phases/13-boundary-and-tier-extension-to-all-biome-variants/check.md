# Phase 13 Verification & Quality Assurance (check.md)

**Phase:** 13-boundary-and-tier-extension-to-all-biome-variants
**Goal:** Extend boundary containment, multi-tier platforms, and torch lighting across all 8 biome arenas with strict Y-window alignment and JIT safety.
**Requirements:** BOUND-01, BOUND-02, BOUND-04, TIER-02, TIER-03
**Status:** Code-Level Verified, Ready for Playtest

---

## 1. 코드 레벨 검증 (Code-Level Verification)

### 1.1 전체 9개 아레나 서브월드 파라미터 매트릭스 검증

| 서브월드 클래스 | 바이옴 판정 유형 | `surfaceY` | 플랫폼 두께 | 플랫폼 구조 | 조명 스타일 | 천장 마진 | 바이옴 유지 안전성 |
| :--- | :--- | :---: | :---: | :---: | :---: | :---: | :---: |
| `BossArenaSubworld` | 플레인 (중립) | 400 | 15 | 3단 (28 간격) | 0 (기본) | 120 | ✅ PASS |
| `BossArenaCorruptionSubworld` | `ZoneCorrupt` (≥300) | 400 | 15 | 3단 (28 간격) | 4 (오염/퍼플) | 120 | ✅ PASS (~3000 타일) |
| `BossArenaHallowSubworld` | `ZoneHallow` (≥125) | 400 | 15 | 3단 (28 간격) | 20 (신성/핑크) | 120 | ✅ PASS (~3000 타일) |
| `BossArenaUnderworldSubworld` | `ZoneUnderworldHeight` (`>600`) | 670 | 10 | 3단 (28 간격) | 7 (데몬) | 65 | ✅ PASS (`Y in [605, 687]`) |
| `BossArenaSpaceSubworld` | `ZoneSkyHeight` (`<=84`) | 70 | 10 | 3단 (18 간격) | 5 (화이트) | 60 | ✅ PASS (`Y in [10, 80]`) |
| `BossArenaJungleSubworld` | `ZoneJungle` (≥140) | 400 | 15 | 3단 (28 간격) | 21 (정글) | 120 | ✅ PASS (~3000 타일) |
| `BossArenaDesertSubworld` | `ZoneDesert` (≥1500) | 400 | 20 | 3단 (28 간격) | 16 (사막) | 120 | ✅ PASS (~4000 타일) |
| `BossArenaAstralSubworld` | `AstralInfection` (>950) | 400 | 15 | 3단 (28 간격) | 4 (퍼플) | 120 | ✅ PASS (~3000 타일) |
| `BossArenaBriarSubworld` | `BriarSurface` (>80, `<=240`) | 150 | 15 | 3단 (28 간격) | 3 (그린) | 80 | ✅ PASS (`Y in [70, 165]`) |

### 1.2 요구사항 충족 및 인베리언트 검증

| 요구사항 ID | 검증 내용 | 검증 결과 |
| :--- | :--- | :---: |
| **`BOUND-01`** | 8개 전체 바이옴 서브월드에 각각의 `surfaceY`에 맞춘 투명 낙하방지 배리어 생성 | ✅ PASS |
| **`BOUND-02`** | Space(`Y in [10, 80] <= 84`) 및 Underworld(`Y in [605, 687] > 600`)의 엄격한 Y-Window 밀폐 | ✅ PASS |
| **`BOUND-04`** | `ArenaBuilder`/`ArenaPolishPass` 내 외부 모드 타입 참조 0개 (순수 원시 타입 API 유지) | ✅ PASS |
| **`TIER-02`** | 다단 플랫폼 추가 후에도 사막(1500), 정글(140) 등 타일 가중치 기준치 초과 유지 | ✅ PASS |
| **`TIER-03`** | 최상단 3단 플랫폼으로 올라가도 수직 거리 56블록으로 `SceneMetrics` 스캔(반경 70) 내 유지 | ✅ PASS |
| **빌드 검증** | `dotnet build BossArenaSubWorld.csproj /warnaserror` (0 오류, 0 경고) | ✅ PASS |

---

## 2. 실제 플레이테스트 검증 체크리스트 (In-Game Playtest Checklist)

### 🌌 특수 Y축 바이옴 아레나
- [ ] **1. Space 아레나 (`ZoneSkyHeight`)**:
  - 진입 시 배경 및 중력이 우주(Sky) 상태로 시작되는가?
  - 3단 플랫폼(`y=34`) 위로 올라가거나 점프/비행하여 천장(`y=10`)에 닿아도 우주 보스가 디스폰되지 않는가?
  - 바닥 플랫폼 아래로 떨어져도 낙하 방지 바닥(`y=80`)에 막혀 공허로 떨어지지 않는가?
- [ ] **2. Underworld 아레나 (`ZoneUnderworldHeight`)**:
  - 진입 시 지옥 배경음 및 용암 배경이 정상 표시되는가?
  - 최상단 3단 플랫폼(`y=614`)에 서서 점프해도 천장 배리어(`y=605`)에 막혀 `Y > 600` 영역 밖으로 나가지 않는가?
  - 월오플 등 지옥 전용 보스가 최상단 플랫폼에서도 격노/디스폰 없이 정상 전투가 유지되는가?

### 🌲 지표면 타일 바이옴 아레나
- [ ] **3. Desert 아레나 (`ZoneDesert`)**:
  - 3단 플랫폼에 올라서도 사막 배경 및 사막 보스(Desert Scourge 등)가 디스폰되지 않고 전투가 유지되는가?
- [ ] **4. Jungle 아레나 (`ZoneJungle`)**:
  - 3단 플랫폼 위에서도 플랜테라/퀸비가 폭주(Enrage)하거나 디스폰되지 않는가?
- [ ] **5. Corruption / Hallow 아레나**:
  - 오염 토치(퍼플) 및 신성 토치(핑크)가 30블록마다 정상 점등되어 있는가?
  - 하이브 마인드 / 퀸 슬라임 등 바이옴 전용 보스가 3단 플랫폼 위에서도 안정적으로 전투되는가?

### 🔮 모드 바이옴 아레나
- [ ] **6. Astral / Briar 아레나 (Calamity / Spirit)**:
  - 아스트랄 / 브라이어 아레나 진입 시 플랫폼 및 토치, 퇴장 포털이 정상 배치되는가?
  - Calamity나 Spirit 모드를 끈 상태에서 모드 로드 시 `JITException` 충돌 없이 정상 구동되는가?

---

## 3. 예상되는 버그 및 취약점 분석 (Expected Bugs & Vulnerabilities)

1. **Space 아레나 비행 날개 천장 충돌**:
   * *동작 원리*: 우주에서는 중력이 낮아 체공 시간이 길어지며, 고성능 날개 착용 시 천장 배리어(`y=10`)에 닿을 수 있습니다. 이는 플레이어가 맵 밖으로 튕겨나가는 것을 막아주는 정상적인 배리어 판정입니다.
2. **Underworld 아레나 대형 보스(Wall of Flesh)의 수직 이동 범위**:
   * *동작 원리*: 월오플의 눈/입 촉수는 수직으로 넓게 퍼지지만, 플랫폼과 천장/바닥 배리어가 `y in [605, 687]`로 감싸고 있어 보스의 메인 코어가 지옥 기준선(`y > 600`)을 절대 이탈하지 않습니다.
3. **모드 비활성화 시 미참조 안전성**:
   * *동작 원리*: `BossArenaAstralSubworld` 및 `BossArenaBriarSubworld`는 Calamity/Spirit 모드가 비활성화되어 있어도 `BossArenaRoutingRegistry`에서 해당 보스 라우팅이 등록되지 않으므로 진입 시도가 차단되며, `ArenaPolishPass` 역시 원시 타입만 사용하여 안전합니다.
