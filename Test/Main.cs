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
			UdpMain(args);
			Loop.Default.BlockingRun();
		}

		public static void TimerMain(string[] args)
		{
			new MicroThread(() => {
				Console.WriteLine("1");
				MicroThread.Active.Sleep(TimeSpan.FromSeconds(2));
				Console.WriteLine("2");
			}).Start();
		}

		public static void YieldMain(string[] args)
		{
			new MicroThread(() => {
				MicroThread.Active.Yield();
				Console.WriteLine("Two");
				MicroThread.Active.Yield();
				Console.WriteLine("Four");
			}).Start();

			new MicroThread(() => {
				Console.WriteLine("One");
				MicroThread.Active.Yield();
				Console.WriteLine("Three");
			}).Start();
		}

		public static void PipeMain(string[] args)
		{
			new MicroThread(() => {
				var server = new BlockingPipeListener();
				server.Bind("file");
				var pipe = server.Accept();
				server.Close();
				byte[] data = new byte[512];
				int n = 0;
				while ((n = pipe.Receive(data)) != 0) {
					Console.WriteLine(n);
					Console.WriteLine(Encoding.ASCII.GetString(data, 0, n));
				}

				pipe.Close();
			}).Start();

			new MicroThread(() => {
				var pipe = new BlockingPipe();
				pipe.Connect("file");
				var bytes = Encoding.ASCII.GetBytes("NESAMONE");
				pipe.Write(bytes);
				pipe.Close();
			}).Start();
		}

		public static void TcpMain(string[] args)
		{
			new MicroThread(() => {
				var server = new BlockingTcpListener();
				server.Bind("127.0.0.1", 7000);
				var tcp = server.Accept();
				tcp.Write(Encoding.ASCII.GetBytes("ASD"));
				tcp.Close();
				server.Close();
			}).Start();

			new MicroThread(() => {
				var tcp = new BlockingTcp();
				tcp.Connect("127.0.0.1", 7000);
				tcp.Write(Encoding.ASCII.GetBytes("NESAMONE"));
				byte[] data = new byte[256];
				int n = 0;

				while ((n = tcp.Receive(data)) != 0) {
					Console.WriteLine(n);
					Console.WriteLine(Encoding.ASCII.GetString(data, 0, n));
				}

				Console.WriteLine("Closing");
				tcp.Close();
			}).Start();
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
					udp.Send("127.0.0.1", 7000, Encoding.ASCII.GetBytes("nesamone " + i));
				}
				udp.Close();
			}).Start();
		}
	}
}
