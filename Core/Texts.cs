using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace AdditionalQuestsRedux.Core;

// Every player-visible string of the mod. Ids live in the language files under _Module\ModuleData\Languages\.
internal static class Texts
{
    private const string GoldIconVariable = "GOLD_ICON";
    private const string IssueGiverTag = "ISSUE_GIVER";
    private const string QuestGiverTag = "QUEST_GIVER";
    private const string SettlementNameVariable = "SETTLEMENT_NAME";
    private const string SettlementVariable = "SETTLEMENT";
    private const string IssueSettlementVariable = "ISSUE_SETTLEMENT";
    private const string QuestSettlementVariable = "QUEST_SETTLEMENT";
    private const string SettlementLinkVariable = "SETTLEMENT_LINK";
    private const string PlayerTag = "PLAYER";
    private const string BanditCultureVariable = "BANDIT_CULTURE";
    private const string SpearsAmountVariable = "SPEARS_AMOUNT";
    private const string RewardAmountVariable = "REWARD_AMOUNT";
    private const string MerchantNameVariable = "MERCHANT_NAME";
    private const string FoodAmountVariable = "FOOD_AMOUNT";
    private const string GoldAmountVariable = "GOLD_AMOUNT";
    private const string DestinationVariable = "DESTINATION";
    private const string OriginVariable = "ORIGIN";
    private const string OldDestinationVariable = "OLD_DESTINATION";
    private const string TroopCountVariable = "TROOP_COUNT";
    private const string CargoValueVariable = "CARGO_VALUE";
    private const string EarnedVariable = "EARNED";
    private const string ProfitVariable = "PROFIT";
    private const string ShareVariable = "SHARE";
    private const string RewardVariable = "REWARD";

    // internal: set from outside Texts (vanilla builds menu option texts from Ids strings; the caravan greeting variables are set globally).
    internal const string AmountVariable = "AMOUNT";
    internal const string CountVariable = "COUNT";
    internal const string TroopVariable = "TROOP";
    internal const string LongDistanceCaravanHometownVariable = "HOMETOWN";
    internal const string LongDistanceCaravanMerchantTag = "MERCHANT";
    internal const string LongDistanceCaravanProtectorTag = "PROTECTOR";

    #region Settings
    public static readonly TextObject ModName = new(Ids.ModName);
    #endregion

    #region Common
    // Same value vanilla quests set; the global GOLD_ICON variable is not guaranteed to be set.
    internal static TextObject WithGoldIcon(this TextObject text) => text.SetTextVariable(GoldIconVariable, Ids.GoldIcon);

    internal static TextObject CancelWarLog(CharacterObject issueGiver) => new TextObject(Ids.CancelWarLog).WithCharacter(IssueGiverTag, issueGiver);

    internal static TextObject CancelRaidLog(TextObject settlementName, CharacterObject issueGiver) =>
        new TextObject(Ids.CancelRaidLog).SetTextVariable(SettlementNameVariable, settlementName).WithCharacter(IssueGiverTag, issueGiver);

    internal static TextObject QuestGiverLostSettlementLog(CharacterObject questGiver, TextObject settlementLink) =>
        new TextObject(Ids.QuestGiverLostSettlementLog).WithCharacter(QuestGiverTag, questGiver).SetTextVariable(SettlementVariable, settlementLink);

    internal static TextObject WarDeclaredLog(CharacterObject questGiver) => new TextObject(Ids.WarDeclaredLog).WithCharacter(QuestGiverTag, questGiver);

    internal static TextObject PlayerDeclaredWarLog(CharacterObject questGiver) => new TextObject(Ids.PlayerDeclaredWarLog).WithCharacter(QuestGiverTag, questGiver);
    #endregion

    #region Bandit Army
    internal static TextObject BanditArmyTitle(TextObject settlementName) => new TextObject(Ids.BanditArmyTitle).SetTextVariable(IssueSettlementVariable, settlementName);

    internal static TextObject BanditArmyDescription(TextObject settlementName) => new TextObject(Ids.BanditArmyDescription).SetTextVariable(IssueSettlementVariable, settlementName);

    public static readonly TextObject BanditArmyIssueBrief = new(Ids.BanditArmyIssueBrief);
    public static readonly TextObject BanditArmyIssueAccept = new(Ids.BanditArmyIssueAccept);
    public static readonly TextObject BanditArmyIssueSolution = new(Ids.BanditArmyIssueSolution);
    public static readonly TextObject BanditArmyIssueSolutionAccept = new(Ids.BanditArmyIssueSolutionAccept);
    public static readonly TextObject BanditArmyRumor = new(Ids.BanditArmyRumor);

    internal static TextObject BanditArmyQuestAcceptLog(TextObject settlementName, CharacterObject questGiver) =>
        new TextObject(Ids.BanditArmyQuestAcceptLog).WithCharacter(QuestGiverTag, questGiver).SetTextVariable(QuestSettlementVariable, settlementName);

    internal static TextObject BanditArmyRaidingLog(TextObject settlementName) => new TextObject(Ids.BanditArmyRaidingLog).SetTextVariable(QuestSettlementVariable, settlementName);

    internal static TextObject BanditArmySuccessLog(TextObject settlementName) => new TextObject(Ids.BanditArmySuccessLog).SetTextVariable(QuestSettlementVariable, settlementName);

    internal static TextObject BanditArmyFailureLog(TextObject settlementName) => new TextObject(Ids.BanditArmyFailureLog).SetTextVariable(QuestSettlementVariable, settlementName);

    internal static TextObject BanditArmyTimeoutLog(TextObject settlementName) => new TextObject(Ids.BanditArmyTimeoutLog).SetTextVariable(QuestSettlementVariable, settlementName);

    internal static TextObject BanditArmyGoneLog(TextObject settlementName, CharacterObject questGiver) =>
        new TextObject(Ids.BanditArmyGoneLog).SetTextVariable(QuestSettlementVariable, settlementName).WithCharacter(QuestGiverTag, questGiver);

    public static readonly TextObject BanditArmyOfferDialog = new(Ids.BanditArmyOfferDialog);
    public static readonly TextObject BanditArmyDiscussDialogStart = new(Ids.BanditArmyDiscussDialogStart);
    public static readonly TextObject BanditArmyDiscussDialogTalk = new(Ids.BanditArmyDiscussDialogTalk);
    public static readonly TextObject BanditArmyDiscussDialogFinish = new(Ids.BanditArmyDiscussDialogFinish);
    public static readonly TextObject BanditArmyBanditConfront = new(Ids.BanditArmyBanditConfront);
    public static readonly TextObject BanditArmyBanditConfrontResponse = new(Ids.BanditArmyBanditConfrontResponse);
    public static readonly TextObject BanditArmyDirection = new(Ids.BanditArmyDirection);

    internal static TextObject BanditArmyHideoutMarked(CharacterObject questGiver) => new TextObject(Ids.BanditArmyHideoutMarked).WithCharacter(QuestGiverTag, questGiver);

    internal static TextObject BanditArmyPartyName(TextObject cultureName) => new TextObject(Ids.BanditArmyPartyName).SetTextVariable(BanditCultureVariable, cultureName);
    #endregion

    #region Militia Spears
    internal static TextObject MilitiaSpearsTitle(TextObject issueSettlement) =>
        new TextObject(Ids.MilitiaSpearsTitle).SetTextVariable(IssueSettlementVariable, issueSettlement);

