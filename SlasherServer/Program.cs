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

            game = new GameHandler();

            users = new List<User>();
            loggedUsers = new Dictionary<Guid, User>();
            activeConnections = new Dictionary<Guid, Socket>();

            Console.WriteLine("---Slasher Server V.0.01---");
            Console.WriteLine();

            Console.WriteLine("Enter the port to listen on:");
            int listenPort = Int32.Parse(Console.ReadLine());

            var serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(new IPEndPoint(IPAddress.Parse(ipString), listenPort));
            serverSocket.Listen(25);

            Thread receiveClientsThread = new Thread(() => ReceiveClients(serverSocket));
            receiveClientsThread.Start();

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

        private static void ParseCommand(string input)
        {
            switch (input.ToLower())
            {
                case "help":
                    Console.WriteLine("List all commands");
                    break;
                case "listusers":
                    foreach (var user in users)
                    {
                        Console.WriteLine(user.Nickname);
                    }
                    break;
                case "listloggedusers":
                    foreach (var pair in loggedUsers)
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

        private static void StartMatch()
        {
            Console.WriteLine("Starting match!");

            BroadcastMessage("A new game is starting, fight for your lives!");

            game.StartGame();

            while (!game.IsOver())
            {
                Thread.Sleep(200);
            }

            game.EndGame();

            BroadcastMessage("\nThe game has ended! ");

            switch (game.Result)
            {
                case "S":
                    BroadcastMessage("Survivors win!");
                    break;
                case "M":
                    //Obtener cual monstruo gano
                    List<IPlayer> winner = game.GetWinners();
                    BroadcastMessage(string.Format("Monster {0} wins!", winner[0].Id));
                    break;
                default:
                    BroadcastMessage("/nNobody wins! Everyone loses! \n\nGit gud guys come on :^)");
                    break;
            }
        }

        private static void BroadcastMessage(string message)
        {
            foreach (Socket connection in activeConnections.Values)
            {
                string printCommand = string.Format("consoleprint#{0}", message);
                SendMessageToClient(connection, printCommand);
            }
        }

        private static void HandleClientConnection(Socket clientConnection)
        {
            bool connectionActive = true;
            Guid socketId = Guid.NewGuid();
            activeConnections.Add(socketId, clientConnection);

            while (connectionActive)
            {
                try
                {
                    var msgLength = new byte[4];
                    clientConnection.Receive(msgLength);

                    int msgLengthInt = BitConverter.ToInt32(msgLength, 0);
                    var msgBytes = new byte[msgLengthInt];

                    clientConnection.Receive(msgBytes);

                    string message = Encoding.ASCII.GetString(msgBytes);

                    string response = ParseClientMessage(message, socketId, clientConnection);

                    if (!string.IsNullOrWhiteSpace(response))
                    {
                        SendMessageToClient(clientConnection, response);
                    }
                }
                catch (SocketException)
                {
                    connectionActive = false;
                }
            }

            Console.WriteLine(string.Format("Connection with client {0} has been terminated.", socketId.ToString()));
        }

        private static string ParseClientMessage(string message, Guid socketId, Socket clientConnection)
        {
            string[] commandArray = message.Split('#');

            switch (commandArray[0].ToLower())
            {
                case "signup":

                    string currentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    string avatarRoute = Path.Combine(currentDir, string.Format("Avatars/{0}Avatar.{1}", commandArray[1], commandArray[2]));

                    users.Add(new User()
                    {
                        Nickname = commandArray[1],
                        AvatarRoute = avatarRoute
                    });

                    using (FileStream stream = new FileStream(avatarRoute, FileMode.Create))
                    {
                        byte[] imagePieces = new byte[8];
                        clientConnection.Receive(imagePieces);
                        long imagePiecesLong = BitConverter.ToInt64(imagePieces, 0);
                        int piecesReceived = 0;
                        int bytesReceived = 0;
                        while (piecesReceived < imagePiecesLong)
                        {
                            //Aca tenemos que hacer un while, para que vaya recibiendo las piezas de la imagen que pesan cada una a lo sumo 1024 bytes.
                            byte[] msgLength = new byte[4];
                            clientConnection.Receive(msgLength);

                            int msgLengthInt = BitConverter.ToInt32(msgLength, 0);
                            var msgBytes = new byte[msgLengthInt];
                            clientConnection.Receive(msgBytes);
                            //Escribir al stream
                            stream.Seek(bytesReceived, SeekOrigin.Begin);

                            stream.Write(msgBytes, 0, msgBytes.Length);

                            bytesReceived += msgBytes.Length;
                            piecesReceived++;
                        }
                    }

                    return string.Format("consoleprint#User {0} was registered!", commandArray[1]);
                case "login":
                    string nickname = commandArray[1];
                    if (users.Any(u => u.Nickname.Equals(nickname)))
                    {
                        if (!loggedUsers.Any(u => u.Value.Nickname.Equals(nickname)))
                        {
                            User userToLog = users.Where(u => u.Nickname.Equals(nickname)).First();
                            loggedUsers.Add(socketId, userToLog);
                            return "loggedin";
                        }
                        else
                        {
                            return "consoleprint#The user is already logged in, please select another nickname";
                        }
                    }
                    else
                    {
                        return "consoleprint#The nickname does not exist";
                    }
                case "logout":
                    if (loggedUsers.ContainsKey(socketId))
                    {
                        loggedUsers.Remove(socketId);
                        return "loggedout";
                    }
                    else
                    {
                        return "consoleprint#You are not logged in, willy nilly";
                    }
                case "joingame":
                    if (game.MatchOngoing)
                    {
                        if (!game.IsGameFull())
                        {
                            IPlayer player;

                            switch (commandArray[1])
                            {
                                case "s":
                                    player = new Survivor()
                                    {
                                        Id = socketId
                                    };
                                    break;
                                case "m":
                                    player = new Monster()
                                    {
                                        Id = socketId
                                    };
                                    break;
                                default:
                                    throw new InvalidOperationException("Something weird happened and I'm not really sure what to do. Player type argument was not in the expected values.");
                            }
                            game.AddPlayer(player);
                            Position position = game.GetPlayerPosition(player);
                            string surroundings = game.GetPlayerSurroundings(position, 1);

                            return string.Format("consoleprint#{0}\n\n{1}", surroundings, "Game joined! Enjoy your stay!");
                        }
                        else
                        {
                            return "consoleprint#The game is full now, should've joined earlier, bud.";
                        }
                    }
                    else
                    {
                        return "consoleprint#Hold your horses please, the match hasn't started yet.";
                    }
                case "move":
                    if (game.MatchOngoing)
                    {
                        IPlayer player = game.GetPlayer(socketId);
                        if (player != null)
                        {
                            if (game.Move(player, ParsePlayerMovement(commandArray[1])))
                            {
                                Position position = game.GetPlayerPosition(player);
                                return string.Format("consoleprint#{0}",game.GetPlayerSurroundings(position,1));
                            }
                            else
                            {
                                Position position = game.GetPlayerPosition(player);
                                return string.Format("consoleprint#{0}\n{1}", game.GetPlayerSurroundings(position, 1), "Your way was blocked by something.");
                            }

                        }
                    }
                    break;
                case "attack":

                    break;
                default:
                    return "";
            }
            throw new InvalidOperationException("An unknown command was received.");
        }

        private static Position ParsePlayerMovement(string movementString)
        {
            Position movement = new Position(0, 0);
            foreach (char direction in movementString.ToLower())
            {
                switch (direction)
                {
                    case 'w':
                        movement.Col += 1;
                        break;
                    case 's':
                        movement.Col -= 1;
                        break;
                    case 'a':
                        movement.Row -= 1;
                        break;
                    case 'd':
                        movement.Row += 1;
                        break;
                    default:
                        break;
                }
            }
            return movement;
        }

        private static void SendMessageToClient(Socket clientConnection, string message)
        {
            var byteMsg = Encoding.ASCII.GetBytes(message);

            var length = byteMsg.Count();
            var posLength = 0;
            byte[] byteLength = BitConverter.GetBytes(length);
            while (posLength < 4)
            {
                var sent = clientConnection.Send(byteLength, posLength, 4 - posLength, SocketFlags.None);
                if (sent == 0)
                {
                    throw new SocketException();
                }
                posLength += sent;
            }
            var pos = 0;
            while (pos < length)
            {
                var sent = clientConnection.Send(byteMsg, pos, length - pos, SocketFlags.None);
                if (sent == 0)
                {
                    throw new SocketException();
                }
                pos += sent;
            }
        }

        private static void ReceiveClients(Socket serverSocket)
        {
            Console.WriteLine("Listening for clients...");
            while (true)
            {
                var client = serverSocket.Accept();
                Console.WriteLine("Client Connected!");

                Thread receiveThread = new Thread(() => HandleClientConnection(client));

                receiveThread.Start();
            }
        }
    }
}
