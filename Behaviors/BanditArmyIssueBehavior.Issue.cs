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

public partial class BanditArmyIssueBehavior
{
    internal class VillageBanditArmyRaidIssue : IssueBase
    {
        [SaveableField(1)]
        private readonly Settlement _banditSettlement;

        public VillageBanditArmyRaidIssue(Hero issueOwner, Settlement banditSettlement) : base(issueOwner, CampaignTime.DaysFromNow(QuestBalance.IssueDurationDays))
        {
            _banditSettlement = banditSettlement;
        }

        public override TextObject Title => Texts.BanditArmyTitle(IssueSettlement.Name);

        public override TextObject Description => Texts.BanditArmyDescription(IssueSettlement.Name);

        public override TextObject IssueBriefByIssueGiver => Texts.BanditArmyIssueBrief;

        public override TextObject IssueAcceptByPlayer => Texts.BanditArmyIssueAccept;

        public override TextObject IssueQuestSolutionExplanationByIssueGiver => Texts.BanditArmyIssueSolution;

        public override TextObject IssueQuestSolutionAcceptByPlayer => Texts.BanditArmyIssueSolutionAccept;

        public override TextObject IssueAsRumorInSettlement => Texts.BanditArmyRumor;

        // Only Governors Handle Issues sees an alternative solution; players never get a "send companion" option.
        public override bool IsThereAlternativeSolution => GhiCompat.IsHandling(this);

        public override bool IsThereLordSolution => false;

        protected override int RewardGold => QuestBalance.BanditArmyRewardGold;

        // Read by Governors Handle Issues: garrison troops, skill check and governor XP.
        public override int AlternativeSolutionBaseNeededMenCount => QuestBalance.BanditArmyGhiNeededMen;

        protected override int CompanionSkillRewardXP => QuestBalance.BanditArmyGhiSkillXp;

        public override AlternativeSolutionScaleFlag AlternativeSolutionScaleFlags => AlternativeSolutionScaleFlag.Casualties | AlternativeSolutionScaleFlag.FailureRisk;

        public override (SkillObject, int) GetAlternativeSolutionSkill(Hero hero) =>
            QuestHelpers.GetBetterSkill(hero, DefaultSkills.Tactics, DefaultSkills.Leadership, QuestBalance.BanditArmyGhiSkill);

        // Governors Handle Issues pays RewardGold and applies RelationshipChangeWithIssueOwner itself.
        protected override void AlternativeSolutionEndWithSuccessConsequence()
        {
            if (IssueOwner == null)
                return;
            ApplySuccessWorldEffects(IssueOwner);
            QuestHelpers.ApplyOtherNotablesRelation(IssueOwner, null, QuestBalance.BanditArmyOtherNotablesRelation);
            RelationshipChangeWithIssueOwner = QuestBalance.BanditArmySuccessRelation;
        }

        protected override void AlternativeSolutionEndWithFailureConsequence()
        {
            if (IssueOwner == null)
                return;
            ApplyFailWorldEffects(IssueOwner);
            RelationshipChangeWithIssueOwner = QuestBalance.BanditArmyFailRelation;
        }

        // Same rewards the player gets from the quest, given to the AI lord who resolved the issue.
        internal void ApplyAiLordRewards(Hero solver)
        {
            if (IssueOwner == null)
                return;
            ApplySuccessWorldEffects(IssueOwner);
            QuestHelpers.ApplyOtherNotablesRelation(IssueOwner, solver, QuestBalance.BanditArmyOtherNotablesRelation);
            ChangeRelationAction.ApplyRelationChangeBetweenHeroes(solver, IssueOwner, QuestBalance.BanditArmySuccessRelation, false);
            GiveGoldAction.ApplyBetweenCharacters(null, solver, RewardGold, true);
            solver.Clan?.AddRenown(QuestBalance.BanditArmySuccessRenown, false);
        }

        public override IssueFrequency GetFrequency() => IssueFrequency.Common;

        public override bool IssueStayAliveConditions() =>
            _banditSettlement.IsActive && _banditSettlement.Hideout.IsInfested && IssueOwner.CurrentSettlement is { IsRaided: false, IsUnderRaid: false };

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
            if (Clan.PlayerClan.Tier < QuestBalance.BanditArmyMinClanTier)
                flag |= PreconditionFlags.ClanTier;
            if (QuestRules.IsBanditArmyMinTroopRequirementEnabled && MobileParty.MainParty.MemberRoster.TotalHealthyCount < QuestBalance.BanditArmyMinTroops)
                flag |= PreconditionFlags.NotEnoughTroops;
            return flag == PreconditionFlags.None;
        }

        protected override QuestBase GenerateIssueQuest(string questId) =>
            new VillageBanditArmyRaidQuest(questId, IssueOwner, CampaignTime.DaysFromNow(QuestBalance.BanditArmyQuestDurationDays), RewardGold, _banditSettlement);

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
