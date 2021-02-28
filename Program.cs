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
}
