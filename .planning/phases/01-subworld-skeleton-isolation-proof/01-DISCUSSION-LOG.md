# Phase 1: Subworld Skeleton & Isolation Proof - Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions are captured in CONTEXT.md — this log preserves the alternatives considered.

**Date:** 2026-08-13
**Phase:** 01-subworld-skeleton-isolation-proof
**Areas discussed:** Test entry/exit trigger, Empty subworld terrain (incl. biome overrides), Isolation-proof method, World-backup & test-world strategy

---

## Test entry/exit trigger

| Option | Description | Selected |
|--------|-------------|----------|
| Debug chat command | `/bossarena-enter` / `/bossarena-exit`; fast to add, trivial to strip out | ✓ |
| Temporary test item | Throwaway ModItem that teleports on use | |
| Debug hotkey | ModKeybind bound to enter/exit | |

**User's choice:** Debug chat command
**Follow-up:** Lifespan of the debug command — "remove once Phase 2 lands" vs. "keep behind a debug flag permanently". User chose: remove once Phase 2's real redirect is complete.

---

## Empty subworld terrain

| Option | Description | Selected |
|--------|-------------|----------|
| Minimal flat platform | Superflat-style single dirt/stone floor | ✓ |
| Complete void | No floor at all, pure air/void | |
| Platform-based decorated arena | Multiple layers of platforms, full fight-ready arena now | |

**User's choice:** Minimal flat platform now; full platform-decorated arena explicitly deferred to a later phase (when boss fights actually happen).

**Follow-up: material** — Stone block (recommended) chosen over wood platform tiles.
**Follow-up: size** — User initially said "최소 1만 블록" (at least 10,000 blocks); clarified via follow-up to mean ~10,000 blocks *wide* (very wide flat plane), not total area.
**Follow-up: thickness** — User: "그냥 돌 블록으로 해" (just make it stone blocks) — no elaborate depth requirement; left as Claude's discretion (thin layer, ~10-20 blocks).
**Follow-up: edge walls** — No boundary walls needed (recommended), given the platform's large width.

**Mid-discussion tangent — biome requirements:** User asked how boss-specific biome requirements (Wall of Flesh/Underworld, Plantera/Jungle, Duke Fishron/Ocean, etc.) should be handled.
- Question: which phase should own "fake biome flag" handling — Phase 1 (infra now) or Phase 2 (when summon-item registry already maps item→boss)?
  User's choice: **Phase 1, prepare infrastructure now.**
- Follow-up: what shape should that infrastructure take now, since no BossRegistry exists yet (Phase 3)?
  Options: (a) general-purpose hook only, to be wired to specific bosses later (recommended); (b) directly parameterize the debug entry command with a biome argument now.
  User's choice: **(a) general-purpose hook only.**

---

## Isolation-proof method

| Option | Description | Selected |
|--------|-------------|----------|
| King Slime kill/drop flag | Cheapest/fastest vanilla boss to summon and kill repeatedly | ✓ (after correction) |
| Direct debug flag toggle | Set `NPC.downedSlimeKing = true` directly, skip the actual fight | |

**User's choice (first pass):** Moon Lord — rejected on follow-up because Moon Lord requires 3 mechanical bosses + Golem defeated first, making repeat testing impractical. User then said: "아니면 그냥 킹슬라임으로해줘" (just use King Slime instead).

**Follow-up (King Slime, method):** Real kill via BossCoreItem-equivalent observation, vs. simple debug flag toggle.
User's choice: **Actually summon/kill King Slime for real** (not a debug flag toggle) — no carrier item involved yet (Phase 3 doesn't exist), the test purely observes whether `NPC.downedSlimeKing` propagates to the main world unassisted after returning.

---

## World-backup & test-world strategy

| Option | Description | Selected |
|--------|-------------|----------|
| New disposable test world | Fresh throwaway world, no risk to the real save | ✓ |
| Backed-up copy of real save (HiPo's_Terrarium) | Tests in an environment matching real play conditions | |

**User's choice:** New disposable test world — further clarified: all other content mods will be unloaded/disabled for this test world, since Phase 1 only needs to prove the generic subworld/isolation mechanism, not any specific mod's boss.

**Follow-up:** Should the VERIFY-02 backup-guidance document still be written now, even though Phase 1's own testing doesn't need it?
User's choice: **Yes — write it now as forward-looking documentation** for later phases (4-8) when testing must happen against the real save with all mods enabled.

---

## Claude's Discretion

- Exact platform Y-level/vertical position within the subworld
- Precise platform thickness value (thin, ~10-20 blocks as a guideline)
- `GenPass` implementation details for the flat stone platform
- Debug command naming/argument syntax
- Format/location of the world-backup guidance document

## Deferred Ideas

- Full arena decoration (multi-layer platforms, aesthetics) — user's "플랫폼을 이용한 아레나로 꾸미기" idea, deferred past Phase 1; bounded by REQUIREMENTS.md's existing "Full arena-building/decoration toolkit" Out of Scope entry.
- Per-boss biome-to-flag mapping — only the generic override hook is built in Phase 1; actual boss-specific wiring starts at Phase 3+.
