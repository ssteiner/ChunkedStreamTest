using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChunkedStreamClient
{
    class Program
    {
        static void Main(string[] args)
        {

            System.Net.ServicePointManager.DefaultConnectionLimit = 10000;

            string baseUrl = @"http://localhost:8091/";

            ServerConnector conn = new ServerConnector(baseUrl);
            conn.StartPolling();

            Console.WriteLine("press enter to quit");
            Console.ReadLine();

        }
    }
}
