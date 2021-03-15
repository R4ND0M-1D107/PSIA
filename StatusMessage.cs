using System;
using System.Net.Sockets;
using System.Text;

namespace udp_random
{
	class StatusMessage
	{
		public static void SendBack(string ip, int port, string msg)
		{
			UdpClient sender = new UdpClient();
			try
			{
				byte[] message = Encoding.ASCII.GetBytes(msg);
				sender.Send(message, message.Length, ip, port);
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
