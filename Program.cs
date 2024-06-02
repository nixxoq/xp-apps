using System;
using System.Net;
using xp_apps.sources;

namespace xp_apps
{
    static class Program
    {
        static void Main()
        {
#if DEBUG
            Console.WriteLine($"[DEBUG] Current architecture: {Constants.OsArchitecture} | Current OS: {Environment.OSVersion}");
#endif
            // Checks if current operating system is Windows XP (NT 5.1 & NT 5.2)
            // However, I am thinking about adding support for Windows Vista when the One-Core-API 4.1.0 will be released 👀
            // if (!Functions.IsWindowsXp())
            // {
            //     Console.WriteLine("This program works only on Windows XP.");
            //     Console.WriteLine("Press any key to exit...");
            //     Console.ReadLine();
            //     return;
            // }

            // Checks if .NET Framework 4.5 or newer is installed
            // Its need for TLS 1.2 protocol
            if (!Functions.IsDotNet45OrNewer())
            {
                Console.WriteLine("This program works only with installed .NET Framework 4.0 and 4.5+\nMake sure you have installed the One-Core-API before installing .NET Framework 4.5+!");
                Console.WriteLine("Press any key to exit...");
                Console.ReadLine();
                return;
            }

            // Initializing Security Protocols for HTTPS requests
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;

            Functions.ParseArgs();
        }
    }
}
