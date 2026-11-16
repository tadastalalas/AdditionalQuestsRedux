using System;
using System.Reflection;
using AdditionalQuestsRedux.Core;
using AdditionalQuestsRedux.Diagnostics;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Issues;

namespace AdditionalQuestsRedux.Behaviors;

// AQ-FEATURE: Vanilla AI lord rewards: an AI lord who resolves a vanilla issue gets its gold reward, renown and relation; the notable issue giver gains power | gate: IsVanillaAiLordRewardsEnabled
public class VanillaAiLordRewardsBehavior : CampaignBehaviorBase
{
    // Namespaces of vanilla issue classes. Our own issues are rewarded by their own behaviors.
    private static readonly string[] VanillaNamespacePrefixes = ["TaleWorlds.", "SandBox.", "StoryMode."];

    // IssueBase.RewardGold is protected; re-verify the name in dnSpy after game updates.
    private const string RewardGoldPropertyName = "RewardGold";

    private static Func<IssueBase, int>? _getRewardGold;
    private static bool _rewardGoldResolved;

    public override void RegisterEvents() => CampaignEvents.OnIssueUpdatedEvent.AddNonSerializedListener(this, OnIssueUpdated);

    // SyncData keys: none.
    public override void SyncData(IDataStore dataStore)
    {
    }

    // Vanilla fires this before finalizing the issue, so the issue owner is still valid.
    private void OnIssueUpdated(IssueBase issue, IssueBase.IssueUpdateDetails details, Hero? issueSolver)
    {
        if (details != IssueBase.IssueUpdateDetails.IssueFinishedByAILord || issue == null || issueSolver == null
            || !QuestRules.IsVanillaAiLordRewardsEnabled || !IsVanillaIssue(issue))
            return;

        int gold = GetRewardGold()?.Invoke(issue) ?? 0;
        if (gold > 0)
            GiveGoldAction.ApplyBetweenCharacters(null, issueSolver, gold, true);
        issueSolver.Clan?.AddRenown(QuestBalance.VanillaAiLordRenown, false);

        Hero? owner = issue.IssueOwner;
        if (owner != null && owner != issueSolver)
        {
            ChangeRelationAction.ApplyRelationChangeBetweenHeroes(issueSolver, owner, QuestBalance.VanillaAiLordRelation, false);
            if (owner.IsNotable)
                owner.AddPower(QuestBalance.VanillaAiLordIssueOwnerPower);
        }
        AqLog.Debug($"{issueSolver.Name} resolved {issue.GetType().Name} and got {gold} gold.");
    }

    private static bool IsVanillaIssue(IssueBase issue)
    {
        string? ns = issue.GetType().Namespace;
        if (ns == null)
            return false;
        for (int i = 0; i < VanillaNamespacePrefixes.Length; i++)
        {
            if (ns.StartsWith(VanillaNamespacePrefixes[i], StringComparison.Ordinal))
                return true;
        }
        return false;
    }

    // Resolved once on first use; a virtual-call delegate, so each issue's own RewardGold override runs.
    private static Func<IssueBase, int>? GetRewardGold()
    {
        if (_rewardGoldResolved)
            return _getRewardGold;
        _rewardGoldResolved = true;

        MethodInfo? getter = AccessTools.PropertyGetter(typeof(IssueBase), RewardGoldPropertyName);
        if (getter == null)
        {
            AqLog.Debug($"IssueBase.{RewardGoldPropertyName} not found; AI lords get no gold for vanilla quests.");
            return null;
        }
        _getRewardGold = AccessTools.MethodDelegate<Func<IssueBase, int>>(getter);
        return _getRewardGold;
    }
}
