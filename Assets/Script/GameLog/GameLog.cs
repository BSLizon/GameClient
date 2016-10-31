using UnityEngine;
using System.Collections;

public enum LogLevel
{
    DEBUG,
    INFO,
    WARN,
    ERROR,
    NONE
}

public class GameLog
{
    public static LogLevel level = LogLevel.DEBUG;

    public static void Debug(object message)
    {
        if (LogLevel.DEBUG >= level)
        {
            UnityEngine.Debug.Log(message);
        }
    }

    public static void Info(object message)
    {
        if (LogLevel.INFO >= level)
        {
            UnityEngine.Debug.Log(message);
        }
    }

    public static void Warn(object message)
    {
        if (LogLevel.WARN >= level)
        {
            UnityEngine.Debug.LogWarning(message);
        }
    }

    public static void Error(object message)
    {
        if (LogLevel.ERROR >= level)
        {
            UnityEngine.Debug.LogError(message);
        }
    }
}
