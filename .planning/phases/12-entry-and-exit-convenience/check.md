# Phase 12 Verification & Quality Assurance (check.md)

**Phase:** 12-entry-and-exit-convenience
**Goal:** Player-controlled boss summon prep timing and safe return portal routing via `SubworldSystem.Exit()`.
**Requirements:** ENTRY-01, ENTRY-02, ENTRY-03
**Status:** Code-Level Verified, Ready for Playtest

---

## 1. 코드 레벨 검증 (Code-Level Verification)

| 검증 항목 | 검증 대상 파일 / 위치 | 검증 기준 | 결과 |
| :--- | :--- | :--- | :---: |
| **자동 소환 제거 (`ENTRY-01`)** | `Systems/BossSummonPlayer.cs` | `OnEnterWorld()`에서 `NPC.SpawnOnPlayer` 호출 제거 확인 | ✅ PASS |
| **Infernum 모드 토글 사전 활성화** | `Systems/BossSummonPlayer.cs` | `ActiveArenaBossNpcType` 기반 `ForceInfernumModeActiveInArena()` 호출 유지 | ✅ PASS |
| **입장 안내 문구 갱신** | `Tiles/Test1Tile.cs` | "준비가 되면 소환 아이템을 사용하세요" 텍스트 적용 확인 | ✅ PASS |
| **아레나 게이트 확장** | `Systems/BiomeOverridePlayer.cs` | `BossArenaRoutingRegistry.IsAnyArenaActive()` 체크 확인 | ✅ PASS |
| **퇴장 포털 타일 (`ENTRY-02`)** | `Tiles/ReturnPortalTile.cs` | 우클릭 시 `SubworldSystem.Exit()` 호출 및 피드백 텍스트 출력 | ✅ PASS |
| **포털 스폰 근처 자동 배치** | `Subworlds/ArenaPolishPass.cs` | 스폰 좌표 옆 `(maxTilesX / 2) + 4, surfaceY - 1`에 포털 생성 | ✅ PASS |
| **기존 Return 버튼 보존 (`ENTRY-03`)** | SubworldLibrary 연동 | `noReturn` 설정 없이 기본 Return UI 유지 | ✅ PASS |
| **컴파일 및 패키징** | `dotnet build BossArenaSubWorld.csproj` | 0 오류, 0 경고 빌드 완료 | ✅ PASS |

---

## 2. 실제 플레이테스트 검증 체크리스트 (In-Game Playtest Checklist)

- [ ] **1. 입장 및 안내 확인 (`ENTRY-01`)**:
  - 메인 월드에서 `Test1Tile`에 소환 아이템(예: 슬라임 왕관)을 들고 우클릭 시 `"보스 아레나로 입장합니다. 준비가 되면 소환 아이템을 사용하세요."` 문구가 출력되는가?
- [ ] **2. 도착 후 대기 및 수동 소환 (`ENTRY-01`)**:
  - 아레나 도착 직후 보스가 즉시 스폰되지 않고 플레이어가 자유롭게 움직일 수 있는가?
  - 포션 복용 및 핫바 정렬 후 들고 있는 소환 아이템을 직접 좌클릭 사용 시 보스가 정상 소환되는가?
- [ ] **3. 퇴장 포털 타일 작동 (`ENTRY-02`)**:
  - 스폰 지점 바로 옆에 `ReturnPortalTile`이 위치해 있는가?
  - `ReturnPortalTile` 우클릭 시 `"메인 월드로 귀환합니다."` 메시지와 함께 메인 월드로 정상 귀환하는가?
  - 귀환 후 `Subworld.OnExit()`의 플래그 복구 가드가 작동하여 메인 월드 데이터 오염이 없는가?
- [ ] **4. 기본 Return UI 버튼 확인 (`ENTRY-03`)**:
  - SubworldLibrary 기본 UI의 Return 버튼을 눌렀을 때도 동일하게 정상 귀환하는가?

---

## 3. 예상되는 버그 및 취약점 분석 (Expected Bugs & Vulnerabilities)

1. **소환 아이템 소모 여부**:
   * *동작 원리*: 아레나 서브월드는 `NoPlayerSaving = false`이므로, 서브월드 내에서 소환 아이템을 사용하면 인벤토리에서 1개 소모되어 메인 월드로 반영됩니다. 이는 바닐라 테라리아의 보스전 표준 규칙과 일치합니다.
2. **특정 조건(밤/바이옴) 필요 아이템 사용 조건 충족**:
   * *위험요소*: 문 젤리 위저드나 스피릿 모드 보스 등 밤 전용 보스 소환 시, 서브월드 시간/바이옴이 맞지 않으면 `CanUseItem()`이 false를 반환할 위험.
   * *대응책*: `ForcedTimeSystem.PreUpdateWorld()`가 해당 보스에 대해 매 틱 밤을 강제 유지하고, 바이옴 아레나 타일이 활성화되어 있으므로 정상 사용 가능.
3. **포털 타일 위 보스 투사체/공격 충돌**:
   * *동작 원리*: `ReturnPortalTile`은 솔리드 블록(`tileSolid = true`)이므로 발판으로 사용 가능하며, 전투 중 실수로 밟더라도 우클릭하지 않는 한 귀환되지 않습니다.
