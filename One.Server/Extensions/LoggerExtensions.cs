using Microsoft.Extensions.Logging;

namespace One.Server.Extensions;

/// <summary>
///     Logger 扩展方法 - 自动添加时间前缀
/// </summary>
public static class LoggerExtensions
{
    private static readonly string TimePrefix = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

    /// <summary>     LogDebug 重载 - 自动添加时间前缀
    /// </summary>
    public static void LogDebugWithTime(this ILogger logger, string message)
    {
        logger.LogDebug("[{Time}] {Message}", TimePrefix, message);
    }

    /// <summary>
    ///     LogDebug 重载 - 自动添加时间前缀 (格式化字符串)
    /// </summary>
    public static void LogDebugWithTime(this ILogger logger, string message, params object?[] args)
    {
        logger.LogDebug("[{Time}] " + message, TimePrefix, args);
    }

    /// <summary>
    ///     LogWarning 重载 - 自动添加时间前缀
    /// </summary>
    public static void LogWarningWithTime(this ILogger logger, string message)
    {
        logger.LogWarning("[{Time}] {Message}", TimePrefix, message);
    }

    /// <summary>
    ///     LogWarning 重载 - 自动添加时间前缀 (格式化字符串)
    /// </summary>
    public static void LogWarningWithTime(this ILogger logger, string message, params object?[] args)
    {
        logger.LogWarning("[{Time}] " + message, TimePrefix, args);
    }

    /// <summary>
    ///     LogInformation 重载 - 自动添加时间前缀
    /// </summary>
    public static void LogInformationWithTime(this ILogger logger, string message)
    {
        logger.LogInformation("[{Time}] {Message}", TimePrefix, message);
    }

    /// <summary>
    ///     LogInformation 重载 - 自动添加时间前缀 (格式化字符串)
    /// </summary>
    public static void LogInformationWithTime(this ILogger logger, string message, params object?[] args)
    {
        logger.LogInformation("[{Time}] " + message, TimePrefix, args);
    }

    /// <summary>
    ///     LogError 重载 - 自动添加时间前缀
    /// </summary>
    public static void LogErrorWithTime(this ILogger logger, string message)
    {
        logger.LogError("[{Time}] {Message}", TimePrefix, message);
    }

    /// <summary>
    ///     LogError 重载 - 自动添加时间前缀 (格式化字符串)
    /// </summary>
    public static void LogErrorWithTime(this ILogger logger, string message, params object?[] args)
    {
        logger.LogError("[{Time}] " + message, TimePrefix, args);
    }
}