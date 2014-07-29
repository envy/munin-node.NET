using System;
using System.Collections.Generic;
using NUnit.Framework;
using munin_node_Service;

namespace Tests.PluginTests
{
	[TestFixture]
	public abstract class MuninPluginTest<TPlugin> where TPlugin : PluginBase, new()
	{
		protected TPlugin Plugin;
		protected abstract string ExpectedName { get; }
		protected abstract PluginBase.Capabilities ExpectedCapabilities { get; }

		[SetUp]
		public void Setup()
		{
			Plugin = new TPlugin();
		}

		[Test]
		public void InitTest()
		{
			Plugin.Initialize(new Dictionary<string, string>());
		}

		[Test]
		public void NameTest()
		{
			string name = Plugin.GetName();
			Assert.That(name, Is.EqualTo(ExpectedName));
		}

		[Test]
		public void CapabilitiesTest()
		{
			PluginBase.Capabilities caps = Plugin.GetCapabilities();
			Assert.That(caps, Is.EqualTo(ExpectedCapabilities));
		}

		[Test]
		public void ConfigTest()
		{
			PluginBase.Capabilities caps = Plugin.GetCapabilities();

			foreach (PluginBase.Capabilities capability in Enum.GetValues(typeof(PluginBase.Capabilities)))
			{
				if (!caps.HasFlag(capability)) continue;
				string config = Plugin.GetConfig(capability);
				Assert.That(IsValidConfig(config, capability), Is.True);
			}
		}

		[Test]
		public void ValueTest()
		{
			PluginBase.Capabilities caps = Plugin.GetCapabilities();

			foreach (PluginBase.Capabilities capability in Enum.GetValues(typeof(PluginBase.Capabilities)))
			{
				if (!caps.HasFlag(capability)) continue;
				string config = Plugin.GetConfig(capability);
				Assert.That(IsValidValue(config, capability), Is.True);
			}
		}

		public bool IsValidConfig(string config, PluginBase.Capabilities caps)
		{
			// ToDo: Check if valid config
			return true;
		}

		public bool IsValidValue(string values, PluginBase.Capabilities caps)
		{
			// ToDo: Chekck if valid values
			return true;
		}
	}
}
