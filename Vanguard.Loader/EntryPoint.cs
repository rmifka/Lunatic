using System;
using System.IO;
using System.Reflection;
using Vanguard.Loader.Util;

namespace Vanguard.Loader;

public class EntryPoint : ILoader
{
    private readonly static ILogger Logger = new Logger();
    private readonly static string ModuleDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Mods");
    private readonly static string LibraryDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Libraries");

    public void Initialize()
    {
        Logger.Info("Vanguard.Loader initialized.");

        if (!Directory.Exists(ModuleDirectory))
        {
            Directory.CreateDirectory(ModuleDirectory);
            Logger.Info("Created Mods directory.");
        }

        if (!Directory.Exists(LibraryDirectory))
        {
            Directory.CreateDirectory(LibraryDirectory);
            Logger.Info("Created Libraries directory.");
        }

        LoadLibraries();

        Logger.Info("Vanguard.Loader finished initializing libraries.");

        LoadModules();

        Logger.Info("Vanguard.Loader finished initializing modules.");
    }

    private static void LoadModules()
    {
        var assemblies = Directory.GetFiles(ModuleDirectory, "*.dll");

        foreach (var assembly in assemblies)
        {
            try
            {
                Assembly.LoadFrom(assembly);
                Logger.Info($"Loaded library: {assembly}");
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to load library: {assembly} - {ex}");
            }
        }
    }

    private static void LoadLibraries()
    {
        var assemblies = Directory.GetFiles(LibraryDirectory, "*.dll");

        foreach (var assembly in assemblies)
        {
            try
            {
                Assembly.LoadFrom(assembly);
                Logger.Info($"Loaded library: {assembly}");
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to load library: {assembly} - {ex}");
            }
        }
    }
}