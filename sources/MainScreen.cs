using System;
using System.Linq;
using Microsoft.Win32;
using static xp_apps.sources.Structures.ApplicationStructure;

namespace xp_apps.sources
{
    internal static class MainScreen
    {
        private static readonly string Help = $"xp-apps ver {Updater.ProgramVersion}" +
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
                            Applications.InstallApplication(appName, force);
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
                        Applications.GetApplications(Categories);
                        return;
                    case "--self-update":
                        Updater.Update();
                        return;
                }
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
#warning this function will be removed soon
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