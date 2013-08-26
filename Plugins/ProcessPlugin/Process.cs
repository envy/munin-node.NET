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
			_config = "multigraph processes_count\n" +
					  "graph_title Processes\n" +
					  "graph_info This graph shows the number of processes\n" +
					  "graph_category processes\n" +
					  "graph_args --base 1000 -l 0\n" +
					  "graph_vlabel Number of processes\n" +
					  "processes.label processes\n" +
					  "processes.min 0\n" +
					  "processes.draw LINE1\n" +
					  "multigraph processes_threads\n" +
					  "graph_title Threads\n" +
					  "graph_info This graph shows the number of threads\n" +
					  "graph_category processes\n" +
					  "graph_args --base 1000 -l 0\n" +
					  "graph_vlabel Number of threads\n" +
					  "threads.label threads\n" +
					  "threads.min 0\n" +
					  "threads.draw LINE1\n" +
					  "multigraph processes_prio\n" +
					  "graph_title Process priority\n" +
					  "graph_category processes\n" +
					  "graph_args --base 1000 -l 0\n" +
					  "graph_vlabel Number of proccesses\n" +
					  "idle.label idle\n" +
					  "idle.min 0\n" +
					  "idle.draw AREA\n" +
					  "lowest.label below normal\n" +
					  "lowest.min 0\n" +
					  "lowest.draw STACK\n" +
					  "normal.label normal\n" +
					  "normal.min 0\n" +
					  "normal.draw STACK\n" +
					  "higher.label above normal\n" +
					  "higher.min 0\n" +
					  "higher.draw STACK\n" +
					  "high.label high\n" +
					  "high.min 0\n" +
					  "high.draw STACK\n" +
					  "highest.label realtime\n" +
					  "highest.min 0\n" +
					  "highest.draw STACK\n" +
					  "unknown.label unknown\n" +
					  "unknown.min 0\n" +
					  "unknown.draw STACK\n";

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
			var proccount = String.Format("multigraph processes_threads\nthreads.value {0}\nmultigraph processes_count\nprocesses.value {1}\n", threads, running);

			/*
			// Some system processes don't like being asked so these linq-queries will throw exceptions.
			var idle = processes.Count(_ => _.PriorityClass == ProcessPriorityClass.Idle);
			var lowest = processes.Count(_ => _.PriorityClass == ProcessPriorityClass.BelowNormal);
			var normal = processes.Count(_ => _.PriorityClass == ProcessPriorityClass.Normal);
			var higher = processes.Count(_ => _.PriorityClass == ProcessPriorityClass.AboveNormal);
			var high = processes.Count(_ => _.PriorityClass == ProcessPriorityClass.High);
			var highest = processes.Count(_ => _.PriorityClass == ProcessPriorityClass.RealTime);
			*/

			int idle = 0, lowest = 0, normal = 0, higher = 0, high = 0, highest = 0, unknown = 0;

			foreach (var process in processes)
			{
				try
				{
					switch (process.PriorityClass)
					{
						case ProcessPriorityClass.Idle:
							idle++;
							break;
						case ProcessPriorityClass.BelowNormal:
							lowest++;
							break;
						case ProcessPriorityClass.Normal:
							normal++;
							break;
						case ProcessPriorityClass.AboveNormal:
							higher++;
							break;
						case ProcessPriorityClass.High:
							high++;
							break;
						case ProcessPriorityClass.RealTime:
							highest++;
							break;
					}
				}
				catch (Win32Exception)
				{
					unknown++;
				}
			}

			var priocount = String.Format("multigraph processes_prio\nidle.value {0}\nlowest.value {1}\nnormal.value {2}\nhigher.value {3}\nhigh.value {4}\nhighest.value {5}\nunknown.value {6}\n", idle, lowest, normal, higher, high, highest, unknown);

			return proccount + priocount;
		}

		public override string GetName()
		{
			return "processes";
		}

	}
}
