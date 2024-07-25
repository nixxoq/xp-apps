using System;
using System.Net;
using System.Threading;
using xp_apps.sources;

namespace xp_apps
{
    public static class Program
    {
        public static void Main()
        {
            SimpleLogger.SetupLog("xp-apps");

#if DEBUG
            SimpleLogger.Logger.Debug(
                $"Current architecture: {Helper.OsArchitecture} | Current OS: {Environment.OSVersion}");
            var args = Helper.GetCommandArgs()?.Length > 0
                ? string.Join(" ", Helper.GetCommandArgs())
                : "No additional arguments";
            SimpleLogger.Logger.Debug($"Used command-line arguments: {args}");
#endif
            
            // ServicePointManager.Expect100Continue = true;
            // ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
            
            if (Convert.ToBoolean(Updater.CheckForUpdates()))
            {
                Console.WriteLine(
                    "A new version of the program is available.\nIf you want to update, please run \"xp-apps --self-update\".");
                Thread.Sleep(2000);
            }
            // Cache.FetchLatestVersion();
            // Checks if current operating system is Windows XP (NT 5.1 & NT 5.2)
            // However, I am thinking about adding support for Windows Vista when the One-Core-API 4.1.0 will be released 👀
            // if (!Functions.IsWindowsXp())
            // {
            //     Console.WriteLine("This program works only on Windows XP.");
            //     Console.WriteLine("Press any key to exit...");
            //     Console.ReadLine();
            //     return;
            // }

            // Checks if .NET Framework 4.5 or newer is installed
            // Its need for TLS 1.2 protocol
            // if (!MainScreen.IsDotNet45OrNewer())
            // {
            //     Console.WriteLine(
            //         "This program works only with installed .NET Framework 4.0 and 4.5+\nMake sure you have installed the One-Core-API before installing .NET Framework 4.5+!");
            //     Console.WriteLine("Press any key to exit...");
            //     Console.ReadLine();
            //     return;
            // }

            MainScreen.ParseArgs();       
        }
    }
}