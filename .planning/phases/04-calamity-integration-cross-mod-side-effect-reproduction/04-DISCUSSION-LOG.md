# Phase 4: Calamity Integration & Cross-Mod Side-Effect Reproduction - Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions are captured in CONTEXT.md — this log preserves the alternatives considered.

**Date:** 2026-08-13
**Phase:** 04-calamity-integration-cross-mod-side-effect-reproduction
**Areas discussed:** Cross-mod access strategy, Worked-example boss selection, Live verification approach, Scope (boss count)

---

## Cross-mod access strategy

| Option | Description | Selected |
|--------|-------------|----------|
| weakReferences + [JITWhenModsEnabled] | Official tModLoader-recommended pattern, already stated in CLAUDE.md. Requires strict per-method isolation of every Calamity-type-touching method + the attribute, or a JIT crash occurs when CalamityMod is disabled. | ✓ |
| Pure runtime reflection | `ModLoader.TryGetMod` + string type/member lookup. No compile-time reference to Calamity types at all, structurally avoids the JIT hazard. Code more verbose; reflection paths can break silently on Calamity updates. `research/PITFALLS.md`'s recommended safer default. | |
| Both (weakRef declaration + reflection internally) | Declare `weakReferences` in build.txt for tModLoader's official convention, but implement all actual member access via reflection anyway — belt-and-suspenders. | |

**User's choice:** weakReferences + [JITWhenModsEnabled] (official/recommended option)
**Notes:** User accepted the trade-off as explained — official pattern requires strict method-isolation discipline (no partial isolation) to avoid JIT crashes when CalamityMod is disabled.

---

## Worked-example boss selection

| Option | Description | Selected |
|--------|-------------|----------|
| Desert Scourge (early, low-risk) | Pre-hardmode early boss, no WorldGen effect — proves netcode/flag reproduction cleanly but not WorldGen (success criterion 3). | |
| A late-game boss the user actually fights | User names a specific boss relevant to their real FPS-relief use case (e.g. Devourer of Gods, Yharon, Supreme Calamitas). | |
| Determined during research-phase | Defer exact boss to research-phase, based on whichever is most representative/safe per DownedBossSystem structure. | |
| (Free text) Earliest WorldGen-triggering boss | User's own criterion: the earliest-in-progression Calamity boss that triggers a WorldGen side effect, so one boss proves both netcode AND WorldGen reproduction simultaneously. | ✓ |

**User's choice:** "가장 처음 WorldGen을 일으키는 보스" (the earliest boss that causes a WorldGen effect) — free-text answer, confirmed via reflect-back.
**Notes:** Exact boss name explicitly deferred to research-phase (user confirmed: "응, 리서치 단계에서 확정해줘" — "yes, confirm it during the research phase"). CalamityMod is installed locally (`Mods/2026.6CalamityMod.tmod`) for that research to inspect directly.

---

## Live verification approach

| Sub-topic | Option | Description | Selected |
|-----------|--------|-------------|----------|
| WorldGen test location | Fresh dedicated test world | Isolates permanent terrain-altering WorldGen effects from the player's real save. | ✓ |
| WorldGen test location | Backed-up main save (Phase 3 style) | Faster to set up, but WorldGen effects permanently alter terrain, requiring backup restoration to undo. | |
| Calamity-disabled safety test | Live in-game toggle checkpoint | Disable CalamityMod, launch, user confirms no JIT crash directly. | ✓ |
| Calamity-disabled safety test | Build/code review only | Static review that `[JITWhenModsEnabled]` boundaries are correct, no live toggle test. | |

**User's choice:** Fresh dedicated test world for WorldGen; live in-game toggle checkpoint for Calamity-disabled safety.
**Notes:** Both were the recommended options; user selected both without requesting elaboration.

---

## Scope (boss count)

| Option | Description | Selected |
|--------|-------------|----------|
| Single boss (1) | The one boss selected under "Worked-example boss selection" above proves both netcode and WorldGen reproduction. Mirrors Phase 3's single-worked-example discipline (King Slime). | ✓ |
| Two bosses (netcode-only + WorldGen-having) | Register a simple early boss (netcode/flag only) plus a separate WorldGen boss, proving the pattern generalizes across two different complexity tiers within this phase. | |

**User's choice:** Single boss (1)
**Notes:** Consistent with the boss-selection decision already covering both success criteria 2 and 3 with one boss.

---

## Claude's Discretion

- Exact Calamity boss name satisfying the "earliest WorldGen-triggering boss" criterion — resolved during research-phase.
- Exact shape/naming of the cross-mod access helper class/file (e.g. `Integrations/CalamityIntegration.cs`).
- Exact `weakReferences` version pin syntax in `build.txt`.
- Whether the dedicated WorldGen test world is discarded after use or kept for future reference.

## Deferred Ideas

- Registering additional Calamity bosses beyond the one worked example (out of this phase's scope).
- Using a late-game/flagship boss the user actually fights as the worked example (considered, not chosen — WorldGen-first criterion won instead).
