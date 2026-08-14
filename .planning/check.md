# 라이브 검증 체크리스트 (전체 취합)

현재 코드로는 완료됐지만 실제 게임에서 사람이 직접 확인해야 하는 항목들을 순서대로 모았다.
Phase 07(고블린 채리엇), Phase 08(전체 파이프라인/Boss Checklist 검증), Phase 10(신규 보스 로스터 라이브 검증)에 걸쳐 있다.

**권장 진행 순서:** ① → ② → ③ → ④ (①③④는 서로 의존성 없어 아무 때나 가능, ②가 이번 세션 재개 지점). Phase 10 코드 등록(10-01~10-05)은 전부 완료·병합·빌드 확인됨 — ④도 이제 바로 진행 가능 (단, ④ 시작 전 tModLoader 재시작 필요, 아래 참고).

---

## ① Phase 08-01 — Boss Checklist 정상 동작 + King Slime/Hive Mind 트래커 UI 확인
*(의존성 없음, 지금 바로 가능)*

### 사전 준비
- [ ] Boss Checklist(JavidPack/BossChecklist)가 Mod Configuration에서 활성화·로드되는지 확인 (`.tmod` 파일이 로컬에 없으면 재구독 후 재실행)
- [ ] 트래커 UI가 인게임에서 정상적으로 열리는지 확인
- [ ] `docs/WORLD_BACKUP_GUIDANCE.md` 절차대로 월드 백업

### King Slime
- [ ] 트래커 UI에서 King Slime이 처치됨으로 표시되는지 확인 (이미 다운 상태일 것)
- [ ] 표시 안 되면: Test1 포탈 + King Slime 소환 아이템 → 서브월드 처치 → `BossCoreItem` 드롭 → 메인 월드 적용 → 재확인

### Hive Mind
- [ ] 트래커 UI에서 Hive Mind가 처치됨으로 표시되는지 확인 (이미 다운 상태일 것)
- [ ] 표시 안 되면: BossArenaCorruptionSubworld 라우팅 + Hive Mind 소환 아이템 → 서브월드 처치 → 드롭 → 적용 → 재확인

### Infernon
- [ ] **재테스트 불필요** — Phase 5에서 이미 트래커 UI 확인 기록 있음 (인용만 함)

**통과 시 resume-signal:** `"wave 1 verified"`

---

## ② Phase 07-02 — 고블린 채리엇 (ContinentOfJourney / Homeward Journey)
*(이번 세션 재개 지점 — 08-03도 이 결과로 동시에 닫힘, 중복 테스트 불필요)*

### Task 1: 파이프라인 + Boss Checklist 인식
- [ ] Homeward Journey(Workshop 2930931197) 구독·활성화, 에러 없이 로드 확인
- [ ] 월드 백업
- [ ] `PurpleFlareGun` 획득 (CheatSheet)
- [ ] `PurpleFlareGun` 소지 상태로 Test1 포탈 우클릭 → 서브월드 진입, 고블린 채리엇 자동 소환 확인
- [ ] 처치 → `BossCoreItem`(태그: `continentofjourney:goblin_chariot`) 드롭 확인 (서브월드 안에서만)
- [ ] 메인 월드로 귀환
- [ ] 드롭된 아이템 사용 → 다음 확인:
  - [ ] `ContinentOfJourney.DownedBossSystem.downedGoblinChariot == true`
  - [ ] Boss Checklist 트래커 UI에서 고블린 채리엇이 처치됨으로 표시 (Homeward Journey 자체 `CoJ_BossChecklist.cs` 통합 확인)
  - [ ] `NPC.SetEventFlagCleared` 중 예외/JITException 없음
- [ ] 같은 아이템 재사용 → 중복 적용/중복 Boss Checklist 항목 없음 확인 (APPLY-04 멱등성)

**통과 시 resume-signal:** `"goblin chariot verified"`

### Task 2: ContinentOfJourney 비활성화 안전성
- [ ] ContinentOfJourney(Homeward Journey)만 비활성화 → 재시작/월드 로드 → JIT 크래시·예외 다이얼로그 없음
- [ ] `Logs/client.log`에 `HomewardJourneyIntegration` 관련 예외 없음 확인
- [ ] King Slime/Hive Mind/Infernon/Thorn/Astrageldon 등 다른 기능 정상 동작 확인
- [ ] ContinentOfJourney 재활성화, 모드 목록 원복 확인

**통과 시 resume-signal:** `"mod-disabled safety verified"`

---

## ③ Phase 08-02 — Thorn(Redemption) + Astrageldon(CatalystMod)
*(①이 끝나야 실행 가능한 wave. Phase 6의 미실행 06-03을 동시에 닫음)*

