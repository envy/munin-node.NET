using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using munin_node_Service;

namespace DiskstatsPlugin
{
	public class Diskstats : PluginBase
	{
		private SortedDictionary<string, List<PerformanceCounter>> _drives;
		private string _config;

		public override Capabilities GetCapabilities()
		{
			return Capabilities.Multigraph;
		}

		public override void Initialize()
		{
			_drives = new SortedDictionary<string, List<PerformanceCounter>>();

			var pcc = new PerformanceCounterCategory("PhysicalDisk");

			foreach (var drive in pcc.GetInstanceNames().Where(drive => !drive.Equals("_Total")))
			{
				var perfcounters = new List<PerformanceCounter>
					{
						new PerformanceCounter("PhysicalDisk", "Disk Reads/sec", drive),
						new PerformanceCounter("PhysicalDisk", "Disk Writes/sec", drive),
						new PerformanceCounter("PhysicalDisk", "Avg. Disk Bytes/Read", drive),
						new PerformanceCounter("PhysicalDisk", "Avg. Disk Bytes/Write", drive)
					};
				_drives.Add(drive, perfcounters);
			}

			_config = "multigraph diskstats_iops\n" +
			          "graph_title Disk IOs per device\n" +
			          "graph_args --base 1000\n" +
			          "graph_vlabel IOs/${graph_period} read (-) / write (+)\n" +
			          "graph_category disk\n";
			foreach (var drive in _drives)
			{
				var driveletter = drive.Key.Reverse().ToArray()[1];
				_config += driveletter + "_rdio.label " + driveletter + ":\n" +
				           driveletter + "_rdio.min 0\n" +
				           driveletter + "_rdio.draw LINE1\n" +
				           driveletter + "_rdio.graph no\n" +
				           driveletter + "_wrio.label " + driveletter + ":\n" +
				           driveletter + "_wrio.min 0\n" +
				           driveletter + "_wrio.draw LINE1\n" +
				           driveletter + "_wrio.negative " + driveletter + "_rdio\n";
				foreach (var counter in drive.Value)
					counter.NextValue();
			}

			foreach (var drive in _drives)
			{
				var driveletter = drive.Key.Reverse().ToArray()[1];
				_config += "multigraph diskstats_iops." + driveletter + "\n" +
				           "graph_title IOs for " + driveletter + "\n" +
				           "graph_args --base 1000\n" +
				           "graph_vlabel Units read (-) / write (+)\n" +
				           "graph_category disk\n" +
				           "graph_info This graph shows the number auf IO operations per second and the average size of these requests.\n" +
				           "rdio.label dummy\n" +
				           "rdio.min 0\n" +
				           "rdio.draw LINE1\n" +
				           "rdio.graph no\n" +
				           "wrio.label IO/sec\n" +
				           "wrio.min 0\n" +
				           "wrio.draw LINE1\n" +
				           "wrio.negative rdio\n" +
				           "avgrdrqsz.label dummy\n" +
				           "avgrdrqsz.min 0\n" +
				           "avgrdrqsz.draw LINE1\n" +
				           "avgrdrqsz.graph 0\n" +
				           "avgwrrqsz.label Req Size (KB)\n" +
				           "avgwrrqsz.info Average request size in kilobytes (1000 based)\n" +
				           "avgwrrqsz.min 0\n" +
				           "avgwrrqsz.draw LINE1\n" +
				           "avgwrrqsz.negative avgrdrqsz\n";
			}
		}

		public override string GetConfig(Capabilities withCapabilities)
		{
			return _config;
		}

		public override string GetValues(Capabilities withCapabilities)
		{
			var values = "multigraph diskstats_iops\n";
			var diskios = "";
			foreach (var drive in _drives)
			{
				var driveletter = drive.Key.Reverse().ToArray()[1];
				var readspersec = DoubleToString(drive.Value[0].NextValue());
				var writespersec = DoubleToString(drive.Value[1].NextValue());
				var bytesperread = DoubleToString(drive.Value[2].NextValue()/1000.0);
				var bytesperwrite = DoubleToString(drive.Value[3].NextValue()/1000.0);

				values += driveletter + "_rdio.value " + readspersec + "\n" +
						  driveletter + "_wrio.value " + writespersec + "\n";

				diskios += "multigraph diskstats_iops." + driveletter + "\n" +
				           "rdio.value " + readspersec + "\n" +
				           "wrio.value " + writespersec + "\n" + 
						   "avgrdrqsz.value " + bytesperread + "\n" +
				           "avgwrrqsz.value " + bytesperwrite + "\n";
			}
			return values + diskios;
		}

		public override string GetName()
		{
			return "diskstats";
		}

		public class KeyComparer : IComparer<string>
		{
			public int Compare(string x, string y)
			{
				return String.Compare(x, y, StringComparison.Ordinal);
			}
		}
	}
}
