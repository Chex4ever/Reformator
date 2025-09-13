using Microsoft.Extensions.Logging;

namespace EmployeeSalaryProcessor.Core.Utilities;

//TODO: All logs from "Application" - :/
public static class AppLogger
{
    private static ILogger? _logger;

    public static void Initialize(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger("Application");
    }

    public static void LogDebug(string message) => _logger?.LogDebug(message);
    public static void LogInformation(string message) => _logger?.LogInformation(message);
    public static void LogWarning(string message) => _logger?.LogWarning(message);
    public static void LogError(string message, Exception? ex = null) => _logger?.LogError(ex, message);
}