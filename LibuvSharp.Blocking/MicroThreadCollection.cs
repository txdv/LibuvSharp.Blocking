using System;
using System.Linq;
using Mono.Tasklets;
using System.Collections.Generic;

namespace LibuvSharp.Blocking
{
	class MicroThreadCollection : List<MicroThread>
	{
		public Loop Loop { get; protected set; }
		public MicroThread ActiveThread { get { return enumerator == null ? null : enumerator.Current; } }

		internal MicroThreadCollection(Loop loop)
		{
			Loop = loop;
		}

		public void Run()
		{
			while (Is(MicroThreadState.Ready, MicroThreadState.Blocking)) {
				Drain();
				Run();
			}
		}

		public void RunOnce()
		{
			Drain();
			Loop.RunOnce();
		}

		public void RunAsync()
		{
			Drain();
			Loop.RunAsync();
		}

		Dictionary<int, MicroThreadCollection> threads = new Dictionary<int, MicroThreadCollection>();
		Continuation Continuation { get; set; }
		IEnumerator<MicroThread> enumerator = null;

		bool Is(params MicroThreadState[] states)
		{
			foreach (var thread in this) {
				if (states.Contains(thread.State)) {
					return true;
				}
			}
			return false;
		}

		void Drain(Action callback)
		{
			var id = System.Threading.Thread.CurrentThread.ManagedThreadId;
			threads[id] = this;

			callback();

			threads.Remove(id);
		}

		void Drain()
		{
			Drain(() => {
				while (Is(MicroThreadState.Ready)) {
					ExecuteMicroThreads();
				}
			});
		}

		void ExecuteMicroThreads()
		{
			if (ActiveThread != null) {
				throw new Exception("no thread must be active");
			}

			enumerator = GetEnumerator();

			if (Continuation == null) {
				Continuation = new Continuation();
				Continuation.Mark();
			}

			switch (Continuation.Store(0)) {
			case 0:
				Next();
				break;
			case 1:
				Continuation = null;
				break;
			}
		}

		public void Next()
		{
			if (enumerator.MoveNext()) {
				switch (ActiveThread.State) {
				case MicroThreadState.Ready:
					ActiveThread.Run();
					break;
				default:
					Next();
					break;
				}
			} else {
				enumerator = null;

				var toremove = this.Where(t => t.State == MicroThreadState.Stopped).ToList();
				foreach (var r in toremove) {
					Remove(r);
				}

				Loop.RunOnce();
				Continuation.Restore(1);
			}
		}
	}

	public static class MicroThreadCollectionExtension
	{
		static Dictionary<Loop, MicroThreadCollection> dict = new Dictionary<Loop, MicroThreadCollection>();

		internal static MicroThreadCollection GetMicroThreadCollection(this Loop loop)
		{
			MicroThreadCollection mt;
			if (!dict.TryGetValue(loop, out mt)) {
				mt = new MicroThreadCollection(loop);
				dict[loop] = mt;
			}
			return mt;
		}

		public static void BlockingRun(this Loop loop)
		{
			GetMicroThreadCollection(loop).Run();
		}

		public static void BlockingRunOnce(this Loop loop)
		{
			GetMicroThreadCollection(loop).RunOnce();
		}

		public static void BlockingRunAsync(this Loop loop)
		{
			GetMicroThreadCollection(loop).RunAsync();
		}
	}
}

