using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Vanguard.Bootstrapper
{
    public static class Bootstrapper
    {
        public static void Init()
        {
            try
            {
                // If already loaded skip
                if (AppDomain.CurrentDomain.GetAssemblies().Any(a => a.GetName().Name == "Vanguard.Loader"))
                {
                    return;
                }

                VanguardBootstrapperLogger.Log("Bootstrapper Initialized!");

                var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                var modsDir = Path.Combine(baseDir, "Mods");
                var loaderPath = Path.Combine(modsDir, "Vanguard.Loader.dll");

                if (!File.Exists(loaderPath))
                {
                    VanguardBootstrapperLogger.Log("Vanguard.Loader.dll not found at: " + loaderPath);
                    return;
                }

                var loaderAssembly = Assembly.LoadFrom(loaderPath);
                var entryType = loaderAssembly.GetType("Vanguard.Loader.EntryPoint");
                var initMethod = entryType?.GetMethod("Initialize", BindingFlags.Public | BindingFlags.Static);

                if (initMethod != null)
                {
                    initMethod.Invoke(null, null);
                    VanguardBootstrapperLogger.Log("Vanguard.Loader initialized!");
                }
                else
                {
                    VanguardBootstrapperLogger.Log("Could not find Initialize method in Loader.");
                }
            }
            catch (Exception ex)
            {
                VanguardBootstrapperLogger.Log($"Bootstrapper Error: {ex}");
            }
        }

        public static class VanguardBootstrapperLogger
        {
            private readonly static string LogFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"Vanguard_Logs", "VanguardBootstrapper.vlog");

            public static void Log(string message)
            {
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
        }
    }
}