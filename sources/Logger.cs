using System;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace xp_apps.sources
{
    public abstract class SimpleLogger
    {
        public static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static void SetupLog()
        {
            var unixStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var timestamp = (long)(DateTime.Now.ToUniversalTime() - unixStart).TotalSeconds;


            var config = new LoggingConfiguration();
            var consoleTarget = new ConsoleTarget
            {
                Name = "console",
                Layout = "[${date}] [${level:uppercase=true}]\n  -> ${message}"
            };

            var fileTarget = new FileTarget
            {
                Name = "File",
                FileName = $"debug-{timestamp}.log",
                Layout = "[${date}] [${level:uppercase=true}]\n  -> ${message}"
            };
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, consoleTarget);
            config.AddRule(LogLevel.Debug, LogLevel.Debug, fileTarget);
            LogManager.Configuration = config;
        }
    }
}