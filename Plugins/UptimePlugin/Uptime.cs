using System;
using System.Collections.Generic;
using System.Diagnostics;
using munin_node_Service;

namespace UptimePlugin
{
	public class Uptime : PluginBase
	{
		private PerformanceCounter _performanceCounter;
		private string _config;

		public override Capabilities GetCapabilities()
		{
			return Capabilities.None;
		}

		public override void Initialize(Dictionary<string, string> config)
		{
			_performanceCounter = new PerformanceCounter("System", "System Up Time");
			_performanceCounter.NextValue();
			_config = "graph_title Uptime\n" +
					  "graph_args --base 1000 -l 0\n" +
					  "graph_scale no\n" +
					  "graph_vlabel uptime in days\n" +
					  "graph_category system\n" +
					  "uptime.label uptime\n" +
					  "uptime.draw AREA\n";
		}

		public override string GetConfig(Capabilities withCapabilities)
		{
			return _config;
		}

		public override string GetValues(Capabilities withCapabilities)
		{
			var ts = TimeSpan.FromSeconds(_performanceCounter.NextValue());
			return String.Format("uptime.value {0}\n", DoubleToString(ts.TotalDays));
		}

		public override string GetName()
		{
			return "uptime";
		}
	}
}
