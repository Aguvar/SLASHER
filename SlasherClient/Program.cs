using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SlasherClient
{
    class Program
    {
        static void Main(string[] args)
        {
            string ipString = ConfigurationManager.AppSettings["ipaddress"];
            string serverIpAddress = ConfigurationManager.AppSettings["serveripaddress"];

            Console.WriteLine("---Slasher Client V.0.01---");
            Console.WriteLine();

            Console.WriteLine("Enter the port to bind the client to:");
            int clientPort = Int32.Parse(Console.ReadLine());

            Console.WriteLine("Enter the server's port:");
            int serverPort = Int32.Parse(Console.ReadLine());

            ClientController clientController = new ClientController();
            clientController.ConnectToServer(ipString, serverIpAddress, clientPort, serverPort);
        }
    }
}
