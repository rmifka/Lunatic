using System;
using System.IO;
using System.Reflection;

namespace Vanguard.Bootstrapper
{
    public static class Bootstrapper
    {
        public static void Init()
        {
            try
            {
                Console.WriteLine("[Vanguard] Bootstrapper Initialized!");

                var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                var modsDir = Path.Combine(baseDir, "Mods");
                var loaderPath = Path.Combine(modsDir, "Vanguard.Loader.dll");

                if (!File.Exists(loaderPath))
                {
                    Console.WriteLine("[Vanguard] Vanguard.Loader.dll not found at: " + loaderPath);
                    return;
                }

                var loaderAssembly = Assembly.LoadFrom(loaderPath);
                var entryType = loaderAssembly.GetType("Vanguard.Loader.Entry");
                var initMethod = entryType?.GetMethod("Initialize", BindingFlags.Public | BindingFlags.Static);

                if (initMethod != null)
                {
                    initMethod.Invoke(null, null);
                    Console.WriteLine("[Vanguard] Vanguard.Loader initialized!");
                }
                else
                {
                    Console.WriteLine("[Vanguard] Could not find Initialize method in Loader.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Vanguard] Bootstrapper Error: {ex}");
            }
        }
    }
}