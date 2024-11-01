using System;
using System.IO;
using System.Timers;

namespace FolderSynchronizer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Set defaults: timer for periodic synchronization, source folder, target folder, and log file
            int intervalMilliseconds = 6000;
            string sourceFolder = "SourceFolder";
            string targetFolder = "TargetFolder";

            // Look for the log file in the Documents folder by default
            string logFileName = "sync.log";
            string logFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), logFileName);

            // Check if command-line arguments are provided and parse them
            if (args.Length >= 3)
            {
                // Parse the interval (first argument)
                if (!int.TryParse(args[0], out int interval) || interval <= 0)
                {
                    Console.WriteLine("Invalid interval provided. Defaulting to 6 seconds.");
                }
                else
                {
                    intervalMilliseconds = interval;
                }

                // Set source and target folders from arguments
                sourceFolder = args[1];
                targetFolder = args[2];

                // Only override logFilePath if a fourth argument is provided
                if (args.Length >= 4)
                {
                    logFilePath = args[3];
                }
            }
            else
            {
                // Display usage if insufficient arguments
                Console.WriteLine("Usage: Synchronizer <interval> <source_folder> <target_folder> [log_file]");
                return;
            }

            // Ensure log directory exists after logFilePath is finalized
            string logDirectory = Path.GetDirectoryName(logFilePath);

            // Check and create log directory if it does not exist
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
                Console.WriteLine($"Log directory '{logDirectory}' was created.");
            }
            else
            {
                Console.WriteLine($"Log directory '{logDirectory}' already exists.");
            }

            // Ensure source and target folders exist
            if (!Directory.Exists(sourceFolder))
            {
                Directory.CreateDirectory(sourceFolder);
                Console.WriteLine($"Folder '{sourceFolder}' was created.");
            }
            else
            {
                Console.WriteLine($"Folder '{sourceFolder}' already exists.");
            }

            if (!Directory.Exists(targetFolder))
            {
                Directory.CreateDirectory(targetFolder);
                Console.WriteLine($"Folder '{targetFolder}' was created.");
            }
            else
            {
                Console.WriteLine($"Folder '{targetFolder}' already exists.");
            }

            // Create a new Timer
            System.Timers.Timer timer = new(intervalMilliseconds);
            timer.Elapsed += (sender, e) => TimerElapsed(sender, e, sourceFolder, targetFolder, logFilePath);
            timer.Start();

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