using System;
using System.IO;
using System.ServiceProcess;


namespace MyFileMonitoringWindowsService
{
    public partial class MyFileMonitoringWindowsService : ServiceBase
    {
        public MyFileMonitoringWindowsService()
        {
            InitializeComponent();

            // Set the CanPauseAndContinue property to true
            CanPauseAndContinue = true; //The service supports pausing and resuming operations.

            // Enable support for OnShutdown
            CanShutdown = true; // The service is notified when the system shuts down.

            // Ensure the folders exist
            clsLogAndValidation.CreateFolderIfDoesNotExist(clsGlobalMembers.sourceFolder);
            clsLogAndValidation.CreateFolderIfDoesNotExist(clsGlobalMembers.destinationFolder);
            clsLogAndValidation.CreateFolderIfDoesNotExist(clsGlobalMembers.logFolder);
        }



        protected override void OnStart(string[] args)
        {

            // Log service start
            clsLogAndValidation.LogServiceEvent("Service started. [Service Side]");

            // Initialize the FileSystemWatcher
            clsGlobalMembers.watcher = new FileSystemWatcher
            {
                Path = clsGlobalMembers.sourceFolder,
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.DirectoryName,
                Filter = "*.*", // Monitor all file types
                EnableRaisingEvents = true // Start monitoring
            };

            // Hook up event handler for file creation
            clsGlobalMembers.watcher.Created += OnFileCreated;
            clsGlobalMembers.watcher.EnableRaisingEvents = true;

            // Log the start of file monitoring
            clsLogAndValidation.LogServiceEvent($"File monitoring started on folder: {clsGlobalMembers.sourceFolder}");
        }

        protected override void OnStop()
        {

            // Log service stop
            clsLogAndValidation.LogServiceEvent("Service stopped. [Service Side]");

            // Stop the FileSystemWatcher
            clsGlobalMembers.watcher.EnableRaisingEvents = false;
            clsGlobalMembers.watcher.Dispose();
        }

        // OnPause Event
        protected override void OnPause()
        {
            clsLogAndValidation.LogServiceEvent("Service Paused");
            clsGlobalMembers.watcher.EnableRaisingEvents = false;
        }

        // OnContinue Event
        protected override void OnContinue()
        {
            clsLogAndValidation.LogServiceEvent("Service Resumed");
            clsGlobalMembers.watcher.EnableRaisingEvents = true;
        }

        // OnShutdown Event
        protected override void OnShutdown()
        {
            clsLogAndValidation.LogServiceEvent("Service Shutdown due to system shutdown");
            // Stop the FileSystemWatcher
            clsGlobalMembers.watcher.EnableRaisingEvents = false;
            clsGlobalMembers.watcher.Dispose();
        }

        private void OnFileCreated(object sender, FileSystemEventArgs e)
        {

            try
            {
                // Give some time for the file name to stabilize If Its Created Inside The source Folder
                // Thread.Sleep(3000);

                while (!IsFileStillInTransition(e.FullPath))
                {
                    continue;
                }

                // Handle the file creation event
                clsLogAndValidation.LogServiceEvent($"File detected: {e.FullPath}");

                // Get the file path from e.FullPath (temporary name)
                string extension = Path.GetExtension(e.FullPath);

                // Get Any File Created In The Source Folder 
                string[] files = Directory.GetFiles(clsGlobalMembers.sourceFolder, "*.*");

                if (files.Length == 1)
                {
                    // Assume the file is the one that was just created or Pasted From Another Folder
                    string actualFilePath = files[0];

                    // Generate the new file name with GUID
                    string newFileName = Path.Combine(clsGlobalMembers.destinationFolder, clsLogAndValidation.GenerateGUID() + extension);

                    // Move and rename the file
                    File.Move(actualFilePath, newFileName);
                    clsLogAndValidation.LogServiceEvent($"File renamed and moved: {actualFilePath} -> {newFileName}");

                }
                else
                    clsLogAndValidation.LogServiceEvent($"Error, There is More Than One File In The Source Folder ?");
            }
            catch (Exception ex)
            {
                clsLogAndValidation.LogServiceEvent($"Error processing file {e.FullPath}: {ex.Message}");
            }
        }

        // check if The File  want The Create Event To Move it To The Destination Folder Is Completed Other ways I dont Want The Event To Rise If This Function Return False
        private static bool IsFileStillInTransition(string filePath)
        {
            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }


        public void StartInConsole()
        {
            // Log service stop
            clsLogAndValidation.LogServiceEvent("Service Start. [Console Side]");

            // Start monitoring files in console mode
            clsGlobalMembers.watcher = new FileSystemWatcher
            {
                Path = clsGlobalMembers.sourceFolder,
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.DirectoryName,
                Filter = "*.*", // Monitor all file types
                EnableRaisingEvents = true // Start monitoring
            };

            clsGlobalMembers.watcher.Created += OnFileCreated;
            clsGlobalMembers.watcher.EnableRaisingEvents = true;
            Console.WriteLine("Press Enter to stop the service...");
            Console.ReadLine();

            clsGlobalMembers.watcher.Dispose();

            // Log service stop
            clsLogAndValidation.LogServiceEvent("Service stopped. [Console Side]");
        }


    }
}
