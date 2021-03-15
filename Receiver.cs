using DamienG.Security.Cryptography;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace udp_random
{
	class Receiver
	{
		public static string ip = "25.83.213.159";
		public static int port = 8001;

		public static void Receive()
		{

			UdpClient receiver = new UdpClient(port);
			IPEndPoint remoteIp = new IPEndPoint(IPAddress.Parse("25.83.213.159"), 8001);

			string path = "C:/Users/fedti/Desktop/";
			string filename = "";
			FileStream fs = null;

			try
			{
				while (true)
				{
					byte[] data = receiver.Receive(ref remoteIp);
					int i = 1;
					/*
					byte[] crc_code = new byte[10];
					while (Char.IsNumber(Convert.ToChar(data[i])))
					{
						crc_code[i] = data[i];
						i++;
					}
					*/

					char switcher = Convert.ToChar(data[0]);
					string message = "";
					string type = "";

					switch (switcher)
					{
						case 's':
							message = System.Text.Encoding.UTF8.GetString(data.Skip(i).ToArray());
							type = "*sto*"; //status online
							Console.ForegroundColor = ConsoleColor.Green; // устанавливаем цвет
							Console.WriteLine(message + " is online!");
							Console.ResetColor(); // сбрасываем в стандартный
							break;
						case 'm':
							message = System.Text.Encoding.UTF8.GetString(data.Skip(i).ToArray());
							type = "*msg*";
							Console.WriteLine("-> " + message);
							break;
						case 'f':
							message = System.Text.Encoding.UTF8.GetString(data.Skip(i).ToArray());
							type = "*fle*";
							filename = message;
							Console.WriteLine("-> " + message);
							if (fs == null)
								fs = File.OpenWrite(path + filename);
							break;
						case 'p':
							fs.Write(data, i, data.Length - i);
							break;
						case 'e':
							filename = "";
							fs.Close();
							fs = null;
							Console.WriteLine("[Downloaded!]");
							break;
					}

					/*
					UInt32 crc_data = Crc32.Compute(data.Skip(i).ToArray());
					string crc_code_str = System.Text.Encoding.UTF8.GetString(crc_code.Take(4).ToArray());
					string crc_data_str = System.Text.Encoding.UTF8.GetString(Encoding.ASCII.GetBytes(crc_data.ToString().Take(4).ToArray())).ToString();
					//string crc_data_str = Encoding.ASCII.GetBytes(crc_data.ToString().Take(4).ToArray()).ToString();
					Console.WriteLine("crc_code = " + crc_code_str + " ~ " + "crc_data = " + crc_data_str);
					i = 0;

					if (crc_code_str != crc_data_str)
						StatusMessage.SendBack(ip, port, "m0");
					else
						StatusMessage.SendBack(ip, port, "m1");
					*/
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
			finally
			{
				receiver.Close();
			}
		}
	}
}
