using System;
using System.Net;

namespace LibuvSharp.Blocking
{
	public class BlockingTcp : BlockingStream
	{
		Tcp Tcp { get; set; }

		public BlockingTcp()
			: this(Loop.Default)
		{
		}

		public BlockingTcp(Loop loop)
			: base(loop)
		{
		}

		public void Connect(IPEndPoint ep)
		{
			var tm = Loop.GetMicroThreadCollection();
			var t = tm.ActiveThread;
			Exception ex = null;

			Tcp.Connect(Loop, ep, (exception, tcp) => {
				if (exception != null) {
					ex = exception;
				} else {
					Handle = tcp;
					Stream = tcp;
					Tcp = tcp;
				}
				t.State = MicroThreadState.Ready;
			});

			t.State = MicroThreadState.Blocking;

			if (t.Continuation.Store(0) == 0) {
				tm.Next();
			} else {
				if (ex != null) {
					throw ex;
				}
			}
		}
	}
}

