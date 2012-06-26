using System;

namespace LibuvSharp.Blocking
{
	public abstract class BlockingHandle
	{
		public Loop Loop { get; protected set; }
		protected Handle Handle { get; set; }

		public BlockingHandle(Loop loop)
		{
			Loop = loop;
		}

		public void Close()
		{
			var tm = Loop.GetMicroThreadCollection();
			var t = tm.ActiveThread;
			Handle.Close(() => {
				t.State = MicroThreadState.Ready;
			});
			t.State = MicroThreadState.Blocking;

			if (t.Continuation.Store(0) == 0) {
				tm.Next();
			}
		}

		public bool Closing {
			get {
				return Handle.Closing;
			}
		}

		public bool Closed {
			get {
				return Handle.Closed;
			}
		}

		public bool Active {
			get {
				return Handle.Active;
			}
		}

		public bool Readable {
			 get {
				return Handle.Readable;
			}
		}

		public bool Writeable {
			 get {
				return Handle.Writeable;
			}
		}
	}
}