    internal static TextObject MilitiaSpearsDescription(TextObject issueSettlement) =>
        new TextObject(Ids.MilitiaSpearsDescription).SetTextVariable(IssueSettlementVariable, issueSettlement);

    public static readonly TextObject MilitiaSpearsIssueBrief = new(Ids.MilitiaSpearsIssueBrief);

    public static readonly TextObject MilitiaSpearsIssueAccept = new(Ids.MilitiaSpearsIssueAccept);

    internal static TextObject MilitiaSpearsIssueSolution(int spearsAmount, int rewardAmount) =>
        new TextObject(Ids.MilitiaSpearsIssueSolution)
            .SetTextVariable(SpearsAmountVariable, spearsAmount)
            .SetTextVariable(RewardAmountVariable, rewardAmount)
            .WithGoldIcon();

    public static readonly TextObject MilitiaSpearsIssueSolutionAccept = new(Ids.MilitiaSpearsIssueSolutionAccept);

    internal static TextObject MilitiaSpearsRumor(CharacterObject questGiver) =>
        new TextObject(Ids.MilitiaSpearsRumor).WithCharacter(QuestGiverTag, questGiver);

    internal static TextObject MilitiaSpearsQuestAcceptLog(CharacterObject questGiver, TextObject questSettlement, int spearsAmount, int rewardAmount) =>
        new TextObject(Ids.MilitiaSpearsQuestAcceptLog)
            .WithCharacter(QuestGiverTag, questGiver)
            .SetTextVariable(QuestSettlementVariable, questSettlement)
            .SetTextVariable(SpearsAmountVariable, spearsAmount)
            .SetTextVariable(RewardAmountVariable, rewardAmount)
            .WithGoldIcon();

    internal static TextObject MilitiaSpearsHasSpearsLog(TextObject questSettlement) =>
        new TextObject(Ids.MilitiaSpearsHasSpearsLog).SetTextVariable(QuestSettlementVariable, questSettlement);

    internal static TextObject MilitiaSpearsHaveSpearsLog(TextObject questSettlement) =>
        new TextObject(Ids.MilitiaSpearsHaveSpearsLog).SetTextVariable(QuestSettlementVariable, questSettlement);

    internal static TextObject MilitiaSpearsTimeoutLog(int spearsAmount) =>
        new TextObject(Ids.MilitiaSpearsTimeoutLog).SetTextVariable(SpearsAmountVariable, spearsAmount);

    internal static TextObject MilitiaSpearsSuccessLog(int spearsAmount) =>
        new TextObject(Ids.MilitiaSpearsSuccessLog).SetTextVariable(SpearsAmountVariable, spearsAmount);

    internal static TextObject MilitiaSpearsThankYouDialog(CharacterObject player) =>
        new TextObject(Ids.MilitiaSpearsThankYouDialog).WithCharacter(PlayerTag, player);

    internal static TextObject MilitiaSpearsWaitingDialog(CharacterObject player) =>
        new TextObject(Ids.MilitiaSpearsWaitingDialog).WithCharacter(PlayerTag, player);

    internal static TextObject MilitiaSpearsDiscussDialog(int spearsAmount) =>
        new TextObject(Ids.MilitiaSpearsDiscussDialog).SetTextVariable(SpearsAmountVariable, spearsAmount);

    public static readonly TextObject MilitiaSpearsHandOverDialog = new(Ids.MilitiaSpearsHandOverDialog);

    public static readonly TextObject MilitiaSpearsNotYetDialog = new(Ids.MilitiaSpearsNotYetDialog);

    public static readonly TextObject MilitiaSpearsNotEnoughSpears = new(Ids.MilitiaSpearsNotEnoughSpears);

    public static readonly TextObject MilitiaSpearsQuestObjective = new(Ids.MilitiaSpearsQuestObjective);
    #endregion

    #region Starving Town
    internal static TextObject StarvingTownTitle(TextObject settlementName) => new TextObject(Ids.StarvingTownTitle).SetTextVariable(IssueSettlementVariable, settlementName);

    internal static TextObject StarvingTownDescription(TextObject settlementName, TextObject merchantName) =>
        new TextObject(Ids.StarvingTownDescription).SetTextVariable(SettlementVariable, settlementName).SetTextVariable(MerchantNameVariable, merchantName);

    public static readonly TextObject StarvingTownIssueBrief = new(Ids.StarvingTownIssueBrief);

    public static readonly TextObject StarvingTownIssueAccept = new(Ids.StarvingTownIssueAccept);

    internal static TextObject StarvingTownIssueSolution(int foodAmount) => new TextObject(Ids.StarvingTownIssueSolution).SetTextVariable(FoodAmountVariable, foodAmount);

    public static readonly TextObject StarvingTownIssueSolutionAccept = new(Ids.StarvingTownIssueSolutionAccept);

    internal static TextObject StarvingTownRumor(CharacterObject questGiver) => new TextObject(Ids.StarvingTownRumor).WithCharacter(QuestGiverTag, questGiver);

    internal static TextObject StarvingTownQuestAcceptLog(CharacterObject questGiver, TextObject settlementName, int foodAmount) =>
        new TextObject(Ids.StarvingTownQuestAcceptLog).WithCharacter(QuestGiverTag, questGiver)
            .SetTextVariable(QuestSettlementVariable, settlementName).SetTextVariable(FoodAmountVariable, foodAmount);

    internal static TextObject StarvingTownHasFoodLog(TextObject settlementName) => new TextObject(Ids.StarvingTownHasFoodLog).SetTextVariable(QuestSettlementVariable, settlementName);

    internal static TextObject StarvingTownHaveFoodNotification(TextObject settlementName) =>
        new TextObject(Ids.StarvingTownHaveFoodNotification).SetTextVariable(QuestSettlementVariable, settlementName);

    internal static TextObject StarvingTownTimeoutLog(CharacterObject questGiver, TextObject settlementName, int foodAmount) =>
        new TextObject(Ids.StarvingTownTimeoutLog).WithCharacter(QuestGiverTag, questGiver)
            .SetTextVariable(QuestSettlementVariable, settlementName).SetTextVariable(FoodAmountVariable, foodAmount);

    internal static TextObject StarvingTownSuccessLog(TextObject settlementName, int foodAmount) =>
        new TextObject(Ids.StarvingTownSuccessLog).SetTextVariable(QuestSettlementVariable, settlementName).SetTextVariable(FoodAmountVariable, foodAmount);

    internal static TextObject StarvingTownThankYouDialog(CharacterObject player) => new TextObject(Ids.StarvingTownThankYouDialog).WithCharacter(PlayerTag, player);

    internal static TextObject StarvingTownWaitingDialog(CharacterObject player) => new TextObject(Ids.StarvingTownWaitingDialog).WithCharacter(PlayerTag, player);

    internal static TextObject StarvingTownDiscussDialog(int foodAmount) => new TextObject(Ids.StarvingTownDiscussDialog).SetTextVariable(FoodAmountVariable, foodAmount);

    public static readonly TextObject StarvingTownHandOverDialog = new(Ids.StarvingTownHandOverDialog);

    public static readonly TextObject StarvingTownNotYetDialog = new(Ids.StarvingTownNotYetDialog);

    public static readonly TextObject StarvingTownNotEnoughFood = new(Ids.StarvingTownNotEnoughFood);

    // Factory: the text is stored in the quest's saved journal log.
    internal static TextObject StarvingTownQuestObjective() => new(Ids.StarvingTownQuestObjective);

