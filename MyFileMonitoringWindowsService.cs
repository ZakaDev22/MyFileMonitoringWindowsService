using System;
using System.Configuration;
using System.IO;
using System.ServiceProcess;
using System.Threading;


namespace MyFileMonitoringWindowsService
{
    public partial class MyFileMonitoringWindowsService : ServiceBase
    {
        private static string sourceFolder = ConfigurationManager.AppSettings["SourceFolder"];
        private static string destinationFolder = ConfigurationManager.AppSettings["DestinationFolder"];
        private static string logFolder = ConfigurationManager.AppSettings["LogFolder"];
        private static string logFilePath = Path.Combine(logFolder, "ServiceStateLog.txt");

        // Instance member for the FileSystemWatcher
        private FileSystemWatcher watcher;

        public static string GenerateGUID()
        {

            // Generate a new GUID
            Guid newGuid = Guid.NewGuid();

            // convert the GUID to a string
            return newGuid.ToString();

        }

        public static bool CreateFolderIfDoesNotExist(string FolderPath)
        {

            // Check if the folder exists
            if (!Directory.Exists(FolderPath))
            {
                try
                {
                    // If it doesn't exist, create the folder
                    Directory.CreateDirectory(FolderPath);
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }

            return true;

        }

        public MyFileMonitoringWindowsService()
        {
            InitializeComponent();
        }

        // Log a message to a file with a timestamp
        //The service logs all its state transitions (Start, Stop, Pause, Continue, Shutdown) to a file named ServiceStateLog.txt in the configured directory.
        // Each log entry includes a timestamp for tracking purposes.
        private void LogServiceEvent(string message)
        {
            string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}\n";
            File.AppendAllText(logFilePath, logMessage);

            // Write to console if running interactively
            if (Environment.UserInteractive)
            {
                Console.WriteLine(logMessage);
            }
        }

        protected override void OnStart(string[] args)
        {

            // Ensure the folders exist
            CreateFolderIfDoesNotExist(sourceFolder);
            CreateFolderIfDoesNotExist(logFolder);

            // Log service start
            LogServiceEvent("Service started.");

            // Initialize the FileSystemWatcher
            watcher = new FileSystemWatcher
            {
                Path = sourceFolder,
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.DirectoryName,
                Filter = "*.*", // Monitor all file types
                EnableRaisingEvents = true // Start monitoring
            };

            // Hook up event handler for file creation
            watcher.Created += OnFileCreated;
            watcher.EnableRaisingEvents = true;

            // Log the start of file monitoring
            LogServiceEvent($"File monitoring started on folder: {sourceFolder}");
        }

        protected override void OnStop()
        {

            // Log service stop
            LogServiceEvent("Service stopped.");

            // Stop the FileSystemWatcher
            watcher.EnableRaisingEvents = false;
            watcher.Dispose();
        }

        private void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            // Handle the file creation event
            LogServiceEvent($"File detected: {e.FullPath}");

            try
            {
                // Give some time for the file name to stabilize
                Thread.Sleep(3000);

                // Get the file path from e.FullPath (temporary name)
                string sourceFilePath = e.FullPath;
                string extension = Path.GetExtension(sourceFilePath);

                // Check if it's the only file with the .txt extension in the folder
                string[] files = Directory.GetFiles(sourceFolder, "*.txt");

                if (files.Length == 1)
                {
                    // Assume the file is the one that was just created
                    string actualFilePath = files[0];

                    // Generate the new file name with GUID
                    string newFileName = Path.Combine(destinationFolder, GenerateGUID() + extension);

                    // Move and rename the file
                    File.Move(actualFilePath, newFileName);
                    LogServiceEvent($"File renamed and moved: {actualFilePath} -> {newFileName}");

                }
            }
            catch (Exception ex)
            {
                LogServiceEvent($"Error processing file {e.FullPath}: {ex.Message}");
            }
        }



        public void StartInConsole()
        {
            // Start monitoring files in console mode
            FileSystemWatcher watcher = new FileSystemWatcher
            {
                Path = sourceFolder,
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.DirectoryName,
                Filter = "*.*", // Monitor all file types
                EnableRaisingEvents = true // Start monitoring
            };
            watcher.Created += OnFileCreated;
            watcher.EnableRaisingEvents = true;
            Console.WriteLine("Press Enter to stop the service...");
            Console.ReadLine();
            watcher.Dispose();
        }
    }
}
