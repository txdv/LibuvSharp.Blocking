using System;
using Mono.Tasklets;

namespace LibuvSharp.Blocking
{
	public enum MicroThreadState
	{
		NotStarted,
		Ready,
		Running,
		Blocking,
		Stopped,
		Done,
	}

	public class MicroThread
	{
		internal Continuation Continuation { get; set; }
		Action cb;

		public Loop Loop { get; protected set; }

		public MicroThreadState State { get; internal set; }

		public MicroThread(Action callback)
			: this(Loop.Default, callback)
		{
		}

		public MicroThread(Action<MicroThread> callback)
			: this(() => callback(this))
		{
		}

		public MicroThread(Loop loop, Action callback)
		{
			Loop = loop;

			State = MicroThreadState.NotStarted;
			Loop.GetMicroThreadCollection().Add(this);

			cb = callback;
		}

		public MicroThread(Loop loop, Action<MicroThread> callback)
			: this(loop, () => callback(this))
		{
		}

		public void Start()
		{
			switch (State) {
			case MicroThreadState.NotStarted:
				State = MicroThreadState.Ready;
				break;
			default:
				break;
			}
		}

		public void Stop()
		{
			switch (State) {
			case MicroThreadState.Running:
				break;
			default:
				break;
			}
		}

		internal void Run()
		{
			State = MicroThreadState.Running;
			if (Continuation == null) {
				Continuation = new Continuation();
				Continuation.Mark();
				cb();
				State = MicroThreadState.Done;
				Loop.GetMicroThreadCollection().Next();
			} else {
				Continuation.Restore(1);
			}
		}


		public void Yield()
		{
			Yield(MicroThreadState.Ready);
		}

		internal void Yield(MicroThreadState newState)
		{
			if (State == MicroThreadState.Running) {
				State = newState;
				if (Continuation.Store(0) == 0) {
					Loop.GetMicroThreadCollection().Next();
				}
			}
		}
	}
}

