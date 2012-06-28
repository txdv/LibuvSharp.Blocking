using System;
using System.Net;

namespace LibuvSharp.Blocking
{
	public class BlockingTcpListener : BlockingListener
	{
		TcpListener TcpListener { get; set; }

		public BlockingTcpListener()
			: this(MicroThreadCollection.Active.Loop)
		{
		}

		BlockingTcpListener(Loop loop)
			: base(loop)
		{
			TcpListener = new TcpListener(loop);
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

		protected override BlockingStream Create(Stream stream)
		{
			return new BlockingTcp(stream as Tcp);
		}

		public BlockingTcp Accept()
		{
			return AcceptBlockingStream() as BlockingTcp;
		}
	}
}

