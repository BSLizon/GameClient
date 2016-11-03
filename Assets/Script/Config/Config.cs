public class Config
{
    public static LogLevel logLevel = LogLevel.DEBUG;

    public static string serverIP = "127.0.0.1";
    public static int serverPort = 8080;

    public static int socketSendTimeout = 4000; //ms
    public static int socketCloseTimeout = 10; //ms
    public static int socketConnectTimeout = 5000; //ms
}
