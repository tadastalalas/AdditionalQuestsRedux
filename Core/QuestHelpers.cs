using System;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace AdditionalQuestsRedux.Core;

internal static class QuestHelpers
{
    // Nearest hideout with enough bandit parties to count as infested, within maxDistance (straight line) of the settlement.
    internal static Settlement? FindNearestInfestedHideout(Settlement from, float maxDistance)
    {
        Settlement? result = null;
        float bestDistanceSquared = maxDistance * maxDistance;
        MBReadOnlyList<Hideout> hideouts = Hideout.All;
        for (int i = 0; i < hideouts.Count; i++)
        {
            Hideout hideout = hideouts[i];
            float distanceSquared = hideout.Settlement.GatePosition.DistanceSquared(from.Position);
            if (distanceSquared <= bestDistanceSquared && hideout.IsInfested)
            {
                bestDistanceSquared = distanceSquared;
                result = hideout.Settlement;
            }
        }
        return result;
    }

    // Shared CanPlayerTakeQuestConditions checks; PreconditionFlags itself is protected inside IssueBase.
    internal static bool IsRelationTooLow(Hero issueGiver) => issueGiver.GetRelationWithPlayer() < QuestBalance.MinRelationWithIssueGiver;

    internal static bool IsAtWarWithPlayer(Hero issueGiver) => FactionManager.IsAtWarAgainstFaction(issueGiver.MapFaction, Hero.MainHero.MapFaction);

    internal static bool IsOneHandedPolearm(ItemObject item) => item.PrimaryWeapon?.WeaponClass == WeaponClass.OneHandedPolearm;

    // Relation with every other notable of the issue owner's settlement. solver null = the player
    // (Governors Handle Issues redirects player relation changes to its governor while it resolves an issue).
    internal static void ApplyOtherNotablesRelation(Hero issueOwner, Hero? solver, int relation)
    {
        Settlement? settlement = issueOwner.CurrentSettlement;
        if (settlement == null)
            return;

        MBReadOnlyList<Hero> notables = settlement.Notables;
        for (int i = 0; i < notables.Count; i++)
        {
            Hero notable = notables[i];
            if (notable == issueOwner || notable == solver)
                continue;
            if (solver == null)
                ChangeRelationAction.ApplyPlayerRelation(notable, relation, false, false);
            else
                ChangeRelationAction.ApplyRelationChangeBetweenHeroes(solver, notable, relation, false);
        }
    }

    // Alternative-solution skill check (used by Governors Handle Issues): the hero's better skill of the two.
    internal static (SkillObject, int) GetBetterSkill(Hero hero, SkillObject first, SkillObject second, int requiredValue) =>
        (hero.GetSkillValue(first) >= hero.GetSkillValue(second) ? first : second, requiredValue);

    internal static int CountPlayerItems(Func<ItemObject, bool> match)
    {
        int count = 0;
        ItemRoster roster = MobileParty.MainParty.ItemRoster;
        for (int i = 0; i < roster.Count; i++)
        {
            ItemRosterElement element = roster.GetElementCopyAtIndex(i);
            ItemObject? item = element.EquipmentElement.Item;
            if (item != null && match(item))
                count += element.Amount;
        }
        return count;
    }

    // Removes up to amount matching items from the player's inventory; returns how many were removed.
    internal static int RemovePlayerItems(Func<ItemObject, bool> match, int amount)
    {
        int remaining = amount;
        ItemRoster roster = MobileParty.MainParty.ItemRoster;
        // Backwards: removing a whole stack compacts the roster.
        for (int i = roster.Count - 1; i >= 0 && remaining > 0; i--)
        {
            ItemRosterElement element = roster.GetElementCopyAtIndex(i);
            ItemObject? item = element.EquipmentElement.Item;
            if (item == null || element.Amount <= 0 || !match(item))
                continue;

            int removed = Math.Min(element.Amount, remaining);
            roster.AddToCounts(element.EquipmentElement, -removed);
            remaining -= removed;
        }
        return amount - remaining;
    }

    // Removes up to maxAmount items of the category and prices each unit at priceMultiplier x the world average price.
    // Returns how many were removed; the caller pays goldEarned.
    internal static int SellPlayerItemsOfCategory(ItemCategory category, int maxAmount, int priceMultiplier, out int goldEarned)
    {
        goldEarned = 0;
        int remaining = maxAmount;
        ItemRoster roster = MobileParty.MainParty.ItemRoster;
        for (int i = roster.Count - 1; i >= 0 && remaining > 0; i--)
        {
            ItemRosterElement element = roster.GetElementCopyAtIndex(i);
            ItemObject? item = element.EquipmentElement.Item;
            if (item == null || element.Amount <= 0 || item.ItemCategory != category)
                continue;

            int sold = Math.Min(element.Amount, remaining);
            goldEarned += QuestHelper.GetAveragePriceOfItemInTheWorld(item) * priceMultiplier * sold;
            roster.AddToCounts(element.EquipmentElement, -sold);
            remaining -= sold;
        }
        return maxAmount - remaining;
    }

    // Average buy price of a weapon class across all town markets, weighted by stock.
    internal static int GetAverageTownPriceOfWeaponClass(WeaponClass weaponClass)
    {
        long totalPrice = 0;
        int totalAmount = 0;
        MBReadOnlyList<Settlement> settlements = Settlement.All;
        for (int i = 0; i < settlements.Count; i++)
        {
            Settlement settlement = settlements[i];
            if (!settlement.IsTown)
                continue;

            ItemRoster roster = settlement.ItemRoster;
            for (int j = 0; j < roster.Count; j++)
            {
                ItemRosterElement element = roster.GetElementCopyAtIndex(j);
                if (element.EquipmentElement.Item?.PrimaryWeapon?.WeaponClass != weaponClass)
                    continue;

                totalPrice += (long)settlement.Town.GetItemPrice(element.EquipmentElement) * element.Amount;
                totalAmount += element.Amount;
            }
        }
        return totalAmount == 0 ? 0 : (int)(totalPrice / totalAmount);
    }
}
