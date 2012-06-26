using System;
using System.Net;
using System.Collections.Generic;

namespace LibuvSharp.Blocking
{
	public abstract class BlockingListener : BlockingHandle
	{
		protected Listener Listener { get; set; }

		public BlockingListener(Loop loop)
			: base(loop)
		{
			Loop = loop;
		}
	}

	public class BlockingTcpListener : BlockingListener
	{
		Queue<Tcp> queue = new Queue<Tcp>();

		TcpListener TcpListener { get; set; }

		public BlockingTcpListener()
			: this(Loop.Default)
		{
		}

		public BlockingTcpListener(Loop loop)
			: base(loop)
		{
			TcpListener = new TcpListener();
			Listener = TcpListener as Listener;
			Handle = TcpListener;
		}

		public void Bind(IPEndPoint ep)
		{
			TcpListener.Bind(ep);
		}

		public void Bind(IPAddress address, int port)
		{
			TcpListener.Bind(address, port);
		}

		public void Bind(string address, int port)
		{
			TcpListener.Bind(address, port);
		}

		bool init = false;
		public BlockingTcp Accept()
		{
			var tm = Loop.GetMicroThreadCollection();
			var t = tm.ActiveThread;

			if (!init) {
				TcpListener.Listen((Tcp tcp) => {
					queue.Enqueue(tcp);
					if (t.State == MicroThreadState.Blocking) {
						t.State = MicroThreadState.Ready;
					}
				});

				init = true;
			}

			if (t.Continuation.Store(0) == 0) {
				tm.Next();
				return null;
			} else {
				return new BlockingTcp(queue.Dequeue());
			}
		}
	}
}

