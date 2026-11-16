using AdditionalQuestsRedux.Behaviors;
using TaleWorlds.SaveSystem;

namespace AdditionalQuestsRedux.Data;

// Save ids are final: base 585820 + local id. Local ids reproduce the per-quest definers of v1.2.0
// (585820 Militia Spears, 585850 Starving Town, 585860 Town Uprising, 585870 Bandit Army), so old saves load.
// Append only; never change or reuse an id.
internal sealed class QuestSaveDefiner : SaveableTypeDefiner
{
    public QuestSaveDefiner() : base(585820)
    {
    }

    protected override void DefineClassTypes()
    {
        AddClassDefinition(typeof(MilitiaSpearsIssueBehavior.HeadmanNeedsMilitiaWeaponsIssue), 1);
        AddClassDefinition(typeof(MilitiaSpearsIssueBehavior.HeadmanNeedsMilitiaWeaponsQuest), 2);
        AddClassDefinition(typeof(StarvingTownIssueBehavior.StarvingTownNeedsFoodIssue), 31);
        AddClassDefinition(typeof(StarvingTownIssueBehavior.StarvingTownNeedsFoodQuest), 32);
        // 41, 42: removed Town Uprising issue/quest (never offered in a release). Never reuse.
        AddClassDefinition(typeof(BanditArmyIssueBehavior.VillageBanditArmyRaidIssue), 51);
        AddClassDefinition(typeof(BanditArmyIssueBehavior.VillageBanditArmyRaidQuest), 52);
        AddClassDefinition(typeof(LongDistanceCaravanIssueBehavior.LongDistanceCaravanIssue), 61);
        AddClassDefinition(typeof(LongDistanceCaravanIssueBehavior.LongDistanceCaravanQuest), 62);
        AddClassDefinition(typeof(FundTheWallsIssueBehavior.FundTheWallsIssue), 71);
        AddClassDefinition(typeof(FundTheWallsIssueBehavior.FundTheWallsQuest), 72);
        // 81, 82: clashed with another mod's save id 585901 (crash at startup). Never reuse.
        AddClassDefinition(typeof(DonateEliteRangersIssueBehavior.DonateEliteRangersIssue), 1001);
        AddClassDefinition(typeof(DonateEliteRangersIssueBehavior.DonateEliteRangersQuest), 1002);
    }
}
