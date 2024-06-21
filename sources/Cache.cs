using System;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace xp_apps.sources
{
    public abstract class Cache
    {
        static readonly bool IsSettingsExist = File.Exists(SettingsPath);

        public static string ApplicationsListPath => Path.Combine(GetExecutableDirectory(), Constants.CacheFolder, Constants.ApplicationsListName);

        // Settings
        static string SettingsPath => Path.Combine(GetExecutableDirectory(), Constants.CacheFolder, Constants.SettingsFileName);

        static string GetExecutableDirectory() => Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);


        static void CreateApplicationFolder(string folderPath = null)
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
                Console.WriteLine($"Path too long to create folder {folderPath}. Place xp-apps in a different folder with a shorter path.");
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

        public static bool IsAppsListExist() => File.Exists(ApplicationsListPath);

        public static bool IsNeedUpdate()
        {
            // true - need update; false - up to date or not exist

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

        static void CreateSettings()
        {
            string json = JsonConvert.SerializeObject(new { version = Constants.ProgramVersion }, Formatting.Indented);

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

            string jsonContent = File.ReadAllText(SettingsPath);
            JObject settingsFile = JObject.Parse(jsonContent);

            if (version != "")
                settingsFile["version"] = version;

            File.WriteAllText(SettingsPath, settingsFile.ToString());
        }
    }
}
