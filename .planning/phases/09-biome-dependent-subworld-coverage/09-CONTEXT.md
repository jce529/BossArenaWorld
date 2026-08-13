# Phase 9: Biome-Dependent Subworld Coverage - Context

**Gathered:** 2026-08-13
**Status:** Ready for planning

<domain>
## Phase Boundary

Phase 9 delivers themed-biome arena `Subworld` variants (GenPass-based, following the `BossArenaCorruptionSubworld`/`CorruptionPlatformPass` precedent from Phase 4) for every Calamity, Spirit, and CatalystMod boss identified as biome-dependent by this session's research — built preemptively, ahead of Phase 6/7 actually registering those bosses. Entry stays through the single existing `Test1Tile` portal with fully automatic routing via `BossArenaRoutingRegistry`; no new player-facing items. Infernum-conditional registration logic and any forced day/night mechanism are explicitly out of this phase's scope.

</domain>

<decisions>
## Implementation Decisions

### Phase sequencing
- **D-01:** Phase 9 proceeds now, ahead of Phase 6/7/8 completing, rather than waiting on ROADMAP.md's stated "Depends on: Phase 8" ordering. Rationale (user-confirmed): this session's research (`09-ALTAR-BIOME-REFERENCE.md`) already fully identifies which Calamity/Spirit/CatalystMod bosses need biome coverage, so there's no need to wait for Phase 6/7 to register those bosses first — the biome `Subworld` variants can be built now and simply connected via `BossArenaRoutingRegistry.Register<T>()` calls when Phase 6/7 later does the actual boss registration. Under the strict ROADMAP wording ("every boss registered in Phases 4-7"), Phase 9 would currently have zero work available (only Hive Mind [already covered, Phase 4] and Infernon [confirmed biome-independent, Phase 5] are registered so far) — this decision resolves that mismatch by reinterpreting Phase 9 as building ahead of registration, not auditing after it.

### Portal / entry architecture — NOT changing
- **D-02:** No new player-facing altar/portal items. The existing single `Test1Tile` (SUBW-02, validated live in Phase 2) remains the ONLY entry point, with fully automatic, invisible-to-the-player routing to the correct biome `Subworld` via `BossArenaRoutingRegistry`, keyed by the boss NPC type the player's held summon item resolves to. This matches ARENA-01's literal wording ("routed biome-variant subworld... via `BossArenaRoutingRegistry`") exactly.
- This directly supersedes the "recolored Demon Altar item per biome" concept explored earlier in the same conversation that produced `09-ALTAR-BIOME-REFERENCE.md`. That document's altar item names (`HallowAltar`, `UnderworldAltar`, etc.) survive ONLY as naming/visual reference for the internal `Subworld`/`GenPass` class names — they must NOT be implemented as separate placeable items. Downstream agents: do not build a multi-altar item system.

### Mod / boss scope for this phase
- **D-03:** Calamity + Spirit + CatalystMod only. Mod of Redemption and NoxusBoss/Wrath of the Gods are entirely excluded — confirmed via this session's multi-pass research that neither contributes any assignable boss (Redemption: every boss is either structure/trigger-gated with no portable item, or the reusable item found — `Hologram Remote` — is location-locked to a fixed arena, not portable; Wrath of the Gods: all 3 bosses excluded per explicit user decision on 2026-08-13, closed, not to be revisited). See `09-ALTAR-BIOME-REFERENCE.md` Sections 1-3 for the full per-boss trail.

### Infernum mod-combination handling — deferred to Phase 6
- **D-04:** Phase 9 does NOT implement the `ModLoader.HasMod("InfernumMode")`-conditional registration logic documented in `09-ALTAR-BIOME-REFERENCE.md` Section 1 (Providence/Profaned Guardians/Ceaseless Void becoming unassignable, The Old Duke becoming assignable, when Infernum is loaded alongside Calamity). Phase 9 only builds the biome `Subworld`/`GenPass` infrastructure itself (e.g. a real Underworld-tile-filled subworld exists and is reachable via `BossArenaRoutingRegistry`). The actual conditional `SummonItemRegistry`/`BossArenaRoutingRegistry.Register<T>()` calls — and the `InfernumMode` presence checks that gate them — belong to Phase 6, when these Calamity bosses are actually registered. Hand `09-ALTAR-BIOME-REFERENCE.md` Section 1's conditional table to Phase 6's research/planning as-is.

