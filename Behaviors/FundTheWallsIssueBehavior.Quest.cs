using System;
using AdditionalQuestsRedux.Core;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace AdditionalQuestsRedux.Behaviors;

public partial class FundTheWallsIssueBehavior
{
    internal class FundTheWallsQuest : QuestBase
    {
        [SaveableField(1)]
        private Settlement _settlement;

        [SaveableField(2)]
        private int _requestedGold;

        public FundTheWallsQuest(string questId, Hero questGiver, CampaignTime duration, int rewardGold, Settlement settlement, int requestedGold)
            : base(questId, questGiver, duration, rewardGold)
        {
            _settlement = settlement;
            _requestedGold = requestedGold;
            SetDialogs();
            AddTrackedObject(_settlement);
            InitializeQuestOnCreation();
        }

        internal Settlement Settlement => _settlement;

        internal int RequestedGold => _requestedGold;

        public override TextObject Title => Texts.FundTheWallsTitle(_settlement.Name);

        public override bool IsRemainingTimeHidden => false;

        private TextObject AcceptedLogText => Texts.FundTheWallsAcceptedLog(QuestGiver.CharacterObject, _settlement.EncyclopediaLinkWithName, _requestedGold);

        private TextObject SuccessLogText => Texts.FundTheWallsSuccessLog(_settlement.EncyclopediaLinkWithName, _requestedGold);

        private TextObject TimeoutLogText => Texts.FundTheWallsTimeoutLog(QuestGiver.CharacterObject, _settlement.EncyclopediaLinkWithName);

        // Vanilla LordNeedsGarrisonTroops texts.
        private TextObject QuestGiverLostSettlementLogText => Texts.QuestGiverLostSettlementLog(QuestGiver.CharacterObject, _settlement.EncyclopediaLinkWithName);

        private TextObject WarDeclaredLogText => Texts.WarDeclaredLog(QuestGiver.CharacterObject);

        private TextObject PlayerDeclaredWarLogText => Texts.PlayerDeclaredWarLog(QuestGiver.CharacterObject);

        protected override void InitializeQuestOnGameLoad() => SetDialogs();

        protected override void RegisterEvents()
        {
            CampaignEvents.OnSettlementOwnerChangedEvent.AddNonSerializedListener(this, OnSettlementOwnerChanged);
            CampaignEvents.WarDeclared.AddNonSerializedListener(this, OnWarDeclared);
            CampaignEvents.OnClanChangedKingdomEvent.AddNonSerializedListener(this, OnClanChangedKingdom);
            CampaignEvents.MapEventStarted.AddNonSerializedListener(this, OnMapEventStarted);
        }

        protected override void SetDialogs()
        {
            OfferDialogFlow = DialogFlow.CreateDialogFlow(DialogTokens.IssueClassicQuestStart)
                .NpcLine(Texts.FundTheWallsOfferThanks(_settlement.Name))
                .Condition(IsTalkingToQuestGiver)
                .Consequence(QuestAcceptedConsequences)
                .CloseDialog();

            DiscussDialogFlow = DialogFlow.CreateDialogFlow(DialogTokens.QuestDiscuss)
                .NpcLine(Texts.FundTheWallsDiscussQuestion(_settlement.Name))
                .Condition(IsTalkingToQuestGiver)
                .PlayerLine(Texts.FundTheWallsDiscussAnswer)
                .CloseDialog();
        }

        private bool IsTalkingToQuestGiver() => Hero.OneToOneConversationHero == QuestGiver;

        private void QuestAcceptedConsequences()
        {
            StartQuest();
            AddLog(AcceptedLogText);
        }

        // Called from the settlement menu option; the gold leaves the game and goes into the vanilla construction reserve.
        internal void HandOverGold()
        {
            if (!IsOngoing || _settlement.Town is not { } town || Hero.MainHero.Gold < _requestedGold)
                return;

            PayIntoReserve(Hero.MainHero, town, _requestedGold);
            AddLog(SuccessLogText);
            Clan.PlayerClan.AddRenown(GetRenownReward(_requestedGold), false);
            ApplyInfluenceReward(Hero.MainHero, _settlement, _requestedGold);
            TraitLevelingHelper.OnIssueSolvedThroughQuest(QuestGiver, [new Tuple<TraitObject, int>(DefaultTraits.Generosity, QuestBalance.FundWallsSuccessGenerosityXp)]);
            RelationshipChangeWithQuestGiver = GetRelationReward(_requestedGold);
            CompleteQuestWithSuccess();
        }

        private void OnSettlementOwnerChanged(Settlement settlement, bool openToClaim, Hero newOwner, Hero oldOwner, Hero capturerHero, ChangeOwnerOfSettlementAction.ChangeOwnerOfSettlementDetail detail)
        {
            if (settlement == _settlement && _settlement.OwnerClan != QuestGiver.Clan)
                CompleteQuestWithCancel(QuestGiverLostSettlementLogText);
        }

        private void OnWarDeclared(IFaction faction1, IFaction faction2, DeclareWarAction.DeclareWarDetail detail) =>
            QuestHelper.CheckWarDeclarationAndFailOrCancelTheQuest(this, faction1, faction2, detail, PlayerDeclaredWarLogText, WarDeclaredLogText);

        private void OnClanChangedKingdom(Clan clan, Kingdom oldKingdom, Kingdom newKingdom, ChangeKingdomAction.ChangeKingdomActionDetail detail, bool showNotification)
        {
            if (QuestGiver.MapFaction.IsAtWarWith(Hero.MainHero.MapFaction))
                CompleteQuestWithCancel(WarDeclaredLogText);
        }

        private void OnMapEventStarted(MapEvent mapEvent, PartyBase attackerParty, PartyBase defenderParty)
        {
            if (QuestHelper.CheckMinorMajorCoercion(this, mapEvent, attackerParty))
                QuestHelper.ApplyGenericMinorMajorCoercionConsequences(this, mapEvent);
        }

        // Called from CompleteQuestWithTimeOut, which finalizes the quest afterwards.
        protected override void OnTimedOut()
        {
            AddLog(TimeoutLogText);
            RelationshipChangeWithQuestGiver = QuestBalance.FundWallsFailRelation;
        }
    }
}
