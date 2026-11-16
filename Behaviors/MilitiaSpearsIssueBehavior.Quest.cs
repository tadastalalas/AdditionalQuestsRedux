using System;
using System.Collections.Generic;
using AdditionalQuestsRedux.Core;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace AdditionalQuestsRedux.Behaviors;

public partial class MilitiaSpearsIssueBehavior
{
    internal class HeadmanNeedsMilitiaWeaponsQuest : QuestBase
    {
        [SaveableField(1)]
        private readonly int _neededSpears;

        [SaveableField(2)]
        private JournalLog? _playerAcceptedQuestLog;

        [SaveableField(3)]
        private JournalLog? _playerHasNeededSpearsLog;

        public HeadmanNeedsMilitiaWeaponsQuest(string questId, Hero questGiver, CampaignTime duration, int rewardGold, int neededSpears)
            : base(questId, questGiver, duration, rewardGold)
        {
            _neededSpears = neededSpears;
            SetDialogs();
            InitializeQuestOnCreation();
        }

        public override TextObject Title => Texts.MilitiaSpearsTitle(QuestGiver.CurrentSettlement.Name);

        public override bool IsRemainingTimeHidden => false;

        private TextObject PlayerAcceptedQuestLogText =>
            Texts.MilitiaSpearsQuestAcceptLog(QuestGiver.CharacterObject, QuestGiver.CurrentSettlement.Name, _neededSpears, RewardGold);

        private TextObject PlayerHasNeededSpearsLogText => Texts.MilitiaSpearsHasSpearsLog(QuestGiver.CurrentSettlement.Name);

        private TextObject PlayerHasNeededSpearsNotificationText => Texts.MilitiaSpearsHaveSpearsLog(QuestGiver.CurrentSettlement.Name);

        private TextObject TimeoutLogText => Texts.MilitiaSpearsTimeoutLog(_neededSpears);

        private TextObject SuccessLogText => Texts.MilitiaSpearsSuccessLog(_neededSpears);

        private TextObject CancelDueToWarLogText => Texts.CancelWarLog(QuestGiver.CharacterObject);

        private TextObject CancelDueToRaidLogText => Texts.CancelRaidLog(QuestGiver.CurrentSettlement.Name, QuestGiver.CharacterObject);

        protected override void InitializeQuestOnGameLoad() => SetDialogs();

        protected override void RegisterEvents()
        {
            CampaignEvents.PlayerInventoryExchangeEvent.AddNonSerializedListener(this, OnPlayerInventoryExchange);
            CampaignEvents.SettlementEntered.AddNonSerializedListener(this, OnSettlementEntered);
            CampaignEvents.WarDeclared.AddNonSerializedListener(this, OnWarDeclared);
            CampaignEvents.OnClanChangedKingdomEvent.AddNonSerializedListener(this, OnClanChangedKingdom);
            CampaignEvents.VillageLooted.AddNonSerializedListener(this, OnVillageLooted);
            CampaignEvents.MapEventStarted.AddNonSerializedListener(this, OnMapEventStarted);
        }

        protected override void SetDialogs()
        {
            TextObject thankYouText = Texts.MilitiaSpearsThankYouDialog(CharacterObject.PlayerCharacter);
            TextObject waitingText = Texts.MilitiaSpearsWaitingDialog(CharacterObject.PlayerCharacter);

            OfferDialogFlow = DialogFlow.CreateDialogFlow(DialogTokens.IssueClassicQuestStart)
                .NpcLine(thankYouText)
                .Condition(IsTalkingToQuestGiver)
                .Consequence(QuestAcceptedConsequences)
                .CloseDialog();

            DiscussDialogFlow = DialogFlow.CreateDialogFlow(DialogTokens.QuestDiscuss)
                .NpcLine(Texts.MilitiaSpearsDiscussDialog(_neededSpears))
                .Condition(IsTalkingToQuestGiver)
                .BeginPlayerOptions()
                .PlayerOption(Texts.MilitiaSpearsHandOverDialog)
                .ClickableCondition(HandOverClickableCondition)
                .NpcLine(thankYouText)
                .Consequence(() => Campaign.Current.ConversationManager.ConversationEndOneShot += Success)
                .CloseDialog()
                .PlayerOption(Texts.MilitiaSpearsNotYetDialog)
                .NpcLine(waitingText)
                .CloseDialog()
                .EndPlayerOptions()
                .CloseDialog();
        }

        private bool IsTalkingToQuestGiver() => Hero.OneToOneConversationHero == QuestGiver;

