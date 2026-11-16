using AdditionalQuestsRedux.Core;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Issues;
using TaleWorlds.CampaignSystem.Settlements;

namespace AdditionalQuestsRedux.Behaviors;

// AQ-FEATURE: Starving Town quest: a merchant in a starving town buys grain, fish and meat at triple price | gate: IsStarvingTownEnabled
// AQ-FEATURE: AI lord rewards: an AI lord who resolves this issue gets the player's rewards and the town gets the same benefits | gate: IsAiLordRewardsEnabled
public partial class StarvingTownIssueBehavior : CampaignBehaviorBase
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
        if (!QuestRules.IsStarvingTownEnabled)
            return;

        Campaign.Current.IssueManager.AddPotentialIssueData(hero, ConditionsHold(hero)
            ? new PotentialIssueData(OnIssueSelected, typeof(StarvingTownNeedsFoodIssue), IssueBase.IssueFrequency.VeryCommon)
            : new PotentialIssueData(typeof(StarvingTownNeedsFoodIssue), IssueBase.IssueFrequency.VeryCommon));
    }

    private static bool ConditionsHold(Hero issueGiver) => issueGiver.IsMerchant && issueGiver.CurrentSettlement is { IsTown: true, IsStarving: true };

    private IssueBase OnIssueSelected(in PotentialIssueData pid, Hero issueOwner) => new StarvingTownNeedsFoodIssue(issueOwner);

    // Vanilla fires this before finalizing the issue, so the issue owner and settlement are still valid.
    private void OnIssueUpdated(IssueBase issue, IssueBase.IssueUpdateDetails details, Hero? issueSolver)
    {
        if (details == IssueBase.IssueUpdateDetails.IssueFinishedByAILord && issueSolver != null && QuestRules.IsAiLordRewardsEnabled
            && issue is StarvingTownNeedsFoodIssue starvingTownIssue)
            starvingTownIssue.ApplyAiLordRewards(issueSolver);
    }

    // Shared by the player quest, the AI lord path and Governors Handle Issues.
    private static void ApplySuccessWorldEffects(Hero issueOwner)
    {
        issueOwner.AddPower(QuestBalance.StarvingTownSuccessPower);
        Settlement? town = issueOwner.CurrentSettlement;
        if (town?.Town == null)
            return;
        town.Town.FoodStocks += QuestBalance.StarvingTownFoodStocksBonus;
        town.Town.Prosperity += QuestBalance.StarvingTownSuccessProsperity;
    }

    private static void ApplyFailWorldEffects(Hero issueOwner)
    {
        issueOwner.AddPower(QuestBalance.StarvingTownFailPower);
        if (issueOwner.CurrentSettlement?.Town is { } town)
            town.Prosperity += QuestBalance.StarvingTownFailProsperity;
    }
}
