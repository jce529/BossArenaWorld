# Milestones

## v1.0 MVP (Shipped: 2026-08-15)

**Phases completed:** 10 phases, 36 plans, 76 tasks

**Key accomplishments:**

- Three debug ModCommands (/bossarena-enter, /bossarena-exit, /bossarena-checkflag) make the Plan 02 subworld reachable and testable in-game, plus a generic ForceZone/PostUpdate infrastructure hook (no active biome mapping yet) for Phase 3+ per-boss zone overrides.
- Live King Slime kill test shows `NPC.downedSlimeKing = True` after the subworld round-trip -- the opposite of the expected/required `False`, contradicting the isolation premise the entire Phase 3+ carrier-item architecture depends on.
- BossRegistry ModSystem (idempotent Apply/TryGetKeyForNpc, vanilla:king_slime via NPC.SetEventFlagCleared) and BossCoreItem carrier item (BossKey persisted across Clone/SaveData/LoadData, UseItem wired to Apply) -- both compile clean against installed tModLoader 1.4.4.9
- Custom IItemDropRule (BossCoreDropRule) gating BossCoreItem drops per-kill on SubworldSystem.IsActive<BossArenaSubworld>(), wired to every BossRegistry-registered NPC type via BossKillGlobalNPC.ModifyNPCLoot -- completes the compile-time kill-to-carrier-item pipeline
- Live King Slime kill/carry/apply cycle empirically confirms all 5 Phase 3 Success Criteria and DROP-02/DROP-03/APPLY-01/APPLY-04 -- the subworld-gated drop, cross-world BossKey survival, correct flag application, and idempotent re-use all work end-to-end against a backed-up world save
- Two live in-game checkpoints closing out Phase 4: confirmed Hive Mind's real WorldGen/netcode/messaging side effects fire correctly via the carrier-item pipeline, and found + fixed a real JIT-crash bug in the CalamityMod isolation boundary during the Calamity-disabled load-safety test.
- Live in-game checkpoints confirm Infernon's downed-flag + HellstoneBrick WorldGen tile-ring replay actually fire on a real kill/carry/apply cycle, and that BossArenaSubWorld loads and runs safely with SpiritMod disabled -- closing out Phase 5's remaining Success Criteria; the reflection-failure robustness checkpoint was explicitly and deliberately skipped as out of Success-Criterion scope.
- Closed by citation: this checkpoint's live verification was performed and recorded under Phase 8's `08-02-PLAN.md` rather than independently under this plan, since Phase 8 execution (2026-08-14) happened before this plan had ever run on its own and `08-02` was explicitly written to cover this exact checkpoint "if not already done."
- ContinentOfJourney.DownedBossSystem.downedGoblinChariot registered into BossRegistry via direct public-static-field write, closing MOD-06 as the fifth and final v1 mod integration
- Live-confirmed Goblin Chariot's full subworld-kill-to-main-world-apply pipeline, Boss Checklist recognition, and ContinentOfJourney-disabled JIT safety -- closing MOD-06 and v1 mod coverage end-to-end.
- Boss Checklist confirmed operational; King Slime and Hive Mind's tracker-UI recognition explicitly confirmed for the first time; Infernon's existing Phase 5 confirmation cited without redundant re-test.
- Live-confirmed Thorn's and Astrageldon's full pipelines, Boss Checklist recognition, and the Moon-Lord-lockout eligibility delegate; live-confirmed Redemption-disabled and CatalystMod-disabled JIT safety. Also closes Phase 6's own outstanding 06-03 checkpoint (no prior independent execution existed).
- Closed by citation: Phase 7's own `07-02-PLAN.md` checkpoint executed first this session and already covers this plan's exact acceptance criteria, so no duplicate live test was performed.
- Two height-only boss-arena subworlds (Underworld, Space) satisfying ZoneUnderworldHeight/ZoneSkyHeight purely via platform Y-position, mirroring the BossArenaCorruptionSubworld template.
- Two vanilla tile-weighted boss-arena subworlds (Hallow, Jungle) satisfying ZoneHallow/ZoneJungle via TileID.Sets weight-table fills, avoiding Jungle's zero-weight-Mud pitfall.
- One vanilla tile-weighted boss-arena subworld (Desert) satisfying ZoneDesert's unusually high 1500-weight threshold via extra fill thickness; the Dungeon pair originally built alongside it was descoped and discarded.
- Two modded-ModBiome boss-arena subworlds (Astral Infection, Briar) satisfying ZoneAstral/InBriar via Calamity/Spirit's IsBiomeActive hook, with Calamity/Spirit type references confined exclusively to their paired GenPass classes for JIT safety; the Sulphurous Sea pair originally built alongside them was descoped and discarded.
- Live in-game confirmation that all 7 biome boss-arena subworlds (Hallow, Underworld, Jungle, Space, Desert, Astral, Briar) actually satisfy their real per-tick Zone/Biome flag on entry, across all three underlying mechanism families (vanilla SceneMetrics tile-weighting, height-only Y-position, and modded ModBiome tile-count hooks) -- with one live-discovered Desert crash fixed mid-checkpoint.
- Live JIT-safety checkpoint for Astral/Briar caught and fixed a real crash (missing `[JITWhenModsEnabled]` on both `GenPass.ApplyPass` overrides), then re-verified clean before deleting the temporary debug entry mechanism to restore D-02 compliance.

---
