using System;
using AdditionalQuestsRedux.Core;
using AdditionalQuestsRedux.Diagnostics;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Issues;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace AdditionalQuestsRedux.Behaviors;

public partial class FundTheWallsIssueBehavior
{
    internal class FundTheWallsIssue : IssueBase
    {
        [SaveableField(1)]
        private Settlement _settlement;

        [SaveableField(2)]
        private int _requestedGold;

        public FundTheWallsIssue(Hero issueOwner, Settlement settlement) : base(issueOwner, CampaignTime.DaysFromNow(QuestBalance.IssueDurationDays))
        {
            _settlement = settlement;
            _requestedGold = GetRequestedGold(settlement.Town);
        }

        internal Settlement Settlement => _settlement;

        public override TextObject Title => Texts.FundTheWallsTitle(_settlement.Name);

        public override TextObject Description => Texts.FundTheWallsIssueDescription(IssueOwner.CharacterObject, _settlement.Name);

        public override TextObject IssueBriefByIssueGiver => Texts.FundTheWallsIssueBrief(_settlement.Name);

        public override TextObject IssueAcceptByPlayer => Texts.FundTheWallsIssueAccept;

        public override TextObject IssueQuestSolutionExplanationByIssueGiver => Texts.FundTheWallsIssueSolutionExplanation(_requestedGold, _settlement.Name);

        public override TextObject IssueQuestSolutionAcceptByPlayer => Texts.FundTheWallsIssueSolutionAccept(_requestedGold, _settlement.Name);

        // Governors Handle Issues only reads notables' issues (IssueManager.GetIssuesInSettlement with notables only), so there is nothing to offer it.
        public override bool IsThereAlternativeSolution => false;

        public override bool IsThereLordSolution => false;

        public override IssueFrequency GetFrequency() => IssueFrequency.Rare;

        // The owner clan must still hold the settlement and the reserve must still be low.
        public override bool IssueStayAliveConditions() =>
            _settlement.Town is { } town && IssueOwner.IsAlive && _settlement.OwnerClan == IssueOwner.Clan
            && IssueOwner.Clan != Clan.PlayerClan && town.BoostBuildingProcess < QuestBalance.FundWallsMaxReserve;

        // Gold is checked when handing it over, not on accept.
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
            if (Clan.PlayerClan.Tier < QuestBalance.FundWallsMinClanTier)
                flag |= PreconditionFlags.ClanTier;
            return flag == PreconditionFlags.None;
        }

        // An AI lord of the same realm who can pay and still keep FundWallsAiMinGoldLeft.
        internal bool CanBeFundedBy(Hero solver) =>
            solver != IssueOwner && solver.MapFaction == _settlement.MapFaction
            && solver.Gold - _requestedGold >= QuestBalance.FundWallsAiMinGoldLeft;

        // Same rewards the player gets from the quest; the AI lord pays the same gold into the same reserve.
        internal void ApplyAiLordRewards(Hero solver)
        {
            if (_settlement.Town is not { } town)
                return;
            int gold = Math.Min(_requestedGold, solver.Gold);
            if (gold <= 0)
                return;
            PayIntoReserve(solver, town, gold);
            ChangeRelationAction.ApplyRelationChangeBetweenHeroes(solver, IssueOwner, GetRelationReward(gold), false);
            solver.Clan?.AddRenown(GetRenownReward(gold), false);
            ApplyInfluenceReward(solver, _settlement, gold);
            AqLog.Debug($"[AQ] Fund the Walls: {solver.Name} paid {gold} into the construction reserve of {_settlement.Name} (now {town.BoostBuildingProcess}).");
        }

        protected override QuestBase GenerateIssueQuest(string questId) =>
            new FundTheWallsQuest(questId, IssueOwner, CampaignTime.DaysFromNow(QuestBalance.FundWallsQuestDurationDays), RewardGold, _settlement, _requestedGold);

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
