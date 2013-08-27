using System;
using System.Diagnostics;
using munin_node_Service;

namespace DHCPPlugin
{
	public class DHCP : PluginBase
	{
		private PerformanceCounter _packetspersec;
		private PerformanceCounter _packetspersec6;
		private string _config;

		public override Capabilities GetCapabilities()
		{
			return Capabilities.None;
		}

		public override void Initialize()
		{
			if (!PerformanceCounterCategory.Exists("DHCP Server"))
			{
				throw new Exception("No DHCP performance counters found!");
			}

			_packetspersec = new PerformanceCounter("DHCP Server", "Packets Received/sec");
			_packetspersec.NextValue();
			_config = "graph_title DHCP traffic\n" +
					  "graph_args --base 1000 -l 0\n" +
					  "graph_category network\n" +
					  "graph_vlabel seconds\n" +
					  "graph_info This graph shows the received packets related to dhcp per second.\n" +
					  "dhcp.label IPv4 packets/sec\n" +
					  "dhcp.min 0\n" +
					  "dhcp.draw LINE1\n";

			if (!PerformanceCounterCategory.Exists("DHCP Server v6")) return;
			_packetspersec6 = new PerformanceCounter("DHCP Server v6", "Packets Received/sec");
			_packetspersec6.NextValue();
			_config += "dhcp6.label IPv6 packets/sec\n" +
			           "dhcp6.min 0\n" +
			           "dhcp6.draw LINE1\n";
		}

		public override string GetConfig(Capabilities withCapabilities)
		{
			return _config;
		}

		public override string GetValues(Capabilities withCapabilities)
		{
			return _packetspersec6 == null ? String.Format("dhcp.value {0}\n", _packetspersec.NextValue()) : String.Format("dhcp.value {0}\ndhcp6.value {1}\n", _packetspersec.NextValue(), _packetspersec6.NextValue());
		}

		public override string GetName()
		{
			return "dhcp";
		}
	}
}
