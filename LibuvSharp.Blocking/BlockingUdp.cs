using System;
using System.Net;
using System.Collections.Generic;
using LibuvSharp;

namespace LibuvSharp.Blocking
{
	public class BlockingUdp
	{
		public Loop Loop { get { return Udp.Loop; } }
		Udp Udp { get; set; }

		Queue<Tuple<IPEndPoint, byte[]>> queue = new Queue<Tuple<IPEndPoint, byte[]>>();

		public BlockingUdp()
			: this(Loop.Default)
		{
		}

		public BlockingUdp(Loop loop)
		{
			Udp = new Udp(loop);
		}

		public void Send(string address, int port, byte[] data)
		{
			var tm = Loop.GetMicroThreadCollection();
			var t = tm.ActiveThread;
			Udp.Send(address, port, data, (_) => {
				t.State = MicroThreadState.Ready;
			});
			t.State = MicroThreadState.Blocking;

			if (t.Continuation.Store(0) == 0) {
				tm.Next();
			}
		}


		public void Bind(string address, int port)
		{
			Udp.Bind(address, port);
		}

		bool receiveing = false;

		IPEndPoint ep2 = null;
		byte[] data2 = null;

		public int ReceiveFrom(byte[] data, ref IPEndPoint ep)
		{
			var tm = Loop.GetMicroThreadCollection();
			var t = tm.ActiveThread;

			if (!receiveing) {
				Udp.Receive((_ep, _data) => {
					if (t.State == MicroThreadState.Blocking) {
						t.State = MicroThreadState.Ready;
						ep2 = _ep;
						data2 = _data;
					} else {
						queue.Enqueue(Tuple.Create(_ep, _data));
					}
				});
				receiveing = true;
			}

			if (queue.Count > 0) {
				var tuple = queue.Dequeue();
				ep = tuple.Item1;
				tuple.Item2.CopyTo(data, 0);
				return data.Length;
			} else {
				t.State = MicroThreadState.Blocking;

				if (t.Continuation.Store(0) == 0) {
					tm.Next();
					return 0;
				} else {
					if (ep2 == null) {
						throw new Exception();
					}
					ep = ep2;
					data2.CopyTo(data, 0);
					return data2.Length;
				}
			}
		}

		public void Close()
		{
			Udp.Close();
		}
	}
}

