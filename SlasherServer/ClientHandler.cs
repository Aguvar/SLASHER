using SlasherServer.Authentication;
using SlasherServer.Game;
using SlasherServer.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;

namespace SlasherServer
{
    public static class ClientHandler
    {
        public static List<User> Users { get; set; }
        public static Dictionary<Guid, User> LoggedUsers { get; set; }
        public static Dictionary<Guid, Socket> ActiveConnections { get; set; }

        private static object loginLock;
        private static object signupLock;

        private static ISlasherLogger logger;

        public static void Initialize()
        {
            Users = new List<User>();
            LoggedUsers = new Dictionary<Guid, User>();
            ActiveConnections = new Dictionary<Guid, Socket>();

            logger = new MsmqLogger();

            loginLock = new object();
            signupLock = new object();
        }

        public static void ListenForConnections(Socket serverSocket)
        {
            try
            {
                Console.WriteLine("Listening for clients...");
                while (true)
                {
                    var client = serverSocket.Accept();
                    Console.WriteLine("Client Connected!");

                    Thread receiveThread = new Thread(() => HandleConnection(client));

                    receiveThread.Start();
                }
            }
            catch (SocketException)
            {
                //Do nothing since it's just closing the console
                Debug.Print("The listening thread has closed");
            }
        }

        private static void HandleConnection(Socket clientConnection)
        {
            bool connectionActive = true;
            Guid socketId = Guid.NewGuid();
            ActiveConnections.Add(socketId, clientConnection);

            while (connectionActive)
            {
                try
                {
                    var msgLength = new byte[4];
                    int totalReceived = 0;
                    while (totalReceived < 4)
                    {
                        var currentlyReceived = clientConnection.Receive(msgLength, totalReceived, 4 - totalReceived, SocketFlags.None);
                        if (currentlyReceived == 0)
                        {
                            throw new SocketException();
                        }
                        totalReceived += currentlyReceived;
                    }

                    int msgLengthInt = BitConverter.ToInt32(msgLength, 0);
                    var msgBytes = new byte[msgLengthInt];
                    totalReceived = 0;
                    while (totalReceived < msgLengthInt)
                    {
                        var currentlyReceived = clientConnection.Receive(msgBytes, totalReceived, msgLengthInt - totalReceived, SocketFlags.None);
                        if (currentlyReceived == 0)
                        {
                            throw new SocketException();
                        }
                        totalReceived += currentlyReceived;
                    }

                    string message = Encoding.ASCII.GetString(msgBytes);

                    try
                    {
                        string response = ParseMessage(message, socketId, clientConnection);

                        if (!string.IsNullOrWhiteSpace(response))
                        {
                            SendMessage(clientConnection, response);
                        }
                    }
                    catch (InvalidOperationException)
                    {
                        //Just ignore the command and alert us
                        Debug.Print($"An unknown command from client {socketId} has been received");
                        Debug.Print($"Received message: {message}");
                    }
                }
                catch (SocketException)
                {
                    connectionActive = false;
                }
            }
            ActiveConnections.Remove(socketId);
            Console.WriteLine(string.Format("Connection with client {0} has been terminated.", socketId.ToString()));
        }

        public static void BroadcastMessage(string message)
        {
            foreach (Socket connection in ActiveConnections.Values)
            {
                string printCommand = string.Format("consoleprint#{0}", message);
                SendMessage(connection, printCommand);
            }
        }

        private static void SendMessage(Socket clientConnection, string message)
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

        private static string ParseMessage(string message, Guid socketId, Socket clientConnection)
        {
            string[] commandArray = message.Split('#');
            string response = string.Empty;

            switch (commandArray[0].ToLower())
            {
                case "signup":
                    string imageFormat = commandArray[2];
                    response = SignUpUser(clientConnection, commandArray[1], imageFormat);
                    return response;
                case "login":
                    response = LogIn(clientConnection, socketId, commandArray[1]);
                    return response;
                case "logout":
                    response = LogOut(socketId);
                    return response;
                case "joingame":
                    response = JoinGame(socketId, commandArray[1]);
                    return response;
                case "move":
                    response = MoveCharacter(socketId, commandArray[1]);
                    return response;
                case "attack":
                    response = PerformAttack(socketId);
                    return response;
            }
            throw new InvalidOperationException("An unknown command was received.");
        }

        private static string PerformAttack(Guid socketId)
        {
            GameHandler game = GetGame();

            IPlayer playerWhoAttacks = game.GetPlayerById(socketId);

            logger.LogMatchLine($"Player {playerWhoAttacks.Nickname} has attacked");

            if (!game.MatchOngoing)
            {
                return "consoleprint#Woah woah, there's no game running right now, chill.";
            }

            if (playerWhoAttacks == null)
            {
                return "consoleprint#You haven't joined the game yet.";
            }

            if (playerWhoAttacks.Health == 0)
            {
                return "consoleprint#The dead cannot attack. Stay dead and stay down.";
            }

            game.ExecutePlayerAttack(playerWhoAttacks);

            return $"consoleprint#{GetPlayerStatusString(playerWhoAttacks)}";
        }

