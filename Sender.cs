using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;
using DamienG.Security.Cryptography;
using System.Security.Cryptography;
using System.Diagnostics;
using System.Threading;

namespace UdpClientApp
{
    class Sender
    {
        const int localPort = 8002;
        const int remotePort = 8001;
        const string remoteAddress = "25.84.62.46";

        const string RECEIVE_CONFIRM = "m1";
        const string RECEIVE_NULL = "m2";
        const int MAX_PACKET_SIZE = 1024;
        const int IDENTIFIER_SIZE = 1;
        const int CRC_SIZE = 4;

        const string NICKNAME =  "Meister351";

        static Sender program;
        static int Timeout;

        //C:/Users/overlord/Documents/Unity/Gunmancer/Gunmancer/Assets/Audio/

        public void Start()
        {
                program = new Sender();
                Console.WriteLine("Enter '1' to send message");
                Console.WriteLine("Enter '2' to send file");
                Console.WriteLine("Enter '0' to exit");
                SendOnlineStatus();
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
                        SendMessage();
                        break;
                    case 2:
                        number = -1;
                        SendFile();
                        break;
                    case 0:
                        MainClass.running = false;
                        break;
                    default:
                        break;
                }
        }
 
        void SendMessage()
        {
            Console.WriteLine("Enter message : ");
            string messageStr = Console.ReadLine();
            UdpClient sender = new UdpClient();
            try
            {
                byte[] packet = encodePacket('m', messageStr);
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
 
        void SendFile(string path, string fileName)
        {
            UdpClient sender = new UdpClient();

            Timeout = 0;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var md5 = MD5.Create();

            try
            {
                long fileSize = new System.IO.FileInfo(path + fileName).Length;
                byte[] packet = encodePacket('f', "[" + fileSize + "]" + fileName);

                //send file name
                bool receivedCorrectly = false;
                while (!receivedCorrectly)
                {
                    receivedCorrectly = trySendPacket(sender, packet);
                }

                //open file
                FileStream file = File.OpenRead(path + fileName);
                byte[] fileData = new byte[file.Length];
                file.Read(fileData, 0, (int)file.Length);
                byte[] hash = md5.ComputeHash(fileData);

                byte[] buffer = new byte[MAX_PACKET_SIZE - CRC_SIZE - IDENTIFIER_SIZE];

                file.Position = 0;
                long sentSize = 0;
                //send file
                while (file.Read(buffer, 0, buffer.Length) > 0)
                {
                    packet = encodePacket('p', buffer);

                    receivedCorrectly = false;
                    while (!receivedCorrectly)
                    {
                        receivedCorrectly = trySendPacket(sender, packet);
                    }

                    sentSize += 1019;
                    float percent = ((float)sentSize / (float)fileSize) * 100;

                    Console.ForegroundColor = ConsoleColor.Yellow;

                    if (percent >= 100)
                    {
                        percent = 100.00f;
                        Console.ForegroundColor = ConsoleColor.Green;
                    }

                    Console.Write("\r{0:000.00}%   ", percent);
                }
                
                Console.WriteLine();
                Console.ResetColor();

                file.Close();

                //send EOF
                packet = encodePacket('e', fileName);

                receivedCorrectly = false;
                while (!receivedCorrectly)
                {
                    receivedCorrectly = trySendPacket(sender, packet);
                }

                if(!fileCorrect(sender))
                {
                    SendFile(path, fileName);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Corrupted!! Sending again.");
                    Console.ResetColor();
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                sender.Close();
                sw.Stop();
                Console.WriteLine("Timeout : " + Timeout);
                Console.WriteLine("Elapsed={0}", sw.Elapsed);
            }
        }

        bool fileCorrect(UdpClient sender)
        {
            byte[] data = Encoding.ASCII.GetBytes(RECEIVE_NULL);
            while(System.Text.Encoding.UTF8.GetString(data) == RECEIVE_NULL)
            {
                IPEndPoint remoteIp = new IPEndPoint(IPAddress.Parse(remoteAddress), localPort);
                UdpClient receiver = new UdpClient(localPort);
                receiver.Client.ReceiveTimeout = 200;

                try
                {
                    data = receiver.Receive(ref remoteIp);
                }
                catch (Exception e)
                {
                    //Console.WriteLine(e.Message);
                    Timeout++;
                }

                if (System.Text.Encoding.UTF8.GetString(data) != RECEIVE_NULL) //if received something
                {
                    receiver.Close();
                    if (System.Text.Encoding.UTF8.GetString(data) == "h1") // if received confirmation
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

                receiver.Close();

                byte[] packet = encodePacket('h', "HashNotDelivered");
                bool receivedCorrectly = false;
                while (!receivedCorrectly)
                {
                    receivedCorrectly = trySendPacket(sender, packet);
                }
            }

            return false;
        }

        void SendFile()
        {
            Console.WriteLine("Enter file path : ");
            Console.ForegroundColor = ConsoleColor.White;
            string path = Console.ReadLine();
            Console.ResetColor();
            Console.WriteLine("Enter file name : ");
            Console.ForegroundColor = ConsoleColor.White;
            string fileName = Console.ReadLine();
            Console.ResetColor();

            SendFile(path, fileName);
        }

        byte[] encodePacket(char identifier, string packet)
        {
            byte[] messagePart = Encoding.ASCII.GetBytes(packet);
            UInt32 crc = Crc32.Compute(messagePart);
            byte[] crcBytes = BitConverter.GetBytes(crc);
            byte[] message = Encoding.ASCII.GetBytes(identifier + packet);
            message = Combine(crcBytes, message);
            return message;
        }

        byte[] encodePacket(char identifier, byte[] packet)
        {
            UInt32 crc = Crc32.Compute(packet);
            byte[] crcBytes = BitConverter.GetBytes(crc);
            byte[] message = addByteToArray(crcBytes, Convert.ToByte(identifier));
            message = Combine(message, packet);
            return message;
        }
        
        byte[] encodePacketOld(char identifier, string packet)
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
            receiver.Client.ReceiveTimeout = 90;


            //send packet
            sender.Send(packet, packet.Length, remoteAddress, remotePort);


                try
                {
                    data = receiver.Receive(ref remoteIp);
                }
                catch (Exception e)
                {
                    //Console.WriteLine(e.Message);
                    Timeout++;
                }

                if (System.Text.Encoding.UTF8.GetString(data) != RECEIVE_NULL) //if received something
                {
                    receiver.Close();
                    if (System.Text.Encoding.UTF8.GetString(data) == RECEIVE_CONFIRM) // if received confirmation
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

            receiver.Close();
            return false;
        }

        void SendOnlineStatus()
        {
            UdpClient sender = new UdpClient();
            try
            {
                byte[] packet = encodePacket('s', NICKNAME);

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
        
        public static byte[] Combine(byte[] first, byte[] second)
        {
            byte[] ret = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, ret, 0, first.Length);
            Buffer.BlockCopy(second, 0, ret, first.Length, second.Length);
            return ret;
        }
  
        public byte[] addByteToArray(byte[] bArray, byte newByte)
        {
            byte[] newArray = new byte[bArray.Length + 1];
            bArray.CopyTo(newArray, 0);
            newArray[bArray.Length] = newByte;
            return newArray;
        }
    }
}