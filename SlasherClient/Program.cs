using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        static void Main(string[] args)
        {
            //Conexion a cliente
            //Crear thread para recibir avisos del servidor

            //Procesar input
            while (true)
            {
                string input = Console.ReadLine();

                ParseCommand(input);
            }
        }

        private static void ParseCommand(string input)
        {
            string[] command = input.ToLower().Split(' ');

            if (command.Length <=0 || command.Length > 3)
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
