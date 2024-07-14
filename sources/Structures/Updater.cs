using Newtonsoft.Json;

namespace xp_apps.sources.Structures
{
    public class UpdateData
    {
        [JsonProperty("version")] public string Version { get; set; }

        [JsonProperty("changes")] public string Changes { get; set; }
    }
}