using AdditionalQuestsRedux.Core;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Issues;
using TaleWorlds.CampaignSystem.Settlements;

namespace AdditionalQuestsRedux.Behaviors;

// AQ-FEATURE: Militia Spears quest: a village notable with a small militia asks for one-handed polearms | gate: IsMilitiaSpearsEnabled
// AQ-FEATURE: AI lord rewards: an AI lord who resolves this issue gets the player's rewards and the village gets the same benefits | gate: IsAiLordRewardsEnabled
public partial class MilitiaSpearsIssueBehavior : CampaignBehaviorBase
{
    public override void RegisterEvents()
    {
        CampaignEvents.OnCheckForIssueEvent.AddNonSerializedListener(this, OnCheckForIssue);
        CampaignEvents.OnIssueUpdatedEvent.AddNonSerializedListener(this, OnIssueUpdated);
    }

    // SyncData keys: none. Issue and quest state is saved on the issue/quest objects (Data\QuestSaveDefiner).
    public override void SyncData(IDataStore dataStore)
    {
    }

    private void OnCheckForIssue(Hero hero)
    {
        if (!QuestRules.IsMilitiaSpearsEnabled)
            return;

        Campaign.Current.IssueManager.AddPotentialIssueData(hero, ConditionsHold(hero)
            ? new PotentialIssueData(OnIssueSelected, typeof(HeadmanNeedsMilitiaWeaponsIssue), IssueBase.IssueFrequency.VeryCommon)
            : new PotentialIssueData(typeof(HeadmanNeedsMilitiaWeaponsIssue), IssueBase.IssueFrequency.VeryCommon));
    }

    private static bool ConditionsHold(Hero issueGiver) =>
        issueGiver.IsRuralNotable && issueGiver.CurrentSettlement is { IsVillage: true } village && village.Militia < QuestBalance.MilitiaSpearsMaxVillageMilitia;

    private IssueBase OnIssueSelected(in PotentialIssueData pid, Hero issueOwner) => new HeadmanNeedsMilitiaWeaponsIssue(issueOwner);

    // Vanilla fires this before finalizing the issue, so the issue owner and settlement are still valid.
    private void OnIssueUpdated(IssueBase issue, IssueBase.IssueUpdateDetails details, Hero? issueSolver)
    {
        if (details == IssueBase.IssueUpdateDetails.IssueFinishedByAILord && issueSolver != null && QuestRules.IsAiLordRewardsEnabled
            && issue is HeadmanNeedsMilitiaWeaponsIssue militiaSpearsIssue)
            militiaSpearsIssue.ApplyAiLordRewards(issueSolver);
    }

    // Shared by the player quest, the AI lord path and Governors Handle Issues.
    private static void ApplySuccessWorldEffects(Hero issueOwner)
    {
        issueOwner.AddPower(QuestBalance.MilitiaSpearsSuccessPower);
        Settlement? village = issueOwner.CurrentSettlement;
        if (village?.Village == null)
            return;
        village.Village.Bound.Town.Prosperity += QuestBalance.MilitiaSpearsSuccessProsperity;
        village.Militia += QuestBalance.MilitiaSpearsSuccessMilitia;
    }

    private static void ApplyFailWorldEffects(Hero issueOwner)
    {
        issueOwner.AddPower(QuestBalance.MilitiaSpearsFailPower);
        if (issueOwner.CurrentSettlement?.Village is { } village)
            village.Bound.Town.Prosperity += QuestBalance.MilitiaSpearsFailProsperity;
    }
}
