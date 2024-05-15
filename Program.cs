using System;
using System.Net;
using xp_apps.sources;

namespace xp_apps
{
    class Program
    {
        static void Main(string[] args)
        {

            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072; //TLS 1.2

            try
            {
                string content = Functions.getFile("https://raw.githubusercontent.com/snaky1a/xp-apps/development/upd.json");
                Functions.parseJson(content);
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
                Console.ReadLine();
            }
        }

    }
}
