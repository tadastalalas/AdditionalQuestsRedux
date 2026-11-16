using AdditionalQuestsRedux.Core;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace AdditionalQuestsRedux.Behaviors;

public partial class DonateEliteRangersIssueBehavior
{
    internal class DonateEliteRangersQuest : QuestBase
    {
        [SaveableField(1)]
        private Settlement _settlement;

        [SaveableField(2)]
        private CharacterObject _troop;

        [SaveableField(3)]
        private int _troopCount;

        public DonateEliteRangersQuest(string questId, Hero questGiver, CampaignTime duration, int rewardGold, Settlement settlement, CharacterObject troop, int troopCount)
            : base(questId, questGiver, duration, rewardGold)
        {
            _settlement = settlement;
            _troop = troop;
            _troopCount = troopCount;
            SetDialogs();
            AddTrackedObject(_settlement);
            InitializeQuestOnCreation();
        }

        internal Settlement Settlement => _settlement;

        internal CharacterObject Troop => _troop;

        internal int TroopCount => _troopCount;

        public override TextObject Title => Texts.DonateEliteRangersTitle(_settlement.Name);

        public override bool IsRemainingTimeHidden => false;

        private TextObject AcceptedLogText =>
            Texts.DonateEliteRangersAcceptedLog(QuestGiver.CharacterObject, _settlement.EncyclopediaLinkWithName, _troopCount, _troop.EncyclopediaLinkWithName, RewardGold);

        private TextObject SuccessLogText =>
            Texts.DonateEliteRangersSuccessLog(_troopCount, _troop.EncyclopediaLinkWithName, _settlement.EncyclopediaLinkWithName);

        private TextObject TimeoutLogText => Texts.DonateEliteRangersTimeoutLog(QuestGiver.CharacterObject, _settlement.EncyclopediaLinkWithName);

        // Vanilla LordNeedsGarrisonTroops texts.
        private TextObject QuestGiverLostSettlementLogText =>
            Texts.QuestGiverLostSettlementLog(QuestGiver.CharacterObject, _settlement.EncyclopediaLinkWithName);

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
                .NpcLine(Texts.DonateEliteRangersOfferAccepted(_settlement.Name))
                .Condition(IsTalkingToQuestGiver)
                .Consequence(QuestAcceptedConsequences)
                .CloseDialog();

            DiscussDialogFlow = DialogFlow.CreateDialogFlow(DialogTokens.QuestDiscuss)
                .NpcLine(Texts.DonateEliteRangersDiscussAsk(_troop.Name, _settlement.Name))
                .Condition(IsTalkingToQuestGiver)
                .PlayerLine(Texts.DonateEliteRangersDiscussNotYet)
                .CloseDialog();
        }

        private bool IsTalkingToQuestGiver() => Hero.OneToOneConversationHero == QuestGiver;

        private void QuestAcceptedConsequences()
        {
            StartQuest();
            AddLog(AcceptedLogText);
        }

        // Called from the settlement menu option; the troops join the garrison and the quest giver pays RewardGold.
        internal void HandOverTroops()
        {
            if (!IsOngoing || !TransferToGarrison(Hero.MainHero, MobileParty.MainParty, _settlement, _troop, _troopCount))
                return;

            AddLog(SuccessLogText);
            ApplyRewards(Hero.MainHero, QuestGiver, _settlement, _troop, _troopCount, RewardGold, false);
            RelationshipChangeWithQuestGiver = QuestBalance.RangersRelation;
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
            RelationshipChangeWithQuestGiver = QuestBalance.RangersFailRelation;
        }
    }
}
