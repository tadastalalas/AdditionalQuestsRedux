using AdditionalQuestsRedux.Compat;
using AdditionalQuestsRedux.Core;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Issues;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace AdditionalQuestsRedux.Behaviors;

public partial class StarvingTownIssueBehavior
{
    internal class StarvingTownNeedsFoodIssue : IssueBase
    {
        public StarvingTownNeedsFoodIssue(Hero issueOwner) : base(issueOwner, CampaignTime.DaysFromNow(QuestBalance.IssueDurationDays))
        {
        }

        public override TextObject Title => Texts.StarvingTownTitle(IssueSettlement.Name);

        public override TextObject Description => Texts.StarvingTownDescription(IssueSettlement.Name, IssueOwner.Name);

        public override TextObject IssueBriefByIssueGiver => Texts.StarvingTownIssueBrief;

        public override TextObject IssueAcceptByPlayer => Texts.StarvingTownIssueAccept;

        public override TextObject IssueQuestSolutionExplanationByIssueGiver => Texts.StarvingTownIssueSolution(QuestBalance.StarvingTownNeededFood);

        public override TextObject IssueQuestSolutionAcceptByPlayer => Texts.StarvingTownIssueSolutionAccept;

        public override TextObject IssueAsRumorInSettlement => Texts.StarvingTownRumor(IssueOwner.CharacterObject);

        // Only Governors Handle Issues sees an alternative solution; players never get a "send companion" option.
        public override bool IsThereAlternativeSolution => GhiCompat.IsHandling(this);

        public override bool IsThereLordSolution => false;

        // Read by Governors Handle Issues: garrison troops, skill check and governor XP.
        public override int AlternativeSolutionBaseNeededMenCount => QuestBalance.StarvingTownGhiNeededMen;

        protected override int CompanionSkillRewardXP => QuestBalance.StarvingTownGhiSkillXp;

        public override (SkillObject, int) GetAlternativeSolutionSkill(Hero hero) =>
            QuestHelpers.GetBetterSkill(hero, DefaultSkills.Trade, DefaultSkills.Steward, QuestBalance.StarvingTownGhiSkill);

        // Governors Handle Issues applies RelationshipChangeWithIssueOwner itself; RewardGold is 0 (the player is paid per food unit).
        protected override void AlternativeSolutionEndWithSuccessConsequence()
        {
            if (IssueOwner == null)
                return;
            ApplySuccessWorldEffects(IssueOwner);
            QuestHelpers.ApplyOtherNotablesRelation(IssueOwner, null, QuestBalance.StarvingTownOtherNotablesRelation);
            RelationshipChangeWithIssueOwner = QuestBalance.StarvingTownSuccessRelation;
        }

        protected override void AlternativeSolutionEndWithFailureConsequence()
        {
            if (IssueOwner == null)
                return;
            ApplyFailWorldEffects(IssueOwner);
            RelationshipChangeWithIssueOwner = QuestBalance.StarvingTownFailRelation;
        }

        // Same rewards the player gets from the quest, given to the AI lord who resolved the issue.
        // No gold: the player's gold is payment for the food sold, not a reward.
        internal void ApplyAiLordRewards(Hero solver)
        {
            if (IssueOwner == null)
                return;
            ApplySuccessWorldEffects(IssueOwner);
            QuestHelpers.ApplyOtherNotablesRelation(IssueOwner, solver, QuestBalance.StarvingTownOtherNotablesRelation);
            ChangeRelationAction.ApplyRelationChangeBetweenHeroes(solver, IssueOwner, QuestBalance.StarvingTownSuccessRelation, false);
            solver.Clan?.AddRenown(QuestBalance.StarvingTownSuccessRenown, false);
        }

        public override IssueFrequency GetFrequency() => IssueFrequency.VeryCommon;

        public override bool IssueStayAliveConditions() => IssueOwner.CurrentSettlement.IsStarving;

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
            return flag == PreconditionFlags.None;
        }

        // Payment happens per delivered unit on hand-over, so the quest carries no fixed reward.
        protected override QuestBase GenerateIssueQuest(string questId) =>
            new StarvingTownNeedsFoodQuest(questId, IssueOwner, CampaignTime.DaysFromNow(QuestBalance.StarvingTownQuestDurationDays), RewardGold, QuestBalance.StarvingTownNeededFood);

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
