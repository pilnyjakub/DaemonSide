using System;
using System.Threading.Tasks;


namespace DaemonSide
{
    class Program
    {
        static void Main(string[] args)
        {
            Check ch = new Check();
            //prihlaseni
            while (true)
            {
                Console.WriteLine("Please log in.\n");
                if(ch.Login()) { break; }
                Console.Clear();
            }

            //nastavení id
            ch.Id();
            ch.UpdatePc();

            //while
            while (true)
            {
                Console.Clear();
                ch.UpdatePc();
                if (Pc.Instance.State == "blocked") { break; }
                Console.WriteLine("ID: " + Pc.Instance.Id);
                Console.WriteLine("OS/Name: " + Pc.Instance.OS + '/' + Pc.Instance.Name);
                Console.WriteLine("IP Address: " + Pc.Instance.IpAddress);
                Console.WriteLine("MAC Address: " + Pc.Instance.MacAddress);
                Console.WriteLine("\nLast Update: " + DateTime.Now);
                ch.DoBackup();
                Console.WriteLine("\nPress any key to update.");
                Task.Factory.StartNew(() => Console.ReadKey()).Wait(TimeSpan.FromSeconds(600.0));
            }
            Console.WriteLine("Your computer has been blocked, please contact the administrator.");
            Console.ReadKey();
        }
    }
}