        private static string SignUpUser(Socket clientConnection, string nickname, string imageFormat)
        {
            string currentExecutionDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string avatarRoute = Path.Combine(currentExecutionDir, string.Format("Avatars/{0}Avatar.{1}", nickname, imageFormat));

            lock (signupLock)
            {
                if (Users.Exists(u => u.Nickname.Equals(nickname)))
                {
                    return string.Format("consoleprint#User {0} is already registered", nickname);
                }

                Users.Add(new User()
                {
                    Nickname = nickname,
                    AvatarRoute = avatarRoute
                });
            }

            using (FileStream stream = new FileStream(avatarRoute, FileMode.Create))
            {
                byte[] imagePiecesBuffer = new byte[8];
                clientConnection.Receive(imagePiecesBuffer);
                long imagePieces = BitConverter.ToInt64(imagePiecesBuffer, 0);
                int piecesReceived = 0;
                int bytesReceived = 0;
                //Receive image pieces, each one weights at most 1024 bytes.
                while (piecesReceived < imagePieces)
                {
                    byte[] msgLength = new byte[4];
                    clientConnection.Receive(msgLength);

                    int msgLengthInt = BitConverter.ToInt32(msgLength, 0);
                    var msgBytes = new byte[msgLengthInt];
                    clientConnection.Receive(msgBytes);

                    //Write to stream
                    stream.Seek(bytesReceived, SeekOrigin.Begin);
                    stream.Write(msgBytes, 0, msgBytes.Length);

                    bytesReceived += msgBytes.Length;
                    piecesReceived++;
                }
            }

            return string.Format("consoleprint#User {0} was registered!", nickname);
        }

        private static string LogIn(Socket clientConnection, Guid socketId, string firstCommand)
        {
            lock (loginLock)
            {
                if (Users.Any(u => u.Nickname.Equals(firstCommand)))
                {
                    if (!LoggedUsers.Any(u => u.Value.Nickname.Equals(firstCommand)))
                    {
                        User userToLog = Users.Where(u => u.Nickname.Equals(firstCommand)).First();
                        LoggedUsers.Add(socketId, userToLog);
                        return "loggedin";
                    }
                    else
                    {
                        return "consoleprint#The user is already logged in, please select another user";
                    }
                }
                else
                {
                    return "consoleprint#The user does not exist";
                }
            }
        }

        private static string LogOut(Guid socketId)
        {
            if (LoggedUsers.ContainsKey(socketId))
            {
                LoggedUsers.Remove(socketId);
                return "loggedout";
            }
            else
            {
                return "consoleprint#You are not logged in, willy nilly";
            }
        }

        private static string JoinGame(Guid socketId, string playerType)
        {
            GameHandler game = GetGame();

            string nickname = LoggedUsers[socketId].Nickname;

            if (game.MatchOngoing)
            {
                if (!game.IsGameFull())
                {
                    IPlayer player;

                    switch (playerType)
                    {
                        case "s":
                            player = new Survivor()
                            {
                                Id = socketId,
                                Nickname = nickname
                            };
                            break;
                        case "m":
                            player = new Monster()
                            {
                                Id = socketId,
                                Nickname = nickname
                            };
                            break;
                        default:
                            throw new InvalidOperationException("Something weird happened and I'm not really sure what to do. Player type argument was not in the expected values.");
                    }
                    game.AddPlayer(player);
                    Position position = game.GetPlayerPosition(player);
                    string surroundings = game.GetPlayerSurroundings(position, 1);

                    logger.LogMatchLine($"Player {player.Nickname} has joined the match");

                    return $"consoleprint#{surroundings}\n\nGame joined! Enjoy your stay!";
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
        }

        private static string MoveCharacter(Guid socketId, string movements)
        {
            GameHandler game = GetGame();

            if (game.MatchOngoing)
            {
                IPlayer player = game.GetPlayerById(socketId);

                logger.LogMatchLine($"Player {player.Nickname} has moved");

                if (player != null)
                {
                    if (player.Health == 0)
                    {
                        return ("consoleprint#The dead cannot move. This is not a zombie setting.");
                    }
                    if (game.Move(player, ParsePlayerMovement(movements)))
                    {
                        Position position = game.GetPlayerPosition(player);
                        return string.Format("consoleprint#{0}", GetPlayerStatusString(player));
                    }
                    else
                    {
                        Position position = game.GetPlayerPosition(player);
                        return string.Format("consoleprint#{0}\n{1}", GetPlayerStatusString(player), "Your way was blocked by something.");
                    }
                }
                else
                {
                    return string.Format("consoleprint#{0}", "Please join a game before moving your character");
                }
            }
            return string.Format("consoleprint#{0}", "There is no ongoing game.");
        }

        private static string GetPlayerStatusString(IPlayer player)
        {
            var game = GetGame();
            return string.Format("{0}\nCurrent Health: {1}", game.GetPlayerSurroundings(game.GetPlayerPosition(player), 1), player.Health);
        }

        private static Position ParsePlayerMovement(string movementString)
        {
            Position movement = new Position(0, 0);
            foreach (char direction in movementString.ToLower())
            {
                switch (direction)
                {
                    case 'w':
                        movement.Col -= 1;
                        break;
                    case 's':
                        movement.Col += 1;
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

        private static GameHandler GetGame()
        {
            return ServerController.game;
        }
    }
}
