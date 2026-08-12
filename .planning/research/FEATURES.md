# Feature Research

**Domain:** tModLoader subworld-based boss arenas & cross-world (modded) boss-progress sync utilities
**Researched:** 2026-08-12
**Confidence:** MEDIUM-HIGH (core mechanics verified against current SubworldLibrary source on GitHub; ecosystem examples verified via multiple independent community sources; exact behavior of individual content mods' OnKill side effects not re-verified here, see PROJECT.md's own mod-specific research)

## Feature Landscape

### Table Stakes (Users Expect These)

Features players assume exist in any "subworld boss arena + progress carries back" mod. Missing these = the mod feels broken or unsafe to use.

| Feature | Why Expected | Complexity | Notes |
|---------|--------------|------------|-------|
| Manual entry into the subworld via item or NPC ("portal" pattern) | This is the dominant UX pattern across the ecosystem (Abyssal Subworld's Diving Leech, Multiverse Reloaded's Portal item, Arena Dimensions' teleport item). Players expect a consumable/reusable item, not a hidden trigger. | LOW | `SubworldSystem.Enter<T>()` does the heavy lifting. HIGH confidence — verified in SubworldLibrary source (`Subworld.cs`, `SubworldSystem.cs`, GitHub `jjohnsnaill/SubworldLibrary`, master branch). |
| Reliable, obvious return-to-main-world exit | Every reviewed example (Arena Dimensions: "reuse portal to return"; SubworldLibrary's own `ReturnDestination`/return button) treats exit as first-class, not an afterthought. Players fear being "stuck" in a subworld. | LOW | SubworldLibrary shows a return button by default; `Subworld.ReturnDestination` controls where exit sends the player. HIGH confidence (source-verified). |
| Player inventory/loot automatically follows the player out of the subworld | This is **not something the mod needs to build** — SubworldLibrary's default `NoPlayerSaving = false` means the player object (inventory, buffs, boss-drop treasure bags) is the *same* live object across the boundary. It only needs to be *not broken* by this mod. | LOW (already provided) | Verified in `Subworld.cs`: `NoPlayerSaving` defaults to `false`, meaning "changes persist." Table stakes risk is negative — accidentally setting this true, or building a custom arena subworld class that overrides it, would silently break loot carry-back. HIGH confidence. |
| Vanilla boss downed-flag sync between subworld and main world | As of recent SubworldLibrary versions, this is already solved natively for **vanilla** flags (`NPC.downedBoss1/2/3`, `downedMoonlord`, `downedPlantBoss`, DD2 event tiers, etc.) via internal `CopyDowned()`/`ReadCopiedDowned()` calls that run automatically on every subworld transition. Players expect vanilla progression to "just work" and will assume modded bosses behave the same. | LOW (already provided for vanilla only) | Verified directly in `SubworldSystem.cs` (GitHub master, functions `CopyDowned`/`ReadCopiedDowned`, both enumerating `NPC.downed*` fields and `DD2Event.Downed*`). Steam changelog (Jan 2025 entry, MEDIUM confidence — paraphrased via WebFetch, not directly quoted) states this was added specifically to fix "downed flags... not saving in Singleplayer." **This does NOT cover modded bosses** (Calamity, Spirit, etc.) — those use mod-private static fields/properties SubworldLibrary has no knowledge of. This is exactly the gap this project's `BossRegistry` fills, and confirms the project's premise is still valid for modded content even though the *vanilla* case is now handled upstream. |
| Modded boss downed-flag reproduction in the main world (per PROJECT.md core value) | This is the mod's entire reason to exist. Ecosystem precedent (Calamity Boss Resyncer) shows real player demand: players get frustrated when Boss Checklist / Lantern Night / other systems don't recognize a boss killed "elsewhere." | HIGH | Per-mod: Calamity wrapper properties + `CalamityNetcode.SyncWorld()` + `SetNewBossJustDowned()`; Spirit raw static fields; others unresearched. Complexity is inherent — no generic shortcut exists (confirmed by SubworldLibrary's own `ICopyWorldData`/`CopyWorldData` API requiring per-key, per-mod opt-in). |
| Faithful side-effect reproduction, not just a boolean flag | Community pitfall reports (Calamity Boss Resyncer's own Steam discussion) show naive flag-only syncing produces silently broken states — e.g., a boss shows "not downed" in Boss Checklist forever even after resyncing, because associated event/world state wasn't reproduced. | HIGH | Confirms PROJECT.md's own stated constraint. MEDIUM confidence (single discussion thread), but consistent with how SubworldLibrary's docs frame `CopyWorldData`/`ReadCopiedWorldData` as raw key-value only — no built-in side-effect replay for any mod's custom logic. |
| Arena isolation guarantee (subworld never has mod content placed in it) | This is the actual value proposition for FPS — if any reviewed content mod's global worldgen/tile hooks run in the subworld, the performance goal is defeated. Precedent: Abyssal Subworld and HomewardSubworld both carve out a clean, purpose-built subworld rather than reusing overworld generation. | MEDIUM | Achieved via `Subworld.Tasks` (custom `GenPass` list) instead of vanilla/modded world generation, likely combined with StructureHelper for hand-built arena geometry (per PROJECT.md constraints). Risk: some mods hook `GlobalNPC`/`ModSystem.PostWorldGen` unconditionally regardless of which `GenPass` list ran — must verify empirically. |
| Safe death handling in the subworld | Default SubworldLibrary behavior returns the player to the main world on death (respawn location is main-world-based unless a mod customizes `SpawnCondition`/respawn hooks). Community reports (Steam Workshop discussion, MEDIUM confidence) show at least one feature *request* for the opposite (respawn where you died) — implying default behavior is "you go home," which matches player mental model of "the arena is a side trip." | LOW (default behavior, mod should not need to change it) | Edge case for THIS mod: does the boss-kill / carrier-item drop survive if the player dies immediately after the kill (before picking up the drop)? Should be tested — item stays on the ground in the subworld and needs to be collected before exiting, or it's lost with the (non-saved) subworld. |
| A documented "back up your world first" safety expectation | The ecosystem is honest about this being risky territory: users of Calamity Boss Resyncer are explicitly advised to back up saves, and SubworldLibrary's own history includes real data-loss-adjacent bugs (GitHub issue #12, `ReadCopiedDowned()` crash; issue #49, server hangs on subworld transition). Players who've been burned by "world sync" mods before will look for this reassurance. | LOW (documentation, not code) | Already reflected in PROJECT.md's plan ("world backup before testing"). Worth surfacing as an explicit, user-facing recommendation (e.g., mod description text), not just an internal dev practice. |

### Differentiators (Competitive Advantage)

Features that set this mod apart from the two nearest ecosystem analogues: (1) single-purpose patch mods like Calamity Boss Resyncer, and (2) SubworldLibrary's own built-in vanilla-only sync.

| Feature | Value Proposition | Complexity | Notes |
|---------|--------------------|------------|-------|
| Generic, multi-mod `BossRegistry` (key → apply-function) architecture | Every existing "fix the resync bug" mod found in research (Calamity Boss Resyncer) is single-mod, single-purpose, and had to be independently rebuilt per content mod. A registry pattern covering Calamity, Spirit, Redemption, CatalystMod, NoxusBoss, ContinentOfJourney/Daybreak in one mod is not something any reviewed competitor does. | MEDIUM (skeleton) + HIGH (per-mod entries) | Directly matches PROJECT.md's stated core value. This is the mod's real differentiator — breadth of coverage, not novelty of mechanism. |
| Explicit, player-controlled application via carrier item (vs. silent automatic resync) | Automatic "resync on every subworld transition" (the pattern SubworldLibrary itself uses for vanilla flags, and that Calamity Boss Resyncer tries to imitate for Calamity) is opaque — players can't tell when/why a flag changed, and can't retry safely if partway broken. A physical carrier item makes the sync event visible, inspectable, and re-triggerable (re-use the item if the first application silently no-ops). | LOW-MEDIUM | This also sidesteps a subtle risk: automatic resync on *every* transition risks re-firing "boss just downed" netcode messages repeatedly if not carefully deduplicated (a real category of bug reported in the Calamity Boss Resyncer Steam discussion). Explicit one-shot item use is easier to make idempotent. |
| Faithful WorldGen side-effect reproduction (not just flags) | Reviewed competitor (Calamity Boss Resyncer) only fixes the downed-boolean; PROJECT.md explicitly requires reproducing ore-gen/dungeon-activation-class WorldGen effects for world-altering bosses (mechanical bosses, Plantera). No reviewed ecosystem mod does this generically. | HIGH | Highest-risk, highest-value differentiator. Needs careful sequencing (WorldGen calls have ordering/state assumptions) and should be validated per-boss. |
| Uniform, low-marginal-cost boss registration | Once the `BossRegistry`/`BossCoreItem`/`GlobalNPC` skeleton exists, adding a new boss is a bounded, well-understood unit of work (find the mod's downed-state API, wire `Apply(key)`), not a bespoke integration. This lets the project realistically cover 6+ large content mods where competitors cover exactly 1. | LOW (once skeleton exists) | Matches PROJECT.md's own "no boss priority ordering in v1" decision rationale — confirmed sound by this research; no evidence in the ecosystem that "worst offender first" ordering has value once the skeleton exists. |

### Anti-Features (Commonly Requested, Often Problematic)

| Feature | Why Requested | Why Problematic | Alternative |
|---------|----------------|------------------|-------------|
| Multiplayer / dedicated-server sync | SubworldLibrary itself advertises "works in Multiplayer with little to no extra work," so it looks free; players will ask for it. | Subserver architecture means the carrier item, and the boss-just-downed netcode calls it triggers, must be correctly synced across the main server and the subserver — real risk of duplicate/out-of-order flag application or desync given the mod is already working around an *existing* SubworldLibrary sync bug. Community reports of subserver-specific issues exist (GitHub issue #49: "Server hangs after moving to other world... in multiplayer"). | Ship singleplayer-only for v1 (already PROJECT.md's decision — confirmed reasonable by this research); revisit only after the core carrier-item pipeline is proven reliable. |
| Automatic/implicit subworld entry (auto-detect "about to fight a boss") | Feels seamless — no need to remember to use an item before a big fight. | Detecting "an imminent lag-heavy boss fight" reliably across many different content mods' summon patterns is itself a hard, mod-specific problem (the exact kind of per-mod research burden this project is trying to minimize for the *sync* side). Also removes player control/predictability — a stated design value already reflected in PROJECT.md. | Manual item/NPC entry (already chosen). Keep it simple and explicit. |
| Silent, automatic "resync everything on every world transition" (the pattern used for vanilla flags, and attempted by Calamity Boss Resyncer for Calamity) | Looks like the "proper" fix, since it's how SubworldLibrary itself handles vanilla flags — no player action required. | For modded bosses this means re-running each mod's full `OnKill`-equivalent side effects (chat messages, netcode syncs, WorldGen effects) on *every* subworld exit, not just once — high risk of duplicate messages/events, and much harder to debug when it silently fails for one boss (exactly the unresolved bug reported against Calamity Boss Resyncer, where a boss stayed permanently unmarked in Boss Checklist despite repeated "fixes"). | Explicit, idempotent, player-triggered carrier item (already chosen) — apply once, on demand, easy to retry and easy to reason about. |
| Boss-priority ordering / phased mod rollout (do Moon Lord/"worst offenders" first) | Feels like sensible risk management — validate the riskiest case first. | Once the registry skeleton exists, marginal cost per boss is uniform (see Differentiators); phased rollout by "severity" adds planning overhead without corresponding benefit, and this project's own PROJECT.md already reasoned through and rejected this. | Register bosses in whatever order the per-mod research naturally completes; no need to special-case. |
| Full arena-building/decoration toolkit (auto-platforms, campfires, honey, aesthetic customization) | Real player demand exists in the ecosystem for this (Steam discussion recommending Luiafk's "Arena platform builder" for exactly this use case) — looks like a nice complementary feature for "boss arena" framing. | Out of scope for the stated core value (progress-sync pipeline). Building/maintaining arena-decoration tooling is a different product with its own complexity, and duplicates existing, popular, well-maintained mods (Luiafk). | Keep the subworld's arena geometry minimal/purpose-built (StructureHelper, per PROJECT.md constraints) and explicitly do not compete with dedicated arena-builder mods; players who want full decoration can layer Luiafk on top if it doesn't conflict with the isolation guarantee. |
| Generic full player-state mirroring (health/buffs/position instantly mirrored between worlds) | Sounds like it would make the subworld "feel" seamless/integrated with the main world. | Unnecessary — SubworldLibrary already carries the live player object across the boundary (see Table Stakes), so real-time mirroring of a *second* copy of player state is redundant complexity solving a problem that doesn't exist for this mod's manual-entry, single-player-at-a-time model. | Rely on SubworldLibrary's existing player-object continuity; do not build a parallel state-sync layer. |

## Feature Dependencies

```
[Subworld isolation guarantee (empty arena, custom GenPass)]
    └──requires──> [SubworldLibrary dependency + subworld entry/exit]

[BossCoreItem drop on kill]
    └──requires──> [Subworld entry/exit working]
    └──requires──> [GlobalNPC.OnKill boss detection]

[BossRegistry.Apply(key) — flag reproduction]
    └──requires──> [Per-mod API research completed (Calamity, Spirit, Redemption, CatalystMod, NoxusBoss, ContinentOfJourney/Daybreak)]
    └──requires──> [BossCoreItem exists and carries a boss key]

[Netcode side-effect replication (e.g. CalamityNetcode.SyncWorld(), SetNewBossJustDowned())]
    └──requires──> [BossRegistry.Apply(key) exists]
    └──enhances──> [3rd-party tracker compatibility (Boss Checklist, etc.)]

[WorldGen side-effect replication (ore gen, dungeon activation)]
    └──requires──> [BossRegistry.Apply(key) exists]
    └──conflicts──> [Silent/automatic resync-on-every-transition pattern]
        (WorldGen side effects must fire exactly once; automatic re-sync on
         every transition risks re-triggering them repeatedly)

[Vanilla downed-flag sync]
    └──already provided by SubworldLibrary──> [no dependency, but must not
        double-apply for modded bosses that also flip an adjacent vanilla flag]

[Multiplayer/dedicated-server support] (deferred)
    └──requires──> [Singleplayer carrier-item pipeline proven reliable first]
```

### Dependency Notes

- **BossRegistry.Apply(key) requires per-mod API research:** Confirmed by SubworldLibrary's own design — its `ICopyWorldData`/`CopyWorldData` API is explicitly key-value and per-caller-opt-in with no generic reflection-based shortcut, reinforcing PROJECT.md's stated constraint that "no generic shortcut" exists across mods with different downed-state APIs (wrapper properties vs. raw static fields).
- **WorldGen side-effect replication conflicts with automatic/repeated resync:** WorldGen calls (ore placement, dungeon activation) are almost certainly not idempotent-safe to call more than once. This is a concrete reason the explicit, one-shot carrier-item pattern (already PROJECT.md's chosen design) is architecturally safer than an automatic "resync on every subworld transition" pattern, beyond just matching player expectations.
- **Vanilla downed-flag sync already provided:** Because SubworldLibrary now handles vanilla flags natively, the BossRegistry only needs to own **modded** flags. Double-check registration doesn't fight with SubworldLibrary's automatic vanilla sync for bosses that are "vanilla underneath" (e.g., Infernum reworks vanilla/Calamity bosses' AI but not their downed-flag storage, per PROJECT.md's own notes) — in those cases, no BossRegistry entry may be needed at all, since the vanilla path already covers it.
- **3rd-party tracker compatibility (Boss Checklist) is enhanced, not required, by faithful side-effect reproduction:** Boss Checklist (and similar) read the same underlying flags this mod is applying — getting the flag application right is sufficient; no explicit Boss Checklist integration code is needed.

## MVP Definition

### Launch With (v1)

Minimum viable product — what's needed to validate the core value from PROJECT.md.

- [ ] Manual subworld entry via item/NPC into a guaranteed-empty arena — essential to the FPS-avoidance premise
- [ ] `BossCoreItem` drop on registered-boss kill (GlobalNPC.OnKill) — essential trigger for the whole pipeline
- [ ] `BossRegistry.Apply(key)` on item use in main world: flag + netcode-sync + WorldGen-side-effect reproduction — this IS the core value; nothing else matters if this doesn't work end-to-end
- [ ] Calamity and Spirit bosses registered (the two mods already researched, per PROJECT.md) — proves the pattern across two structurally different APIs (wrapper properties vs. raw static fields)
- [ ] Singleplayer only, manual entry only — matches PROJECT.md's explicit v1 scope decisions
- [ ] End-to-end verification in singleplayer with a world backup taken beforehand — non-negotiable given the ecosystem's track record of subtle sync bugs (SubworldLibrary GitHub issues #12, #49; Calamity Boss Resyncer's unresolved reports)

### Add After Validation (v1.x)

- [ ] Redemption, CatalystMod, NoxusBoss, ContinentOfJourney/Daybreak boss registration — trigger: v1 pipeline proven reliable for the two already-researched mods
- [ ] Explicit compatibility pass against Boss Checklist and any other flag-reading trackers the user relies on — trigger: any report of a tracker not picking up an applied flag (mirrors the exact unresolved Calamity Boss Resyncer bug found in research)
- [ ] Handling for bosses whose "downed" state depends on a multi-boss combo (e.g., paired/simultaneous encounters) — trigger: encountered while researching Redemption/CatalystMod/NoxusBoss, since Infernum-style paired fights (Bereft Vassal + Great Sand Shark) were found to be a real-world source of tracking bugs

### Future Consideration (v2+)

- [ ] Multiplayer / dedicated-server support — defer until singleplayer carrier-item pipeline is proven; subserver-specific sync bugs are a known, real risk category in this ecosystem
- [ ] Automatic subworld entry detection — defer indefinitely per PROJECT.md; adds detection complexity disproportionate to value
- [ ] In-subworld boss selection/summon UI for multiple bosses per visit — nice-to-have UX polish, not needed to validate the core sync mechanism

## Feature Prioritization Matrix

| Feature | User Value | Implementation Cost | Priority |
|---------|------------|----------------------|----------|
| Subworld entry/exit (empty arena) | HIGH | LOW | P1 |
| BossCoreItem drop on kill | HIGH | LOW | P1 |
| BossRegistry.Apply — flag reproduction | HIGH | HIGH | P1 |
| Netcode side-effect replication | HIGH | MEDIUM | P1 |
| WorldGen side-effect replication | HIGH | HIGH | P1 |
| Player-safety / world-backup guidance | MEDIUM | LOW | P1 |
| Coverage of remaining mods (Redemption, CatalystMod, NoxusBoss, ContinentOfJourney/Daybreak) | HIGH | HIGH (per mod) | P2 |
| Multi-boss/combo-encounter handling | MEDIUM | MEDIUM | P2 |
| Boss Checklist / tracker compatibility verification | MEDIUM | LOW | P2 |
| Multiplayer/dedicated-server support | LOW (for this user's stated use case) | HIGH | P3 |
| Automatic subworld entry detection | LOW | HIGH | P3 |
| Arena decoration/build tooling | LOW | MEDIUM | P3 (explicitly not planned) |

**Priority key:**
- P1: Must have for launch
- P2: Should have, add when possible
- P3: Nice to have / explicitly deferred or rejected

## Competitor Feature Analysis

| Feature | Calamity Boss Resyncer | SubworldLibrary (built-in) | This mod's approach |
|---------|--------------------------|------------------------------|------------------------|
| Scope | Calamity only | Vanilla `NPC.downed*` + DD2 event flags only | Multi-mod: Calamity, Spirit, Redemption, CatalystMod, NoxusBoss, ContinentOfJourney/Daybreak |
| Sync trigger | Automatic, on subworld transition (copies its own internal `DownedBossSystem` shadow) | Automatic, on every subworld transition (`CopyDowned`/`ReadCopiedDowned`) | Explicit, player-triggered via `BossCoreItem` use |
| Side effects beyond the boolean flag | Not reproduced (per its own Steam discussion, bosses can remain permanently unmarked in Boss Checklist despite "fixing") | N/A — vanilla-only, uses vanilla's own flag semantics | Explicitly reproduces netcode sync calls and WorldGen effects (mechanical bosses, Plantera-class effects) |
| Known reliability | Unresolved bug reports for paired/Infernum-mode bosses (Bereft Vassal, Great Sand Shark); mod later removed from Steam for guideline violations | Reliable for vanilla flags as of the library's Jan 2025-era fix (per changelog) | Unproven — this is exactly what PROJECT.md's "verify end-to-end" requirement targets |
| Multiplayer | Unclear/unverified | Yes, subserver-based | Explicitly out of scope for v1 |

## Sources

- SubworldLibrary source, GitHub `jjohnsnaill/SubworldLibrary` (master branch): `Subworld.cs` (`ICopyWorldData`, `CopyMainWorldData`/`ReadCopiedMainWorldData`/`CopySubworldData`/`ReadCopiedSubworldData`, `NoPlayerSaving`, `ReturnDestination`) and `SubworldSystem.cs` (`CopyDowned`/`ReadCopiedDowned`, `CopyWorldData`/`ReadCopiedWorldData<T>`) — HIGH confidence, read directly from raw GitHub source, 2026-08-12.
- SubworldLibrary GitHub issues: `#12` "Invalid operation at this state" (`ReadCopiedDowned()` stack trace), `#49` "Server hangs after moving to other world... in multiplayer" — MEDIUM confidence (titles/summaries only, via search).
- SubworldLibrary Steam Workshop changelog (`steamcommunity.com/sharedfiles/filedetails/changelog/2785100219`) — MEDIUM confidence, paraphrased via WebFetch; states downed-flag/bestiary caching fix for singleplayer subworld transitions.
- Calamity Boss Resyncer, Steam Workshop `id=3417899539` and its discussion thread — MEDIUM confidence; describes the mod's purpose (copy `DownedBossSystem` into its own shadow system) and an unresolved bug where Infernum-mode paired bosses (Bereft Vassal, Great Sand Shark) remain unmarked despite repeated fixes.
- Boss Checklist, GitHub `JavidPack/BossChecklist` and Steam Workshop `id=2669644269` — HIGH confidence for scale/popularity claim (listed as 3rd most-subscribed tModLoader mod, 3M+ subscribers per search result), MEDIUM confidence for full feature list (Boss Log, records, radar, loot checklist).
- Abyssal Subworld (Homeward Journey ecosystem), Steam Workshop `id=3554145193` — MEDIUM confidence; portal-item entry pattern (Diving Leech).
- Arena Dimensions, Steam Workshop `id=2988966458` — MEDIUM confidence; reusable single-item portal-in/portal-out pattern.
- Multiverse Reloaded, Steam Workshop `id=3114599426` — LOW-MEDIUM confidence; craftable Portal item / command-based subworld travel, multi-subworld hub pattern.
- Steam Community, tModLoader general discussion re: "insta arena item" — LOW confidence (single thread); indicates real demand for Luiafk's arena-platform-builder as a *separate* concern from progress-sync mods.
- AnswerOverflow snippet title, "Entering Subworlds in Infernum resets progress in the main..." — LOW confidence (title only, page inaccessible), but directly corroborates PROJECT.md's stated known blocker from an independent source.

---
*Feature research for: tModLoader subworld-based boss arena + cross-world modded-boss-progress-sync mods*
*Researched: 2026-08-12*
