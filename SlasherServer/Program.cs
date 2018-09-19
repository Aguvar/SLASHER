using SlasherServer.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SlasherServer
{
    class Program
    {
        private const string IpString = "192.168.0.50";

        private static List<User> Users;
        private static Dictionary<Guid, User> LoggedUsers;

        static void Main(string[] args)
        {
            Users = new List<User>();
            LoggedUsers = new Dictionary<Guid, User>();

            Console.WriteLine("---Slasher Server V.0.01---");
            Console.WriteLine();
            Console.WriteLine("Enter the port to listen on:");
            int listenPort = Int32.Parse(Console.ReadLine());

            var serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(new IPEndPoint(IPAddress.Parse(IpString), listenPort));
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
                    foreach (var user in Users)
                    {
                        Console.WriteLine(user.Nickname);
                    }
                    break;
                case "listloggedusers":
                    foreach (var pair in LoggedUsers)
                    {
                        Console.WriteLine(pair.Value.Nickname);
                    }
                    break;
                default:
                    break;
            }
        }

        private static void HandleClientConnection(Socket client)
        {
            Guid socketId = Guid.NewGuid();

            while (true)
            {
                var msgLength = new byte[4];
                client.Receive(msgLength);

                int msgLengthInt = BitConverter.ToInt32(msgLength, 0);
                var msgBytes = new byte[msgLengthInt];
                client.Receive(msgBytes);
                //Console.WriteLine(Encoding.ASCII.GetString(msgBytes));
                string message = Encoding.ASCII.GetString(msgBytes);

                string response = ParseClientMessage(message, socketId);

                if (!string.IsNullOrWhiteSpace(response))
                {
                    SendMessageToClient(client, response);
                }
            }
        }

        private static string ParseClientMessage(string message, Guid socketId)
        {
            string[] commandArray = message.Split('#');

            switch (commandArray[0].ToLower())
            {
                case "signup":
                    Users.Add(new User()
                    {
                        Nickname = commandArray[1],
                        PhotoRoute = commandArray[2]
                    });
                    return string.Format("consoleprint#User {0} was registered!", commandArray[1]);
                case "login":
                    string nickname = commandArray[1];
                    if (Users.Any(u => u.Nickname.Equals(nickname)))
                    {
                        if (!LoggedUsers.Any(u => u.Value.Nickname.Equals(nickname)))
                        {
                            User userToLog = Users.Where(u => u.Nickname.Equals(nickname)).First();
                            LoggedUsers.Add(socketId, userToLog);
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
                    if (LoggedUsers.ContainsKey(socketId))
                    {
                        LoggedUsers.Remove(socketId);
                        return "loggedout";
                    }
                    else
                    {
                        return "consoleprint#You are not logged in, willy nilly";
                    }
                default:
                    return "";
            }
            throw new InvalidOperationException("An unknown command was received.");
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
