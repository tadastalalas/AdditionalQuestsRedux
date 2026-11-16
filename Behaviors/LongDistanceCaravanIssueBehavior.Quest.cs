using System;
using System.Collections.Generic;
using AdditionalQuestsRedux.Core;
using AdditionalQuestsRedux.Diagnostics;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;
using TaleWorlds.SaveSystem;
using MathF = TaleWorlds.Library.MathF;

namespace AdditionalQuestsRedux.Behaviors;

public partial class LongDistanceCaravanIssueBehavior
{
    // Escort part mirrors vanilla EscortMerchantCaravanIssueQuest (spawn, speed matching, raiders, lose-sight, hostility).
    // Differences: a fixed far destination with town stops on the way, real cargo bought and sold, and the caravan is
    // kept as a normal caravan only if the merchant owns none; otherwise it is disbanded so caravans never pile up.
    internal class LongDistanceCaravanQuest : QuestBase
    {
        private const string CaravanTalkToken = "aq_long_caravan_talk";
        private const string CaravanPartyNameId = "str_caravan_party_name";
        private const string CaravanOwnerTag = "OWNER";
        private const string RaiderTemplateId = "kingdom_hero_party_caravan_ambushers";
        private const string RaiderPartyIdPrefix = "aq_long_caravan_raiders_";
        private const string SumpterHorseId = "sumpter_horse";
        private const string CamelMountId = "camel";
        private const string MuleMountId = "mule";
        private const int CaravanMasterLevel = 26;
        private static readonly string[] CamelCultureIds = ["aserai", "khuzait"];
        private static readonly string[] CamelHarnessIds = ["camel_saddle_a", "camel_saddle_b"];
        private static readonly string[] MuleHarnessIds = ["mule_load_a", "mule_load_b", "mule_load_c"];

        [SaveableField(1)]
        private MobileParty? _caravan;

        [SaveableField(2)]
        private readonly Settlement _origin;

        [SaveableField(3)]
        private Settlement _destination;

        [SaveableField(4)]
        private MobileParty? _raiderParty;

        [SaveableField(5)]
        private MobileParty? _otherBanditParty;

        [SaveableField(6)]
        private int _fundedGold;

        [SaveableField(7)]
        private bool _isPlayerNotifiedForDanger;

        [SaveableField(8)]
        private int _raiderFollowHours;

        [SaveableField(9)]
        private int _otherBanditFollowHours;

        [SaveableField(10)]
        private bool _ambushRolledThisLeg;

        [SaveableField(11)]
        private bool _arrived;

        [SaveableField(12)]
        private Settlement _lastSettlement;

        [SaveableField(13)]
        private int _waitedHours;

        [SaveableField(14)]
        private Settlement? _waypoint;

        public LongDistanceCaravanQuest(string questId, Hero questGiver, CampaignTime duration, int rewardGold, Settlement origin, Settlement destination)
            : base(questId, questGiver, duration, rewardGold)
        {
            _origin = origin;
            _destination = destination;
            _lastSettlement = origin;
            SetDialogs();
            InitializeQuestOnCreation();
        }

        public override TextObject Title => Texts.LongDistanceCaravanTitle(_destination.Name);

        public override bool IsRemainingTimeHidden => false;

        private static float RaiderSpawnDistance => AverageTownDistance * QuestBalance.LongCaravanRaiderSpawnTowns;

        private static float BanditAttackRadius => Campaign.Current.Models.EncounterModel.GetEncounterJoiningRadius * QuestBalance.LongCaravanBanditAttackRadiusFactor;

        protected override void InitializeQuestOnGameLoad()
        {
            // Vanilla does the same: a caravan that is already a normal caravan cannot be escorted any more.
            if (_caravan is { IsCaravan: true })
            {
                CompleteQuestWithCancel();
                return;
            }
            if (_caravan is { IsActive: true })
                _caravan.Ai.SetDoNotMakeNewDecisions(true);
            if (_raiderParty is { IsActive: true })
                _raiderParty.Ai.SetDoNotMakeNewDecisions(true);
            if (_otherBanditParty is { IsActive: true })
                _otherBanditParty.Ai.SetDoNotMakeNewDecisions(true);
            SetDialogs();
        }

        protected override void RegisterEvents()
        {
            CampaignEvents.SettlementEntered.AddNonSerializedListener(this, OnSettlementEntered);
            CampaignEvents.OnSettlementLeftEvent.AddNonSerializedListener(this, OnSettlementLeft);
            CampaignEvents.MapEventEnded.AddNonSerializedListener(this, OnMapEventEnded);
            CampaignEvents.WarDeclared.AddNonSerializedListener(this, OnWarDeclared);
            CampaignEvents.OnClanChangedKingdomEvent.AddNonSerializedListener(this, OnClanChangedKingdom);
            CampaignEvents.OnSettlementOwnerChangedEvent.AddNonSerializedListener(this, OnSettlementOwnerChanged);
            CampaignEvents.HourlyTickPartyEvent.AddNonSerializedListener(this, OnPartyHourlyTick);
        }

        protected override void SetDialogs()
        {
            OfferDialogFlow = DialogFlow.CreateDialogFlow(DialogTokens.IssueClassicQuestStart)
                .NpcLine(Texts.LongDistanceCaravanOfferDialog())
                .Condition(IsTalkingToQuestGiver)
                .Consequence(QuestAcceptedConsequences)
                .CloseDialog();

            DiscussDialogFlow = DialogFlow.CreateDialogFlow(DialogTokens.QuestDiscuss)
                .NpcLine(Texts.LongDistanceCaravanDiscussDialog())
                .Condition(IsTalkingToQuestGiver)
                .CloseDialog();

            Campaign.Current.ConversationManager.AddDialogFlow(GetCaravanGreetingDialogFlow(), this);
            Campaign.Current.ConversationManager.AddDialogFlow(GetCaravanTaskDialogFlow(), this);
            Campaign.Current.ConversationManager.AddDialogFlow(GetCaravanLootDialogFlow(), this);
            Campaign.Current.ConversationManager.AddDialogFlow(GetCaravanFarewellDialogFlow(), this);
        }

