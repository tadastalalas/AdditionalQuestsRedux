using AdditionalQuestsRedux.Core;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;
using TaleWorlds.SaveSystem;

namespace AdditionalQuestsRedux.Behaviors;

public partial class BanditArmyIssueBehavior
{
    internal class VillageBanditArmyRaidQuest : QuestBase
    {
        private const string LooterTroopId = "looter";
        private const string BanditArmyPartyIdPrefix = "aq_bandit_army_";

        [SaveableField(1)]
        private MobileParty? _banditArmyParty;

        [SaveableField(2)]
        private readonly Settlement _banditSettlement;

        // Save ids 3 and 4 belonged to removed fields (state enum, unused journal log). Never reuse them.

        public VillageBanditArmyRaidQuest(string questId, Hero questGiver, CampaignTime duration, int rewardGold, Settlement banditSettlement)
            : base(questId, questGiver, duration, rewardGold)
        {
            _banditSettlement = banditSettlement;
            SetDialogs();
            InitializeQuestOnCreation();
        }

        public override TextObject Title => Texts.BanditArmyTitle(QuestGiver.CurrentSettlement.Name);

        public override bool IsRemainingTimeHidden => false;

        private TextObject PlayerAcceptedQuestLogText => Texts.BanditArmyQuestAcceptLog(QuestGiver.CurrentSettlement.Name, QuestGiver.CharacterObject);

        private TextObject BanditArmyRaidingLogText => Texts.BanditArmyRaidingLog(QuestGiver.CurrentSettlement.Name);

        private TextObject SuccessLogText => Texts.BanditArmySuccessLog(QuestGiver.CurrentSettlement.Name);

        private TextObject FailureRaidLogText => Texts.BanditArmyFailureLog(QuestGiver.CurrentSettlement.Name);

        private TextObject TimeoutLogText => Texts.BanditArmyTimeoutLog(QuestGiver.CurrentSettlement.Name);

        private TextObject CancelDueToWarLogText => Texts.CancelWarLog(QuestGiver.CharacterObject);

        private TextObject CancelDueToRaidLogText => Texts.CancelRaidLog(QuestGiver.CurrentSettlement.Name, QuestGiver.CharacterObject);

        private TextObject CancelDueToBanditArmyGoneLogText => Texts.BanditArmyGoneLog(QuestGiver.CurrentSettlement.Name, QuestGiver.CharacterObject);

        protected override void InitializeQuestOnGameLoad()
        {
            if (_banditArmyParty is { IsActive: true })
                _banditArmyParty.Ai.SetDoNotMakeNewDecisions(true);
            SetDialogs();
        }

        protected override void RegisterEvents()
        {
            CampaignEvents.MapEventEnded.AddNonSerializedListener(this, OnMapEventEnded);
            CampaignEvents.MapEventStarted.AddNonSerializedListener(this, OnMapEventStarted);
            CampaignEvents.WarDeclared.AddNonSerializedListener(this, OnWarDeclared);
            CampaignEvents.OnClanChangedKingdomEvent.AddNonSerializedListener(this, OnClanChangedKingdom);
            CampaignEvents.SettlementEntered.AddNonSerializedListener(this, OnSettlementEntered);
            CampaignEvents.VillageBeingRaided.AddNonSerializedListener(this, OnVillageBeingRaided);
            CampaignEvents.RaidCompletedEvent.AddNonSerializedListener(this, OnRaidCompleted);
        }

