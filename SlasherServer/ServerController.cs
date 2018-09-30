using SlasherServer.Game;
using SlasherServer.Interfaces;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SlasherServer
{
    public class ServerController
    {
        public static GameHandler game;

        public ServerController()
        {
            game = new GameHandler();
        }

        public void StartServer(string ipString, int listenPort)
        {
            var serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(new IPEndPoint(IPAddress.Parse(ipString), listenPort));
            serverSocket.Listen(25);

            ClientHandler.Initialize();
            Thread receiveClientsThread = new Thread(() => ClientHandler.ListenForConnections(serverSocket));

            receiveClientsThread.Start();

            ListenForCommands();
        }

        private static void ListenForCommands()
        {
            Console.WriteLine();
            Thread.Sleep(100);

            Console.Write("Starting commands console");
            Thread.Sleep(1000);
            Console.Write(".");
            Thread.Sleep(1000);
            Console.Write(".");
            Thread.Sleep(1000);
            Console.Write(".");
            Thread.Sleep(1000);

            Console.WriteLine();
            Console.WriteLine();

            Console.WriteLine("Console startup completed!");
            Console.WriteLine("Type \"Help\" to list all available commands");

            while (true)
            {
                Console.WriteLine();
                Console.WriteLine("Enter a command, please:");
                string input = Console.ReadLine();

                ParseCommand(input);
            }
        }

        private static void StartMatch()
        {
            Console.WriteLine("Starting match!");

            ClientHandler.BroadcastMessage("A new game is starting, fight for your lives!");

            game.StartGame();

            while (!game.IsOver())
            {
                Thread.Sleep(200);
            }

            game.EndGame();

            ClientHandler.BroadcastMessage("\nThe game has ended! ");

            switch (game.MatchResult)
            {
                case "S":
                    ClientHandler.BroadcastMessage("Survivors win!");
                    break;
                case "M":
                    //Obtener cual monstruo gano
                    List<IPlayer> winner = game.GetWinners();
                    ClientHandler.BroadcastMessage(string.Format("Monster {0} wins!", winner[0].Id));
                    break;
                default:
                    ClientHandler.BroadcastMessage("/nNobody wins! Everyone loses! \n\nGit gud guys come on :^)");
                    break;
            }
        }

        private static void ParseCommand(string input)
        {
            switch (input.ToLower())
            {
                case "help":
                    Console.WriteLine("List all commands");
                    break;
                case "listusers":
                    foreach (var user in ClientHandler.Users)
                    {
                        Console.WriteLine(user.Nickname);
                    }
                    break;
                case "listloggedusers":
                    foreach (var pair in ClientHandler.LoggedUsers)
                    {
                        Console.WriteLine(pair.Value.Nickname);
                    }
                    break;
                case "startgame":
                    StartMatch();
                    break;
                default:
                    break;
            }
        }
    }

}