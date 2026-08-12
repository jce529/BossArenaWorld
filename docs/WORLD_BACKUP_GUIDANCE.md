# World Backup Guidance

This guidance applies to live testing against a real save (starting Phase 4, when
content mods like Calamity/Spirit are enabled). Phase 1 does NOT require this
procedure — Phase 1 testing happens on a disposable, mod-free test world (see D-13
in `01-CONTEXT.md`).

## Default tModLoader Save Locations (Windows)

- Worlds path: `%UserProfile%\Documents\My Games\Terraria\tModLoader\Worlds\`
- Players path: `%UserProfile%\Documents\My Games\Terraria\tModLoader\Players\`
- On this machine, resolved:
  - `C:\Users\chang\Documents\My Games\Terraria\tModLoader\Worlds\`
  - `C:\Users\chang\Documents\My Games\Terraria\tModLoader\Players\`

## Backup Procedure (required before testing against a real save)

1. Before testing any phase against the real save (`HiPo's_Terrarium`), copy both the
   `.wld` file (and its `.bak` if present) from `Worlds\` and the corresponding `.plr`
   file from `Players\` to a separate, timestamped backup folder (e.g.
   `Worlds\_backups\2026-08-13_pre-phase4\`).
2. Confirm the backup copy is readable (file size > 0, not zero-byte) before
   proceeding with any live test.
3. Only after the backup is confirmed should live testing begin.

## Subworld File Location

Subworld files (when a `Subworld` has `ShouldSave = true`) are stored under
`Worlds\<main-world-UniqueId-GUID>\<ModName>_<SubworldClassName>.wld`. The Phase 1
boss-arena subworld (`BossArenaSubworld`) has `ShouldSave = false`, so no subworld
file is ever written for it — nothing to back up for the arena itself. Documented
for later phases in case any subworld's `ShouldSave` value changes.

## Phase 1 Testing Note

Phase 1's own isolation-proof test (King Slime kill/exit test) runs on a fresh,
disposable test world with only SubworldLibrary and this mod enabled — no other
content mods, and not the real save. No backup is required for that disposable
world. This guidance document exists now so it is ready to follow starting Phase 4.
