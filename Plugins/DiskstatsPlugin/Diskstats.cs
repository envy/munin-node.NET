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
						new PerformanceCounter("PhysicalDisk", "Disk Reads/sec", drive), // ios
						new PerformanceCounter("PhysicalDisk", "Disk Writes/sec", drive),
						new PerformanceCounter("PhysicalDisk", "Avg. Disk Bytes/Read", drive),
						new PerformanceCounter("PhysicalDisk", "Avg. Disk Bytes/Write", drive),
						new PerformanceCounter("PhysicalDisk", "Avg. Disk sec/Transfer", drive), // latency
						new PerformanceCounter("PhysicalDisk", "Avg. Disk sec/Read", drive),
						new PerformanceCounter("PhysicalDisk", "Avg. Disk sec/Write", drive),
						new PerformanceCounter("PhysicalDisk", "Disk Read Bytes/sec", drive), // throughput
						new PerformanceCounter("PhysicalDisk", "Disk Write Bytes/sec", drive)
					};
				_drives.Add(drive, perfcounters);
			}

			var iopsconfig = "multigraph diskstats_iops\n" +
							  "graph_title Disk IOs per device\n" +
							  "graph_args --base 1000\n" +
							  "graph_vlabel IOs/${graph_period} read (-) / write (+)\n" +
							  "graph_category disk\n";

			var latencyconfig = "multigraph diskstats_latency\n" +
								  "graph_title Disk latency per device\n" +
								  "graph_args --base 1000\n" +
								  "graph_vlabel Average IO Wait (seconds)\n" +
								  "graph_category disk\n";

			var throughputconfig = "multigraph diskstats_throughput\n" +
			                       "graph_title Throughput per device\n" +
			                       "graph_args --base 1024\n" +
			                       "graph_vlabel Bytes/${graph_period} read (-) / write (+)\n" +
			                       "graph_category disk\n" +
			                       "graph_info This graph shows the average throuhput for the given disk in bytes.\n";

			foreach (var drive in _drives)
			{
				var drivenumber = "disk_" + drive.Key[0];
				var drivename = drivenumber;
				if (drive.Key.Contains(" "))
				{
					drivename += drive.Key.Split(' ').Aggregate((name, next) => name + ", " + next);
				}
				// TODO: show something else for drives with volumes without letters

				iopsconfig += drivenumber + "_rdio.label " + drivename + "\n" +
						   drivenumber + "_rdio.min 0\n" +
						   drivenumber + "_rdio.draw LINE1\n" +
						   drivenumber + "_rdio.graph no\n" +
						   drivenumber + "_wrio.label " + drivename + "\n" +
						   drivenumber + "_wrio.min 0\n" +
						   drivenumber + "_wrio.draw LINE1\n" +
						   drivenumber + "_wrio.negative " + drivenumber + "_rdio\n";

				latencyconfig += drivenumber + "_avgwait.label " + drivename + "\n" +
				                 drivenumber + "_avgwait.info Average wait time for an I/O request\n" +
				                 drivenumber + "_avgwait.min 0\n" +
				                 drivenumber + "_avgwait.draw LINE1\n";

				throughputconfig += drivenumber + "_rdbytes.label " + drivename + "\n" +
				                    drivenumber + "_rdbytes.min 0\n" +
				                    drivenumber + "_rdbytes.draw LINE1\n" +
				                    drivenumber + "_rdbytes.graph no\n" +
									drivenumber + "_wrbytes.label " + drivename + "\n" +
				                    drivenumber + "_wrbytes.min 0\n" +
				                    drivenumber + "_wrbytes.draw LINE1\n" +
				                    drivenumber + "_wrbytes.negative " + drivenumber + "_rdbytes\n";

				foreach (var counter in drive.Value)
					counter.NextValue();
			}



			foreach (var drive in _drives)
			{
				var drivenumber = "disk_" + drive.Key[0];
				var drivename = drivenumber;
				if (drive.Key.Contains(" "))
				{
					drivename += drive.Key.Split(' ').Aggregate((name, next) => name + ", " + next);
				}
				// TODO: show something else for drives with volumes without letters

				iopsconfig += "multigraph diskstats_iops." + drivenumber + "\n" +
						   "graph_title IOs for " + drivename + "\n" +
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

				latencyconfig += "multigraph diskstats_latency." + drivenumber + "\n" +
				                 "graph_title Average latency for " + drivename + "\n" +
				                 "graph_args --base 1000 --logarithmic\n" +
				                 "graph_vlabel seconds\n" +
				                 "graph_category disk\n" +
				                 "graph_info This graph shows average waiting time/latency for different categories of disk operations.\n" +
				                 "avgwait.label IO Wait time\n" +
				                 "avgwait.min 0\n" +
				                 "avgwait.draw LINE1\n" +
				                 "avgrdwait.label Read IO Wait time\n" +
				                 "avgrdwait.min 0\n" +
				                 "avgrdwait.warning 0:1\n" +
				                 "avgrdwait.draw LINE1\n" +
				                 "avgwrwait.label Write IO Wait time\n" +
				                 "avgwrwait.min 0\n" +
				                 "avgwrwait.warning 0:1\n" +
				                 "avgwrwait.draw LINE1\n";

				throughputconfig += "multigraph diskstats_throughput." + drivenumber + "\n" +
				                    "graph_title Disk throughput for " + drivename + "\n" +
				                    "graph_args --base 1024\n" +
				                    "graph_vlabel Pr ${graph_period} read(-) / write (+)\n" +
				                    "graph_category disk\n" +
				                    "graph_info This graph shows the disk throughput in bytes pr ${graph_period}\n" +
				                    "rdbytes.label invisible\n" +
				                    "rdbytes.min 0\n" +
				                    "rdbytes.draw LINE1\n" +
				                    "rdbytes.graph no\n" +
				                    "wrbytes.label Bytes\n" +
				                    "wrbytes.min 0\n" +
				                    "wrbytes.draw LINE1\n" +
				                    "wrbytes.negative rdbytes\n";
			}

			_config = iopsconfig + latencyconfig + throughputconfig;
		}

		public override string GetConfig(Capabilities withCapabilities)
		{
			return _config;
		}

		public override string GetValues(Capabilities withCapabilities)
		{
			var iopsvalues = "multigraph diskstats_iops\n";
			var latencyvalues = "multigraph diskstats_latency\n";
			var throughputvalues = "multigraph diskstats_throughput\n";
			var diskios = "";
			var latencies = "";
			var throughputs = "";
			foreach (var drive in _drives)
			{
				var drivenumber = "disk_" + drive.Key[0];
				var readspersec = DoubleToString(drive.Value[0].NextValue());
				var writespersec = DoubleToString(drive.Value[1].NextValue());
				var bytesperread = DoubleToString(drive.Value[2].NextValue() / 1000.0);
				var bytesperwrite = DoubleToString(drive.Value[3].NextValue() / 1000.0);
				var secpertransfer = DoubleToString(drive.Value[4].NextValue());
				var secperread = DoubleToString(drive.Value[5].NextValue());
				var secperwrite = DoubleToString(drive.Value[6].NextValue());
				var readbytespersec = DoubleToString(drive.Value[7].NextValue());
				var writebytespersec = DoubleToString(drive.Value[8].NextValue());

				iopsvalues += drivenumber + "_rdio.value " + readspersec + "\n" +
						  drivenumber + "_wrio.value " + writespersec + "\n";

				diskios += "multigraph diskstats_iops." + drivenumber + "\n" +
						   "rdio.value " + readspersec + "\n" +
						   "wrio.value " + writespersec + "\n" +
						   "avgrdrqsz.value " + bytesperread + "\n" +
						   "avgwrrqsz.value " + bytesperwrite + "\n";

				latencyvalues += drivenumber + "_avgwait.value " + secpertransfer + "\n";

				latencies += "multigraph diskstats_latency." + drivenumber + "\n" +
				             "avgwait.value " + secpertransfer + "\n" +
				             "avgrdwait.value " + secperread + "\n" +
				             "avgwrwait.value " + secperwrite + "\n";

				throughputvalues += drivenumber + "_rdbytes.value " + readbytespersec + "\n" +
				                    drivenumber + "_wrbytes.value " + writebytespersec + "\n";

				throughputs += "multigraph diskstats_throughput." + drivenumber + "\n" +
				               "rdbytes.value " + readbytespersec + "\n" +
				               "wrbytes.value " + writespersec + "\n";
			}
			return iopsvalues + diskios + latencyvalues + latencies + throughputvalues + throughputs;
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
