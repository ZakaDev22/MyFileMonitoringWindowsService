using System.Configuration;
using System.IO;


namespace MyFileMonitoringWindowsService
{
    public class clsGlobalMembers
    {
        public static string sourceFolder = ConfigurationManager.AppSettings["SourceFolder"];
        public static string destinationFolder = ConfigurationManager.AppSettings["DestinationFolder"];
        public static string logFolder = ConfigurationManager.AppSettings["LogFolder"];
        public static string logFilePath = Path.Combine(logFolder, "ServiceStateLog.txt");

        // Instance member for the FileSystemWatcher
        public static FileSystemWatcher watcher;
    }
}
