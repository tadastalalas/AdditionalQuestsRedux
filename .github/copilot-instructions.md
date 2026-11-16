---
apply: always
---

# AdditionalQuestsRedux: workspace instructions (they override the global file)

## Start here
- Read `FEATURES.md` (repo root) first. It maps every feature to its gate (setting) and files, so open only the files you need.
- Keep `FEATURES.md` up to date in the same change set.

## Identity
- <Prefix>: AQ (`Aq` in type names). <Root> namespace and ModuleId: AdditionalQuestsRedux. ModuleName: Additional Quests Redux.
- Logger: `Diagnostics\AqLog.cs` (Debug / Warn / Error; `AqLog.IsDebugEnabled` before building strings on hot paths).
- Settings: `Core\ModSettings.cs` only (MCM when loaded, else `ModConfiguration.xml`); gameplay code reads them through the
  read-only facade `Core\QuestRules.cs` (IsX). Gameplay numbers: `Core\QuestBalance.cs`.
- `Core\ModInfo.cs` holds the ModuleId (also the Harmony id), file and folder names and the module root path.
- Harmony: no attribute patches and no `PatchAll`. The only patch is the optional-mod compat `Compat\GhiCompat.TryApply`
  (Governors Handle Issues, `AccessTools.TypeByName`), called once from `SubModule.OnBeforeInitialModuleScreenSetAsRoot`.
  Its targets are GHI methods, not vanilla: re-verify them against GHI in dnSpy after every GHI update.
  `ModSettings.Reload` runs later (`OnGameStart`), so GhiCompat debug lines are written only once a campaign has started; warnings always.

## Layout deviations
- `Diagnostics\ConsoleCommands.cs`: cheat/test console commands (`additional_quests.*`). Their output stays English (not in Texts).
- `Core\QuestHelpers.cs`, `Core\QuestText.cs` (`WithCharacter`), `Core\DialogTokens.cs`: helpers shared by all quests.
- Each quest is one partial behavior: `Behaviors\<Quest>IssueBehavior.cs` (behavior + AI paths), `.Issue.cs` (nested IssueBase),
  `.Quest.cs` (nested QuestBase); Long Distance Caravan also has `.Trade.cs`.

## Multi-version (1.5.7, 1.4.8, 1.4.7)
- 1.4.7 / 1.4.8 behavior is the reference. 1.5.7 adaptations live only in `#else` of `#if LOWER_THAN_1_5` and must behave the same.
- The `#if` blocks are listed per file in `DevChangelog.md` (AQ v1.5.7.0).

## Saves
- No SyncData (every behavior's SyncData is empty).
- `Data\QuestSaveDefiner.cs`, base id 585820. Used ids (issue/quest pairs): 1/2 Militia Spears, 31/32 Starving Town, 51/52 Bandit Army,
  61/62 Long Distance Caravan, 71/72 Fund the Walls, 1001/1002 Donate Elite Rangers. Reserved, never reuse: 41/42 (removed Town Uprising),
  81/82 (clashed with another mod). Append only.

## Kept exceptions (released identifiers, never change)
- MCM Id and FolderName "AdditionalQuestsRedux" (equal to the ModuleId), MCM FormatType "json", MCM property names in `Data\MCMSettings.cs`.
- Behavior class names (data-store ids) and nested issue/quest class names (saved types).
- Text ids without a random suffix (`AQBA…`, `AQMS…`, `AQST…`, `AQGeneric…`, …). New ids use `AQ_` + 7 random characters.
- Vanilla text ids reused with the exact vanilla text (e.g. `zS68eOsl`, `JIWVeTMD`, `bqeWVVEE`) have no rows in our language files.
