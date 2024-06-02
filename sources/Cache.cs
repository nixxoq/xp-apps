using System;
using System.IO;
using System.Net;

namespace xp_apps.sources
{
    public class Cache
    {
        const string ApplicationsListName = "upd.json";

        const string CacheFolder = "cache";

        public static string ApplicationsListPath => Path.Combine(CacheFolder, ApplicationsListName);

        static bool CreateApplicationFolder(string folderPath = CacheFolder)
        {
            if (Directory.Exists(folderPath))
                return true;

            try
            {
                Directory.CreateDirectory(folderPath);
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine($"Access denied to creating folder {folderPath}");
                throw;
            }
            catch (PathTooLongException)
            {
                Console.WriteLine($"Path too long to create folder {folderPath}. Place xp-apps in a different folder with a shorter path.");
                throw;
            }
        }

        /// <summary>
        ///     Creates the application list file if it doesn't exist on cache folder.
        /// </summary>
        public static string UpdateApplicationList(bool isNeedUpdate = false)
        {
            CreateApplicationFolder();
            if (IsAppsListExist() && !isNeedUpdate)
                return File.ReadAllText(ApplicationsListPath);

            Functions.DownloadFile(Constants.ApplicationsList, ApplicationsListPath);
            Console.Clear();

            return File.ReadAllText(ApplicationsListPath);
        }

        public static bool IsAppsListExist()
        {
            return File.Exists(ApplicationsListPath);
        }

        public static bool IsNeedUpdate()
        {
            // true - need update; false - up-to-date or not exist

            WebClient client = Functions.GetClient();
            client.OpenRead(Constants.ApplicationsList);
            long filesize = Convert.ToInt64(client.ResponseHeaders.Get("Content-Length"));

            if (!File.Exists(ApplicationsListPath) || new FileInfo(ApplicationsListPath).Length != filesize)
                return true;

#if DEBUG
            Console.WriteLine(
                "[DEBUG] main application list is up-to-date."
            );
#endif
            return false;
        }
    }
}
