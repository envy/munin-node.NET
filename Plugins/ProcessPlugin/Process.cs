using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using munin_node_Service;

namespace ProcessPlugin
{
	public class Process : PluginBase
	{
		private string _config = "";

		public override Capabilities GetCapabilities()
		{
			return Capabilities.Multigraph;
		}

		public override void Initialize()
		{
			_config = "multigraph processes_count" +
			          "graph_title This graph shows the number of processes and threads.\n" +
			          "graph_category processes\n" +
			          "graph_args --base 1000 -l 0\n" +
			          "graph_vlabel Number of processes / threads\n" +
					  "threads.label Running threads\n" +
			          "threads.min 0\n" +
			          "threads.draw LINE1\n" +
			          "processes.label Running processes\n" +
			          "processes.min 0\n" +
			          "processes.draw LINE1\n";

		}

		public override string GetConfig(Capabilities withCapabilities)
		{
			return _config;
		}

		public override string GetValues(Capabilities withCapabilities)
		{
			var processes = System.Diagnostics.Process.GetProcesses();
			var threads = processes.Select(_ => _.Threads).Select(_ => _.Count).Aggregate((count, next) => count + next);
			var running = processes.Count();
			return String.Format("multigraph processes_count\nthreads.value {0}\nprocesses.value {1}\n", threads, running);
		}

		public override string GetName()
		{
			return "processes";
		}
	}
}