        // Counts live instead of trusting the journal: items can be lost without an inventory event (capture, loot, crafting).
        private bool HandOverClickableCondition(out TextObject? explanation)
        {
            UpdateProgress();
            if (CountSpearsOnPlayer() >= _neededSpears)
            {
                explanation = null;
                return true;
            }
            explanation = Texts.MilitiaSpearsNotEnoughSpears;
            return false;
        }

        private void QuestAcceptedConsequences()
        {
            StartQuest();
            _playerAcceptedQuestLog = AddDiscreteLog(PlayerAcceptedQuestLogText, Texts.MilitiaSpearsQuestObjective,
                Math.Min(CountSpearsOnPlayer(), _neededSpears), _neededSpears);
            UpdateProgress();
        }

        private static int CountSpearsOnPlayer() => QuestHelpers.CountPlayerItems(QuestHelpers.IsOneHandedPolearm);

        private void UpdateProgress()
        {
            if (_playerAcceptedQuestLog == null)
                return;

            int spears = CountSpearsOnPlayer();
            _playerAcceptedQuestLog.UpdateCurrentProgress(Math.Min(spears, _neededSpears));
            if (_playerHasNeededSpearsLog == null && spears >= _neededSpears)
            {
                _playerHasNeededSpearsLog = AddLog(PlayerHasNeededSpearsLogText);
                MBInformationManager.AddQuickInformation(PlayerHasNeededSpearsNotificationText);
            }
            else if (_playerHasNeededSpearsLog != null && spears < _neededSpears)
            {
                RemoveLog(_playerHasNeededSpearsLog);
                _playerHasNeededSpearsLog = null;
            }
        }

        private void OnPlayerInventoryExchange(List<(ItemRosterElement, int)> purchasedItems, List<(ItemRosterElement, int)> soldItems, bool isTrading) => UpdateProgress();

        private void OnSettlementEntered(MobileParty party, Settlement settlement, Hero hero)
        {
            if (party == MobileParty.MainParty)
                UpdateProgress();
        }

        private void OnWarDeclared(IFaction faction1, IFaction faction2, DeclareWarAction.DeclareWarDetail detail) =>
            QuestHelper.CheckWarDeclarationAndFailOrCancelTheQuest(this, faction1, faction2, detail, CancelDueToWarLogText, CancelDueToWarLogText, forceCancel: true);

        private void OnClanChangedKingdom(Clan clan, Kingdom oldKingdom, Kingdom newKingdom, ChangeKingdomAction.ChangeKingdomActionDetail detail, bool showNotification)
        {
            if (QuestGiver.CurrentSettlement.MapFaction.IsAtWarWith(Hero.MainHero.MapFaction))
                CompleteQuestWithCancel(CancelDueToWarLogText);
        }

        private void OnVillageLooted(Village village)
        {
            if (village.Settlement == QuestGiver.CurrentSettlement)
                CompleteQuestWithCancel(CancelDueToRaidLogText);
        }

        private void OnMapEventStarted(MapEvent mapEvent, PartyBase attackerParty, PartyBase defenderParty)
        {
            if (QuestHelper.CheckMinorMajorCoercion(this, mapEvent, attackerParty))
                QuestHelper.ApplyGenericMinorMajorCoercionConsequences(this, mapEvent);
        }

        private void Success()
        {
            AddLog(SuccessLogText);
            TraitLevelingHelper.OnIssueSolvedThroughQuest(QuestGiver,
            [
                new Tuple<TraitObject, int>(DefaultTraits.Mercy, QuestBalance.MilitiaSpearsSuccessMercyXp),
                new Tuple<TraitObject, int>(DefaultTraits.Generosity, QuestBalance.MilitiaSpearsSuccessGenerosityXp),
            ]);
            QuestHelpers.RemovePlayerItems(QuestHelpers.IsOneHandedPolearm, _neededSpears);
            GiveGoldAction.ApplyBetweenCharacters(null, Hero.MainHero, RewardGold);
            ApplySuccessWorldEffects(QuestGiver);
            RelationshipChangeWithQuestGiver = QuestBalance.MilitiaSpearsSuccessRelation;
            CompleteQuestWithSuccess();
        }

        // Called from CompleteQuestWithTimeOut, which finalizes the quest afterwards.
        protected override void OnTimedOut()
        {
            AddLog(TimeoutLogText);
            ApplyFailWorldEffects(QuestGiver);
            RelationshipChangeWithQuestGiver = QuestBalance.MilitiaSpearsFailRelation;
        }
    }
}
