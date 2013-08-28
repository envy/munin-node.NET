using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using munin_node_Service;

namespace DiskUsagePlugin
{
	public class DiskUsage : PluginBase
	{
		private List<DriveInfo> _drives;
		private string _config;

		public override Capabilities GetCapabilities()
		{
			return Capabilities.None;
		}

		public override void Initialize(Dictionary<string, string> config)
		{
			_drives = new List<DriveInfo>();
			foreach (var drive in DriveInfo.GetDrives().Where(drive => drive.DriveType == DriveType.Fixed && drive.IsReady))
			{
				_drives.Add(drive);
			}

			_config = "graph_title Volume usage in percent\n" +
					  "graph_args --upper-limit 100 -l 0\n" +
					  "graph_vlabel %\n" +
					  "graph_scale no\n" +
					  "graph_category disk\n";
			foreach (var drive in _drives)
			{
				var driveletter = drive.Name[0];
				var label = String.IsNullOrWhiteSpace(drive.VolumeLabel) ? drive.Name[0].ToString(CultureInfo.InvariantCulture) : String.Format("{0} ({1})", drive.Name[0], drive.VolumeLabel);
				_config += driveletter + ".label " + label + "\n" +
						   driveletter + ".warning 92\n" +
						   driveletter + ".critical 98\n";
			}
		}

		public override string GetConfig(Capabilities withCapabilities)
		{
			return _config;
		}

		public override string GetValues(Capabilities withCapabilities)
		{
			var values = "";
			foreach (var drive in _drives)
			{
				var driveletter = drive.Name[0];
				var space = DoubleToString((drive.TotalSize - drive.TotalFreeSpace) / (double)drive.TotalSize * 100.0);
				values += driveletter + ".value " + space + "\n";
			}

			return values;
		}

		public override string GetName()
		{
			return "df";
		}
	}
}
