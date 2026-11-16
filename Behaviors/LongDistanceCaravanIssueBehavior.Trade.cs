using System.Collections.Generic;
using AdditionalQuestsRedux.Core;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace AdditionalQuestsRedux.Behaviors;

// Real market trade shared by the escorted caravan, the AI lord path and Governors Handle Issues.
// Prices come from the vanilla market model without a trading party, so every path sees the same prices.
public partial class LongDistanceCaravanIssueBehavior
{
    private static float AverageTownDistance =>
        Campaign.Current.GetAverageDistanceBetweenClosestTwoTownsWithNavigationType(MobileParty.NavigationType.Default);

    private static float GetTownDistance(Settlement from, Settlement to) =>
        Campaign.Current.Models.MapDistanceModel.GetDistance(from, to, false, false, MobileParty.NavigationType.Default);

    private static bool HasEnoughBudget(Hero merchant) =>
        merchant.Gold >= QuestBalance.LongCaravanMerchantReserve + QuestBalance.LongCaravanMinBudget;

    private static int GetBudget(Hero merchant) =>
        MathF.Max(0, MathF.Min(merchant.Gold - QuestBalance.LongCaravanMerchantReserve, QuestBalance.LongCaravanMaxBudget));

    // Food is excluded: the caravan would eat it on the road.
    private static bool IsCargoItem(ItemObject item) =>
        item.IsTradeGood && !item.IsFood && !item.NotMerchandise && item.Value > 0 && item.ItemCategory != null;

    private static bool IsCargoElement(ItemRosterElement element) =>
        element.Amount > 0 && element.EquipmentElement.ItemModifier == null && IsCargoItem(element.EquipmentElement.Item);

    private static bool IsValidDestination(Settlement settlement, IFaction caravanFaction) =>
        settlement.IsTown && settlement.IsActive && !settlement.IsUnderSiege && !FactionManager.IsAtWarAgainstFaction(settlement.MapFaction, caravanFaction);

    // Strict: path distance within the configured band and at least one item with the minimum margin.
    // Relaxed (console command only): any valid town at least the minimum distance away.
    internal static Town? FindDestination(Town origin, bool strict)
    {
        IFaction faction = origin.Settlement.MapFaction;
        float average = AverageTownDistance;
        float minDistance = average * QuestBalance.LongCaravanMinDistanceTowns;
        float maxDistance = average * QuestBalance.LongCaravanMaxDistanceTowns;
        List<Town> candidates = [];
        MBReadOnlyList<Town> towns = Town.AllTowns;
        for (int i = 0; i < towns.Count; i++)
        {
            Town town = towns[i];
            if (town == origin || !IsValidDestination(town.Settlement, faction))
                continue;
            // Path distance is never shorter than the straight line, so this skips most towns cheaply.
            if (strict && town.Settlement.Position.DistanceSquared(origin.Settlement.Position) > maxDistance * maxDistance)
                continue;
            float distance = GetTownDistance(origin.Settlement, town.Settlement);
            if (distance < minDistance)
                continue;
            if (strict && (distance > maxDistance || GetBestMargin(origin, town) < QuestBalance.LongCaravanMinMargin))
                continue;
            candidates.Add(town);
        }
        return candidates.Count > 0 ? candidates[MBRandom.RandomInt(candidates.Count)] : null;
    }

    private static float GetBestMargin(Town origin, Town destination)
    {
        float best = 0f;
        ItemRoster market = origin.Settlement.ItemRoster;
        for (int i = 0; i < market.Count; i++)
        {
            ItemRosterElement element = market.GetElementCopyAtIndex(i);
            if (!IsCargoElement(element))
                continue;
            int buy = origin.GetItemPrice(element.EquipmentElement);
            int sell = destination.GetItemPrice(element.EquipmentElement, null, true);
            float margin = (float)sell / buy;
            if (margin > best)
                best = margin;
        }
        return best;
    }

    // Sell price after the planned units of the same category are already in the town's store (vanilla adds item value per unit).
    private static int GetSimulatedSellPrice(Town town, ItemObject item, Dictionary<ItemCategory, int> addedValue)
    {
        ItemData data = town.MarketData.GetCategoryData(item.ItemCategory);
        addedValue.TryGetValue(item.ItemCategory, out int extra);
        return Campaign.Current.Models.TradeItemPriceFactorModel.GetPrice(new EquipmentElement(item), null, null, true, data.InStoreValue + extra, data.Supply, data.Demand);
    }

