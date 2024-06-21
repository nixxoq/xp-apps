using System;
using System.IO;
using System.Net;
using System.Threading;
using Ionic.Zip;
using xp_apps.sources;

namespace xp_apps.Updater
{
    abstract class Updater
    {
        static void Main(string[] args)
        {
            Thread.Sleep(2000);

            WebClient client = new WebClient();
            client.DownloadFile(Constants.LatestReleaseZip, "xp-apps.zip");

            using (ZipFile zipFile = new ZipFile("xp-apps.zip"))
            {
                foreach (ZipEntry file in zipFile)
                    file.Extract(".", ExtractExistingFileAction.OverwriteSilently);
            }

            File.Delete("xp-apps.zip");

            Console.WriteLine("Application has been updated successfully. Press any key to exit...");
            Console.ReadLine();
        }
    }
}
