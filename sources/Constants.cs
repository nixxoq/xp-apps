using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace xp_apps.sources
{
    public static class Constants
    {
        // Major.Minor.Patch.Revision
        public const string ProgramVersion = "0.1.0.1";

        public const string UpdateJson = "https://raw.githubusercontent.com/Snaky1a/xp-apps/development/upd.json";

        public static readonly Applications ProgramsList = Functions.GetUpdates();

        public static IEnumerable<(string ProgramName, JObject ProgramDetails)> GetProgramDetails(IEnumerable<ProgramContainer> programContainers)
        {
            return from programContainer in programContainers
                from program in programContainer.Programs
                let programName = program.Key
                let programDetails = (JObject)program.Value
                select (programName, programDetails);
        }
    }

    // public class Application
    // {
    //     [JsonProperty("filename")]
    //     public string Filename { get; set; }
    //
    //     [JsonProperty("url")]
    //     public string Url { get; set; }
    // }
    //
    // public class Category
    // {
    //     [JsonProperty("name")]
    //     public string Name { get; set; }
    //
    //     [JsonProperty("filename")]
    //     public string Filename { get; set; }
    //
    //     [JsonProperty("url")]
    //     public string Url { get; set; }
    //     
    // }
    //
    // public class Applications
    // {
    //     [JsonProperty("browsers")]
    //     public Dictionary<string, List<Category>> Browsers { get; set; }
    //
    //     [JsonProperty("category2")]
    //     public List<Category> Category2 { get; set; }
    // }

    public class Applications
    {
        [JsonProperty("browsers")]
        public List<ProgramContainer> Browsers { get; set; }

        [JsonProperty("text")]
        public List<ProgramContainer> Text { get; set; }
    }

    public class ProgramContainer
    {
        [JsonExtensionData]
        public IDictionary<string, JToken> Programs { get; } = new Dictionary<string, JToken>();
    }

}
