﻿using System;
using System.IO;
using System.Net;
using System.Threading;
using Ionic.Zip;
using xp_apps.sources;

namespace xp_apps.Updater
{
    internal abstract class Updater
    {
        private static void Main(string[] args)
        {
            Thread.Sleep(2000);

            using (var client = new WebClient())
            {
                client.DownloadFile(Constants.LatestReleaseZip, "xp-apps.zip");
            }

            using (var zipFile = new ZipFile("xp-apps.zip"))
            {
                foreach (var file in zipFile) file.Extract(".", ExtractExistingFileAction.OverwriteSilently);
            }

            File.Delete("xp-apps.zip");

            Console.WriteLine("Application has been updated successfully. Press any key to exit...");
            Console.ReadLine();
        }
    }
}