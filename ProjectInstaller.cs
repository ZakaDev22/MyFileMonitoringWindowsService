using System.ComponentModel;
using System.ServiceProcess;

namespace MyFileMonitoringWindowsService
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        private ServiceProcessInstaller processInstaller;
        private ServiceInstaller serviceInstaller;

        public ProjectInstaller()
        {
            InitializeComponent();

            // Initialize ServiceProcessInstaller
            processInstaller = new ServiceProcessInstaller
            {
                // Run the service under the local system account
                Account = ServiceAccount.LocalSystem
            };

            // Initialize ServiceInstaller
            serviceInstaller = new ServiceInstaller
            {
                // Set the name of the service
                ServiceName = "MyFileMonitoringWindowsService",
                DisplayName = "My File Monitor Service State Implementation Example",
                Description = "A Windows Service that demonstrates all service states and events.",
                StartType = ServiceStartMode.Manual
            };

            // Add both installers to the Installers collection
            Installers.Add(processInstaller);
            Installers.Add(serviceInstaller);
        }
    }
}
