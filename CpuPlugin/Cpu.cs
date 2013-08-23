using System;
using System.Diagnostics;
using munin_node_Service;

namespace CpuPlugin
{
	public class Cpu : PluginBase
	{
		private PerformanceCounter _idleTime;
		private PerformanceCounter _userTime;
		private PerformanceCounter _privilegedTime;
		private PerformanceCounter _interruptTime;
		private PerformanceCounter _dpcTime;

		public override void Initialize()
		{
			_idleTime = new PerformanceCounter("Processor", "% Idle Time", "_Total");
			_userTime = new PerformanceCounter("Processor", "% User Time", "_Total");
			_privilegedTime = new PerformanceCounter("Processor", "% Privileged Time", "_Total");
			_interruptTime = new PerformanceCounter("Processor", "% Interrupt Time", "_Total");
			_dpcTime = new PerformanceCounter("Processor", "% DPC Time", "_Total");
			_idleTime.NextValue();
			_userTime.NextValue();
			_privilegedTime.NextValue();
			_interruptTime.NextValue();
			_dpcTime.NextValue();
		}

		public override string GetConfig()
		{
			return "graph_title CPU usage\n" +
				   "graph_order user privileged interrupt dpc idle\n" +
				   "graph args --base 1000 -r -l 0 --upper-limit 100\n" +
				   "graph_vlabel %\n" +
				   "graph_scale no\n" +
				   "graph_info This graph shows how CPU time is spent.\n" +
				   "graph_category system\n" +
				   "graph_period second\n" +
				   "user.label user\n" +
				   "user.min 0\n" +
				   "user.draw AREA\n" +
				   "user.info CPU time spent in user mode\n" +
				   "privileged.label privileged\n" +
				   "privileged.min 0\n" +
				   "privileged.draw STACK\n" +
				   "privileged.info CPU time spent in privileged mode\n" +
				   "interrupt.label interrupt\n" +
				   "interrupt.min 0\n" +
				   "interrupt.draw STACK\n" +
				   "interrupt.info CPU time spent dealing with interrupts\n" +
				   "dpc.label dpc\n" +
				   "dpc.min 0\n" +
				   "dpc.draw STACK\n" +
				   "dpc.info CPU time spent in deferred procedure calls (dpc)\n" +
				   "idle.label idle\n" +
				   "idle.min 0\n" +
				   "idle.draw STACK\n" +
				   "idle.info Idle CPU time";
		}

		public override string GetValues()
		{
			return String.Format("user.value {0}\nprivileged.value {1}\ninterrupt.value {2}\ndpc.value {3}\nidle.value {4}", DoubleToString(_userTime.NextValue()),
								 DoubleToString(_privilegedTime.NextValue()), DoubleToString(_interruptTime.NextValue()), DoubleToString(_dpcTime.NextValue()), DoubleToString(_idleTime.NextValue()));
		}

		public override string GetName()
		{
			return "cpu";
		}
	}
}
