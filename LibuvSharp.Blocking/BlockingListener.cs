using System;
using System.Collections.Generic;

namespace LibuvSharp.Blocking
{
	public abstract class BlockingListener : BlockingHandle
	{
		Queue<Stream> queue = new Queue<Stream>();

		protected Listener Listener { get; set; }

		public BlockingListener(Loop loop)
			: base(loop)
		{
			Loop = loop;
		}

		bool init = false;
		public Stream AcceptStream()
		{
			var thread = Thread;

			if (!init) {
				Listener.Listen((stream) => {
					queue.Enqueue(stream);
					thread.Resume();
				});

				init = true;
			}

			thread.Yield(MicroThreadState.Blocking);
			return queue.Dequeue();
		}

		protected abstract BlockingStream Create(Stream stream);

		public BlockingStream AcceptBlockingStream()
		{
			return Create(AcceptStream());
		}
	}
}

