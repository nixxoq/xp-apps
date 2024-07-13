using System;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace xp_apps.sources
{
    public abstract class Cache
    {
        private static readonly bool IsSettingsExist = File.Exists(SettingsPath);

        public static string ApplicationsListPath => Path.Combine(GetExecutableDirectory(), Constants.CacheFolder,
            Constants.ApplicationsListName);

        // Settings
        private static string SettingsPath =>
            Path.Combine(GetExecutableDirectory(), Constants.CacheFolder, Constants.SettingsFileName);

        private static string GetExecutableDirectory()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }


        private static void CreateApplicationFolder(string folderPath = null)
        {
            if (folderPath == null) folderPath = Path.Combine(GetExecutableDirectory(), Constants.CacheFolder);

            if (Directory.Exists(folderPath)) return;

            try
            {
                Directory.CreateDirectory(folderPath);
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine($"Access denied to creating folder {folderPath}");
            }
            catch (PathTooLongException)
            {
                Console.WriteLine(
                    $"Path too long to create folder {folderPath}. Place xp-apps in a different folder with a shorter path.");
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
            // true - need update; false - up to date or not exist

            var client = Functions.GetClient();
            client.OpenRead(Constants.ApplicationsList);
            var filesize = Convert.ToInt64(client.ResponseHeaders.Get("Content-Length"));

            if (!File.Exists(ApplicationsListPath) || new FileInfo(ApplicationsListPath).Length != filesize)
                return true;

#if DEBUG
            SimpleLogger.Logger.Debug("main application list is up-to-date.");
#endif
            return false;
        }

        private static void CreateSettings()
        {
            var json = JsonConvert.SerializeObject(new { version = Constants.ProgramVersion }, Formatting.Indented);

            File.WriteAllText(SettingsPath, json);

            Console.WriteLine(File.ReadAllText(SettingsPath));
        }

        public static void UpdateSettings(string version = "")
        {
            if (!IsSettingsExist)
            {
                CreateSettings();
                return;
            }

            var jsonContent = File.ReadAllText(SettingsPath);
            var settingsFile = JObject.Parse(jsonContent);

            if (version != "")
                settingsFile["version"] = version;

            File.WriteAllText(SettingsPath, settingsFile.ToString());
        }
    }
}