    internal static TextObject StarvingTownGaveFoodMeatLog(int foodAmount, int goldAmount) =>
        new TextObject(Ids.StarvingTownGaveFoodMeatLog).SetTextVariable(FoodAmountVariable, foodAmount).SetTextVariable(GoldAmountVariable, goldAmount).WithGoldIcon();

    internal static TextObject StarvingTownGaveFoodFishLog(int foodAmount, int goldAmount) =>
        new TextObject(Ids.StarvingTownGaveFoodFishLog).SetTextVariable(FoodAmountVariable, foodAmount).SetTextVariable(GoldAmountVariable, goldAmount).WithGoldIcon();

    internal static TextObject StarvingTownGaveFoodGrainLog(int foodAmount, int goldAmount) =>
        new TextObject(Ids.StarvingTownGaveFoodGrainLog).SetTextVariable(FoodAmountVariable, foodAmount).SetTextVariable(GoldAmountVariable, goldAmount).WithGoldIcon();
    #endregion

    #region Long Distance Caravan
    // Factories everywhere: every text is handed to vanilla (overrides, journal logs, quick info, dialog lines, party name).
    internal static TextObject LongDistanceCaravanTitle(TextObject destination) => new TextObject(Ids.LongDistanceCaravanTitle).SetTextVariable(DestinationVariable, destination);

    internal static TextObject LongDistanceCaravanDescription(CharacterObject questGiver, TextObject origin, TextObject destination) =>
        new TextObject(Ids.LongDistanceCaravanDescription).WithCharacter(QuestGiverTag, questGiver)
            .SetTextVariable(OriginVariable, origin).SetTextVariable(DestinationVariable, destination);

    internal static TextObject LongDistanceCaravanIssueBrief(TextObject destination) => new TextObject(Ids.LongDistanceCaravanIssueBrief).SetTextVariable(DestinationVariable, destination);

    internal static TextObject LongDistanceCaravanIssueAccept() => new(Ids.LongDistanceCaravanIssueAccept);

    internal static TextObject LongDistanceCaravanIssueSolution(TextObject destination, int troopCount) =>
        new TextObject(Ids.LongDistanceCaravanIssueSolution).SetTextVariable(DestinationVariable, destination).SetTextVariable(TroopCountVariable, troopCount);

    internal static TextObject LongDistanceCaravanIssueSolutionAccept() => new(Ids.LongDistanceCaravanIssueSolutionAccept);

    internal static TextObject LongDistanceCaravanRumor(CharacterObject questGiver, TextObject destination) =>
        new TextObject(Ids.LongDistanceCaravanRumor).WithCharacter(QuestGiverTag, questGiver).SetTextVariable(DestinationVariable, destination);

    internal static TextObject LongDistanceCaravanAcceptedLog(CharacterObject questGiver, TextObject originLink, TextObject destinationLink, int cargoValue) =>
        new TextObject(Ids.LongDistanceCaravanAcceptedLog).WithCharacter(QuestGiverTag, questGiver)
            .SetTextVariable(OriginVariable, originLink).SetTextVariable(DestinationVariable, destinationLink)
            .WithGoldIcon().SetTextVariable(CargoValueVariable, cargoValue);

    internal static TextObject LongDistanceCaravanSuccessLog(CharacterObject questGiver, TextObject destinationLink, int earned, int profit, int share) =>
        new TextObject(Ids.LongDistanceCaravanSuccessLog).WithCharacter(QuestGiverTag, questGiver)
            .SetTextVariable(DestinationVariable, destinationLink)
            .SetTextVariable(EarnedVariable, earned).SetTextVariable(ProfitVariable, profit).SetTextVariable(ShareVariable, share)
            .WithGoldIcon();

    internal static TextObject LongDistanceCaravanRedirectLog(TextObject oldDestinationLink, TextObject destinationLink) =>
        new TextObject(Ids.LongDistanceCaravanRedirectLog).SetTextVariable(OldDestinationVariable, oldDestinationLink).SetTextVariable(DestinationVariable, destinationLink);

    internal static TextObject LongDistanceCaravanNoDestinationLog(CharacterObject questGiver) =>
        new TextObject(Ids.LongDistanceCaravanNoDestinationLog).WithCharacter(QuestGiverTag, questGiver);

    internal static TextObject LongDistanceCaravanLostTrackLog(CharacterObject issueGiver) =>
        new TextObject(Ids.LongDistanceCaravanLostTrackLog).WithCharacter(IssueGiverTag, issueGiver);

    internal static TextObject LongDistanceCaravanDestroyedByBanditsLog(CharacterObject issueGiver) =>
        new TextObject(Ids.LongDistanceCaravanDestroyedByBanditsLog).WithCharacter(IssueGiverTag, issueGiver);

    internal static TextObject LongDistanceCaravanDestroyedLog() => new(Ids.LongDistanceCaravanDestroyedLog);

    internal static TextObject LongDistanceCaravanAttackedByPlayerLog() => new(Ids.LongDistanceCaravanAttackedByPlayerLog);

    internal static TextObject LongDistanceCaravanTimeoutLog() => new(Ids.LongDistanceCaravanTimeoutLog);

    internal static TextObject LongDistanceCaravanOfferDialog() => new(Ids.LongDistanceCaravanOfferDialog);

    internal static TextObject LongDistanceCaravanDiscussDialog() => new(Ids.LongDistanceCaravanDiscussDialog);

    // HOMETOWN / MERCHANT / PROTECTOR are set globally by the quest when the caravan talks.
    internal static TextObject LongDistanceCaravanGreetingDialog() => new(Ids.LongDistanceCaravanGreetingDialog);

    internal static TextObject LongDistanceCaravanAboutTaskOption(CharacterObject questGiver) =>
        new TextObject(Ids.LongDistanceCaravanAboutTaskOption).WithCharacter(QuestGiverTag, questGiver);

    internal static TextObject LongDistanceCaravanFollowUsDialog() => new(Ids.LongDistanceCaravanFollowUsDialog);

    internal static TextObject LongDistanceCaravanFarewellOption() => new(Ids.LongDistanceCaravanFarewellOption);

    internal static TextObject LongDistanceCaravanLootOption() => new(Ids.LongDistanceCaravanLootOption);

    internal static TextObject LongDistanceCaravanLootRefuseDialog() => new(Ids.LongDistanceCaravanLootRefuseDialog);

    internal static TextObject LongDistanceCaravanAttackOption() => new(Ids.LongDistanceCaravanAttackOption);

    internal static TextObject LongDistanceCaravanGoInPeaceOption() => new(Ids.LongDistanceCaravanGoInPeaceOption);

    internal static TextObject LongDistanceCaravanTravelingNotice(TextObject settlementName) =>
        new TextObject(Ids.LongDistanceCaravanTravelingNotice).SetTextVariable(SettlementNameVariable, settlementName);

    internal static TextObject LongDistanceCaravanMovingLog(TextObject settlementLink) =>
        new TextObject(Ids.LongDistanceCaravanMovingLog).SetTextVariable(SettlementNameVariable, settlementLink);

    internal static TextObject LongDistanceCaravanLoseSightNotice() => new(Ids.LongDistanceCaravanLoseSightNotice);

    internal static TextObject LongDistanceCaravanRaidersName() => new(Ids.LongDistanceCaravanRaidersName);

