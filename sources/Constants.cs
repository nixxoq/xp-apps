using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace xp_apps.sources
{
    public static class Constants
    {
        // Major.Minor.Patch.Revision
        public const string ProgramVersion = "0.1.1.0";

        public const string UpdateJson = "https://raw.githubusercontent.com/Snaky1a/xp-apps/development/upd.json";

        public static readonly Applications ProgramsList = Functions.GetUpdates();

        public static readonly string OsArchitecture = Environment.Is64BitOperatingSystem ? "x64" : "x86";

        public static IEnumerable<(string ProgramName, JObject ProgramDetails)> GetProgramDetails(IEnumerable<BaseProgramContainer> programContainers)
        {
            return from programContainer in programContainers
                from program in programContainer.Applications
                let programName = program.Key
                let programDetails = (JObject)program.Value
                select (programName, programDetails);
        }
    }

    public class Applications
    {
        [JsonProperty("browsers")]
        public List<ProgramContainer> Browsers { get; set; }

        [JsonProperty("text")]
        public List<ProgramContainer> Text { get; set; }
    }

    public abstract class BaseProgramContainer
    {
        [JsonExtensionData]
        // ReSharper disable once CollectionNeverUpdated.Global
        public IDictionary<string, JToken> Applications { get; } = new Dictionary<string, JToken>();
    }

    public class ProgramContainer : BaseProgramContainer
    {
    }
}