        private DialogFlow GetCaravanGreetingDialogFlow() =>
            DialogFlow.CreateDialogFlow(DialogTokens.Start, DialogTokens.QuestCharacterPriority)
                .NpcLine(Texts.LongDistanceCaravanGreetingDialog())
                .Condition(IsTalkingToCaravan)
                .GotoDialogState(CaravanTalkToken);

        private DialogFlow GetCaravanTaskDialogFlow() =>
            DialogFlow.CreateDialogFlow(CaravanTalkToken, DialogTokens.QuestCharacterPriority)
                .BeginPlayerOptions()
                .PlayerOption(Texts.LongDistanceCaravanAboutTaskOption(QuestGiver.CharacterObject))
                .Condition(IsTalkingToCaravan)
                .NpcLine(Texts.LongDistanceCaravanFollowUsDialog())
                .Consequence(LeaveEncounter)
                .CloseDialog()
                .EndPlayerOptions();

        private DialogFlow GetCaravanFarewellDialogFlow() =>
            DialogFlow.CreateDialogFlow(CaravanTalkToken, DialogTokens.QuestCharacterPriority)
                .BeginPlayerOptions()
                .PlayerOption(Texts.LongDistanceCaravanFarewellOption())
                .Condition(IsTalkingToCaravan)
                .NpcLine(Texts.LongDistanceCaravanFollowUsDialog())
                .Consequence(LeaveEncounter)
                .CloseDialog()
                .EndPlayerOptions();

        private DialogFlow GetCaravanLootDialogFlow() =>
            DialogFlow.CreateDialogFlow(CaravanTalkToken, DialogTokens.QuestCharacterPriority)
                .BeginPlayerOptions()
                .PlayerOption(Texts.LongDistanceCaravanLootOption())
                .Condition(CanLootCaravan)
                .NpcLine(Texts.LongDistanceCaravanLootRefuseDialog())
                .BeginPlayerOptions()
                .PlayerOption(Texts.LongDistanceCaravanAttackOption())
                .Consequence(AttackCaravan)
                .CloseDialog()
                .PlayerOption(Texts.LongDistanceCaravanGoInPeaceOption())
                .Consequence(LeaveEncounter)
                .CloseDialog()
                .EndPlayerOptions()
                .EndPlayerOptions();

        private bool IsTalkingToQuestGiver() => Hero.OneToOneConversationHero == QuestGiver;

        // Same checks as vanilla caravan_talk_on_condition; also sets the greeting variables.
        private bool IsTalkingToCaravan()
        {
            MobileParty? party = MobileParty.ConversationParty;
            CharacterObject? character = CharacterObject.OneToOneConversationCharacter;
            if (_caravan == null || party != _caravan || character == null || character.IsHero || !party.IsCustomParty
                || !party.MemberRoster.Contains(character) || party.Party.Owner == Hero.MainHero)
                return false;
            SetCaravanGreetingVariables(party);
            return true;
        }

        private bool CanLootCaravan()
        {
            MobileParty? party = MobileParty.ConversationParty;
            if (_caravan == null || party != _caravan || party.IsCaravan || party.Party.MapFaction == Hero.MainHero.MapFaction)
                return false;
            SetCaravanGreetingVariables(party);
            return true;
        }

        private static void SetCaravanGreetingVariables(MobileParty party)
        {
            MBTextManager.SetTextVariable(Texts.LongDistanceCaravanHometownVariable, party.HomeSettlement.EncyclopediaLinkWithName);
            StringHelpers.SetCharacterProperties(Texts.LongDistanceCaravanMerchantTag, party.Party.Owner.CharacterObject);
            StringHelpers.SetCharacterProperties(Texts.LongDistanceCaravanProtectorTag, party.HomeSettlement.OwnerClan.Leader.CharacterObject);
        }

        private static void LeaveEncounter()
        {
            if (PlayerEncounter.Current != null)
                PlayerEncounter.LeaveEncounter = true;
        }

        private static void AttackCaravan() => BeHostileAction.ApplyEncounterHostileAction(PartyBase.MainParty, MobileParty.ConversationParty.Party);

        private void QuestAcceptedConsequences()
        {
            StartQuest();
            int cargoValue = SpawnCaravan();
            AddLog(Texts.LongDistanceCaravanAcceptedLog(QuestGiver.CharacterObject, _origin.EncyclopediaLinkWithName, _destination.EncyclopediaLinkWithName, cargoValue));
            SetNextStop(_origin);
        }

