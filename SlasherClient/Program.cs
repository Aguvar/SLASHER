using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlasherClient
{
    class Program
    {
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

            if (command.Length <= 0 || command.Length > 3)
            {
                Console.WriteLine("Invalid input");
                return;
            }

            switch (command[0])
            {
                case "move":
                    //Manejar el movimiento
                    for (int i = 1; i < command.Length; i++)
                    {
                        switch (command[i])
                        {
                            case "W":
                                break;
                            case "A":
                                break;
                            case "S":
                                break;
                            case "D":
                                break;
                            default:
                                Console.WriteLine("Invalid input");
                                return;
                        }
                    }
                    break;
                case "attack":
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
