using System.ComponentModel;
using System.ServiceProcess;
using System.Configuration.Install;

namespace munin_node_Service
{
	[RunInstaller(true)]
	public class MuninServiceInstaller : Installer
	{
		public MuninServiceInstaller()
		{
			var serviceProcessInstaller = new ServiceProcessInstaller();
			var serviceInstaller = new ServiceInstaller();

			serviceProcessInstaller.Account = ServiceAccount.LocalSystem;
			serviceProcessInstaller.Username = null;
			serviceProcessInstaller.Password = null;

			serviceInstaller.DisplayName = "munin-node Service";
			serviceInstaller.StartType = ServiceStartMode.Automatic;

			serviceInstaller.ServiceName = "munin-node Service";

			Installers.Add(serviceProcessInstaller);
			Installers.Add(serviceInstaller);
		}
	}
}
