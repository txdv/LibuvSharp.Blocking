using System;

namespace LibuvSharp.Blocking
{
	public class BlockingPipe : BlockingStream
	{
		Pipe Pipe { get; set; }

		public BlockingPipe()
			: base(Loop.Default)
		{
		}

		public BlockingPipe(Loop loop)
			: base(loop)
		{
		}

		internal BlockingPipe(Pipe pipe)
			: base(pipe.Loop)
		{
			Handle = pipe;
			Stream = pipe;
			Pipe = pipe;
		}

		public void Connect(string file)
		{
			var tm = Loop.GetMicroThreadCollection();
			var t = tm.ActiveThread;
			Exception ex = null;

			Pipe.Connect(Loop, file, (exception, pipe) => {
				if (exception != null) {
					ex = exception;
				} else {
					Handle = pipe;
					Stream = pipe;
					Pipe = pipe;
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

