using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;

namespace NetworkPlugin
{
	public class NetworkAdapter
	{
		public string Name { private set; get; }

		public string DeviceName { private set; get; }

		public string DeviceId { private set; get; }

		private readonly PerformanceCounter _bytespersecin;
		private readonly PerformanceCounter _bytespersecout;

		public NetworkAdapter(ManagementBaseObject adapter)
		{
			Name = (string)adapter["NetConnectionID"];
			DeviceName = (string)adapter["Name"];
			DeviceId = (string)adapter["DeviceID"];

			DeviceName = DeviceName.Replace('(', '[').Replace(')', ']'); // for intel: Intel(R) => Intel[R] in perfcounter
			DeviceName = DeviceName.Replace('#', '_'); // for cards with same name which are numbered like this: #2 #3 => _2 _3

			_bytespersecout = new PerformanceCounter("Network Adapter", "Bytes Sent/sec", DeviceName);
			_bytespersecin = new PerformanceCounter("Network Adapter", "Bytes Received/sec", DeviceName);

			_bytespersecin.NextValue();
			_bytespersecout.NextValue();
		}

		public double GetInBitsPerSec()
		{
			return _bytespersecin.NextValue()*8;
		}

		public double GetOutBitsPerSec()
		{
			return _bytespersecout.NextValue() * 8;
		}
	}
}
