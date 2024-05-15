using System;
using System.Net;
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
                         $"\n-l, --list, --list-applications\t\tList all available applications in the repository" +
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
                    break;
                }
                else if (arg.Equals("-h") || arg.Equals("--help"))
                {
                    Console.WriteLine(HELP);
                    break;
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

        /// <summary>
        /// Parse string to dynamic object
        /// </summary>
        /// <param name="content">String to parse</param>
        public static void parseJson(string content)
        {
            dynamic Jobj = JsonConvert.DeserializeObject(content);
            getApplications(Jobj);
        }

        /// <summary>
        /// Get all applications available to install
        /// </summary>
        /// <param name="json">Applications list (json object)</param>
        static void getApplications(dynamic json)
        {
            int count = 1;
            foreach (var category in json)
            {
                string categoryName = category.Name;
                foreach (var app in category.Value)
                {
                    Console.WriteLine($"{count}. [{categoryName}] - [{app.Value["filename"]}]");
                    count++;
                }
            }
        }
    }
}