    // Buys one unit at a time, always the item with the best margin, so prices react to every purchase like vanilla trade.
    // Moves the units from the origin market into cargo and returns the price paid. The caller pays the town.
    private static int BuyCargo(Town origin, Town destination, int budget, float freeWeight, ItemRoster cargo)
    {
        ItemRoster market = origin.Settlement.ItemRoster;
        Dictionary<ItemCategory, int> addedSellValue = [];
        Dictionary<ItemObject, int> spentPerItem = [];
        int maxItemSpend = (int)(budget * QuestBalance.LongCaravanMaxItemBudgetShare);
        int maxPlannedSales = (int)(destination.Gold * QuestBalance.LongCaravanMaxDestinationGoldShare);
        int spent = 0;
        int plannedSales = 0;
        for (int unit = 0; unit < QuestBalance.LongCaravanMaxPurchaseUnits; unit++)
        {
            int bestIndex = -1;
            float bestMargin = QuestBalance.LongCaravanMinMargin;
            int bestBuy = 0;
            int bestSell = 0;
            for (int i = 0; i < market.Count; i++)
            {
                ItemRosterElement element = market.GetElementCopyAtIndex(i);
                if (!IsCargoElement(element))
                    continue;
                ItemObject item = element.EquipmentElement.Item;
                if (item.Weight > freeWeight)
                    continue;
                int buy = origin.GetItemPrice(element.EquipmentElement);
                spentPerItem.TryGetValue(item, out int itemSpent);
                if (spent + buy > budget || itemSpent + buy > maxItemSpend)
                    continue;
                int sell = GetSimulatedSellPrice(destination, item, addedSellValue);
                if (plannedSales + sell > maxPlannedSales)
                    continue;
                float margin = (float)sell / buy;
                if (margin < bestMargin)
                    continue;
                bestIndex = i;
                bestMargin = margin;
                bestBuy = buy;
                bestSell = sell;
            }
            if (bestIndex < 0)
                break;

            EquipmentElement chosen = market.GetElementCopyAtIndex(bestIndex).EquipmentElement;
            ItemObject chosenItem = chosen.Item;
            market.AddToCounts(chosen, -1);
            cargo.AddToCounts(chosen, 1);
            spent += bestBuy;
            plannedSales += bestSell;
            freeWeight -= chosenItem.Weight;
            spentPerItem.TryGetValue(chosenItem, out int chosenSpent);
            spentPerItem[chosenItem] = chosenSpent + bestBuy;
            addedSellValue.TryGetValue(chosenItem.ItemCategory, out int chosenAdded);
            addedSellValue[chosenItem.ItemCategory] = chosenAdded + chosenItem.Value;
        }
        return spent;
    }

    // Same tax vanilla SellItemsAction takes when a town sells goods: paid by the town to its owner's trade tax.
    private static void ApplyTownSaleTax(Town town, int saleTotal)
    {
        if (saleTotal <= 0)
            return;
        int tax = MBRandom.RoundRandomized(saleTotal * Campaign.Current.Models.SettlementTaxModel.GetTownTaxRatio(town));
        town.ChangeGold(-tax);
        town.TradeTaxAccumulated += (int)Campaign.Current.Models.SettlementTaxModel.GetTownCommissionChangeBasedOnSecurity(town, tax);
    }

    // Sells cargo (and food when asked) unit by unit while the town can pay. Pack animals are never sold.
    // Moves the units into the town market and returns the gold earned. The caller collects it from the town.
    private static int SellCargo(Town town, ItemRoster roster, bool includeFood)
    {
        ItemRoster market = town.Settlement.ItemRoster;
        int earned = 0;
        // Backwards: a removed element is replaced by the last one, which was already visited.
        for (int i = roster.Count - 1; i >= 0; i--)
        {
            ItemRosterElement element = roster.GetElementCopyAtIndex(i);
            ItemObject item = element.EquipmentElement.Item;
            bool isFood = includeFood && item.IsFood && element.EquipmentElement.ItemModifier == null;
            if (!IsCargoElement(element) && !isFood)
                continue;
            for (int unit = 0; unit < element.Amount; unit++)
            {
                int price = town.GetItemPrice(element.EquipmentElement, null, true);
                if (earned + price > town.Gold)
                    break;
                roster.AddToCounts(element.EquipmentElement, -1);
                market.AddToCounts(element.EquipmentElement, 1);
                earned += price;
            }
        }
        return earned;
    }

