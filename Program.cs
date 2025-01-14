using System;
using System.ServiceProcess;

namespace MyFileMonitoringWindowsService
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
       /* static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new Service1()
            };
            ServiceBase.Run(ServicesToRun);
        }*/

        static void Main()
        {
            if (Environment.UserInteractive)
            {
                // Running in console mode
                Console.WriteLine("Running in console mode...");
                MyFileMonitoringWindowsService service = new MyFileMonitoringWindowsService();
                service.StartInConsole();
            }
            else
            {
                // Running as a Windows Service
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                    new MyFullServiceStateImplementation()
                };
                ServiceBase.Run(ServicesToRun);
            }

        }
    }
}
