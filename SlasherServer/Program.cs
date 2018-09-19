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

        static void Main(string[] args)
        {
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
                default:
                    break;
            }
        }

        private static void ReceiveMsgService(Socket client)
        {
            while (true)
            {
                var msgLength = new byte[4];
                client.Receive(msgLength);

                int msgLengthInt = BitConverter.ToInt32(msgLength, 0);
                var msgBytes = new byte[msgLengthInt];
                client.Receive(msgBytes);
                Console.WriteLine(Encoding.ASCII.GetString(msgBytes));
            }
        }

        private static void SendMsgService(Socket client)
        {
            while (true)
            {
                var msg = Console.ReadLine();
                var byteMsg = Encoding.ASCII.GetBytes(msg);

                var length = byteMsg.Count();
                var posLength = 0;
                byte[] byteLength = BitConverter.GetBytes(length);
                while (posLength < 4)
                {
                    var sent = client.Send(byteLength, posLength, 4 - posLength, SocketFlags.None);
                    if (sent == 0)
                    {
                        throw new SocketException();
                    }
                    posLength += sent;
                }
                var pos = 0;
                while (pos < length)
                {
                    var sent = client.Send(byteMsg, pos, length - pos, SocketFlags.None);
                    if (sent == 0)
                    {
                        throw new SocketException();
                    }
                    pos += sent;
                }
            }
        }

        private static void ReceiveClients(Socket serverSocket)
        {
            Console.WriteLine("Listening for clients...");
            while (true)
            {
                var client = serverSocket.Accept();
                Console.WriteLine("Client Connected!");

                Thread sendThread = new Thread(() => SendMsgService(client));
                Thread receiveThread = new Thread(() => ReceiveMsgService(client));

                sendThread.Start();
                receiveThread.Start();
            }
        }
    }
}