### Task 1: 파이프라인 + Boss Checklist + 문 로드 락아웃
- [ ] `HeartOfThorns` 획득 → Test1 포탈 우클릭 → 서브월드 진입, Thorn 자동 소환 확인
- [ ] Thorn 처치 → `BossCoreItem`(태그: `redemption:thorn`) 드롭 확인
- [ ] 메인 월드 복귀 → 아이템 사용 → 다음 확인:
  - [ ] `Redemption.Globals.RedeBossDowned.downedThorn == true`
  - [ ] `ThornDowned` 채팅 메시지 출력
  - [ ] `RedeWorld.Alignment` +2 (직접 확인 어려우면 코드 레벨 보증으로 수용)
  - [ ] Boss Checklist 트래커 UI에서 Thorn 처치됨 표시
- [ ] `AstralCommunicator` 획득 → Test1 포탈 우클릭 → **기본 아레나**(Astral 바이옴 아님, 정상 동작)에서 Astrageldon 자동 소환 확인
- [ ] Astrageldon 처치 → `BossCoreItem`(태그: `catalyst:astrageldon`) 드롭 확인
- [ ] 메인 월드 복귀 → 아이템 사용 → 다음 확인:
  - [ ] `CatalystMod.WorldDefeats.downedAstrageldon == true`
  - [ ] `MetanovaGenerator.Generate()` 광맥 타일이 실제로 생성됨
  - [ ] Boss Checklist 트래커 UI에서 Astrageldon 처치됨 표시
- [ ] **문 로드 락아웃 (신규 항목)**:
  - [ ] 차단 케이스: 문 로드 처치 + Astrageldon 미처치 상태에서 `AstralCommunicator`로 Test1 우클릭 → 서브월드 진입 안 됨, 아이템 미소모 확인
  - [ ] 정상 케이스: Astrageldon을 먼저 처치한 뒤에는 문 로드를 잡아도 `AstralCommunicator` 리다이렉트가 정상 작동하는지 확인
- [ ] Thorn/Astrageldon 아이템 재사용 → 중복 메시지/중복 광맥 생성/중복 Boss Checklist 항목 없음 확인 (APPLY-04)

**통과 시 resume-signal:** `"thorn and astrageldon verified"`

### Task 2: Redemption/CatalystMod 비활성화 안전성
- [ ] Redemption만 비활성화 → 정상 로드, `Logs/client.log`에 `RedemptionIntegration` 예외 없음 → 재활성화
- [ ] CatalystMod만 비활성화 → 정상 로드, `Logs/client.log`에 `CatalystIntegration` 예외 없음 → 재활성화
- [ ] 둘 다 재활성화된 상태로 모드 목록 원복 확인

**통과 시 resume-signal:** `"mod-disabled safety verified"`

---

## ④ Phase 10-06 — 신규 보스 로스터(18종) 라이브 검증
*(Phase 10의 코드 등록 10-01~10-05가 전부 완료·병합·빌드 확인됨. 이제 라이브 테스트 가능.)*

