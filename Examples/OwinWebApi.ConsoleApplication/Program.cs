using System;
using System.Net.Http;
using Microsoft.Owin.Hosting;

namespace OwinWebApi.ConsoleApplication
{
    class Program
    {
        static void Main()
        {
            const string baseAddress = "http://localhost:9123/";

            // Start the OWIN host.
            using (WebApp.Start<Startup>(baseAddress))
            {
                // Create HttpClient and make a request to the test controller.
                var client = new HttpClient();
                var response = client.GetAsync(baseAddress + "api/test").Result;

                Console.WriteLine(response);
                Console.WriteLine(response.Content.ReadAsStringAsync().Result);
            }

            Console.ReadLine();
        }
    }
}
