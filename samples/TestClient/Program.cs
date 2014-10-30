using System;
using System.Net.Http;
using Microsoft.Net.Http.Client;

namespace TestClient
{
    class Program
    {
        public static void Main(string[] args)
        {
            // System.Diagnostics.Debugger.Launch();
            HttpClient client = new HttpClient(new ManagedHandler()
            {
                // ProxyAddress = new Uri("http://itgproxy:80")
            });

            var response = client.GetAsync(
                // "https://www.myget.org/f/aspnetwebstacknightly/"
                "https://packages.nuget.org/v1/Package/Download/EntityFramework/4.1.10331.0"
                ).Result;
            Console.WriteLine(response);
            // Console.WriteLine(response.Content.ReadAsStringAsync().Result);
        }
    }
}
