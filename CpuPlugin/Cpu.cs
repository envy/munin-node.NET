using System;
using System.Diagnostics;
using munin_node_Service;

namespace CpuPlugin
{
	public class Cpu : PluginBase
	{
		private PerformanceCounter _userTime;
		private PerformanceCounter _privilegedTime;
		private PerformanceCounter _interruptTime;
		private PerformanceCounter _dpcTime;

		public override Capabilities GetCapabilities()
		{
			return Capabilities.None;
		}

		public override void Initialize()
		{
			_userTime = new PerformanceCounter("Processor", "% User Time", "_Total");
			_privilegedTime = new PerformanceCounter("Processor", "% Privileged Time", "_Total");
			_interruptTime = new PerformanceCounter("Processor", "% Interrupt Time", "_Total");
			_dpcTime = new PerformanceCounter("Processor", "% DPC Time", "_Total");
			_userTime.NextValue();
			_privilegedTime.NextValue();
			_interruptTime.NextValue();
			_dpcTime.NextValue();
		}

		public override string GetConfig(Capabilities withCapabilities)
		{
			return "graph_title CPU usage\n" +
				   "graph_order privileged interrupt dpc user idle\n" +
				   "graph args --base 1000 -r -l 0 --upper-limit 100\n" +
				   "graph_vlabel %\n" +
				   "graph_scale no\n" +
				   "graph_info This graph shows how CPU time is spent.\n" +
				   "graph_category system\n" +
				   "graph_period second\n" +
				   "user.label user\n" +
				   "user.min 0\n" +
				   "user.draw STACK\n" +
				   "user.info CPU time spent in user mode\n" +
			       "user.colour 00ffff\n" +
				   "privileged.label privileged\n" +
				   "privileged.min 0\n" +
				   "privileged.draw AREA\n" +
				   "privileged.info CPU time spent in privileged mode\n" +
			       "privileged.colour FF0000\n" +
				   "interrupt.label interrupt\n" +
				   "interrupt.min 0\n" +
				   "interrupt.draw STACK\n" +
				   "interrupt.info CPU time spent dealing with interrupts\n" +
				   "interrupt.colour ffa400\n" +
				   "dpc.label dpc\n" +
				   "dpc.min 0\n" +
				   "dpc.draw STACK\n" +
				   "dpc.info CPU time spent in deferred procedure calls (dpc)\n" +
			       "dpc.colour ffa07a\n" +
				   "idle.label idle\n" +
				   "idle.min 0\n" +
				   "idle.draw STACK\n" +
				   "idle.info Idle CPU time\n" +
			       "idle.colour 90ee90\n";
		}

		public override string GetValues(Capabilities withCapabilities)
		{
			var user = _userTime.NextValue();
			var kernel = _privilegedTime.NextValue();
			var interrupts = _interruptTime.NextValue();
			var dpc = _dpcTime.NextValue();
			var idle = 100.0 - user - kernel - interrupts - dpc;
			return String.Format("user.value {0}\nprivileged.value {1}\ninterrupt.value {2}\ndpc.value {3}\nidle.value {4}", DoubleToString(user),
								 DoubleToString(kernel), DoubleToString(interrupts), DoubleToString(dpc), DoubleToString(idle));
		}

		public override string GetName()
		{
			return "cpu";
		}
	}
}
