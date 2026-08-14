# Phase 7: ContinentOfJourney/Daybreak (Homeward Journey) Integration - Context

**Gathered:** 2026-08-14
**Status:** Ready for planning

<domain>
## Phase Boundary

Identifies ContinentOfJourney/Daybreak's actual mod identity, researches its downed-progress API, and registers at least one of its bosses into `BossRegistry`/`SummonItemRegistry`, completing v1 mod coverage per MOD-06. **NoxusBoss (Devourer of Universes) is no longer part of this phase's scope** — it was removed from v1 entirely during this discuss-phase session (see Decisions below). This phase's original title ("NoxusBoss & ContinentOfJourney/Daybreak Integration") has been renamed accordingly in ROADMAP.md.

</domain>

<decisions>
## Implementation Decisions

### Mod identity resolution
- **D-01:** "ContinentOfJourney" is **Homeward Journey** by GabeHasWon, Steam Workshop id `2930931197` (https://steamcommunity.com/sharedfiles/filedetails/?id=2930931197). Two prior research passes (during Phase 9 prep, documented in `09-ALTAR-BIOME-REFERENCE.md` Section 5 Open Item 1) could not resolve the literal name "ContinentOfJourney" via Workshop/wiki/mirror search. The user supplied this Workshop link directly during this discuss-phase session, confirming the working guess that the phase title's parenthetical "(Homeward series)" was the actual pointer all along. Homeward Journey has 15 bosses across pre-hardmode, hardmode, and post-Moon Lord tiers (confirmed via terrariamods.wiki.gg).
- **D-02:** "Daybreak" is reconfirmed as `gold-meridian/daybreak-mod` — a boss-less library dependency required by Wrath of the Gods, not a separate boss-bearing mod. The user independently verified this ("신들의분노 모드의 의존성모드였으니까 아무것도 없는게 맞고"). No registration target exists under this name; it is not part of this phase's research or registration scope.

### NoxusBoss removed from v1 scope (not deferred — a permanent cut)
- **D-03:** NoxusBoss (Devourer of Universes and its other bosses) is removed from this phase's scope, and from v1 scope entirely — not deferred to a later phase or backlog. User's rationale: most NoxusBoss bosses are quest-triggered (Solyn's moon-event questline) or already run in their own dedicated subworld/arena mechanic, so they don't fit this project's plain-summon-item carrier-item redirect pattern the way Calamity/Spirit/Redemption/CatalystMod's bosses do. Explicitly "계획없음" (no plan to revisit).
  - **Downstream doc updates already applied in this session** (not left for planner): `ROADMAP.md` Phase 7 title/Goal/Success Criteria/Requirements trimmed to MOD-06 only (NoxusBoss's former Success Criterion 1 removed); `REQUIREMENTS.md` MOD-05 marked Removed with a Traceability row and Out of Scope table entry; `PROJECT.md` moved the NoxusBoss requirement line to Out of Scope and added two Key Decisions rows (NoxusBoss removal; ContinentOfJourney identification). Researcher/planner do not need to re-derive or re-justify this — treat it as already-locked project state, same as any other completed phase's roadmap edit.

### Boss selection within Homeward Journey's roster
- **D-04:** Which specific Homeward Journey boss to register as the "one worked example" (mirroring the Phase 6 Redemption/Thorn and CatalystMod/Astrageldon pattern — MOD-06 only requires "at least one") is **Claude's Discretion**, to be resolved during research. User's explicit guidance for the tiebreaker: prefer whichever boss has the lowest downed-progress-API research risk (e.g. avoid Master-Mode-only alt-AI complexity, avoid bosses gated behind in-mod structure/progression requirements analogous to Redemption's structure-gated bosses from `09-ALTAR-BIOME-REFERENCE.md` Section 2) — same "research finds the actual API shape, doesn't guess from the wiki" discipline used in every prior mod-integration phase. Document the chosen boss and why in code comments, mirroring the Phase 6 Thorn/Astrageldon precedent.

### Biome/arena routing — apply Phase 9's wiki-thematic principle from the start
- **D-05:** The selected Homeward Journey boss should be routed to a themed arena subworld following Phase 9's "wiki-thematic assignment" principle (same principle reconfirmed in Phase 10 D-01) — assign the boss's wiki-stated biome/arena even if research finds no functional `Zone*`/`CheckActive` AI dependency on it. This applies from the start of this phase's research, not as an afterthought. Research must:
  - Check whether the selected boss's biome matches one of the 7 existing Phase 9 subworlds (`BossArenaHallowSubworld`, `BossArenaUnderworldSubworld`, `BossArenaAstralSubworld`, `BossArenaJungleSubworld`, `BossArenaSpaceSubworld`, `BossArenaDesertSubworld`, `BossArenaBriarSubworld`).
  - If not (e.g. Homeward Journey is known to add its own new "Abyss" biome, per wiki search during this discussion — post-Plantera content), flag this explicitly as a new-subworld-build implication for planning to size, rather than silently defaulting to the plain `BossArenaSubworld`.
  - Document (functional vs. thematic-only) reasoning in code comments either way, per the Phase 6/9/10 established discipline.

### Claude's Discretion
- Exact boss pick within Homeward Journey's 15-boss roster (D-04).
- Whether the chosen boss's biome needs a new subworld built in this phase, or reuses an existing Phase 9 one (D-05) — resolved during research.
- Whether new integration code goes into a new `Integrations/HomewardJourneyIntegration.cs` (most likely, first boss from this mod) vs. any other file split — follow the established one-file-per-mod convention unless research finds a reason not to.
- Exact per-boss `OnKill()` decompiled-source verification (side effects, player-scoped vs. world-scoped classification, actual downed-flag API shape — wrapper property vs. raw static field vs. reflection-only) — same discipline as every prior mod-integration phase, performed during research, not decided here.

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Requirements this phase implements
- `.planning/REQUIREMENTS.md` §"Mod Coverage (MOD)" — MOD-06 (MOD-05/NoxusBoss removed, see D-03)
- `.planning/ROADMAP.md` §"Phase 7" — Goal/Success Criteria/Scope note (updated in this session)

### Prior research on this exact identity question
- `.planning/phases/09-biome-dependent-subworld-coverage/09-ALTAR-BIOME-REFERENCE.md` Section 5 "Open items" — Item 1 (ContinentOfJourney unresolved, now resolved by D-01) and Item 2 (Daybreak library-only, now reconfirmed by D-02). Read for the exact research-methodology precedent (Workshop/wiki/mirror search) even though the identity question itself is now closed.

### Locked patterns from Phase 3-6/9/10 (extend unchanged into this phase)
- `.planning/PROJECT.md` §Key Decisions — the Phase 4 lambda/`[JITWhenModsEnabled]` JIT-crash lesson (commit `0e19600`); the Phase 9 lesson that lazy construction inside a containing class is NOT sufficient JIT protection (every method touching a weak-referenced mod's types needs its own `[JITWhenModsEnabled]` tag); the two new rows added in this session (NoxusBoss removal, ContinentOfJourney identification)
- `.planning/phases/06-redemption-catalystmod-integration/06-CONTEXT.md` — most recent precedent for per-boss Zone-dependency verification discipline (Thorn/Astrageldon both confirmed via full decompiled-source read, not assumption) and the "one worked-example boss per mod" scope pattern this phase follows for Homeward Journey
- `.planning/phases/10-full-calamity-spirit-boss-roster-registration-and-biome-subworld-routing/10-CONTEXT.md` D-01 — the "wiki-thematic assignment" principle this phase's D-05 reapplies
- `Integrations/CalamityIntegration.cs`, `Integrations/SpiritIntegration.cs`, `Integrations/RedemptionIntegration.cs`, `Integrations/CatalystIntegration.cs` — direct templates: `PostSetupContent()` with `ModLoader.HasMod("<ModName>")` guard, `[JITWhenModsEnabled("<ModName>")]`-tagged methods, named (never lambda) delegates
- `Systems/BossArenaRoutingRegistry.cs` + the 7 biome-variant subworlds from Phase 9 — the routing targets D-05 checks against
- `.planning/research/PITFALLS.md` §"Pitfall 5" (player-scoped vs. world-scoped double-grants), §"Pitfall 2/4" (JIT isolation, reflective lookup fragility)

### World-safety / process references
- `docs/WORLD_BACKUP_GUIDANCE.md` — backup procedure before any live verification touching the main save

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `Systems/BossRegistry.cs`, `Systems/SummonItemRegistry.cs` — fully boss-agnostic, zero changes needed
- `Systems/BossArenaRoutingRegistry.cs` — generic `Register<T>(bossNpcType)`, ready to accept the new Homeward Journey boss
- 7 existing biome `Subworld`/`GenPass` pairs from Phase 9 — potential direct routing target per D-05 (pending research confirming which, if any, matches)

### Established Patterns
- Per-boss decompiled-source verification (not wiki-only) for `OnKill()` side effects, player/world-scope classification, and actual downed-flag API shape — locked discipline since Phase 4, reconfirmed every phase since
- `[JITWhenModsEnabled("<ModName>")]` on every method touching a weak-referenced mod's types, named delegates only (never inline lambdas)
- "One worked-example boss per mod" scope discipline (Phase 6 precedent) — this phase follows the same shape for Homeward Journey, satisfying MOD-06's "at least one" requirement

### Integration Points
- New `Integrations/HomewardJourneyIntegration.cs` (or similar name — Claude's Discretion) will be created, following the existing per-mod integration file pattern
- `Libs/` currently has no `HomewardJourney.dll`/`ContinentOfJourney.dll` — will need extraction (Phase 6 `scripts/extract_tmod.py` precedent) or the user subscribing/enabling the mod locally, to be resolved during research/planning

</code_context>

<specifics>
## Specific Ideas

No visual/content specifics beyond the mod-identity resolution itself — this discussion was primarily about resolving a long-standing open research question (ContinentOfJourney's identity) plus a scope-reduction decision (dropping NoxusBoss), consistent with this project's prior mod-integration phase discussions.

</specifics>

<deferred>
## Deferred Ideas

- **NoxusBoss** — not deferred, permanently removed from v1 scope (D-03). Do not resurrect without a new, explicit user request and its own scoping discussion; see `PROJECT.md` Out of Scope for the locked rationale.
- **Homeward Journey full-roster expansion** (beyond the single boss registered this phase) — out of this phase's scope, same "one worked example, prove the pattern first" discipline used in Phase 6 for Redemption/CatalystMod. A future phase analogous to Phase 10 (full Calamity/Spirit roster) could pick this up later if the user wants it, but nothing is scheduled.
- **New Homeward Journey "Abyss" biome** (mentioned in wiki search results, post-Plantera content) — if the selected boss (D-04) turns out to require it, building a new `BossArenaAbyssSubworld` is new work for this phase's planning to size, not a resurrection of any prior discarded code (no prior Abyss subworld has ever existed in this project, unlike the Dungeon/Sulphurous Sea case from Phase 9 D-07).

### Reviewed Todos (not folded)
None — `todo match-phase` returned 0 matches for Phase 7.

</deferred>

---

*Phase: 07-noxusboss-continentofjourney-daybreak-integration*
*Context gathered: 2026-08-14*
