using System;
using System.IO;
using Newtonsoft.Json;
using Lunatic.Public.Interfaces;

namespace Lunatic.Configuration
{
    public static class ConfigurationManager
    {
        private static readonly string ConfigDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs");

        public static T Load<T>(string moduleName) where T : IModuleConfiguration, new()
        {
            if (!Directory.Exists(ConfigDirectory))
                Directory.CreateDirectory(ConfigDirectory);

            string configPath = Path.Combine(ConfigDirectory, $"{moduleName}.json");

            if (!File.Exists(configPath))
            {
                var defaultConfig = new T();
                Save(moduleName, defaultConfig); // Save default
                return defaultConfig;
            }

            try
            {
                string json = File.ReadAllText(configPath);
                return JsonConvert.DeserializeObject<T>(json) ?? new T();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Lunatic.Config] Failed to load config for '{moduleName}': {ex.Message}");
                return new T();
            }
        }

        public static void Save<T>(string moduleName, T config) where T : IModuleConfiguration
        {
            try
            {
                string configPath = Path.Combine(ConfigDirectory, $"{moduleName}.json");
                string json = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(configPath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Lunatic.Config] Failed to save config for '{moduleName}': {ex.Message}");
            }
        }
    }
}