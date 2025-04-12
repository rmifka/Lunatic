using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Lunatic.Bootstrapper
{
    public static class Bootstrapper
    {
        private static readonly string LibraryDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Libraries");

        private static void LoadLibraries()
        {
            var libFiles = Directory.GetFiles(LibraryDirectory);

            foreach (var libPath in libFiles)
            {
                try
                {
                    Assembly.LoadFrom(libPath);
                    LunaticBootstrapperLogger.Log($"Loaded library: {libPath}");
                }
                catch (Exception ex)
                {
                    LunaticBootstrapperLogger.Log($"Failed to load library: {libPath} - {ex}");
                }
            }
        }

        public static void Init()
        {
            const string LunaticLoader = "Lunatic.Loader";
            const string LunaticPublic = "Lunatic.Public";

            if (AppDomain.CurrentDomain.GetAssemblies().Any(a => a.FullName.Contains(LunaticLoader)))
            {
                LunaticBootstrapperLogger.Log("Bootstrapper already initialized!");
                LunaticBootstrapperLogger.Omit();
                return;
            }

            if (!Directory.Exists("Lunatic_Logs"))
            {
                Directory.CreateDirectory("Lunatic_Logs");
            }

            try
            {
                LoadLibraries();
                LunaticBootstrapperLogger.Log("Bootstrapper Initialized!");

                var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                var dataFolders = Directory.GetDirectories(baseDir, "*_Data", SearchOption.TopDirectoryOnly);
                var managedPath = Path.Combine(dataFolders[0], "Managed");

                var publicPath = Path.Combine(managedPath, $"{LunaticPublic}.dll");
                Assembly.LoadFrom(publicPath);
                LunaticBootstrapperLogger.Log($"{LunaticPublic} loaded!");

                var loaderPath = Path.Combine(managedPath, $"{LunaticLoader}.dll");

                if (!File.Exists(loaderPath))
                {
                    LunaticBootstrapperLogger.Log($"{LunaticLoader}.dll not found at: " + loaderPath);
                    return;
                }

                var loaderAssembly = Assembly.LoadFrom(loaderPath);
                var entryType = loaderAssembly.GetType($"{LunaticLoader}.EntryPoint");
                var initMethod = entryType?.GetMethod("Initialize", BindingFlags.Public | BindingFlags.Static);

                if (initMethod != null)
                {
                    initMethod.Invoke(null, null);
                    LunaticBootstrapperLogger.Log($"{LunaticLoader} initialized!");
                }
                else
                {
                    LunaticBootstrapperLogger.Log($"Could not find Initialize method in {LunaticLoader}.");
                }
            }
            catch (Exception ex)
            {
                LunaticBootstrapperLogger.Log($"Bootstrapper Error: {ex}");
            }
        }

        private static class LunaticBootstrapperLogger
        {
            private static readonly string LogFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Lunatic_Logs",
                $"LunaticBootstrapper.{DateTime.Now:yyyy-MM-dd}.vlog");

            private static bool ShouldLog = true;

            public static void Log(string message)
            {
                if (!ShouldLog)
                {
                    return;
                }

                try
                {
                    using (var writer = new StreamWriter(LogFilePath, true))
                    {
                        writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}");
                        Console.WriteLine($"[Lunatic] {message}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Lunatic] Logger Error: {ex}");
                }
            }

            public static void Omit()
            {
                // stop logging
                ShouldLog = false;
            }
        }
    }
}