    // Cheapest market food first; if the market runs short, grain from the town's food stocks at the grain price.
    // Moves the food into the roster and returns the price paid. The caller pays the town.
    private static int BuyFood(Town town, ItemRoster roster, int units)
    {
        ItemRoster market = town.Settlement.ItemRoster;
        int spent = 0;
        while (units > 0)
        {
            int cheapestIndex = -1;
            int cheapestPrice = int.MaxValue;
            for (int i = 0; i < market.Count; i++)
            {
                ItemRosterElement element = market.GetElementCopyAtIndex(i);
                if (element.Amount <= 0 || !element.EquipmentElement.Item.IsFood || element.EquipmentElement.ItemModifier != null)
                    continue;
                int price = town.GetItemPrice(element.EquipmentElement);
                if (price < cheapestPrice)
                {
                    cheapestIndex = i;
                    cheapestPrice = price;
                }
            }
            if (cheapestIndex < 0)
                break;
            EquipmentElement food = market.GetElementCopyAtIndex(cheapestIndex).EquipmentElement;
            market.AddToCounts(food, -1);
            roster.AddToCounts(food, 1);
            spent += cheapestPrice;
            units--;
        }

        int fromStocks = MathF.Min(units, MathF.Floor(town.FoodStocks));
        if (fromStocks > 0)
        {
            town.FoodStocks -= fromStocks;
            roster.AddToCounts(DefaultItems.Grain, fromStocks);
            spent += fromStocks * town.GetItemPrice(DefaultItems.Grain);
        }
        return spent;
    }

    // AI lord and Governors Handle Issues path: the merchant buys at home and sells at the destination for real.
    // Unsold leftovers go back to the home market. Returns the merchant's profit (may be negative).
    internal static int RunInstantTrade(Hero merchant, Town origin, Town destination)
    {
        int budget = GetBudget(merchant);
        if (budget <= 0)
            return 0;

        ItemRoster cargo = new();
        int spent = BuyCargo(origin, destination, budget, float.MaxValue, cargo);
        if (spent <= 0)
            return 0;
        GiveGoldAction.ApplyForCharacterToSettlement(merchant, origin.Settlement, spent, true);
        ApplyTownSaleTax(origin, spent);

        int earned = SellCargo(destination, cargo, false);
        GiveGoldAction.ApplyForSettlementToCharacter(destination.Settlement, merchant, earned, true);
        int returned = SellCargo(origin, cargo, false);
        GiveGoldAction.ApplyForSettlementToCharacter(origin.Settlement, merchant, returned, true);
        return earned + returned - spent;
    }

    // Next town on the way: the destination if it is close, else the valid town within one leg that gets closest to it.
    private static Settlement FindNextStop(Settlement from, Settlement destination, IFaction caravanFaction)
    {
        float average = AverageTownDistance;
        float remaining = GetTownDistance(from, destination);
        if (remaining <= average * QuestBalance.LongCaravanDirectLegTowns)
            return destination;

        float maxLeg = average * QuestBalance.LongCaravanMaxLegTowns;
        Settlement best = destination;
        float bestRemaining = remaining;
        MBReadOnlyList<Town> towns = Town.AllTowns;
        for (int i = 0; i < towns.Count; i++)
        {
            Settlement stop = towns[i].Settlement;
            if (stop == from || stop == destination || !IsValidDestination(stop, caravanFaction))
                continue;
            if (stop.Position.DistanceSquared(from.Position) > maxLeg * maxLeg || GetTownDistance(from, stop) > maxLeg)
                continue;
            float stopRemaining = GetTownDistance(stop, destination);
            if (stopRemaining < bestRemaining)
            {
                best = stop;
                bestRemaining = stopRemaining;
            }
        }
        return best;
    }

    // Walks the same route the caravan will take and converts it to days, plus the waits in towns and slack.
    private static float EstimateTripDays(Settlement origin, Settlement destination, IFaction caravanFaction)
    {
        float distance = 0f;
        int legs = 0;
        Settlement current = origin;
        while (current != destination && legs < QuestBalance.LongCaravanMaxRouteLegs)
        {
            Settlement next = FindNextStop(current, destination, caravanFaction);
            distance += GetTownDistance(current, next);
            current = next;
            legs++;
        }
        float hoursPerDay = CampaignTime.HoursInDay;
        return distance / (QuestBalance.LongCaravanEstimatedSpeed * hoursPerDay)
               + legs * QuestBalance.LongCaravanSettlementWaitHours / hoursPerDay
               + QuestBalance.LongCaravanExtraDays;
    }
    
    private static float GetQuestDurationDays(Settlement origin, Settlement destination, IFaction caravanFaction) =>
        MathF.Max(QuestBalance.LongCaravanMinQuestDays, EstimateTripDays(origin, destination, caravanFaction) * QuestBalance.LongCaravanDurationFactor);
}
