using munin_node_Service;

namespace munin_node_Standalone
{
	class Program
	{
		static void Main(string[] args)
		{
			(new MuninNode(null)).Start();
		}
	}
}
