using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Vanguard.Loader.Util;

public class ConsoleHook : IDisposable
{
    // Create a new Console window and bind console.writeline to it

    public static StreamWriter ConsoleOutStream { get; private set; }

    public static void Initialize()
    {
        AllocConsole();

        ConsoleOutStream = new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true };
        Console.SetOut(ConsoleOutStream);
    }

    [DllImport("kernel32")]
    private static extern bool AllocConsole();

    public void Dispose()
    {
        ConsoleOutStream?.Dispose();
    }
}