        protected override void SetDialogs()
        {
            OfferDialogFlow = DialogFlow.CreateDialogFlow(DialogTokens.IssueClassicQuestStart)
                .NpcLine(Texts.BanditArmyOfferDialog)
                .Condition(IsTalkingToQuestGiver)
                .Consequence(QuestAcceptedConsequences)
                .CloseDialog();

            DiscussDialogFlow = DialogFlow.CreateDialogFlow(DialogTokens.QuestDiscuss)
                .NpcLine(Texts.BanditArmyDiscussDialogStart)
                .Condition(IsTalkingToQuestGiver)
                .BeginPlayerOptions()
                .PlayerOption(Texts.BanditArmyDiscussDialogTalk)
                .NpcLine(Texts.BanditArmyDiscussDialogFinish)
                .CloseDialog()
                .EndPlayerOptions()
                .CloseDialog();

            QuestCharacterDialogFlow = DialogFlow.CreateDialogFlow(DialogTokens.Start, DialogTokens.QuestCharacterPriority)
                .NpcLine(Texts.BanditArmyBanditConfront)
                .Condition(IsTalkingToBanditArmyLeader)
                .BeginPlayerOptions()
                .PlayerOption(Texts.BanditArmyBanditConfrontResponse)
                .CloseDialog()
                .EndPlayerOptions()
                .CloseDialog();
        }

        private bool IsTalkingToQuestGiver() => Hero.OneToOneConversationHero == QuestGiver;

        private bool IsTalkingToBanditArmyLeader() =>
            _banditArmyParty is { IsActive: true } && CharacterObject.OneToOneConversationCharacter == ConversationHelper.GetConversationCharacterPartyLeader(_banditArmyParty.Party);

        private void QuestAcceptedConsequences()
        {
            StartQuest();
            AddLog(PlayerAcceptedQuestLogText);
            // 1.4.7, 1.4.8 only. 1.5.7 removed Hideout.IsSpotted: Settlement.IsVisible below is the spotted flag (vanilla NearbyBanditBaseIssue sets only that).
#if LOWER_THAN_1_5
            // 1.4.7, 1.4.8 only: 1.5.7 has no Hideout.IsSpotted; IsVisible below is the spotted flag there.
            _banditSettlement.Hideout.IsSpotted = true;
#endif
            _banditSettlement.IsVisible = true;
            CreateBanditArmyParty();
            QuestHelper.AddMapArrowFromPointToTarget(Texts.BanditArmyDirection, QuestGiver.CurrentSettlement.Position, _banditSettlement.Position,
                QuestBalance.BanditArmyMapArrowLife, QuestBalance.BanditArmyMapArrowError);
            MBInformationManager.AddQuickInformation(Texts.BanditArmyHideoutMarked(QuestGiver.CharacterObject));
        }

        protected override void HourlyTick()
        {
            // Not spawned yet: the quest is created before the offer dialog accepts it.
            if (_banditArmyParty == null)
                return;

            if (!_banditArmyParty.IsActive)
            {
                CompleteQuestWithCancel(CancelDueToBanditArmyGoneLogText);
                return;
            }

            if (_banditArmyParty.MapEvent == null)
                EnsureMarchingOnVillage();
        }

        private void OnMapEventEnded(MapEvent mapEvent)
        {
            if (!mapEvent.IsPlayerMapEvent || _banditArmyParty == null || !IsPartyInvolved(mapEvent, _banditArmyParty.Party))
                return;

            if (mapEvent.WinningSide == mapEvent.PlayerSide)
            {
                OnBanditArmyDefeated();
                return;
            }

            // Player lost or retreated: the bandits resume their march.
            if (_banditArmyParty.IsActive)
                EnsureMarchingOnVillage();
        }

        private static bool IsPartyInvolved(MapEvent mapEvent, PartyBase party) => IsOnSide(mapEvent.DefenderSide, party) || IsOnSide(mapEvent.AttackerSide, party);

        private static bool IsOnSide(MapEventSide side, PartyBase party)
        {
            MBReadOnlyList<MapEventParty> parties = side.Parties;
            for (int i = 0; i < parties.Count; i++)
            {
                if (parties[i].Party == party)
                    return true;
            }
            return false;
        }