### Day/night forcing — out of scope
- **D-05:** A forced day/night mechanism (needed for Astrum Deus/Astrum Aureus specifically when Infernum is loaded, per `09-ALTAR-BIOME-REFERENCE.md` Section 1, and for the unrelated Section 4 time-gated boss list) is explicitly OUT of Phase 9's scope. ARENA-01 covers biome/Zone-flag dependence only, not time-of-day. This is not currently tracked as any REQUIREMENTS.md item — note it as a candidate future requirement rather than building it now. The Astral Infection subworld variant built in this phase does NOT need to force night; that gap is inherited by whichever future phase picks up Astrum Deus/Aureus's Infernum-specific delta.

### Build scope — all 9 biome variants, not a prioritized subset
- **D-06:** Build all 9 biome `Subworld` variants identified as needed by the D-03 mod scope in this single phase: Hallow, Underworld, Astral Infection, Jungle, Space, Dungeon, Desert, Briar, Sulphurous Sea. Matches this project's already-established principle (`PROJECT.md` Out of Scope / Key Decisions: "no boss priority ordering... marginal registration cost is uniform once the skeleton exists") extended to biome-variant construction — same reasoning, since each variant is a GenPass + Subworld pair built by copying the `CorruptionPlatformPass`/`BossArenaCorruptionSubworld` template. Sulphurous Sea is included even though it currently has zero assignable boss without Infernum (The Old Duke only becomes assignable when Infernum is loaded, per D-04, and that registration is deferred to Phase 6) — built now for the same uniform-cost reasoning, ready for Phase 6 to connect later.

### Claude's Discretion
- Exact vanilla/modded tile IDs, weights, and fill thickness needed to reliably satisfy each biome's Zone-flag detection (e.g. which tiles set `ZoneHallow`, `ZoneUnderworld`/whatever the real underlying flag is, `ZoneJungle`, etc.) — follow the exact decompilation-verified methodology already used for `CorruptionPlatformPass` (trace `Player.UpdateBiomes()` → `SceneMetrics` → `TileLoader.RecountTiles()` → `TileID.Sets.<Biome>Biome` weight table for each target biome), not assumed from memory.
- Exact class/file naming for the 9 new `Subworld`/`GenPass` pairs (this document's names — `BossArenaHallowSubworld`, `HallowPlatformPass`, etc. — are a reasonable default following the Corruption precedent, but planning may adjust).
- Whether each new Subworld class duplicates `BossArenaCorruptionSubworld`'s vanilla-downed-flag `OnEnter`/`OnExit` snapshot/restore guard verbatim (per its own code comment, this guard is required independently for every `Subworld` subclass in this mod) — expected yes, but confirm during planning.
- Astral Infection biome specifically needs an `Astral Beacon`-equivalent or real Astral Infection tile composition sourced from Calamity — research needed on which tiles/structures actually satisfy Calamity's Astral Infection zone check (this is a modded biome, not vanilla, so the `CorruptionPlatformPass` vanilla-tile-ID methodology needs a Calamity-specific equivalent).

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Primary research input for this phase
- `.planning/phases/09-biome-dependent-subworld-coverage/09-ALTAR-BIOME-REFERENCE.md` — the compiled, multi-pass-verified research this entire phase is built on: per-boss wiki-sourced biome requirements across Calamity/Spirit/Redemption/CatalystMod/Wrath of the Gods, the Infernum mod-combination delta table (Section 1), the confirmed-excluded boss list (Sections 2-3), the biome→altar-naming→subworld table (Section 3... now Section 4 context), and open unresolved items (Section 5). MANDATORY primary input — read in full before research/planning.

### Phase requirements & goal
- `.planning/ROADMAP.md` (Phase 9 section, "### Phase 9: Biome-Dependent Subworld Coverage") — goal statement and 3 success criteria
- `.planning/REQUIREMENTS.md` (ARENA-01) — formal requirement wording this phase satisfies

### Precedent methodology to replicate per biome
- `.planning/debug/resolved/hivemind-zonecorrupt-despawn-corruption-subworld.md` — the exact decompilation-tracing methodology (`Player.UpdateBiomes()` → `SceneMetrics.EnoughTilesForCorruption` → `TileLoader.RecountTiles()` → `TileID.Sets.CorruptBiome` weight table → full-platform-width fill) that produced `CorruptionPlatformPass`. Apply the same tracing per target biome, don't assume tile IDs from memory.

### Existing code this phase extends (read before writing new code)
- `Systems/BossArenaRoutingRegistry.cs` — the routing mechanism every new biome variant registers into (`Register<T>(bossNpcType)`, `Enter`, `IsAnyArenaActive`)
- `Subworlds/BossArenaCorruptionSubworld.cs` — template every new `Subworld` subclass should mirror, including its documented vanilla-downed-flag `OnEnter`/`OnExit` snapshot/restore guard (required independently per subclass, not inherited)
- `Subworlds/CorruptionPlatformPass.cs` — template every new `GenPass` should mirror (full-platform-width fill, explicit spawn point set)
- `Tiles/Test1Tile.cs`, `Systems/BossSummonPlayer.cs` — confirms the single-portal, auto-routing architecture that must NOT change per D-02

### Registration pattern precedent (for Phase 6/7's future reference, not this phase's own work)
- `Integrations/CalamityIntegration.cs`, `Integrations/SpiritIntegration.cs` — the per-mod boss registration pattern Phase 6/7 will follow when actually connecting bosses to the subworlds this phase builds

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `BossArenaRoutingRegistry.Register<T>(bossNpcType)` — generic registration call, ready to accept 9 new `Subworld` types once built (actual `Register` calls deferred to Phase 6/7 per D-04, but the registry itself needs no changes)
- `CorruptionPlatformPass` — direct copy-paste template for the other 8 `GenPass` implementations (same full-width-fill, explicit-spawn-point-set structure)
- `BossArenaCorruptionSubworld` — direct copy-paste template for the other 8 `Subworld` classes, including the vanilla-downed-flag guard duplication

### Established Patterns
- Full-platform-width tile fill (not just spawn-point-local) is required because vanilla recomputes Zone flags every tick from a live per-tick tile scan centered on the player (`Player.UpdateBiomes()`) — confirmed via decompilation, not assumption
- GenPass-based real biome tiles, not `ModPlayer` Zone-flag overrides — this alternative was explicitly presented to and rejected by the user twice already (Phase 4); do not re-litigate
- Each `Subworld` subclass independently needs `BossArenaCorruptionSubworld`'s vanilla-downed-flag `OnEnter`/`OnExit` snapshot/restore guard duplicated (SubworldLibrary's `CopyDowned()`/`ReadCopiedDowned()` applies per-subworld, not project-wide) — see `.planning/debug/resolved/isolation-premise-flag-persistence.md`

