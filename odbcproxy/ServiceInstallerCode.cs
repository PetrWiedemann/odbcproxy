using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration.Install;
using System.ComponentModel;
using System.ServiceProcess;

namespace net.pdynet.odbcproxy
{
    [RunInstaller(true)]
    [System.ComponentModel.DesignerCategory("")]
    public class ServiceInstallerCode : Installer
    {
        public ServiceInstallerCode()
        {
            ServiceProcessInstaller serviceProcessInstaller = new ServiceProcessInstaller();
            ServiceInstaller serviceInstaller = new ServiceInstaller();

            serviceProcessInstaller.Account = ServiceAccount.LocalSystem;
            serviceProcessInstaller.Username = null;
            serviceProcessInstaller.Password = null;

            serviceInstaller.ServiceName = "OdbcProxy";
            serviceInstaller.DisplayName = "OdbcProxy";
            serviceInstaller.Description = "Web service ODBC proxy.";

            serviceInstaller.StartType = ServiceStartMode.Automatic;

            this.Installers.Add(serviceProcessInstaller);
            this.Installers.Add(serviceInstaller);
        }
    }
}
