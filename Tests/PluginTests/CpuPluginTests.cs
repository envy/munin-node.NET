using CpuPlugin;
using munin_node_Service;

namespace Tests.PluginTests
{
    public class CpuPluginTests : MuninPluginTest<Cpu>
    {
	    protected override string ExpectedName
	    {
			get { return "cpu"; }
	    }

		protected override PluginBase.Capabilities ExpectedCapabilities
	    {
			get { return PluginBase.Capabilities.None; }
	    }
    }
}