### Integration Points
- None for actual bosses yet (that's Phase 6/7's job) — this phase's only integration point is that the 9 new `Subworld` types exist and are structurally ready for a future `BossArenaRoutingRegistry.Register<T>()` call

</code_context>

<specifics>
## Specific Ideas

No visual/content specifics beyond what's captured in Decisions — this discussion was entirely structural/scope-focused (sequencing, architecture, mod scope, ownership boundaries), not visual design. The one specific to flag clearly for downstream agents: the "recolored Demon Altar item per biome" visual concept explored earlier in this same conversation is explicitly NOT being implemented (see D-02) — its naming survives only as internal class-name reference in `09-ALTAR-BIOME-REFERENCE.md`.

</specifics>

<deferred>
## Deferred Ideas

- **Mod of Redemption bosses, NoxusBoss/Wrath of the Gods bosses** — permanently excluded, not deferred-for-later. Confirmed via research + explicit user decision (2026-08-13). Do not resurface without new user instruction. See `09-ALTAR-BIOME-REFERENCE.md` Sections 2-3.
- **Infernum-conditional registration logic** (`ModLoader.HasMod("InfernumMode")` gating for Providence/Profaned Guardians/Ceaseless Void/The Old Duke) — deferred to Phase 6, per D-04.
- **Forced day/night utility mechanism** — deferred, not currently a tracked requirement anywhere in REQUIREMENTS.md; candidate for a future phase/requirement if the user wants to pursue Astrum Deus/Aureus's Infernum night-delta or the Section 4 time-gated boss list later.
- **ContinentOfJourney / Daybreak mod identification** — still an unresolved research gap from this session (neither could be identified via web/Workshop search); needs a user-supplied Workshop ID or author name. Not blocking Phase 9; relevant to a future Phase 6/7-equivalent pass if these mods are ever pinned down.
- **Bloodworm Platter's Sulphurous Sea requirement — item gate or AI gate?** Unresolved from this session's research; needs a decompiled-source check (same discipline as `04-RESEARCH.md`/`05-RESEARCH.md`) before Phase 6 finalizes whether The Old Duke strictly needs the Sulphurous Sea subworld or just benefits from it safely. Noted for Phase 6's research, not this phase's.
- **Multi-altar player-facing UX** (colored altar items per biome, letting the player choose) — considered and rejected in favor of keeping the existing single-portal architecture (D-02). Not scheduled for any future phase unless the user explicitly revisits this decision.

### Reviewed Todos (not folded)
None — no pending todos matched this phase (`todo match-phase` returned zero matches).

</deferred>

---

*Phase: 09-biome-dependent-subworld-coverage*
*Context gathered: 2026-08-13*
