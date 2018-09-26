using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SlasherClient
{
    //Para pasar imagenes usar Filestream y almacenar las imagenes en una carpeta del servidor

    class Program
    {
        private const string UP_MOVEMENT_ARG = "N";
        private const string LEFT_MOVEMENT_ARG = "W";
        private const string RIGHT_MOVEMENT_ARG = "E";
        private const string DOWN_MOVEMENT_ARG = "S";
        private const string ATTACK_COMMAND = "attack";
        private const string MOVE_COMMAND = "move";

        private static bool loggedIn = false;

        private static Socket serverConnection;

        static void Main(string[] args)
        {
            string ipString = ConfigurationManager.AppSettings["ipaddress"];

            Console.WriteLine("---Slasher Client V.0.01---");

            //Conexion a cliente
            //Crear thread para recibir avisos del servidor
            Console.WriteLine("Enter the port to bind the client to:");
            int clientPort = Int32.Parse(Console.ReadLine());

            Console.WriteLine("Enter the server's port:");
            int serverPort = Int32.Parse(Console.ReadLine());

            serverConnection = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverConnection.Bind(new IPEndPoint(IPAddress.Parse(ipString), clientPort));

            serverConnection.Connect(new IPEndPoint(IPAddress.Parse(ipString), serverPort));
            Console.WriteLine("Connected to server!");

            Thread receiveThread = new Thread(() => ReceiveMsgService(serverConnection));

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
                //Console.WriteLine(Encoding.ASCII.GetString(msgBytes));

                string message = Encoding.ASCII.GetString(msgBytes);

                ParseServerMessage(message);
            }
        }

        private static void ParseServerMessage(string message)
        {
            string[] commandArray = message.Split('#');

            switch (commandArray[0].ToLower())
            {
                case "consoleprint":
                    Console.WriteLine(commandArray[1]);
                    break;
                case "loggedin":
                    loggedIn = true;
                    Console.WriteLine("You are now logged in to the server!");
                    break;
                case "loggedout":
                    loggedIn = false;
                    Console.WriteLine("You are now logged out from the server!");
                    break;
                default:
                    break;
            }
        }

        private static void SendMessageToServer(Socket serverSocket, string message)
        {
            byte[] byteMsg = Encoding.ASCII.GetBytes(message);

            int length = byteMsg.Count();
            int posLength = 0;
            byte[] byteLength = BitConverter.GetBytes(length);
            while (posLength < 4)
            {
                var sent = serverSocket.Send(byteLength, posLength, 4 - posLength, SocketFlags.None);
                if (sent == 0)
                {
                    throw new SocketException();
                }
                posLength += sent;
            }
            var pos = 0;
            while (pos < length)
            {
                var sent = serverSocket.Send(byteMsg, pos, length - pos, SocketFlags.None);
                if (sent == 0)
                {
                    throw new SocketException();
                }
                pos += sent;
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
                case "signup":
                    SignupRoutine();

                    break;
                case "login":
                    LoginRoutine();
                    break;
                case "logout":
                    LogoutRoutine();
                    break;
                default:
                    Console.WriteLine("Invalid input");
                    return;
            }
        }

        private static void LogoutRoutine()
        {
            SendMessageToServer(serverConnection, string.Format("logout"));
        }

        private static void LoginRoutine()
        {
            Console.WriteLine();
            Console.WriteLine("Please enter which nickname you want to login with:");
            string nickname = Console.ReadLine();

            SendMessageToServer(serverConnection, string.Format("login#{0}", nickname));
        }

        private static void SignupRoutine()
        {
            string nickname = "";
            string errorMsg = "";
            while (string.IsNullOrEmpty(nickname))
            {
                Console.WriteLine(errorMsg);
                Console.WriteLine("Enter your desired nickname:");
                nickname = Console.ReadLine();
                if (string.IsNullOrEmpty(nickname))
                {
                    errorMsg = "Your nickname cannot be empty";
                }
            }
            errorMsg = "";
            string photoRoute = "";
            while (string.IsNullOrEmpty(photoRoute))
            {
                Console.WriteLine(errorMsg);
                Console.WriteLine("Enter the your avatar route:");
                photoRoute = Console.ReadLine();


            }

            var filepathArray = photoRoute.Split('.');
            string imageFormat = filepathArray[filepathArray.Length - 1];

            string command = string.Format("signup#{0}#{1}", nickname, imageFormat);

            SendMessageToServer(serverConnection, command);

            //Send image
            using (FileStream stream = new FileStream(photoRoute, FileMode.Open))
            {
                long imageSize = stream.Length;
                long pieces = (imageSize + 1024 - 1) / 1024;

                byte[] bytePieces = BitConverter.GetBytes(pieces);
                int piecesPointer = 0;
                while (piecesPointer < 8)
                {
                    var sent = serverConnection.Send(bytePieces, piecesPointer, 8 - piecesPointer, SocketFlags.None);
                    if (sent == 0)
                    {
                        throw new SocketException();
                    }
                    piecesPointer += sent;
                }

                byte[] imageBuffer = new byte[1024];
                while (stream.Read(imageBuffer, 0, imageBuffer.Length) > 0)
                {
                    int length = imageBuffer.Count();
                    int posLength = 0;
                    byte[] byteLength = BitConverter.GetBytes(length);
                    while (posLength < 4)
                    {
                        var sent = serverConnection.Send(byteLength, posLength, 4 - posLength, SocketFlags.None);
                        if (sent == 0)
                        {
                            throw new SocketException();
                        }
                        posLength += sent;
                    }
                    var pos = 0;
                    while (pos < length)
                    {
                        var sent = serverConnection.Send(imageBuffer, pos, length - pos, SocketFlags.None);
                        if (sent == 0)
                        {
                            throw new SocketException();
                        }
                        pos += sent;
                    }
                }
            }
        }
    }
}
