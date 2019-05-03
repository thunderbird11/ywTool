using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using System.Collections.Concurrent;
using ywTool.Objects;
using ywTool.BaseObjects;
using System.Drawing;
using System.Net.Mail;

namespace ywTool
{
    public class GlobalSettings
    {
        public static AppState AppState = new AppState();
        public static string EncryptionSalt = "WeAreTheBest!";
        public static string ToolResourceDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resource");
        public static string ModulesDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Modules");
        public static string NodeEngineDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "NodeEngine");
        public static string MessageTemplate = File.ReadAllText(Path.Combine(ToolResourceDir, "MessageTemplate.html"));
        public static string InstructionTemplate = File.ReadAllText(Path.Combine(ToolResourceDir, "InstructionTemplate.html"));
        public static string MouldeNameSpace = "ywTool.Modules";
        public static Thread DispatchThread = null;
        public static string OutputFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "OutputFile");
        public static string OutputData = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "OutputData");
        public static string OutputTmp = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "OutputTmp");
        public static Image LogoImage = null;

        public static List<CmdStructure> AvailableCmd = new List<CmdStructure>();
        public static Queue<string> CmdQueue = new Queue<string>();

        public static ConcurrentDictionary<string, string> UserVariables = new ConcurrentDictionary<string, string>();
        public static List<DisplayedMessage> Message = new List<DisplayedMessage>();
        public static List<string> CmdHistory = new List<string>();
        public static int CmdCursor = -1;

        public static bool Init()
        {
            AvailableCmd = CmdRouter.GetAllModuleCmd();
            if (!Directory.Exists(OutputFolder))
                Directory.CreateDirectory(OutputFolder);
            if (!Directory.Exists(OutputData))
                Directory.CreateDirectory(OutputData);
            if (!Directory.Exists(OutputTmp))
                Directory.CreateDirectory(OutputTmp);
            if (!Directory.Exists(ModulesDir))
                Directory.CreateDirectory(ModulesDir);

            DispatchThread = new Thread(new ThreadStart(CmdRouter.DispatchCmd));
            DispatchThread.Start();
            UserVariables.AddOrUpdate(InternalVariable.TmpFolder, OutputTmp, (k, o) => o);
            UserVariables.AddOrUpdate(InternalVariable.ResourceFolder, ToolResourceDir, (k, o) => o);
            AppState.ToolInitialized = true;
            return true;
        }

        public static bool Destory()
        {
            CmdHistory.Clear();
            if (DispatchThread != null && DispatchThread.IsAlive)
                DispatchThread.Abort();
            return true;
        }

        public static string GetMessage()
        {
            return DisplayedMessage.GetMessage(new DisplayedMessageOption());
        }

        

    }
}
