using System.Text;

namespace ValorantEssentials.Utilities
{
    public enum LogLevel
    {
        Debug,
        Info,
        Success,
        Warning,
        Error,
        Action
    }

    public interface ILogger
    {
        event EventHandler<LogEventArgs>? LogAdded;
        void Log(string message, LogLevel level = LogLevel.Info);
        void LogDebug(string message);
        void LogInfo(string message);
        void LogSuccess(string message);
        void LogWarning(string message);
        void LogError(string message);
        void LogAction(string message);
        string GetLogContent();
        void Clear();
    }

    public class Logger : ILogger
    {
        private readonly StringBuilder _logBuilder = new();
        private readonly object _lock = new();

        public event EventHandler<LogEventArgs>? LogAdded;

        public void Log(string message, LogLevel level = LogLevel.Info)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            var logEntry = $"{timestamp} - {message}";

            lock (_lock)
            {
                _logBuilder.AppendLine(logEntry);
            }

            LogAdded?.Invoke(this, new LogEventArgs(message, level, timestamp));
        }

        public void LogDebug(string message) => Log(message, LogLevel.Debug);
        public void LogInfo(string message) => Log(message, LogLevel.Info);
        public void LogSuccess(string message) => Log(message, LogLevel.Success);
        public void LogWarning(string message) => Log(message, LogLevel.Warning);
        public void LogError(string message) => Log(message, LogLevel.Error);
        public void LogAction(string message) => Log(message, LogLevel.Action);

        public string GetLogContent()
        {
            lock (_lock)
            {
                return _logBuilder.ToString();
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _logBuilder.Clear();
            }
        }
    }

    public class LogEventArgs : EventArgs
    {
        public string Message { get; }
        public LogLevel Level { get; }
        public string Timestamp { get; }

        public LogEventArgs(string message, LogLevel level, string timestamp)
        {
            Message = message;
            Level = level;
            Timestamp = timestamp;
        }
    }

    public static class LoggerExtensions
    {
        public static void LogException(this ILogger logger, Exception ex, string? context = null)
        {
            var message = string.IsNullOrEmpty(context) 
                ? $"Exception: {ex.Message}" 
                : $"Exception in {context}: {ex.Message}";
            
            logger.LogError(message);
            
            if (ex.InnerException != null)
            {
                logger.LogError($"Inner exception: {ex.InnerException.Message}");
            }
        }

        public static void LogOperation(this ILogger logger, string operation, bool success, string? details = null)
        {
            var message = success ? $"{operation} completed successfully" : $"{operation} failed";
            if (!string.IsNullOrEmpty(details))
            {
                message += $": {details}";
            }
            
            if (success)
                logger.LogSuccess(message);
            else
                logger.LogError(message);
        }

        public static void LogPerformance(this ILogger logger, string operation, TimeSpan duration)
        {
            logger.LogInfo($"{operation} completed in {duration.TotalMilliseconds:F0}ms");
        }
    }
}