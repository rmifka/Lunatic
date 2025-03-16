using Vanguard.Public.Interfaces;
using UnityEngine;

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

public class TestComponent : MonoBehaviour
{
    private void Start()
    {
        DontDestroyOnLoad(this);
        Module.VanguardLogger.Debug("Hello from the test component!");
    }
}