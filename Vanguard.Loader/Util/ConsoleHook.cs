using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Vanguard.Loader.Util;

public class ConsoleHook : IDisposable
{
    // Create a new Console window and bind console.writeline to it

    public static StreamWriter ConsoleOutStream { get; private set; }

    public void Dispose()
    {
        ConsoleOutStream?.Dispose();
    }

    public static void Initialize()
    {
        /*
        if (!(Environment.GetCommandLineArgs().Length > 1 && Environment.GetCommandLineArgs()[1] == "--debug"))
        {
            return;
        }
*/
        AllocConsole();
        ConsoleOutStream = new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true };
        Console.SetOut(ConsoleOutStream);
    }

    [DllImport("kernel32")]
    private static extern bool AllocConsole();
}