using System;
using System.Linq;

namespace QueueDisplay.Services;

public static class QueueAppSettings
{
    public static string TellerId =>
        GetValue("teller", "TELLER_ID", "AUTO");

    public static string ApiBaseUrl =>
        GetValue("api", "QUEUE_API_BASE_URL", "http://192.168.88.6:5092/");

    public static string SocketHost =>
        GetValue("socket", "QUEUE_SOCKET_HOST", "192.168.88.6");

    public static int SocketPort
    {
        get
        {
            string value =
                GetValue("socketPort", "QUEUE_SOCKET_PORT", "5000");

            return int.TryParse(value, out int port)
                ? port
                : 5000;
        }
    }

    private static string GetValue(
        string argumentName,
        string environmentName,
        string fallback)
    {
        string? argumentValue =
            Environment.GetCommandLineArgs()
                .Skip(1)
                .Select(arg => arg.Split('=', 2))
                .Where(parts => parts.Length == 2)
                .Where(parts => parts[0].Equals(
                    "--" + argumentName,
                    StringComparison.OrdinalIgnoreCase))
                .Select(parts => parts[1])
                .FirstOrDefault();

        if (!string.IsNullOrWhiteSpace(argumentValue))
            return argumentValue;

        string? environmentValue =
            Environment.GetEnvironmentVariable(environmentName);

        if (!string.IsNullOrWhiteSpace(environmentValue))
            return environmentValue;

        return fallback;
    }
}
