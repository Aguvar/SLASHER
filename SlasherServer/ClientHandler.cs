using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using SlasherServer.Authentication;
using SlasherServer.Game;
using SlasherServer.Interfaces;

namespace SlasherServer
{
    public static class ClientHandler
    {
        public static List<User> Users {get; set;}
        public static Dictionary<Guid, User> LoggedUsers {get; set;}
        public static Dictionary<Guid, Socket> ActiveConnections {get; set;}

        public static void Initialize()
        {
            Users = new List<User>();
            LoggedUsers = new Dictionary<Guid, User>();
            ActiveConnections = new Dictionary<Guid, Socket>();
        }

        public static void ListenForConnections(Socket serverSocket)
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
                    clientConnection.Receive(msgLength);

                    int msgLengthInt = BitConverter.ToInt32(msgLength, 0);
                    var msgBytes = new byte[msgLengthInt];

                    clientConnection.Receive(msgBytes);

                    string message = Encoding.ASCII.GetString(msgBytes);

                    string response = ParseMessage(message, socketId, clientConnection);

                    if (!string.IsNullOrWhiteSpace(response))
                    {
                        SendMessage(clientConnection, response);
                    }
                }
                catch (SocketException)
                {
                    connectionActive = false;
                }
            }

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
            string firstArgument = commandArray[1];      //either nickname, character type or movements to perform 
            string secondArgument = commandArray[2];
            string response = string.Empty;

            switch (commandArray[0].ToLower())
            {
                case "signup":
                    response = SignUpUser(clientConnection, firstArgument, secondArgument);
                    return response;
                case "login":
                    response = LogIn(clientConnection, socketId, firstArgument);
                    return response;
                case "logout":
                    response = LogOut(socketId);
                    return response;
                case "joingame":
                    response = JoinGame(socketId, firstArgument);
                    return response;
                case "move":
                    response = MoveCharacter(socketId, firstArgument);
                    return response;
                case "attack":
                    response = PerformAttack(socketId);
                    return response;
            }
            throw new InvalidOperationException("An unknown command was received.");
        }

        private static string PerformAttack(Guid socketId)
        {
            throw new NotImplementedException();
        }

        private static string SignUpUser(Socket clientConnection, string nickname, string imageFormat)
        {
            string currentExecutionDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string avatarRoute = Path.Combine(currentExecutionDir, string.Format("Avatars/{0}Avatar.{1}", nickname, imageFormat));

            Users.Add(new User()
            {
                Nickname = nickname,
                AvatarRoute = avatarRoute
            });

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
                    return "consoleprint#The user is already logged in, please select another firstCommand";
                }
            }
            else
            {
                return "consoleprint#The firstCommand does not exist";
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
        }

        private static string MoveCharacter(Guid socketId, string movements)
        {
            GameHandler game = GetGame();

            if (game.MatchOngoing)
            {
                IPlayer player = game.GetPlayer(socketId);
                if (player != null)
                {
                    if (game.Move(player, ParsePlayerMovement(movements)))
                    {
                        Position position = game.GetPlayerPosition(player);
                        return string.Format("consoleprint#{0}", game.GetPlayerSurroundings(position, 1));
                    }
                    else
                    {
                        Position position = game.GetPlayerPosition(player);
                        return string.Format("consoleprint#{0}\n{1}", game.GetPlayerSurroundings(position, 1), "Your way was blocked by something.");
                    }
                }
                else
                {
                    return string.Format("consoleprint#{0}", "Please join a game before moving your character");
                }
            }
            return string.Format("consoleprint#{0}", "There is no ongoing game.");
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

        private static GameHandler GetGame()
        {
            return ServerController.game;
        }
    }
}