        // Returns the price paid for the cargo.
        private int SpawnCaravan()
        {
            Town origin = _origin.Town;
            GetCaravanVisuals(QuestGiver.Culture, out string mountId, out string harnessId);
            TextObject name = GameTexts.FindText(CaravanPartyNameId).WithCharacter(CaravanOwnerTag, QuestGiver.CharacterObject);
            MobileParty caravan = CustomPartyComponent.CreateCustomPartyWithTroopRoster(_origin.GatePosition, 0f, _origin, name, QuestGiver.Clan,
                TroopRoster.CreateDummyTroopRoster(), TroopRoster.CreateDummyTroopRoster(), QuestGiver, mountId, harnessId, QuestBalance.LongCaravanBaseSpeed);
            _caravan = caravan;
            caravan.Aggressiveness = 0f;

            PartyTemplateObject template = CaravanHelper.GetRandomCaravanTemplate(QuestGiver.Culture, false, true);
            MobilePartyHelper.FillPartyManuallyAfterCreation(caravan, template, QuestBalance.LongCaravanGuards);
            CharacterObject? master = FindCaravanMaster(QuestGiver.Culture);
            if (master != null)
                caravan.MemberRoster.AddToCounts(master, 1, true);
            caravan.Party.SetVisualAsDirty();

            ItemObject? packAnimal = FindCheapestPackAnimal();
            if (packAnimal != null)
                caravan.ItemRoster.AddToCounts(packAnimal, (int)(caravan.MemberRoster.TotalManCount * QuestBalance.LongCaravanPackAnimalsPerMan));

            // The merchant funds the trip; every purchase is paid from the caravan's own purse to the town market.
            caravan.InitializePartyTrade(0);
            GiveGoldAction.ApplyForCharacterToParty(QuestGiver, caravan.Party, GetBudget(QuestGiver), true);
            _fundedGold = caravan.PartyTradeGold;

            int foodUnits = MathF.Ceiling(QuestDueTime.RemainingDaysFromNow * MathF.Abs(caravan.FoodChange) * QuestBalance.LongCaravanFoodMargin);
            PayTown(origin, BuyFood(origin, caravan.ItemRoster, foodUnits));

            float freeWeight = caravan.InventoryCapacity - caravan.TotalWeightCarried;
            int cargoValue = BuyCargo(origin, _destination.Town, caravan.PartyTradeGold, freeWeight, caravan.ItemRoster);
            PayTown(origin, cargoValue);

            AddTrackedObject(caravan);
            caravan.SetPartyUsedByQuest(true);
            caravan.Ai.SetDoNotMakeNewDecisions(true);
            caravan.IgnoreByOtherPartiesTill(QuestDueTime);
            return cargoValue;
        }

        private void PayTown(Town town, int amount)
        {
            if (_caravan == null || amount <= 0)
                return;
            GiveGoldAction.ApplyForPartyToSettlement(_caravan.Party, town.Settlement, amount, true);
            ApplyTownSaleTax(town, amount);
        }

        // Same leader vanilla escort caravans get: the culture's level 26 infantry caravan guard.
        private static CharacterObject? FindCaravanMaster(CultureObject culture)
        {
            MBReadOnlyList<CharacterObject> all = CharacterObject.All;
            for (int i = 0; i < all.Count; i++)
            {
                CharacterObject character = all[i];
                if (character.Occupation == Occupation.CaravanGuard && character.IsInfantry && character.Level == CaravanMasterLevel && character.Culture == culture)
                    return character;
            }
            return culture.CaravanMaster;
        }

        private static ItemObject? FindCheapestPackAnimal()
        {
            ItemObject? cheapest = null;
            MBReadOnlyList<ItemObject> items = Items.All;
            for (int i = 0; i < items.Count; i++)
            {
                ItemObject item = items[i];
                if (item.ItemCategory == DefaultItemCategories.PackAnimal && !item.NotMerchandise && (cheapest == null || item.Value < cheapest.Value))
                    cheapest = item;
            }
            return cheapest;
        }

        private static void GetCaravanVisuals(CultureObject culture, out string mountId, out string harnessId)
        {
            if (Array.IndexOf(CamelCultureIds, culture.StringId) >= 0)
            {
                mountId = CamelMountId;
                harnessId = CamelHarnessIds[MBRandom.RandomInt(CamelHarnessIds.Length)];
                return;
            }
            mountId = MuleMountId;
            harnessId = MuleHarnessIds[MBRandom.RandomInt(MuleHarnessIds.Length)];
        }

        private void SetNextStop(Settlement from)
        {
            if (_caravan is not { IsActive: true })
                return;
            Settlement next = FindNextStop(from, _destination, _caravan.MapFaction);
            if (_waypoint != null && _waypoint != next && IsTracked(_waypoint))
                RemoveTrackedObject(_waypoint);
            _waypoint = next;
            SetPartyAiAction.GetActionForVisitingSettlement(_caravan, next, MobileParty.NavigationType.Default, false, false);
            _caravan.Ai.SetDoNotMakeNewDecisions(true);
            MBInformationManager.AddQuickInformation(Texts.LongDistanceCaravanTravelingNotice(next.Name),
                QuestBalance.LongCaravanStopNoticeExtraMs, PartyBaseHelper.GetVisualPartyLeader(_caravan.Party));
            AddLog(Texts.LongDistanceCaravanMovingLog(next.EncyclopediaLinkWithName), true);
            if (!IsTracked(next))
                AddTrackedObject(next);
        }

        protected override void HourlyTick()
        {
            // Not spawned yet: the quest is created before the offer dialog accepts it.
            if (!IsOngoing || _caravan == null)
                return;

            if (!_caravan.IsActive)
            {
                AddLog(Texts.LongDistanceCaravanDestroyedLog());
                Fail();
                return;
            }

            if (_arrived && _caravan.CurrentSettlement == _destination)
            {
                Arrive();
                return;
            }
            _arrived = false;
            CheckDestination();
            if (!IsOngoing)
                return;

            if (_caravan.CurrentSettlement != null)
                WaitInSettlement();
            else if (_caravan.MapEvent == null)
            {
                if (_caravan.TargetSettlement == null)
                    SetNextStop(_lastSettlement);
                AdjustCaravanSpeed();
            }

            CheckLoseSight();
            if (!IsOngoing)
                return;

            ThinkAboutSpawningRaiders();
            CheckCaravanMapEvent();
            UpdateBanditFollowing();
        }

