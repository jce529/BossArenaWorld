# Phase 6 라이브 검증 체크리스트

Wave 1(06-01: DLL 연결)·Wave 2(06-02: Thorn/Astrageldon 등록 + 문 로드 락아웃)는 코드 레벨로 완료·병합됨(`dotnet build` 통과). 아래는 06-03-PLAN.md의 두 체크포인트 + 이번 세션에서 추가된 문 로드 락아웃 항목을 실제 게임에서 직접 확인해야 하는 목록으로 정리한 것.

## 사전 준비
- [ ] Redemption(Workshop 2893332653), CatalystMod(Workshop 2838015851) 구독 후 tModLoader 실행해 `Mods\` 폴더에 동기화
- [ ] Mod Configuration에서 Redemption + CatalystMod 활성화 (CalamityMod/SpiritMod/SubworldLibrary/CheatSheet/BossChecklist/BossArenaSubWorld와 함께), 리로드하여 에러 없이 로드되는지 확인
- [ ] `docs/WORLD_BACKUP_GUIDANCE.md` 절차대로 월드 백업 또는 새 테스트용 throwaway 월드 준비

## 1. Thorn (Redemption)
- [ ] `HeartOfThorns` 아이템 획득 (CheatSheet 아이템 스폰 메뉴)
- [ ] Test1 포탈 타일 우클릭(HeartOfThorns 소지 상태) → 서브월드 진입, Thorn 자동 소환 확인
- [ ] Thorn 처치 → `BossCoreItem`(태그: `redemption:thorn`) 드롭 확인 (서브월드 안에서만)
- [ ] SubworldLibrary Return으로 메인 월드 복귀
- [ ] 드롭된 `BossCoreItem` 사용 → 다음 확인:
  - [ ] `Redemption.Globals.RedeBossDowned.downedThorn == true` (Boss Checklist 트래커 UI 또는 CheatSheet 변수 조회로 확인)
  - [ ] `Mods.Redemption.StatusMessage.Progression.ThornDowned` 채팅 메시지 출력
  - [ ] `RedeWorld.Alignment` +2 (직접 확인 어려우면 코드 레벨 보증으로 수용 가능)
- [ ] 같은 아이템 재사용 → 메시지 중복/재적용 없음 확인 (APPLY-04 멱등성)

## 2. Astrageldon (CatalystMod)
- [ ] `AstralCommunicator` 아이템 획득 (CheatSheet)
- [ ] Test1 포탈 타일 우클릭(AstralCommunicator 소지 상태) → 서브월드 진입, Astrageldon 자동 소환 확인
- [ ] Astrageldon 처치 → `BossCoreItem`(태그: `catalyst:astrageldon`) 드롭 확인
- [ ] 메인 월드 복귀
- [ ] 드롭된 `BossCoreItem` 사용 → 다음 확인:
  - [ ] `CatalystMod.WorldDefeats.downedAstrageldon == true`
  - [ ] `MetanovaGenerator.Generate()` 광맥 타일이 플레이어 근처에 실제로 생성됨
  - [ ] `NPC.SetEventFlagCleared` 호출 중 예외/크래시 없음
- [ ] 같은 아이템 재사용 → 중복 광맥 생성/재적용 없음 확인 (APPLY-04 멱등성)

## 3. 문 로드 락아웃 (이번 세션 추가 항목 — 06-03 원안에는 없음, 신규 확인 필요)
CatalystMod 실제 `AstralCommunicator.CanUseItem()`의 문 로드 락아웃을 우리 리다이렉트 파이프라인에도 재현했는지 확인:
- [ ] **차단 케이스**: 문 로드를 먼저 처치하고 Astrageldon은 아직 안 잡은 캐릭터로, `AstralCommunicator`를 들고 Test1 포탈 우클릭 → 서브월드 진입이 되지 않고(아이템 소모 없이) 아무 반응이 없는지 확인
- [ ] **정상 케이스**: Astrageldon을 먼저 잡은 뒤(`WorldDefeats.downedAstrageldon == true`) 문 로드를 잡아도, `AstralCommunicator`로 재소환 리다이렉트가 계속 정상 작동하는지 확인 (재사용 시나리오)

## 4. 모드 비활성화 안전성 (JIT 크래시 검증)
- [ ] Redemption만 비활성화 후 재시작/월드 로드 → JIT 크래시·예외 다이얼로그 없음, `Logs/client.log`에 `RedemptionIntegration` 관련 예외 없음 → 재활성화
- [ ] CatalystMod만 비활성화 후 재시작/월드 로드 → JIT 크래시·예외 다이얼로그 없음, `Logs/client.log`에 `CatalystIntegration` 관련 예외 없음 → 재활성화
- [ ] 둘 다 재활성화된 상태로 모드 목록 원복 확인

---

전체 항목 통과 시:
- 1~2번 항목 결과 → 06-03-PLAN.md Task 1의 `resume-signal`("both bosses verified" 또는 실패 지점 설명)
- 3번 항목 → 별도로 통과/실패 알려주기 (신규 항목이라 SUMMARY.md에 별도 기록)
- 4번 항목 결과 → 06-03-PLAN.md Task 2의 `resume-signal`("mod-disabled safety verified" 또는 예외 내용)

결과를 알려주시면 `06-03-SUMMARY.md` 작성 및 Phase 6 완료 처리를 진행합니다.
