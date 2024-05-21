using System;
using System.Net;
using System.Diagnostics;
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
                            return;
                            // string appName = args[i + 1];
                            //InstallApplication(appName);
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
            {
                return client.DownloadString(url);
            }
        }

        public static void DownloadFile(string url, string filename)
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

        //public static Category getUpdates()
        //{
        //    // Initializing Security Protocols for HTTPS requests
        //    ServicePointManager.Expect100Continue = true;
        //    ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
        //    // hardcode... yeah
        //    // TODO: add UPDATE_JSON variable to Constants.cs
        //    string content = JsonConvert.SerializeObject(getContent(Constants.UPDATE_JSON));
        //    Category category = JsonConvert.DeserializeObject<Category>(content);
        //    return category;
        //}

        public static Applications GetUpdates()
        {
            // Initializing Security Protocols for HTTPS requests
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;

            // Retrieve JSON content from the specified URL
            string jsonContent = GetContent(Constants.UpdateJson);

            // Log/print the JSON content for inspection
            Console.WriteLine("Retrieved JSON content:");
            Console.WriteLine(jsonContent);

            Applications applications = JsonConvert.DeserializeObject<Applications>(jsonContent);
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

        public static Application FindApplication(string appName)
        {
            dynamic apps = Constants.ProgramsList;
            foreach (dynamic category in apps)
            {
                foreach (var app in category.Value)
                {
                    if (app.Name.Equals(appName))
                    {
                        string appValue = JsonConvert.SerializeObject(app.Value);
                        Application application = JsonConvert.DeserializeObject<Application>(appValue);
                        return application;
                    }
                }
            }
            return null;
        }

        //static void InstallApplication(string appName)
        //{
        //    Application application = FindApplication(appName);

        //    if (application != null)
        //    {
        //        Console.WriteLine($"Found application {appName}\nDownloading file {application.FileName}...");
        //        DownloadFile(application.url, application.FileName);
        //    }
        //}

        /// <summary>
        /// Get all applications available to install
        /// </summary>
        /// <param name="json">Applications list (json object)</param>
        static void GetApplications(dynamic json)
        {
            Console.WriteLine($"List of available applications:\nFormat:\n[Category]      [Application Name]");
            foreach (dynamic category in json)
            {
                string categoryName = category.Name;
                foreach (dynamic app in category.Value)
                    Console.WriteLine($"{categoryName}\t\t{app.Value["filename"]}");
            }
        }

        /// <summary>
        /// Checks if the current Windows version is Windows XP
        /// </summary>
        public static bool IsWindowsXp()
        {
            OperatingSystem os = Environment.OSVersion;
            Version osv = os.Version;

            if (os.Platform == PlatformID.Win32NT)
                if ((osv.Major == 5 && osv.Minor == 1) || (osv.Major == 5 && osv.Minor == 2))
                    return true;

            return false;
        }



        // https://learn.microsoft.com/en-us/dotnet/framework/migration-guide/how-to-determine-which-versions-are-installed#query-the-registry-using-code
        public static bool IsDotNet45OrNewer()
        {
            RegistryKey localkey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\");

            if (localkey != null && localkey.GetValue("Release") != null)
                if ((int)localkey.GetValue("Release") >= 378389)
                    return true;

            return false;
        }
    }
}
