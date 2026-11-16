namespace AdditionalQuestsRedux.Core;

// Gameplay numbers for all quests. Relation values go through RelationshipChangeWithQuestGiver,
// which vanilla applies once when the quest completes.
internal static class QuestBalance
{
    // Shared
    internal const float IssueDurationDays = 20f;
    internal const float MinRelationWithIssueGiver = -10f;

    // Bandit Army
    internal const float BanditArmyHideoutMaxDistance = 35f; // straight-line map distance from the village gate (v1.2: squared 1225)
    internal const float BanditArmyQuestDurationDays = 10f;
    internal const int BanditArmyRewardGold = 3000;
    internal const int BanditArmyMinClanTier = 1;
    internal const int BanditArmyMinTroops = 50; // healthy members incl. the player; MCM can disable this
    internal const int BanditArmySizeRollMin = 30; // party size = roll + one extra roll per player clan tier
    internal const int BanditArmySizeRollMax = 75;
    internal const float BanditArmyLooterShare = 0.5f; // the rest are hideout-culture bandits
    internal const float BanditArmyLooterShareVariance = 0.1f; // +/- around the share
    internal const int BanditArmyFoodItemTypes = 2; // food is split between grain and meat
    internal const float BanditArmyMapArrowLife = 5f;
    internal const float BanditArmyMapArrowError = 0.1f;
    internal const float BanditArmySuccessRenown = 5f;
    internal const float BanditArmySuccessPower = 25f;
    internal const int BanditArmySuccessRelation = 20;
    internal const int BanditArmyOtherNotablesRelation = 10;
    internal const float BanditArmySuccessHearth = 100f;
    internal const float BanditArmyFailPower = -25f;
    internal const int BanditArmyFailRelation = -10;
    internal const int BanditArmyGhiNeededMen = 40; // Governors Handle Issues only: garrison troops required
    internal const int BanditArmyGhiSkill = 100; // GHI only: Tactics or Leadership
    internal const int BanditArmyGhiSkillXp = 500; // GHI only: governor skill XP on success

    // Militia Spears
    internal const float MilitiaSpearsMaxVillageMilitia = 20f;
    internal const float MilitiaSpearsTestMilitia = 10f; // give_militia_spears console command lowers village militia to this; must stay below MilitiaSpearsMaxVillageMilitia
    internal const float MilitiaSpearsQuestDurationDays = 14f;
    internal const int MilitiaSpearsNeededSpears = 20;
    internal const int MilitiaSpearsBaseRewardGold = 500; // plus the average town market price of the needed spears
    internal const int MilitiaSpearsSuccessMercyXp = 50;
    internal const int MilitiaSpearsSuccessGenerosityXp = 30;
    internal const float MilitiaSpearsSuccessPower = 25f;
    internal const float MilitiaSpearsSuccessProsperity = 50f; // bound town
    internal const float MilitiaSpearsSuccessMilitia = 20f;
    internal const int MilitiaSpearsSuccessRelation = 10;
    internal const float MilitiaSpearsFailPower = -15f;
    internal const float MilitiaSpearsFailProsperity = -25f; // bound town
    internal const int MilitiaSpearsFailRelation = -5;
    internal const int MilitiaSpearsGhiNeededMen = 5; // Governors Handle Issues only: garrison troops required
    internal const int MilitiaSpearsGhiSkill = 60; // GHI only: Trade or Smithing
    internal const int MilitiaSpearsGhiSkillXp = 800; // GHI only: governor skill XP on success

    // Starving Town
    internal const float StarvingTownQuestDurationDays = 14f;
    internal const int StarvingTownNeededFood = 300;
    internal const int StarvingTownPriceMultiplier = 3; // times the world average price per delivered unit
    internal const float StarvingTownFoodStocksBonus = 300f;
    internal const float StarvingTownSuccessRenown = 3f;
    internal const float StarvingTownSuccessPower = 25f;
    internal const int StarvingTownSuccessRelation = 10;
    internal const int StarvingTownOtherNotablesRelation = 5;
    internal const float StarvingTownSuccessProsperity = 100f;
    internal const float StarvingTownFailPower = -25f;
    internal const float StarvingTownFailProsperity = -50f;
    internal const int StarvingTownFailRelation = -10;
    internal const int StarvingTownGhiNeededMen = 8; // Governors Handle Issues only: garrison troops required
    internal const int StarvingTownGhiSkill = 60; // GHI only: Trade or Steward
    internal const int StarvingTownGhiSkillXp = 800; // GHI only: governor skill XP on success

