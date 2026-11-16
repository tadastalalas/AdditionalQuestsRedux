using System;
using System.Collections.Generic;
using AdditionalQuestsRedux.Core;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace AdditionalQuestsRedux.Behaviors;

public partial class StarvingTownIssueBehavior
{
    internal class StarvingTownNeedsFoodQuest : QuestBase
    {
        [SaveableField(1)]
        private readonly int _neededFood;

        [SaveableField(2)]
        private JournalLog? _playerAcceptedQuestLog;

        [SaveableField(3)]
        private JournalLog? _playerHasNeededFoodLog;

        public StarvingTownNeedsFoodQuest(string questId, Hero questGiver, CampaignTime duration, int rewardGold, int neededFood)
            : base(questId, questGiver, duration, rewardGold)
        {
            _neededFood = neededFood;
            SetDialogs();
            InitializeQuestOnCreation();
        }

        public override TextObject Title => Texts.StarvingTownTitle(QuestGiver.CurrentSettlement.Name);

        public override bool IsRemainingTimeHidden => false;

        private TextObject PlayerAcceptedQuestLogText => Texts.StarvingTownQuestAcceptLog(QuestGiver.CharacterObject, QuestGiver.CurrentSettlement.Name, _neededFood);

        private TextObject PlayerHasNeededFoodLogText => Texts.StarvingTownHasFoodLog(QuestGiver.CurrentSettlement.Name);

        private TextObject PlayerHasNeededFoodNotificationText => Texts.StarvingTownHaveFoodNotification(QuestGiver.CurrentSettlement.Name);

        private TextObject TimeoutLogText => Texts.StarvingTownTimeoutLog(QuestGiver.CharacterObject, QuestGiver.CurrentSettlement.Name, _neededFood);

        private TextObject SuccessLogText => Texts.StarvingTownSuccessLog(QuestGiver.CurrentSettlement.Name, _neededFood);

        private TextObject CancelDueToWarLogText => Texts.CancelWarLog(QuestGiver.CharacterObject);

        protected override void InitializeQuestOnGameLoad() => SetDialogs();

        protected override void RegisterEvents()
        {
            CampaignEvents.PlayerInventoryExchangeEvent.AddNonSerializedListener(this, OnPlayerInventoryExchange);
            CampaignEvents.OnPartyConsumedFoodEvent.AddNonSerializedListener(this, OnPartyConsumedFood);
            CampaignEvents.SettlementEntered.AddNonSerializedListener(this, OnSettlementEntered);
            CampaignEvents.WarDeclared.AddNonSerializedListener(this, OnWarDeclared);
            CampaignEvents.OnClanChangedKingdomEvent.AddNonSerializedListener(this, OnClanChangedKingdom);
            CampaignEvents.MapEventStarted.AddNonSerializedListener(this, OnMapEventStarted);
        }

        protected override void SetDialogs()
        {
            TextObject thankYouText = Texts.StarvingTownThankYouDialog(CharacterObject.PlayerCharacter);
            TextObject waitingText = Texts.StarvingTownWaitingDialog(CharacterObject.PlayerCharacter);

            OfferDialogFlow = DialogFlow.CreateDialogFlow(DialogTokens.IssueClassicQuestStart)
                .NpcLine(thankYouText)
                .Condition(IsTalkingToQuestGiver)
                .Consequence(QuestAcceptedConsequences)
                .CloseDialog();

            DiscussDialogFlow = DialogFlow.CreateDialogFlow(DialogTokens.QuestDiscuss)
                .NpcLine(Texts.StarvingTownDiscussDialog(_neededFood))
                .Condition(IsTalkingToQuestGiver)
                .BeginPlayerOptions()
                .PlayerOption(Texts.StarvingTownHandOverDialog)
                .ClickableCondition(HandOverClickableCondition)
                .NpcLine(thankYouText)
                .Consequence(() => Campaign.Current.ConversationManager.ConversationEndOneShot += Success)
                .CloseDialog()
                .PlayerOption(Texts.StarvingTownNotYetDialog)
                .NpcLine(waitingText)
                .CloseDialog()
                .EndPlayerOptions()
                .CloseDialog();
        }

        private bool IsTalkingToQuestGiver() => Hero.OneToOneConversationHero == QuestGiver;