**⚠ 먼저 확인**: 마지막 병합(10-05, The Old Duke) 직후 `dotnet build`가 `TML003: Please close tModLoader or disable the mod in-game to build mods directly` 오류로 실패했습니다 (C# 컴파일 자체는 0 warnings/0 errors — 순수 tModLoader가 실행 중이라 `.tmod` 패키징만 잠긴 상태). 즉 **현재 로드된 `.tmod`는 Wave 4(10-04)까지만 반영**돼 있고 The Old Duke(10-05) 코드는 아직 패키징 안 됐습니다. tModLoader를 완전히 종료한 뒤 `dotnet build BossArenaSubWorld.csproj`를 한 번 더 돌려서 최신 `.tmod`를 만들고 재시작한 다음 아래 테스트를 진행해주세요.

전 항목 통과 시 10-VALIDATION.md의 "Sampling Rate" 기준에 따라 18개 보스를 전부 개별 테스트하지 않고, 대표 샘플 + 전체 게이팅 매트릭스로 검증합니다.

### 사전 준비
- [ ] tModLoader 완전 종료 → `dotnet build` 재실행 → `.tmod` 최신화 확인
- [ ] CalamityMod, SpiritMod, **InfernumMode** 활성화 (BossArenaSubWorld, SubworldLibrary, CheatSheet, BossChecklist와 함께)
- [ ] 월드 백업

### Task 1: 등록 매트릭스 + 대표 보스 라이브 전투 검증
- [ ] **Zone-functional 보스**: `ExoticPheromones`(Dragonfolly) → Test1 → `BossArenaJungleSubworld` 진입 확인 → 1~2분 전투해도 despawn/enrage 없음 확인
- [ ] `ScarabIdol`(Scarabeus) → Test1 → `BossArenaDesertSubworld` 진입, 정상 데미지(1/3 축소 아님) 확인
- [ ] 둘 다 처치 → 메인 월드 복귀 → 아이템 사용 → `downedDragonfolly`, `DownedScarabeus` true 확인
- [ ] **Zone-thematic 보스 샘플**: `JewelCrown`(Ancient Avian) → Test1 → `BossArenaSpaceSubworld` 정상 전투 → 처치 후 `DownedAncientAvian` true 확인
- [ ] **폴리모픽 MarkofProvidence (3개 Zone 전부 테스트, InfernumMode 비활성 상태에서)**:
  - [ ] 던전에서 사용 → Ceaseless Void 소환 확인
  - [ ] 지하세계(Underworld)에서 사용 → Signus 소환 확인
  - [ ] 우주(Space, 맵 상단)에서 사용 → Storm Weaver 소환 확인
  - [ ] 3번 다 아이템 소모 안 됨 확인
  - [ ] 셋 중 하나 처치 후 아이템 사용 → 해당 `downedCeaselessVoid`/`downedSignus`/`downedStormWeaver` true 확인
- [ ] **Infernum 게이팅 매트릭스 — InfernumMode 비활성 상태**:
  - [ ] `ProfanedCore`(Providence), `ProfanedShard`(Profaned Guardians), 던전에서 `MarkofProvidence`(Ceaseless Void) 전부 정상 리다이렉트 확인
  - [ ] `BloodwormPlatter`(The Old Duke)는 리다이렉트 전혀 안 됨 확인 (아이템도 존재 안 할 수 있음 — InfernumMode 자체가 로드 안 됐으므로)
- [ ] **Infernum 게이팅 매트릭스 — InfernumMode 활성 상태**:
  - [ ] `ProfanedCore`/`ProfanedShard`/던전 `MarkofProvidence` 전부 리다이렉트 안 됨(아이템 미소모, 메시지 없음) 확인 — Infernum 자체 구조물 기반 트리거로 넘어감
  - [ ] `BloodwormPlatter` 정상 리다이렉트 → The Old Duke 자동 소환 → 처치 후 아이템 사용 → `downedBoomerDuke` true 확인 (`downedOldDuke` 아님) + Sea King 상점 해금 확인
  - [ ] Astrum Deus(`Starcore`)/Astrum Aureus(`AstralChunk`) 둘 다 `BossArenaAstralSubworld` 진입 시 강제로 밤(`Main.dayTime == false`)이 되는지 확인
- [ ] **InfernumMode 비활성 상태에서는** Astrum Deus/Aureus 밤 강제 안 됨(평범한 낮 아레나) 확인
- [ ] **재사용 멱등성**: 이미 처치한 보스 중 하나의 `BossCoreItem`을 재사용 → 중복 메시지/월드젠 없음 확인

**통과 시 resume-signal:** `"all verified"`

### Task 2: 강제 밤 지속 — 풀 전투 시간 검증 (Pitfall 6)
- [ ] `DreamlightJellyItem`(Moon Jelly Wizard) 또는 `DuskCrown`(Dusking) 중 하나(가능하면 둘 다)로 소환 → 서두르지 않고 자연스러운 풀 전투 시간 동안 진행
- [ ] 전투 내내 화면이 계속 밤으로 유지되는지 확인 (도착 순간만이 아니라 전투 끝까지)
- [ ] 전투 중간에 `Main.dayTime`이 낮으로 전환돼 보스가 despawn/비활성화되지 않는지 확인

**통과 시 resume-signal:** `"forced night confirmed persistent"`

### Task 3: CalamityMod/SpiritMod 비활성화 JIT 안전성
- [ ] CalamityMod만 비활성화 → 재시작/로드 → 크래시·JITException 없음, `Logs/client.log`에 `CalamityIntegration` 관련 예외 없음 → 재활성화
- [ ] SpiritMod만 비활성화 → 재시작/로드 → 크래시·JITException 없음, `Logs/client.log`에 `SpiritIntegration` 관련 예외 없음 → 재활성화
- [ ] 둘 다 재활성화 상태로 모드 목록 원복 확인

**통과 시 resume-signal:** `"mod-disabled safety verified"`

대상 보스 전체 (18종, 등록 완료):
- Calamity 12종: Providence, Profaned Guardians, Ceaseless Void, The Old Duke, Signus, Storm Weaver, Astrum Deus, Astrum Aureus, Dragonfolly, Devourer of Gods, Yharon, Supreme Witch Calamitas
- Spirit 6종: Ancient Avian, Scarabeus, Vinewrath Bane, Moon Jelly Wizard, Dusking, Atlas

---

각 항목 결과(통과 여부, 실패 시 어느 단계에서 뭘 관찰했는지)를 알려주시면 해당 SUMMARY.md 작성 및 다음 단계 진행하겠습니다.
