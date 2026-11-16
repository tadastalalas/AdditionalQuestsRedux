using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using AdditionalQuestsRedux.Data;
using AdditionalQuestsRedux.Diagnostics;

namespace AdditionalQuestsRedux.Core;

// The only place mod code reads settings from. MCM loaded -> MCM. MCM not loaded -> ModConfiguration.xml.
internal static class ModSettings
{
    private const string McmAssemblyName = "MCMv5";

    private static ModConfiguration _xml = new();
    private static XmlSerializer? _serializer;
    private static bool _useMcm;

    internal static bool AiLordRewardsEnabled => _useMcm ? McmAiLordRewardsEnabled() : _xml.Quests.AiLordRewardsEnabled;

    internal static bool VanillaAiLordRewardsEnabled => _useMcm ? McmVanillaAiLordRewardsEnabled() : _xml.Quests.VanillaAiLordRewardsEnabled;

    internal static bool BanditArmyEnabled => _useMcm ? McmBanditArmyEnabled() : _xml.BanditArmy.Enabled;

    internal static bool BanditArmyMinTroopReqDisabled => _useMcm ? McmBanditArmyMinTroopReqDisabled() : _xml.BanditArmy.MinTroopReqDisabled;

    internal static float BanditArmySizeMultiplier => _useMcm ? McmBanditArmySizeMultiplier() : _xml.BanditArmy.SizeMultiplier;

    internal static bool MilitiaSpearsEnabled => _useMcm ? McmMilitiaSpearsEnabled() : _xml.MilitiaSpears.Enabled;

    internal static bool StarvingTownEnabled => _useMcm ? McmStarvingTownEnabled() : _xml.StarvingTown.Enabled;

    internal static bool LongDistanceCaravanEnabled => _useMcm ? McmLongDistanceCaravanEnabled() : _xml.LongDistanceCaravan.Enabled;

    internal static bool FundTheWallsEnabled => _useMcm ? McmFundTheWallsEnabled() : _xml.FundTheWalls.Enabled;

    internal static bool DonateEliteRangersEnabled => _useMcm ? McmDonateEliteRangersEnabled() : _xml.DonateEliteRangers.Enabled;

    internal static bool LoggingEnabled => _useMcm ? McmLoggingEnabled() : _xml.TechnicalSettings.LoggingEnabled;

    private static XmlSerializer Serializer => _serializer ??= new XmlSerializer(typeof(ModConfiguration));

    // Called on every campaign start.
    internal static void Reload()
    {
        _useMcm = false;
        _xml = new ModConfiguration();

        if (IsMcmLoaded())
        {
            if (McmHasSettings())
            {
                _useMcm = true;
                return;
            }

            AqLog.Warn("MCM is loaded but returned no settings for this mod. Using " + ModInfo.ConfigurationFileName + " instead.");
        }

        _xml = LoadFromFile();
    }

    private static bool IsMcmLoaded()
    {
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        for (int i = 0; i < assemblies.Length; i++)
        {
            if (assemblies[i].GetName().Name == McmAssemblyName) return true;
        }

        return false;
    }