        private void WaitInSettlement()
        {
            if (_caravan == null)
                return;
            _waitedHours++;
            if (_waitedHours < QuestBalance.LongCaravanSettlementWaitHours)
                return;
            _waitedHours = 0;
            LeaveSettlementAction.ApplyForParty(_caravan);
        }

        // Vanilla AdjustCaravansSpeed: keep the caravan just slower than the player, with a step cap so it can never loop forever.
        private void AdjustCaravanSpeed()
        {
            if (_caravan == null || !MobileParty.MainParty.IsActive || _caravan.PartyComponent is not CustomPartyComponent component)
                return;
            for (int step = 0; step < QuestBalance.LongCaravanSpeedMaxSteps; step++)
            {
                float playerSpeed = MobileParty.MainParty.Speed;
                float caravanSpeed = _caravan.Speed;
                if (caravanSpeed >= playerSpeed && component.BaseSpeed > QuestBalance.LongCaravanSpeedStep)
                    component.SetBaseSpeed(component.BaseSpeed - QuestBalance.LongCaravanSpeedStep);
                else if (playerSpeed - caravanSpeed > QuestBalance.LongCaravanSpeedTolerance)
                    component.SetBaseSpeed(component.BaseSpeed + QuestBalance.LongCaravanSpeedStep);
                else
                    return;
            }
        }

        private void CheckLoseSight()
        {
            if (_caravan is not { IsActive: true } || _caravan.IsVisible)
                return;
            float distance = _caravan.Position.Distance(MobileParty.MainParty.Position);
            float seeingRange = MobileParty.MainParty.SeeingRange;
            if (!_isPlayerNotifiedForDanger && distance >= seeingRange + QuestBalance.LongCaravanLoseSightWarnDistance)
            {
                MBInformationManager.AddQuickInformation(Texts.LongDistanceCaravanLoseSightNotice());
                _isPlayerNotifiedForDanger = true;
            }
            else if (distance >= seeingRange + QuestBalance.LongCaravanLoseSightFailDistance)
            {
                AddLog(Texts.LongDistanceCaravanLostTrackLog(QuestGiver.CharacterObject));
                Fail();
            }
        }

        // Vanilla ThinkAboutSpawningBanditParty, once per leg with a chance instead of once per quest.
        private void ThinkAboutSpawningRaiders()
        {
            if (_caravan == null || _ambushRolledThisLeg || _raiderParty != null)
                return;
            Settlement? target = _caravan.TargetSettlement;
            if (target == null)
                return;
            float distance = _caravan.CurrentSettlement != null
                ? GetTownDistance(_caravan.CurrentSettlement, target)
                : Campaign.Current.Models.MapDistanceModel.GetDistance(_caravan, target, false, MobileParty.NavigationType.Default, out float _);
            float spawnDistance = RaiderSpawnDistance;
            if (distance <= QuestBalance.LongCaravanRaiderMinTargetDistance || distance >= spawnDistance)
                return;

            _ambushRolledThisLeg = true;
            if (MBRandom.RandomFloat >= QuestBalance.LongCaravanAmbushChance)
            {
                if (AqLog.IsDebugEnabled)
                return;
            }
            ActivateRaiders(target);
            if (_raiderParty == null)
            {
                if (AqLog.IsDebugEnabled)
                return;
            }
            if (AqLog.IsDebugEnabled)
            _raiderFollowHours = (int)(spawnDistance / (_raiderParty.Speed + _caravan.Speed)) + QuestBalance.LongCaravanFollowExtraHours;
        }

        // Vanilla ActivateBanditParty: caravan ambushers spawned at the next stop's gate, sized to the escort.
        private void ActivateRaiders(Settlement target)
        {
            if (_caravan == null)
                return;
            Hideout? hideout = SettlementHelper.FindNearestHideoutToMobileParty(_caravan, _caravan.NavigationCapability, settlement => settlement.IsActive);
            if (hideout == null)
                return;
            Clan? clan = FindBanditClan(hideout.Settlement.Culture);
            PartyTemplateObject? template = MBObjectManager.Instance.GetObject<PartyTemplateObject>(RaiderTemplateId) ?? clan?.DefaultPartyTemplate;
            if (clan == null || template == null || template.Stacks.Count == 0)
                return;

            MobileParty raiders = BanditPartyComponent.CreateBanditParty(RaiderPartyIdPrefix + StringId, clan, hideout, false, template, target.GatePosition);
            raiders.Party.SetCustomName(Texts.LongDistanceCaravanRaidersName());
            MobileParty.UpdateLocator(raiders);
            raiders.ActualClan = clan;
            raiders.MemberRoster.Clear();

            List<(PartyTemplateStack, float)> weights = [];
            for (int i = 0; i < template.Stacks.Count; i++)
            {
                PartyTemplateStack stack = template.Stacks[i];
                weights.Add((stack, QuestBalance.LongCaravanRaiderLevelWeightBase - stack.Character.Level));
            }
            int count = MathF.Min(QuestBalance.LongCaravanMaxRaiders,
                (int)((MobileParty.MainParty.MemberRoster.TotalHealthyCount + _caravan.MemberRoster.TotalHealthyCount) * QuestBalance.LongCaravanRaiderShare));
            for (int i = 0; i < count; i++)
                raiders.MemberRoster.AddToCounts(MBRandom.ChooseWeighted(weights).Character, 1);

            raiders.ItemRoster.AddToCounts(DefaultItems.Grain, count);
            ItemObject? sumpterHorse = MBObjectManager.Instance.GetObject<ItemObject>(SumpterHorseId);
            if (sumpterHorse != null)
                raiders.ItemRoster.AddToCounts(sumpterHorse, count);
            raiders.IgnoreByOtherPartiesTill(QuestDueTime);
            SetPartyAiAction.GetActionForEngagingParty(raiders, _caravan, MobileParty.NavigationType.Default, false);
            raiders.Ai.SetDoNotMakeNewDecisions(true);
            AddTrackedObject(raiders);
            _raiderParty = raiders;
        }

