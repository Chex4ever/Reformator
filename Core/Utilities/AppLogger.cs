using Microsoft.Extensions.Logging;

namespace EmployeeSalaryProcessor.Core.Utilities;

public static class AppLogger
{
    private static ILoggerFactory? _loggerFactory;

    public static void Initialize(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
    }

    public static void LogDebug(object source, string message)
    {
        var className = source.GetType().Name;
        var logger = _loggerFactory?.CreateLogger(className);
        logger?.LogDebug(message);
    }
    public static void LogInfo(object source, string message)
    {
        var className = source.GetType().Name;
        var logger = _loggerFactory?.CreateLogger(className);
        logger?.LogInformation(message);
    }
    public static void LogWarning(object source, string message, Exception? ex = null)
    {
        var className = source.GetType().Name;
        var logger = _loggerFactory?.CreateLogger(className);
        logger?.LogWarning(ex, message);
    }
    public static void LogError(object source, string message, Exception? ex = null)
    {
        var className = source.GetType().Name;
        var logger = _loggerFactory?.CreateLogger(className);
        logger?.LogError(ex, message);
    }
}