        private void OnMapEventStarted(MapEvent mapEvent, PartyBase attackerParty, PartyBase defenderParty)
        {
            if (QuestHelper.CheckMinorMajorCoercion(this, mapEvent, attackerParty))
                QuestHelper.ApplyGenericMinorMajorCoercionConsequences(this, mapEvent);
        }

        private void OnWarDeclared(IFaction faction1, IFaction faction2, DeclareWarAction.DeclareWarDetail detail) =>
            QuestHelper.CheckWarDeclarationAndFailOrCancelTheQuest(this, faction1, faction2, detail, CancelDueToWarLogText, CancelDueToWarLogText, forceCancel: true);

        private void OnClanChangedKingdom(Clan clan, Kingdom oldKingdom, Kingdom newKingdom, ChangeKingdomAction.ChangeKingdomActionDetail detail, bool showNotification)
        {
            if (QuestGiver.CurrentSettlement.MapFaction.IsAtWarWith(Hero.MainHero.MapFaction))
                CompleteQuestWithCancel(CancelDueToWarLogText);
        }

        private void OnSettlementEntered(MobileParty party, Settlement settlement, Hero hero)
        {
            if (_banditArmyParty != null && party == _banditArmyParty && settlement == QuestGiver.CurrentSettlement
                && PlayerCaptivity.IsCaptive && PlayerCaptivity.CaptorParty == _banditArmyParty.Party)
                EndCaptivityAction.ApplyByEscape(Hero.MainHero);
        }

        // Fires when a raid starts. The raid map event already exists, so its attacker identifies the raider.
        private void OnVillageBeingRaided(Village village)
        {
            if (village.Settlement != QuestGiver.CurrentSettlement)
                return;

            PartyBase? raider = village.Settlement.Party.MapEvent?.AttackerSide.LeaderParty;
            if (_banditArmyParty != null && raider == _banditArmyParty.Party)
            {
                AddLog(BanditArmyRaidingLogText);
                return;
            }

            CompleteQuestWithCancel(CancelDueToRaidLogText);
        }

        // Fires for every finished raid, including raids beaten off by the defenders.
        private void OnRaidCompleted(BattleSideEnum winnerSide, RaidEventComponent raidEvent)
        {
            if (winnerSide != BattleSideEnum.Attacker || _banditArmyParty == null
                || raidEvent.MapEventSettlement != QuestGiver.CurrentSettlement || raidEvent.AttackerSide.LeaderParty != _banditArmyParty.Party)
                return;

            ApplyFailWorldEffects(QuestGiver);
            RelationshipChangeWithQuestGiver = QuestBalance.BanditArmyFailRelation;
            CompleteQuestWithFail(FailureRaidLogText);
        }

        private void OnBanditArmyDefeated()
        {
            AddLog(SuccessLogText);
            Clan.PlayerClan.AddRenown(QuestBalance.BanditArmySuccessRenown, false);
            GiveGoldAction.ApplyBetweenCharacters(null, Hero.MainHero, RewardGold);
            ApplySuccessWorldEffects(QuestGiver);
            RelationshipChangeWithQuestGiver = QuestBalance.BanditArmySuccessRelation;
            QuestHelpers.ApplyOtherNotablesRelation(QuestGiver, null, QuestBalance.BanditArmyOtherNotablesRelation);
            CompleteQuestWithSuccess();
        }

        // Called from CompleteQuestWithTimeOut, which finalizes the quest afterwards.
        protected override void OnTimedOut() => AddLog(TimeoutLogText);

        protected override void OnFinalize() => ReleaseBanditArmyParty();

        private void EnsureMarchingOnVillage()
        {
            if (_banditArmyParty != null && _banditArmyParty.DefaultBehavior != AiBehavior.RaidSettlement)
                SetPartyAiAction.GetActionForRaidingSettlement(_banditArmyParty, QuestGiver.CurrentSettlement, MobileParty.NavigationType.Default, false, false);
        }

