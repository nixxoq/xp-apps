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
    static class Functions
    {
        static readonly string Help = $"XP-Apps ver {Constants.ProgramVersion}" +
                                      "\n\nList of available arguments:\n\n[Option]\t\t\t\t[Description]" +
                                      "\n-h, --help\t\t\t\tDisplay this help message" +
                                      "\n-i, --install\t\t\t\tInstall Application from XP-Apps repository" +
                                      "\n-l, --list, --list-applications,\tList all available applications in the repository \n--list-apps or --apps" +
                                      "\n\nExample:\n    xp-apps.exe -i PyCharm2023 or xp-apps.exe --install PyCharm2023";

        static string[] GetCommandArgs()
        {
            string[] args = Environment.GetCommandLineArgs();
            return args.Skip(1).ToArray();
        }

        /// <summary>
        ///     Parse arguments from command line
        /// </summary>
        public static void ParseArgs()
        {
            string[] args = GetCommandArgs();

            if (args.Length == 0)
            {
                Console.WriteLine(Help);
                return;
            }

            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];

                switch (arg)
                {
                    case "-i":
                    case "--install":
                    {
                        if (i + 1 < args.Length)
                        {
                            string appName = args[i + 1];
                            bool force = i + 2 < args.Length && args[i + 2].Equals("--force");
                            InstallApplication(appName, force);
                        }
                        else
                            Console.WriteLine("Error: Missing application name for install.");
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
            WebClient client = new WebClient();
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

            using (WebClient client = new WebClient())
            {
                char[] animationChars = { '/', '-', '\\', '|' };
                int animationIndex = 0;
                Stopwatch stopwatch = new Stopwatch();

                client.DownloadProgressChanged += (sender, e) =>
                {
                    double speed = e.BytesReceived / 1024d / stopwatch.Elapsed.TotalSeconds;
                    double remainingBytes = e.TotalBytesToReceive - e.BytesReceived;
                    double remainingSeconds = remainingBytes / 1024d / speed;

                    TimeSpan remainingTime = TimeSpan.FromSeconds(remainingSeconds);
                    char animationChar = animationChars[animationIndex++ % animationChars.Length];

                    Console.Write(
                        $"\r{animationChar} " + string.Format(
                            @"Downloading {0} | {1}% completed | {2:0.00} MB/s | {3:hh\:mm\:ss} remaining",
                            filename, e.ProgressPercentage, speed / 1024d,
                            remainingTime
                        )
                    );
                };

                client.DownloadFileCompleted += (sender, e) =>
                {
                    Thread.Sleep(1000);
                    Console.WriteLine(e.Error != null ? $"\nError: {e.Error.Message}" : $"\n{filename} download completed.\n");

                };
                stopwatch.Start();
                client.DownloadFileAsync(new Uri(url), filename);

                while (client.IsBusy) Thread.Sleep(100);

                stopwatch.Stop();
            }
        }

        public static bool IsNetworkAvailable()
        {
            Ping ping = new Ping();
            PingReply reply = ping.Send("www.google.com", 5000);

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

            string jsonContent = Cache.IsNeedUpdate() ? Cache.UpdateApplicationList(true) : File.ReadAllText(Cache.ApplicationsListPath);
            return JsonConvert.DeserializeObject<Applications>(jsonContent);
        }


        /// <summary>
        ///     Finds the application details based on the provided application name.
        /// </summary>
        /// <param name="appName">The name of the application to find.</param>
        /// <returns>The details of the application if found, otherwise null.</returns>
        static JObject FindApplication(string appName)
        {
            Applications apps = Constants.ProgramsList;

#if DEBUG
            Console.WriteLine($"[DEBUG] Searching {appName} in Browsers category...");
#endif

            foreach ((string programName, JObject programDetails) in Constants.GetProgramDetails(apps.Browsers))
            {
                string architecture = programDetails.GetValue("architecture").ToString();

                if (programName.Equals(appName, StringComparison.OrdinalIgnoreCase) &&
                    (architecture.Equals("any", StringComparison.OrdinalIgnoreCase) ||
                     architecture.Equals(Constants.OsArchitecture, StringComparison.OrdinalIgnoreCase))
                   )
                    return programDetails;

                if (programDetails.GetValue("aliases")
                    .ToObject<string[]>()
                    .Any(
                        alias =>
                            alias.Equals(appName, StringComparison.OrdinalIgnoreCase)
                            && (architecture.Equals(
                                    "any", StringComparison.OrdinalIgnoreCase
                                ) ||
                                architecture.Equals(Constants.OsArchitecture, StringComparison.OrdinalIgnoreCase))
                    ))
                    return programDetails;
            }

#if DEBUG
            Console.WriteLine($"[DEBUG] Searching {appName} in Vista native applications category...");
#endif

            foreach ((string programName, JObject programDetails) in Constants.GetProgramDetails(apps.VistaApplications))
            {
                string architecture = programDetails.GetValue("architecture").ToString();

                if (programName.Equals(appName, StringComparison.OrdinalIgnoreCase) &&
                    (architecture.Equals("any", StringComparison.OrdinalIgnoreCase) ||
                     architecture.Equals(Constants.OsArchitecture, StringComparison.OrdinalIgnoreCase)))
                    return programDetails;

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
        static void InstallApplication(string appName, bool isForce)
        {
            JObject application = FindApplication(appName);
            if (application == null)
            {
                Console.Write($"Could not find application {appName}.");

                // check if similar applications exist
                var similarApps = FindSimilarApplications(appName);
                if (similarApps.Any()) Console.WriteLine($" Did you mean: {string.Join(", ", similarApps.Distinct())}?");

                return;
            }

            string filename = application.GetValue("filename").ToString();
            string url = application.GetValue("url").ToString();

            Console.WriteLine($"Found application {appName}");

            WebClient client = GetClient();
            client.OpenRead(url);
            long filesize = Convert.ToInt64(client.ResponseHeaders.Get("Content-Length"));

            if (File.Exists(filename) && isForce) File.Delete(filename);


            // check if downloaded file is not corrupted
            if (File.Exists(filename) && new FileInfo(filename).Length == filesize)
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
        ///     Finds the similar applications based on the provided application name.
        /// </summary>
        /// <param name="appName">The name of the application to find.</param>
        /// <returns>The list of similar applications if found, otherwise null.</returns>
        static List<string> FindSimilarApplications(string appName)
        {
            Applications apps = Constants.ProgramsList;
            var allApps = new List<string>();

            foreach ((string programName, JObject programDetails) in Constants.GetProgramDetails(apps.Browsers))
            {
                allApps.Add(programName);
                allApps.AddRange(programDetails.GetValue("aliases").ToObject<string[]>());
            }

            foreach ((string programName, JObject programDetails) in Constants.GetProgramDetails(apps.VistaApplications))
            {
                allApps.Add(programName);
                allApps.AddRange(programDetails.GetValue("aliases").ToObject<string[]>());
            }

            int threshold = Math.Max(appName.Length / 2, 2);

            var potentialMatches =
            (
                from name in allApps
                let distance = LevenshteinDistance(appName, name)
                where distance <= threshold || name.IndexOf(appName, StringComparison.OrdinalIgnoreCase) >= 0
                select name
            ).ToList();

            return potentialMatches
                .Distinct()
                .OrderBy(x => LevenshteinDistance(appName, x))
                .ThenBy(x => x)
                .Take(5)
                .ToList();
        }

        /// <summary>
        ///     Calculates the Levenshtein distance between two strings.
        /// </summary>
        static int LevenshteinDistance(string s, string t)
        {
            // If one of the strings is empty or null, return the length of the other string as Levenshtein distance.
            if (string.IsNullOrEmpty(s)) return t?.Length ?? 0;
            if (string.IsNullOrEmpty(t)) return s.Length;

            int n = s.Length, m = t.Length;

            // Create a two-dimensional array d with size (n+1) x (m+1), where d[i, j] represents the Levenshtein distance between
            // the first i characters of string s and the first j characters of string t.
            int[,] d = new int[n + 1, m + 1];

            // Fill the first row and the first column of the array d with values from 0 to the lengths of strings s and t respectively.
            for (int i = 0; i <= n; i++) d[i, 0] = i;
            for (int j = 0; j <= m; j++) d[0, j] = j;

            // Iterate through the remaining elements of array d and compute the Levenshtein distance between all prefixes of strings s and t.
            for (int i = 1; i <= n; i++)
            for (int j = 1; j <= m; j++)
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
        static void GetApplications(Applications json)
        {
            Console.WriteLine("List of available applications:\nFormat:\n[Category]\n  [Application Name]\n");

            // Get all applications from Browsers category
            Console.WriteLine("Browsers:");
            foreach ((string programName, JObject programDetails) in Constants.GetProgramDetails(json.Browsers))
                Console.WriteLine($"  {programName} | aliases: {string.Join(", ", programDetails.GetValue("aliases").ToObject<string[]>())}");

            // Get all applications from Vista native applications category
            Console.WriteLine("Vista native applications:");
            foreach ((string programName, JObject programDetails) in Constants.GetProgramDetails(json.VistaApplications))
                Console.WriteLine($"  {programName} | aliases: {string.Join(", ", programDetails.GetValue("aliases").ToObject<string[]>())}");
        }

        /// <summary>
        ///     Checks if the current Windows version is Windows XP
        /// </summary>
        public static bool IsWindowsXp()
        {
            OperatingSystem os = Environment.OSVersion;
            Version osv = os.Version;

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
            RegistryKey dotNetKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\");

            if (dotNetKey?.GetValue("Release") == null) return false;

            return (int)dotNetKey.GetValue("Release") >= 378389;
        }
    }
}
