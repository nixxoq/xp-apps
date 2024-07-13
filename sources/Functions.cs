using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace xp_apps.sources
{
    internal static class Functions
    {
        private static readonly string Help = $"xp-apps ver {Constants.ProgramVersion}" +
                                              "\n\nList of available arguments:\n\n[Option]\t\t\t\t[Description]" +
                                              "\n-h, --help\t\t\t\tDisplay this help message" +
                                              "\n-i, --install\t\t\t\tInstall Application from XP-Apps repository" +
                                              "\n-l, --list, --list-applications,\tList all available applications in the repository \n--list-apps or --apps" +
                                              "\n\nExample:\n    xp-apps.exe -i PyCharm2023 or xp-apps.exe --install PyCharm2023";

        public static string[] GetCommandArgs()
        {
            return Environment.GetCommandLineArgs().Skip(1).ToArray();
        }

        /// <summary>
        ///     Parse arguments from command line
        /// </summary>
        public static void ParseArgs()
        {
            var args = GetCommandArgs();

            if (args.Length == 0)
            {
                Console.WriteLine(Help);
                return;
            }

            for (var i = 0; i < args.Length; i++)
            {
                var arg = args[i];

                switch (arg)
                {
                    case "-i":
                    case "--install":
                    {
                        if (i + 1 < args.Length)
                        {
                            var appName = args[i + 1];
                            var force = i + 2 < args.Length && args[i + 2].Equals("--force");
                            InstallApplication(appName, force);
                        }
                        else
                        {
                            Console.WriteLine("Error: Missing application name for install.");
                        }

                        return;
                    }
                    case "-h":
                    case "--help":
                        Console.WriteLine(Help);
                        return;
                    case "-l":
                    case "--list":
                    case "--list-applications":
                    case "--list-apps":
                    case "--apps":
                        GetApplications(Constants.ProgramsList);
                        return;
                    case "--self-update":
                        Updater.Update();
                        return;
                }
            }
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

        public static bool IsNetworkAvailable()
        {
            var ping = new Ping();
            var reply = ping.Send("www.google.com", 5000);

            return reply != null && reply.Status == IPStatus.Success;
        }

        /// <summary>
        ///     Retrieves the applications list updates from the server and deserializes them into an Applications object.
        /// </summary>
        /// <returns>The deserialized Applications object containing the updates.</returns>
        public static Applications GetUpdates()
        {
            if (!IsNetworkAvailable())
            {
                if (!Cache.IsAppsListExist())
                {
                    Console.WriteLine("No internet connection. Please check your internet connection and try again.");
                    Environment.Exit(0);
                }
                else
                {
                    Console.WriteLine("No internet connection. Using cached application list.");
                    return JsonConvert.DeserializeObject<Applications>(File.ReadAllText(Cache.ApplicationsListPath));
                }
            }

            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;

            var jsonContent = Cache.IsNeedUpdate()
                ? Cache.UpdateApplicationList(true)
                : File.ReadAllText(Cache.ApplicationsListPath);
            return JsonConvert.DeserializeObject<Applications>(jsonContent);
        }


        /// <summary>
        ///     Finds the application details based on the provided application name.
        /// </summary>
        /// <param name="appName">The name of the application to find.</param>
        /// <returns>The details of the application if found, otherwise null.</returns>
        private static JObject FindApplication(string appName)
        {
            return Constants.Categories
                .Select(
                    category =>
                        FindApplicationInCategory(appName, category.Value, category.Key)
                )
                .FirstOrDefault(result => result != null);
        }

        /// <summary>
        ///     Finds the application details in a specific category.
        /// </summary>
        /// <param name="appName">The name of the application to find.</param>
        /// <param name="category">The category to search in.</param>
        /// <param name="categoryName">The name of the category for debug output.</param>
        /// <returns>The details of the application if found, otherwise null.</returns>
        private static JObject FindApplicationInCategory(string appName, List<ProgramContainer> category,
            string categoryName)
        {
#if DEBUG
            SimpleLogger.Logger.Debug($"Searching {appName} in {categoryName} category...");
            // Console.WriteLine($"[DEBUG] Searching {appName} in {categoryName} category...");
#endif

            foreach (var (programName, programDetails) in Constants.GetProgramDetails(category))
            {
                // ReSharper disable once PossibleNullReferenceException
                var architecture = programDetails.GetValue("architecture").ToString();

                if (programName.Equals(appName, StringComparison.OrdinalIgnoreCase) &&
                    (architecture.Equals("any", StringComparison.OrdinalIgnoreCase) ||
                     architecture.Equals(Constants.OsArchitecture, StringComparison.OrdinalIgnoreCase)))
                    return programDetails;

                // ReSharper disable once PossibleNullReferenceException
                if (programDetails.GetValue("aliases").ToObject<string[]>().Any(
                        alias => alias.Equals(appName, StringComparison.OrdinalIgnoreCase) &&
                                 (architecture.Equals("any", StringComparison.OrdinalIgnoreCase) ||
                                  architecture.Equals(Constants.OsArchitecture, StringComparison.OrdinalIgnoreCase))
                    ))
                    return programDetails;
            }

            return null;
        }

        /// <summary>
        ///     Downloads a file from the specified URL and saves it with the given filename.
        ///     Displays a progress animation in the console while downloading.
        /// </summary>
        /// <param name="appName">Application name to install</param>
        /// <param name="isForce">Force download if file already exists</param>
        private static void InstallApplication(string appName, bool isForce)
        {
            var application = FindApplication(appName);
            if (application == null)
            {
                Console.Write($"Could not find application {appName}.");

                // check if similar applications exist
                var similarApps = FindSimilarApplications(appName);
                if (similarApps.Any())
                    Console.WriteLine($" Did you mean: {string.Join(", ", similarApps.Distinct())}?");

                return;
            }

            var filename = application.GetValue("filename")?.ToString();
            var url = application.GetValue("url")?.ToString();

            Console.WriteLine($"Found application {appName}");

            var client = GetClient();
            client.OpenRead(url ?? string.Empty);
            var filesize = Convert.ToInt64(client.ResponseHeaders.Get("Content-Length"));

            if (File.Exists(filename) && isForce)
                if (filename != null)
                    File.Delete(filename);


            // check if downloaded file is not corrupted
            if (filename != null && File.Exists(filename) && new FileInfo(filename).Length == filesize)
            {
                Console.WriteLine(
                    "File already exists and is not corrupted. Skipping download." +
                    $"\nIf you want force download, use 'xp-apps.exe -i {appName} --force'"
                );
                return;
            }

            Console.WriteLine($"Downloading file {filename}...");
            DownloadFile(url, filename);
        }

        /// <summary>
        ///     Finds similar applications based on the provided application name.
        /// </summary>
        /// <param name="appName">The name of the application to find.</param>
        /// <returns>The list of similar applications if found, otherwise null.</returns>
        private static List<string> FindSimilarApplications(string appName)
        {
            var allApps = Constants.Categories
                .SelectMany(c => Constants.GetProgramDetails(c.Value))
                .SelectMany(p => new[] { p.ProgramName }.Concat(p.ProgramDetails["aliases"].ToObject<string[]>()))
                .ToList();

            var threshold = Math.Max(appName.Length / 2, 2);

            return allApps
                .Where(
                    name => LevenshteinDistance(appName, name) <= threshold ||
                            name.IndexOf(appName, StringComparison.OrdinalIgnoreCase) >= 0
                )
                .Distinct()
                .OrderBy(name => LevenshteinDistance(appName, name))
                .ThenBy(name => name)
                .Take(5)
                .ToList();
        }

        /// <summary>
        ///     Calculates the Levenshtein distance between two strings.
        /// </summary>
        private static int LevenshteinDistance(string s, string t)
        {
            // If one of the strings is empty or null, return the length of the other string as Levenshtein distance.
            if (string.IsNullOrEmpty(s)) return t?.Length ?? 0;
            if (string.IsNullOrEmpty(t)) return s.Length;

            int n = s.Length, m = t.Length;

            // Create a two- dimensional array d with size (n+1) x (m+1), where d[i, j] represents the Levenshtein distance between
            // the first i characters of string s and the first j characters of string t.
            var d = new int[n + 1, m + 1];

            // Fill the first row and the first column of the array d with values from 0 to the lengths of strings s and t respectively.
            for (var i = 0; i <= n; i++) d[i, 0] = i;
            for (var j = 0; j <= m; j++) d[0, j] = j;

            // Iterate through the remaining elements of array d and compute the Levenshtein distance between all prefixes of strings s and t.
            for (var i = 1; i <= n; i++)
            for (var j = 1; j <= m; j++)
                d[i, j] = Math.Min(
                    Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                    d[i - 1, j - 1] + (s[i - 1] == t[j - 1] ? 0 : 1)
                );

            return d[n, m];
        }

        /// <summary>
        ///     Get all applications available to install
        /// </summary>
        /// <param name="json">Applications list (json object)</param>
        private static void GetApplications(Applications json)
        {
            Console.WriteLine("List of available applications:\nFormat:\n[Category]\n  [Application Name]\n");

            // Get all applications from Browsers category
            Console.WriteLine("Browsers:");
            foreach (var (programName, programDetails) in Constants.GetProgramDetails(json.Browsers))
                if (!Convert.ToBoolean(programDetails.GetValue("aliases")))
                {
                    Console.WriteLine($"  {programName}");
                }
                else
                {
                    var aliases = programDetails.GetValue("aliases");
                    if (aliases != null)
                        Console.WriteLine(
                            $"  {programName} | aliases: " +
                            $"{string.Join(", ", aliases.ToObject<string[]>())}");
                }

            // Get all applications from Vista native applications category
            Console.WriteLine("Vista native applications:");
            foreach (var (programName, programDetails) in Constants.GetProgramDetails(json.VistaApplications))
                if (!Convert.ToBoolean(programDetails.GetValue("aliases")))
                {
                    Console.WriteLine($"  {programName}");
                }
                else
                {
                    var aliases = programDetails.GetValue("aliases");
                    if (aliases != null)
                        Console.WriteLine(
                            $"  {programName} | aliases: " +
                            $"{string.Join(", ", aliases.ToObject<string[]>())}");
                }
        }

        /// <summary>
        ///     Checks if the current Windows version is Windows XP
        /// </summary>
        public static bool IsWindowsXp()
        {
            var os = Environment.OSVersion;
            var osv = os.Version;

            if (os.Platform != PlatformID.Win32NT) return false;

            switch (osv.Major)
            {
                case 5 when osv.Minor == 1:
                case 5 when osv.Minor == 2:
                    return true;
                default:
                    return false;
            }
        }


        // https://learn.microsoft.com/en-us/dotnet/framework/migration-guide/how-to-determine-which-versions-are-installed#query-the-registry-using-code
        public static bool IsDotNet45OrNewer()
        {
            using (var dotNetKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32)
                       .OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\"))
            {
                if (dotNetKey?.GetValue("Release") == null) return false;

                return (int)dotNetKey.GetValue("Release") >= 378389;
            }
        }
    }
}