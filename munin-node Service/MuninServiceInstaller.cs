using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Configuration.Install;

namespace munin_node_Service
{
	[RunInstaller(true)]
	public class MuninServiceInstaller : Installer
	{
		public MuninServiceInstaller()
		{
			ServiceProcessInstaller serviceProcessInstaller = new ServiceProcessInstaller();
			ServiceInstaller serviceInstaller = new ServiceInstaller();

			serviceProcessInstaller.Account = ServiceAccount.LocalSystem;
			serviceProcessInstaller.Username = null;
			serviceProcessInstaller.Password = null;

			serviceInstaller.DisplayName = "munin-node Service";
			serviceInstaller.StartType = ServiceStartMode.Automatic;

			serviceInstaller.ServiceName = "munin-node Service";

			this.Installers.Add(serviceProcessInstaller);
			this.Installers.Add(serviceInstaller);
		}
	}
}
