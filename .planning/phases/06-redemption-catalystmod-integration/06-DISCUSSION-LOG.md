# Phase 6: Redemption & CatalystMod Integration - Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions are captured in CONTEXT.md — this log preserves the alternatives considered.

**Date:** 2026-08-14
**Phase:** 06-redemption-catalystmod-integration
**Areas discussed:** CatalystMod source-access approach, Redemption worked-example boss selection, CatalystMod worked-example boss selection

---

## CatalystMod source-access approach

| Option | Description | Selected |
|--------|-------------|----------|
| ilspycmd로 직접 디컴파일 | Same approach Phase 4/5/9 used for Calamity/Spirit — decompile the installed .tmod's embedded DLL directly. Bypasses the modder's explicit "hide from tModReader" preference. | ✓ |
| 런타임 리플렉션만 사용 | Blind runtime type/member probing, no static decompile. Respects modder's wish more, but slower and more fragile. | |
| 공개 문서/위키/깃허브만 참고 | No decompilation at all. Research blocked if public docs are insufficient. | |

**User's choice:** ilspycmd로 직접 디컴파일 (추천)
**Notes:** CatalystMod's `extract.log` shows the modder explicitly hid code, resources, and even the raw DLL from tModReader, and a `HelloDataminers.txt` file signals deliberate anti-datamining intent. User made an informed choice to proceed with decompilation anyway, for personal/individual use against their own installed copy (no redistribution planned).

---

## Redemption worked-example boss selection

| Option | Description | Selected |
|--------|-------------|----------|
| 리서치가 정하게 (풍부한 side-effect 기준) | Same selection discipline as Phase 4 (Hive Mind)/Phase 5 (Infernon): decompile all 10 bosses' OnKill(), pick the richest. | ✓ |
| 특정 보스를 직접 지정 | User names a specific boss (e.g. one they fight often) to prioritize. | |

**User's choice:** 리서치가 정하게 다설 (추천)
**Notes:** No specific boss requested. Research-phase will evaluate all 10 Redemption bosses (ADD, Cleaver, Erhan, Gigapora, Keeper, KSIII, Neb, Obliterator, PatientZero, SeedOfInfection, Thorn).

---

## CatalystMod worked-example boss selection

| Option | Description | Selected |
|--------|-------------|----------|
| Astrageldon을 지정 | Prominent in CatalystMod's asset tree (dedicated loading-screen art, background, pet projectile) — likely the headline/final boss. | ✓ |
| 리서치가 정하게 다설 | Same richest-side-effect heuristic as Redemption. | |

**User's choice:** Astrageldon을 지정 (추천)
**Notes:** Research-phase must still confirm Astrageldon has a boss-level OnKill() with reproducible side effects before finalizing; if not, falls back to the richest-side-effect heuristic among whatever other CatalystMod bosses are discovered.

---

## Claude's Discretion

- Exact `weakReferences` version pin syntax for Redemption/CatalystMod in `build.txt`
- Exact naming of `Integrations/RedemptionIntegration.cs` / `Integrations/CatalystIntegration.cs`
- Whether either selected boss has a biome/Zone-dependent AI despawn requirement needing a `BossArenaRoutingRegistry` entry
- Player-scoped vs. world-scoped side-effect classification per boss
- Lambda-avoidance / per-method `[JITWhenModsEnabled]` tagging discipline (locked project-wide rule, not reconsidered)

## Deferred Ideas

- Registering the remaining 9 Redemption bosses and any other CatalystMod bosses beyond Astrageldon — out of scope, same as every prior mod-integration phase.
- Retroactive biome-classification sweep for whichever Phase 6/7 bosses turn out to be biome-dependent — Phase 9 only covered bosses that existed in the registry at the time (Calamity, Spirit); no currently-scoped phase owns this follow-up yet.
