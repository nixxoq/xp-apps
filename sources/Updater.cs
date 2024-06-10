using System;
using System.Diagnostics;
using System.IO;
using System.Net;

namespace xp_apps.sources
{
    public abstract class Updater
    {
        // application version
        static string FetchLatestVersion()
        {
            if (!Functions.IsNetworkAvailable())
            {
                Console.WriteLine("Failed to fetch latest version. No internet connection.");
                return null;
            }

            WebClient client = Functions.GetClient();

            try
            {
                Stream stream = client.OpenRead(Constants.LatestReleaseVersion);
                if (stream != null)
                {
                    string content = new StreamReader(stream).ReadToEnd();
                    return content;
                }
            }
            catch (WebException e)
            {
                Console.WriteLine($"Failed to fetch latest version. {e.Message}");
                return null;
            }

            return null;
        }

        public static bool? CheckForUpdates()
        {
            string content = FetchLatestVersion();

            return !content?.Equals(Constants.ProgramVersion);

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
                bool internetAvailable = Functions.IsNetworkAvailable();
                Console.WriteLine(!internetAvailable ? "Failed to fetch latest update. No internet connection." : "Application is already up-to-date.");
                return;
            }


            Process.Start("Updater.exe");
            Environment.Exit(0);
        }
    }
}