    internal static TextObject LongDistanceCaravanAmbushedNotice() => new(Ids.LongDistanceCaravanAmbushedNotice);

    internal static TextObject LongDistanceCaravanEnteredLog(TextObject settlementLink) =>
        new TextObject(Ids.LongDistanceCaravanEnteredLog).SetTextVariable(SettlementLinkVariable, settlementLink);
    #endregion

    #region Fund the Walls
    public static readonly TextObject FundTheWallsNotEnoughGold = new(Ids.FundTheWallsNotEnoughGold);

    internal static TextObject FundTheWallsTitle(TextObject settlementName) => new TextObject(Ids.FundTheWallsTitle).SetTextVariable(SettlementVariable, settlementName);

    internal static TextObject FundTheWallsIssueDescription(CharacterObject questGiver, TextObject settlementName) =>
        new TextObject(Ids.FundTheWallsIssueDescription).WithCharacter(QuestGiverTag, questGiver).SetTextVariable(SettlementVariable, settlementName);

    internal static TextObject FundTheWallsIssueBrief(TextObject settlementName) => new TextObject(Ids.FundTheWallsIssueBrief).SetTextVariable(SettlementVariable, settlementName);

    public static readonly TextObject FundTheWallsIssueAccept = new(Ids.FundTheWallsIssueAccept);

    internal static TextObject FundTheWallsIssueSolutionExplanation(int amount, TextObject settlementName) =>
        new TextObject(Ids.FundTheWallsIssueSolutionExplanation).SetTextVariable(AmountVariable, amount).SetTextVariable(SettlementVariable, settlementName).WithGoldIcon();

    internal static TextObject FundTheWallsIssueSolutionAccept(int amount, TextObject settlementName) =>
        new TextObject(Ids.FundTheWallsIssueSolutionAccept).SetTextVariable(AmountVariable, amount).SetTextVariable(SettlementVariable, settlementName).WithGoldIcon();

    internal static TextObject FundTheWallsAcceptedLog(CharacterObject questGiver, TextObject settlementLink, int amount) =>
        new TextObject(Ids.FundTheWallsAcceptedLog).WithCharacter(QuestGiverTag, questGiver).SetTextVariable(SettlementVariable, settlementLink)
            .SetTextVariable(AmountVariable, amount).WithGoldIcon();

    internal static TextObject FundTheWallsSuccessLog(TextObject settlementLink, int amount) =>
        new TextObject(Ids.FundTheWallsSuccessLog).SetTextVariable(SettlementVariable, settlementLink).SetTextVariable(AmountVariable, amount).WithGoldIcon();

    internal static TextObject FundTheWallsTimeoutLog(CharacterObject questGiver, TextObject settlementLink) =>
        new TextObject(Ids.FundTheWallsTimeoutLog).WithCharacter(QuestGiverTag, questGiver).SetTextVariable(SettlementVariable, settlementLink);

    internal static TextObject FundTheWallsOfferThanks(TextObject settlementName) => new TextObject(Ids.FundTheWallsOfferThanks).SetTextVariable(SettlementVariable, settlementName);

    internal static TextObject FundTheWallsDiscussQuestion(TextObject settlementName) => new TextObject(Ids.FundTheWallsDiscussQuestion).SetTextVariable(SettlementVariable, settlementName);

    public static readonly TextObject FundTheWallsDiscussAnswer = new(Ids.FundTheWallsDiscussAnswer);
    #endregion

    #region Donate Elite Rangers
    internal static TextObject DonateEliteRangersNotEnoughTroopsTooltip(int count, TextObject troopName) =>
        new TextObject(Ids.DonateEliteRangersNotEnoughTroopsTooltip).SetTextVariable(CountVariable, count).SetTextVariable(TroopVariable, troopName);

    internal static TextObject DonateEliteRangersTitle(TextObject settlementName) =>
        new TextObject(Ids.DonateEliteRangersTitle).SetTextVariable(SettlementVariable, settlementName);

    internal static TextObject DonateEliteRangersDescription(CharacterObject questGiver, int count, TextObject troopName, TextObject settlementName) =>
        new TextObject(Ids.DonateEliteRangersDescription)
            .WithCharacter(QuestGiverTag, questGiver)
            .SetTextVariable(CountVariable, count)
            .SetTextVariable(TroopVariable, troopName)
            .SetTextVariable(SettlementVariable, settlementName);

    internal static TextObject DonateEliteRangersIssueBrief(TextObject settlementName) =>
        new TextObject(Ids.DonateEliteRangersIssueBrief).SetTextVariable(SettlementVariable, settlementName);

    public static readonly TextObject DonateEliteRangersIssueAccept = new(Ids.DonateEliteRangersIssueAccept);

    internal static TextObject DonateEliteRangersSolutionExplanation(int count, TextObject troopName, TextObject settlementName, int rewardGold) =>
        new TextObject(Ids.DonateEliteRangersSolutionExplanation)
            .SetTextVariable(CountVariable, count)
            .SetTextVariable(TroopVariable, troopName)
            .SetTextVariable(SettlementVariable, settlementName)
            .SetTextVariable(RewardVariable, rewardGold)
            .WithGoldIcon();

    internal static TextObject DonateEliteRangersSolutionAccept(int count, TextObject troopName, TextObject settlementName) =>
        new TextObject(Ids.DonateEliteRangersSolutionAccept)
            .SetTextVariable(CountVariable, count)
            .SetTextVariable(TroopVariable, troopName)
            .SetTextVariable(SettlementVariable, settlementName);

    internal static TextObject DonateEliteRangersAcceptedLog(CharacterObject questGiver, TextObject settlementLink, int count, TextObject troopLink, int rewardGold) =>
        new TextObject(Ids.DonateEliteRangersAcceptedLog)
            .WithCharacter(QuestGiverTag, questGiver)
            .SetTextVariable(SettlementVariable, settlementLink)
            .SetTextVariable(CountVariable, count)
            .SetTextVariable(TroopVariable, troopLink)
            .SetTextVariable(RewardVariable, rewardGold)
            .WithGoldIcon();

    internal static TextObject DonateEliteRangersSuccessLog(int count, TextObject troopLink, TextObject settlementLink) =>
        new TextObject(Ids.DonateEliteRangersSuccessLog)
            .SetTextVariable(CountVariable, count)
            .SetTextVariable(TroopVariable, troopLink)
            .SetTextVariable(SettlementVariable, settlementLink);

    internal static TextObject DonateEliteRangersTimeoutLog(CharacterObject questGiver, TextObject settlementLink) =>
        new TextObject(Ids.DonateEliteRangersTimeoutLog).WithCharacter(QuestGiverTag, questGiver).SetTextVariable(SettlementVariable, settlementLink);

    internal static TextObject DonateEliteRangersOfferAccepted(TextObject settlementName) =>
        new TextObject(Ids.DonateEliteRangersOfferAccepted).SetTextVariable(SettlementVariable, settlementName);

    internal static TextObject DonateEliteRangersDiscussAsk(TextObject troopName, TextObject settlementName) =>
        new TextObject(Ids.DonateEliteRangersDiscussAsk).SetTextVariable(TroopVariable, troopName).SetTextVariable(SettlementVariable, settlementName);

    public static readonly TextObject DonateEliteRangersDiscussNotYet = new(Ids.DonateEliteRangersDiscussNotYet);
    #endregion

