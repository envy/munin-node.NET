using System.Collections.Generic;
using System.Management;
using munin_node_Service;

namespace NetworkPlugin
{
	public class Network : PluginBase
	{
		private List<NetworkAdapter> _adapters;

		private string _config;

		public override Capabilities GetCapabilities()
		{
			return Capabilities.Multigraph;
		}

		public override void Initialize()
		{
			_adapters = new List<NetworkAdapter>();

			_config = "multigraph network\n" +
					  "graph_title Network traffic\n" +
					  "graph_args --base 1000\n" +
					  "graph_vlabel bits in (-) / out (+) per ${graph_period}\n" +
					  "graph_category network\n" +
					  "graph_info This graph shows the traffic for all interfaces in bits/sec not bytes/sec\n";

			var graphs = "";

			var adapters = new ManagementObjectSearcher(@"SELECT * FROM Win32_NetworkAdapter WHERE NetConnectionStatus = 2 AND PhysicalAdapter = 1 AND NOT PNPDeviceID LIKE 'ROOT\\%'");
			foreach (var adapter in adapters.Get())
			{
				var _if = new NetworkAdapter(adapter);
				_adapters.Add(_if);
				_config += "if_" + _if.DeviceId + "_down.label received\n" +
						   "if_" + _if.DeviceId + "_down.graph no\n" +
						   "if_" + _if.DeviceId + "_down.min 0\n" +
						   "if_" + _if.DeviceId + "_up.label " + _if.Name + " bps\n" +
						   "if_" + _if.DeviceId + "_up.negative if_" + _if.DeviceId + "_down\n" +
						   "if_" + _if.DeviceId + "_up.min 0\n" +
						   "if_" + _if.DeviceId + "_up.info Traffic of the " + _if.Name + " interface.\n";
				graphs += "multigraph network." + _if.DeviceId + "\n" +
				          "graph_title " + _if.Name + " traffic\n" +
				          "graph_args --base 1000\n" +
				          "graph_vlabel bits in (-) / out (+) per ${graph_period}\n" +
				          "graph_category network\n" +
				          "down.label bps\n" +
				          "down.graph no\n" +
				          "down.min 0\n" +
				          "up.label bps\n" +
				          "up.min 0\n" +
				          "up.negative down\n";
			}

			_config += graphs;
		}

		public override string GetConfig(Capabilities withCapabilities)
		{
			return _config;
		}

		public override string GetValues(Capabilities withCapabilities)
		{
			var values = "multigraph network\n";
			var graphs = "";
			
			foreach (var adapter in _adapters)
			{
				var down = DoubleToString(adapter.GetInBitsPerSec());
				var up = DoubleToString(adapter.GetOutBitsPerSec());
				values += "if_" + adapter.DeviceId + "_down.value " + down + "\n" +
						  "if_" + adapter.DeviceId + "_up.value " + up + "\n";
				graphs += "multigraph network." + adapter.DeviceId + "\n" +
				          "down.value " + down + "\n" +
				          "up.value " + up + "\n";
			}
			return values + graphs;
		}

		public override string GetName()
		{
			return "if";
		}
	}
}
