using System;
using System.Net;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace xp_apps.sources
{
    static class Functions
    {
        static readonly string Help = $"XP-Apps ver {Constants.ProgramVersion}" +
                                      $"\n\nList of available arguments:\n\n[Option]\t\t\t\t[Description]" +
                                      $"\n-h, --help\t\t\t\tDisplay this help message" +
                                      $"\n-i, --install\t\t\t\tInstall Application from XP-Apps repository" +
                                      $"\n-l, --list, --list-applications,\tList all available applications in the repository \n--list-apps or --apps" +
                                      $"\n\nExample:\n    xp-apps.exe -i PyCharm2023 or xp-apps.exe --install PyCharm2023";

        /// <summary>
        /// Parse arguments from command line
        /// </summary>
        /// <param name="args">arguments from main function</param>
        public static void ParseArgs(string[] args)
        {
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
                            // return;
                            string appName = args[i + 1];
                            InstallApplication(appName);
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
                }
            }
            Console.WriteLine(Help);
        }

        /// <summary>
        /// Get content from URL
        /// </summary>
        /// <param name="url">URL link</param>
        /// <returns>URL content</returns>
        static string GetContent(string url)
        {
            using (WebClient client = new WebClient())
                return client.DownloadString(url);
        }

        static void DownloadFile(string url, string filename)
        {

            using (WebClient client = new WebClient())
            {
                char[] animationChars = new char[] { '/', '-', '\\', '|' };
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
                            "Downloading {0} | {1}% completed | {2:0.00} MB/s | {3:hh\\:mm\\:ss} remaining",
                            filename, e.ProgressPercentage, speed / 1024d,
                            remainingTime
                        )
                    );
                };

                client.DownloadFileCompleted += (sender, e) =>
                {
                    Console.WriteLine(e.Error != null ? $"\nError: {e.Error.Message}" : $"\n{filename} download completed.");
                };

                stopwatch.Start();
                client.DownloadFileAsync(new Uri(url), filename);

                while (client.IsBusy)
                    Thread.Sleep(100);

                stopwatch.Stop();
            }
        }

        public static Applications GetUpdates()
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;

            string jsonContent = GetContent(Constants.UpdateJson);

            var applications = JsonConvert.DeserializeObject<Applications>(jsonContent);
            return applications;
        }

        /// <summary>
        /// Parse string to dynamic object
        /// </summary>
        /// <param name="content">String to parse</param>
        public static object ParseJson(string content)
        {
            object jsonObj = JsonConvert.DeserializeObject(content);
            return jsonObj;
        }

        static Category FindApplication(string appName)
        {
            Applications apps = Constants.ProgramsList;

            // find in Browsers category
            return apps.Browsers.SelectMany(category => category.Value).FirstOrDefault(app => app.Name.Equals(appName));
        }

        static void InstallApplication(string appName)
        {
            Category application = FindApplication(appName);

            if (application == null) return;

            Console.WriteLine($"Found application {appName}\nDownloading file {application.Filename}...");
            DownloadFile(application.Url, application.Filename);
        }

        /// <summary>
        /// Get all applications available to install
        /// </summary>
        /// <param name="json">Applications list (json object)</param>
        static void GetApplications(Applications json)
        {
            Console.WriteLine($"List of available applications:\nFormat:\n[Category]\t\t[Application Name]");

            // Get all applications from Browsers category
            foreach (var browser in json.Browsers)
            {
                string categoryName = browser.Key;
                foreach (Category browserValue in browser.Value)
                    Console.WriteLine($"{categoryName}\t\t{browserValue.Name}");
            }

            // Get all applications from test category
            foreach (Category tool in json.Category2)
                Console.WriteLine($"{tool.Name}");
        }

        /// <summary>
        /// Checks if the current Windows version is Windows XP
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
