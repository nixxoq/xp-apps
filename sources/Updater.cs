using System;
using System.Diagnostics;
using Newtonsoft.Json;
using xp_apps.sources.Structures;

namespace xp_apps.sources
{
    public abstract class Updater
    {
        public const string ProgramVersion = "0.4.0-dev3";

        // todo: search another ways to check for latest release version
        // P.S. I know only one - upload data.json on every release
        private const string LatestReleaseVersion = "http://data.nixxoq.xyz/xp-apps/data.json";
        public const string LatestReleaseZip = "https://github.com/nixxoq/xp-apps/releases/latest/download/xp_apps.zip";

        // application version
        private static string FetchLatestVersion()
        {
            if (!Helper.IsNetworkAvailable())
            {
                Console.WriteLine("Failed to fetch latest version. No internet connection.");
                return null;
            }

            var content = CurlWrapper.GetFileContent(LatestReleaseVersion);
            if (content != null) return content;

            Console.WriteLine($"Failed to fetch latest version.");
            return null;
        }

        public static bool? CheckForUpdates()
        {
            var content = FetchLatestVersion();
            if (content == null) return null;
            var jsonData = JsonConvert.DeserializeObject<UpdateData>(content);
            return !jsonData.Version?.Equals(ProgramVersion);
        }

        public static void Update()
        {
            if (CheckForUpdates() == null)
            {
                Console.WriteLine("Failed to fetch latest update. Please try again later.");
                return;
            }

            if (!Convert.ToBoolean(CheckForUpdates()))
            {
                var internetAvailable = Helper.IsNetworkAvailable();
                Console.WriteLine(!internetAvailable
                    ? "Failed to fetch latest update. No internet connection."
                    : "Application is already up-to-date.");
                return;
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = "Updater.exe",
                Arguments = string.Join(" ", Helper.GetCommandArgs()),
                UseShellExecute = true,
                CreateNoWindow = false,
            };

            var process = new Process();
            process.StartInfo = startInfo;

            Console.WriteLine("Starting Updater...");
            process.Start();
            Environment.Exit(0);
        }
    }
}