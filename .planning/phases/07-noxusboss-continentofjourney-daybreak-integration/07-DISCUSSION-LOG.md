# Phase 7: ContinentOfJourney/Daybreak (Homeward Journey) Integration - Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions are captured in CONTEXT.md — this log preserves the alternatives considered.

**Date:** 2026-08-14
**Phase:** 07-noxusboss-continentofjourney-daybreak-integration
**Areas discussed:** Mod identity (ContinentOfJourney/Daybreak), NoxusBoss scope, Homeward Journey boss selection, NoxusBoss disposition (backlog vs. removal), biome/arena routing principle

---

## Mod Identity — ContinentOfJourney/Daybreak

| Option | Description | Selected |
|--------|-------------|----------|
| Homeward Journey (GabeHasWon), guessed | Matches `GabeHasWon/HomewardSubworld` reference already noted in STACK.md; "(Homeward series)" parenthetical in ROADMAP.md Phase 7 title as the actual pointer | ✓ (initial guess) |
| A different specific mod | User supplies exact Workshop ID/author name | |
| Excluded from this phase | Skip identification, register NoxusBoss only | |

**User's choice:** Selected "Homeward Journey (guessed)" as the working hypothesis, then independently supplied Steam Workshop link `https://steamcommunity.com/sharedfiles/filedetails/?id=2930931197` for verification.

**Verification:** WebSearch on the exact Workshop id confirmed it resolves to "Homeward Journey" — guess confirmed correct.

**Notes:** User separately confirmed the Daybreak research finding independently ("Daybreak는 신들의분노 모드의 의존성모드였으니까 아무것도 없는게 맞고") — Daybreak is a boss-less library dependency of Wrath of the Gods, matching the Phase 9-era research finding (`gold-meridian/daybreak-mod`).

---

## Homeward Journey Boss Selection

| Option | Description | Selected |
|--------|-------------|----------|
| Claude's Discretion during research | Pick lowest-API-research-risk boss (no Master-Mode alt-AI, no structure-gating), document rationale in code | ✓ |
| Wall of Shadow (post-Moon Lord first boss) | Represents mod's core progression gate | |
| Direct user pick (The Son, The Lifebringer, etc.) | User names a specific boss | |

**User's choice:** "리서치 재량으로 Claude가 결정" (Claude's Discretion, research picks lowest-risk boss).

---

## NoxusBoss Access Method → Superseded by Scope Removal

| Option | Description | Selected |
|--------|-------------|----------|
| Extract via `scripts/extract_tmod.py` from Workshop cache (Phase 6 CatalystMod precedent) | No local install needed | |
| User subscribes/enables locally before research | Direct Mods/ folder access | |

**User's response (free text):** "이전에 녹서스모드의 보스들은 이번에 대상에서 제외했잖아" ("Didn't we already exclude NoxusBoss's bosses from this round?") — did not select either option; instead flagged that NoxusBoss should not be in scope at all.

**Follow-up clarification asked:** Whether this referred to Phase 10 CONTEXT.md's note that NoxusBoss is "Phase 7's unstarted scope, not [Phase 10's]" (meaning Phase 7 still owns it), or a request to remove NoxusBoss from Phase 7 itself.

**User's answer:** "아니오 — NoxusBoss를 Phase 7에서도 제외하고 싶어" (No — I want NoxusBoss excluded from Phase 7 too).

**Follow-up: where should NoxusBoss go?**

| Option | Description | Selected |
|--------|-------------|----------|
| Backlog (999.x parking lot) | Revisit later if reconsidered | |
| Fully removed (Out of Scope) | No plan to revisit | ✓ (via free text) |

**User's answer (free text):** "애초에 대부분 퀘스트형 보스거나 자신만의 서브월드를 가졌기 때문에 계획없음" (Most of them are quest-type bosses or already have their own subworld, so there's no plan [for them]) — interpreted as full removal, not backlog, given "계획없음" (no plan).

**Resulting action (this session, not deferred to planner):** Updated `ROADMAP.md` (Phase 7 title/Goal/Success Criteria/Requirements), `REQUIREMENTS.md` (MOD-05 marked Removed, Out of Scope table, Traceability row, coverage counts), `PROJECT.md` (moved requirement to Out of Scope, added two Key Decisions rows, updated Context/mod-research-status prose), and `STATE.md` (Roadmap Evolution entry, Blockers/Concerns update).

---

## Biome/Arena Routing Principle

| Option | Description | Selected |
|--------|-------------|----------|
| Default `BossArenaSubworld` only (plain arena), route to Phase 9 biome subworld only if research confirms functional Zone dependency | Lower risk since Homeward Journey's actual Zone dependencies are unresearched | |
| Apply Phase 9/10 wiki-thematic-assignment principle from the start | Route to wiki-stated biome regardless of confirmed functional dependency, consistent with D-01 (Phase 9 CONTEXT.md) and D-01 (Phase 10 CONTEXT.md) | ✓ |

**User's choice:** "처음부터 Phase 9 위키-테마형 배치 원칙 적용" (Apply Phase 9's wiki-thematic placement principle from the start).

---

## Claude's Discretion

- Exact Homeward Journey boss pick within its 15-boss roster (tiebreaker: lowest research risk)
- Whether the chosen boss needs a new subworld (e.g. a new "Abyss" biome) or reuses one of the 7 existing Phase 9 subworlds
- New integration file naming/placement (`Integrations/HomewardJourneyIntegration.cs`, following existing per-mod convention)
- Per-boss decompiled-source verification of the actual downed-progress API shape

## Deferred Ideas

- Homeward Journey full-roster expansion beyond the single Phase 7 boss — no phase scheduled, analogous to a future "Phase 10-style" expansion if ever requested
- New Homeward Journey "Abyss" biome subworld, if the selected boss requires it — sized during this phase's planning if needed, not a resurrection of any prior discarded Phase 9 code (no prior Abyss subworld ever existed)

## Removed From Scope (not deferred)

- NoxusBoss (Devourer of Universes and its other bosses) — permanently removed from v1, not backlogged. See CONTEXT.md D-03 and `PROJECT.md` Out of Scope for full rationale.
