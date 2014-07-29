using System;
using System.Collections.Generic;
using System.Globalization;

namespace munin_node_Service
{
	public abstract class PluginBase
	{
		/// <summary>
		/// Helper method to format doubles to two decimal places and a dot decimal separator.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string DoubleToString(double value)
		{
			return value.ToString(CultureInfo.InvariantCulture);
		}

		[Flags]
		public enum Capabilities
		{
			/// <summary>
			/// Your plugin support neither multigraph nor dirtyconfig. It will always be queried.
			/// </summary>
			None = 0,
			/// <summary>
			/// If your plugin reports dirtyconfig capability, it will always be queried, so please check the withCapabilites variable if you should fetch during your config.
			/// </summary>
			DirtyConfig = 1,
			/// <summary>
			/// If your plugin reports multigraph capability, it will only be queried if the munin server reports that it understands multigraph.
			/// </summary>
			Multigraph = 2,
			/// <summary>
			/// See <see cref="PluginBase.Capabilities.DirtyConfig"/> and <see cref="PluginBase.Capabilities.Multigraph"/>
			/// </summary>
			DirtyConfigMultigraph = 3,
		}

		/// <summary>
		/// Return which Capabilities your plugin supports.
		/// </summary>
		/// <returns></returns>
		public abstract Capabilities GetCapabilities();

		/// <summary>
		/// Do all your startuplogic here.
		/// </summary>
		/// <param name="config">The plugin configuration as specified in munin-node.cfg. If there is no config, an empty dictionary is given.</param>
		public abstract void Initialize(Dictionary<string, string> config);

		/// <summary>
		/// Return your config values for graph building.
		/// Please watch the withCapabilities variable for dirtyconfig if your plugin says it supports it.
		/// </summary>
		/// <param name="withCapabilities">The capabilities of the asking munin server</param>
		/// <returns></returns>
		public abstract string GetConfig(Capabilities withCapabilities);

		/// <summary>
		/// Return your values for graph building
		/// </summary>
		/// <param name="withCapabilities">The capabilities of the asking munin server</param>
		/// <returns></returns>
		public abstract string GetValues(Capabilities withCapabilities);

		/// <summary>
		/// Return your plugin name.
		/// </summary>
		/// <returns></returns>
		public abstract string GetName();
	}
}
