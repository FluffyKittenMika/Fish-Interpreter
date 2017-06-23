using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fish
{
    class Program
    {
        static void Main(string[] args)
        {
            string input;
            Fish fish;
            while (true)
            {
                input = Console.ReadLine();
                fish = new Fish(input);
            }
            
        }
    }
}
