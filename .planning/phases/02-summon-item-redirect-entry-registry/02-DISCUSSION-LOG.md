# Phase 2: Summon-Item Redirect & Entry Registry - Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions are captured in CONTEXT.md — this log preserves the alternatives considered.

**Date:** 2026-08-13
**Phase:** 02-summon-item-redirect-entry-registry
**Areas discussed:** 대상 보스/아이템 선택, 소환 아이템 범위, 리다이렉트 피드백, 보스 자동 소환 위치/타이밍, (user-introduced) 제단형 포탈 오브젝트 설계

---

## 대상 보스/아이템 선택 (Proof boss/item)

| Option | Description | Selected |
|--------|-------------|----------|
| King Slime (Slime Crown) 유지 | Same test boss as Phase 1; non-consumable item simplifies SUBW-04 verification | ✓ |
| 소모성 아이템으로 교체 (예: Suspicious Looking Eye) | Tests the harder "cancel use before consumption" case immediately | |
| 둘 다 (King Slime + 소모성 아이템 하나) | Proves both categories this phase | |

**User's choice:** King Slime (Slime Crown) 유지
**Notes:** None additional.

---

## 소환 아이템 범위 (Registry scope — trigger types)

| Option | Description | Selected |
|--------|-------------|----------|
| 단순 아이템 사용형만 (Slime Crown류) | One `PreUseItem`-style cancel pattern suffices; altar-thrown/bulb-break types need separate hooks and are pushed out of scope | ✓ |
| 아이템 사용형 + 제단/투척형까지 | Broader coverage now, more complexity, risks scope bleed | |

**User's choice:** 단순 아이템 사용형만
**Notes:** None additional.

---

## 진입 방식 (Entry mechanism) — user-introduced mid-discussion, supersedes original design

The user interrupted the standard discussion flow to request a fundamentally different entry mechanism than what PROJECT.md/REQUIREMENTS.md originally specified ("existing summon item alone is the trigger, no separate portal item"). Clarifying sub-questions and answers:

**Q: What kind of object is the new portal item ("Test1")?**
| Option | Selected |
|--------|----------|
| Placeable tile/furniture (altar-style) | ✓ |
| Non-placeable inventory item | |

**Q: What exactly is the "interaction"?**
| Option | Selected |
|--------|----------|
| Right-click the placed tile while holding the boss-summon item | ✓ |
| Use the summon item near the tile (item's own use-action redirected) | |

**Q: Does this fully replace the original item-only design?**
| Option | Selected |
|--------|----------|
| Yes, fully replaces it | ✓ |
| No, keep both as alternate entry paths | |

**Q: What does "reuse the Corruption Altar's appearance" mean technically?**
| Option | Selected |
|--------|----------|
| Sprite/appearance-only copy on a brand-new custom `ModTile` (no inherited vanilla altar behavior) | ✓ |
| Literal reuse of the vanilla Demon/Crimson Altar tile type (inherits hammer-smash hardmode trigger, etc.) | |

**Q: How is "Test1" obtained for this phase?**
| Option | Selected |
|--------|----------|
| Creative menu / debug-only, no crafting recipe | ✓ |
| Simple placeholder crafting recipe (e.g. 50 stone) defined now | |

**Q: Is "Test1" a temporary or final name?**
| Option | Selected |
|--------|----------|
| Temporary — rename before ship | ✓ |
| Final in-code name | |

**Resulting decision:** A new placeable `ModTile` (working name `Test1`), visually benchmarking the Corruption Altar sprite only (no inherited vanilla altar behavior), obtained via Creative/debug for this phase. Right-clicking it while holding a registered boss-summon item triggers the subworld redirect. This fully supersedes the original "summon item alone is the trigger" design — PROJECT.md and REQUIREMENTS.md were updated in this session to reflect the reversal (old decision marked Superseded, not deleted).

---

## 보스 자동 소환 위치/타이밍 (Boss auto-summon timing/location)

| Option | Description | Selected |
|--------|-------------|----------|
| 도착 즉시, 플레이어 위치 근처에서 소환 | No separate spawn-point management needed | |
| 아레나 내 고정된 지점에서 소환 | Guarantees a consistent location on the wide flat platform | |
| (user's own answer, not among the offered options) 서브월드에서 보스 소환아이템으로 소환 | Replay the held summon item's own use-effect inside the subworld — generalizes to any future item without per-boss spawn code | ✓ |

**User's choice:** 서브월드에서 보스 소환아이템으로 소환 (replay-item-use mechanism)
**Notes:** This was the user's own idea, offered directly rather than picking from the presented options. It elegantly avoids needing per-boss spawn logic — recorded as D-09 in CONTEXT.md.

---

## 리다이렉트 피드백 (Redirect UX feedback)

| Option | Description | Selected |
|--------|-------------|----------|
| 간단한 채팅 메시지만 | Minimal feedback, tells the player what happened | ✓ |
| 피드백 없이 즉시 전환 | Simplest, matches debug command behavior | |
| 화면 전환 효과/사운드까지 | Most polished, but adds cost outside this phase's core concern | |

**User's choice:** 간단한 채팅 메시지만
**Notes:** None additional.

---

## Claude's Discretion

- Exact chat message wording for the redirect feedback
- `Test1` tile's texture-reuse implementation approach
- Exact mechanism for replaying a held item's use-effect in the subworld
- Tile placement rules beyond visual similarity to the Corruption Altar

## Deferred Ideas

- Non-"simple-use" summon triggers (altar-thrown, bulb-break) — out of this phase's registry scope
- Screen-transition effects / sound cues on redirect
- `Test1`'s final name/itemization/crafting recipe
- Removing Phase 1's debug commands — timing (this phase vs. a follow-up) to be confirmed at planning time
