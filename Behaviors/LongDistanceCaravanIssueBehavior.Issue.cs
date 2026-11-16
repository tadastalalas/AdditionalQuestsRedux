using AdditionalQuestsRedux.Compat;
using AdditionalQuestsRedux.Core;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Issues;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace AdditionalQuestsRedux.Behaviors;

public partial class LongDistanceCaravanIssueBehavior
{
    internal class LongDistanceCaravanIssue : IssueBase
    {
        [SaveableField(1)]
        private Settlement _destination;

        public LongDistanceCaravanIssue(Hero issueOwner, Settlement destination) : base(issueOwner, CampaignTime.DaysFromNow(QuestBalance.IssueDurationDays))
        {
            _destination = destination;
        }

        public override TextObject Title => Texts.LongDistanceCaravanTitle(_destination.Name);

        public override TextObject Description =>
            Texts.LongDistanceCaravanDescription(IssueOwner.CharacterObject, IssueOwner.CurrentSettlement.Name, _destination.Name);

        public override TextObject IssueBriefByIssueGiver => Texts.LongDistanceCaravanIssueBrief(_destination.Name);

        public override TextObject IssueAcceptByPlayer => Texts.LongDistanceCaravanIssueAccept();

        public override TextObject IssueQuestSolutionExplanationByIssueGiver =>
            Texts.LongDistanceCaravanIssueSolution(_destination.Name, QuestBalance.LongCaravanMinTroops);

        public override TextObject IssueQuestSolutionAcceptByPlayer => Texts.LongDistanceCaravanIssueSolutionAccept();

        public override TextObject IssueAsRumorInSettlement => Texts.LongDistanceCaravanRumor(IssueOwner.CharacterObject, _destination.Name);

        // Only Governors Handle Issues sees an alternative solution; players never get a "send companion" option.
        public override bool IsThereAlternativeSolution => GhiCompat.IsHandling(this);

        public override bool IsThereLordSolution => false;

        // Read by Governors Handle Issues: garrison troops, skill check and governor XP.
        public override int AlternativeSolutionBaseNeededMenCount => QuestBalance.LongCaravanGhiNeededMen;

        protected override int CompanionSkillRewardXP => QuestBalance.LongCaravanGhiSkillXp;

        public override (SkillObject, int) GetAlternativeSolutionSkill(Hero hero) =>
            QuestHelpers.GetBetterSkill(hero, DefaultSkills.Trade, DefaultSkills.Tactics, QuestBalance.LongCaravanGhiSkill);

        // Governors Handle Issues applies RelationshipChangeWithIssueOwner itself; RewardGold is 0, the profit share is paid here.
        // GHI pays its own gold to the player for player-clan governors and to the governor when it solves for AI towns; the share follows the same rule.
        protected override void AlternativeSolutionEndWithSuccessConsequence()
        {
            if (IssueOwner?.CurrentSettlement?.Town is not { } origin)
                return;
            int profit = RunTradeToValidDestination(origin);
            Hero? governor = origin.Governor;
            Hero recipient = governor == null || governor.Clan == Clan.PlayerClan ? Hero.MainHero : governor;
            PayProfitShare(IssueOwner, recipient, profit);
            ApplySuccessWorldEffects(IssueOwner);
            QuestHelpers.ApplyOtherNotablesRelation(IssueOwner, null, QuestBalance.LongCaravanOtherNotablesRelation);
            RelationshipChangeWithIssueOwner = QuestBalance.LongCaravanSuccessRelation;
        }

        protected override void AlternativeSolutionEndWithFailureConsequence()
        {
            if (IssueOwner == null)
                return;
            ApplyFailWorldEffects(IssueOwner, QuestBalance.LongCaravanFailPower, QuestBalance.LongCaravanFailProsperity);
            RelationshipChangeWithIssueOwner = QuestBalance.LongCaravanFailRelation;
        }

        // Same rewards the player gets from the quest, given to the AI lord who resolved the issue.
        // The merchant runs the same real trade; the lord gets the same profit share.
        internal void ApplyAiLordRewards(Hero solver)
        {
            if (IssueOwner?.CurrentSettlement?.Town is not { } origin)
                return;
            int profit = RunTradeToValidDestination(origin);
            PayProfitShare(IssueOwner, solver, profit);
            ApplySuccessWorldEffects(IssueOwner);
            QuestHelpers.ApplyOtherNotablesRelation(IssueOwner, solver, QuestBalance.LongCaravanOtherNotablesRelation);
            ChangeRelationAction.ApplyRelationChangeBetweenHeroes(solver, IssueOwner, QuestBalance.LongCaravanSuccessRelation, false);
            solver.Clan?.AddRenown(QuestBalance.LongCaravanSuccessRenown, false);
        }

        // The destination may have become hostile since the issue appeared; then the merchant picks another far town.
        private int RunTradeToValidDestination(Town origin)
        {
            Town? destination = _destination.Town;
            if (destination == null || !IsValidDestination(_destination, origin.Settlement.MapFaction))
                destination = FindDestination(origin, true) ?? FindDestination(origin, false);
            return destination == null ? 0 : RunInstantTrade(IssueOwner, origin, destination);
        }

        public override IssueFrequency GetFrequency() => IssueFrequency.Rare;

        public override bool IssueStayAliveConditions() =>
            IssueOwner.CurrentSettlement is { IsTown: true, IsUnderSiege: false } origin
            && HasEnoughBudget(IssueOwner)
            && IsValidDestination(_destination, origin.MapFaction);

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
            if (QuestHelpers.IsAtWarWithPlayer(issueGiver) || FactionManager.IsAtWarAgainstFaction(_destination.MapFaction, Hero.MainHero.MapFaction))
                flag |= PreconditionFlags.AtWar;
            if (Clan.PlayerClan.Tier < QuestBalance.LongCaravanMinClanTier)
                flag |= PreconditionFlags.ClanTier;
            if (MobileParty.MainParty.MemberRoster.TotalHealthyCount < QuestBalance.LongCaravanMinTroops)
                flag |= PreconditionFlags.NotEnoughTroops;
            return flag == PreconditionFlags.None;
        }

        // The reward is a share of the real trade profit, paid on arrival, so the quest carries no fixed reward.
        protected override QuestBase GenerateIssueQuest(string questId) =>
            new LongDistanceCaravanQuest(questId, IssueOwner,
                CampaignTime.DaysFromNow(GetQuestDurationDays(IssueOwner.CurrentSettlement, _destination, IssueOwner.CurrentSettlement.MapFaction)),
                RewardGold, IssueOwner.CurrentSettlement, _destination);

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
