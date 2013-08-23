using System.Globalization;

namespace munin_node_Service
{
	public abstract class PluginBase
	{
		public string DoubleToString(double value)
		{
			return value.ToString("0.00", CultureInfo.InvariantCulture);
		}
		public abstract void Initialize();
		public abstract string GetConfig();
		public abstract string GetValues();
		public abstract string GetName();
	}
}
