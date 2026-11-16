using AdditionalQuestsRedux.Core;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Issues;
using TaleWorlds.CampaignSystem.Settlements;

namespace AdditionalQuestsRedux.Behaviors;

// AQ-FEATURE: Bandit Army quest: a village notable near an infested hideout asks the player to stop a bandit army raid | gate: IsBanditArmyEnabled
// AQ-FEATURE: AI lord rewards: an AI lord who resolves this issue gets the player's rewards and the village gets the same benefits | gate: IsAiLordRewardsEnabled
public partial class BanditArmyIssueBehavior : CampaignBehaviorBase
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
        if (!QuestRules.IsBanditArmyEnabled || !hero.IsNotable)
            return;

        Settlement? hideout = ConditionsHold(hero)
            ? QuestHelpers.FindNearestInfestedHideout(hero.CurrentSettlement, QuestBalance.BanditArmyHideoutMaxDistance)
            : null;
        Campaign.Current.IssueManager.AddPotentialIssueData(hero, hideout != null
            ? new PotentialIssueData(OnIssueSelected, typeof(VillageBanditArmyRaidIssue), IssueBase.IssueFrequency.Common, hideout)
            : new PotentialIssueData(typeof(VillageBanditArmyRaidIssue), IssueBase.IssueFrequency.Common));
    }

    // A looted or currently raided village cannot be the raid target.
    private static bool ConditionsHold(Hero issueGiver) =>
        issueGiver.IsRuralNotable && issueGiver.CurrentSettlement is { IsVillage: true, IsRaided: false, IsUnderRaid: false };

    private IssueBase OnIssueSelected(in PotentialIssueData pid, Hero issueOwner) => new VillageBanditArmyRaidIssue(issueOwner, (Settlement)pid.RelatedObject);

    // Vanilla fires this before finalizing the issue, so the issue owner and settlement are still valid.
    private void OnIssueUpdated(IssueBase issue, IssueBase.IssueUpdateDetails details, Hero? issueSolver)
    {
        if (details == IssueBase.IssueUpdateDetails.IssueFinishedByAILord && issueSolver != null && QuestRules.IsAiLordRewardsEnabled
            && issue is VillageBanditArmyRaidIssue banditArmyIssue)
            banditArmyIssue.ApplyAiLordRewards(issueSolver);
    }

    // Shared by the player quest, the AI lord path and Governors Handle Issues.
    private static void ApplySuccessWorldEffects(Hero issueOwner)
    {
        issueOwner.AddPower(QuestBalance.BanditArmySuccessPower);
        if (issueOwner.CurrentSettlement?.Village is { } village)
            village.Hearth += QuestBalance.BanditArmySuccessHearth;
    }

    private static void ApplyFailWorldEffects(Hero issueOwner) => issueOwner.AddPower(QuestBalance.BanditArmyFailPower);
}
