using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using munin_node_Service;

namespace DNSPlugin
{
	public class DNS : PluginBase
	{
		private PerformanceCounter _queriespersec;

		public override Capabilities GetCapabilities()
		{
			return Capabilities.None;
		}

		public override void Initialize()
		{
			if (!PerformanceCounterCategory.Exists("DNS"))
				throw new Exception("No DNS performance counters found!");

			_queriespersec = new PerformanceCounter("DNS", "Total Query Received/sec");
			_queriespersec.NextValue();
		}

		public override string GetConfig(Capabilities withCapabilities)
		{
			return "graph_title DNS queries\n" +
			       "graph_args --base 1000\n" +
			       "graph_vlabel seconds\n" +
			       "graph_category network\n" +
			       "graph_info This graph shows the number of DNS queries received per second\n" +
			       "queries.label queries/sec\n" +
			       "queries.min 0\n" +
			       "queries.draw LINE1\n";
		}

		public override string GetValues(Capabilities withCapabilities)
		{
			return String.Format("queries.value {0}\n", DoubleToString(_queriespersec.NextValue()));
		}

		public override string GetName()
		{
			return "dns";
		}
	}
}
