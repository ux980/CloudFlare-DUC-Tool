using CF_DUC_Tool.src.Core;
using CF_DUC_Tool.src.Core.Exceptions;

namespace CF_DUC_Tool.src.Infrastructure
{

    public class ConsoleLogger : ILogger
    {
        private void Write(string prefix, string msg, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] [{prefix}] {msg}");
            Console.ResetColor();
        }
        public void LogInfo(string m) => Write("INFO", m, ConsoleColor.White);
        public void LogSuccess(string m) => Write("OK", m, ConsoleColor.Green);
        public void LogWarning(string m) => Write("WARN", m, ConsoleColor.Yellow);
        public void LogError(string m) => Write("ERR", m, ConsoleColor.Red);
        public void LogException(Exception ex) => HandleException(ex);

        public void HandleException(Exception ex)
        {
            if (ex is DdnsBaseException ddnsEx)
            {
                var color = ddnsEx.Severity == ErrorSeverity.Critical ? ConsoleColor.DarkRed : ConsoleColor.Red;
                Write(ddnsEx.Severity.ToString().ToUpper(), ddnsEx.Message, color);
            }
            else
            {
                Write("CRASH", ex.Message, ConsoleColor.DarkMagenta);
                Console.WriteLine(ex.StackTrace);
            }
        }
    }

    public class FileLogger : ILogger
    {
        private readonly string _path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app.log");
        private readonly object _lock = new();

        private void Write(string type, string msg)
        {
            lock (_lock) File.AppendAllText(_path, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{type}] {msg}{Environment.NewLine}");
        }
        public void LogInfo(string m) => Write("INFO", m);
        public void LogSuccess(string m) => Write("OK", m);
        public void LogWarning(string m) => Write("WARN", m);
        public void LogError(string m) => Write("ERR", m);
        public void LogException(Exception ex) => Write("EXCEPTION", ex.Message);
    }

    public class CompositeLogger : ILogger
    {
        private readonly ILogger[] _loggers;
        public CompositeLogger(params ILogger[] loggers) => _loggers = loggers;

        public void LogInfo(string m) => Array.ForEach(_loggers, l => l.LogInfo(m));
        public void LogSuccess(string m) => Array.ForEach(_loggers, l => l.LogSuccess(m));
        public void LogWarning(string m) => Array.ForEach(_loggers, l => l.LogWarning(m));
        public void LogError(string m) => Array.ForEach(_loggers, l => l.LogError(m));
        public void LogException(Exception ex) => Array.ForEach(_loggers, l => l.LogException(ex));
    }
}
