using System;
using System.Diagnostics;
using System.Threading;
using xp_apps.sources;

namespace xp_apps
{
    public static class Program
    {
        public static void Main()
        {
            Logger.SetupLog("xp-apps");

#if DEBUG
            Logger.LogManager.Debug(
                $"Current architecture: {Helper.OsArchitecture} | Current OS: {Environment.OSVersion}");
            var args = Helper.GetCommandArgs()?.Length > 0
                ? string.Join(" ", Helper.GetCommandArgs())
                : "No additional arguments";
            Logger.LogManager.Debug($"Used command-line arguments: {args}");
#endif
            Console.CancelKeyPress += OnExit;

            if (Convert.ToBoolean(Updater.CheckForUpdates()))
            {
                Console.WriteLine(
                    "[Update]: A new version of the program is available.\n[Tip]: If you want to update, please run \"xp-apps --self-update\".");
                Thread.Sleep(2000);
            }

            // Funny moment: this program works on Linux too
            if (MainScreen.IsWindowsNt5)
            {
                Console.WriteLine("This program works only on Windows XP.");
                Console.WriteLine("Press any key to exit...");
                Console.ReadLine();
                return;
            }

            MainScreen.ParseArgs();
        }

        private static void OnExit(object sender, ConsoleCancelEventArgs e)
        {
            Console.WriteLine("\nctrl + c key detected, exiting...");

            Process.Start(new ProcessStartInfo
            {
                FileName = "taskkill",
                Arguments = "/f /im curl.exe",
                CreateNoWindow = true,
                UseShellExecute = false
            });

            Environment.Exit(0);
        }
    }
}