using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace xp_apps.sources.Structures
{
    public class ApplicationStructure
    {
        public class Applications
        {
            [JsonExtensionData] public IDictionary<string, JToken> Categories { get; set; }
        }

        public class ProgramContainer
        {
            public Dictionary<string, ProgramDetails> Applications { get; set; }
        }

        public class ProgramDetails
        {
            public string[] Aliases { get; set; }
            public string Filename { get; set; }
            public string Url { get; set; }
            public string Name { get; set; }
            public string Architecture { get; set; }
        }

        private static Dictionary<string, List<ProgramContainer>> _categories;

        public static Dictionary<string, List<ProgramContainer>> Categories =>
            _categories ?? (_categories = GenerateCategories());

        private static Dictionary<string, List<ProgramContainer>> GenerateCategories()
        {
            var categories = new Dictionary<string, List<ProgramContainer>>();

            foreach (var category in sources.Applications.ProgramsList.Categories)
            {
                var programList = category.Value.ToObject<List<JObject>>();
                var containers = new List<ProgramContainer>();

                foreach (var programObject in programList)
                {
                    foreach (var program in programObject)
                    {
                        var container = new ProgramContainer
                        {
                            Applications = new Dictionary<string, ProgramDetails>
                            {
                                { program.Key, program.Value.ToObject<ProgramDetails>() }
                            }
                        };
                        containers.Add(container);
                    }
                }

                categories.Add(category.Key, containers);
            }

            return categories;
        }

        public static ProgramDetails GetProgramDetailsByName(string category, string programName)
        {
            if (!Categories.TryGetValue(category, out var programContainers)) return null;
            foreach (var programContainer in programContainers)
            {
                if (programContainer.Applications.TryGetValue(programName, out var details))
                {
                    return details;
                }
            }

            return null;
        }

        public static IEnumerable<(string ProgramName, ProgramDetails ProgramDetails)>
            GetProgramDetails(
                IEnumerable<ProgramContainer> programContainers)
        {
            var details = new List<(string ProgramName, ProgramDetails ProgramDetails)>();

            foreach (var programContainer in programContainers)
            {
                details.AddRange(from program in programContainer.Applications
                    let programName = program.Key
                    let programDetails = program.Value
                    select (programName, programDetails));
            }

            return details;
        }
    }
}