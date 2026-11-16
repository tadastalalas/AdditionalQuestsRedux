using AdditionalQuestsRedux.Core;
using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Base.Global;

namespace AdditionalQuestsRedux.Data;

// Used only when MCM is loaded. The only file that may use MCM types.
internal sealed class MCMSettings : AttributeGlobalSettings<MCMSettings>
{
    // Released format: changing it resets every player's saved settings.
    private const string SaveFormat = "json";
    private const string PercentFormat = "#0%";

    // Released Id (= ModuleId): changing it resets every player's saved settings.
    public override string Id => ModInfo.ModuleId;

    public override string DisplayName => Texts.ModName.ToString();

    public override string FolderName => ModInfo.ModuleId;

    public override string FormatType => SaveFormat;

    [SettingPropertyBool(Texts.Ids.AiLordRewards, Order = 0, RequireRestart = false, HintText = Texts.Ids.AiLordRewardsHint)]
    [SettingPropertyGroup(Texts.Ids.GroupQuests, GroupOrder = 0)]
    public bool AiLordRewardsEnabled { get; set; } = ModConfiguration.AiLordRewardsEnabledDefault;

    [SettingPropertyBool(Texts.Ids.VanillaAiLordRewards, Order = 1, RequireRestart = false, HintText = Texts.Ids.VanillaAiLordRewardsHint)]
    [SettingPropertyGroup(Texts.Ids.GroupQuests, GroupOrder = 0)]
    public bool VanillaAiLordRewardsEnabled { get; set; } = ModConfiguration.VanillaAiLordRewardsEnabledDefault;

    [SettingPropertyBool(Texts.Ids.QuestEnabled, Order = 0, RequireRestart = false, HintText = Texts.Ids.BanditArmyEnabledHint)]
    [SettingPropertyGroup(Texts.Ids.GroupBanditArmy, GroupOrder = 0)]
    public bool BanditArmyEnabled { get; set; } = ModConfiguration.BanditArmyEnabledDefault;

    [SettingPropertyBool(Texts.Ids.BanditArmyMinTroopReqDisabled, Order = 1, RequireRestart = false, HintText = Texts.Ids.BanditArmyMinTroopReqDisabledHint)]
    [SettingPropertyGroup(Texts.Ids.GroupBanditArmy, GroupOrder = 0)]
    public bool BanditArmyMinTroopReqDisabled { get; set; } = ModConfiguration.BanditArmyMinTroopReqDisabledDefault;

    [SettingPropertyFloatingInteger(Texts.Ids.BanditArmySizeMultiplier, ModConfiguration.BanditArmySizeMultiplierMin, ModConfiguration.BanditArmySizeMultiplierMax, PercentFormat, Order = 2, RequireRestart = false, HintText = Texts.Ids.BanditArmySizeMultiplierHint)]
    [SettingPropertyGroup(Texts.Ids.GroupBanditArmy, GroupOrder = 0)]
    public float BanditArmySizeMultiplier { get; set; } = ModConfiguration.BanditArmySizeMultiplierDefault;

    [SettingPropertyBool(Texts.Ids.QuestEnabled, Order = 0, RequireRestart = false, HintText = Texts.Ids.MilitiaSpearsEnabledHint)]
    [SettingPropertyGroup(Texts.Ids.GroupMilitiaSpears, GroupOrder = 1)]
    public bool MilitiaSpearsEnabled { get; set; } = ModConfiguration.MilitiaSpearsEnabledDefault;

    [SettingPropertyBool(Texts.Ids.QuestEnabled, Order = 0, RequireRestart = false, HintText = Texts.Ids.StarvingTownEnabledHint)]
    [SettingPropertyGroup(Texts.Ids.GroupStarvingTown, GroupOrder = 2)]
    public bool StarvingTownEnabled { get; set; } = ModConfiguration.StarvingTownEnabledDefault;

    [SettingPropertyBool(Texts.Ids.QuestEnabled, Order = 0, RequireRestart = false, HintText = Texts.Ids.LongDistanceCaravanEnabledHint)]
    [SettingPropertyGroup(Texts.Ids.GroupLongDistanceCaravan, GroupOrder = 3)]
    public bool LongDistanceCaravanEnabled { get; set; } = ModConfiguration.LongDistanceCaravanEnabledDefault;

    [SettingPropertyBool(Texts.Ids.QuestEnabled, Order = 0, RequireRestart = false, HintText = Texts.Ids.FundTheWallsEnabledHint)]
    [SettingPropertyGroup(Texts.Ids.GroupFundTheWalls, GroupOrder = 4)]
    public bool FundTheWallsEnabled { get; set; } = ModConfiguration.FundTheWallsEnabledDefault;

    [SettingPropertyBool(Texts.Ids.QuestEnabled, Order = 0, RequireRestart = false, HintText = Texts.Ids.DonateEliteRangersEnabledHint)]
    [SettingPropertyGroup(Texts.Ids.GroupDonateEliteRangers, GroupOrder = 5)]
    public bool DonateEliteRangersEnabled { get; set; } = ModConfiguration.DonateEliteRangersEnabledDefault;

    // "Technical settings" is always the last group and "Logging for debugging" its last entry.
    [SettingPropertyBool(Texts.Ids.LoggingEnabled, Order = 0, RequireRestart = false, HintText = Texts.Ids.LoggingEnabledHint)]
    [SettingPropertyGroup(Texts.Ids.GroupTechnical, GroupOrder = 1)]
    public bool LoggingEnabled { get; set; } = ModConfiguration.LoggingEnabledDefault;
}