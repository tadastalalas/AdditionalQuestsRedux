using System;
using System.Reflection;
using AdditionalQuestsRedux.Behaviors;
using AdditionalQuestsRedux.Core;
using AdditionalQuestsRedux.Diagnostics;
using HarmonyLib;
using TaleWorlds.CampaignSystem.Issues;

namespace AdditionalQuestsRedux.Compat;

// AQ-FEATURE: Governors Handle Issues compat: GHI governors resolve our quests while they are enabled in the settings and get the quest rewards; players never get a companion option | gate: GHI installed + IsBanditArmyEnabled/IsMilitiaSpearsEnabled/IsStarvingTownEnabled/IsLongDistanceCaravanEnabled | Prefix+Finalizer+Postfix: mark the issue GHI is resolving; follow our quest switches
internal static class GhiCompat
{
    // GHI names; re-verify against GHI in dnSpy after every GHI update.
    private const string SettingsTypeName = "GovernorsHandleIssues.GHISettings";
    private const string AutoResolveEnabledMethodName = "AutoResolveEnabledForQuestType";
    private const string BehaviorTypeName = "GovernorsHandleIssues.GovernorsHandleIssuesCampaignBehavior";
    private const string QuestResolutionMethodName = "DoQuestResolutionMode";

    private static bool _applied;
    private static IssueBase? _resolvingIssue;

    // True only while GHI's DoQuestResolutionMode runs for this exact issue.
    internal static bool IsHandling(IssueBase issue) => _resolvingIssue == issue;

    // Called from SubModule once all modules are loaded.
    internal static void TryApply(Harmony harmony)
    {
        if (_applied)
            return;
        _applied = true;

        Type? settingsType = AccessTools.TypeByName(SettingsTypeName);
        Type? behaviorType = AccessTools.TypeByName(BehaviorTypeName);
        if (settingsType == null && behaviorType == null)
        {
            AqLog.Debug("Governors Handle Issues not found, compatibility patch not applied.");
            return;
        }

        MethodInfo? autoResolveEnabled = settingsType == null ? null : AccessTools.Method(settingsType, AutoResolveEnabledMethodName, [typeof(IssueBase)]);
        MethodInfo? questResolution = behaviorType == null ? null : AccessTools.Method(behaviorType, QuestResolutionMethodName);
        if (autoResolveEnabled == null || questResolution == null)
        {
            AqLog.Warn($"Governors Handle Issues found, but {SettingsTypeName}.{AutoResolveEnabledMethodName} or {BehaviorTypeName}.{QuestResolutionMethodName} is missing; compatibility patch not applied.");
            return;
        }

        harmony.Patch(questResolution,
            prefix: new HarmonyMethod(typeof(GhiCompat), nameof(QuestResolutionPrefix)),
            finalizer: new HarmonyMethod(typeof(GhiCompat), nameof(QuestResolutionFinalizer)));
        harmony.Patch(autoResolveEnabled, postfix: new HarmonyMethod(typeof(GhiCompat), nameof(AutoResolveEnabledPostfix)));
        AqLog.Debug("Governors Handle Issues found, compatibility patch applied.");
    }

    // Void prefix: never skips GHI, only records which issue it is resolving.
    // __args instead of a named parameter, so a renamed GHI parameter cannot break patching.
    private static void QuestResolutionPrefix(object[] __args) => _resolvingIssue = FindIssue(__args);

    // Runs even if GHI throws, so the marker never leaks into the player's dialogs.
    private static void QuestResolutionFinalizer() => _resolvingIssue = null;

    // GHI returns true for issue types it does not know; disabled quests are left alone.
    private static void AutoResolveEnabledPostfix(object[] __args, ref bool __result)
    {
        IssueBase? issue = FindIssue(__args);
        if (issue != null && IsOurQuestDisabled(issue))
            __result = false;
    }

    private static IssueBase? FindIssue(object[]? args)
    {
        if (args == null)
            return null;
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] is IssueBase issue)
                return issue;
        }
        return null;
    }

    private static bool IsOurQuestDisabled(IssueBase issue) => issue switch
    {
        BanditArmyIssueBehavior.VillageBanditArmyRaidIssue => !QuestRules.IsBanditArmyEnabled,
        MilitiaSpearsIssueBehavior.HeadmanNeedsMilitiaWeaponsIssue => !QuestRules.IsMilitiaSpearsEnabled,
        StarvingTownIssueBehavior.StarvingTownNeedsFoodIssue => !QuestRules.IsStarvingTownEnabled,
        LongDistanceCaravanIssueBehavior.LongDistanceCaravanIssue => !QuestRules.IsLongDistanceCaravanEnabled,
        _ => false,
    };
}