        private void CreateBanditArmyParty()
        {
            Clan banditClan = FindBanditClan();
            int partySize = RollPartySize();
            float looterShare = QuestBalance.BanditArmyLooterShare
                + MBRandom.RandomFloatRanged(-QuestBalance.BanditArmyLooterShareVariance, QuestBalance.BanditArmyLooterShareVariance);
            int looterCount = (int)(partySize * looterShare);

            CharacterObject? looter = MBObjectManager.Instance.GetObject<CharacterObject>(LooterTroopId);
            PartyTemplateObject? banditTemplate = banditClan.DefaultPartyTemplate;
            if (looter == null)
                looterCount = 0;
            else if (banditTemplate == null)
                looterCount = partySize;
            int banditCount = partySize - looterCount;

            _banditArmyParty = BanditPartyComponent.CreateBanditParty(BanditArmyPartyIdPrefix + StringId, banditClan, _banditSettlement.Hideout, false, null, _banditSettlement.GatePosition);
            // Vanilla helper sizes the hideout culture's template to the requested count; it clears the roster first.
            if (banditTemplate != null && banditCount > 0)
                MobilePartyHelper.FillPartyManuallyAfterCreation(_banditArmyParty, banditTemplate, banditCount);
            if (looter != null && looterCount > 0)
                _banditArmyParty.MemberRoster.AddToCounts(looter, looterCount);

            _banditArmyParty.Party.SetCustomName(Texts.BanditArmyPartyName(_banditSettlement.Culture.Name));

            // Enough food for the whole quest, computed after all troops are in.
            int foodNeeded = MathF.Ceiling(QuestDueTime.RemainingDaysFromNow * MathF.Abs(_banditArmyParty.FoodChange));
            int grain = foodNeeded / QuestBalance.BanditArmyFoodItemTypes;
            _banditArmyParty.ItemRoster.AddToCounts(DefaultItems.Grain, grain);
            _banditArmyParty.ItemRoster.AddToCounts(DefaultItems.Meat, foodNeeded - grain);

            _banditArmyParty.SetPartyUsedByQuest(true);
            _banditArmyParty.IgnoreByOtherPartiesTill(QuestDueTime);
            _banditArmyParty.Aggressiveness = 0f;
            _banditArmyParty.Ai.SetDoNotMakeNewDecisions(true);
            _banditArmyParty.Party.SetVisualAsDirty();
            AddTrackedObject(_banditArmyParty);
            EnsureMarchingOnVillage();
        }

        // Same lookup vanilla quests use: the bandit faction of the hideout's culture, else any bandit faction.
        private Clan FindBanditClan()
        {
            CultureObject banditCulture = _banditSettlement.Culture;
            foreach (Clan clan in Clan.BanditFactions)
            {
                if (clan.Culture == banditCulture)
                    return clan;
            }
            return Clan.All.GetRandomElementWithPredicate(clan => clan.IsBanditFaction);
        }

        private static int RollPartySize()
        {
            int size = MBRandom.RandomInt(QuestBalance.BanditArmySizeRollMin, QuestBalance.BanditArmySizeRollMax);
            for (int i = 0; i < Clan.PlayerClan.Tier; i++)
                size += MBRandom.RandomInt(QuestBalance.BanditArmySizeRollMin, QuestBalance.BanditArmySizeRollMax);
            return (int)(size * QuestRules.BanditArmySizeMultiplier);
        }

        private void ReleaseBanditArmyParty()
        {
            if (_banditArmyParty is not { IsActive: true })
                return;

            _banditArmyParty.SetPartyUsedByQuest(false);
            _banditArmyParty.IgnoreByOtherPartiesTill(CampaignTime.Now);
            _banditArmyParty.Aggressiveness = 1f;
            if (_banditArmyParty.CurrentSettlement != null)
                LeaveSettlementAction.ApplyForParty(_banditArmyParty);
            _banditArmyParty.Ai.SetDoNotMakeNewDecisions(false);
        }
    }
}
