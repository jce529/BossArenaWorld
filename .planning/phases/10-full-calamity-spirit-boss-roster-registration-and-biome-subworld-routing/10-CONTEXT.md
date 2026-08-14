# Phase 10: Full Calamity/Spirit Boss Roster Registration & Biome Subworld Routing - Context

**Gathered:** 2026-08-14
**Status:** Ready for planning

<domain>
## Phase Boundary

Registers the remaining Calamity and Spirit bosses already biome-classified in `09-ALTAR-BIOME-REFERENCE.md` into `BossRegistry`/`SummonItemRegistry`, and wires each biome-dependent (or wiki-thematically-assigned) boss to its matching `BossArenaRoutingRegistry` subworld built in Phase 9. Only Hive Mind (Calamity) and Infernon (Spirit) are currently registered — this phase expands to the full researched roster for these two mods only. Redemption, CatalystMod, NoxusBoss, and ContinentOfJourney/Daybreak roster expansion remain out of scope (Redemption/CatalystMod already have their one worked-example boss each from Phase 6; NoxusBoss/ContinentOfJourney/Daybreak are Phase 7's unstarted scope, not this phase's).

</domain>

<decisions>
## Implementation Decisions

### Biome assignment principle — carried forward from Phase 9, reconfirmed
- **D-01:** Keep Phase 9's "wiki-thematic assignment" principle: every boss gets its wiki-stated biome arena even when the boss's own AI has no `Zone*`/`CheckActive` dependency on that biome (e.g. Infernon already precedents this). User explicitly reconfirmed this after being shown that this project's `SummonItemRegistry` pipeline bypasses the source item's real `CanUseItem()`/`UseItem()` entirely (calls `NPC.SpawnOnPlayer` directly), so most wiki-stated biome/time requirements have zero functional effect on our redirect flow — the value is purely thematic/cosmetic arena-matching, not despawn-prevention. Despawn-prevention-necessary routing (Hive Mind/Corruption precedent) and thematic-only routing are NOT distinguished in implementation — both use the same `BossArenaRoutingRegistry.Register<T>()` call, per-boss research must still explicitly document which reason applies (functional vs. thematic) in code comments, mirroring the Phase 6 Thorn/Astrageldon discipline of documenting the Zone-check research finding either way.

