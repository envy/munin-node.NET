using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace munin_node_Service
{
	public class MuninNodeTestStarter
	{
		public static void Main()
		{
			(new MuninNode(null)).Start();
		}
	}
}