    internal static class Ids
    {
        #region Settings
        public const string ModName = "{=AQ_k3Tz9Qe}Additional Quests Redux";
        public const string GroupQuests = "{=AQ_Rw7pL2x}Quests";
        public const string GroupBanditArmy = GroupQuests + "/{=AQ_b8NfQ4m}Bandit Army";
        public const string GroupMilitiaSpears = GroupQuests + "/{=AQ_Hq5vD1s}Militia Spears";
        public const string GroupStarvingTown = GroupQuests + "/{=AQ_y2GcX7k}Starving Town";
        public const string GroupLongDistanceCaravan = GroupQuests + "/{=AQ_ShuaUUm}Long Distance Caravan";
        public const string GroupFundTheWalls = GroupQuests + "/{=AQ_7wmctzL}Fund the Walls";
        public const string GroupDonateEliteRangers = GroupQuests + "/{=AQ_dR9kq2V}Donate Elite Rangers";
        public const string GroupTechnical = "{=AQ_tWD2HrQ}Technical settings";
        public const string QuestEnabled = "{=AQ_Pm4tZ8w}Quest Enabled";
        public const string AiLordRewards = "{=AQ_Xk4mT9b}AI lords get quest rewards";
        public const string AiLordRewardsHint = "{=AQ_q7RzW2n}When an AI lord resolves one of these quests, the lord gets the gold, renown and relation a player would get, and the village or town gets the same benefits. [Default: enabled]";
        public const string VanillaAiLordRewards = "{=AQ_Vd3nH8s}AI lords get vanilla quest rewards";
        public const string VanillaAiLordRewardsHint = "{=AQ_g6PwK1z}When an AI lord resolves a vanilla quest, the lord gets the quest's gold reward, some renown and better relation with the quest giver, and the quest giver gains power. [Default: enabled]";
        public const string BanditArmyEnabledHint = "{=AQ_e9JrB3n}Lets village notables offer the Bandit Army quest. Quests already offered or accepted are not affected. [Default: enabled]";
        public const string BanditArmyMinTroopReqDisabled = "{=AQ_V6hW1yd}Remove minimum troop requirement";
        public const string BanditArmyMinTroopReqDisabledHint = "{=AQ_s3KxM5q}Removes the minimum requirement of 50 troops to start the Bandit Army quest. [Default: disabled]";
        public const string BanditArmySizeMultiplier = "{=AQ_C7uN2fa}Bandit Army Size";
        public const string BanditArmySizeMultiplierHint = "{=AQ_j4QbT9r}Changes the size of the bandit army party. Applies to bandit armies spawned after the change. [Default: 100%]";
        public const string MilitiaSpearsEnabledHint = "{=AQ_Lz8eY3p}Lets village notables offer the Militia Spears quest. Quests already offered or accepted are not affected. [Default: enabled]";
        public const string StarvingTownEnabledHint = "{=AQ_f1XdR6v}Lets town merchants offer the Starving Town quest. Quests already offered or accepted are not affected. [Default: enabled]";
        public const string LongDistanceCaravanEnabledHint = "{=AQ_kdH4PaP}Lets town merchants offer the Long Distance Caravan quest. Quests already offered or accepted are not affected. [Default: enabled]";
        public const string FundTheWallsEnabledHint = "{=AQ_Rp85ntf}Lets lords offer the Fund the Walls quest for their towns and castles. Quests already offered or accepted are not affected. [Default: enabled]";
        public const string DonateEliteRangersEnabledHint = "{=AQ_Tr4nGrs}Lets lords offer the Donate Elite Rangers quest for their towns and castles. Quests already offered or accepted are not affected. [Default: enabled]";
        public const string LoggingEnabled = "{=AQ_N5gRw2c}Logging for debugging";
        public const string LoggingEnabledHint = "{=AQ_u8LmK4d}Logging for debugging (English only). [Default: disabled]";
        #endregion

        #region Common
        // Not translated: "{=!}" marks a raw value with no language entry.
        public const string GoldIcon = "{=!}<img src=\"General\\Icons\\Coin@2x\" extend=\"6\">";
        public const string CancelWarLog = "{=AQGenericCancelWarLog}Your clan is now at war with the {ISSUE_GIVER.LINK}'s lord. Your agreement with {ISSUE_GIVER.LINK} was canceled.";
        public const string CancelRaidLog = "{=AQGenericCancelRaidLog}{SETTLEMENT_NAME} was raided by someone else. Your agreement with {ISSUE_GIVER.LINK} was canceled.";
        // Vanilla LordNeedsGarrisonTroops texts: vanilla ids, no rows in our language files.
        public const string QuestGiverLostSettlementLog = "{=zS68eOsl}{QUEST_GIVER.LINK} has lost {SETTLEMENT} and your agreement with {?QUEST_GIVER.GENDER}her{?}his{\\?} canceled.";
        public const string WarDeclaredLog = "{=JIWVeTMD}Your clan is now at war with {QUEST_GIVER.LINK}'s realm. Your agreement with {QUEST_GIVER.LINK} was canceled.";
        public const string PlayerDeclaredWarLog = "{=bqeWVVEE}Your actions have started a war with {QUEST_GIVER.LINK}'s faction. {?QUEST_GIVER.GENDER}She{?}He{\\?} cancels your agreement and the quest is a failure.";
        #endregion

        #region Bandit Army
        public const string BanditArmyTitle = "{=AQBATitle}Bandit Army Heading to {ISSUE_SETTLEMENT}!";
        public const string BanditArmyDescription = "{=AQBADescription}A bandit army is heading to {ISSUE_SETTLEMENT} for a raid. The people of {ISSUE_SETTLEMENT} need help defending themselves.";
        public const string BanditArmyIssueBrief = "{=AQBAIssueBrief}Dire news has reached me, my scouts say that an army of rabble led by a self-proclaimed bandit \"king\" are making their way to our village right now, intent on taking everything we have. I've sent word to our nobles for help but I fear it may be too late by the time they get here.";
        public const string BanditArmyIssueAccept = "{=AQBAIssueAccept}That is troubling news, how will you defend yourselves?";
        public const string BanditArmyIssueSolution = "{=AQBAIssueSolution}Our militia will fight for our homes, but the more people we have the better. The enemy will be mostly comprised of rabble, just looters looking for easy money but they will have some better trained bandits as well. Will you help us defend the village?";
        public const string BanditArmyIssueSolutionAccept = "{=AQBAIssueSolutionAccept}Yes, I will help defend the village.";
        public const string BanditArmyRumor = "{=AQBARumor}I heard that an army of bandits are heading their way here right now! We need to run, or hide or-or anything but stay here! Please, you must help us!";
        public const string BanditArmyQuestAcceptLog = "{=AQBAQuestAcceptLog}The village of {QUEST_SETTLEMENT} is about to be raided by a bandit army from a nearby hideout! {QUEST_GIVER.LINK} has asked you to stop the bandits from devastating the village.{newline}{newline}Stop the bandits from reaching {QUEST_SETTLEMENT}.";
        public const string BanditArmyRaidingLog = "{=AQBABanditRaidingLog}The bandit army is raiding {QUEST_SETTLEMENT}! Protect the villagers!";
        public const string BanditArmySuccessLog = "{=AQBASuccessLog}You have defeated the bandit army that was going to raid {QUEST_SETTLEMENT}. The villagers are very grateful. The faction ruling the village have awarded you a bounty for defending the village.";
        public const string BanditArmyFailureLog = "{=AQBAFailureLog}You've failed to stop the bandit army. The bandits ravaged {QUEST_SETTLEMENT} and left.";
        public const string BanditArmyTimeoutLog = "{=AQBATimeoutLog}You have taken too long to defeat the bandit army. Fortunately, the nobles ruling {QUEST_SETTLEMENT} have been able to respond in time. Your agreement was canceled.";
        public const string BanditArmyGoneLog = "{=AQ_T07dYvH}The bandit army heading to {QUEST_SETTLEMENT} was destroyed by someone else. Your agreement with {QUEST_GIVER.LINK} was canceled.";
        public const string BanditArmyOfferDialog = "{=AQBAOfferDialog}Thank you. The bandits are on their way from a hideout close by. My scouts will show you where it is. We will pray for your success.";
        public const string BanditArmyDiscussDialogStart = "{=AQBADiscussDialogStart}I don't think they'll be long now. I hope you can stop them in time.";
        public const string BanditArmyDiscussDialogTalk = "{=AQBADiscussDialogTalk}Don't worry, we will crush their \"army\".";
        public const string BanditArmyDiscussDialogFinish = "{=AQBADiscussDialogFinish}Good, good. Thank you again.";
        public const string BanditArmyBanditConfront = "{=AQBABanditConfront}Who the hell are you? This 'ere is an army, we're on our way to a village to collect some \"taxes\". If you know what's good for you, you'll stay outta our way!";
        public const string BanditArmyBanditConfrontResponse = "{=AQBABanditConfrontResponse}You won't be raiding around here anytime soon, we're here to stop you!";
        public const string BanditArmyDirection = "{=AQBADirection}Direction to Bandits";
        public const string BanditArmyHideoutMarked = "{=AQBABanditHideoutMarked}{QUEST_GIVER.NAME} has marked the bandit hideout. The bandit army will head from there to the village.";
        public const string BanditArmyPartyName = "{=AQBABanditArmyName}{BANDIT_CULTURE} Army";
        #endregion

