using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Lunatic.Public.Interfaces;
using HarmonyLib;
using Lunatic.Loader.Util;

namespace Lunatic.Loader;

public class EntryPoint
{
    private static readonly ILunaticLogger LunaticLogger = new LunaticLogger();
    private static readonly string ModuleDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Mods");
    private static readonly string LibraryDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Libraries");
    private static readonly List<IModule> Modules = [];

    public static void Initialize()
    {
        LunaticLogger.Info("Lunatic.Loader initialized.");

        EnsureDirectory(ModuleDirectory, "Mods");
        EnsureDirectory(LibraryDirectory, "Libraries");

        LunaticLogger.Info("Lunatic.Loader finished initializing libraries.");

        LoadModules();

        foreach (var module in Modules)
        {
            module.Initialize(LunaticLogger);
        }

        LunaticLogger.Info("Lunatic.Loader finished initializing modules.");
    }

    private static void EnsureDirectory(string path, string name)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
            LunaticLogger.Info($"Created {name} directory.");
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

                var moduleTypes = modAssembly.GetTypes()
                    .Where(t =>
                        typeof(IModule).IsAssignableFrom(t)
                        && !t.IsInterface
                        && !t.IsAbstract);

                InitializeHarmony(modAssembly);

                foreach (var type in moduleTypes)
                {
                    if (Activator.CreateInstance(type) is IModule moduleInstance)
                    {
                        Modules.Add(moduleInstance);
                        LunaticLogger.Info($"Registered module: {type.FullName}");
                    }
                }
            }
            catch (Exception ex)
            {
                LunaticLogger.Error($"Failed to load mod: {assemblyPath} - {ex}");
            }
        }
    }


    private static void InitializeHarmony(Assembly assembly)
    {
        LunaticLogger.Info("Initializing Harmony patches...");
        try
        {
            HarmonyInstance.PatchAll(assembly);
            LunaticLogger.Info("Harmony patches applied successfully.");
        }
        catch (Exception ex)
        {
            LunaticLogger.Error($"Harmony failed to initialize: {ex}");
        }
    }


    private static readonly Harmony HarmonyInstance = new("com.Lunatic.Loader");
}