        private static Clan? FindBanditClan(CultureObject culture)
        {
            Clan? fallback = null;
            foreach (Clan clan in Clan.BanditFactions)
            {
                if (clan.Culture == culture)
                    return clan;
                fallback ??= clan;
            }
            return fallback;
        }

        // A started ambush waits for the player for a few hours, then the battle may resolve without them.
        private void CheckCaravanMapEvent()
        {
            MapEvent? mapEvent = _caravan?.MapEvent;
            if (mapEvent is { IsInvulnerable: true } && mapEvent.BattleStartTime.ElapsedHoursUntilNow > QuestBalance.LongCaravanInvulnerableHours)
                mapEvent.IsInvulnerable = false;
        }

        private void UpdateBanditFollowing()
        {
            _raiderFollowHours--;
            _otherBanditFollowHours--;
            if (_raiderParty != null && (!_raiderParty.IsActive || (_raiderFollowHours < 0 && _raiderParty.MapEvent == null && !_raiderParty.IsVisible)))
                ReleaseRaiders();
            if (_otherBanditParty != null && (!_otherBanditParty.IsActive || _otherBanditFollowHours < 0))
                ReleaseOtherBandits();
        }

        private void OnPartyHourlyTick(MobileParty mobileParty)
        {
            if (!IsOngoing || _caravan is not { IsActive: true })
                return;
            CheckPartyAndMakeItAttackTheCaravan(mobileParty);
            CheckEncounterForBanditParty(_raiderParty);
            CheckEncounterForBanditParty(_otherBanditParty);
        }

        // Vanilla CheckPartyAndMakeItAttackTheCaravan: a stronger or faster bandit party nearby may go for the caravan between towns.
        private void CheckPartyAndMakeItAttackTheCaravan(MobileParty mobileParty)
        {
            if (_caravan == null || _otherBanditParty != null || mobileParty == _raiderParty || !mobileParty.IsBandit || mobileParty.IsCurrentlyUsedByAQuest
                || mobileParty.MapEvent != null || mobileParty.NavigationCapability != MobileParty.NavigationType.Default
                || mobileParty.Party.NumberOfHealthyMembers <= _caravan.Party.NumberOfHealthyMembers)
                return;
            float distanceSquared = mobileParty.Position.DistanceSquared(_caravan.Position);
            if (mobileParty.Speed <= _caravan.Speed && distanceSquared >= QuestBalance.LongCaravanBanditCloseDistanceSquared)
                return;
            float radius = BanditAttackRadius;
            if (distanceSquared >= radius * radius)
                return;

            Settlement? target = _caravan.TargetSettlement;
            if (target == null)
                return;
            float toTarget;
            float toLast;
            if (_caravan.CurrentSettlement != null)
            {
                toTarget = GetTownDistance(_caravan.CurrentSettlement, target);
                toLast = GetTownDistance(_caravan.CurrentSettlement, _lastSettlement);
            }
            else
            {
                toTarget = Campaign.Current.Models.MapDistanceModel.GetDistance(_caravan, target, false, MobileParty.NavigationType.Default, out float _);
                toLast = Campaign.Current.Models.MapDistanceModel.GetDistance(_caravan, _lastSettlement, false, MobileParty.NavigationType.Default, out float _);
            }
            if (toTarget <= QuestBalance.LongCaravanBanditMinTownDistance || toLast <= QuestBalance.LongCaravanBanditMinTownDistance)
                return;

            SetPartyAiAction.GetActionForEngagingParty(mobileParty, _caravan, MobileParty.NavigationType.Default, false);
            mobileParty.Ai.SetDoNotMakeNewDecisions(true);
            if (!IsTracked(mobileParty))
                AddTrackedObject(mobileParty);
            _otherBanditFollowHours = (int)(distanceSquared / (mobileParty.Speed + _caravan.Speed)) + QuestBalance.LongCaravanFollowExtraHours;
            _otherBanditParty = mobileParty;
        }

        private void CheckEncounterForBanditParty(MobileParty? bandits)
        {
            if (_caravan == null || bandits is not { IsActive: true } || bandits.MapEvent != null || _caravan.MapEvent != null || _caravan.CurrentSettlement != null
                || bandits.Position.DistanceSquared(_caravan.Position) > QuestBalance.LongCaravanEncounterDistanceSquared)
                return;
            EncounterManager.StartPartyEncounter(bandits.Party, _caravan.Party);
            MBInformationManager.AddQuickInformation(Texts.LongDistanceCaravanAmbushedNotice());
            if (_caravan.MapEvent != null)
                _caravan.MapEvent.IsInvulnerable = true;
        }

