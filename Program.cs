using System;
using System.ServiceProcess;

namespace MyFileMonitoringWindowsService
{
    internal static class Program
    {


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
                    new MyFileMonitoringWindowsService()
                };
                ServiceBase.Run(ServicesToRun);
            }

        }
    }
}
