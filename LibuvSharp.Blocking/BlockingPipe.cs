using System;

namespace LibuvSharp.Blocking
{
	public class BlockingPipe : BlockingStream
	{
		Pipe Pipe { get; set; }

		public BlockingPipe()
			: base(MicroThreadCollection.Active.Loop)
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
			var thread = Thread;
			Exception ex = null;

			Pipe.Connect(Loop, file, (exception, pipe) => {
				if (exception != null) {
					ex = exception;
				} else {
					Handle = pipe;
					Stream = pipe;
					Pipe = pipe;
				}
				thread.Resume();
			});

			thread.Yield(MicroThreadState.Blocking);
			if (ex != null) {
				throw ex;
			}
		}
	}
}

