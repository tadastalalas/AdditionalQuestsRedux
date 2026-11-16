namespace AdditionalQuestsRedux.Core;

// Read-only gates over ModSettings (MCM when loaded, ModConfiguration.xml otherwise).
internal static class QuestRules
{
    internal static bool IsBanditArmyEnabled => ModSettings.BanditArmyEnabled;

    internal static bool IsBanditArmyMinTroopRequirementEnabled => !ModSettings.BanditArmyMinTroopReqDisabled;

    internal static float BanditArmySizeMultiplier => ModSettings.BanditArmySizeMultiplier;

    internal static bool IsMilitiaSpearsEnabled => ModSettings.MilitiaSpearsEnabled;

    internal static bool IsStarvingTownEnabled => ModSettings.StarvingTownEnabled;

    internal static bool IsLongDistanceCaravanEnabled => ModSettings.LongDistanceCaravanEnabled;

    internal static bool IsFundTheWallsEnabled => ModSettings.FundTheWallsEnabled;

    internal static bool IsDonateEliteRangersEnabled => ModSettings.DonateEliteRangersEnabled;

    internal static bool IsAiLordRewardsEnabled => ModSettings.AiLordRewardsEnabled;

    internal static bool IsVanillaAiLordRewardsEnabled => ModSettings.VanillaAiLordRewardsEnabled;
}