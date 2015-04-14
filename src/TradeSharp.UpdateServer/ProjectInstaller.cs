using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.ServiceProcess;


namespace TradeSharp.UpdateServer
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();
        }

        private void SetServicePropertiesFromCommandLine(ServiceInstaller serviceInstaller)
        {
            string[] commandlineArgs = Environment.GetCommandLineArgs();

            string servicename;
            string servicedisplayname;
            ParseServiceNameSwitches(commandlineArgs, out servicename, out servicedisplayname);

            serviceInstaller.ServiceName = servicename;
            serviceInstaller.DisplayName = servicedisplayname;
        }

        private void ParseServiceNameSwitches(string[] commandlineArgs, out string serviceName, out string serviceDisplayName)
        {
            var servicenameswitch = (from s in commandlineArgs where s.StartsWith("/servicename") select s).FirstOrDefault();
            var servicedisplaynameswitch = (from s in commandlineArgs where s.StartsWith("/servicedisplayname") select s).FirstOrDefault();

            if (servicenameswitch == null)
                throw new ArgumentException("Argument 'servicename' is missing");
            if (servicedisplaynameswitch == null)
                throw new ArgumentException("Argument 'servicedisplayname' is missing");
            if (!(servicenameswitch.Contains('=') || servicenameswitch.Split('=').Length < 2))
                throw new ArgumentNullException("The /servicename switch is malformed");

            if (!(servicedisplaynameswitch.Contains('=') || servicedisplaynameswitch.Split('=').Length < 2))
                throw new ArgumentNullException("The /servicedisplaynameswitch switch is malformed");

            serviceName = servicenameswitch.Split('=')[1];
            serviceDisplayName = servicedisplaynameswitch.Split('=')[1];

            serviceName = serviceName.Trim('"');
            serviceDisplayName = serviceDisplayName.Trim('"');
        }
    }
}
