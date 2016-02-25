using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Topshelf;

namespace Service1
{
    class Program
    {
        static void Main(string[] args)
        {
            HostFactory.Run(cfg =>
            {
                cfg.UseNLog();
                cfg.Service<FinanceFileSyncService>(svcIns =>
                {
                    svcIns.ConstructUsing(
                        () => new FinanceFileSyncService());
                    svcIns.WhenStarted(ffss => ffss.Start());
                    svcIns.WhenStopped(ffss => ffss.Stop());
                    svcIns.WhenPaused(ffss => ffss.Pause());
                    svcIns.WhenContinued(ffss => ffss.Continue());
                    svcIns.WhenCustomCommandReceived((ffss, hostControl, commandNumber) =>
                    {
                        ffss.CustomCommand(commandNumber);
                    });
                });
                //cfg.RunAs("username", "password");
                //otherwise run as local system (this has high prevliges).
                cfg.EnableServiceRecovery(recoveryOption =>
                {
                    recoveryOption.RestartService(1);
                });
                cfg.EnablePauseAndContinue();
                cfg.SetServiceName("FinanceDocSync");
                cfg.SetDisplayName("Finance Files Backup Sync");
                cfg.SetDescription("Syncronizes the finance folder on local to backup");
                cfg.StartAutomatically();
            });

            //var svc = new FinanceFileSyncService();
            //svc.OnNewFile(new object(), new FileSystemEventArgs(WatcherChangeTypes.Created,  "C:\0", "name"));

            Console.Read();
        }
    }
}
