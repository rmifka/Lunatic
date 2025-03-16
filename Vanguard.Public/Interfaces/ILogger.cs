using System;

namespace Vanguard.Public.Interfaces;

public interface ILogger
{
    void Log(string message);
    void Critical(string message);
    void Error(string message);
    void Warning(string message);
    void Info(string message);
    void Debug(string message);
    void Trace(string message);

    void Log(object obj);
    void Critical(object obj);
    void Error(object obj);
    void Warning(object obj);
    void Info(object obj);
    void Debug(object obj);
    void Trace(object obj);

    void LogException(Exception ex);
}