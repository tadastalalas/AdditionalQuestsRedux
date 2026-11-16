using System;

namespace AdditionalQuestsRedux.Data;

// Shape of ModConfiguration.xml and the only place where defaults and ranges live (MCMSettings uses them too).
// One section per MCM group, same names, same order.
public class ModConfiguration
{
    public const bool AiLordRewardsEnabledDefault = true;
    public const bool VanillaAiLordRewardsEnabledDefault = true;
    public const bool BanditArmyEnabledDefault = true;
    public const bool BanditArmyMinTroopReqDisabledDefault = false;
    public const float BanditArmySizeMultiplierDefault = 1f;
    public const float BanditArmySizeMultiplierMin = 0.5f;
    public const float BanditArmySizeMultiplierMax = 5f;
    public const bool MilitiaSpearsEnabledDefault = true;
    public const bool StarvingTownEnabledDefault = true;
    public const bool LongDistanceCaravanEnabledDefault = true;
    public const bool FundTheWallsEnabledDefault = true;
    public const bool DonateEliteRangersEnabledDefault = true;
    public const bool LoggingEnabledDefault = false;

    public QuestsSection Quests { get; set; } = new();

    public BanditArmySection BanditArmy { get; set; } = new();

    public MilitiaSpearsSection MilitiaSpears { get; set; } = new();

    public StarvingTownSection StarvingTown { get; set; } = new();

    public LongDistanceCaravanSection LongDistanceCaravan { get; set; } = new();

    public FundTheWallsSection FundTheWalls { get; set; } = new();

    public DonateEliteRangersSection DonateEliteRangers { get; set; } = new();

    // Always the last section.
    public TechnicalSettingsSection TechnicalSettings { get; set; } = new();

    internal void Clamp()
    {
        float multiplier = BanditArmy.SizeMultiplier;
        BanditArmy.SizeMultiplier = float.IsNaN(multiplier) ? BanditArmySizeMultiplierDefault : Math.Min(Math.Max(multiplier, BanditArmySizeMultiplierMin), BanditArmySizeMultiplierMax);
    }

    public class QuestsSection
    {
        public bool AiLordRewardsEnabled { get; set; } = AiLordRewardsEnabledDefault;
        public bool VanillaAiLordRewardsEnabled { get; set; } = VanillaAiLordRewardsEnabledDefault;
    }

    public class BanditArmySection
    {
        public bool Enabled { get; set; } = BanditArmyEnabledDefault;
        public bool MinTroopReqDisabled { get; set; } = BanditArmyMinTroopReqDisabledDefault;
        public float SizeMultiplier { get; set; } = BanditArmySizeMultiplierDefault;
    }

    public class MilitiaSpearsSection
    {
        public bool Enabled { get; set; } = MilitiaSpearsEnabledDefault;
    }

    public class StarvingTownSection
    {
        public bool Enabled { get; set; } = StarvingTownEnabledDefault;
    }

    public class LongDistanceCaravanSection
    {
        public bool Enabled { get; set; } = LongDistanceCaravanEnabledDefault;
    }

    public class FundTheWallsSection
    {
        public bool Enabled { get; set; } = FundTheWallsEnabledDefault;
    }

    public class DonateEliteRangersSection
    {
        public bool Enabled { get; set; } = DonateEliteRangersEnabledDefault;
    }

    public class TechnicalSettingsSection
    {
        // Always the last entry.
        public bool LoggingEnabled { get; set; } = LoggingEnabledDefault;
    }
}