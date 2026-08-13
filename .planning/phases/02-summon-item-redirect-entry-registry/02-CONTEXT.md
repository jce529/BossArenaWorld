# Phase 2: Summon-Item Redirect & Entry Registry - Context

**Gathered:** 2026-08-13
**Status:** Ready for planning

<domain>
## Phase Boundary

Using a registered boss-summon item at a new dedicated portal tile redirects the player into the boss-arena subworld instead of summoning the boss in the main world, with the boss auto-summoning on arrival and the summon item preserved. This phase does NOT build the carrier-item/BossRegistry pipeline (Phase 3) — it only proves entry works, continuing to observe (not fix) any downed-flag behavior via the debug tooling built in Phase 1.

</domain>

<decisions>
## Implementation Decisions

### Entry mechanism — portal tile (supersedes original "item-only" design)
- **D-01:** The original PROJECT.md decision ("existing summon items alone are the trigger, no separate portal item") is explicitly reversed for this phase, per user request during discussion. See PROJECT.md Key Decisions table for the superseded/superseding rows.
- **D-02:** A new placeable tile/furniture object, working name `Test1` (internal name only — NOT final, rename before ship), is the subworld's portal object.
- **D-03:** `Test1`'s appearance is a **brand-new custom `ModTile`** that visually benchmarks the Corruption Altar sprite (texture reused/referenced for visual similarity only). It must NOT reuse the actual vanilla Demon Altar/Crimson Altar tile type — no inherited vanilla altar behavior (hammer-smash hardmode trigger, "a horrible chill..." message, altar-crafting-recipe unlocks, etc.). This is a from-scratch tile with only our own interaction logic attached.
- **D-04:** Interaction trigger: player right-clicks the placed `Test1` tile while holding a registered boss-summon item in hand. This is the sole redirect trigger for this phase — direct use of the summon item elsewhere is untouched and keeps its normal vanilla/modded main-world behavior.
- **D-05:** Acquisition for this phase: `Test1` has no crafting recipe. It's obtained via the Creative menu / debug-only means (consistent with Phase 1's debug-tooling pattern) since the internal name and final itemization aren't decided yet.

### Summon-item registry scope
- **D-06:** SUBW-01's central registry is data-driven/extensible in shape, but only needs one populated entry for this phase's proof.
- **D-07:** Registry scope for v1 of this phase is limited to **simple "use item to summon" style items** (e.g. Slime Crown, Suspicious Looking Eye) — NOT structurally different triggers like altar-thrown items (Guide Voodoo Doll) or bulb-break summons (Plantera). Those remain out of this phase's scope; revisit when/if a boss needing them is registered in a later phase.

### Proof boss
- **D-08:** King Slime, via Slime Crown, is the boss/item used to prove this phase's mechanism — continuity with Phase 1's isolation-proof test, and Slime Crown is a non-consumable item so SUBW-04's "item not consumed" requirement is trivially satisfied for this proof.

### Boss auto-summon mechanism
- **D-09:** Once the player arrives in the subworld, the boss is summoned by **replaying (re-triggering) the same held summon item's own use-effect** inside the subworld — not bespoke per-boss spawn code. This generalizes cleanly to any future item registered under D-07's scope, since "replay the item's use effect" works identically regardless of which specific boss the item summons.
- **D-10:** No specific spawn position/timing logic is needed beyond "immediately after arrival, in the subworld" — the existing 10,000-block-wide flat platform (Phase 1) is wide open, so there's no positioning concern to solve.

### Redirect feedback
- **D-11:** A simple chat message is shown to the player at the moment of redirect (e.g. confirming they're being sent to the boss arena). No screen-transition effects or sound cues — those are explicitly out of this phase's scope (would be UX polish, not core mechanism).

### Claude's Discretion
- Exact chat message wording for D-11
- `Test1` tile's exact texture-reuse implementation approach (e.g. `ModContent.Request` against the vanilla altar's asset path vs. a copied sprite file) — technical detail, not a user-facing decision
- Exact mechanism for "replaying" a held item's use-effect in the subworld (e.g. calling the item's `UseItem`/`UseStyle` logic directly vs. another approach) — implementation detail for research/planning
- Tile placement rules (where in the main world the player may place `Test1`, light source, break/interaction sounds, etc.) beyond "visually similar to Corruption Altar"

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Project-level requirements (updated during this discussion)
- `.planning/REQUIREMENTS.md` §"Subworld & Entry (SUBW)" — SUBW-01 through SUBW-04, rewritten during this discussion to reflect the portal-tile design (D-01 through D-04)
- `.planning/PROJECT.md` §"Key Decisions" — records the superseded "item-only entry" decision and the new portal-tile decision, with rationale and date

