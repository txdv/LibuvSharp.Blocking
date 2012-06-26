using System;
using System.Net;
using System.Text;
using LibuvSharp;
using LibuvSharp.Blocking;

namespace Test
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			new MicroThread(() => {
				var tcp = new BlockingTcp();
				tcp.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 7000));
				tcp.Write(Encoding.ASCII.GetBytes("NESAMONE"));
				byte[] data = new byte[512];
				int n = tcp.Receive(data);
				Console.WriteLine (Encoding.ASCII.GetString(data, 0, n));
				tcp.Close();

			}).Start();

			Loop.Default.BlockingRun();
		}

		public static void UdpMain(string[] args)
		{
			new MicroThread(() => {
				var udp = new BlockingUdp();
				udp.Bind("127.0.0.1", 7000);
				byte[] data = new byte[10];
				IPEndPoint ep = null;
				for (int i = 0; i < 10; i++) {
					int n = udp.ReceiveFrom(data, ref ep);
					Console.WriteLine(Encoding.ASCII.GetString(data, 0, n));
				}
				udp.Close();
			}).Start();

			new MicroThread(() => {
				var udp = new BlockingUdp();
				for (int i = 0; i < 10; i++) {
					udp.Send("127.0.0.1", 7000, Encoding.ASCII.GetBytes("nesamone"));
				}
				udp.Close();
			}).Start();

			Loop.Default.BlockingRun();
		}
	}
}