    // Long Distance Caravan (distances in "towns" are multiples of the average distance between two closest towns)
    internal const int LongCaravanMinClanTier = 4;
    internal const int LongCaravanMinTroops = 50; // healthy members incl. the player
    internal const int LongCaravanMerchantReserve = 1000; // gold the merchant always keeps
    internal const int LongCaravanMinBudget = 4000; // merchant gold above the reserve needed to offer the quest
    internal const int LongCaravanMaxBudget = 10000; // most the merchant invests in food and cargo
    internal const float LongCaravanMinDistanceTowns = 4f; // destination path distance from the origin
    internal const float LongCaravanMaxDistanceTowns = 10f;
    internal const float LongCaravanMinMargin = 1.25f; // destination sell price / origin buy price, per unit
    internal const float LongCaravanMaxItemBudgetShare = 0.25f; // one item type takes at most this share of the budget
    internal const float LongCaravanMaxDestinationGoldShare = 0.5f; // planned sales use at most this share of the destination's gold
    internal const int LongCaravanMaxPurchaseUnits = 600; // safety cap on unit-by-unit buying
    internal const int LongCaravanGuards = 20; // plus the caravan master
    internal const float LongCaravanPackAnimalsPerMan = 1f; // vanilla escort caravans: 0.5
    internal const float LongCaravanFoodMargin = 1.25f; // food bought = estimated days x daily use x this
    internal const float LongCaravanBaseSpeed = 4f; // vanilla escort custom party base speed
    internal const float LongCaravanDirectLegTowns = 1.5f; // goes straight to the destination from this close
    internal const float LongCaravanMaxLegTowns = 2f; // longest leg between waypoint towns
    internal const int LongCaravanMaxRouteLegs = 30; // safety cap when simulating the route
    internal const float LongCaravanRedirectExtraTowns = 2f; // a new destination may be this much farther than the old one
    internal const float LongCaravanEstimatedSpeed = 3f; // map distance per hour used for the quest duration
    internal const float LongCaravanExtraDays = 3f; // slack added to the estimated trip
    internal const float LongCaravanMinQuestDays = 30f; // quest time limit never below this
    internal const float LongCaravanDurationFactor = 1.5f; // time limit = estimated trip x this, covers slow escorts and battles
    internal const int LongCaravanSettlementWaitHours = 5; // vanilla escort: 5
    internal const int LongCaravanMaxReinforcements = 15; // vanilla escort: up to 15 guards hired per town
    internal const float LongCaravanAmbushChance = 0.5f; // rolled once per leg
    internal const int LongCaravanMaxRaiders = 40; // vanilla escort: 40
    internal const float LongCaravanRaiderShare = 0.7f; // of player + caravan healthy troops; vanilla escort: 0.7
    internal const float LongCaravanRaiderLevelWeightBase = 64f; // raider troop weight = this - troop level, so low tiers are common; vanilla escort: 64
    internal const float LongCaravanRaiderMinTargetDistance = 10f; // vanilla escort: raiders spawn when 10 < distance to the next town
    internal const float LongCaravanRaiderSpawnTowns = 1.25f; // ... and distance < this; vanilla escort: 1.25
    internal const float LongCaravanBanditAttackRadiusFactor = 2.5f; // x encounter joining radius; vanilla escort: 2.5
    internal const float LongCaravanBanditMinTownDistance = 5f; // other bandits attack only this far from both towns; vanilla escort: 5
    internal const float LongCaravanBanditCloseDistanceSquared = 9f; // slower bandits still attack this close; vanilla escort: 9
    internal const int LongCaravanFollowExtraHours = 5; // vanilla escort: 5
    internal const float LongCaravanEncounterDistanceSquared = 1f; // vanilla escort: 1
    internal const float LongCaravanInvulnerableHours = 3f; // caravan battle waits for the player; vanilla escort: 3
    internal const float LongCaravanLoseSightWarnDistance = 3f; // beyond the player's seeing range; vanilla escort: 3
    internal const float LongCaravanLoseSightFailDistance = 20f; // vanilla escort: 20
    internal const float LongCaravanSpeedStep = 0.05f; // vanilla escort speed matching
    internal const float LongCaravanSpeedTolerance = 1f;
    internal const int LongCaravanSpeedMaxSteps = 200; // safety cap on speed matching
    internal const float LongCaravanSlowSpeed = 2f; // after a won battle, slower caravans get spare horses; vanilla escort: 2
    internal const int LongCaravanSpareHorses = 5; // vanilla escort: 5
    internal const float LongCaravanPlayerProfitShare = 0.5f; // also the AI lord's and the GHI share
    internal const float LongCaravanSuccessRenown = 5f;
    internal const int LongCaravanSuccessRelation = 10;
    internal const int LongCaravanOtherNotablesRelation = 5;
    internal const float LongCaravanSuccessPower = 20f;
    internal const float LongCaravanSuccessProsperity = 20f;
    internal const int LongCaravanSuccessHonor = 50; // vanilla escort: 50
    internal const float LongCaravanFailPower = -10f; // vanilla escort: -10
    internal const int LongCaravanFailRelation = -5;
    internal const int LongCaravanFailHonor = -20;
    internal const float LongCaravanFailProsperity = -10f;
    internal const int LongCaravanHostileRelation = -10; // player attacked the caravan; vanilla escort: -10
    internal const int LongCaravanHostileHonor = -80;
    internal const float LongCaravanHostileProsperity = -20f;
    internal const float LongCaravanTimeoutPower = -5f; // vanilla escort: -5
    internal const float LongCaravanTimeoutProsperity = -20f;
    internal const int LongCaravanGhiNeededMen = 20; // Governors Handle Issues only: garrison troops required
    internal const int LongCaravanGhiSkill = 100; // GHI only: Trade or Tactics
    internal const int LongCaravanGhiSkillXp = 800; // GHI only: governor skill XP on success
    internal const int LongCaravanStopNoticeExtraMs = 100; // extra on-screen time of the "We are traveling to ..." notice; vanilla escort: 100

