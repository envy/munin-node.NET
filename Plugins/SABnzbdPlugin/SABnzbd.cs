using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml.Linq;
using munin_node_Service;

namespace SABnzbdPlugin
{
	public class SABnzbd : PluginBase
	{
		private string _apikey;
		private string _base;

		public override Capabilities GetCapabilities()
		{
			return Capabilities.None;
		}

		public override void Initialize(Dictionary<string, string> config)
		{
			if (!config.ContainsKey("host"))
			{
				throw new Exception("No host specified in config file");
			}
			string hostname = config["host"];

			var port = !config.ContainsKey("port") ? 8080 : int.Parse(config["port"]);
			_apikey = !config.ContainsKey("apikey") ? null : config["apikey"];

			_base = String.Format("http://{0}:{1}", hostname, port);
			WebRequest.DefaultWebProxy = null;
			var authmode = DownloadString("api?mode=auth").ToLower().Trim();
			if (authmode == "apikey" && _apikey == null)
			{
				throw new Exception("Server requires apikey, no apikey given in config file.");
			}
		}

		private string DownloadString(string url)
		{
			var request = (HttpWebRequest)WebRequest.Create(_base + "/" + url);
			request.Method = "GET";
			using (var response = (HttpWebResponse)request.GetResponse())
			{
				var stream = response.GetResponseStream();
				if (stream == null)
					return null;
				using (var reader = new StreamReader(stream))
				{
					return reader.ReadToEnd();
				}
			}

		}

		public override string GetConfig(Capabilities withCapabilities)
		{
			return "graph_title SABnzbd Queue status\n" +
				   "graph_args --base 1000\n" +
				   "graph_category services\n" +
				   "graph_vlabel bytes\n" +
				   "bleft.label bytes left\n" +
				   "bleft.min 0\n" +
				   "bleft.draw LINE1\n";
		}

		public override string GetValues(Capabilities withCapabilities)
		{
			var data = _apikey != null ? DownloadString("api?mode=queue&output=xml&apikey=" + _apikey) : DownloadString("api?mode=queue&output=xml" + _apikey);

			var xml = XDocument.Parse(data);
			var bleft = Double.Parse(xml.Descendants("queue").Select(_ => _.Element("mbleft")).First().Value, CultureInfo.InvariantCulture) * 1000000;
			return "bleft.value " + DoubleToString(bleft) + "\n";
		}

		public override string GetName()
		{
			return "sabnzbd";
		}
	}
}