        #region Militia Spears
        public const string MilitiaSpearsTitle = "{=AQMSTitle}{ISSUE_SETTLEMENT} Needs Spears for Militia";
        public const string MilitiaSpearsDescription = "{=AQMSDescription}The headman of {ISSUE_SETTLEMENT} needs spears for the militia.";
        public const string MilitiaSpearsIssueBrief = "{=AQMSIssueBrief}I'm currently organizing the militia of the village, however we lack weapons to train and fight with. In particular, we have very few spears in the armoury.";
        public const string MilitiaSpearsIssueAccept = "{=AQMSIssueAccept}How can I help?";
        public const string MilitiaSpearsIssueSolution = "{=AQMSIssueSolution}I can handle training my people into militia fighters, but I need someone to find spears to fight with. You can buy them in the surrounding towns. You can even make your own, if you are a decent smith. We will need {SPEARS_AMOUNT} spears in total. I will pay you {REWARD_AMOUNT}{GOLD_ICON} for the spears.";
        public const string MilitiaSpearsIssueSolutionAccept = "{=AQMSIssueSolutionAccept}I will find the spears you need.";
        public const string MilitiaSpearsRumor = "{=AQMSRumor}The young'uns of the village have all signed up to the newly forming militia. But I heard {QUEST_GIVER.NAME} doesn't have enough spears for even half of the men to train with, so they've been using wooden sticks to train with. I hope they don't end up having to use those same sticks in a real fight!";
        public const string MilitiaSpearsQuestAcceptLog = "{=AQMSQuestAcceptLog}{QUEST_GIVER.LINK}, a {?QUEST_GIVER.GENDER}headwoman{?}headman{\\?} of {QUEST_SETTLEMENT} asked you to deliver {SPEARS_AMOUNT} spears to {?QUEST_GIVER.GENDER}her{?}him{\\?} for the village militia. This will help boost the number of able militia in the village.{newline}{newline}You have agreed to bring them {SPEARS_AMOUNT} spears as soon as possible. You will be paid {REWARD_AMOUNT}{GOLD_ICON} for the spears.{newline}{newline}The spears should be one handed polearms.";
        public const string MilitiaSpearsHasSpearsLog = "{=AQMSHasSpearsLog}You now have enough spears to complete the quest. Return to {QUEST_SETTLEMENT} to hand them over.";
        public const string MilitiaSpearsHaveSpearsLog = "{=AQMSHaveSpearsLog}You have enough spears to complete the quest. Return to {QUEST_SETTLEMENT} to hand them over.";
        public const string MilitiaSpearsTimeoutLog = "{=AQMSTimeoutLog}You have failed to deliver {SPEARS_AMOUNT} spears to the villagers. They won't be able to properly train their militia. The Headman is disappointed.";
        public const string MilitiaSpearsSuccessLog = "{=AQMSSuccessLog}You have delivered {SPEARS_AMOUNT} spears to the villagers. Their militia is ready to train and defend the village. The Headman and the villagers are grateful.";
        public const string MilitiaSpearsThankYouDialog = "{=AQMSThankYouDialog}Thank you, {?PLAYER.GENDER}milady{?}sir{\\?}! We will make good use of these spears.";
        public const string MilitiaSpearsWaitingDialog = "{=AQMSWaitingDialog}We await those spears, {?PLAYER.GENDER}milady{?}sir{\\?}.";
        public const string MilitiaSpearsDiscussDialog = "{=AQMSDiscussDialog}Have you brought {SPEARS_AMOUNT} spears?";
        public const string MilitiaSpearsHandOverDialog = "{=AQMSHandOverDialog}Yes. Here they are.";
        public const string MilitiaSpearsNotYetDialog = "{=AQMSNotYetDialog}I'm working on it.";
        public const string MilitiaSpearsNotEnoughSpears = "{=AQMSNotEnoughSpears}You don't have enough spears.";
        public const string MilitiaSpearsQuestObjective = "{=AQMSQuestObjective}Collect one handed polearms";
        #endregion

