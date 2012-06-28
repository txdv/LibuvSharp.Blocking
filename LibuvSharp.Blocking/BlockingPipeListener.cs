using System;

namespace LibuvSharp.Blocking
{
	public class BlockingPipeListener : BlockingListener
	{
		PipeListener PipeListener { get; set; }

		public BlockingPipeListener()
			: this(MicroThreadCollection.Active.Loop)
		{
		}

		BlockingPipeListener(Loop loop)
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

		public BlockingPipe Accept()
		{
			return AcceptBlockingStream() as BlockingPipe;

		}

		protected override BlockingStream Create(Stream stream)
		{
			return new BlockingPipe(stream as Pipe);
		}
	}
}

