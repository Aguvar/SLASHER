using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SlasherClient
{
    class Program
    {
        private const string UP_MOVEMENT_ARG = "N";
        private const string LEFT_MOVEMENT_ARG = "W";
        private const string RIGHT_MOVEMENT_ARG = "E";
        private const string DOWN_MOVEMENT_ARG = "S";
        private const string ATTACK_COMMAND = "attack";
        private const string MOVE_COMMAND = "move";

        private const string IpString = "192.168.0.50";

        static void Main(string[] args)
        {
            Console.WriteLine("---Slasher Client V.0.01---");

            //Conexion a cliente
            //Crear thread para recibir avisos del servidor
            Console.WriteLine("Enter the port to bind the client to:");
            int clientPort = Int32.Parse(Console.ReadLine());

            Console.WriteLine("Enter the server's port:");
            int serverPort = Int32.Parse(Console.ReadLine());

            var client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            client.Bind(new IPEndPoint(IPAddress.Parse(IpString), clientPort));

            client.Connect(new IPEndPoint(IPAddress.Parse(IpString), serverPort));
            Console.WriteLine("Connected to server!");

            Thread sendThread = new Thread(() => SendMsgService(client));
            Thread receiveThread = new Thread(() => ReceiveMsgService(client));

            sendThread.Start();
            receiveThread.Start();

            //Procesar input
            while (true)
            {
                Console.WriteLine("Enter your commands!");
                string input = Console.ReadLine();

                ParseCommand(input);
            }
        }

        private static void ReceiveMsgService(Socket client)
        {
            while (true)
            {
                var msgLength = new byte[4];
                client.Receive(msgLength);

                var msgBytes = new byte[BitConverter.ToInt32(msgLength, 0)];
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

        private static void ParseCommand(string input)
        {
            string[] command = input.ToLower().Split(' ');

            if (command.Length <= 0 || command.Length > 3)
            {
                Console.WriteLine("Invalid input");
                return;
            }

            switch (command[0])
            {
                case MOVE_COMMAND:
                    //Manejar el movimiento
                    for (int i = 1; i < command.Length; i++)
                    {
                        switch (command[i])
                        {
                            case UP_MOVEMENT_ARG:
                                break;
                            case LEFT_MOVEMENT_ARG:
                                break;
                            case RIGHT_MOVEMENT_ARG:
                                break;
                            case DOWN_MOVEMENT_ARG:
                                break;
                            case "D":
                                break;
                            default:
                                Console.WriteLine("Invalid input");
                                return;
                        }
                    }
                    break;
                case ATTACK_COMMAND:
                    //Manejar el ataque
                    Console.WriteLine("You attack");
                    break;
                default:
                    Console.WriteLine("Invalid input");
                    return;
            }
        }
    }
}
