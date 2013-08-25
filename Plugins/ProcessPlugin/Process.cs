using System;
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
					  "graph order threads, running exited\n" +
			          "threads.label Threads\n" +
			          "threads.min 0\n" +
			          "threads.draw LINE1\n" +
			          "running.label Running processes\n" +
			          "runnung.min 0\n" +
			          "running.draw AREA\n" +
			          "exited.label Exited processes\n" +
			          "exited.min 0\n" +
			          "exited.draw STACK\n";

		}

		public override string GetConfig(Capabilities withCapabilities)
		{
			return _config;
		}

		public override string GetValues(Capabilities withCapabilities)
		{
			var processes = System.Diagnostics.Process.GetProcesses();
			var threads = 0;
			var running = 0;
			var exited = 0;
			foreach (var process in processes)
			{
				if (!process.HasExited)
				{
					running++;
					threads += process.Threads.Count;
				}
				else
				{
					exited++;
				}
			}
			return String.Format("multigraph processes_count\nthreads.value {0}\nrunning.value {1}\nexited.value {2}", threads, running, exited);
		}

		public override string GetName()
		{
			return "processes";
		}
	}
}
