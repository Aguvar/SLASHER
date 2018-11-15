using System;

namespace SlasherServer
{
    class Program
    {
        static void Main(string[] args)
        {
            

            Console.WriteLine("---Slasher Server V.0.01---");
            Console.WriteLine();

            Console.WriteLine("Enter the port to listen on:");
            int listenPort = Int32.Parse(Console.ReadLine());

            ServerController serverController = new ServerController();
            serverController.StartServer(listenPort);
        }
    }
}
