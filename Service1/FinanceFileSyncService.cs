using System;
using System.IO;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Topshelf.Logging;

namespace Service1
{
    public class FinanceFileSyncService
    {
        private FileSystemWatcher _watcher;
        private static readonly LogWriter Log = HostLogger.Get<FinanceFileSyncService>();
        private string _destinationBasePath;
        private string _baseFolder;
        private string _sourceBasePath;

        public bool Start()
        {

            //from app.config appSettings
            _destinationBasePath = ConfigurationManager.AppSettings["DestinationBasePath"].ToLower();
            _baseFolder = ConfigurationManager.AppSettings["BaseFolder"].ToLower();
            _sourceBasePath = ConfigurationManager.AppSettings["SourceBasePath"].ToLower();
             
            var sourceFolderWatched = Path.Combine(_sourceBasePath,_baseFolder);

            _watcher = new FileSystemWatcher(sourceFolderWatched);
            //_watcher.Filter = "*.*";  //this is the default
            _watcher.Created += OnNewFile;
            _watcher.Renamed += OnRenamed;
            _watcher.IncludeSubdirectories = true;
            _watcher.EnableRaisingEvents = true;
            
            return true;
        }

        private void OnRenamed(object sender, RenamedEventArgs e)
        {
            Log.WarnFormat("***OnRenamed, source path: {0}, oldName: {1} ", e.FullPath, e.OldName);

            //this is used to distinguish between a file or folder (for the end point)
            FileAttributes attr = System.IO.File.GetAttributes(e.FullPath);

            //check if it's a folder
            if ((attr & FileAttributes.Directory) != 0)
            {
                var oldFolderName = Path.GetFileName(e.OldFullPath);
                if (oldFolderName != null && oldFolderName.ToLower() == "new folder")
                {
                    //if the old name is new folder then just create the renamed folder
                    var destPath = GetDestinationPath(e.FullPath, false);

                }
                else //rename the old folder
                {
                    var destOldPath = GetDestinationPath(e.OldFullPath,false);
                    var destNewPath = GetDestinationPath(e.FullPath, false);
                    //var newFolderName = Path.GetFileName(e.FullPath);
                    Directory.Move(destOldPath, destNewPath);
                }
            }

            //you have to get the destination old path and then you can rename the folder

            }

        public bool Pause()
        {
            _watcher.EnableRaisingEvents = false;
            return true;
        }

        public bool Continue()
        {
            _watcher.EnableRaisingEvents = true;
            return true;
        }

        public bool Stop()
        {
            _watcher.Dispose();
            return true;
        }

        public void CustomCommand(int commandNumber)
        {
            
            Log.InfoFormat("Custom command being called here with custom number: '{0}'", commandNumber.ToString());
        }

        /// <summary>
        /// This function responds to any new file/folder 
        /// if it's a folder - do nothing
        /// if it's a file - create the folder if it doesn't exist
        /// copy the source file to the destination folder
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">
        /// FileSystemEventArgs e gives you path information about the new file/folder
        /// </param>
        public void OnNewFile(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine("***OnNewFile:");
            Log.InfoFormat("***OnNewFile:");
            Log.InfoFormat("source fullPath: '{0}'",e.FullPath);
            
            //source full path (includes the name of the file (or e.Name = the name of the new folder)
            var sourceFullPath = e.FullPath;

            //this is used to distinguish between a file or folder (for the end point)
            FileAttributes attr = System.IO.File.GetAttributes(sourceFullPath);
            
            //if it's a folder then do nothing now
            //new folders will get created when there is a new file added to them 
            if ((attr & FileAttributes.Directory) != 0)
            {
                return;
            }

            //at this point we are dealing with files only
            //this removes the file name leaving only the path
            var sourcePath = Path.GetDirectoryName(sourceFullPath);
            Console.WriteLine("source path = " + sourcePath);
            
            //this gets the destination folder (it's created if it doesn't exist)  
            var destinationFolder = GetDestinationPath(sourcePath, true);
            Console.WriteLine("destinationFolder: " + destinationFolder);

            var fileName = Path.GetFileName(e.FullPath); 
            
            //copy the file to destination
            if (fileName != null)
            {
                File.Copy(e.FullPath, Path.Combine(destinationFolder, fileName));
                Log.WarnFormat("File {0} was copied to {1}.", fileName, destinationFolder);
            }
        }

        /// <summary>
        /// This will return the destination path based on the source path passed in
        /// Optional - will create the destination path if the second parameter is true
        /// </summary>
        /// <param name="srcDirName"></param>
        /// <param name="createDestinationFolder"></param>
        /// <returns>The destination path</returns>
        private string GetDestinationPath(string srcDirName, bool createDestinationFolder=false)
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
            if (createDestinationFolder)
            {
                var directoryInfo = Directory.CreateDirectory(destinationFolder);
            }

            return destinationFolder;
            
        }
    }
}