using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Threading;
using DamienG.Security.Cryptography;
using System.Diagnostics;
using System.Collections.Generic;


namespace UdpClientApp
{
    class Sender
    {
        const int localPort = 8002;
        const int remotePort = 8001;
        const string remoteAddress = "25.84.62.46";
        static string fileName;
        static string path;
        static bool running;

        const string RECEIVE_CONFIRM = "m1";
        const string RECEIVE_NULL = "m2";
        const int MAX_PACKET_SIZE = 1024;
        const int IDENTIFIER_SIZE = 1;
        const int CRC_SIZE = 10;

        const string NICKNAME =  "Meister351";

        static Sender program;

        //C:/Users/overlord/Documents/Unity/

        public void Start()
        {
                program = new Sender();
                Console.WriteLine("Enter '1' to send message");
                Console.WriteLine("Enter '2' to send file");
                Console.WriteLine("Enter '0' to exit");
                program.SendOnlineStatus();
        }

        public void Update()
        {
                string input = Console.ReadLine();
                int number = -1;
                Int32.TryParse(input, out number);

                switch (number)
                {
                    case 1:
                        number = -1;
                        program.SendMessage();
                        Console.WriteLine("Command done. Enter new command");
                        break;
                    case 2:
                        number = -1;
                        program.SendFile();
                        Console.WriteLine("Command done. Enter new command");
                        break;
                    case 0:
                        MainClass.running = false;
                        break;
                    default:
                        break;
                }
        }

        public byte[] encodePacket(char identifier, string packet)
        {
            byte[] messagePart = Encoding.ASCII.GetBytes(packet);
            UInt32 crc = Crc32.Compute(messagePart);
            byte[] crcBytes = BitConverter.GetBytes(crc);
            byte[] firstPart = Encoding.ASCII.GetBytes('[');
            byte[] secondPart = Encoding.ASCII.GetBytes(']' + identifier + packet);
            byte[] message = Combine(firstPart, crcBytes, secondPart);
            return message;
        }

        public static byte[] Combine(byte[] first, byte[] second, byte[] third)
        {
            byte[] ret = new byte[first.Length + second.Length + third.Length];
            Buffer.BlockCopy(first, 0, ret, 0, first.Length);
            Buffer.BlockCopy(second, 0, ret, first.Length, second.Length);
            Buffer.BlockCopy(third, 0, ret, first.Length + second.Length,
                             third.Length);
            return ret;
        }


        public byte[] encodePacket(char identifier, byte[] packet)
        {
            UInt32 crc = Crc32.Compute(packet);
            byte[] message = Encoding.ASCII.GetBytes(crc.ToString() + identifier + packet);
            return message;
        }
        
        public byte[] encodePacketOld(char identifier, string packet)
        {
            byte[] message = Encoding.ASCII.GetBytes(identifier + packet);
            return message;
        }

        bool trySendPacket( UdpClient sender, byte[] packet)
        {
            //setup confiramtion receiver
            IPEndPoint remoteIp = new IPEndPoint(IPAddress.Parse(remoteAddress), localPort);
            byte[] data = Encoding.ASCII.GetBytes(RECEIVE_NULL);
            UdpClient receiver = new UdpClient(localPort);

            //send packet
            sender.Send(packet, packet.Length, remoteAddress, remotePort);

            //start timer
            var waitTime = new TimeSpan(0, 0, 1);
            var waitUntil = DateTime.Now + waitTime;

            //wait for confiramation
            while (DateTime.Now <= waitUntil)
            {
                data = receiver.Receive(ref remoteIp);
                if (System.Text.Encoding.UTF8.GetString(data) != RECEIVE_NULL) //if received something
                {
                    receiver.Close();
                    if (System.Text.Encoding.UTF8.GetString(data) == RECEIVE_CONFIRM) // if received confirmation
                    {
                        Console.WriteLine("Confirm");
                        return true;
                    } else
                    {
                        Console.WriteLine("!Confirm");
                        return false;
                    }
                }
            }

            receiver.Close();
            Console.WriteLine("TimeOut");
            return false;
        }

        public void SendOnlineStatus()
        {
            UdpClient sender = new UdpClient();
            try
            {
                byte[] packet = encodePacketOld('s', NICKNAME);
                sender.Send(packet, packet.Length, remoteAddress, remotePort);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                sender.Close();
            }
        }

        public void SendMessage()
        {
            Console.WriteLine("Enter message : ");
            string messageStr = Console.ReadLine();
            UdpClient sender = new UdpClient();
            try
            {
                byte[] packet = encodePacketOld('m', messageStr);
                bool receivedCorrectly = false;
                while (!receivedCorrectly)
                {
                    receivedCorrectly = trySendPacket(sender, packet);  
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                sender.Close();
            }
        }

        public void SendFile()
        {
            Console.WriteLine("Enter file path : ");
            path = Console.ReadLine();
            Console.WriteLine("Enter file name : ");
            fileName = Console.ReadLine();

            UdpClient sender = new UdpClient();


            try
            {
                byte[] packet =  encodePacket('f', fileName);

                //send file name
                bool receivedCorrectly = false;
                while (!receivedCorrectly)
                {
                    receivedCorrectly = trySendPacket(sender, packet);
                }

                //open file
                FileStream file = File.OpenRead(path + fileName);
                byte[] buffer = new byte[MAX_PACKET_SIZE - CRC_SIZE - IDENTIFIER_SIZE];

                //send file
                while (file.Read(buffer, 0, buffer.Length) > 0)
                {
                    packet = encodePacket('p', buffer);

                    while (!receivedCorrectly)
                    {
                        receivedCorrectly = trySendPacket(sender, packet);
                    }
                }

                file.Close();

                //send EOF
                packet = encodePacket('e', fileName);

                receivedCorrectly = false;
                while (!receivedCorrectly)
                {
                    receivedCorrectly = trySendPacket(sender, packet);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                sender.Close();
            }
        }
    }
}