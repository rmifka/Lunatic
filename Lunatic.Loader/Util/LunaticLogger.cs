using System;
using System.Diagnostics;
using System.IO;
using Lunatic.Public.Interfaces;

namespace Lunatic.Loader.Util;

public class LunaticLogger : IDisposable, ILunaticLogger
{
    private readonly static string DirectoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Lunatic_Logs");
    private readonly static string LogFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Lunatic_Logs", "LunaticLoader.vlog");

    private readonly StreamWriter _fileWriter;

    public LunaticLogger()
    {
        if (!Directory.Exists(DirectoryPath))
        {
            Directory.CreateDirectory(DirectoryPath);
        }

        if (!File.Exists(LogFilePath))
        {
            File.Create(LogFilePath).Close();
        }

        ConsoleHook.Initialize();
        _fileWriter = new StreamWriter(LogFilePath, true) { AutoFlush = true };
    }

    public void Dispose()
    {
        _fileWriter?.Dispose();
    }

    public void Log(string message)
    {
        LogInternal(message, "INFO", ConsoleColor.White);
    }

    public void Critical(string message)
    {
        LogInternal(message, "CRITICAL", ConsoleColor.DarkMagenta);
    }

    public void Error(string message)
    {
        LogInternal(message, "ERROR", ConsoleColor.Red);
    }

    public void Warning(string message)
    {
        LogInternal(message, "WARNING", ConsoleColor.Yellow);
    }

    public void Info(string message)
    {
        LogInternal(message, "INFO", ConsoleColor.White);
    }

    public void Debug(string message)
    {
        LogInternal(message, "DEBUG", ConsoleColor.Cyan);
    }

    public void Trace(string message)
    {
        LogInternal(message, "TRACE", ConsoleColor.Gray);
    }

    public void Log(object obj)
    {
        Log(obj.ToString());
    }

    public void Critical(object obj)
    {
        Critical(obj.ToString());
    }

    public void Error(object obj)
    {
        Error(obj.ToString());
    }

    public void Warning(object obj)
    {
        Warning(obj.ToString());
    }

    public void Info(object obj)
    {
        Info(obj.ToString());
    }

    public void Debug(object obj)
    {
        Debug(obj.ToString());
    }

    public void Trace(object obj)
    {
        Trace(obj.ToString());
    }

    public void LogException(Exception ex)
    {
        Error(ex.Message);
        Error(ex.StackTrace);
    }

    private static string GetCaller()
    {
        var stackTrace = new StackTrace();
        var methodBase = stackTrace.GetFrame(3).GetMethod();
        return $"{methodBase.ReflectedType?.Name}.{methodBase.Name}";
    }

    private void LogInternal(string message, string logType, ConsoleColor color)
    {
        var caller = GetCaller();
        var logMessage = $"[{DateTime.Now:HH:mm:ss}|{logType}|{caller}] {message}";
        Console.ForegroundColor = color;
        Console.WriteLine(logMessage);
        Console.ResetColor();
        _fileWriter.WriteLine(logMessage);
    }
}