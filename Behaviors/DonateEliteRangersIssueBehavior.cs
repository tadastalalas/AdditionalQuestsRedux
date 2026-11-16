using AdditionalQuestsRedux.Core;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Issues;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace AdditionalQuestsRedux.Behaviors;

// AQ-FEATURE: Donate Elite Rangers quest: a lord asks for the best foot archers or crossbowmen of his kingdom's culture for the garrison of his town or castle; the player leaves them there for gold, relation, renown and influence | gate: IsDonateEliteRangersEnabled
// AQ-FEATURE: AI lord rewards: an AI lord of the same realm entering that settlement with enough of those troops may leave them in the garrison and gets the same rewards | gate: IsAiLordRewardsEnabled
public partial class DonateEliteRangersIssueBehavior : CampaignBehaviorBase
{
    // Vanilla menus: town and castle inside, and the gate menus when the player is not let in.
    private const string TownMenuId = "town";
    private const string CastleMenuId = "castle";
    private const string TownGuardMenuId = "town_guard";
    private const string CastleGuardMenuId = "castle_guard";
    private const string HandOverOptionId = "aq_donate_elite_rangers_hand_over";
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
        if (!QuestRules.IsDonateEliteRangersEnabled)
            return;

        CharacterObject? troop = ConditionsHold(hero) ? FindRangerTroop(hero.Clan) : null;
        Settlement? settlement = troop != null ? FindSettlementToGarrison(hero, troop) : null;
        Campaign.Current.IssueManager.AddPotentialIssueData(hero, settlement != null && troop != null
            ? new PotentialIssueData(OnIssueSelected, typeof(DonateEliteRangersIssue), IssueBase.IssueFrequency.Rare, new RangersIssueData(settlement, troop))
            : new PotentialIssueData(typeof(DonateEliteRangersIssue), IssueBase.IssueFrequency.Rare));
    }

    // The owner clan's leader asks, never a governor: governors change, the owner is checked by the issue and the quest.
    private static bool ConditionsHold(Hero issueGiver) =>
        issueGiver.IsLord && !issueGiver.IsPrisoner && issueGiver.Clan is { } clan && clan.Leader == issueGiver
        && clan != Clan.PlayerClan && !clan.IsMinorFaction;

    // The clan's town or castle with the smallest garrison that has room, food and wage budget for the troops.
    private static Settlement? FindSettlementToGarrison(Hero giver, CharacterObject troop)
    {
        Settlement? result = null;
        int smallestGarrison = int.MaxValue;
        MBReadOnlyList<Settlement> settlements = giver.Clan.Settlements;
        for (int i = 0; i < settlements.Count; i++)
        {
            Settlement settlement = settlements[i];
            if (!settlement.IsFortification || settlement.Town == null || settlement.IsUnderSiege)
                continue;
            int count = GetRangerCount(settlement);
            int garrisonCount = GetGarrisonCount(settlement.Town);
            if (garrisonCount >= smallestGarrison || giver.Gold < GetRewardGold(troop, count) + QuestBalance.RangersGiverMinGoldLeft
                || !CanGarrisonTake(settlement.Town, troop, count))
                continue;
            smallestGarrison = garrisonCount;
            result = settlement;
        }
        return result;
    }

    private static int GetGarrisonCount(Town town) => town.GarrisonParty?.MemberRoster.TotalManCount ?? 0;

    // Room under the garrison size limit, food stocks that do not start shrinking, and wage budget if the owner limits it.
    private static bool CanGarrisonTake(Town town, CharacterObject troop, int count)
    {
        float sizeLimit = Campaign.Current.Models.PartySizeLimitModel.CalculateGarrisonPartySizeLimit(town.Settlement).ResultNumber;
        if (GetGarrisonCount(town) + count > sizeLimit)
            return false;
        if (town.FoodChange - (float)count / Campaign.Current.Models.SettlementFoodModel.NumberOfMenOnGarrisonToEatOneFood < 0f)
            return false;
        MobileParty? garrison = town.GarrisonParty;
        return garrison == null || !garrison.HasLimitedWage() || garrison.GetAvailableWageBudget() >= count * troop.TroopWage;
    }

    // Highest-tier foot archer or crossbowman of the kingdom's culture (basic and noble trees); mounted archers only as fallback.
    internal static CharacterObject? FindRangerTroop(Clan clan)
    {
        CultureObject? culture = clan.Kingdom?.Culture ?? clan.Culture;
        if (culture == null)
            return null;
        CharacterObject? bestFoot = null;
        CharacterObject? bestAny = null;
        FindBestRanged(culture.BasicTroop, ref bestFoot, ref bestAny);
        FindBestRanged(culture.EliteBasicTroop, ref bestFoot, ref bestAny);
        return bestFoot ?? bestAny;
    }

    private static void FindBestRanged(CharacterObject? baseTroop, ref CharacterObject? bestFoot, ref CharacterObject? bestAny)
    {
        if (baseTroop == null)
            return;
        foreach (CharacterObject troop in CharacterHelper.GetTroopTree(baseTroop))
        {
            if (!troop.IsRanged)
                continue;
            if (IsBetter(troop, bestAny))
                bestAny = troop;
            if (!troop.IsMounted && IsBetter(troop, bestFoot))
                bestFoot = troop;
        }
    }

    private static bool IsBetter(CharacterObject troop, CharacterObject? current) =>
        current == null || troop.Tier > current.Tier || (troop.Tier == current.Tier && troop.Level > current.Level);

    internal static int GetRangerCount(Settlement settlement) =>
        settlement.IsCastle ? QuestBalance.RangersCastleCount : QuestBalance.RangersTownCount;

    // Vanilla garrison troops quest formula with a higher multiplier; recruit cost without a buyer's perks.
    internal static int GetRewardGold(CharacterObject troop, int count) =>
        (int)(QuestBalance.RangersBaseRewardGold
            + QuestBalance.RangersRecruitCostMultiplier * Campaign.Current.Models.PartyWageModel.GetTroopRecruitmentCost(troop, null).RoundedResultNumber * count);

    internal static int GetHealthyCount(TroopRoster roster, CharacterObject troop)
    {
        int index = roster.FindIndexOfTroop(troop);
        return index < 0 ? 0 : roster.GetElementNumber(index) - roster.GetElementWoundedNumber(index);
    }

    // Moves healthy troops from the party into the garrison, at the front of the roster: vanilla garrison desertion
    // takes troops from the end first. Fires the vanilla troops-given event like the party screen does.
    internal static bool TransferToGarrison(Hero giver, MobileParty party, Settlement settlement, CharacterObject troop, int count)
    {
        Town? town = settlement.Town;
        if (town == null || GetHealthyCount(party.MemberRoster, troop) < count)
            return false;
        if (town.GarrisonParty == null)
            settlement.AddGarrisonParty();
        MobileParty? garrison = town.GarrisonParty;
        if (garrison == null)
            return false;

        party.MemberRoster.AddToCounts(troop, -count);
        garrison.MemberRoster.AddToCounts(troop, count, insertAtFront: true);
        TroopRoster donated = TroopRoster.CreateDummyTroopRoster();
        donated.AddToCounts(troop, count);
        CampaignEventDispatcher.Instance.OnTroopGivenToSettlement(giver, settlement, donated);
        return true;
    }

    // Gold comes from the issue giver (clamped to his gold), then relation, renown and the vanilla leave-garrison influence.
    internal static void ApplyRewards(Hero solver, Hero issueGiver, Settlement settlement, CharacterObject troop, int count, int rewardGold, bool applyRelation)
    {
        GiveGoldAction.ApplyBetweenCharacters(issueGiver, solver, rewardGold);
        if (applyRelation)
            ChangeRelationAction.ApplyRelationChangeBetweenHeroes(solver, issueGiver, QuestBalance.RangersRelation, false);
        solver.Clan?.AddRenown(QuestBalance.RangersRenown, false);

        // Vanilla GarrisonTroopsCampaignBehavior.ApplyKingdomInfluenceBonusForLeavingTroopToGarrison: not for the owner clan.
        if (solver.Clan == null || solver.Clan == settlement.OwnerClan || solver.Clan.Kingdom == null || solver.MapFaction != settlement.MapFaction)
            return;
        float power = Campaign.Current.Models.MilitaryPowerModel.GetTroopPower(troop, BattleSideEnum.Defender, MapEvent.PowerCalculationContext.Siege, 0f);
        GainKingdomInfluenceAction.ApplyForLeavingTroopToGarrison(solver, power * count / QuestBalance.RangersInfluenceDivisor);
    }

    private IssueBase OnIssueSelected(in PotentialIssueData pid, Hero issueOwner)
    {
        RangersIssueData data = (RangersIssueData)pid.RelatedObject;
        return new DonateEliteRangersIssue(issueOwner, data.Settlement, data.Troop);
    }

    // Vanilla fires this before finalizing the issue, so the issue owner and settlement are still valid.
    private void OnIssueUpdated(IssueBase issue, IssueBase.IssueUpdateDetails details, Hero? issueSolver)
    {
        if (details == IssueBase.IssueUpdateDetails.IssueFinishedByAILord && issueSolver != null && QuestRules.IsAiLordRewardsEnabled
            && issue is DonateEliteRangersIssue rangersIssue)
            rangersIssue.ApplyAiLordRewards(issueSolver);
    }

    // Vanilla AI lords only resolve notables' issues. This gives lords' Donate Elite Rangers issues the same rule and chances.
    // The troops move before the issue completes, so the rewards handler only pays.
    private void OnSettlementEntered(MobileParty? party, Settlement settlement, Hero? hero)
    {
        if (party == null || party.IsMainParty || party.Army != null || !settlement.IsFortification
            || !QuestRules.IsAiLordRewardsEnabled || !Campaign.Current.GameStarted)
            return;

        Hero? solver = party.LeaderHero;
        if (solver == null || !solver.IsLord || solver.Clan == null || solver.Clan == Clan.PlayerClan || solver.Clan.IsMinorFaction)
            return;
        if (settlement.OwnerClan?.Leader?.Issue is not DonateEliteRangersIssue issue || !issue.IsOngoingWithoutQuest || issue.Settlement != settlement)
            return;

        float chance = settlement.OwnerClan == solver.Clan ? QuestBalance.RangersAiOwnClanChance : QuestBalance.RangersAiOtherClanChance;
        if (MBRandom.RandomFloat < chance && issue.CanBeDonatedBy(solver, party) && TransferToGarrison(solver, party, settlement, issue.Troop, issue.TroopCount))
            issue.CompleteIssueWithAiLord(solver);
    }

    private void OnSessionLaunched(CampaignGameStarter starter)
    {
        starter.AddGameMenuOption(TownMenuId, HandOverOptionId, Texts.Ids.DonateEliteRangersHandOverOption, aq_donate_elite_rangers_hand_over_on_condition, aq_donate_elite_rangers_hand_over_on_consequence, isLeave: false, SettlementMenuOptionIndex);
        starter.AddGameMenuOption(CastleMenuId, HandOverOptionId, Texts.Ids.DonateEliteRangersHandOverOption, aq_donate_elite_rangers_hand_over_on_condition, aq_donate_elite_rangers_hand_over_on_consequence, isLeave: false, SettlementMenuOptionIndex);
        starter.AddGameMenuOption(TownGuardMenuId, HandOverOptionId, Texts.Ids.DonateEliteRangersHandOverOption, aq_donate_elite_rangers_hand_over_on_condition, aq_donate_elite_rangers_hand_over_on_consequence, isLeave: false, GuardMenuOptionIndex);
        starter.AddGameMenuOption(CastleGuardMenuId, HandOverOptionId, Texts.Ids.DonateEliteRangersHandOverOption, aq_donate_elite_rangers_hand_over_on_condition, aq_donate_elite_rangers_hand_over_on_consequence, isLeave: false, GuardMenuOptionIndex);
    }

    private bool aq_donate_elite_rangers_hand_over_on_condition(MenuCallbackArgs args)
    {
        DonateEliteRangersQuest? quest = FindQuest(Settlement.CurrentSettlement);
        if (quest == null)
            return false;

        args.optionLeaveType = GameMenuOption.LeaveType.DonateTroops;
        args.OptionQuestData = GameMenuOption.IssueQuestFlags.ActiveIssue;
        args.Text?.SetTextVariable(Texts.CountVariable, quest.TroopCount).SetTextVariable(Texts.TroopVariable, quest.Troop.Name);
        if (GetHealthyCount(MobileParty.MainParty.MemberRoster, quest.Troop) < quest.TroopCount)
        {
            args.IsEnabled = false;
            args.Tooltip = Texts.DonateEliteRangersNotEnoughTroopsTooltip(quest.TroopCount, quest.Troop.Name);
        }
        return true;
    }

    private void aq_donate_elite_rangers_hand_over_on_consequence(MenuCallbackArgs args)
    {
        FindQuest(Settlement.CurrentSettlement)?.HandOverTroops();
        args.MenuContext?.Refresh();
    }

    private static DonateEliteRangersQuest? FindQuest(Settlement? settlement)
    {
        if (settlement == null)
            return null;
        MBReadOnlyList<QuestBase> quests = Campaign.Current.QuestManager.Quests;
        for (int i = 0; i < quests.Count; i++)
        {
            if (quests[i] is DonateEliteRangersQuest quest && quest.IsOngoing && quest.Settlement == settlement)
                return quest;
        }
        return null;
    }

    // PotentialIssueData carries one related object; the issue needs the settlement and the troop.
    private sealed class RangersIssueData(Settlement settlement, CharacterObject troop)
    {
        internal Settlement Settlement { get; } = settlement;

        internal CharacterObject Troop { get; } = troop;
    }
}