### Prior phase context
- `.planning/phases/01-subworld-skeleton-isolation-proof/01-CONTEXT.md` — D-10/D-11/D-12 (King Slime as the isolation-proof test boss, real-kill methodology); this phase reuses that same boss/item for continuity (D-08)
- `Debug/SubworldDebugCommands.cs` — Phase 1's debug enter/exit/checkflag commands remain in place for this phase (per Phase 1's D-02, they're deleted only once Phase 2's real redirect fully lands and is verified — confirm at planning time whether removal belongs in this phase's plan or a follow-up)

### Isolation/flag-persistence background (informs but does not change this phase's scope)
- `.planning/debug/isolation-premise-flag-persistence.md` — resolved debug session; explains why `BossArenaSubworld.OnEnter()`/`OnExit()` already defensively snapshot/restore vanilla downed flags. Relevant because this phase's King Slime proof will exercise that same guard again via the new portal-tile entry path instead of the debug `/bossarena-enter` command.

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `Subworlds/BossArenaSubworld.cs` — the `Subworld` subclass (`SubworldSystem.Enter<BossArenaSubworld>()`/`Exit()` target). Already has `OnEnter()`/`OnExit()` guards for the vanilla downed-flag leak (see isolation-premise debug doc above) — no changes needed for this phase, but new entry code must still route through `SubworldSystem.Enter<BossArenaSubworld>()` the same way the debug command does.
- `Systems/BiomeOverridePlayer.cs` — generic `Player.Zone*` override hook (D-09 from Phase 1). Not needed for King Slime (no biome requirement), but establishes the pattern if a later boss under this same phase's item-replay mechanism needs a biome override before its summon item will work.
- `Debug/SubworldDebugCommands.cs` — `/bossarena-enter`, `/bossarena-exit`, `/bossarena-checkflag`. Useful for manually verifying the portal-tile redirect lands the player in the same subworld, and for re-checking `NPC.downedSlimeKing` behavior after using the new entry path.

### Established Patterns
- `SubworldLibrary`'s `SubworldSystem.Enter<T>()` is the confirmed, working call for subworld entry (used by the debug command in Phase 1) — the new portal-tile interaction should call this directly, not reinvent entry.
- No existing `ModTile` or `GlobalItem`/tile-interaction hook exists yet in this codebase — this phase introduces both patterns for the first time.

### Integration Points
- New files needed (naming/location at planner's discretion): a `ModTile` for `Test1`, its corresponding `ModItem` (placeable item that places the tile), and a tile-interaction hook (`ModTile.RightClick` or equivalent) that reads the player's held item, checks it against the registry, and calls `SubworldSystem.Enter<BossArenaSubworld>()`.
- The summon-item → boss registry (SUBW-01) is new infrastructure — no existing registry pattern in this codebase to extend (Phase 1 had no registries). Natural home is alongside where `BossRegistry` will eventually live (Phase 3), but Phase 2 only needs the item→subworld-entry side, not the boss-kill→apply side.

</code_context>

<specifics>
## Specific Ideas

- The portal tile should look like the Corruption Altar (visual reference only) — the user specifically named this tile as their mental model for "what a boss-arena portal looks like."
- The generalized "replay the held item's use-effect in the subworld" mechanism (D-09) was the user's own idea, offered as a clarification when asked where/when the boss should auto-summon — it avoids needing per-boss spawn logic entirely for any item in D-07's scope.

</specifics>

<deferred>
## Deferred Ideas

- **Non-"simple-use" summon triggers** (altar-thrown items like Guide Voodoo Doll, bulb-break like Plantera) — explicitly out of this phase's registry scope (D-07). Revisit when a boss needing one of these trigger types is actually registered (likely Phase 4+ as Calamity/other mods' bosses come online).
- **Screen-transition effects / sound cues on redirect** — considered and explicitly deferred in favor of a simple chat message (D-11). Could be revisited as UX polish later, but isn't blocking any requirement.
- **`Test1`'s final name/itemization/crafting recipe** — explicitly deferred; this phase only needs a Creative-menu-obtainable placeholder to prove the mechanism (D-05).
- **Removing Phase 1's debug commands** — Phase 1's CONTEXT.md (D-02) says they're deleted once the real redirect lands; whether that removal happens inside this phase's plan or a small follow-up should be confirmed during planning, not assumed here.

</deferred>

---

*Phase: 02-summon-item-redirect-entry-registry*
*Context gathered: 2026-08-13*
