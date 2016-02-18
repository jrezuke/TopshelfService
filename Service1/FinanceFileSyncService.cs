using System;
using System.IO;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service1
{
    public class FinanceFileSyncService
    {
        private FileSystemWatcher _watcher;
        private string _destinationBasePath;
        private string _destinationBaseFolder;

        public bool Start()
        {
            _watcher = new FileSystemWatcher(@"C:\Users\jojo\Documents\Finance", "*.*");
            _watcher.Created += OnNewFile;
            _watcher.IncludeSubdirectories = true;
            _watcher.EnableRaisingEvents = true;

            //from app.config appSettings
            _destinationBasePath = ConfigurationManager.AppSettings["DestinationBasePath"].ToString();
            _destinationBaseFolder = ConfigurationManager.AppSettings["DestinationBaseFolder"].ToString();
            return true;
        }

        public bool Stop()
        {
            _watcher.Dispose();
            return true;
        }

        public void OnNewFile(object sender, FileSystemEventArgs e)
        {
            //source full path (includes the name)
            var srcFullPath = e.FullPath;
            //this removes the file name leaving only the dir
            var srcDirName = Path.GetDirectoryName(srcFullPath);

            //check if folder exists (recursively if needed), if not then create it  
            CreateFolderNoExist(srcDirName);
            

            

            Console.WriteLine("Destination base path: " + Path.GetDirectoryName(_destinationBasePath));
            Console.WriteLine("Source full path: " + srcFullPath);
            //Console.WriteLine("Source dir name: " + dirName);
            //Console.WriteLine("Source name: " + e.Name);
        }

        private bool CreateFolderNoExist(string srcDirName)
        {
            Console.WriteLine("***CreateFolderNoExist:");

             
            var pos = 0;
            //finance sb configurable
            var fldrAfterFinance = "";
            if (srcDirName != null)
            {
                //if the file is in destination base folder then use dest base path 
                if (srcDirName.EndsWith(_destinationBaseFolder))
                    return true;

                pos = srcDirName.IndexOf("Finance", StringComparison.CurrentCulture);
                fldrAfterFinance = srcDirName.Substring(pos + 8);
            }


            Console.WriteLine("pos: " + pos);
            Console.WriteLine("folder after finance: " + fldrAfterFinance);

            return true;
        }
    }
}