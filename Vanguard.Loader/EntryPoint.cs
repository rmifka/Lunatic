using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Vanguard.Loader.Util;
using Vanguard.Public.Interfaces;
using HarmonyLib;

namespace Vanguard.Loader;

public class EntryPoint
{
    private readonly static ILogger Logger = new Logger();
    private readonly static string ModuleDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Mods");
    private readonly static string LibraryDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Libraries");
    private readonly static List<IModule> Modules = [];

    public static void Initialize()
    {
        Logger.Info("Vanguard.Loader initialized.");

        EnsureDirectory(ModuleDirectory, "Mods");
        EnsureDirectory(LibraryDirectory, "Libraries");

        Logger.Info("Vanguard.Loader finished initializing libraries.");

        InitializeHarmony();
        LoadModules();

        foreach (var module in Modules)
        {
            module.Initialize(Logger);
        }

        Logger.Info("Vanguard.Loader finished initializing modules.");
    }

    private static void EnsureDirectory(string path, string name)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
            Logger.Info($"Created {name} directory.");
        }
    }

    private static void LoadModules()
    {
        var assemblyFiles = Directory.GetFiles(ModuleDirectory, "*.dll");

        foreach (var assemblyPath in assemblyFiles)
        {
            try
            {
                var modAssembly = Assembly.LoadFrom(assemblyPath);
                Logger.Info($"Loaded mod assembly: {assemblyPath}");

                // Find all types that implement IModule
                var moduleTypes = modAssembly.GetTypes()
                    .Where(t => typeof(IModule).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

                foreach (var type in moduleTypes)
                {
                    if (Activator.CreateInstance(type) is IModule moduleInstance)
                    {
                        Modules.Add(moduleInstance);
                        Logger.Info($"Registered module: {type.FullName}");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to load mod: {assemblyPath} - {ex}");
            }
        }
    }


    private static void InitializeHarmony()
    {
        Logger.Info("Initializing Harmony patches...");
        try
        {
            var harmony = new Harmony("com.vanguard.loader");
            harmony.PatchAll();
            Logger.Info("Harmony patches applied successfully.");
        }
        catch (Exception ex)
        {
            Logger.Error($"Harmony failed to initialize: {ex}");
        }
    }
}