using System;
using Mono.Tasklets;

namespace LibuvSharp.Blocking
{
	public enum MicroThreadState
	{
		NotStarted,
		Ready,
		Running,
		Stopped,
		Blocking
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

		public MicroThread(Loop loop, Action callback)
		{
			Loop = loop;

			State = MicroThreadState.NotStarted;
			Loop.GetMicroThreadCollection().Add(this);

			cb = callback;
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
			if (Continuation == null) {
				Continuation = new Continuation();
				Continuation.Mark();
				cb();
				State = MicroThreadState.Stopped;
				Loop.GetMicroThreadCollection().Next();
			} else {
				Continuation.Restore(1);
			}
		}
	}
}

