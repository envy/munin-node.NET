using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace munin_node_Service
{
	public class Configuration
	{
		private Dictionary<string, Dictionary<string, string>> _config;

		public Configuration()
		{
		}

		public Configuration(string file)
		{
			LoadFile(file);
		}

		public void LoadFile(string file)
		{
			_config = new Dictionary<string, Dictionary<string, string>>();
			if (!File.Exists(file))
				return;
			TextReader tr = new StreamReader(file);
			string line;
			var currentsection = new Dictionary<string, string>();
			while ((line = tr.ReadLine()) != null)
			{
				line = line.Trim();
				if (line.Length == 0 || line.First() == '#')
				{
					continue; // do nothing with comments and emtpy lines
				}

				if (line.First() == '[' && line.Last() == ']')
				{
					var newsection = line.Trim(new[] {'[', ']'});
					
					if (_config.ContainsKey(newsection))
					{
						currentsection = _config[newsection];
					}
					else
					{
						currentsection = new Dictionary<string, string>();
						_config.Add(newsection, currentsection);
					}
				}

				var split = line.IndexOf('=');
				if (split < 0 || split == 0 || split == line.Length - 1)
				{
					continue; // do nothing if not a key=value pair
				}

				var key = line.Substring(0, split).Trim().ToLower();
				var value = line.Substring(split + 1).Trim();
				currentsection.Add(key, value);
			}
			tr.Close();
		}

		/// <summary>
		/// Return the config for a specific section. If the section does not exists, an empty dictionary si returned
		/// </summary>
		/// <param name="section"></param>
		/// <returns></returns>
		public Dictionary<string, string> GetSectionConfig(string section)
		{
			if (_config == null)
				throw new Exception("No config loaded!");

			return _config.ContainsKey(section) ? _config[section] : new Dictionary<string, string>();
		}
	}
}
