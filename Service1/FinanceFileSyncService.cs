using System;
using System.IO;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service1
{
    public class FinanceFileSyncService
    {
        private FileSystemWatcher _watcher;
        private string _destinationBasePath;
        private string _baseFolder;
        private string _sourceBasePath;

        public bool Start()
        {
            //from app.config appSettings
            _destinationBasePath = ConfigurationManager.AppSettings["DestinationBasePath"].ToLower();
            _baseFolder = ConfigurationManager.AppSettings["BaseFolder"].ToLower();
            _sourceBasePath = ConfigurationManager.AppSettings["SourceBasePath"].ToLower();
            //the 
            var sourceFolderWatched = Path.Combine(_sourceBasePath,_baseFolder);
            _watcher = new FileSystemWatcher(sourceFolderWatched, "*.*");
            _watcher.Created += OnNewFile;
            _watcher.IncludeSubdirectories = true;
            _watcher.EnableRaisingEvents = true;

            
            return true;
        }

        public bool Stop()
        {
            _watcher.Dispose();
            return true;
        }

        public void OnNewFile(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine("***OnNewFile:");

            //source full path (includes the name of the file (or e.Name = the name of the new folder)
            var srcFullPath = e.FullPath;
            
            FileAttributes attr = System.IO.File.GetAttributes(e.FullPath);

            //if it's a new folder, do nothing
            //you may want to handle new folders here in the future 
            if ((attr & FileAttributes.Directory) != 0)
            {
                return;
            }

            //at this point we are dealing with files only

            //this removes the file name leaving only the dir
            var srcDirName = Path.GetDirectoryName(srcFullPath);
            Console.WriteLine("srcDirName: " + srcDirName);
            
            //this gets the destination folder (it's created if it doesn't exist)  
            var destinationFolder = GetDestinationPath(srcDirName);
            Console.WriteLine("destinationFolder: " + destinationFolder);

            //copy the file to destination
            File.Copy(e.FullPath, Path.Combine(destinationFolder, Path.GetFileName(e.FullPath)));

        }

        private string GetDestinationPath(string srcDirName)
        {
            Console.WriteLine("***GetDestinationPath:");

            //if the file is in destination base folder then use dest base path 
            if (srcDirName.EndsWith(_baseFolder))
                return Path.Combine(_destinationBasePath, _baseFolder);

            //if there are there any subfolders after the base folder - grab them and form the destination folder
            var pos = 0;
            var foldersAfterBase = "";
            
            pos = srcDirName.IndexOf(_baseFolder, StringComparison.CurrentCulture);
            foldersAfterBase = srcDirName.Substring(_baseFolder.Length + pos + 1);

            var destinationFolder = Path.Combine(_destinationBasePath, _baseFolder, foldersAfterBase);
            
            //this will create the folder if it doesn't exist otherwise just returns directory info on the existing folder
            //returns directory info on the created folder as well
            var directoryInfo = Directory.CreateDirectory(destinationFolder);

            return destinationFolder;
            
        }
    }
}