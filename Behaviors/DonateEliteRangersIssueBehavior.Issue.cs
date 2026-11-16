using AdditionalQuestsRedux.Core;
using AdditionalQuestsRedux.Diagnostics;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Issues;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace AdditionalQuestsRedux.Behaviors;

public partial class DonateEliteRangersIssueBehavior
{
    internal class DonateEliteRangersIssue : IssueBase
    {
        [SaveableField(1)]
        private Settlement _settlement;

        [SaveableField(2)]
        private CharacterObject _troop;

        [SaveableField(3)]
        private int _troopCount;

        [SaveableField(4)]
        private int _rewardGold;

        public DonateEliteRangersIssue(Hero issueOwner, Settlement settlement, CharacterObject troop)
            : base(issueOwner, CampaignTime.DaysFromNow(QuestBalance.IssueDurationDays))
        {
            _settlement = settlement;
            _troop = troop;
            _troopCount = GetRangerCount(settlement);
            _rewardGold = GetRewardGold(troop, _troopCount);
        }

        internal Settlement Settlement => _settlement;

        internal CharacterObject Troop => _troop;

        internal int TroopCount => _troopCount;

        protected override int RewardGold => _rewardGold;

        public override TextObject Title => Texts.DonateEliteRangersTitle(_settlement.Name);

        public override TextObject Description =>
            Texts.DonateEliteRangersDescription(IssueOwner.CharacterObject, _troopCount, _troop.Name, _settlement.Name);

        public override TextObject IssueBriefByIssueGiver => Texts.DonateEliteRangersIssueBrief(_settlement.Name);

        public override TextObject IssueAcceptByPlayer => Texts.DonateEliteRangersIssueAccept;

        public override TextObject IssueQuestSolutionExplanationByIssueGiver =>
            Texts.DonateEliteRangersSolutionExplanation(_troopCount, _troop.Name, _settlement.Name, _rewardGold);

        public override TextObject IssueQuestSolutionAcceptByPlayer =>
            Texts.DonateEliteRangersSolutionAccept(_troopCount, _troop.Name, _settlement.Name);

        // Governors Handle Issues only reads notables' issues (IssueManager.GetIssuesInSettlement with notables only), so there is nothing to offer it.
        public override bool IsThereAlternativeSolution => false;

        public override bool IsThereLordSolution => false;

        public override IssueFrequency GetFrequency() => IssueFrequency.Rare;

        // The owner clan must still hold the settlement. Room, food and gold are checked only when the issue is offered.
        public override bool IssueStayAliveConditions() =>
            _settlement.Town != null && _troop != null && IssueOwner.IsAlive && _settlement.OwnerClan == IssueOwner.Clan
            && IssueOwner.Clan != Clan.PlayerClan;

        // Troops are checked when handing them over, not on accept.
        protected override bool CanPlayerTakeQuestConditions(Hero issueGiver, out PreconditionFlags flag, out Hero? relationHero, out SkillObject? skill, out int requiredGold)
        {
            skill = null;
            requiredGold = 0;
            relationHero = null;
            flag = PreconditionFlags.None;
            if (QuestHelpers.IsRelationTooLow(issueGiver))
            {
                flag |= PreconditionFlags.Relation;
                relationHero = issueGiver;
            }
            if (QuestHelpers.IsAtWarWithPlayer(issueGiver))
                flag |= PreconditionFlags.AtWar;
            if (Clan.PlayerClan.Tier < QuestBalance.RangersMinClanTier)
                flag |= PreconditionFlags.ClanTier;
            return flag == PreconditionFlags.None;
        }

        // An AI lord of the same realm whose party has enough healthy troops of the requested type.
        internal bool CanBeDonatedBy(Hero solver, MobileParty party) =>
            solver != IssueOwner && solver.MapFaction == _settlement.MapFaction && GetHealthyCount(party.MemberRoster, _troop) >= _troopCount;

        // Same rewards the player gets from the quest; the troops were already moved into the garrison.
        internal void ApplyAiLordRewards(Hero solver)
        {
            ApplyRewards(solver, IssueOwner, _settlement, _troop, _troopCount, _rewardGold, true);
            AqLog.Debug($"[AQ] Donate Elite Rangers: {solver.Name} left {_troopCount} {_troop.Name} in the garrison of {_settlement.Name}.");
        }

        protected override QuestBase GenerateIssueQuest(string questId) =>
            new DonateEliteRangersQuest(questId, IssueOwner, CampaignTime.DaysFromNow(QuestBalance.RangersQuestDurationDays), _rewardGold, _settlement, _troop, _troopCount);

        protected override void CompleteIssueWithTimedOutConsequences()
        {
        }

        protected override void OnGameLoad()
        {
        }

        protected override void HourlyTick()
        {
        }
    }
}