### Infernum mod-combination gating — full implementation, not deferred
- **D-02:** Implement the full conditional-registration matrix from `09-ALTAR-BIOME-REFERENCE.md` Section 1, not just the Calamity-only baseline:
  - Providence, Profaned Guardians, Ceaseless Void: register ONLY when `ModLoader.HasMod("CalamityMod") && !ModLoader.HasMod("InfernumMode")`. When Infernum is present, do NOT register these three at all (their summon items must fall through untouched to Infernum's own structure-gated `CanUseItem`/`UseItem` — registering anyway would produce a silent soft-lock, per the reference doc's explicit warning).
  - The Old Duke: register (Bloodworm Platter → Sulphurous Sea, or a plain arena if research resolves Open Item 3 as item-gate-only — see Claude's Discretion) ONLY when `ModLoader.HasMod("CalamityMod") && ModLoader.HasMod("InfernumMode")`. Without Infernum, no summon item exists to hook — nothing to register.
  - Astrum Deus / Astrum Aureus: register the Astral altar unconditionally, but when `ModLoader.HasMod("InfernumMode")` is also true, additionally force night in that subworld before/during the summon (same forced-night mechanism as D-04's day/night utility, applied conditionally here).
  - Providence / Profaned Guardians are valid under either Hallow or Underworld per vanilla Calamity — pick one consistently (Claude's Discretion, document the choice) since this project's registries map one NPC type to one subworld type.

### Roster scope — full researched list, one phase
- **D-03:** Register the complete `09-ALTAR-BIOME-REFERENCE.md` Section 3 roster in this phase (not a reduced/prioritized subset), split across multiple plans/waves as needed for execution pacing. Confirmed list (excluding Hive Mind and Infernon, already registered):
  - **Calamity:** Providence, Profaned Guardians, Ceaseless Void (Infernum-gated per D-02), The Old Duke (Infernum-gated per D-02), Signus, Storm Weaver, Astrum Deus, Astrum Aureus (both Infernum-conditional night per D-02), Dragonfolly, Devourer of Gods, Exo Mechs, Yharon, Supreme Witch Calamitas (plain arena — Calamity's own "Altar of the Accursed" furniture just needs to be placeable in the default `BossArenaSubworld`, no new biome subworld).
  - **Spirit:** Ancient Avian, Starplate Voyager, Scarabeus, Vinewrath Bane, Moon Jelly Wizard, Dusking (both night-gated per D-04), Atlas (plain arena).
- This matches `PROJECT.md`'s "no boss priority ordering" principle — marginal registration cost is uniform once the pattern is proven (Phase 3-6 precedent), so there is no reason to special-case a subset now that the full researched list already exists.

### Time-gated bosses — included, new forced day/night utility required
- **D-04:** Include Moon Jelly Wizard and Dusking (Spirit, both night-gated per `09-ALTAR-BIOME-REFERENCE.md` Section 4) in this phase's scope, and build the forced-night utility mechanism Phase 9 explicitly deferred (09-CONTEXT.md D-05). This utility is also reused for Astrum Deus/Astrum Aureus's Infernum-conditional night requirement (D-02).
  - Redemption's Section 4 time-gated bosses (The Keeper, Omega Cleaver, Omega Gigapora, Nebuleus, Fowl Emperor, King Slayer III) are OUT of scope — Redemption roster expansion is not this phase's mod scope (see Phase Boundary).
  - Mechanism is Claude's Discretion (research/planning decides exact implementation — e.g. setting `Main.time`/`Main.dayTime` on subworld `OnEnter()`, mirroring how biome tiles are auto-placed via `GenPass` with no player-facing choice). No new player-facing UI/item — this is an automatic subworld-setup step, consistent with the project's existing "zero new player action beyond the existing portal tile" design (PROJECT.md Out of Scope: "Automatic subworld entry" is excluded, but this is about subworld *setup*, not entry *triggering*, so it doesn't conflict).

### Claude's Discretion
- Exact per-boss `OnKill()` decompiled-source verification (side effects, player-scoped vs. world-scoped classification, actual `Zone*`/`CheckActive` dependency) — same discipline as every prior mod-integration phase (Phase 3-6), performed during research, not decided here.
- Open Item 3 from `09-ALTAR-BIOME-REFERENCE.md`: whether The Old Duke's Sulphurous Sea requirement is an item-level `CanUseItem()` gate (irrelevant to this project's pipeline, could skip the Sulphurous Sea subworld) or an AI-level `Zone*` dependency (requires it, like Hive Mind/Corruption) — needs a decompiled-source check before finalizing whether `BossArenaSulphurousSubworld`/`SulphurousPlatformPass` need to be rebuilt (D-07 in 09-CONTEXT.md discarded the Wave-1 build; rebuilding is new work if AI-level dependency is confirmed).
- Exact forced day/night utility mechanism (D-04) — e.g. subworld `OnEnter()` time-set vs. a dedicated `ModSystem` hook, whichever fits the existing `BossArenaXSubworld` template most cleanly.
- Whether new integration code extends the existing `Integrations/CalamityIntegration.cs`/`Integrations/SpiritIntegration.cs` files (one file per mod, growing) or splits into multiple files per mod for readability given the roster size — follow whichever keeps `[JITWhenModsEnabled]`-per-method discipline clean; existing single-file-per-mod convention is the default unless file size becomes unwieldy.
- Providence/Profaned Guardians' Hallow-vs-Underworld altar choice (see D-02) — pick one, document the choice in code comments.

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Requirements this phase implements
- `.planning/REQUIREMENTS.md` §"Arena Biome Coverage (ARENA)" — ARENA-01 (boss-classification-and-routing half, explicitly left outstanding after Phase 9)
- `.planning/ROADMAP.md` §"Phase 10" — Goal/Success Criteria (TBD, not yet written — this phase was added via `/gsd:add-phase`, not the original roadmap)

### Primary research input — MANDATORY, full read required
- `.planning/phases/09-biome-dependent-subworld-coverage/09-ALTAR-BIOME-REFERENCE.md` — the complete per-boss biome/Infernum-combination/time-gating research this entire phase is built on. Section 1 (Infernum matrix), Section 3 (final biome/altar/boss table), Section 4 (time-gated bosses), Section 5 (open items, especially Item 3 re: The Old Duke).

### Locked patterns from Phase 3-6 (extend unchanged into this phase)
- `.planning/PROJECT.md` §Key Decisions — the Phase 4 lambda/`[JITWhenModsEnabled]` JIT-crash lesson (commit `0e19600`); the Phase 9 lesson that lazy construction inside a containing class is NOT sufficient JIT protection (every method touching a weak-referenced mod's types needs its own `[JITWhenModsEnabled]` tag)
- `.planning/phases/06-redemption-catalystmod-integration/06-CONTEXT.md` — most recent precedent for per-boss Zone-dependency verification discipline (Thorn/Astrageldon both confirmed via full decompiled-source read, not assumption) and the `SummonItemRegistry` eligibility-delegate extension pattern (used for Astrageldon's Moon-Lord-lockout — may be relevant if any Phase 10 boss has a similar item-breaking precondition)
- `Integrations/CalamityIntegration.cs`, `Integrations/SpiritIntegration.cs` — direct templates: `PostSetupContent()` with `ModLoader.HasMod("<ModName>")` guard, `[JITWhenModsEnabled("<ModName>")]`-tagged methods, named (never lambda) delegates
- `Systems/BossArenaRoutingRegistry.cs` + the 7 biome-variant subworlds (Phase 9: `BossArenaHallowSubworld`, `BossArenaUnderworldSubworld`, `BossArenaAstralSubworld`, `BossArenaJungleSubworld`, `BossArenaSpaceSubworld`, `BossArenaDesertSubworld`, `BossArenaBriarSubworld`) — the exact subworlds this phase's `Register<T>()` calls target. Dungeon and Sulphurous Sea variants do NOT currently exist (discarded per D-07) — Ceaseless Void (Infernum-off case, Dungeon) and Polterghast (Spirit, Dungeon, not in this phase's Spirit list but worth noting) and The Old Duke (Sulphurous Sea, pending Open Item 3) may require rebuilding one or both.
- `.planning/phases/09-biome-dependent-subworld-coverage/09-CONTEXT.md` — D-05 (forced day/night explicitly deferred, now picked up by this phase's D-04), D-07 (Dungeon/Sulphurous Sea discard rationale — do not silently resurrect discarded code, any rebuild is new work)
- `.planning/research/PITFALLS.md` §"Pitfall 5" (player-scoped vs. world-scoped double-grants), §"Pitfall 2/4" (JIT isolation, reflective lookup fragility)

### World-safety / process references
- `docs/WORLD_BACKUP_GUIDANCE.md` — backup procedure before any live verification touching the main save

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `Systems/BossRegistry.cs`, `Systems/SummonItemRegistry.cs` — fully boss-agnostic, zero changes needed beyond the `canSummon` eligibility-delegate extension already added in Phase 6 (reusable if any Phase 10 boss needs similar item-breaking-precondition logic)
- `Systems/BossArenaRoutingRegistry.cs` — generic `Register<T>(bossNpcType)`, ready to accept many more calls
- 7 existing biome `Subworld`/`GenPass` pairs from Phase 9 — direct routing targets for most of this phase's roster

### Established Patterns
- Per-boss decompiled-source verification (not wiki-only) for `OnKill()` side effects, player/world-scope classification, and actual `Zone*` dependency — locked discipline since Phase 4, reconfirmed every phase since
- `[JITWhenModsEnabled("<ModName>")]` on every method touching a weak-referenced mod's types, named delegates only (never inline lambdas)

### Integration Points
- `Integrations/CalamityIntegration.cs` — will grow to register ~11 more Calamity bosses
- `Integrations/SpiritIntegration.cs` — will grow to register ~6 more Spirit bosses
- New forced day/night utility (D-04) — likely a new small helper, exact shape is Claude's Discretion

</code_context>

<specifics>
## Specific Ideas

No visual/content specifics — this discussion was entirely about scope and implementation-approach decisions (biome-assignment principle, Infernum conditionality, roster completeness, time-gating), consistent with this project's prior mod-integration phase discussions.

</specifics>

<deferred>
## Deferred Ideas

- **Redemption full-roster expansion** (beyond Thorn, Phase 6) — most of Redemption's remaining bosses are structure-gated/non-portable per `09-ALTAR-BIOME-REFERENCE.md` Section 2 (confirmed, not just unresearched); explicitly out of this phase's mod scope (Calamity/Spirit only, per phase title).
- **CatalystMod full-roster expansion** (beyond Astrageldon, Phase 6) — not researched beyond Astrageldon; out of this phase's mod scope.
- **NoxusBoss / ContinentOfJourney / Daybreak** — Phase 7's unstarted scope, not this phase's. ContinentOfJourney's exact mod identity remains unresolved (09-ALTAR-BIOME-REFERENCE.md Section 5, Open Item 1) — still needs the user to supply a Workshop ID/author name whenever Phase 7 is picked up.
- **Other Calamity-adjacent rework mods** (Fargo's Mod, Fargo's Soul Mod, Calamity Community Remix, etc.) — not audited for structure-gating effects analogous to Infernum (09-ALTAR-BIOME-REFERENCE.md Section 1, Open Item 4). Only Infernum is handled by D-02. Flagged as a known gap for a later milestone if the user runs other Calamity-rework mods.
- **Dungeon / Sulphurous Sea subworld rebuild** — not decided here; contingent on Open Item 3's resolution during research (Claude's Discretion above). If needed, this is new work, not a resurrection of the discarded Phase 9 Wave-1 code (unreachable from master per D-07).

### Reviewed Todos (not folded)
None — `todo match-phase` returned 0 matches for Phase 10.

</deferred>

---

*Phase: 10-full-calamity-spirit-boss-roster-registration-and-biome-subworld-routing*
*Context gathered: 2026-08-14*
