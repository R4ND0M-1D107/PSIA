using System;
using UdpClientApp;

namespace UdpClientApp
{
    class MainClass
    {
        public static bool running = true;

        public static void Main(string[] args)
        {
            Sender sender = new Sender();
            sender.Start();
            
            while(running)
            {
                sender.Update();
            }

            Console.WriteLine("End");
        }
    }
}
