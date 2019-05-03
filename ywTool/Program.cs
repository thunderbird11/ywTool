using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ywTool
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {           
            GlobalSettings.Init();

            if (args.Length == 1)
            {
                Common.CommFuns.WriteLog("Starting...");
                GlobalSettings.CmdQueue.Enqueue("command runbatch -i " + args[0]);
                while (GlobalSettings.CmdQueue.Count > 0)
                {
                    Common.CommFuns.WriteLog("Waiting for ----" + GlobalSettings.CmdQueue.Peek());
                    System.Threading.Thread.Sleep(1000);
                }
                GlobalSettings.Destory();
                Common.CommFuns.WriteLog("Ending...");
            }
            else {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new ywTool());
                GlobalSettings.Destory();
            }
        }
    }
}
