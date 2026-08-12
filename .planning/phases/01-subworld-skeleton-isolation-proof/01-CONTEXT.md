# Phase 1: Subworld Skeleton & Isolation Proof - Context

**Gathered:** 2026-08-13
**Status:** Ready for planning

<domain>
## Phase Boundary

A dedicated boss-arena subworld exists that has never had any mod content placed in it, and the player can reliably enter and exit it, with the founding "flags don't cross worlds" premise proven empirically rather than assumed. This phase does NOT build the real summon-item redirect (Phase 2) or the carrier-item pipeline (Phase 3) — it proves the subworld itself and the isolation bug it works around.

</domain>

<decisions>
## Implementation Decisions

### Test entry/exit mechanism
- **D-01:** Use a debug-only chat command (e.g. `/bossarena-enter`, `/bossarena-exit`) to enter/exit the subworld for Phase 1 testing, since Phase 2's real summon-item redirect doesn't exist yet.
- **D-02:** This debug command is fully removed once Phase 2's real redirect lands — not kept behind a permanent debug flag. It's a Phase-1-only verification tool.

### Empty subworld terrain
- **D-03:** The subworld has a minimal flat platform, not a bare void and not a fully decorated arena. Purpose is a walkable surface sufficient for Phase 1's isolation-proof test.
- **D-04:** Full arena decoration (multi-layer platforms, aesthetics) is explicitly deferred — Phase 1 builds only the minimal platform. This stays consistent with REQUIREMENTS.md's Out of Scope item "Full arena-building/decoration toolkit" (duplicates Luiafk).
- **D-05:** Platform material: stone blocks.
- **D-06:** Platform width: approximately 10,000 blocks wide, horizontal flat plane (user's explicit request — large enough to accommodate any boss's movement range in later phases).
- **D-07:** Platform thickness: simple/thin stone layer, no elaborate depth requirement (user: "just make it stone blocks"). Claude's discretion on exact value — a thin layer (roughly 10-20 blocks) is reasonable.
- **D-08:** No edge/boundary walls at the platform's ends — not needed given the platform's width.

