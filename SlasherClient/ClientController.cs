using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SlasherClient
{
    public class ClientController
    {
        private const string UP_MOVEMENT_ARG = "w";
        private const string LEFT_MOVEMENT_ARG = "a";
        private const string RIGHT_MOVEMENT_ARG = "d";
        private const string DOWN_MOVEMENT_ARG = "s";
        private const string ATTACK_COMMAND = "attack";
        private const string MOVE_COMMAND = "move";

        private static bool terminateClient = false;

        private static bool loggedIn = false;

        private static Socket serverConnection;


        public void ConnectToServer(string ipString, string serverIpAddress, int clientPort, int serverPort)
        {
            try
            {
                serverConnection = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                serverConnection.Bind(new IPEndPoint(IPAddress.Parse(ipString), clientPort));

                serverConnection.Connect(new IPEndPoint(IPAddress.Parse(serverIpAddress), serverPort));
                Console.WriteLine("Connected to server!");

                Thread receiveThread = new Thread(() => ReceiveMsgService(serverConnection));

                receiveThread.Start();

                Console.WriteLine();
                Console.WriteLine("Type \"Help\" to list all available commands");

                //Procesar input
                while (!terminateClient)
                {
                    Console.WriteLine();
                    Console.WriteLine("Enter your commands!");
                    string input = Console.ReadLine();

                    ParseCommand(input);
                }

                serverConnection.Close();

            }
            catch (SocketException)
            {
                Console.WriteLine();
                Console.WriteLine("Could not connect to server.");
                Console.WriteLine("The client will now terminate.");
                terminateClient = true;
            }

            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        private static void ReceiveMsgService(Socket client)
        {
            while (!terminateClient)
            {
                try
                {
                    var msgLength = new byte[4];
                    client.Receive(msgLength);

                    var msgBytes = new byte[BitConverter.ToInt32(msgLength, 0)];
                    client.Receive(msgBytes);

                    string message = Encoding.ASCII.GetString(msgBytes);

                    ParseServerMessage(message);
                }
                catch (SocketException)
                {
                    Console.WriteLine();
                    Console.WriteLine("Connection to server lost");
                    Console.WriteLine("The client will now terminate");
                    terminateClient = true;
                }
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
            string[] command = input.Trim().ToLower().Split(' ');

            if (command.Length <= 0 || command.Length > 3)
            {
                Console.WriteLine("Invalid input");
                return;
            }

            switch (command[0])
            {
                case MOVE_COMMAND:
                    string commandToSend = "move#";
                    for (int i = 1; i < command.Length; i++)
                    {
                        switch (command[i])
                        {
                            case UP_MOVEMENT_ARG:
                                commandToSend += UP_MOVEMENT_ARG;
                                break;
                            case LEFT_MOVEMENT_ARG:
                                commandToSend += LEFT_MOVEMENT_ARG;
                                break;
                            case RIGHT_MOVEMENT_ARG:
                                commandToSend += RIGHT_MOVEMENT_ARG;
                                break;
                            case DOWN_MOVEMENT_ARG:
                                commandToSend += DOWN_MOVEMENT_ARG;
                                break;
                            default:
                                Console.WriteLine("Invalid input");
                                return;
                        }
                    }
                    SendMessageToServer(serverConnection, commandToSend);
                    break;
                case ATTACK_COMMAND:
                    SendMessageToServer(serverConnection, "attack");
                    break;
                case "joingame":
                    JoinOngoingGame();
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
                case "help":
                    PrintHelp();
                    break;
                case "exit":
                    ExitRoutine();
                    break;
                default:
                    Console.WriteLine("Invalid input");
                    return;
            }
        }

        private static void PrintHelp()
        {
            Console.WriteLine(
                "\nAvailable commands: " +
                "\nsignup - Register a user in the server" +
                "\nlogin - Login to the server" +
                "\nlogout - Logout from the server" +
                "\nhelp - Prints this very same text. Duh" +
                "\nexit - Closes the connection to the server and shuts down the client" +
                "\n\nGame controls:" +
                "\nattack - Self explanatory" +
                "\nmove <dir> <dir> - Move in a certain direction specified by the <dir> parameter. <dir> can be one of these: " +
                $"\n{UP_MOVEMENT_ARG} - Up" +
                $"\n{DOWN_MOVEMENT_ARG} - Down" +
                $"\n{LEFT_MOVEMENT_ARG} - Left" +
                $"\n{RIGHT_MOVEMENT_ARG} - Right" +
                "\n\nReading the map:" +
                "\n* - You, the player, a thinking entity. Hopefully" +
                "\nS - A survivor" +
                "\nM - A monster" +
                "\nB - A dead body" +
                "\nX - A wall" +
                "\n- - Grass, you can only move through grass");
        }

        private static void ExitRoutine()
        {
            terminateClient = true;
        }

        private static void JoinOngoingGame()
        {
            if (loggedIn)
            {
                Console.WriteLine();
                Console.WriteLine("Choose your side!");
                Console.WriteLine("-- Survivors --\nSurvive as a team, play as a team and die as a team. Cooperation is key to survive the hordes of monsters. Reduced health and attack but you can gang up on monsters with the help of other survivors. Defeat all of them and the whole team wins!");
                Console.WriteLine("-- Monsters --\nTo each his own. In this dog eat dog world only the biggest, meanest monsters survive. Feast on measly survivors and surpass other monsters to win! Higher health and attack but there can only be one winner! Good luck!");
                string choice = string.Empty;
                while (choice != "s" && choice != "m")
                {
                    Console.WriteLine();
                    Console.WriteLine("Your choice? (S/M)");
                    choice = Console.ReadLine().ToLower();
                }

                SendMessageToServer(serverConnection, string.Format("joingame#{0}", choice));
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine("You need to be logged in to the server to perform that action.");
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
                Console.WriteLine("Enter your avatar's route:");
                photoRoute = Console.ReadLine();


            }

            var filepathArray = photoRoute.Split('.');
            string imageFormat = filepathArray[filepathArray.Length - 1];

            string command = string.Format("signup#{0}#{1}", nickname, imageFormat);


            try
            {
                //Send image
                using (FileStream stream = new FileStream(photoRoute, FileMode.Open))
                {
                    SendMessageToServer(serverConnection, command);

                    long imageSize = stream.Length;
                    long pieces = (imageSize + 1024 - 1) / 1024;

                    byte[] bytePieces = BitConverter.GetBytes(pieces);
                    int piecesPointer = 0;
                    while (piecesPointer < 8)
                    {
                        var sentBytes = serverConnection.Send(bytePieces, piecesPointer, 8 - piecesPointer, SocketFlags.None);
                        if (sentBytes == 0)
                        {
                            throw new SocketException();
                        }
                        piecesPointer += sentBytes;
                    }

                    byte[] imageBuffer = new byte[1024];
                    while (stream.Read(imageBuffer, 0, imageBuffer.Length) > 0)
                    {
                        int length = imageBuffer.Count();
                        int posLength = 0;
                        byte[] byteLength = BitConverter.GetBytes(length);
                        while (posLength < 4)
                        {
                            var sentBytes = serverConnection.Send(byteLength, posLength, 4 - posLength, SocketFlags.None);
                            if (sentBytes == 0)
                            {
                                throw new SocketException();
                            }
                            posLength += sentBytes;
                        }
                        var pos = 0;
                        while (pos < length)
                        {
                            var sentBytes = serverConnection.Send(imageBuffer, pos, length - pos, SocketFlags.None);
                            if (sentBytes == 0)
                            {
                                throw new SocketException();
                            }
                            pos += sentBytes;
                        }
                    }
                }
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("Could not open the specified file");
            }
        }
    }
}