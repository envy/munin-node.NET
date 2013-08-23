using System;
using System.Diagnostics;
using munin_node_Service;

namespace CpuPlugin
{
    public class Cpu : PluginBase
    {
		private PerformanceCounter idleTime, userTime, privilegedTime;

	    public override void Initialize()
	    {
			idleTime = new PerformanceCounter("Processor", "% Idle Time", "_Total");
			userTime = new PerformanceCounter("Processor", "% User Time", "_Total");
			privilegedTime = new PerformanceCounter("Processor", "% Privileged Time", "_Total");
		    idleTime.NextValue();
		    userTime.NextValue();
		    privilegedTime.NextValue();
	    }

	    public override string GetConfig()
	    {
		    return "graph_title CPU usage\n" +
		           "graph_order user privileged idle\n" +
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
		           "idle.label idle\n" +
		           "idle.min 0\n" +
		           "idle.draw STACK\n" +
		           "idle.info Idle CPU time";
	    }

	    public override string GetValues()
	    {
		    return String.Format("user.value {0}\nprivileged.value {1}\nidle.value {2}", DoubleToString(userTime.NextValue()),
		                         DoubleToString(privilegedTime.NextValue()), DoubleToString(idleTime.NextValue()));
	    }

	    public override string GetName()
	    {
		    return "cpu";
	    }
    }
}