        #region Starving Town
        public const string StarvingTownTitle = "{=AQSTitle}Food Crisis in {ISSUE_SETTLEMENT}";
        public const string StarvingTownDescription = "{=AQSTDescription}The town of {SETTLEMENT} is starving, {MERCHANT_NAME} needs help to restock the town's food supplies.";
        public const string StarvingTownIssueBrief = "{=AQSTIssueBrief}The food situation is bad, the town is starving. We've had to ration out all the food coming into the city but it's getting harder and harder each day to put meals in front of people. If this keeps up for much longer, we will either all starve to death or the people will riot.";
        public const string StarvingTownIssueAccept = "{=AQSTIssueAccept}Can I help? I have some food to spare.";
        public const string StarvingTownIssueSolution = "{=AQSTIssueSolution}I thank you for your kindness, but we will require a larger amount of food to restock the city, enough to keep us going while we sort out the source of our food shortage. If you can supply Grain, Meat and Fish, I will pay you triple the average price for each. We will need at least {FOOD_AMOUNT} units of food in total to keep the town supplied.";
        public const string StarvingTownIssueSolutionAccept = "{=AQSTIssueSolutionAccept}I will head out to find food for the town.";
        public const string StarvingTownRumor = "{=AQSTRumor}I'm so hungry... oh, sorry, did I walk into you? It's been so hard lately... I haven't eaten in so long... If only someone could help {QUEST_GIVER.NAME} bring more food into town...";
        public const string StarvingTownQuestAcceptLog = "{=AQSTQuestAcceptLog}{QUEST_GIVER.LINK}, a merchant in the town of {QUEST_SETTLEMENT} asked you to deliver {FOOD_AMOUNT} food to the town, to help fulfil the current food crisis. The food can either be Grain, Meat or Fish.{newline}{newline}You will be paid triple the average price of each good on delivery.";
        public const string StarvingTownHasFoodLog = "{=AQSTHasFoodLog}You now have enough food to complete the quest. Return to {QUEST_SETTLEMENT} to hand it over.";
        public const string StarvingTownHaveFoodNotification = "{=AQSTHaveFoodLog}You have enough food to complete the quest. Return to {QUEST_SETTLEMENT} to hand it over.";
        public const string StarvingTownTimeoutLog = "{=AQSTTimeoutLog}You have failed to deliver {FOOD_AMOUNT} food to {QUEST_SETTLEMENT}. The people continue to starve and {QUEST_GIVER.LINK} is disappointed.";
        public const string StarvingTownSuccessLog = "{=AQSTSuccessLog}You have delivered {FOOD_AMOUNT} food to {QUEST_SETTLEMENT}. The people rejoice at the delivery of food. You have saved the people from starvation.";
        public const string StarvingTownThankYouDialog = "{=AQSTThankYouDialog}Thank you, {?PLAYER.GENDER}milady{?}sir{\\?}! This food will help us all.";
        public const string StarvingTownWaitingDialog = "{=AQSTWaitingDialog}We await the food supplies, {?PLAYER.GENDER}milady{?}sir{\\?}.";
        public const string StarvingTownDiscussDialog = "{=AQSTDiscussDialog}Have you brought {FOOD_AMOUNT} food?";
        public const string StarvingTownHandOverDialog = "{=AQSTHandOverDialog}Yes. Here it is.";
        public const string StarvingTownNotYetDialog = "{=AQSTNotYetDialog}I'm working on it.";
        public const string StarvingTownNotEnoughFood = "{=AQSTNotEnoughFood}You don't have enough food.";
        public const string StarvingTownQuestObjective = "{=AQSTQuestObjective}Collect Grain/Fish/Meat.";
        public const string StarvingTownGaveFoodMeatLog = "{=AQSTGaveFoodMeat}You gave {FOOD_AMOUNT} meat. In return you got {GOLD_AMOUNT}{GOLD_ICON}.";
        public const string StarvingTownGaveFoodFishLog = "{=AQSTGaveFoodFish}You gave {FOOD_AMOUNT} fish. In return you got {GOLD_AMOUNT}{GOLD_ICON}.";
        public const string StarvingTownGaveFoodGrainLog = "{=AQSTGaveFoodGrain}You gave {FOOD_AMOUNT} grain. In return you got {GOLD_AMOUNT}{GOLD_ICON}.";
        #endregion

        #region Long Distance Caravan
        public const string LongDistanceCaravanTitle = "{=AQ_Y9tybDT}Long Distance Caravan to {DESTINATION}";
        public const string LongDistanceCaravanDescription = "{=AQ_XX5gNZk}{QUEST_GIVER.NAME}, a merchant of {ORIGIN}, is sending a heavily loaded caravan to the far town of {DESTINATION} and needs a strong escort for the whole road.";
        public const string LongDistanceCaravanIssueBrief = "{=AQ_wVcaPYi}The markets around here are full of goods that sell for a fortune in {DESTINATION}. I am putting most of my savings into one big caravan, but it is a long road, and a caravan that rich draws every bandit along the way.";
        public const string LongDistanceCaravanIssueAccept = "{=AQ_XZWCGX2}A long road needs a strong escort. What do you propose?";
        public const string LongDistanceCaravanIssueSolution = "{=AQ_vhXhJyN}Ride with my caravan all the way to {DESTINATION}. It will stop in towns along the road to rest. Keep it in sight and keep it alive. When the goods are sold, I will give you half of the profit. Bring at least {TROOP_COUNT} men, the road is dangerous.";
        public const string LongDistanceCaravanIssueSolutionAccept = "{=AQ_ExQRvbj}My men are ready. Load the wagons.";
        public const string LongDistanceCaravanRumor = "{=AQ_pEEJxX2}They say {QUEST_GIVER.NAME} wants to send a whole fortune in goods to {DESTINATION}. A madman or a genius, if anyone can keep the bandits off it.";
        public const string LongDistanceCaravanAcceptedLog = "{=AQ_pHy3hjk}{QUEST_GIVER.LINK}, a merchant of {ORIGIN}, has loaded a caravan with goods worth {CARGO_VALUE}{GOLD_ICON} for the far town of {DESTINATION}. You have agreed to escort it all the way there. When the goods are sold, {?QUEST_GIVER.GENDER}she{?}he{\\?} will give you half of the profit.";
        public const string LongDistanceCaravanSuccessLog = "{=AQ_2aFLWP3}The caravan of {QUEST_GIVER.LINK} reached {DESTINATION} and sold its goods for {EARNED}{GOLD_ICON}. The trip made a profit of {PROFIT}{GOLD_ICON} and your share is {SHARE}{GOLD_ICON}.";
        public const string LongDistanceCaravanRedirectLog = "{=AQ_7DkphCy}{OLD_DESTINATION} can no longer be reached safely. The caravan is heading to {DESTINATION} instead.";
        public const string LongDistanceCaravanNoDestinationLog = "{=AQ_dgTZCFa}No town is left where the caravan of {QUEST_GIVER.LINK} can sell its goods safely. {?QUEST_GIVER.GENDER}She{?}He{\\?} recalled the caravan and canceled your agreement.";
        // Vanilla id.
        public const string LongDistanceCaravanLostTrackLog = "{=y62dyzH6}You have lost the track of caravan. Your agreement with {ISSUE_GIVER.LINK} is failed.";
        // Vanilla id.
        public const string LongDistanceCaravanDestroyedByBanditsLog = "{=MhvyTcrH}The caravan is destroyed by some bandits. Your agreement with {ISSUE_GIVER.LINK} is failed.";
        // Vanilla id.
        public const string LongDistanceCaravanDestroyedLog = "{=zk9QyKIz}The caravan was destroyed. Quest failed.";
        // Vanilla id.
        public const string LongDistanceCaravanAttackedByPlayerLog = "{=Rd3m5kyk}You have attacked the caravan.";
        // Vanilla id.
        public const string LongDistanceCaravanTimeoutLog = "{=pUrSIed8}You have failed to escort the caravan to its destination.";
        // Vanilla id.
        public const string LongDistanceCaravanOfferDialog = "{=TdwKwExD}Thank you. You can find the caravan just outside the settlement.[if:convo_grateful]";
        // Vanilla id.
        public const string LongDistanceCaravanDiscussDialog = "{=vtZYmAaR}I feel good knowing that you're looking after my caravan. Safe journeys, my friend![if:convo_grateful]";
        // Vanilla id.
        public const string LongDistanceCaravanGreetingDialog = "{=FpUybbSk}Greetings. This caravan is owned by {MERCHANT.LINK}. We trade under the protection of {PROTECTOR.LINK}, master of {HOMETOWN}. How may we help you?[if:convo_normal]";
        // Vanilla id.
        public const string LongDistanceCaravanAboutTaskOption = "{=ZAqEJI9T}About the task {QUEST_GIVER.LINK} gave me.";
        // Vanilla id.
        public const string LongDistanceCaravanFollowUsDialog = "{=heWYa9Oq}I feel safe knowing that you're looking after us. Please continue to follow us my friend!";
        // Vanilla id.
        public const string LongDistanceCaravanFarewellOption = "{=1IJouNaM}Carry on, then. Farewell.";
        // Vanilla id.
        public const string LongDistanceCaravanLootOption = "{=WOBy5UfY}Hand over your goods, or die!";
        // Vanilla id.
        public const string LongDistanceCaravanLootRefuseDialog = "{=QNaKmkt9}We're paid to guard this caravan. If you want to rob it, it's going to be over our dead bodies![if:convo_angry][ib:aggressive]";
        // Vanilla id.
        public const string LongDistanceCaravanAttackOption = "{=EhxS7NQ4}So be it. Attack!";
        // Vanilla id.
        public const string LongDistanceCaravanGoInPeaceOption = "{=bfPsE9M1}You must have misunderstood me. Go in peace.";
        // Vanilla id.
        public const string LongDistanceCaravanTravelingNotice = "{=OjI8uGFa}We are traveling to {SETTLEMENT_NAME}.";
        // Vanilla id.
        public const string LongDistanceCaravanMovingLog = "{=QDpfYm4c}The caravan is moving to {SETTLEMENT_NAME}.";
        // Vanilla id.
        public const string LongDistanceCaravanLoseSightNotice = "{=2y9DhzCR}You are about to lose sight of the caravan. Find the caravan before they are in danger!";
        // Vanilla id.
        public const string LongDistanceCaravanRaidersName = "{=u1Pkt4HC}Raiders";
        // Vanilla id.
        public const string LongDistanceCaravanAmbushedNotice = "{=o8uAzFaJ}The caravan you are protecting is ambushed by raiders!";
        // Vanilla id.
        public const string LongDistanceCaravanEnteredLog = "{=0wj3HIbh}Caravan entered {SETTLEMENT_LINK}.";
        #endregion

