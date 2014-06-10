using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Net.Http.Client;

namespace TestClient
{
    class Program
    {
        public static void Main(string[] args)
        {
            HttpClient client = new HttpClient(new ManagedHandler()
            {
                // ProxyAddress = new Uri("http://itgproxy:80")
            });

            var response = client.GetAsync("https://www.myget.org/f/aspnetwebstacknightly/").Result;
            Console.WriteLine(response);
            // Console.WriteLine(response.Content.ReadAsStringAsync().Result);
        }
    }
}
