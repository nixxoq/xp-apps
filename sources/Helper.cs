using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Threading;

namespace xp_apps.sources
{
    public abstract class Helper
    {
        ////////////////////////
        /// HELPER FUNCTIONS ///
        ////////////////////////
        public static readonly string OsArchitecture = Environment.Is64BitOperatingSystem ? "x64" : "x86";

        public static readonly string WorkDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static readonly string CurrentFile = Assembly.GetExecutingAssembly().GetName().Name;

        public static string ExtractFileNameFromUrl(string url) =>
            url.Substring(url.LastIndexOf('/') + 1);

        public const string ProgramVersion = "0.2.0-dev2";

        public static void CreateApplicationFolder(string folderPath = null)
        {
            if (folderPath == null) folderPath = Path.Combine(WorkDir, Cache.CacheFolder);

            if (Directory.Exists(folderPath)) return;

            try
            {
                Directory.CreateDirectory(folderPath);
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine($"Access denied to creating folder {folderPath}");
            }
            catch (PathTooLongException)
            {
                Console.WriteLine(
                    $"Path too long to create folder {folderPath}. Place xp-apps in a different folder with a shorter path.");
            }
        }

        ///////////////
        /// NETWORK ///
        ///////////////
        public static bool IsNetworkAvailable()
        {
            var ping = new Ping();
            var reply = ping.Send("www.google.com", 5000);

            return reply != null && reply.Status == IPStatus.Success;
        }

        public static WebClient GetClient()
        {
            var client = new WebClient();
            return client;
        }

        /// <summary>
        ///     Downloads a file from the specified URL and saves it with the given filename.
        ///     Displays a progress animation in the console while downloading.
        /// </summary>
        /// <param name="url">The URL of the file to download.</param>
        /// <param name="filename">The name of the file to save the downloaded content to.</param>
        public static void DownloadFile(string url, string filename)
        {
            using (var client = new WebClient())
            {
                char[] animationChars = { '/', '-', '\\', '|' };
                var animationIndex = 0;
                var stopwatch = new Stopwatch();

                client.DownloadProgressChanged += (sender, e) =>
                {
                    var speed = e.BytesReceived / 1024d / stopwatch.Elapsed.TotalSeconds;
                    var remainingBytes = e.TotalBytesToReceive - e.BytesReceived;
                    var remainingSeconds = remainingBytes / 1024d / speed;

                    var remainingTime = TimeSpan.FromSeconds(remainingSeconds);
                    var animationChar = animationChars[animationIndex++ % animationChars.Length];

                    Console.Write(
                        $"\r{animationChar} " +
                        $@"Downloading {filename} | {e.ProgressPercentage}% completed | {speed / 1024d:0.00} MB/s | {remainingTime:hh\:mm\:ss} remaining"
                    );
                };

                client.DownloadFileCompleted += (sender, e) =>
                {
                    Thread.Sleep(1000);
                    Console.WriteLine(e.Error != null
                        ? $"\nError: {e.Error.Message}"
                        : $"\n{filename} download completed.\n");
                };
                stopwatch.Start();
                client.DownloadFileAsync(new Uri(url), filename);

                while (client.IsBusy) Thread.Sleep(100);

                stopwatch.Stop();
            }
        }

        public static string[] GetCommandArgs()
        {
            return Environment.GetCommandLineArgs().Skip(1).ToArray();
        }
    }
}