        private void OnSettlementEntered(MobileParty party, Settlement settlement, Hero hero)
        {
            if (!IsOngoing || _caravan == null || party != _caravan)
                return;

            _lastSettlement = settlement;
            _waitedHours = 0;
            ReleaseRaiders();
            ReleaseOtherBandits();

            if (settlement == _destination)
            {
                // Selling and disbanding run on the next hourly tick: removing the party inside this event would break later listeners.
                _arrived = true;
                return;
            }

            AddLog(Texts.LongDistanceCaravanEnteredLog(settlement.EncyclopediaLinkWithName), true);
            // Vanilla escort caravans hire a few local guards in every town they visit.
            int missing = QuestBalance.LongCaravanGuards + 1 - _caravan.MemberRoster.TotalManCount;
            if (missing > 0)
                _caravan.AddElementToMemberRoster(settlement.Culture.CaravanGuard, MBRandom.RandomInt(MathF.Min(QuestBalance.LongCaravanMaxReinforcements, missing)));
        }

        private void OnSettlementLeft(MobileParty party, Settlement settlement)
        {
            if (!IsOngoing || _caravan == null || party != _caravan)
                return;
            AdjustCaravanSpeed();
            _waitedHours = 0;
            _ambushRolledThisLeg = false;
            if (IsTracked(settlement))
                RemoveTrackedObject(settlement);
            if (party.TargetSettlement == null || party.TargetSettlement == settlement)
                SetNextStop(settlement);
        }

        private void OnMapEventEnded(MapEvent mapEvent)
        {
            if (!IsOngoing || _caravan == null || !IsInvolved(mapEvent, _caravan.Party))
                return;

            if (!mapEvent.HasWinner)
            {
                if (_caravan.MemberRoster.TotalManCount <= 0)
                {
                    AddLog(Texts.LongDistanceCaravanDestroyedLog());
                    Fail();
                }
                return;
            }

            if (mapEvent.Winner != _caravan.MapEventSide)
            {
                if (!IsInvolved(mapEvent, PartyBase.MainParty))
                    AddLog(Texts.LongDistanceCaravanDestroyedByBanditsLog(QuestGiver.CharacterObject));
                else if (_caravan.MapEventSide == MobileParty.MainParty.MapEventSide)
                    AddLog(Texts.LongDistanceCaravanDestroyedLog());
                else
                {
                    FailByPlayerHostility();
                    return;
                }
                Fail();
                return;
            }

            if (_raiderParty is { IsActive: true } && IsInvolved(mapEvent, _raiderParty.Party))
                DestroyPartyAction.Apply(MobileParty.MainParty.Party, _raiderParty);
            if (_otherBanditParty is { IsActive: true } && IsInvolved(mapEvent, _otherBanditParty.Party))
                DestroyPartyAction.Apply(MobileParty.MainParty.Party, _otherBanditParty);

            if (_caravan.MemberRoster.TotalManCount <= 0)
            {
                AddLog(Texts.LongDistanceCaravanDestroyedLog());
                Fail();
                return;
            }

            // Vanilla: a caravan that lost its pack animals in battle gets a few horses so it can keep up.
            if (_caravan.Speed < QuestBalance.LongCaravanSlowSpeed)
            {
                ItemObject? sumpterHorse = MBObjectManager.Instance.GetObject<ItemObject>(SumpterHorseId);
                if (sumpterHorse != null)
                    _caravan.ItemRoster.AddToCounts(sumpterHorse, QuestBalance.LongCaravanSpareHorses);
            }
        }

        // MapEventEnded fires for every battle: plain loops instead of InvolvedParties (LINQ over an iterator).
        private static bool IsInvolved(MapEvent mapEvent, PartyBase party) => IsOnSide(mapEvent.DefenderSide, party) || IsOnSide(mapEvent.AttackerSide, party);

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

        private void OnWarDeclared(IFaction faction1, IFaction faction2, DeclareWarAction.DeclareWarDetail detail)
        {
            if (!IsOngoing || _caravan == null)
                return;
            if (detail == DeclareWarAction.DeclareWarDetail.CausedByPlayerHostility && (faction1 == _caravan.MapFaction || faction2 == _caravan.MapFaction)
                && PlayerEncounter.Current != null && PlayerEncounter.PlayerIsAttacker)
            {
                FailByPlayerHostility();
                return;
            }
            CheckWarWithPlayer();
            CheckDestination();
        }

        private void OnClanChangedKingdom(Clan clan, Kingdom oldKingdom, Kingdom newKingdom, ChangeKingdomAction.ChangeKingdomActionDetail detail, bool showNotification)
        {
            CheckWarWithPlayer();
            CheckDestination();
        }

        private void OnSettlementOwnerChanged(Settlement settlement, bool openToClaim, Hero newOwner, Hero oldOwner, Hero capturerHero, ChangeOwnerOfSettlementAction.ChangeOwnerOfSettlementDetail detail)
        {
            CheckWarWithPlayer();
            CheckDestination();
        }

        private void CheckWarWithPlayer()
        {
            if (IsOngoing && _origin.MapFaction.IsAtWarWith(Hero.MainHero.MapFaction))
                CompleteQuestWithCancel(Texts.CancelWarLog(QuestGiver.CharacterObject));
        }

        // A destination at war with the caravan or under siege is replaced; a blocked next stop is re-planned.
        private void CheckDestination()
        {
            if (!IsOngoing || _caravan is not { IsActive: true })
                return;
            IFaction faction = _caravan.MapFaction;
            if (!IsValidDestination(_destination, faction))
            {
                Redirect(faction);
                if (!IsOngoing)
                    return;
            }
            if (_caravan.CurrentSettlement == null && _caravan.MapEvent == null && (_waypoint == null || !IsValidDestination(_waypoint, faction)))
                SetNextStop(_lastSettlement);
        }

