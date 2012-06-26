using System;
using System.Linq;
using Mono.Tasklets;
using System.Collections.Generic;

namespace LibuvSharp.Blocking
{
	class MicroThreadCollection : List<MicroThread>
	{
		Continuation Continuation { get; set; }

		IEnumerator<MicroThread> enumerator = null;

		internal MicroThreadCollection(Loop loop)
		{
			Loop = loop;
		}

		public Loop Loop { get; protected set; }

		public MicroThread ActiveThread { get { return enumerator == null ? null : enumerator.Current; } }

		public void Run()
		{
			while (true) {
				bool todo = false;
				foreach (var thread in this) {
					if (thread.State == MicroThreadState.Ready || thread.State == MicroThreadState.Blocking) {
						todo = true;
						break;
					}
				}

				if (todo) {
					RunOnce();
				} else {
					return;
				}
			}
		}

		public void RunOnce()
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

		public void RunAsync()
		{
			throw new NotImplementedException();
		}

		internal void Next()
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

