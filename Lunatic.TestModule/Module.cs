using UnityEngine;
using Lunatic.Public.Interfaces;

namespace Lunatic.TestModule;

public class Module : IModule
{
    public static ILunaticLogger LunaticLogger;

    public void Initialize(ILunaticLogger lunaticLogger)
    {
        lunaticLogger.Debug("Hello from the test module!");
        LunaticLogger = lunaticLogger;
        new GameObject("TestObject").AddComponent<TestComponent>();
    }
}