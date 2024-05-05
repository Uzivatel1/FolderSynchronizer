using System;
using System.IO;
using System.Timers;

namespace FolderSynchronizer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Set the default interval (in milliseconds) for the timer
            int intervalMilliseconds = 6000; // 6 seconds
            string sourceFolder = "WorkFolder";
            string targetFolder = "ReplicaFolder";
            string logFileName = "sync.log";
            string logFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), logFileName);

            // Check if command-line arguments are provided and parse them
            if (args.Length >= 3)
            {
                // Parse command-line arguments for interval, source folder, and target folder
                // Update intervalMilliseconds, sourceFolder, and targetFolder accordingly
                if (!int.TryParse(args[0], out int interval) || interval <= 0)
                {
                    Console.WriteLine("Invalid interval provided. Defaulting to 6 seconds.");
                }
                else
                {
                    intervalMilliseconds = interval;
                }

                sourceFolder = args[1];
                targetFolder = args[2];

                // Optional: check if provided folders exist and create them if they don't
                if (!Directory.Exists(sourceFolder))
                {
                    Directory.CreateDirectory(sourceFolder);
                    Console.WriteLine($"Folder '{sourceFolder}' was created.");
                    LogToFile(logFilePath, $"Folder '{sourceFolder}' was created at " + DateTime.Now.ToString());
                }

                if (!Directory.Exists(targetFolder))
                {
                    Directory.CreateDirectory(targetFolder);
                    Console.WriteLine($"Folder '{targetFolder}' was created.");
                    LogToFile(logFilePath, $"Folder '{targetFolder}' was created at " + DateTime.Now.ToString());
                }

                // Check if a log file path is provided
                if (args.Length >= 4)
                {
                    logFilePath = args[3];
                }
            }
            else
            {
                Console.WriteLine("Usage: Synchronizer <interval> <source_folder> <target_folder> [log_file]");
                return;
            }

            // Create a new Timer
            System.Timers.Timer timer = new(intervalMilliseconds);

            // Hook up the Elapsed event
            timer.Elapsed += (sender, e) => TimerElapsed(sender, e, sourceFolder, targetFolder, logFilePath);

            // Start the timer
            timer.Start();

            // Wait here to prevent the application from exiting immediately
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        static void TimerElapsed(object sender, ElapsedEventArgs e, string sourceFolder, string targetFolder, string logFilePath)
        {
            // Logic to execute periodically
            CopyFolder(sourceFolder, targetFolder, logFilePath);
            Console.WriteLine("Folders were synchronized.");

            // Log the synchronization to the specified log file
            LogToFile(logFilePath, "Folders synchronized at " + DateTime.Now.ToString());
        }

        static void CopyFolder(string sourceFolder, string targetFolder, string logFilePath)
        {
            // Check if the source folder exists
            if (!Directory.Exists(sourceFolder))
                throw new DirectoryNotFoundException($"Source folder '{sourceFolder}' does not exist.");

            // Check if the target folder exists, create if it doesn't
            if (!Directory.Exists(targetFolder))
                Directory.CreateDirectory(targetFolder);

            // Copy files from sourceFolder to targetFolder
            string[] files = Directory.GetFiles(sourceFolder);
            foreach (string file in files)
            {
                string fileName = Path.GetFileName(file);
                string destFile = Path.Combine(targetFolder, fileName);
                if (File.GetLastWriteTime(file) != File.GetLastWriteTime(destFile))
                {
                    File.Copy(file, destFile, true);
                    Console.WriteLine($"File '{fileName}' was copied.");
                    LogToFile(logFilePath, $"File '{fileName}' was copied at " + DateTime.Now.ToString());
                }
            }

            // Delete files in targetFolder that don't exist in sourceFolder
            string[] filesToDelete = Directory.GetFiles(targetFolder);
            foreach (string file in filesToDelete)
            {
                string fileName = Path.GetFileName(file);
                string sourceFile = Path.Combine(sourceFolder, fileName);
                if (!File.Exists(sourceFile))
                {
                    File.Delete(file);
                    Console.WriteLine($"File '{fileName}' was removed.");
                    LogToFile(logFilePath, $"File '{fileName}' was removed at " + DateTime.Now.ToString());
                }
            }

            // Copy subfolders recursively
            string[] subfolders = Directory.GetDirectories(sourceFolder);
            foreach (string subfolder in subfolders)
            {
                string subfolderName = Path.GetFileName(subfolder);
                string targetSubfolder = Path.Combine(targetFolder, subfolderName);
                if (Directory.GetLastWriteTime(subfolder) != Directory.GetLastWriteTime(targetSubfolder))
                {
                    CopyFolder(subfolder, targetSubfolder, logFilePath);
                    Directory.SetLastWriteTime(targetSubfolder, Directory.GetLastWriteTime(subfolder));
                    Console.WriteLine($"Folder '{subfolderName}' was copied.");
                    LogToFile(logFilePath, $"Folder '{subfolderName}' was copied at " + DateTime.Now.ToString());
                }
            }

            // Delete subfolders in targetFolder that don't exist in sourceFolder
            string[] foldersToDelete = Directory.GetDirectories(targetFolder);
            foreach (string folder in foldersToDelete)
            {
                string folderName = Path.GetFileName(folder);
                string sourceSubfolder = Path.Combine(sourceFolder, folderName);
                if (!Directory.Exists(sourceSubfolder))
                {
                    Directory.Delete(folder, true);
                    Console.WriteLine($"Folder '{folderName}' was removed.");
                    LogToFile(logFilePath, $"Folder '{folderName}' was removed at " + DateTime.Now.ToString());
                }
            }
        }

        static void LogToFile(string logFilePath, string message)
        {
            using StreamWriter writer = new(logFilePath, true);
            writer.WriteLine(message);
        }
    }
}