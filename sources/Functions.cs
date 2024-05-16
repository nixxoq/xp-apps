using System;
using System.Net;
using Microsoft.Win32;
using Newtonsoft.Json;
using xp_apps.sources.constants;

namespace xp_apps.sources
{
    class Functions
    {
        private static string HELP = $"XP-Apps ver {Constants.PROGRAM_VERSION}" +
                         $"\n\nList of available arguments:\n\n[Option]\t\t\t\t[Description]" +
                         $"\n-h, --help\t\t\t\tDisplay this help message" +
                         $"\n-i, --install\t\t\t\tInstall Application from XP-Apps repository" +
                         $"\n-l, --list, --list-applications,\tList all available applications in the repository \n--list-apps or --apps" +
                         $"\n\nExample:\n    xp-apps.exe -i PyCharm2023 or xp-apps.exe --install PyCharm2023";

        /// <summary>
        /// Parse arguments from command line
        /// </summary>
        /// <param name="args">arguments from main function</param>
        public static void parseArgs(string[] args)
        {
            foreach (string arg in args)
            {
                if (arg.Equals("-i") || arg.Equals("--install"))
                {
                    Console.WriteLine("Unimplemented.");
                    return;
                }
                else if (arg.Equals("-h") || arg.Equals("--help"))
                {
                    Console.WriteLine(HELP);
                    return;
                }
                else if (arg.Equals("-l") || arg.Equals("--list") || arg.Equals("--list-applications") || arg.Equals("--list-apps") || arg.Equals("--apps"))
                {
                    getApplications(Constants.programs_list);
                    return;
                }
            }
            Console.WriteLine(HELP);
        }

        /// <summary>
        /// Get content from URL
        /// </summary>
        /// <param name="url">URL link</param>
        /// <returns>URL content</returns>
        public static string getContent(string url)
        {
            using (WebClient client = new WebClient())
            {
                return client.DownloadString(url);
            }
        }

        public static object getUpdates()
        {
            // hardcode... yeah
            // TODO: add UPDATE_JSON variable to Constants.cs
            return parseJson(getContent("https://raw.githubusercontent.com/Snaky1a/xp-apps/development/upd.json"));
        }

        /// <summary>
        /// Parse string to dynamic object
        /// </summary>
        /// <param name="content">String to parse</param>
        public static object parseJson(string content)
        {
            object Jobj = JsonConvert.DeserializeObject(content);
            //getApplications(Jobj);
            //return Jobj.ToString();
            return Jobj;
        }

        /// <summary>
        /// Get all applications available to install
        /// </summary>
        /// <param name="json">Applications list (json object)</param>
        static void getApplications(dynamic json)
        {
            Console.WriteLine($"List of available applications:\nFormat:\n[Category]      [Application Name]");
            foreach (dynamic category in json)
            {
                string categoryName = category.Name;
                foreach (var app in category.Value)
                    Console.WriteLine($"{categoryName}\t\t{app.Value["filename"]}");
            }
        }

        /// <summary>
        /// Checks if the current Windows version is Windows XP
        /// </summary>
        public static bool isWindowsXP()
        {
            OperatingSystem os = Environment.OSVersion;
            Version osv = os.Version;

            if (os.Platform == PlatformID.Win32NT)
            {
                if (osv.Major == 5 && osv.Minor == 1)
                    return true;
                if (osv.Major == 5 && osv.Minor == 5)
                    return true;
            }
            return false;
        }



        // https://learn.microsoft.com/en-us/dotnet/framework/migration-guide/how-to-determine-which-versions-are-installed#query-the-registry-using-code
        public static bool IsDotNet45orNewer()
        {
            var localkey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\");

            if (localkey != null && localkey.GetValue("Release") != null)
                if ((int)localkey.GetValue("Release") >= 378389)
                    return false;

            return true;

            //if (releaseKey >= 533320)
            //    return "4.8.1 or later";
            //if (releaseKey >= 528040)
            //    return "4.8";
            //if (releaseKey >= 461808)
            //    return "4.7.2";
            //if (releaseKey >= 461308)
            //    return "4.7.1";
            //if (releaseKey >= 460798)
            //    return "4.7";
            //if (releaseKey >= 394802)
            //    return "4.6.2";
            //if (releaseKey >= 394254)
            //    return "4.6.1";
            //if (releaseKey >= 393295)
            //    return "4.6";
            //if (releaseKey >= 379893)
            //    return "4.5.2";
            //if (releaseKey >= 378675)
            //    return "4.5.1";
            //if (releaseKey >= 378389)
            //    return "4.5";

            //return "No 4.5 or later version detected";
        }
    }
}
