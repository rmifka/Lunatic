using System.Linq;
using Lunatic.Configuration;
using UnityEngine;
using Lunatic.Public.Interfaces;

namespace Lunatic.TestModule;

public class Module : IModule
{
    public static ILunaticLogger LunaticLogger;

    private ModConfigTest _config;

    public void Initialize(ILunaticLogger lunaticLogger)
    {
        _config = ConfigurationManager.Load<ModConfigTest>("TestModule");

        lunaticLogger.Debug("Hello from the test module!");
        LunaticLogger = lunaticLogger;
        LogConfigValues(_config);
        new GameObject("TestObject").AddComponent<TestComponent>();
    }

    private void LogConfigValues(ModConfigTest config)
    {
        foreach (var field in config.GetType().GetProperties())
        {
            var value = field.GetValue(config);
            LunaticLogger.Debug($"{field.Name}: {value}");
        }
    }

    private class ModConfigTest : IModuleConfiguration
    {
        public string Name { get; set; } = "TestModule";

        public string Version { get; set; } = "1.0.0";

        public string Author { get; set; } = "Renschi";

        public int TestValue { get; set; } = 42;
    }
}