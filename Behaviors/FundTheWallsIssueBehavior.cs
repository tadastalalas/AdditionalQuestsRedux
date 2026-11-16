using System;
using AdditionalQuestsRedux.Core;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Issues;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace AdditionalQuestsRedux.Behaviors;

// AQ-FEATURE: Fund the Walls quest: a lord asks for gold for the construction reserve of his town or castle; the player hands it over there for relation, renown, influence and Generosity | gate: IsFundTheWallsEnabled
// AQ-FEATURE: AI lord rewards: an AI lord of the same realm entering that settlement may pay the gold himself and gets the same relation, renown and influence | gate: IsAiLordRewardsEnabled
public partial class FundTheWallsIssueBehavior : CampaignBehaviorBase
{
    // Vanilla menus: town and castle inside, and the gate menus when the player is not let in.
    private const string TownMenuId = "town";
    private const string CastleMenuId = "castle";
    private const string TownGuardMenuId = "town_guard";
    private const string CastleGuardMenuId = "castle_guard";
    private const string HandOverOptionId = "aq_fund_the_walls_hand_over";
    // The menu API takes the raw string; AMOUNT is set in the condition callback.
    private const string HandOverOptionText = Texts.Ids.FundTheWallsHandOverOption;
    private const int SettlementMenuOptionIndex = 0; // vanilla garrison troops quest: 0 in the town menu, 2 in the gate menus
    private const int GuardMenuOptionIndex = 2;

    public override void RegisterEvents()
    {
        CampaignEvents.OnCheckForIssueEvent.AddNonSerializedListener(this, OnCheckForIssue);
        CampaignEvents.OnIssueUpdatedEvent.AddNonSerializedListener(this, OnIssueUpdated);
        CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
        CampaignEvents.SettlementEntered.AddNonSerializedListener(this, OnSettlementEntered);
    }

    // SyncData keys: none. Issue and quest state is saved on the issue/quest objects (Data\QuestSaveDefiner).
    public override void SyncData(IDataStore dataStore)
    {
    }

    private void OnCheckForIssue(Hero hero)
    {
        if (!QuestRules.IsFundTheWallsEnabled)
            return;

        Settlement? settlement = ConditionsHold(hero) ? FindSettlementToFund(hero.Clan) : null;
        Campaign.Current.IssueManager.AddPotentialIssueData(hero, settlement != null
            ? new PotentialIssueData(OnIssueSelected, typeof(FundTheWallsIssue), IssueBase.IssueFrequency.Rare, settlement)
            : new PotentialIssueData(typeof(FundTheWallsIssue), IssueBase.IssueFrequency.Rare));
    }

    // The owner clan's leader asks, never a governor: governors change, the owner is checked by the issue and the quest.
    private static bool ConditionsHold(Hero issueGiver) =>
        issueGiver.IsLord && !issueGiver.IsPrisoner && issueGiver.Clan is { } clan && clan.Leader == issueGiver
        && clan != Clan.PlayerClan && !clan.IsMinorFaction;

    // The clan's town or castle with the lowest construction reserve, among those building something.
    private static Settlement? FindSettlementToFund(Clan clan)
    {
        Settlement? result = null;
        int lowestReserve = QuestBalance.FundWallsMaxReserve;
        MBReadOnlyList<Settlement> settlements = clan.Settlements;
        for (int i = 0; i < settlements.Count; i++)
        {
            Settlement settlement = settlements[i];
            Town? town = settlement.Town;
            if (!settlement.IsFortification || town == null || settlement.IsUnderSiege || town.BuildingsInProgress.Count == 0
                || town.BoostBuildingProcess >= lowestReserve)
                continue;
            lowestReserve = town.BoostBuildingProcess;
            result = settlement;
        }
        return result;
    }

    private IssueBase OnIssueSelected(in PotentialIssueData pid, Hero issueOwner) =>
        new FundTheWallsIssue(issueOwner, (Settlement)pid.RelatedObject);

    // Vanilla fires this before finalizing the issue, so the issue owner and settlement are still valid.
    private void OnIssueUpdated(IssueBase issue, IssueBase.IssueUpdateDetails details, Hero? issueSolver)
    {
        if (details == IssueBase.IssueUpdateDetails.IssueFinishedByAILord && issueSolver != null && QuestRules.IsAiLordRewardsEnabled
            && issue is FundTheWallsIssue fundIssue)
            fundIssue.ApplyAiLordRewards(issueSolver);
    }

    // Vanilla AI lords only resolve notables' issues (IssuesCampaignBehavior.OnSettlementEntered uses
    // IssueManager.GetIssuesInSettlement, notables only). This gives lords' Fund the Walls issues the same rule and chances.
    private void OnSettlementEntered(MobileParty? party, Settlement settlement, Hero? hero)
    {
        if (party == null || party.IsMainParty || party.Army != null || !settlement.IsFortification
            || !QuestRules.IsAiLordRewardsEnabled || !Campaign.Current.GameStarted)
            return;

        Hero? solver = party.LeaderHero;
        if (solver == null || !solver.IsLord || solver.Clan == null || solver.Clan == Clan.PlayerClan || solver.Clan.IsMinorFaction)
            return;
        if (settlement.OwnerClan?.Leader?.Issue is not FundTheWallsIssue issue || !issue.IsOngoingWithoutQuest || issue.Settlement != settlement)
            return;

        float chance = settlement.OwnerClan == solver.Clan ? QuestBalance.FundWallsAiOwnClanChance : QuestBalance.FundWallsAiOtherClanChance;
        if (MBRandom.RandomFloat < chance && issue.CanBeFundedBy(solver))
            issue.CompleteIssueWithAiLord(solver);
    }

