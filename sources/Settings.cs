using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace xp_apps.sources
{
    public class Settings
    {
        private const string SettingsFileName = "settings.json";

        private static string SettingsPath =>
            Path.Combine(Helper.WorkDir, Cache.CacheFolder, SettingsFileName);

        private static readonly bool IsSettingsExist = File.Exists(SettingsPath);

        private static void CreateSettings()
        {
            var json = JsonConvert.SerializeObject(new { version = Updater.ProgramVersion }, Formatting.Indented);

            File.WriteAllText(SettingsPath, json);

#if DEBUG
            SimpleLogger.Logger.Info($"Settings file content:\n{json}");
#endif
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

    public abstract class Cache
    {
        public const string CacheFolder = "cache";
    }
}