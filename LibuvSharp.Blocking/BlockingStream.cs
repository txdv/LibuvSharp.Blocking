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
			var tm = Loop.GetMicroThreadCollection();
			var t = tm.ActiveThread;
			Stream.Write(data, length, (_) => {
				t.State = MicroThreadState.Ready;
			});

			t.State = MicroThreadState.Blocking;
		}

		public void Write(byte[] data)
		{
			Write(data, data.Length);
		}

		bool init = false;
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
				Stream.Resume();
			}

			if (buffers.Length > 0) {
				return Copy(data);
			} else {
				t.State = MicroThreadState.Blocking;
				if (t.Continuation.Store(0) == 0) {
					tm.Next();
					return 0;
				} else {
					return Copy(data);
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

