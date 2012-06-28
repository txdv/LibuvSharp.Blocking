using System;
using System.Net;

namespace LibuvSharp.Blocking
{
	public class BlockingTcp : BlockingStream
	{
		Tcp Tcp { get; set; }

		public BlockingTcp()
			: base(MicroThreadCollection.Active.Loop)
		{
		}

		internal BlockingTcp(Tcp tcp)
			: base(tcp.Loop)
		{
			Handle = tcp;
			Stream = tcp;
			Tcp = tcp;
		}

		public void Connect(IPEndPoint ep)
		{
			var thread = Thread;
			Exception ex = null;

			Tcp.Connect(Loop, ep, (exception, tcp) => {
				if (exception != null) {
					ex = exception;
				} else {
					Handle = tcp;
					Stream = tcp;
					Tcp = tcp;
				}
				thread.Resume();
			});

			thread.Yield(MicroThreadState.Blocking);

			if (ex != null) {
				throw ex;
			}
		}

		public void Connect(IPAddress address, int port)
		{
			Connect(new IPEndPoint(address, port));
		}

		public void Connect(string address, int port)
		{
			Connect(IPAddress.Parse(address), port);
		}
	}
}

