# Phase 11 Verification & Quality Assurance (check.md)

**Phase:** 11-shared-arena-polish-foundation-plain-arena
**Goal:** Mod-agnostic boundary containment, torch lighting, and multi-tier platform foundation on plain arena.
**Requirements:** BOUND-03, LIGHT-01, LIGHT-02, TIER-01
**Status:** Code-Level Verified, Ready for Playtest

---

## 1. 코드 레벨 검증 (Code-Level Verification)

| 검증 항목 | 검증 대상 파일 / 위치 | 검증 기준 | 결과 |
| :--- | :--- | :--- | :---: |
| **투명 솔리드 배리어** | `Tiles/BoundaryTile.cs` | `tileSolid=true`, `tileBlockLight=false`, `PreDraw=>false` | ✅ PASS |
| **JIT 안전 원시 타입 API** | `Subworlds/ArenaBuilder.cs` | 외부 모드 타입 참조 0개, 순수 `int`/`ushort` 시그니처 | ✅ PASS |
| **4방향 경계벽 수학적 검증** | `Subworlds/ArenaBuilder.cs` | 하단 낙하 방지 바닥, 상단 천장(120블록 상공), 좌/우 밀폐 | ✅ PASS |
| **다단 플랫폼 구조** | `Subworlds/ArenaBuilder.cs` | 28블록(바닐라 점프 높이) 간격의 3단 플랫폼(돌 바닥 + 2단 나무) | ✅ PASS |
| **토치 조명 배치** | `Subworlds/ArenaBuilder.cs` | `WorldGen.PlaceTile` 기반 30블록 간격 프레이밍 배치 | ✅ PASS |
| **서브월드 Tasks 연동** | `Subworlds/BossArenaSubworld.cs` | `FlatStonePlatformPass` 후속으로 `ArenaPolishPass` 등록 | ✅ PASS |
| **컴파일 및 패키징** | `dotnet build BossArenaSubWorld.csproj` | 0 오류, 0 경고 빌드 완료 | ✅ PASS |

---

## 2. 실제 플레이테스트 검증 체크리스트 (In-Game Playtest Checklist)

- [ ] **1. 스폰 정상 여부**: 플레인 아레나 입장 시 플레이어가 공중에 뜨거나 플랫폼에 끼이지 않고 플랫폼 위(`surfaceY - 3`)에 안전하게 스폰되는가?
- [ ] **2. 다단 플랫폼 점프 이동성 (`TIER-01`)**:
  - 기본 점프(더블 점프 악세서리 없는 순수 바닐라 점프)로 1단 바닥 ↔ 2단 플랫폼 ↔ 3단 플랫폼 사이를 부드럽게 오르내릴 수 있는가?
  - 플랫폼 아래로 내려가기(하단 키 + 점프)가 정상 작동하는가?
- [ ] **3. 조명 시각적 확인 (`LIGHT-01`, `LIGHT-02`)**:
  - 돌 바닥 및 각 플랫폼 티어를 따라 약 30블록 간격으로 토치가 정상 배치되어 있는가?
  - 토치 그래픽 프레임 깨짐이나 공중 부유 현상이 없는가?
- [ ] **4. 공허 낙하 방지 경계벽 작동 (`BOUND-03`)**:
  - 플랫폼 끝 좌우로 이동 시 투명 벽에 막혀 아레나 밖으로 이탈하지 않는가?
  - 플랫폼 아래로 떨어졌을 때 바닥에 깔린 투명 배리어에 의해 공허로 끝없이 추락하지 않고 발이 딛어지는가?
  - 상공으로 높이 비행 시 상단 천장 배리어에 의해 적정 고도에서 containment가 유지되는가?

---

## 3. 예상되는 버그 및 취약점 분석 (Expected Bugs & Vulnerabilities)

1. **토치 스폰 억제 오해 (`LIGHT-02`)**:
   * *위험요소*: 테라리아 엔진 특성상 토치 조명은 몬스터 스폰율을 낮추지 않습니다.
   * *대응책*: 코드 및 문서에 visual-only로 명시했으며, 추후 필요 시 안전 벽(Safe Wall) 또는 `EditSpawnRate` 훅을 통해 처리.
2. **보스 비행 궤적 및 천장 충돌**:
   * *위험요소*: 상단 천장이 120블록 상공에 위치하므로, 비행형 보스(문로드, 듀크 피시론 등) 전투 시 플레이어가 천장에 닿을 수 있음.
   * *대응책*: 현재 120블록은 일반 점프 및 저/중티어 날개 비행에 충분한 공간이며, 향후 대형 보스 테스트 결과에 따라 마진 조절 가능.
3. **낙하형 타일(Sand/Silt)과의 상호작용**:
   * *위험요소*: Phase 13/14에서 Desert/Astral 등 모래 계열 타일 적용 시 지지 기반이 없으면 스택 오버플로 발생 위험.
   * *대응책*: `ArenaBuilder`에 솔리드 타일 선행 배치 규칙 준수.
