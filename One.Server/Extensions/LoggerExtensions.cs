namespace One.Server.Extensions;

/// <summary>Logger 扩展方法 - 自动添加时间前缀</summary>
public static class LoggerExtensions
{
    /// <summary>获取时间前缀</summary>
    private static string GetTimePrefix() => DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

    #region ILogger<T> 扩展方法 (推荐使用)

    /// <summary>LogDebug 重载 - 自动添加时间前缀</summary>
    public static void LogDebugWithTime<T>(this ILogger<T> logger, string message)
    {
        logger.LogDebug("[{Time}] {Message}", GetTimePrefix(), message);
    }

    /// <summary>LogDebug 重载 - 自动添加时间前缀 (支持结构化日志参数)</summary>
    public static void LogDebugWithTime<T>(this ILogger<T> logger, string message, params object?[] args)
    {
        logger.LogDebug("[{Time}] " + message, new object?[] { GetTimePrefix() }.Concat(args).ToArray());
    }

    /// <summary>LogWarning 重载 - 自动添加时间前缀</summary>
    public static void LogWarningWithTime<T>(this ILogger<T> logger, string message)
    {
        logger.LogWarning("[{Time}] {Message}", GetTimePrefix(), message);
    }

    /// <summary>LogWarning 重载 - 自动添加时间前缀 (支持结构化日志参数)</summary>
    public static void LogWarningWithTime<T>(this ILogger<T> logger, string message, params object?[] args)
    {
        logger.LogWarning("[{Time}] " + message, new object?[] { GetTimePrefix() }.Concat(args).ToArray());
    }

    /// <summary>LogWarning 重载 - 自动添加时间前缀 (异常日志)</summary>
    public static void LogWarningWithTime<T>(this ILogger<T> logger, Exception exception, string message)
    {
        logger.LogWarning(exception, "[{Time}] {Message}", GetTimePrefix(), message);
    }

    /// <summary>LogWarning 重载 - 自动添加时间前缀 (异常日志 + 结构化参数)</summary>
    public static void LogWarningWithTime<T>(this ILogger<T> logger, Exception exception, string message, params object?[] args)
    {
        logger.LogWarning(exception, "[{Time}] " + message, new object?[] { GetTimePrefix() }.Concat(args).ToArray());
    }

    /// <summary>LogInformation 重载 - 自动添加时间前缀</summary>
    public static void LogInformationWithTime<T>(this ILogger<T> logger, string message)
    {
        logger.LogInformation("[{Time}] {Message}", GetTimePrefix(), message);
    }

    /// <summary>LogInformation 重载 - 自动添加时间前缀 (支持结构化日志参数)</summary>
    public static void LogInformationWithTime<T>(this ILogger<T> logger, string message, params object?[] args)
    {
        logger.LogInformation("[{Time}] " + message, new object?[] { GetTimePrefix() }.Concat(args).ToArray());
    }

    /// <summary>LogError 重载 - 自动添加时间前缀</summary>
    public static void LogErrorWithTime<T>(this ILogger<T> logger, string message)
    {
        logger.LogError("[{Time}] {Message}", GetTimePrefix(), message);
    }

    /// <summary>LogError 重载 - 自动添加时间前缀 (支持结构化日志参数)</summary>
    public static void LogErrorWithTime<T>(this ILogger<T> logger, string message, params object?[] args)
    {
        logger.LogError("[{Time}] " + message, new object?[] { GetTimePrefix() }.Concat(args).ToArray());
    }

    /// <summary>LogError 重载 - 自动添加时间前缀 (异常日志)</summary>
    public static void LogErrorWithTime<T>(this ILogger<T> logger, Exception exception, string message)
    {
        logger.LogError(exception, "[{Time}] {Message}", GetTimePrefix(), message);
    }

    /// <summary>LogError 重载 - 自动添加时间前缀 (异常日志 + 结构化参数)</summary>
    public static void LogErrorWithTime<T>(this ILogger<T> logger, Exception exception, string message, params object?[] args)
    {
        logger.LogError(exception, "[{Time}] " + message, new object?[] { GetTimePrefix() }.Concat(args).ToArray());
    }

