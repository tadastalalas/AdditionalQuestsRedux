using AdditionalQuestsRedux.Behaviors;
using AdditionalQuestsRedux.Compat;
using AdditionalQuestsRedux.Core;
using AdditionalQuestsRedux.Diagnostics;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace AdditionalQuestsRedux;

public class SubModule : MBSubModuleBase
{
    private readonly Harmony _harmony = new(ModInfo.ModuleId);

    // All module assemblies are loaded by now, so optional-mod types can be found. TryApply runs once.
    protected override void OnBeforeInitialModuleScreenSetAsRoot()
    {
        base.OnBeforeInitialModuleScreenSetAsRoot();
        GhiCompat.TryApply(_harmony);
    }

    protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
    {
        base.OnGameStart(game, gameStarterObject);

        // Custom battles and other non-campaign modes have no campaign behaviors.
        if (game.GameType is not Campaign || gameStarterObject is not CampaignGameStarter starter)
            return;

        ModSettings.Reload();

        // Always registered so MCM toggles apply without reloading; each behavior checks its QuestRules gate.
        starter.AddBehavior(new BanditArmyIssueBehavior());
        starter.AddBehavior(new MilitiaSpearsIssueBehavior());
        starter.AddBehavior(new StarvingTownIssueBehavior());
        starter.AddBehavior(new LongDistanceCaravanIssueBehavior());
        starter.AddBehavior(new FundTheWallsIssueBehavior());
        starter.AddBehavior(new DonateEliteRangersIssueBehavior());
        starter.AddBehavior(new VanillaAiLordRewardsBehavior());

        AqLog.Debug(ModInfo.ModuleId + " loaded successfully.");
    }
}
