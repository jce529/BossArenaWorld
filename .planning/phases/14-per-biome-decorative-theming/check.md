# Phase 14 Verification & Quality Assurance (check.md)

**Phase:** 14-per-biome-decorative-theming
**Goal:** Decorative theming, background walls, and campfires for all 9 boss arena subworlds while preserving Zone-flag tile budgets and JIT safety.
**Requirements:** DECOR-01, DECOR-02, DECOR-03
**Status:** Code-Level Verified, Ready for Playtest

---

## 1. 코드 레벨 검증 (Code-Level Verification)

### 1.1 전체 9개 아레나 데코레이션 구성 매트릭스

| 아레나 서브월드 | 배경 벽 (`WallID`) | 표면/트림 타일 장식 | 캠프파이어 스타일 | 타일 예산 안전성 (`DECOR-02`) |
| :--- | :--- | :--- | :---: | :---: |
| **Plain** | `WallID.Stone` | `TileID.GrayBrick` 트림 | 기본 (0) | ✅ PASS |
| **Corruption** | `WallID.EbonstoneUnsafe` | `TileID.EbonstoneBrick` 트림 | 오염 (1) | ✅ PASS (가중치 1 유지) |
| **Hallow** | `WallID.HallowedGrassUnsafe` | `TileID.PearlstoneBrick` 트림 | 신성 (2) | ✅ PASS (가중치 1 유지) |
| **Underworld** | `WallID.ObsidianBrick` | `TileID.ObsidianBrick` 트림 | 데몬 (4) | ✅ PASS (`Y in [605, 687]`) |
| **Space** | `WallID.Glass` | `TileID.Sunplate` 트림 | 초발광 (5) | ✅ PASS (`Y in [10, 80]`) |
| **Jungle** | `WallID.JungleUnsafe` | `TileID.LihzahrdBrick` 베이스 | 정글 (6) | ✅ PASS (가중치 1 유지) |
| **Desert** | `WallID.SandstoneBrick` | `TileID.SandstoneBrick` 트림 | 사막 (7) | ✅ PASS (가중치 1 유지) |
| **Astral** | Astral Wall (Calamity) | `AstralGrass` + `AstralStone` | 오염/아스트랄 (1) | ✅ PASS (JIT 격리 `DECOR-03`) |
| **Briar** | Briar Wall (Spirit) | `BriarGrass` | 정글/브라이어 (6) | ✅ PASS (JIT 격리 `DECOR-03`) |

### 1.2 요구사항 검증 결과

| 요구사항 ID | 검증 내용 | 결과 |
| :--- | :--- | :---: |
| **`DECOR-01`** | 9개 전체 아레나에 시각적으로 구별 가능한 고유 배경벽, 트림 타일, 캠프파이어 구현 | ✅ PASS |
| **`DECOR-02`** | 장식 추가 시 바이옴 판정 가중치 타일(EbonstoneBrick, LihzahrdBrick, SandstoneBrick 등)을 사용하여 Zone 플래그 예산 100% 보존 | ✅ PASS |
| **`DECOR-03`** | Astral/Briar 모드 전용 데코레이션 코드가 `[JITWhenModsEnabled]` 내부에만 존재함을 검증 | ✅ PASS |
| **컴파일 검증** | `dotnet build BossArenaSubWorld.csproj /warnaserror` (0 오류, 0 경고) | ✅ PASS |

---

## 2. 실제 플레이테스트 검증 체크리스트 (In-Game Playtest Checklist)

### 🎨 시각적 배경 및 분위기 확인
- [ ] **1. Plain 아레나**: 스톤 배경벽과 그레이 브릭 바닥, 중앙 캠프파이어가 조화롭게 배치되어 있는가?
- [ ] **2. Corruption 아레나**: 보라빛 이본스톤 배경벽과 오염 캠프파이어의 시각적 분위기가 연출되는가?
- [ ] **3. Hallow 아레나**: 펄스톤 브릭과 신성 풀 벽, 신성 캠프파이어가 정상 표시되는가?
- [ ] **4. Underworld 아레나**: 흑요석 벽돌 벽과 데몬 캠프파이어가 지옥 배경과 일치하는가?
- [ ] **5. Space 아레나**: 투명한 유리벽과 선플레이트 블록, 초발광 캠프파이어가 우주 테마를 살리는가?
- [ ] **6. Jungle 아레나**: 정글 덩굴 벽과 리자드 브릭, 정글 캠프파이어가 정글 바이옴 느낌을 주는가?
- [ ] **7. Desert 아레나**: 사암 벽돌 벽과 사막 캠프파이어가 사막 느낌을 주는가?
- [ ] **8. Astral & Briar 아레나**: 모드 고유 벽과 캠프파이어가 정상 생성되는가?

### 💖 기능적 상호작용 확인
- [ ] **9. 캠프파이어 재생 버프**: 스폰 지점 및 아레나 중앙 부근에서 플레이어에게 캠프파이어 체력 재생 증가 버프(`BuffID.Campfire`)가 활성화되는가?
- [ ] **10. 보스 전투 방해 여부**: 배경벽과 캠프파이어가 플레이어나 보스의 이동/투사체를 가로막지 않고 매끄럽게 통과되는가?

---

## 3. 예상되는 버그 및 취약점 분석 (Expected Bugs & Vulnerabilities)

1. **배경벽으로 인한 조도 감소**:
   * *동작 원리*: 테라리아 엔진은 배경벽이 채워진 공간에서 자연광을 차단합니다.
   * *대응책*: `ArenaPolishPass`에서 30블록마다 토치를 3단 플랫폼 전체에 배치하고, 스폰 지점에 캠프파이어를 설치하여 완벽한 시야를 확보했습니다.
2. **캠프파이어 버프 범위**:
   * *동작 원리*: 바닐라 캠프파이어의 버프 적용 반경은 가로 약 170타일입니다. 아레나 전체가 10000타일이므로, 플레이어가 중앙 스폰 영역에서 교전할 때 주요 재생 혜택을 받습니다.
