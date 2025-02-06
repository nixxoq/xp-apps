using System;
using Microsoft.Win32;
using static xp_apps.sources.Structures.ApplicationStructure;

namespace xp_apps.sources
{
    internal static class MainScreen
    {
        public static readonly bool IsWindowsNt5 = IsWindowsXp();

        private static readonly string Help = $"xp-apps ver {Updater.ProgramVersion}" +
                                              "\n\nList of available arguments:\n\n[Option]\t\t\t\t[Description]" +
                                              "\n-h, --help\t\t\t\tDisplay this help message" +
                                              "\n-i, --install\t\t\t\tInstall Application from XP-Apps repository" +
                                              "\n-l, --list, --list-applications,\tList all available applications in the repository \n--list-apps or --apps" +
                                              $"\n\nExample:\n    {Helper.CurrentFile} -i PyCharm2023" +
                                              $"\n    {Helper.CurrentFile} --install PyCharm2023";

        /// <summary>
        ///     Parse arguments from command line
        /// </summary>
        public static void ParseArgs()
        {
            var args = Helper.GetCommandArgs();

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
        private static bool IsWindowsXp()
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
    }
}