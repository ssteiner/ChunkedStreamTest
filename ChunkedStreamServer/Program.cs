using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Owin.Hosting;

namespace ChunkedStreamServer
{
    class Program
    {
        static void Main(string[] args)
        {
            startOwnServer();
        }

        private static void startOwnServer()
        {
            string baseAddress = "http://+:8091/";

            // Start OWIN host 
            using (WebApp.Start<Startup>(url: baseAddress))
            {
                // Create HttpCient and make a request to api/values 
                Console.WriteLine("Server started on " + baseAddress);
                Console.ReadLine();
            }
        }
    }
}
