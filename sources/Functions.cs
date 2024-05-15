using System;
using System.Net;
using Newtonsoft.Json;

namespace xp_apps.sources
{
    class Functions
    {
        public static string getFile(string url)
        {
            using (WebClient client = new WebClient())
            {
                return client.DownloadString(url);
            }
        }

        static void getApplications(dynamic json)
        {
            int count = 1;
            foreach (var category in json)
            {
                string categoryName = category.Name;
                foreach (var app in category.Value)
                {
                    Console.WriteLine($"{count}. [{categoryName}] - [{app.Value["filename"]}]");
                    count++;
                }
            }
        }

        public static void parseJson(string content)
        {
            dynamic Jobj = JsonConvert.DeserializeObject(content);
            getApplications(Jobj);
        }
    }
}
