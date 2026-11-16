using AdditionalQuestsRedux.Core;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Issues;
using TaleWorlds.CampaignSystem.Settlements;

namespace AdditionalQuestsRedux.Behaviors;

// AQ-FEATURE: Long Distance Caravan quest: a rich town merchant sends a loaded caravan to a far town; the escort gets half of the real trade profit | gate: IsLongDistanceCaravanEnabled
// AQ-FEATURE: AI lord rewards: an AI lord who resolves this issue gets the player's rewards and the merchant runs the same trade | gate: IsAiLordRewardsEnabled
public partial class LongDistanceCaravanIssueBehavior : CampaignBehaviorBase
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
        if (!QuestRules.IsLongDistanceCaravanEnabled)
            return;

        Town? destination = ConditionsHold(hero) ? FindDestination(hero.CurrentSettlement.Town, true) : null;
        Campaign.Current.IssueManager.AddPotentialIssueData(hero, destination != null
            ? new PotentialIssueData(OnIssueSelected, typeof(LongDistanceCaravanIssue), IssueBase.IssueFrequency.Rare, destination.Settlement)
            : new PotentialIssueData(typeof(LongDistanceCaravanIssue), IssueBase.IssueFrequency.Rare));
    }

    // Vanilla escort caravans skip port towns too: their caravans may be naval.
    private static bool ConditionsHold(Hero issueGiver) =>
        issueGiver.IsMerchant && issueGiver.CurrentSettlement is { IsTown: true, HasPort: false, IsStarving: false, IsUnderSiege: false } && HasEnoughBudget(issueGiver);

    private IssueBase OnIssueSelected(in PotentialIssueData pid, Hero issueOwner) =>
        new LongDistanceCaravanIssue(issueOwner, (Settlement)pid.RelatedObject);

    // Vanilla fires this before finalizing the issue, so the issue owner and settlement are still valid.
    private void OnIssueUpdated(IssueBase issue, IssueBase.IssueUpdateDetails details, Hero? issueSolver)
    {
        if (details == IssueBase.IssueUpdateDetails.IssueFinishedByAILord && issueSolver != null && QuestRules.IsAiLordRewardsEnabled
            && issue is LongDistanceCaravanIssue caravanIssue)
            caravanIssue.ApplyAiLordRewards(issueSolver);
    }

    // Shared by the player quest, the AI lord path and Governors Handle Issues.
    private static void ApplySuccessWorldEffects(Hero issueOwner)
    {
        issueOwner.AddPower(QuestBalance.LongCaravanSuccessPower);
        if (issueOwner.CurrentSettlement?.Town is { } town)
            town.Prosperity += QuestBalance.LongCaravanSuccessProsperity;
    }

    private static void ApplyFailWorldEffects(Hero issueOwner, float power, float prosperity)
    {
        issueOwner.AddPower(power);
        if (issueOwner.CurrentSettlement?.Town is { } town)
            town.Prosperity += prosperity;
    }

    // The merchant pays the share out of the profit the trade just made.
    private static void PayProfitShare(Hero merchant, Hero recipient, int profit)
    {
        int share = GetProfitShare(profit);
        if (share > 0)
            GiveGoldAction.ApplyBetweenCharacters(merchant, recipient, share);
    }

    private static int GetProfitShare(int profit) => profit > 0 ? (int)(profit * QuestBalance.LongCaravanPlayerProfitShare) : 0;
}
