using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

namespace udp_random
{
	class Program
	{
		static void Main(string[] args) {
			Thread receiveThread = new Thread(new ThreadStart(Reciever));
			receiveThread.Start();

			Program program = new Program();

			//C:/Users/overlord/Documents/PSIA/

			localPort = 8002;
			remotePort = 8001;
			remoteAddress = "25.84.62.46";

			Console.WriteLine("Enter '1' to send message");
			Console.WriteLine("Enter '2' to send file");
			Console.WriteLine("Enter '0' to exit");

			running = true;
			while (running)
			{
				string input = Console.ReadLine();
				int number;
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
						running = !running;
						break;
					default:
						break;
				}
			}
		}

		private static void Reciever() {


			UdpClient receiver = new UdpClient(8001);
			IPEndPoint remoteIp = new IPEndPoint(IPAddress.Parse("25.83.213.159"), 8001);

			string path = "C:/Users/fedti/Desktop/";
			string filename = "";
			FileStream fs = null;

			try {
				while (true) {
					byte[] data = receiver.Receive(ref remoteIp);
					char switcher = Convert.ToChar(data[0]);
					string message = "";

					switch (switcher)
					{
						case 'm':
							message = System.Text.Encoding.UTF8.GetString(data.Skip(1).ToArray());
							Console.WriteLine("*msg*");
							Console.WriteLine("Nekdo: {0}", message);
							break;
						case 'f':
							message = System.Text.Encoding.UTF8.GetString(data.Skip(1).ToArray());
							Console.WriteLine("*file*");
							filename = message;
							Console.WriteLine("Nekdo: {0}", message);
							if (fs == null)
								fs = File.OpenWrite(path + filename);
							break;
						case 'p':
							fs.Write(data, 1, data.Length - 1);
							break;
						case 'e':
							filename = "";
							fs.Close();
							fs = null;
							Console.WriteLine(switcher);
							break;
					}

				}
			}
			catch (Exception ex) {
				Console.WriteLine(ex.Message);
			}
			finally {
				receiver.Close();
			}
		}
	}

	public void SendMessage()
	{
		Console.WriteLine("Enter message : ");
		string messageStr = Console.ReadLine();
		UdpClient sender = new UdpClient();
		try
		{
			byte[] message = Encoding.ASCII.GetBytes("m" + messageStr);

			//send file name
			sender.Send(message, message.Length, remoteAddress, remotePort);
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
			//file name identificator + file name
			byte[] fileNameBytes = Encoding.ASCII.GetBytes("f" + fileName);

			//send file name
			sender.Send(fileNameBytes, fileNameBytes.Length, remoteAddress, remotePort);

			//open file
			FileStream fs = File.OpenRead(path + fileName);
			byte[] buffer = new byte[1024];

			//set file identificator
			buffer[0] = Encoding.ASCII.GetBytes("p")[0];

			//send file
			while (fs.Read(buffer, 1, buffer.Length - 1) > 0)
			{
				UTF8Encoding temp = new UTF8Encoding(true);
				sender.Send(buffer, buffer.Length, remoteAddress, remotePort);
			}

			fs.Close();

			byte[] message = Encoding.ASCII.GetBytes("e" + fileName);

			//send file name
			sender.Send(message, message.Length, remoteAddress, remotePort);

			//close file
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
