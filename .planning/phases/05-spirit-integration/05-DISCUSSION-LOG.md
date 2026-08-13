# Phase 5: Spirit Integration - Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions are captured in CONTEXT.md — this log preserves the alternatives considered.

**Date:** 2026-08-13
**Phase:** 05-spirit-integration
**Areas discussed:** SpiritMod installation/extraction blocker, Worked-example boss selection criterion, Live verification approach, Player-scoped vs. world-scoped classification, Scope (boss count)

---

## Pre-discussion blocker: SpiritMod not installed

Before gray-area discussion could start, codebase scouting found SpiritMod's `.tmod` absent from the `Mods/` folder (only CalamityMod + BossArenaSubWorld present), despite `ModConfigs/SpiritMod_SpiritClientConfig.json` evidence of prior use.

| Option | Description | Selected |
|--------|-------------|----------|
| Re-subscribe/extract via Steam Workshop now | Same rigor as Phase 4's live-decompile approach | ✓ |
| Proceed on PROJECT.md's existing (unverified) notes | Faster but accuracy risk | |

**User's choice:** Extract now — and requested the full list of mods needed for Phases 5-8 so they could extract everything at once.

**Follow-up:** User reported "전부 추출은 했는데" (extracted everything) but the `Mods/` folder still only showed CalamityMod. Investigation found the Steam Workshop content cache (`D:/SteamLibrary/steamapps/workshop/content/1281930/`) DID have all required mods subscribed/cached (SpiritMod, Redemption, CatalystMod, NoxusBoss, ContinentOfJourney, Daybreak, BossChecklist, InfernumMode) — they just hadn't synced into the actual `Mods/` folder tModLoader reads from yet.

**User's follow-up question:** Whether "Wrath of the Gods" mod bundles/includes NoxusBoss.

**Finding:** No — they are separate mods. NoxusBoss.tmod (workshop ID 2995193002) is already correctly cached as its own standalone mod. Only `WrathoftheGodsKR.tmod` (Korean localization patch) is subscribed — the base English "Wrath of the Gods" mod itself is NOT subscribed, and the KR patch is non-functional without it. Not relevant to Phase 5-8 registration work per PROJECT.md's existing note (Wrath reworks NoxusBoss's boss AI only, no separate downed flag).

**Resolution:** User then revealed `C:\Users\chang\Documents\My Games\Terraria\tModLoader\ModReader\` — a folder containing already-decompiled full source for every needed mod (SpiritMod, CalamityMod, CatalystMod, ContinentOfJourney, Daybreak, InfernumMode, NoxusBoss, Redemption, BossChecklist). This unblocked direct source reading without needing the `Mods/` folder sync or fresh decompilation at all.

---

## Worked-example boss selection criterion

Scouting `ModReader/SpiritMod/MyWorld.cs` and `NPCs/BossDownedTracker.cs` first revealed a significant correction: Spirit's actual downed-tracking API for real bosses is `BossDownedTracker.IsBossDowned<T>()` (a generic `Dictionary<string,bool>`-backed `GlobalNPC`), not the plain `MyWorld` static bools `PROJECT.md` described (those remain accurate only for non-boss events/minibosses).

Further scouting (grep for `OnKill` across all 8 tracked bosses' source files) found only Infernon has a boss-level `OnKill()` override with a real side effect (a small `TileID.HellstoneBrick` ring placed around its own death position) — the other 7 bosses have no such override.

| Option | Description | Selected |
|--------|-------------|----------|
| Infernon | Mirrors Phase 4's D-02 criterion (richest/earliest side-effect boss); only boss with any OnKill side effect | ✓ |
| Simplest boss (e.g. Scarabeus) | No WorldGen requirement in Phase 5's success criteria, so a plain-flag boss would also technically satisfy the phase | |

**User's choice:** Infernon (recommended option).

---

## Live verification approach

Since Infernon has a real (if cosmetic) WorldGen tile-mutation effect, mirrors Phase 4's D-04 dilemma.

| Option | Description | Selected |
|--------|-------------|----------|
| New throwaway test world | Same as Phase 4 D-04 — isolates permanent tile changes from the real save | ✓ |
| Reuse existing backed-up main save | Simpler but risks permanent (if minor) tile changes to the real world | |

**User's choice:** New throwaway world (recommended option, same as Phase 4).

---

## Player-scoped vs. world-scoped classification (Success Criterion 2)

Initial question (premised on an unverified double-grant risk analogous to Calamity's `SetNewBossJustDowned()`) was challenged by the user: "정확히 어떤 부분에서 2중지급이 일어나는거야?" (exactly where would double-granting happen?).

**Re-investigation:** Neither `BossDownedTracker.OnKill()` (pure world-scoped dictionary write + singleplayer-no-op netcode) nor `Infernon.OnKill()` (world-scoped `Main.tile` mutation) write anything to the player object. No player-scoped double-grant risk was found — this is the same category as Phase 3's King Slime (no player-scoped reward), not Phase 4's Hive Mind (which had a confirmed player-scoped side effect).

**Follow-up nuance surfaced:** Infernon's tile-ring is anchored to the boss's own `NPC.position`, which won't exist at `BossCoreItem`-use time in the main world — a position-anchoring question, not a double-grant question.

**User's response:** Confirmed this is inconsequential — the real ring already draws harmlessly inside the subworld's own throwaway platform during the actual kill (same category as Phase 4's "double Sky Ore message" cosmetic-only finding). Main-world replay should simply anchor on the player's current position; no special design effort warranted.

**Resolution:** Documented as an explicit "no player-scoped risk found" classification (satisfies Success Criterion 2's requirement to classify explicitly) + the position-anchoring detail noted as Claude's Discretion, low-stakes.

---

## Scope (boss count)

| Option | Description | Selected |
|--------|-------------|----------|
| One boss only (Infernon) | Same POC-first discipline as Phase 3 (King Slime) and Phase 4 (Hive Mind) | ✓ |
| Multiple Spirit bosses this phase | Would front-load work; conflicts with "no boss priority ordering" v1 principle's actual intent (uniform low marginal cost, not "do them all at once") | |

**User's choice:** One boss only (recommended option).

---

## Claude's Discretion

- Exact `weakReferences` version pin for SpiritMod in `build.txt`
- Exact naming of `Integrations/SpiritIntegration.cs`
- Infernon tile-ring replay position anchoring (player position at BossCoreItem-use time)
- Whether to replay Spirit's `NetMessage.SendData(MessageID.WorldData)` in the main-world apply path (default: yes, for fidelity, matching Phase 4's `CalamityNetcode.SyncWorld()` choice)

## Deferred Ideas

- Registering the remaining 7 Spirit bosses (Scarabeus, AncientFlyer, SteamRaiderHead, Atlas, MoonWizard, ReachBoss1, Dusking) — future work, near-zero marginal cost once pattern proven
- Wrath of the Gods base mod (not currently subscribed, only its KR patch is) — not a roadmap item, noted for user's own gameplay awareness only