    private void OnSessionLaunched(CampaignGameStarter starter)
    {
        starter.AddGameMenuOption(TownMenuId, HandOverOptionId, HandOverOptionText, aq_fund_the_walls_hand_over_on_condition, aq_fund_the_walls_hand_over_on_consequence, isLeave: false, SettlementMenuOptionIndex);
        starter.AddGameMenuOption(CastleMenuId, HandOverOptionId, HandOverOptionText, aq_fund_the_walls_hand_over_on_condition, aq_fund_the_walls_hand_over_on_consequence, isLeave: false, SettlementMenuOptionIndex);
        starter.AddGameMenuOption(TownGuardMenuId, HandOverOptionId, HandOverOptionText, aq_fund_the_walls_hand_over_on_condition, aq_fund_the_walls_hand_over_on_consequence, isLeave: false, GuardMenuOptionIndex);
        starter.AddGameMenuOption(CastleGuardMenuId, HandOverOptionId, HandOverOptionText, aq_fund_the_walls_hand_over_on_condition, aq_fund_the_walls_hand_over_on_consequence, isLeave: false, GuardMenuOptionIndex);
    }

    private bool aq_fund_the_walls_hand_over_on_condition(MenuCallbackArgs args)
    {
        FundTheWallsQuest? quest = FindQuest(Settlement.CurrentSettlement);
        if (quest == null)
            return false;

        args.optionLeaveType = GameMenuOption.LeaveType.Trade;
        args.OptionQuestData = GameMenuOption.IssueQuestFlags.ActiveIssue;
        args.Text?.SetTextVariable(Texts.AmountVariable, quest.RequestedGold).WithGoldIcon();
        if (Hero.MainHero.Gold < quest.RequestedGold)
        {
            args.IsEnabled = false;
            args.Tooltip = Texts.FundTheWallsNotEnoughGold;
        }
        return true;
    }

    private void aq_fund_the_walls_hand_over_on_consequence(MenuCallbackArgs args)
    {
        FindQuest(Settlement.CurrentSettlement)?.HandOverGold();
        args.MenuContext?.Refresh();
    }

    private static FundTheWallsQuest? FindQuest(Settlement? settlement)
    {
        if (settlement == null)
            return null;
        MBReadOnlyList<QuestBase> quests = Campaign.Current.QuestManager.Quests;
        for (int i = 0; i < quests.Count; i++)
        {
            if (quests[i] is FundTheWallsQuest quest && quest.IsOngoing && quest.Settlement == settlement)
                return quest;
        }
        return null;
    }

    // Scales with prosperity inside the castle or town range, rounded to FundWallsGoldStep.
    internal static int GetRequestedGold(Town town)
    {
        float scale = town.IsCastle
            ? GetScale(town.Prosperity, QuestBalance.FundWallsCastleMinProsperity, QuestBalance.FundWallsCastleMaxProsperity)
            : GetScale(town.Prosperity, QuestBalance.FundWallsTownMinProsperity, QuestBalance.FundWallsTownMaxProsperity);
        float gold = town.IsCastle
            ? QuestBalance.FundWallsCastleMinGold + scale * (QuestBalance.FundWallsCastleMaxGold - QuestBalance.FundWallsCastleMinGold)
            : QuestBalance.FundWallsTownMinGold + scale * (QuestBalance.FundWallsTownMaxGold - QuestBalance.FundWallsTownMinGold);
        return (int)Math.Round(gold / QuestBalance.FundWallsGoldStep) * QuestBalance.FundWallsGoldStep;
    }

    private static float GetScale(float value, float min, float max) => Math.Max(0f, Math.Min(1f, (value - min) / (max - min)));

    private static float GetRewardScale(int gold) => GetScale(gold, QuestBalance.FundWallsCastleMinGold, QuestBalance.FundWallsTownMaxGold);

    private static int GetRelationReward(int gold) =>
        (int)Math.Round(QuestBalance.FundWallsMinRelation + GetRewardScale(gold) * (QuestBalance.FundWallsMaxRelation - QuestBalance.FundWallsMinRelation));

    private static float GetRenownReward(int gold) =>
        QuestBalance.FundWallsMinRenown + GetRewardScale(gold) * (QuestBalance.FundWallsMaxRenown - QuestBalance.FundWallsMinRenown);

    // The giver pays first (null recipient: the gold leaves the game), then the reserve grows; same as vanilla BuildingHelper.BoostBuildingProcessWithGold.
    private static void PayIntoReserve(Hero giver, Town town, int gold)
    {
        GiveGoldAction.ApplyBetweenCharacters(giver, null, gold, giver != Hero.MainHero);
        town.BoostBuildingProcess += gold;
    }

    // Vanilla denars-to-influence rate, only for a clan of the settlement's own kingdom.
    private static void ApplyInfluenceReward(Hero hero, Settlement settlement, int gold)
    {
        if (hero.Clan?.Kingdom == null || hero.MapFaction != settlement.MapFaction)
            return;
        GainKingdomInfluenceAction.ApplyForDefault(hero, gold * Campaign.Current.Models.DiplomacyModel.DenarsToInfluence());
    }
}