    // Every method that touches MCMSettings must be NoInlining and may only be called when MCM is loaded.
    // Otherwise the game tries to load MCM's classes and crashes when MCM is not installed.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool McmHasSettings() => MCMSettings.Instance != null;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool McmAiLordRewardsEnabled() => MCMSettings.Instance?.AiLordRewardsEnabled ?? ModConfiguration.AiLordRewardsEnabledDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool McmVanillaAiLordRewardsEnabled() => MCMSettings.Instance?.VanillaAiLordRewardsEnabled ?? ModConfiguration.VanillaAiLordRewardsEnabledDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool McmBanditArmyEnabled() => MCMSettings.Instance?.BanditArmyEnabled ?? ModConfiguration.BanditArmyEnabledDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool McmBanditArmyMinTroopReqDisabled() => MCMSettings.Instance?.BanditArmyMinTroopReqDisabled ?? ModConfiguration.BanditArmyMinTroopReqDisabledDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static float McmBanditArmySizeMultiplier() => MCMSettings.Instance?.BanditArmySizeMultiplier ?? ModConfiguration.BanditArmySizeMultiplierDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool McmMilitiaSpearsEnabled() => MCMSettings.Instance?.MilitiaSpearsEnabled ?? ModConfiguration.MilitiaSpearsEnabledDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool McmStarvingTownEnabled() => MCMSettings.Instance?.StarvingTownEnabled ?? ModConfiguration.StarvingTownEnabledDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool McmLongDistanceCaravanEnabled() => MCMSettings.Instance?.LongDistanceCaravanEnabled ?? ModConfiguration.LongDistanceCaravanEnabledDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool McmFundTheWallsEnabled() => MCMSettings.Instance?.FundTheWallsEnabled ?? ModConfiguration.FundTheWallsEnabledDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool McmDonateEliteRangersEnabled() => MCMSettings.Instance?.DonateEliteRangersEnabled ?? ModConfiguration.DonateEliteRangersEnabledDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool McmLoggingEnabled() => MCMSettings.Instance?.LoggingEnabled ?? ModConfiguration.LoggingEnabledDefault;

    private static ModConfiguration LoadFromFile()
    {
        string path = Path.Combine(ModInfo.ModuleRoot, ModInfo.ConfigurationFileName);
        ModConfiguration configuration = new();

        if (!File.Exists(path))
        {
            AqLog.Warn(ModInfo.ConfigurationFileName + " was missing. Created it with the default settings.");
            TryWrite(path, configuration);
            return configuration;
        }

        try
        {
            configuration = Read(path);
            if (!HasEverySetting(path, configuration))
            {
                AqLog.Warn(ModInfo.ConfigurationFileName + " had missing settings. Added them with their default values.");
                TryWrite(path, configuration);
            }
        }
        catch (Exception ex) when (ex is InvalidOperationException or XmlException)
        {
            AqLog.Warn(ModInfo.ConfigurationFileName + " was broken and was replaced with the default settings. Reason: " + (ex.InnerException?.Message ?? ex.Message));
            configuration = new ModConfiguration();
            TryWrite(path, configuration);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            AqLog.Warn(ModInfo.ConfigurationFileName + " could not be opened. Using the default settings. Reason: " + ex.Message);
            configuration = new ModConfiguration();
        }

        // Out-of-range values are corrected in memory only; the file keeps what the player typed.
        configuration.Clamp();
        return configuration;
    }

    private static ModConfiguration Read(string path)
    {
        using FileStream stream = File.OpenRead(path);
        return Serializer.Deserialize(stream) as ModConfiguration ?? new ModConfiguration();
    }

    private static void TryWrite(string path, ModConfiguration configuration)
    {
        try
        {
            XmlWriterSettings writerSettings = new() { Indent = true, Encoding = Encoding.UTF8 };
            using XmlWriter writer = XmlWriter.Create(path, writerSettings);
            Serializer.Serialize(writer, configuration, NoNamespaces());
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            AqLog.Warn(ModInfo.ConfigurationFileName + " could not be written. The settings are kept in memory for this session. Reason: " + ex.Message);
        }
    }

    private static bool HasEverySetting(string path, ModConfiguration configuration)
    {
        XDocument complete = new();
        using (XmlWriter writer = complete.CreateWriter())
        {
            Serializer.Serialize(writer, configuration, NoNamespaces());
        }

        XElement? fileRoot = XDocument.Load(path).Root;
        return fileRoot != null && complete.Root != null && ContainsEveryElement(fileRoot, complete.Root);
    }

    private static bool ContainsEveryElement(XElement actual, XElement expected)
    {
        foreach (XElement expectedChild in expected.Elements())
        {
            XElement? actualChild = actual.Element(expectedChild.Name);
            if (actualChild == null || !ContainsEveryElement(actualChild, expectedChild)) return false;
        }

        return true;
    }

    private static XmlSerializerNamespaces NoNamespaces()
    {
        XmlSerializerNamespaces namespaces = new();
        namespaces.Add(string.Empty, string.Empty);
        return namespaces;
    }
}