        private void Redirect(IFaction faction)
        {
            Settlement? newDestination = FindRedirectDestination(faction);
            if (newDestination == null)
            {
                CompleteQuestWithCancel(Texts.LongDistanceCaravanNoDestinationLog(QuestGiver.CharacterObject));
                return;
            }

            Settlement oldDestination = _destination;
            _destination = newDestination;
            CampaignTime newDueTime = CampaignTime.DaysFromNow(GetQuestDurationDays(_lastSettlement, newDestination, faction));
            if (newDueTime > QuestDueTime)
                ChangeQuestDueTime(newDueTime);
            AddLog(Texts.LongDistanceCaravanRedirectLog(oldDestination.EncyclopediaLinkWithName, _destination.EncyclopediaLinkWithName));
            if (_caravan is { CurrentSettlement: null, MapEvent: null })
                SetNextStop(_lastSettlement);
        }

        // The town within a reasonable detour that pays the most for the cargo; else the nearest valid town.
        private Settlement? FindRedirectDestination(IFaction faction)
        {
            if (_caravan == null)
                return null;
            float maxDistance = GetTownDistance(_lastSettlement, _destination) + AverageTownDistance * QuestBalance.LongCaravanRedirectExtraTowns;
            Settlement? best = null;
            int bestValue = -1;
            Settlement? nearest = null;
            float nearestDistance = float.MaxValue;
            MBReadOnlyList<Town> towns = Town.AllTowns;
            for (int i = 0; i < towns.Count; i++)
            {
                Settlement candidate = towns[i].Settlement;
                if (candidate == _origin || candidate == _lastSettlement || candidate == _destination || !IsValidDestination(candidate, faction))
                    continue;
                float distance = GetTownDistance(_lastSettlement, candidate);
                if (distance < nearestDistance)
                {
                    nearest = candidate;
                    nearestDistance = distance;
                }
                if (distance > maxDistance)
                    continue;
                int value = GetCargoSellValue(towns[i], _caravan.ItemRoster);
                if (value > bestValue)
                {
                    best = candidate;
                    bestValue = value;
                }
            }
            return best ?? nearest;
        }

        private static int GetCargoSellValue(Town town, ItemRoster roster)
        {
            int value = 0;
            for (int i = 0; i < roster.Count; i++)
            {
                ItemRosterElement element = roster.GetElementCopyAtIndex(i);
                if (IsCargoElement(element))
                    value += town.GetItemPrice(element.EquipmentElement, null, true) * element.Amount;
            }
            return value;
        }

        private void Arrive()
        {
            if (_caravan == null)
                return;
            Town town = _destination.Town;
            int earned = SellCargo(town, _caravan.ItemRoster, !WillKeepCaravan());
            if (earned > 0)
                GiveGoldAction.ApplyForSettlementToParty(_destination, _caravan.Party, earned, true);
            int profit = _caravan.PartyTradeGold - _fundedGold;
            int share = GetProfitShare(profit);
            if (share > 0)
                GiveGoldAction.ApplyForPartyToCharacter(_caravan.Party, Hero.MainHero, share);

            AddLog(Texts.LongDistanceCaravanSuccessLog(QuestGiver.CharacterObject, _destination.EncyclopediaLinkWithName, earned, profit, share));
            Clan.PlayerClan.AddRenown(QuestBalance.LongCaravanSuccessRenown, false);
            TraitLevelingHelper.OnIssueSolvedThroughQuest(QuestGiver, [new Tuple<TraitObject, int>(DefaultTraits.Honor, QuestBalance.LongCaravanSuccessHonor)]);
            ApplySuccessWorldEffects(QuestGiver);
            QuestHelpers.ApplyOtherNotablesRelation(QuestGiver, null, QuestBalance.LongCaravanOtherNotablesRelation);
            RelationshipChangeWithQuestGiver = QuestBalance.LongCaravanSuccessRelation;
            CompleteQuestWithSuccess();
        }

        private void Fail()
        {
            ApplyFailWorldEffects(QuestGiver, QuestBalance.LongCaravanFailPower, QuestBalance.LongCaravanFailProsperity);
            RelationshipChangeWithQuestGiver = QuestBalance.LongCaravanFailRelation;
            TraitLevelingHelper.OnIssueFailed(QuestGiver, [new Tuple<TraitObject, int>(DefaultTraits.Honor, QuestBalance.LongCaravanFailHonor)]);
            CompleteQuestWithFail();
        }

        private void FailByPlayerHostility()
        {
            ApplyFailWorldEffects(QuestGiver, QuestBalance.LongCaravanFailPower, QuestBalance.LongCaravanHostileProsperity);
            RelationshipChangeWithQuestGiver = QuestBalance.LongCaravanHostileRelation;
            TraitLevelingHelper.OnIssueFailed(QuestGiver, [new Tuple<TraitObject, int>(DefaultTraits.Honor, QuestBalance.LongCaravanHostileHonor)]);
            AddLog(Texts.LongDistanceCaravanAttackedByPlayerLog(), true);
            CompleteQuestWithFail();
        }

        // Called from CompleteQuestWithTimeOut, which finalizes the quest afterwards.
        protected override void OnTimedOut()
        {
            ApplyFailWorldEffects(QuestGiver, QuestBalance.LongCaravanTimeoutPower, QuestBalance.LongCaravanTimeoutProsperity);
            RelationshipChangeWithQuestGiver = QuestBalance.LongCaravanFailRelation;
            TraitLevelingHelper.OnIssueFailed(QuestGiver, [new Tuple<TraitObject, int>(DefaultTraits.Honor, QuestBalance.LongCaravanFailHonor)]);
            AddLog(Texts.LongDistanceCaravanTimeoutLog());
        }

