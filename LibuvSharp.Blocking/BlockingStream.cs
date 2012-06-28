using System;

namespace LibuvSharp.Blocking
{
	abstract public class BlockingStream : BlockingHandle
	{
		ByteBuffers buffers = new ByteBuffers();

		protected Stream Stream { get; set; }

		protected BlockingStream(Loop loop)
			: base(loop)
		{
		}

		public void Write(byte[] data, int length)
		{
			var thread = Thread;
			Stream.Write(data, length, (_) => {
				thread.Resume();
			});
			thread.Yield(MicroThreadState.Blocking);
		}

		public void Write(byte[] data)
		{
			Write(data, data.Length);
		}

		bool init = false;
		bool closed = false;
		public int Receive(byte[] data)
		{
			var tm = Loop.GetMicroThreadCollection();
			var t = tm.ActiveThread;

			if (!init) {
				Stream.OnRead += (_data) => {
					buffers.Add(new ByteBuffer(_data));
					if (t.State == MicroThreadState.Blocking) {
						t.State = MicroThreadState.Ready;
					}
				};

				Stream.CloseEvent += () => {
					closed = true;
					if (t.State == MicroThreadState.Blocking) {
						t.State = MicroThreadState.Ready;
					}
				};
				Stream.Resume();
				init = true;
			}


			if (t.Continuation.Store(0) == 0) {
				t.State = MicroThreadState.Blocking;
				tm.Next();
				return 0;
			} else {
				if (closed) {
					return 0;
				} else {
					int n = Copy(data);
					return n;
				}
			}
		}

		int Copy(byte[] data)
		{
			int min = Math.Min(buffers.Length, data.Length);
			buffers.CopyTo(data, min);
			buffers.Skip(min);
			return min;
		}
	}
}

