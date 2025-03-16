using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Vanguard.Bootstrapper
{
    public static class Bootstrapper
    {
        private readonly static string LibraryDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Libraries");

        private static void LoadHarmony()
        {
            var libFiles = Directory.GetFiles(LibraryDirectory, "0Harmony*.dll");

            foreach (var libPath in libFiles)
            {
                try
                {
                    Assembly.LoadFrom(libPath);
                    VanguardBootstrapperLogger.Log($"Loaded library: {libPath}");
                }
                catch (Exception ex)
                {
                    VanguardBootstrapperLogger.Log($"Failed to load library: {libPath} - {ex}");
                }
            }
        }

        public static void Init()
        {
            const string vanguardLoader = "Vanguard.Loader";
            const string vanguardPublic = "Vanguard.Public";

            if (AppDomain.CurrentDomain.GetAssemblies().Any(a => a.FullName.Contains(vanguardLoader)))
            {
                VanguardBootstrapperLogger.Log("Bootstrapper already initialized!");
                VanguardBootstrapperLogger.Omit();
                return;
            }

            if (!Directory.Exists("Vanguard_Logs"))
            {
                Directory.CreateDirectory("Vanguard_Logs");
            }

            try
            {
                LoadHarmony();
                VanguardBootstrapperLogger.Log("Bootstrapper Initialized!");

                var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                var dataFolders = Directory.GetDirectories(baseDir, "*_Data", SearchOption.TopDirectoryOnly);
                var managedPath = Path.Combine(dataFolders[0], "Managed");

                var publicPath = Path.Combine(managedPath, $"{vanguardPublic}.dll");
                Assembly.LoadFrom(publicPath);
                VanguardBootstrapperLogger.Log($"{vanguardPublic} loaded!");

                var loaderPath = Path.Combine(managedPath, $"{vanguardLoader}.dll");

                if (!File.Exists(loaderPath))
                {
                    VanguardBootstrapperLogger.Log($"{vanguardLoader}.dll not found at: " + loaderPath);
                    return;
                }

                var loaderAssembly = Assembly.LoadFrom(loaderPath);
                var entryType = loaderAssembly.GetType($"{vanguardLoader}.EntryPoint");
                var initMethod = entryType?.GetMethod("Initialize", BindingFlags.Public | BindingFlags.Static);

                if (initMethod != null)
                {
                    initMethod.Invoke(null, null);
                    VanguardBootstrapperLogger.Log($"{vanguardLoader} initialized!");
                }
                else
                {
                    VanguardBootstrapperLogger.Log($"Could not find Initialize method in {vanguardLoader}.");
                }
            }
            catch (Exception ex)
            {
                VanguardBootstrapperLogger.Log($"Bootstrapper Error: {ex}");
            }
        }

        public static class VanguardBootstrapperLogger
        {
            private readonly static string LogFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Vanguard_Logs",
                $"VanguardBootstrapper.{DateTime.Now:yyyy-MM-dd}.vlog");

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
                        Console.WriteLine($"[Vanguard] {message}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Vanguard] Logger Error: {ex}");
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