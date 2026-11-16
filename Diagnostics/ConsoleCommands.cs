using System;
using System.Collections.Generic;
using System.Reflection;
using AdditionalQuestsRedux.Behaviors;
using AdditionalQuestsRedux.Core;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Issues;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;

namespace AdditionalQuestsRedux.Diagnostics;

internal static class ConsoleCommands
{
    private const string CommandGroup = "additional_quests";
    private const string UninstallCommand = "uninstall";
    private const string GiveBanditArmyCommand = "give_bandit_army";
    private const string GiveMilitiaSpearsCommand = "give_militia_spears";
    private const string GiveStarvingTownCommand = "give_starving_town";
    private const string GiveLongDistanceCaravanCommand = "give_long_distance_caravan";
    private const string GiveFundTheWallsCommand = "give_fund_the_walls";
    private const string GiveDonateEliteRangersCommand = "give_donate_elite_rangers";
    private const string AddTroopsArgument = "troops";
    private const string VillageRequiredMessage = "Enter a village first.";
    private const string TownRequiredMessage = "Enter a town first.";

    // additional_quests.uninstall: stops new issues and cancels every issue/quest of this mod so the save can drop it.
    [CommandLineFunctionality.CommandLineArgumentFunction(UninstallCommand, CommandGroup)]
    internal static string UninstallAdditionalQuests(List<string> args)
    {
        if (!CampaignCheats.CheckCheatUsage(ref CampaignCheats.ErrorType))
            return CampaignCheats.ErrorType;

        Campaign.Current.CampaignBehaviorManager.RemoveBehavior<BanditArmyIssueBehavior>();
        Campaign.Current.CampaignBehaviorManager.RemoveBehavior<MilitiaSpearsIssueBehavior>();
        Campaign.Current.CampaignBehaviorManager.RemoveBehavior<StarvingTownIssueBehavior>();
        Campaign.Current.CampaignBehaviorManager.RemoveBehavior<LongDistanceCaravanIssueBehavior>();
        Campaign.Current.CampaignBehaviorManager.RemoveBehavior<FundTheWallsIssueBehavior>();
        Campaign.Current.CampaignBehaviorManager.RemoveBehavior<DonateEliteRangersIssueBehavior>();

        // Copy first: deactivating removes entries from the live dictionary.
        List<IssueBase> issues = [.. Campaign.Current.IssueManager.Issues.Values];
        Assembly modAssembly = typeof(ConsoleCommands).Assembly;
        for (int i = 0; i < issues.Count; i++)
        {
            if (issues[i].GetType().Assembly == modAssembly)
                Campaign.Current.IssueManager.DeactivateIssue(issues[i]);
        }

        return "Uninstall successful. You may now save, exit and uninstall AdditionalQuests.";
    }
    
    // additional_quests.give_bandit_army: in a village, a rural notable gets the Bandit Army issue (nearest infested hideout, any distance).
    [CommandLineFunctionality.CommandLineArgumentFunction(GiveBanditArmyCommand, CommandGroup)]
    internal static string GiveBanditArmy(List<string> args)
    {
        if (!CampaignCheats.CheckCheatUsage(ref CampaignCheats.ErrorType))
            return CampaignCheats.ErrorType;

        Settlement? village = MobileParty.MainParty.CurrentSettlement;
        if (village is not { IsVillage: true })
            return VillageRequiredMessage;
        if (village.IsRaided || village.IsUnderRaid)
            return "This village is looted or under raid.";

        Settlement? hideout = QuestHelpers.FindNearestInfestedHideout(village, QuestBalance.BanditArmyHideoutMaxDistance)
            ?? QuestHelpers.FindNearestInfestedHideout(village, float.MaxValue);
        if (hideout == null)
            return "No infested hideout on the map.";

        return GiveIssue(village, hero => hero.IsRuralNotable, new PotentialIssueData(
            (in PotentialIssueData _, Hero owner) => new BanditArmyIssueBehavior.VillageBanditArmyRaidIssue(owner, hideout),
            typeof(BanditArmyIssueBehavior.VillageBanditArmyRaidIssue), IssueBase.IssueFrequency.Common));
    }

