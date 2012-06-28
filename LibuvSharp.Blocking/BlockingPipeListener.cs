using System;
using System.Collections.Generic;

namespace LibuvSharp.Blocking
{
	public class BlockingPipeListener : BlockingListener
	{
		Queue<Pipe> queue = new Queue<Pipe>();

		PipeListener PipeListener { get; set; }

		public BlockingPipeListener()
			: this(Loop.Default)
		{
		}

		public BlockingPipeListener(Loop loop)
			: base(loop)
		{
			PipeListener = new PipeListener(loop);
			Listener = PipeListener as Listener;
			Handle = PipeListener;
		}

		public void Bind(string file)
		{
			PipeListener.Bind(file);
		}

		bool init = false;
		public BlockingPipe Accept()
		{
			var thread = Thread;

			if (!init) {
				PipeListener.Listen((pipe) => {
					queue.Enqueue(pipe as Pipe);
					if (thread.State == MicroThreadState.Blocking) {
						thread.State = MicroThreadState.Ready;
					}
				});

				init = true;
			}

			thread.Yield();
			return new BlockingPipe(queue.Dequeue());
		}
	}
}

