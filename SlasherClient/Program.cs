using System;
using System.Configuration;

namespace SlasherClient
{
    class Program
    {
        static void Main(string[] args)
        {
            string ipString = ConfigurationManager.AppSettings["ipaddress"];
            string serverIpAddress = ConfigurationManager.AppSettings["serveripaddress"];

            Console.WriteLine("---Slasher Client V0.01---");

            Console.WriteLine(" _______  _        _______  _______           _______  _______ " );
            Console.WriteLine("(  ____ \\( \\      (  ___  )(  ____ \\|\\     /|(  ____ \\(  ____ )" );
            Console.WriteLine("| (    \\/| (      | (   ) || (    \\/| )   ( || (    \\/| (    )|" );
            Console.WriteLine("| (_____ | |      | (___) || (_____ | (___) || (__    | (____)|" );
            Console.WriteLine("(_____  )| |      |  ___  |(_____  )|  ___  ||  __)   |     __)" );
            Console.WriteLine("      ) || |      | (   ) |      ) || (   ) || (      | (\\ (   " );
            Console.WriteLine("/\\____) || (____/\\| )   ( |/\\____) || )   ( || (____/\\| ) \\ \\__" );
            Console.WriteLine("\\_______)(_______/|/     \\|\\_______)|/     \\|(_______/|/   \\__/  V0.01" );
            Console.WriteLine("                                                               ");

            try
            {
                Console.WriteLine("Enter the port to bind the client to:");
                int clientPort = Int32.Parse(Console.ReadLine());

                Console.WriteLine("Enter the server's port:");
                int serverPort = Int32.Parse(Console.ReadLine());

                ClientController clientController = new ClientController();
                clientController.ConnectToServer(ipString, serverIpAddress, clientPort, serverPort);
            }
            catch (FormatException)
            {
                Console.WriteLine();
                Console.WriteLine("Invalid port format");
                Console.WriteLine("The client will now terminate");
                Console.WriteLine();
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
            }
        }
    }
}
