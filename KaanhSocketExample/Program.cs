using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KaanhSocketExample
{
    class Program
    {
        static void Main(string[] args)
        {
            KaanhSocket bot = new KaanhSocket("127.0.0.1", "5867");
            bot.Connect();
           // bot.Send("ds");
            while (true)
            {
                string str = Console.ReadLine();
                bot.Send(str);
            }
        }
    }
}
