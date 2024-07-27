using System;
using System.IO;
using System.Net;
using System.Threading;
using Ionic.Zip;

namespace xp_apps.Updater
{
    internal abstract class Updater
    {
        private static void Main(string[] args)
        {
            Thread.Sleep(2000);

#warning TODO: Reimplement using http or CurlWrapper (if data.nixxoq.xyz is not available)
            using (var client = new WebClient())
            {
                client.DownloadFile(sources.Updater.LatestReleaseZip, "xp-apps.zip");
            }

            using (var zipFile = new ZipFile("xp-apps.zip"))
            {
                foreach (var file in zipFile) file.Extract(".", ExtractExistingFileAction.OverwriteSilently);
            }

#warning TODO: Rename original exe file if user changed (why not?)
            File.Delete("xp-apps.zip");

            Console.WriteLine("Application has been updated successfully. Press any key to exit...");
            Console.ReadLine();
        }
    }
}