    // additional_quests.give_militia_spears: in a village, a rural notable gets the Militia Spears issue (village militia lowered so the issue stays alive).
    [CommandLineFunctionality.CommandLineArgumentFunction(GiveMilitiaSpearsCommand, CommandGroup)]
    internal static string GiveMilitiaSpears(List<string> args)
    {
        if (!CampaignCheats.CheckCheatUsage(ref CampaignCheats.ErrorType))
            return CampaignCheats.ErrorType;

        Settlement? village = MobileParty.MainParty.CurrentSettlement;
        if (village is not { IsVillage: true })
            return VillageRequiredMessage;
        if (village.Militia > QuestBalance.MilitiaSpearsTestMilitia)
            village.Militia = QuestBalance.MilitiaSpearsTestMilitia;

        return GiveIssue(village, hero => hero.IsRuralNotable, new PotentialIssueData(
            (in PotentialIssueData _, Hero owner) => new MilitiaSpearsIssueBehavior.HeadmanNeedsMilitiaWeaponsIssue(owner),
            typeof(MilitiaSpearsIssueBehavior.HeadmanNeedsMilitiaWeaponsIssue), IssueBase.IssueFrequency.VeryCommon));
    }

    // additional_quests.give_starving_town: in a town, a merchant gets the Starving Town issue (town food set to 0 so it starves).
    [CommandLineFunctionality.CommandLineArgumentFunction(GiveStarvingTownCommand, CommandGroup)]
    internal static string GiveStarvingTown(List<string> args)
    {
        if (!CampaignCheats.CheckCheatUsage(ref CampaignCheats.ErrorType))
            return CampaignCheats.ErrorType;

        Settlement? town = MobileParty.MainParty.CurrentSettlement;
        if (town is not { IsTown: true })
            return TownRequiredMessage;
        town.Town.FoodStocks = 0;

        return GiveIssue(town, hero => hero.IsMerchant, new PotentialIssueData(
            (in PotentialIssueData _, Hero owner) => new StarvingTownIssueBehavior.StarvingTownNeedsFoodIssue(owner),
            typeof(StarvingTownIssueBehavior.StarvingTownNeedsFoodIssue), IssueBase.IssueFrequency.VeryCommon));
    }

    // additional_quests.give_long_distance_caravan: in a town, a merchant gets the Long Distance Caravan issue (merchant gold raised to the full budget; any distance if no far town fits).
    [CommandLineFunctionality.CommandLineArgumentFunction(GiveLongDistanceCaravanCommand, CommandGroup)]
    internal static string GiveLongDistanceCaravan(List<string> args)
    {
        if (!CampaignCheats.CheckCheatUsage(ref CampaignCheats.ErrorType))
            return CampaignCheats.ErrorType;

        Settlement? town = MobileParty.MainParty.CurrentSettlement;
        if (town is not { IsTown: true })
            return TownRequiredMessage;
        if (town.IsUnderSiege)
            return "This town is under siege.";

        Town? destination = LongDistanceCaravanIssueBehavior.FindDestination(town.Town, true)
            ?? LongDistanceCaravanIssueBehavior.FindDestination(town.Town, false);
        if (destination == null)
            return "No town at peace with this town pays more for its goods.";

        return GiveIssue(town, hero => hero.IsMerchant, new PotentialIssueData(
            (in PotentialIssueData _, Hero owner) =>
            {
                int neededGold = QuestBalance.LongCaravanMerchantReserve + QuestBalance.LongCaravanMaxBudget;
                if (owner.Gold < neededGold)
                    owner.ChangeHeroGold(neededGold - owner.Gold);
                return new LongDistanceCaravanIssueBehavior.LongDistanceCaravanIssue(owner, destination.Settlement);
            },
            typeof(LongDistanceCaravanIssueBehavior.LongDistanceCaravanIssue), IssueBase.IssueFrequency.Rare));
    }

    // additional_quests.give_fund_the_walls: in a town or castle, the owner clan's leader gets the Fund the Walls issue (construction reserve set to 0 if it is too high).
    [CommandLineFunctionality.CommandLineArgumentFunction(GiveFundTheWallsCommand, CommandGroup)]
    internal static string GiveFundTheWalls(List<string> args)
    {
        if (!CampaignCheats.CheckCheatUsage(ref CampaignCheats.ErrorType))
            return CampaignCheats.ErrorType;

        Settlement? settlement = MobileParty.MainParty.CurrentSettlement;
        if (settlement is not { IsFortification: true })
            return "Enter a town or castle first.";
        Hero? owner = settlement.OwnerClan?.Leader;
        if (owner == null || !owner.IsAlive || owner.Clan == Clan.PlayerClan)
            return "The owner clan's leader must be alive and not in your clan.";
        if (owner.Issue != null && !owner.Issue.IsOngoingWithoutQuest)
            return $"{owner.Name} already has a quest in progress.";
        if (settlement.Town.BoostBuildingProcess >= QuestBalance.FundWallsMaxReserve)
            settlement.Town.BoostBuildingProcess = 0;

        return CreateIssue(owner, settlement, new PotentialIssueData(
            (in PotentialIssueData _, Hero issueOwner) => new FundTheWallsIssueBehavior.FundTheWallsIssue(issueOwner, settlement),
            typeof(FundTheWallsIssueBehavior.FundTheWallsIssue), IssueBase.IssueFrequency.Rare));
    }

