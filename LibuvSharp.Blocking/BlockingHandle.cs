using System;

namespace LibuvSharp.Blocking
{
	public abstract class BlockingHandle
	{
		public Loop Loop { get; protected set; }
		protected Handle Handle { get; set; }
		MicroThreadCollection ThreadCollection { get; set; }
		protected MicroThread Thread {
			get {
				return ThreadCollection.ActiveThread;
			}
		}

		public BlockingHandle(Loop loop)
		{
			Loop = loop;
			ThreadCollection = Loop.GetMicroThreadCollection();
		}

		public void Close()
		{
			Handle.Close();
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

