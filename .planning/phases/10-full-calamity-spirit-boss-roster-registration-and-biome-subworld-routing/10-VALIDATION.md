---
phase: 10
slug: full-calamity-spirit-boss-roster-registration-and-biome-subworld-routing
status: draft
nyquist_compliant: false
wave_0_complete: false
created: 2026-08-14
---

# Phase 10 — Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | None — tModLoader mod, no automated in-game test harness (matches Phase 1-6/9's established precedent) |
| **Config file** | none |
| **Quick run command** | `dotnet build BossArenaSubWorld.csproj` (compile-check only — catches type/reference errors, does NOT catch JIT-safety issues, those need the mod-disabled live load test) |
| **Full suite command** | N/A — "full verification" is the live in-game checklist below, per-boss, following the `check.md` precedent from Phase 6 |
| **Estimated runtime** | ~10-20 seconds (build only; live checkpoints are manual and unbounded) |

---

## Sampling Rate

- **After every task commit:** `dotnet build BossArenaSubWorld.csproj` (compile-check every registration added)
- **After every plan wave:** Full mod-disabled load-safety smoke test (CalamityMod disabled, SpiritMod disabled, both disabled) — Phase 4/9 precedent
- **Before `/gsd:verify-work`:** Live in-game checklist covering at minimum: one Zone-functional boss (Dragonfolly or Scarabeus), one Zone-thematic-only boss, both forced-night bosses (full-duration fight, per Open Question 3), the polymorphic `MarkofProvidence` item routing correctly to all 2-3 reachable bosses, and the Infernum-conditional gating (with and without InfernumMode enabled)
- **Max feedback latency:** ~20 seconds (build gate only; live checkpoints are manual and unbounded)

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|-----------|-------------------|-------------|--------|
| 10-01 | 01 | 0 | ARENA-01 | build | `dotnet build BossArenaSubWorld.csproj` | ❌ W0 | ⬜ pending |
| 10-0x | TBD | 1 | ARENA-01 | build (compile-time type check, per boss registration) | `dotnet build BossArenaSubWorld.csproj` | ❌ W0 | ⬜ pending |
| 10-0x | TBD | 1 | ARENA-01 | manual-only, live in-game | kill each registered boss in its routed subworld, return, use `BossCoreItem`, confirm downed flag + side effects apply | ❌ W0 | ⬜ pending |
| 10-0x | TBD | 1 | ARENA-01 (Zone-functional) | manual-only, several-minute live fight | full-duration fight for Dragonfolly (Jungle leave-timer), Scarabeus (Desert damage-scaling), Moon Jelly Wizard + Dusking (forced-night persistence, Open Question 3) — confirm no despawn/underperformance | ❌ W0 | ⬜ pending |
| 10-0x | TBD | 1 | ARENA-01 (polymorphic) | manual-only, live in-game | use `MarkofProvidence` from each of the 2-3 reachable Zones, confirm it resolves to Ceaseless Void / Signus / Storm Weaver correctly | ❌ W0 | ⬜ pending |
| 10-0x | TBD | 1 | ARENA-01 (Infernum-gated) | manual-only, live in-game, two mod configurations | with InfernumMode disabled: Providence/Profaned Guardians/Ceaseless Void register, The Old Duke does not. With InfernumMode enabled: reverse, plus Astrum Deus/Aureus force night | ❌ W0 | ⬜ pending |
| 10-0x | TBD | final | ARENA-01 (JIT safety) | manual-only, real checkpoint | disable CalamityMod and (separately) SpiritMod in Mod Configuration, launch, confirm no `JITException` in client log | ❌ W0 | ⬜ pending |

*Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky*

---

## Wave 0 Requirements

*Existing infrastructure covers all phase requirements — `Libs/CalamityMod.dll`, `Libs/SpiritMod.dll`, and `ModReader/InfernumMode/build.txt` are already present locally per 10-RESEARCH.md Sources. No new Wave 0 extraction/reference work is needed unless planning finds a gap.*

---

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|--------------------|
| Per-boss downed-flag + side-effect application | ARENA-01 | No automated tModLoader in-game test harness exists in this project (established precedent since Phase 1) | Kill each registered boss in its routed arena subworld, return to main world, use the dropped `BossCoreItem`, confirm the correct downed flag/field is set and any documented side effect (chat message, netcode sync, WorldGen) fires |
| Zone-functional despawn/underperformance check | ARENA-01 | Despawn timers and damage-scaling can only be observed during a real, several-minute-long fight — no static check substitutes | Fight Dragonfolly to confirm no Jungle leave-timer despawn; fight Scarabeus to confirm correct Desert damage scaling; fight Moon Jelly Wizard/Dusking for the full fight duration to confirm forced night persists (Open Question 3 — whether `Main.time` advances naturally inside a subworld during an active fight is unconfirmed by decompile) |
| `MarkofProvidence` polymorphic resolution | ARENA-01 | The item's boss resolution depends on the player's live Zone state at use-time — cannot be verified by static analysis | Use `MarkofProvidence` while positioned to trigger each of the 2-3 reachable Zone conditions, confirm it resolves to and summons the correct boss (Ceaseless Void / Signus / Storm Weaver) each time |
| Infernum-conditional registration matrix | ARENA-01 | Whether a boss is registered at all depends on `ModLoader.HasMod("InfernumMode")` at runtime — requires toggling the actual mod | With InfernumMode disabled, confirm Providence/Profaned Guardians/Ceaseless Void register and The Old Duke does not (no summon item to hook). With InfernumMode enabled, confirm the reverse, and that Astrum Deus/Astrum Aureus force night in their subworld |
| CalamityMod-disabled / SpiritMod-disabled load safety | ARENA-01 | JIT-crash behavior can only be observed by actually disabling each mod and relaunching tModLoader — no static check can substitute (per Phase 4/5/9 precedent, including the real JITException caught live in Phase 09-07) | Disable CalamityMod only (leave SpiritMod/others as-is), relaunch, confirm no `JITException` naming any Phase 10 registration method in `Logs/client.log`; re-enable; repeat for SpiritMod only |

---

## Validation Sign-Off

- [ ] All tasks have `<automated>` verify or Wave 0 dependencies
- [ ] Sampling continuity: no 3 consecutive tasks without automated verify
- [ ] Wave 0 covers all MISSING references (none currently identified — see Wave 0 Requirements)
- [ ] No watch-mode flags
- [ ] Feedback latency < 20s (build gate)
- [ ] `nyquist_compliant: true` set in frontmatter

**Approval:** pending
