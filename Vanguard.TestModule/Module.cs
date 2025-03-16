using UnityEngine;
using Vanguard.Public.Interfaces;

namespace Vanguard.TestModule;

public class Module : IModule
{
    public static IVanguardLogger VanguardLogger;

    public void Initialize(IVanguardLogger vanguardLogger)
    {
        vanguardLogger.Debug("Hello from the test module!");
        VanguardLogger = vanguardLogger;
        new GameObject("TestObject").AddComponent<TestComponent>();
    }
}