        #region Fund the Walls
        public const string FundTheWallsHandOverOption = "{=AQ_gwZpb5w}Hand over {AMOUNT}{GOLD_ICON} for the construction works";
        // Vanilla id.
        public const string FundTheWallsNotEnoughGold = "{=d0kbtGYn}You don't have enough gold.";
        public const string FundTheWallsTitle = "{=AQ_h9vC8AC}Fund the Walls of {SETTLEMENT}";
        public const string FundTheWallsIssueDescription = "{=AQ_IVFYrru}{QUEST_GIVER.NAME} wants the construction works in {SETTLEMENT} to go faster, but the treasury is short of coin.";
        public const string FundTheWallsIssueBrief = "{=AQ_0Mffnlm}The builders in {SETTLEMENT} are working too slowly. Every denar in my treasury goes to my men in the field, and the masons do not work for promises.";
        public const string FundTheWallsIssueAccept = "{=AQ_2LPPl97}Perhaps I can help. What do you need?";
        public const string FundTheWallsIssueSolutionExplanation = "{=AQ_B4pZqEB}Take {AMOUNT}{GOLD_ICON} to {SETTLEMENT} and hand it to my steward there. With that coin the works will go much faster. I will not forget such generosity.";
        public const string FundTheWallsIssueSolutionAccept = "{=AQ_Z8hQlbU}I will bring {AMOUNT}{GOLD_ICON} to {SETTLEMENT}.";
        public const string FundTheWallsAcceptedLog = "{=AQ_1t9xD52}{QUEST_GIVER.LINK}, the {?QUEST_GIVER.GENDER}lady{?}lord{\\?} of {SETTLEMENT}, asked you to fund the construction works there. Go to {SETTLEMENT} and hand over {AMOUNT}{GOLD_ICON} to the steward.";
        public const string FundTheWallsSuccessLog = "{=AQ_sJC2SY2}You handed over {AMOUNT}{GOLD_ICON} to the steward of {SETTLEMENT}. The construction works will go faster.";
        public const string FundTheWallsTimeoutLog = "{=AQ_XUmYvlU}You did not bring the coin to {SETTLEMENT} in time. {QUEST_GIVER.LINK} is disappointed.";
        public const string FundTheWallsOfferThanks = "{=AQ_XulpSCr}Thank you. My steward in {SETTLEMENT} will be expecting you.";
        public const string FundTheWallsDiscussQuestion = "{=AQ_wez8WeZ}Has the coin reached {SETTLEMENT} yet?";
        public const string FundTheWallsDiscussAnswer = "{=AQ_rmmM5jw}Not yet. I am on my way.";
        #endregion

        #region Donate Elite Rangers
        // Passed as a string to CampaignGameStarter.AddGameMenuOption; COUNT and TROOP are set in the option's condition.
        public const string DonateEliteRangersHandOverOption = "{=AQ_Hv3mR8t}Hand over {COUNT} {TROOP} to the garrison";
        public const string DonateEliteRangersNotEnoughTroopsTooltip = "{=AQ_Nq7wE2c}You need {COUNT} healthy {TROOP} in your party.";
        public const string DonateEliteRangersTitle = "{=AQ_Lk2rP9d}Elite Rangers for {SETTLEMENT}";
        public const string DonateEliteRangersDescription = "{=AQ_Wf6bT1x}{QUEST_GIVER.NAME} wants {COUNT} {TROOP} to guard the walls of {SETTLEMENT}.";
        public const string DonateEliteRangersIssueBrief = "{=AQ_Zm8cY4h}The walls of {SETTLEMENT} are only as strong as the bows behind them. My garrison needs real marksmen, not farm boys with hunting bows, and training such men takes years I do not have.";
        public const string DonateEliteRangersIssueAccept = "{=AQ_Pj3vN6s}What kind of men do you need?";
        public const string DonateEliteRangersSolutionExplanation = "{=AQ_Gx5tK8m}Bring {COUNT} {TROOP} to {SETTLEMENT} and leave them with the garrison. I will pay you {REWARD}{GOLD_ICON} for them, more than it cost you to train them.";
        public const string DonateEliteRangersSolutionAccept = "{=AQ_Rb1yQ7w}I will bring {COUNT} {TROOP} to {SETTLEMENT}.";
        public const string DonateEliteRangersAcceptedLog = "{=AQ_Cs9dL2f}{QUEST_GIVER.LINK}, the {?QUEST_GIVER.GENDER}lady{?}lord{\\?} of {SETTLEMENT}, asked you to bring {COUNT} {TROOP} to the garrison there. {?QUEST_GIVER.GENDER}She{?}He{\\?} will pay you {REWARD}{GOLD_ICON} when they are handed over.";
        public const string DonateEliteRangersSuccessLog = "{=AQ_Ye4hV7n}You left {COUNT} {TROOP} with the garrison of {SETTLEMENT}.";
        public const string DonateEliteRangersTimeoutLog = "{=AQ_Kw6pX3j}You did not bring the troops to {SETTLEMENT} in time. {QUEST_GIVER.LINK} is disappointed.";
        public const string DonateEliteRangersOfferAccepted = "{=AQ_Uf2mB8q}Good. The garrison commander in {SETTLEMENT} will take them in.";
        public const string DonateEliteRangersDiscussAsk = "{=AQ_Ia7sD5r}Have you brought the {TROOP} to {SETTLEMENT}?";
        public const string DonateEliteRangersDiscussNotYet = "{=AQ_Oq3nF9k}Not yet. Men like that are not found in every village.";
        #endregion
    }
}
