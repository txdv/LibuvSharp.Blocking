using System;
using System.Net;
using System.Collections.Generic;
using LibuvSharp;

namespace LibuvSharp.Blocking
{
	public class BlockingUdp : BlockingHandle
	{
		Udp Udp { get; set; }

		Queue<Tuple<IPEndPoint, byte[]>> queue = new Queue<Tuple<IPEndPoint, byte[]>>();

		public BlockingUdp()
			: this(MicroThreadCollection.Active.Loop)
		{
		}

		private BlockingUdp(Loop loop)
			: this(loop, new Udp())
		{
		}

		internal BlockingUdp(Loop loop, Udp udp)
			: base(loop)
		{
			Udp = udp;
			Handle = udp;
		}

		public void Send(IPEndPoint ep, byte[] data, int length)
		{
			var thread = Thread;
			Udp.Send(ep, data, length, (_) => {
				thread.Resume();
			});
			thread.Block();
		}

		public void Send(IPEndPoint ep, byte[] data)
		{
			Send(ep, data, data.Length);
		}

		public void Send(IPAddress address, int port, byte[] data, int length)
		{
			Send(new IPEndPoint(address, port), data, length);
		}

		public void Send(IPAddress address, int port, byte[] data)
		{
			Send(address, port, data, data.Length);
		}

		public void Send(string address, int port, byte[] data, int length)
		{
			Send(IPAddress.Parse(address), port, data, length);
		}

		public void Send(string address, int port, byte[] data)
		{
			Send(address, port, data, data.Length);
		}

		public void Bind(string address, int port)
		{
			Udp.Bind(address, port);
		}

		bool receiveing = false;

		public int ReceiveFrom(byte[] data, ref IPEndPoint ep)
		{
			var thread = Thread;

			if (!receiveing) {
				Udp.Receive((_ep, _data) => {
					queue.Enqueue(Tuple.Create(_ep, _data));
					thread.Resume();
				});
				receiveing = true;
			}

			if (queue.Count == 0) {
				thread.Block();
			}

			var tuple = queue.Dequeue();
			ep = tuple.Item1;
			tuple.Item2.CopyTo(data, 0);
			return data.Length;
		}
	}
}