    #endregion ILogger<T> 扩展方法 (推荐使用)

    #region ILogger 扩展方法 (兼容使用)

    /// <summary>LogDebug 重载 - 自动添加时间前缀</summary>
    public static void LogDebugWithTime(this ILogger logger, string message)
    {
        logger.LogDebug("[{Time}] {Message}", GetTimePrefix(), message);
    }

    /// <summary>LogDebug 重载 - 自动添加时间前缀 (支持结构化日志参数)</summary>
    public static void LogDebugWithTime(this ILogger logger, string message, params object?[] args)
    {
        logger.LogDebug("[{Time}] " + message, new object?[] { GetTimePrefix() }.Concat(args).ToArray());
    }

    /// <summary>LogWarning 重载 - 自动添加时间前缀</summary>
    public static void LogWarningWithTime(this ILogger logger, string message)
    {
        logger.LogWarning("[{Time}] {Message}", GetTimePrefix(), message);
    }

    /// <summary>LogWarning 重载 - 自动添加时间前缀 (支持结构化日志参数)</summary>
    public static void LogWarningWithTime(this ILogger logger, string message, params object?[] args)
    {
        logger.LogWarning("[{Time}] " + message, new object?[] { GetTimePrefix() }.Concat(args).ToArray());
    }

    /// <summary>LogWarning 重载 - 自动添加时间前缀 (异常日志)</summary>
    public static void LogWarningWithTime(this ILogger logger, Exception exception, string message)
    {
        logger.LogWarning(exception, "[{Time}] {Message}", GetTimePrefix(), message);
    }

    /// <summary>LogWarning 重载 - 自动添加时间前缀 (异常日志 + 结构化参数)</summary>
    public static void LogWarningWithTime(this ILogger logger, Exception exception, string message, params object?[] args)
    {
        logger.LogWarning(exception, "[{Time}] " + message, new object?[] { GetTimePrefix() }.Concat(args).ToArray());
    }

    /// <summary>LogInformation 重载 - 自动添加时间前缀</summary>
    public static void LogInformationWithTime(this ILogger logger, string message)
    {
        logger.LogInformation("[{Time}] {Message}", GetTimePrefix(), message);
    }

    /// <summary>LogInformation 重载 - 自动添加时间前缀 (支持结构化日志参数)</summary>
    public static void LogInformationWithTime(this ILogger logger, string message, params object?[] args)
    {
        logger.LogInformation("[{Time}] " + message, new object?[] { GetTimePrefix() }.Concat(args).ToArray());
    }

    /// <summary>LogError 重载 - 自动添加时间前缀</summary>
    public static void LogErrorWithTime(this ILogger logger, string message)
    {
        logger.LogError("[{Time}] {Message}", GetTimePrefix(), message);
    }

    /// <summary>LogError 重载 - 自动添加时间前缀 (支持结构化日志参数)</summary>
    public static void LogErrorWithTime(this ILogger logger, string message, params object?[] args)
    {
        logger.LogError("[{Time}] " + message, new object?[] { GetTimePrefix() }.Concat(args).ToArray());
    }

    /// <summary>LogError 重载 - 自动添加时间前缀 (异常日志)</summary>
    public static void LogErrorWithTime(this ILogger logger, Exception exception, string message)
    {
        logger.LogError(exception, "[{Time}] {Message}", GetTimePrefix(), message);
    }

    /// <summary>LogError 重载 - 自动添加时间前缀 (异常日志 + 结构化参数)</summary>
    public static void LogErrorWithTime(this ILogger logger, Exception exception, string message, params object?[] args)
    {
        logger.LogError(exception, "[{Time}] " + message, new object?[] { GetTimePrefix() }.Concat(args).ToArray());
    }

    #endregion ILogger 扩展方法 (兼容使用)
}