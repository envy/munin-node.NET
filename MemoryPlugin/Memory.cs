using System;
using System.Diagnostics;
using Microsoft.VisualBasic.Devices;
using munin_node_Service;

namespace MemoryPlugin
{
	public class Memory : PluginBase
	{
		private string _config;
		private ComputerInfo _info;
		private PerformanceCounter _cache;
		private PerformanceCounter _free;
		private PerformanceCounter _committed;
		private PerformanceCounter _swap;


		public override Capabilities GetCapabilities()
		{
			return Capabilities.None;
		}

		public override void Initialize()
		{
			_info = new ComputerInfo();
			_cache = new PerformanceCounter("Memory", "Cache Bytes");
			_free = new PerformanceCounter("Memory", "Free & Zero Page List Bytes");
			_committed = new PerformanceCounter("Memory", "Committed Bytes");
			_swap = new PerformanceCounter("Memory", "Pool Paged Bytes");

			_cache.NextValue();
			_free.NextValue();
			_committed.NextValue();
			_swap.NextValue();

			_config = "graph_args --base 1024 -l 0 --upper-limit " + _info.TotalPhysicalMemory + "\n" +
			          "graph_vlabel Bytes\n" +
			          "graph_title Memory Usage\n" +
			          "graph_category system\n" +
			          "graph_info This graph shows what the machine uses memory for.\n" +
			          "graph_order apps standby free swap\n" +
			          "apps.label apps\n" +
			          "apps.draw AREA\n" +
			          "apps.info Memory used by user-space applications\n" +
			          "standby.label standby\n" +
			          "standby.draw STACK\n" +
			          "standby.info Parked file and code cache.\n" +
			          "free.label unused\n" +
			          "free.draw STACK\n" +
			          "free.info Memory not used at all.\n" +
					  "swap.label swap\n" +
					  "swap.draw STACK\n" +
					  "swap.info Swap space used.\n" +
			          "committed.label committed\n" +
			          "committed.draw LINE2\n" +
			          "committed.info The amount of memory allocated to programs.\n";
		}

		public override string GetConfig(Capabilities withCapabilities)
		{
			return _config;
		}

		public override string GetValues(Capabilities withCapabilities)
		{
			var apps = _info.TotalPhysicalMemory - _info.AvailablePhysicalMemory;
			var free = (ulong)_free.RawValue;
			var committed = (ulong)_committed.RawValue;
			var standby = _info.AvailablePhysicalMemory - free;
			var swap = (ulong) _swap.RawValue;
			var values = String.Format("apps.value {0}\nstandby.value {1}\nfree.value {2}\nswap.value {3}\ncommitted.value {4}\n", apps, standby, free, swap, committed);
			return values;
		}

		public override string GetName()
		{
			return "memory";
		}
	}
}
