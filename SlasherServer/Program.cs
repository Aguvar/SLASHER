using System;
using System.Configuration;

namespace SlasherServer
{
    class Program
    {
        static void Main(string[] args)
        {
            string ipString = ConfigurationManager.AppSettings["ipaddress"];

            Console.WriteLine("---Slasher Server V.0.01---");
            Console.WriteLine();

            Console.WriteLine("Enter the port to listen on:");
            int listenPort = Int32.Parse(Console.ReadLine());

            ServerController serverController = new ServerController();
            serverController.StartServer(ipString, listenPort);
        }
    }
}