    // Fund the Walls (the gold goes into vanilla Town.BoostBuildingProcess, the construction reserve)
    internal const int FundWallsMaxReserve = 5000; // offered and kept alive only while the reserve is below this
    internal const int FundWallsCastleMinGold = 5000;
    internal const int FundWallsCastleMaxGold = 15000;
    internal const float FundWallsCastleMinProsperity = 500f; // lowest starting castle prosperity is 420
    internal const float FundWallsCastleMaxProsperity = 2000f; // vanilla Mid prosperity level
    internal const int FundWallsTownMinGold = 10000;
    internal const int FundWallsTownMaxGold = 30000;
    internal const float FundWallsTownMinProsperity = 2000f; // vanilla Mid prosperity level
    internal const float FundWallsTownMaxProsperity = 5000f; // vanilla High prosperity level
    internal const int FundWallsGoldStep = 1000; // requested gold is rounded to this
    internal const int FundWallsMinClanTier = 4;
    internal const float FundWallsQuestDurationDays = 30f; // vanilla garrison troops quest: 30
    internal const int FundWallsMinRelation = 5; // relation and renown scale with the gold, from FundWallsCastleMinGold to FundWallsTownMaxGold
    internal const int FundWallsMaxRelation = 20;
    internal const float FundWallsMinRenown = 3f;
    internal const float FundWallsMaxRenown = 15f;
    internal const int FundWallsSuccessGenerosityXp = 50;
    internal const int FundWallsFailRelation = -5; // vanilla garrison troops quest: -5
    internal const int FundWallsAiMinGoldLeft = 10000; // an AI lord pays only if this much gold stays with him
    internal const float FundWallsAiOwnClanChance = 0.05f; // per settlement entry; vanilla AI issue completion: 0.05 in the own clan's settlement
    internal const float FundWallsAiOtherClanChance = 0.01f; // vanilla: 0.01 elsewhere

    // Donate Elite Rangers (the best foot archers or crossbowmen of the owner's kingdom culture go into the garrison)
    internal const int RangersCastleCount = 10;
    internal const int RangersTownCount = 20;
    internal const int RangersMinClanTier = 4;
    internal const float RangersQuestDurationDays = 60f; // vanilla garrison troops quest: 30, but these are top-tier troops
    internal const int RangersBaseRewardGold = 1500; // vanilla garrison troops quest: 1500 + 1.5 x recruit cost x count
    internal const float RangersRecruitCostMultiplier = 2f;
    internal const int RangersGiverMinGoldLeft = 10000; // the lord offers it only if he can pay and still keep this much
    internal const int RangersRelation = 10; // vanilla garrison troops quest: 2
    internal const float RangersRenown = 5f; // vanilla garrison troops quest: 1
    internal const float RangersInfluenceDivisor = 3f; // vanilla GarrisonTroopsCampaignBehavior: siege troop power / 3
    internal const int RangersFailRelation = -5; // vanilla garrison troops quest: -5
    internal const float RangersAiOwnClanChance = 0.05f; // per settlement entry, same as Fund the Walls
    internal const float RangersAiOtherClanChance = 0.01f;

    // Vanilla issues resolved by AI lords (gold is the issue's own RewardGold)
    internal const int VanillaAiLordRelation = 5; // AI lord <-> issue giver
    internal const float VanillaAiLordRenown = 1f;
    internal const float VanillaAiLordIssueOwnerPower = 10f; // notable issue givers only
}
