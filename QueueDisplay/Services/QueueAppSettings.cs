using System;
using System.Linq;

namespace QueueDisplay.Services;

/// <summary>
/// Teller app-ийн API, socket, teller тохиргоог command line argument болон environment variable-аас уншина.
/// </summary>
public static class QueueAppSettings
{
    /// <summary>
    /// Энэ компьютерийн teller дугаар. AUTO үед socket server автоматаар teller онооно.
    /// </summary>
    public static string TellerId =>
        GetValue("teller", "TELLER_ID", "AUTO");

    /// <summary>
    /// Bank API серверийн үндсэн URL.
    /// </summary>
    public static string ApiBaseUrl =>
        GetValue("api", "QUEUE_API_BASE_URL", "http://192.168.88.6:5092/");

    /// <summary>
    /// Queue display socket server-ийн host хаяг.
    /// </summary>
    public static string SocketHost =>
        GetValue("socket", "QUEUE_SOCKET_HOST", "192.168.88.6");

    /// <summary>
    /// Queue display socket server-ийн port.
    /// </summary>
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

    /// <summary>
    /// Эхлээд command line argument, дараа нь environment variable, эцэст нь default утгаас тохиргоо уншина.
    /// </summary>
    /// <param name="argumentName">Command line argument-ийн нэр.</param>
    /// <param name="environmentName">Environment variable-ийн нэр.</param>
    /// <param name="fallback">Утга олдохгүй үед ашиглах default.</param>
    /// <returns>Олдсон тохиргооны утга.</returns>
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
