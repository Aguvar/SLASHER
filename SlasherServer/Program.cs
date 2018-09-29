using SlasherServer.Authentication;
using SlasherServer.Game;
using SlasherServer.Interfaces;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SlasherServer
{
    class Program
    {
        private static List<User> users;
        private static Dictionary<Guid, User> loggedUsers;
        private static Dictionary<Guid, Socket> activeConnections;

        private static GameHandler game;

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
