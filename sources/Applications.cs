using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using xp_apps.sources.Structures;
using static xp_apps.sources.Structures.ApplicationStructure;

namespace xp_apps.sources
{
    public abstract class Applications
    {
        private const string ApplicationDb = "https://raw.githubusercontent.com/nixxoq/xp-apps/development/upd.json";
        private static string ApplicationsListName => Helper.ExtractFileNameFromUrl(ApplicationDb);

        private static string ApplicationsListPath => Path.Combine(
            Helper.WorkDir,
            Cache.CacheFolder,
            ApplicationsListName);

        private static readonly bool IsAppsListExist = File.Exists(ApplicationsListPath);

        public static readonly ApplicationStructure.Applications ProgramsList = GetUpdates();

        /// <summary>
        ///     Creates the application list file if it doesn't exist on cache folder.
        /// </summary>
        private static string UpdateApplicationList(bool isNeedUpdate = false)
        {
            Helper.CreateApplicationFolder();
            if (IsAppsListExist && !isNeedUpdate)
                return File.ReadAllText(ApplicationsListPath);

            Helper.DownloadFile(ApplicationDb, ApplicationsListPath);
            Console.Clear();

            return File.ReadAllText(ApplicationsListPath);
        }

        private static bool IsNeedUpdate()
        {
            // true - need update; false - up to date or not exist

            var client = Helper.GetClient();
            client.OpenRead(ApplicationDb);
            var filesize = Convert.ToInt64(client.ResponseHeaders.Get("Content-Length"));

            if (!File.Exists(ApplicationsListPath) || new FileInfo(ApplicationsListPath).Length != filesize)
                return true;

#if DEBUG
            SimpleLogger.Logger.Debug("main application list is up-to-date.");
#endif
            return false;
        }

        /// <summary>
        ///     Get all applications available to install
        /// </summary>
        /// <param name="categories">Dictionary of application categories and their program containers</param>
        public static void GetApplications(Dictionary<string, List<ProgramContainer>> categories)
        {
            Console.WriteLine("List of available applications:\nFormat:\n[Category]\n  [Application Name]\n");

            foreach (var category in categories)
            {
                Console.WriteLine($"{category.Key}:");
                foreach (var (programName, programDetails) in GetProgramDetails(category.Value))
                {
                    Console.WriteLine(programDetails.Aliases.Any()
                        ? $"  {programName} | aliases: {string.Join(", ", programDetails.Aliases)}"
                        : $"  {programName}");
                }
            }
        }

        /// <summary>
        ///     Finds the application details based on the provided application name.
        /// </summary>
        /// <param name="appName">The name of the application to find.</param>
        /// <returns>The details of the application if found, otherwise null.</returns>
        private static ProgramDetails FindApplication(string appName)
        {
            return Categories
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
        private static ProgramDetails FindApplicationInCategory(string appName, List<ProgramContainer> category,
            string categoryName)
        {
#if DEBUG
            SimpleLogger.Logger.Debug($"Searching {appName} in {categoryName} category...");
#endif

            foreach (var (programName, programDetails) in GetProgramDetails(category))
            {
                if (programName.Equals(appName, StringComparison.OrdinalIgnoreCase) &&
                    (programDetails.Architecture.Equals("any", StringComparison.OrdinalIgnoreCase) ||
                     programDetails.Architecture.Equals(Helper.OsArchitecture, StringComparison.OrdinalIgnoreCase)))
                    return programDetails;

                if (programDetails.Aliases.Any(
                        alias => alias.Equals(appName, StringComparison.OrdinalIgnoreCase) &&
                                 (programDetails.Architecture.Equals("any", StringComparison.OrdinalIgnoreCase) ||
                                  programDetails.Architecture.Equals(Helper.OsArchitecture,
                                      StringComparison.OrdinalIgnoreCase))
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
        public static void InstallApplication(string appName, bool isForce)
        {
            var applicationDetails = FindApplication(appName);
            if (applicationDetails == null)
            {
                Console.Write($"Could not find application {appName}.");

                // check if similar applications exist
                var similarApps = FindSimilarApplications(appName);
                if (similarApps.Any())
                    Console.WriteLine($" Did you mean: {string.Join(", ", similarApps.Distinct())}?");

                return;
            }

            var filename = applicationDetails.Filename;
            var url = applicationDetails.Url;

            Console.WriteLine($"Found application {appName}");

            var client = Helper.GetClient();
            client.OpenRead(url ?? string.Empty);
            var filesize = Convert.ToInt64(client.ResponseHeaders.Get("Content-Length"));

            var downloadedIn = Path.Combine(Helper.WorkDir, filename);
            Console.WriteLine(
                $"Application name: {applicationDetails.Name}\nSize (in MB): {filesize / (1024 * 1024)}" +
                $"Filename: {applicationDetails.Filename}\nWill be downloaded in {downloadedIn}");

            if (File.Exists(filename) && isForce)
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

            Helper.DownloadFile(url, downloadedIn);
        }

        /// <summary>
        ///     Finds similar applications based on the provided application name.
        /// </summary>
        /// <param name="appName">The name of the application to find.</param>
        /// <returns>The list of similar applications if found, otherwise null.</returns>
        private static List<string> FindSimilarApplications(string appName)
        {
            var allApps = Categories
                .SelectMany(c => GetProgramDetails(c.Value))
                .SelectMany(p => new[] { p.ProgramName }.Concat(p.ProgramDetails.Aliases))
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
            if (string.IsNullOrEmpty(s)) return t?.Length ?? 0;
            if (string.IsNullOrEmpty(t)) return s.Length;

            int n = s.Length, m = t.Length;

            var d = new int[n + 1, m + 1];

            for (var i = 0; i <= n; i++) d[i, 0] = i;
            for (var j = 0; j <= m; j++) d[0, j] = j;

            for (var i = 1; i <= n; i++)
            for (var j = 1; j <= m; j++)
                d[i, j] = Math.Min(
                    Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                    d[i - 1, j - 1] + (s[i - 1] == t[j - 1] ? 0 : 1)
                );

            return d[n, m];
        }

        /// <summary>
        ///     Retrieves the applications list updates from the server and deserializes them into an Applications object.
        /// </summary>
        /// <returns>The deserialized Applications object containing the updates.</returns>
        private static ApplicationStructure.Applications GetUpdates()
        {
            if (!Helper.IsNetworkAvailable())
            {
                if (!IsAppsListExist)
                {
                    Console.WriteLine("No internet connection. Please check your internet connection and try again.");
                    Environment.Exit(0);
                }
                else
                {
                    Console.WriteLine("No internet connection. Using cached application list.");
                    return JsonConvert.DeserializeObject<ApplicationStructure.Applications>(
                        File.ReadAllText(ApplicationsListPath));
                }
            }

            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;

            var jsonContent = IsNeedUpdate()
                ? UpdateApplicationList(true)
                : File.ReadAllText(ApplicationsListPath);

            return JsonConvert.DeserializeObject<ApplicationStructure.Applications>(jsonContent);
        }
    }
}