    // additional_quests.give_donate_elite_rangers: in a town or castle, the owner clan's leader gets the Donate Elite Rangers issue.
    // Optional argument "troops": the requested troops are also added to your party, to test the hand-over.
    [CommandLineFunctionality.CommandLineArgumentFunction(GiveDonateEliteRangersCommand, CommandGroup)]
    internal static string GiveDonateEliteRangers(List<string> args)
    {
        if (!CampaignCheats.CheckCheatUsage(ref CampaignCheats.ErrorType))
            return CampaignCheats.ErrorType;

        Settlement? settlement = MobileParty.MainParty.CurrentSettlement;
        if (settlement is not { IsFortification: true })
            return "Enter a town or castle first.";
        Hero? owner = settlement.OwnerClan?.Leader;
        if (owner == null || !owner.IsAlive || owner.Clan == Clan.PlayerClan)
            return "The owner clan's leader must be alive and not in your clan.";
        if (owner.Issue != null && !owner.Issue.IsOngoingWithoutQuest)
            return $"{owner.Name} already has a quest in progress.";
        CharacterObject? troop = DonateEliteRangersIssueBehavior.FindRangerTroop(owner.Clan);
        if (troop == null)
            return $"{owner.Clan.Culture?.Name} has no ranged troop.";

        string result = CreateIssue(owner, settlement, new PotentialIssueData(
            (in PotentialIssueData _, Hero issueOwner) => new DonateEliteRangersIssueBehavior.DonateEliteRangersIssue(issueOwner, settlement, troop),
            typeof(DonateEliteRangersIssueBehavior.DonateEliteRangersIssue), IssueBase.IssueFrequency.Rare));
        if (args.Count > 0 && args[0] == AddTroopsArgument)
            MobileParty.MainParty.MemberRoster.AddToCounts(troop, DonateEliteRangersIssueBehavior.GetRangerCount(settlement));
        return result;
    }

    // Ignores the MCM toggles and the spawn conditions; the player's take-quest checks (tier, relation, troops) still apply.
    private static string GiveIssue(Settlement settlement, Func<Hero, bool> isGiver, PotentialIssueData issueData)
    {
        Hero? giver = FindIssueGiver(settlement, isGiver);
        return giver == null ? $"No suitable notable without an active quest in {settlement.Name}." : CreateIssue(giver, settlement, issueData);
    }

    private static string CreateIssue(Hero giver, Settlement settlement, PotentialIssueData issueData)
    {
        // Frees the hero: an issue nobody has taken yet is dropped.
        if (giver.Issue != null)
            Campaign.Current.IssueManager.DeactivateIssue(giver.Issue);
        if (giver.Issue != null)
            return $"Could not free {giver.Name}: their current issue is still there.";

        Campaign.Current.IssueManager.CreateNewIssue(in issueData, giver);
        IssueBase? issue = giver.Issue;
        if (issue == null)
        {
            AqLog.Debug($"Test issue not created for {giver.Name} in {settlement.Name}.");
            return $"Could not create the issue for {giver.Name}.";
        }
        if (!issue.IssueStayAliveConditions())
            return $"{giver.Name} got {issue.Title}, but its stay-alive conditions fail, so the game will drop it.";

        AqLog.Debug($"Test issue {issue.Title} given to {giver.Name} in {settlement.Name}.");
        return $"{giver.Name} in {settlement.Name} now has {issue.Title}. Talk to them to take it.";
    }

    // Prefers a notable without an issue; otherwise one whose issue nobody has taken yet.
    private static Hero? FindIssueGiver(Settlement settlement, Func<Hero, bool> isGiver)
    {
        Hero? fallback = null;
        MBReadOnlyList<Hero> notables = settlement.Notables;
        for (int i = 0; i < notables.Count; i++)
        {
            Hero notable = notables[i];
            if (!notable.IsAlive || !isGiver(notable))
                continue;
            if (notable.Issue == null)
                return notable;
            if (notable.Issue.IsOngoingWithoutQuest)
                fallback ??= notable;
        }
        return fallback;
    }
}
