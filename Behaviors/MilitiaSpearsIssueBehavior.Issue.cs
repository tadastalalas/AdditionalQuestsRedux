using AdditionalQuestsRedux.Compat;
using AdditionalQuestsRedux.Core;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Issues;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace AdditionalQuestsRedux.Behaviors;

public partial class MilitiaSpearsIssueBehavior
{
    internal class HeadmanNeedsMilitiaWeaponsIssue : IssueBase
    {
        public HeadmanNeedsMilitiaWeaponsIssue(Hero issueOwner) : base(issueOwner, CampaignTime.DaysFromNow(QuestBalance.IssueDurationDays))
        {
        }

        public override TextObject Title => Texts.MilitiaSpearsTitle(IssueSettlement.Name);

        public override TextObject Description => Texts.MilitiaSpearsDescription(IssueSettlement.Name);

        public override TextObject IssueBriefByIssueGiver => Texts.MilitiaSpearsIssueBrief;

        public override TextObject IssueAcceptByPlayer => Texts.MilitiaSpearsIssueAccept;

        public override TextObject IssueQuestSolutionExplanationByIssueGiver =>
            Texts.MilitiaSpearsIssueSolution(QuestBalance.MilitiaSpearsNeededSpears, RewardGold);

        public override TextObject IssueQuestSolutionAcceptByPlayer => Texts.MilitiaSpearsIssueSolutionAccept;

        public override TextObject IssueAsRumorInSettlement => Texts.MilitiaSpearsRumor(IssueOwner.CharacterObject);

        // Only Governors Handle Issues sees an alternative solution; players never get a "send companion" option.
        public override bool IsThereAlternativeSolution => GhiCompat.IsHandling(this);

        public override bool IsThereLordSolution => false;

        protected override int RewardGold =>
            QuestBalance.MilitiaSpearsBaseRewardGold + QuestHelpers.GetAverageTownPriceOfWeaponClass(WeaponClass.OneHandedPolearm) * QuestBalance.MilitiaSpearsNeededSpears;

        // Read by Governors Handle Issues: garrison troops, skill check and governor XP.
        public override int AlternativeSolutionBaseNeededMenCount => QuestBalance.MilitiaSpearsGhiNeededMen;

        protected override int CompanionSkillRewardXP => QuestBalance.MilitiaSpearsGhiSkillXp;

        public override (SkillObject, int) GetAlternativeSolutionSkill(Hero hero) =>
            QuestHelpers.GetBetterSkill(hero, DefaultSkills.Trade, DefaultSkills.Crafting, QuestBalance.MilitiaSpearsGhiSkill);

        // Governors Handle Issues pays RewardGold and applies RelationshipChangeWithIssueOwner itself.
        protected override void AlternativeSolutionEndWithSuccessConsequence()
        {
            if (IssueOwner == null)
                return;
            ApplySuccessWorldEffects(IssueOwner);
            RelationshipChangeWithIssueOwner = QuestBalance.MilitiaSpearsSuccessRelation;
        }

        protected override void AlternativeSolutionEndWithFailureConsequence()
        {
            if (IssueOwner == null)
                return;
            ApplyFailWorldEffects(IssueOwner);
            RelationshipChangeWithIssueOwner = QuestBalance.MilitiaSpearsFailRelation;
        }

        // Same rewards the player gets from the quest (traits stay player-only), given to the AI lord who resolved the issue.
        internal void ApplyAiLordRewards(Hero solver)
        {
            if (IssueOwner == null)
                return;
            ApplySuccessWorldEffects(IssueOwner);
            ChangeRelationAction.ApplyRelationChangeBetweenHeroes(solver, IssueOwner, QuestBalance.MilitiaSpearsSuccessRelation, false);
            GiveGoldAction.ApplyBetweenCharacters(null, solver, RewardGold, true);
        }

        public override IssueFrequency GetFrequency() => IssueFrequency.VeryCommon;

        public override bool IssueStayAliveConditions() => IssueOwner.CurrentSettlement.Militia <= QuestBalance.MilitiaSpearsMaxVillageMilitia;

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

        protected override QuestBase GenerateIssueQuest(string questId) =>
            new HeadmanNeedsMilitiaWeaponsQuest(questId, IssueOwner, CampaignTime.DaysFromNow(QuestBalance.MilitiaSpearsQuestDurationDays), RewardGold, QuestBalance.MilitiaSpearsNeededSpears);

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