        protected override void OnFinalize() => SettleCaravan();

        // A merchant with no caravan keeps this one as a normal caravan; otherwise it is disbanded, so the world caravan count never grows.
        private bool WillKeepCaravan() => QuestGiver.IsAlive && QuestGiver.OwnedCaravans.Count == 0;

        private void SettleCaravan()
        {
            ReleaseRaiders();
            ReleaseOtherBandits();
            if (_caravan is not { IsActive: true })
                return;
            if (IsTracked(_caravan))
                RemoveTrackedObject(_caravan);

            // A party cannot be removed while its battle is ending; vanilla converts it in the same spot.
            if (_caravan.MapEvent != null)
            {
                if (QuestGiver.IsAlive)
                    ConvertToMerchantCaravan();
                else
                    ReleaseCaravan();
                return;
            }
            if (_caravan.MemberRoster.TotalManCount <= 0)
            {
                DestroyPartyAction.Apply(null, _caravan);
                return;
            }
            if (WillKeepCaravan())
            {
                ConvertToMerchantCaravan();
                return;
            }
            DisbandCaravan();
        }

        // The caravan starts its normal life with the standard trade purse; the rest goes back to the merchant, or the merchant tops it up.
        private void ConvertToMerchantCaravan()
        {
            if (_caravan == null)
                return;
            int difference = _caravan.PartyTradeGold - Campaign.Current.Models.CaravanModel.GetInitialTradeGold(QuestGiver, false, false);
            if (difference > 0)
                GiveGoldAction.ApplyForPartyToCharacter(_caravan.Party, QuestGiver, difference, true);
            else if (difference < 0)
                GiveGoldAction.ApplyForCharacterToParty(QuestGiver, _caravan.Party, -difference, true);
            CaravanPartyComponent.ConvertPartyToCaravanParty(_caravan, QuestGiver, _origin);
            ReleaseCaravan();
        }

        private void ReleaseCaravan()
        {
            if (_caravan == null)
                return;
            _caravan.SetPartyUsedByQuest(false);
            _caravan.Ai.SetDoNotMakeNewDecisions(false);
            _caravan.IgnoreByOtherPartiesTill(CampaignTime.Now);
        }

        // Unsold goods are sold in the nearest market and all gold returns to the merchant before the caravan disbands.
        private void DisbandCaravan()
        {
            if (_caravan == null)
                return;
            Settlement? market = _caravan.CurrentSettlement is { IsTown: true } current ? current : FindNearestMarket();
            if (market != null)
            {
                int earned = SellCargo(market.Town, _caravan.ItemRoster, true);
                if (earned > 0)
                    GiveGoldAction.ApplyForSettlementToParty(market, _caravan.Party, earned, true);
                MoveLeftoverGoods(_caravan.ItemRoster, market.ItemRoster);
            }
            if (QuestGiver.IsAlive && _caravan.PartyTradeGold > 0)
                GiveGoldAction.ApplyForPartyToCharacter(_caravan.Party, QuestGiver, _caravan.PartyTradeGold, true);
            DestroyPartyAction.ApplyForDisbanding(_caravan, market ?? _caravan.CurrentSettlement);
        }

        private Settlement? FindNearestMarket()
        {
            if (_caravan == null)
                return null;
            Settlement? nearest = null;
            float nearestDistance = float.MaxValue;
            MBReadOnlyList<Town> towns = Town.AllTowns;
            for (int i = 0; i < towns.Count; i++)
            {
                Settlement candidate = towns[i].Settlement;
                if (!IsValidDestination(candidate, _caravan.MapFaction))
                    continue;
                float distance = candidate.Position.DistanceSquared(_caravan.Position);
                if (distance < nearestDistance)
                {
                    nearest = candidate;
                    nearestDistance = distance;
                }
            }
            return nearest;
        }

        // Goods the town could not pay for stay in its market instead of vanishing with the party. Pack animals leave with the guards.
        private static void MoveLeftoverGoods(ItemRoster from, ItemRoster to)
        {
            for (int i = from.Count - 1; i >= 0; i--)
            {
                ItemRosterElement element = from.GetElementCopyAtIndex(i);
                if (element.Amount <= 0 || (!IsCargoElement(element) && !element.EquipmentElement.Item.IsFood))
                    continue;
                to.AddToCounts(element.EquipmentElement, element.Amount);
                from.AddToCounts(element.EquipmentElement, -element.Amount);
            }
        }

        // Raiders out of the player's sight are removed so ambush parties never pile up on the map; visible ones become normal bandits.
        private void ReleaseRaiders()
        {
            MobileParty? raiders = _raiderParty;
            _raiderParty = null;
            if (raiders is not { IsActive: true })
                return;
            if (IsTracked(raiders))
                RemoveTrackedObject(raiders);
            if (raiders.MapEvent == null && !raiders.IsVisible)
            {
                DestroyPartyAction.Apply(null, raiders);
                return;
            }
            raiders.Ai.SetDoNotMakeNewDecisions(false);
            raiders.IgnoreByOtherPartiesTill(CampaignTime.Now);
        }

        private void ReleaseOtherBandits()
        {
            MobileParty? bandits = _otherBanditParty;
            _otherBanditParty = null;
            if (bandits is not { IsActive: true })
                return;
            if (IsTracked(bandits))
                RemoveTrackedObject(bandits);
            if (bandits.MapEvent == null)
                bandits.SetMoveModeHold();
            bandits.Ai.SetDoNotMakeNewDecisions(false);
        }
    }
}
