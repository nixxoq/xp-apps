using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using xp_apps.sources.Structures;

namespace xp_apps.sources
{
    public abstract class Updater
    {
        public const string ProgramVersion = "0.4.0-dev2";

        private const string LatestReleaseVersion = "http://data.nixxoq.xyz/xp-apps/data.json";
        public const string LatestReleaseZip = "https://github.com/nixxoq/xp-apps/releases/latest/download/xp-apps.zip";

        // application version
        private static string FetchLatestVersion()
        {
            if (!Helper.IsNetworkAvailable())
            {
                Console.WriteLine("Failed to fetch latest version. No internet connection.");
                return null;
            }

#warning TODO: Reimplement using CurlWrapper ?
            using (var client = Helper.GetClient())
            {
                try
                {
                    var stream = client.OpenRead(LatestReleaseVersion);
                    if (stream != null)
                    {
                        var content = new StreamReader(stream).ReadToEnd();
                        return content;
                    }
                }
                catch (WebException e)
                {
                    Console.WriteLine($"Failed to fetch latest version. {e.Message}");
                    return null;
                }
            }

            return null;
        }

        public static bool? CheckForUpdates()
        {
            var content = FetchLatestVersion();
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

            Process.Start("Updater.exe");
            Environment.Exit(0);
        }
    }
}