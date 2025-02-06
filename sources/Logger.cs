using System;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace xp_apps.sources
{
    public abstract class Logger
    {
        public static readonly NLog.Logger LogManager = NLog.LogManager.GetCurrentClassLogger();

        public static void SetupLog(string appName)
        {
            var unixStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var timestamp = (long)(DateTime.Now.ToUniversalTime() - unixStart).TotalSeconds;


            var config = new LoggingConfiguration();
            var consoleTarget = new ConsoleTarget
            {
                Name = "console",
                Layout = $"[{appName}] [${{date}}] [${{level:uppercase=true}}]\n  -> ${{message}}"
            };

            var fileTarget = new FileTarget
            {
                Name = "File",
                FileName = $"debug-{appName}-{timestamp}.log",
                Layout = "[${date}] [${level:uppercase=true}]\n  -> ${message}"
            };
            config.AddRule(LogLevel.Debug, LogLevel.Debug, consoleTarget);
            config.AddRule(LogLevel.Debug, LogLevel.Info, fileTarget);
            NLog.LogManager.Configuration = config;
        }
    }
}