        // Counts live instead of trusting the journal: the party eats food every day.
        private bool HandOverClickableCondition(out TextObject? explanation)
        {
            UpdateProgress();
            if (CountFoodOnPlayer() >= _neededFood)
            {
                explanation = null;
                return true;
            }
            explanation = Texts.StarvingTownNotEnoughFood;
            return false;
        }

        private void QuestAcceptedConsequences()
        {
            StartQuest();
            _playerAcceptedQuestLog = AddDiscreteLog(PlayerAcceptedQuestLogText, Texts.StarvingTownQuestObjective(),
                Math.Min(CountFoodOnPlayer(), _neededFood), _neededFood);
            UpdateProgress();
        }

        private static bool IsQuestFood(ItemObject item) =>
            item.ItemCategory == DefaultItemCategories.Grain || item.ItemCategory == DefaultItemCategories.Fish || item.ItemCategory == DefaultItemCategories.Meat;

        private static int CountFoodOnPlayer() => QuestHelpers.CountPlayerItems(IsQuestFood);

        private void UpdateProgress()
        {
            if (_playerAcceptedQuestLog == null)
                return;

            int food = CountFoodOnPlayer();
            _playerAcceptedQuestLog.UpdateCurrentProgress(Math.Min(food, _neededFood));
            if (_playerHasNeededFoodLog == null && food >= _neededFood)
            {
                _playerHasNeededFoodLog = AddLog(PlayerHasNeededFoodLogText);
                MBInformationManager.AddQuickInformation(PlayerHasNeededFoodNotificationText);
            }
            else if (_playerHasNeededFoodLog != null && food < _neededFood)
            {
                RemoveLog(_playerHasNeededFoodLog);
                _playerHasNeededFoodLog = null;
            }
        }

        private void OnPlayerInventoryExchange(List<(ItemRosterElement, int)> purchasedItems, List<(ItemRosterElement, int)> soldItems, bool isTrading) => UpdateProgress();

        private void OnPartyConsumedFood(MobileParty party)
        {
            if (party == MobileParty.MainParty)
                UpdateProgress();
        }

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

        private void OnMapEventStarted(MapEvent mapEvent, PartyBase attackerParty, PartyBase defenderParty)
        {
            if (QuestHelper.CheckMinorMajorCoercion(this, mapEvent, attackerParty))
                QuestHelper.ApplyGenericMinorMajorCoercionConsequences(this, mapEvent);
        }

        private void Success()
        {
            AddLog(SuccessLogText);

            // Meat first, then fish, then grain; every food item of those categories counts.
            int remaining = _neededFood;
            int totalGold = 0;
            remaining -= SellFood(DefaultItemCategories.Meat, remaining, Texts.StarvingTownGaveFoodMeatLog, ref totalGold);
            remaining -= SellFood(DefaultItemCategories.Fish, remaining, Texts.StarvingTownGaveFoodFishLog, ref totalGold);
            SellFood(DefaultItemCategories.Grain, remaining, Texts.StarvingTownGaveFoodGrainLog, ref totalGold);
            GiveGoldAction.ApplyBetweenCharacters(null, Hero.MainHero, totalGold);

            ApplySuccessWorldEffects(QuestGiver);
            Clan.PlayerClan.AddRenown(QuestBalance.StarvingTownSuccessRenown, false);
            RelationshipChangeWithQuestGiver = QuestBalance.StarvingTownSuccessRelation;
            QuestHelpers.ApplyOtherNotablesRelation(QuestGiver, null, QuestBalance.StarvingTownOtherNotablesRelation);
            CompleteQuestWithSuccess();
        }

        // saleLog gets (units sold, gold received).
        private int SellFood(ItemCategory category, int maxAmount, Func<int, int, TextObject> saleLog, ref int totalGold)
        {
            int sold = QuestHelpers.SellPlayerItemsOfCategory(category, maxAmount, QuestBalance.StarvingTownPriceMultiplier, out int gold);
            if (sold > 0)
            {
                AddLog(saleLog(sold, gold));
                totalGold += gold;
            }
            return sold;
        }

        // Called from CompleteQuestWithTimeOut, which finalizes the quest afterwards.
        protected override void OnTimedOut()
        {
            AddLog(TimeoutLogText);
            ApplyFailWorldEffects(QuestGiver);
            RelationshipChangeWithQuestGiver = QuestBalance.StarvingTownFailRelation;
        }
    }
}
