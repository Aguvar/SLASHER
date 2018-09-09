using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlasherServer
{
    class Program
    {
        static void Main(string[] args)
        {
            bool condicion1 = true;
            bool condicion2 = true;
            bool condicion3 = true;
            //No lo hagas asi
            if (condicion1)
            {
                if (condicion2)
                {
                    if (condicion3)
                    {
                        Console.WriteLine("el triangulo esta bien");
                    }
                    else
                    {
                        Console.WriteLine("el triangulo esta mal");
                    }
                }
                else
                {
                    Console.WriteLine("el triangulo esta mal");
                }
            }
            else
            {
                Console.WriteLine("el triangulo esta mal");
            }
            //Hacelo asi
            if (condicion1 && condicion2 && condicion3)
            {
                Console.WriteLine("el triangulo esta bien");
            }
            else
            {
                Console.WriteLine("el triangulo esta mal");
            }
        }
    }
}
