public enum LogLevel
{
    DEBUG,
    INFO,
    WARN,
    ERROR,
    NONE
}

public class Log
{
    public static void Debug(object message)
    {
        if (LogLevel.DEBUG >= Config.logLevel)
        {
            UnityEngine.Debug.Log(message);
        }
    }

    public static void Info(object message)
    {
        if (LogLevel.INFO >= Config.logLevel)
        {
            UnityEngine.Debug.Log(message);
        }
    }

    public static void Warn(object message)
    {
        if (LogLevel.WARN >= Config.logLevel)
        {
            UnityEngine.Debug.LogWarning(message);
        }
    }

    public static void Error(object message)
    {
        if (LogLevel.ERROR >= Config.logLevel)
        {
            UnityEngine.Debug.LogError(message);
        }
    }
}