### Biome zone override infrastructure
- **D-09:** Build a general-purpose hook/function now that can force-set `Player.Zone*` flags while inside the subworld, since some bosses (Wall of Flesh needs Underworld, Plantera needs Jungle, Duke Fishron needs Ocean, etc.) require specific biome conditions to spawn or behave correctly. This is infrastructure only — no boss-to-biome mapping exists yet (that's populated per-boss starting Phase 3+ once `BossRegistry` exists).

### Isolation-proof method
- **D-10:** Use King Slime as the test boss for the empirical isolation-proof test (not Moon Lord — rejected because Moon Lord requires defeating 3 mechanical bosses + Golem first, making it impractical to test repeatedly).
- **D-11:** Actually summon and kill King Slime for real inside the subworld (not just toggling `NPC.downedSlimeKing` via debug command) — validates real gameplay behavior, not just the boolean.
- **D-12:** No `BossCoreItem`/`BossRegistry` carrier-item is used in this test — those don't exist until Phase 3. The test purely observes whether `NPC.downedSlimeKing` propagates from the subworld back to the main world on return, with no explicit sync action taken. Per `research/PITFALLS.md` Pitfall 1, the expected (correct) result is that it does NOT propagate — that's the premise being proven.

### World-backup & test-world strategy
- **D-13:** Phase 1's own testing happens on a fresh, disposable test world with all other content mods (Calamity, Spirit, etc.) unloaded/disabled — not the player's real save (`HiPo's_Terrarium`). No backup is needed for this throwaway world.
- **D-14:** The VERIFY-02 world-backup guidance deliverable is still written in this phase, but as forward-looking documentation for later phases (4-8) when testing must happen against the real save with all content mods enabled. It is not exercised by Phase 1's own testing.

### Claude's Discretion
- Exact platform Y-level/vertical position within the subworld
- Precise platform thickness value (guideline: thin, ~10-20 blocks)
- `GenPass` implementation details for generating the flat stone platform
- Debug command naming/argument syntax specifics
- Format and location of the world-backup guidance document (e.g. a markdown doc vs. inline code comments)

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Subworld isolation premise (the core thing Phase 1 proves)
- `.planning/research/PITFALLS.md` §"Pitfall 1: Boss 'downed' flags are isolated per world file and do not survive the subworld round-trip" — the exact bug this phase must empirically reproduce and confirm; includes the recommended test methodology (kill a vanilla boss, return without a carrier item, confirm main-world flag stays false) and the `Subworld.ShouldSave = false` recommendation
- `.planning/research/PITFALLS.md` §"Pitfall-to-Phase Mapping" table — confirms Pitfall 1 is scoped to "Foundational subworld-setup phase" (this phase)

### Subworld architecture pattern
- `.planning/research/ARCHITECTURE.md` §"Pattern 1: Subworld as an isolated, no-save dimension" — `Subworld` subclass shape (`Width`/`Height`/`Tasks`/`ShouldSave`/`NoPlayerSaving`), including the explicit warning that `NoPlayerSaving` must stay `false` so the player's inventory (and later, the carrier item) survives the trip
- `.planning/research/ARCHITECTURE.md` §"Recommended Project Structure" — places the subworld definition at `Subworlds/BossArenaSubworld.cs`

### Project-level requirements
- `.planning/REQUIREMENTS.md` §"Subworld & Entry (SUBW)" — SUBW-05 (zero placed mod content), SUBW-06 (reliable exit/return)
- `.planning/REQUIREMENTS.md` §"Verification & Safety (VERIFY)" — VERIFY-02 (world-backup guidance documented and followed before live testing)
- `.planning/REQUIREMENTS.md` §"Out of Scope" — "Full arena-building/decoration toolkit" explicitly excluded; governs the D-03/D-04 scope line drawn in this phase

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- None yet — this is the first implementation phase. `BossArenaSubWorld.cs` currently contains only the empty `Mod` class skeleton generated by the tModLoader "New Mod" wizard.

### Established Patterns
- No in-repo patterns exist yet. `build.txt` does not yet declare `modReferences = SubworldLibrary` — this must be added as part of this phase's implementation.
- No `ExampleMod` reference copy exists locally in this repo; `research/ARCHITECTURE.md` and `research/PITFALLS.md` (see canonical_refs above) already distill the relevant SubworldLibrary/ExampleMod patterns, so re-fetching ExampleMod source is not required to start this phase.

### Integration Points
- `Subworlds/BossArenaSubworld.cs` (new file) is the natural home for the `Subworld` subclass — SubworldLibrary auto-discovers any `Subworld` subclass in the mod, no manual registration needed.
- The debug entry/exit chat commands (D-01) are a temporary, separate concern from the eventual `SubworldEntrySystem`/summon-item redirect built in Phase 2 — keep them in an isolated file so removal in Phase 2 is a clean deletion, not a refactor.

</code_context>

<specifics>
## Specific Ideas

- Platform should be very wide (~10,000 blocks) so it can accommodate large-movement-range bosses in later phases without needing to be resized.
- The biome-override hook is meant to be generic/reusable — a single function capable of forcing any `Player.Zone*` flag — not hardcoded to any specific boss's needs yet.

</specifics>

<deferred>
## Deferred Ideas

- **Arena decoration (multi-layer platforms, aesthetics)** — user raised "서브 월드 내부를 플랫폼을 이용한 아레나로 꾸미기" (decorating the subworld interior as an arena using platforms). Resolved: Phase 1 builds only the minimal flat platform; richer arena construction is deferred to whichever later phase actually needs a fight-ready arena (Phase 2 onward), and stays bounded by REQUIREMENTS.md's existing "Full arena-building/decoration toolkit" Out of Scope entry (duplicates Luiafk) — should not grow into a dedicated decoration-toolkit phase.
- **Per-boss biome-to-flag mapping** — only the generic override hook (D-09) is built now. Actually wiring specific bosses to specific biome flags belongs with each boss's registration, starting Phase 3 (vanilla POC) and continuing through Phase 4+ (Calamity, Spirit, etc.).

</deferred>

---

*Phase: 01-subworld-skeleton-isolation-proof*
*Context gathered: 2026-08-13*
