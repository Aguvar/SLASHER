using SlasherServer.Authentication;
using SlasherServer.Game;
using SlasherServer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SlasherServer
{
    public class ServerController
    {
        private const string helpString = "Listing all commands:\nlistusers - List all registered users\nlistloggedusers - List all currently logged users\nstartgame - Starts a new Slasher game. Disables all other commands for the duration of the game\nexit - Closes all currently active connections and shuts down the server";
        public static GameHandler game;
        private static bool terminateConsole = false;
        private Socket listeningSocket;

        private static ISlasherLogger logger;

        public ServerController()
        {
            logger = new MsmqLogger();
            game = new GameHandler();
        }

        public void StartServer(string ipString, int listenPort)
        {
            var serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(new IPEndPoint(IPAddress.Parse(ipString), listenPort));
            listeningSocket = serverSocket;
            serverSocket.Listen(25);

            ClientHandler.Initialize();
            Thread receiveClientsThread = new Thread(() => ClientHandler.ListenForConnections(serverSocket));

            receiveClientsThread.Start();

            ListenForCommands();

            listeningSocket.Close();

            Console.WriteLine();
            Console.WriteLine("The console will now terminate");
            Console.WriteLine();
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
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

            while (!terminateConsole)
            {
                Console.WriteLine();
                Console.WriteLine("Enter a command, please:");
                string input = Console.ReadLine();

                ParseCommand(input);
            }
        }

        private static void ExecuteGame()
        {
            Console.WriteLine();
            Console.WriteLine("Starting game!");

            ClientHandler.BroadcastMessage("\nA new game is starting, fight for your lives!");

            game.StartGame();
            logger.LogMatchLine("A new match has started");

            while (!game.IsOver())
            {
                Thread.Sleep(200);
            }

            game.EndGame();
            logger.LogMatchLine("The match has ended");
            logger.FinishMatchLog();

            Console.WriteLine();
            Console.WriteLine("The game has ended!");

            ClientHandler.BroadcastMessage("\nThe game has ended! ");

            List<IPlayer> winners = game.Winners;

            SubmitPlayerStatistics(ClientHandler.Users, game.Players, winners);

            switch (game.MatchResult)
            {
                case "s":
                    ClientHandler.BroadcastMessage("Survivors win!");
                    ClientHandler.BroadcastMessage("These are the brave warriors who survived:");
                    foreach (var winner in winners)
                    {
                        if (ClientHandler.LoggedUsers.Keys.Contains(winner.Id))
                        {
                            var name = ClientHandler.LoggedUsers[winner.Id].Nickname;
                            ClientHandler.BroadcastMessage(name);
                        }
                    }
                    break;
                case "m":
                    //Obtener cual monstruo gano
                    var monsterName = "X";
                    if (ClientHandler.LoggedUsers.Keys.Contains(winners[0].Id))
                    {
                        monsterName = ClientHandler.LoggedUsers[winners[0].Id].Nickname;
                    }
                    ClientHandler.BroadcastMessage(string.Format("Monster {0} wins!", monsterName));
                    break;
                default:
                    ClientHandler.BroadcastMessage("\nNobody wins! Everyone loses! \n\nGit gud guys come on :^)");
                    break;
            }
        }

        private static void SubmitPlayerStatistics(List<User> users, List<IPlayer> players, List<IPlayer> winners)
        {
            foreach (var player in players)
            {
                string nickname = player.Nickname;
                bool winner = winners.Exists(w => w.Id.Equals(player.Id));

                logger.LogPlayerMatchResult(nickname, player.GetPlayerType(), winner);
                logger.LogPlayerScore(nickname, player.Score, player.GetPlayerType());
            }
        }

        private static void ParseCommand(string input)
        {
            switch (input.ToLower())
            {
                case "help":
                    PrintHelp();
                    break;
                case "listusers":
                    PrintRegisteredUsers();
                    break;
                case "listloggedusers":
                    PrintLoggedUsers();
                    break;
                case "startgame":
                    ExecuteGame();
                    break;
                case "exit":
                    ExitRoutine();
                    break;
                default:
                    break;
            }
        }

        private static void PrintLoggedUsers()
        {
            foreach (var pair in ClientHandler.LoggedUsers)
            {
                Console.WriteLine(pair.Value.Nickname);
            }
        }

        private static void PrintRegisteredUsers()
        {
            foreach (var user in ClientHandler.Users)
            {
                Console.WriteLine(user.Nickname);
            }
        }

        private static void PrintHelp()
        {
            Console.WriteLine();
            Console.WriteLine(helpString);
        }

        private static void ExitRoutine()
        {
            ClientHandler.BroadcastMessage("The server is closing");

            foreach (var connection in ClientHandler.ActiveConnections.Values)
            {
                connection.Close();
            }
            terminateConsole = true;
        }
    }

}