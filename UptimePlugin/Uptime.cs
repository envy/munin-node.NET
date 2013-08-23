using System;
using System.Diagnostics;
using munin_node_Service;

namespace UptimePlugin
{
    public class Uptime : PluginBase
    {
	    private PerformanceCounter _performanceCounter;

	    public override void Initialize()
	    {
		    //_performanceCounter = GetPerformanceCounter(PerformanceCounterNames.System, PerformanceCounterNames.SystemUpTime);
			_performanceCounter = new PerformanceCounter("System", "System Up Time");
		    _performanceCounter.NextValue();
	    }

	    public override string GetConfig()
	    {
		    return "graph_title Uptime\n" +
		           "graph_args --base 1000 -l 0\n" +
		           "graph_scale no\n" +
		           "graph_vlabel uptime in days\n" +
		           "graph_category system\n" +
		           "uptime.label uptime\n" +
		           "uptime.draw AREA\n" +
				   ".\n";
	    }

	    public override string GetValues()
	    {
		    var ts = TimeSpan.FromSeconds(_performanceCounter.NextValue());
			return String.Format("uptime.value {0}\n.\n", DoubleToString(ts.TotalDays));
	    }

	    public override string GetName()
	    {
		    return "uptime";
	    }
    }
}
