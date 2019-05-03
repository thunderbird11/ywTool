using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ywTool.BaseObjects;

namespace ywTool.Objects
{
    public class CmdRouter
    {
        public static void DispatchCmd()
        {
            try
            {
                while (true)
                {
                    if (GlobalSettings.CmdQueue.Count == 0)
                    {
                        Thread.Sleep(500);
                        continue;
                    }
                    var cmd = GlobalSettings.CmdQueue.Dequeue();
                    GlobalSettings.CmdHistory.Add(cmd);
                    var cm = new CmdStructure(cmd);
                    MessageStructure ms = null;
                    bool isContinue = true;
                    if (!GlobalSettings.AppState.ToolInitialized)
                    {
                        GlobalSettings.Message.Add(new DisplayedMessage()
                        {
                            Start = DateTime.Now,
                            Cmd = cmd,
                            MessageId = Guid.NewGuid().ToString(),
                            State = CmdRunState.EXITWITHERROR,
                            MessageList = new List<MessageStructure>() { new MessageStructure( "Fail to initialize, please contact administrator", MessageType.ERROR) }
                        });
                        isContinue = false;
                    }
                    else if ( (ms = cm.ReplaceVariable())!=null)
                    {
                        GlobalSettings.Message.Add(new DisplayedMessage()
                        {
                            Start = DateTime.Now,
                            Cmd = cmd,
                            MessageId = Guid.NewGuid().ToString(),
                            State = CmdRunState.INVALID,
                            MessageList = new List<MessageStructure>() { ms }
                        });
                        isContinue = false;
                    }
                    else if (!cm.ValidCommand())
                    {
                        var message = "";
                        if (!cm.ModuleNameIsValid)
                            message = "Invalid Command,use <font color=green><b>Command List</b></font> to get help";
                        else
                            message = $"Invalid Command,use <font color=green><b>Command List {cm.ModuleName}</b></font> to get help";
                        GlobalSettings.Message.Add(new DisplayedMessage()
                        {
                            Start = DateTime.Now,
                            Cmd = cmd,
                            MessageId = Guid.NewGuid().ToString(),
                            State = CmdRunState.INVALID,
                            MessageList = new List<MessageStructure>() { new MessageStructure(message,MessageType.ERROR) }
                        });
                        isContinue = false;
                    }
                    else if (!cm.CanRun())
                    {
                        var message = "Cannot run this command. Make sure already logon";
                        GlobalSettings.Message.Add(new DisplayedMessage()
                        {
                            Start = DateTime.Now,
                            Cmd = cmd,
                            MessageId = Guid.NewGuid().ToString(),
                            State = CmdRunState.INVALID,
                            MessageList = new List<MessageStructure>() { new MessageStructure(message, MessageType.ERROR) }
                        });
                        isContinue = false;
                    }
                    else if (cm.RequiredHelp())
                    {
                        GlobalSettings.Message.Add(new DisplayedMessage()
                        {
                            Start = DateTime.Now,
                            Cmd = cmd,
                            MessageId = Guid.NewGuid().ToString(),
                            State = CmdRunState.FINISHED,
                            MessageList = cm.GetHelp()
                        });
                        isContinue = false;
                    }
                    else if (!cm.ProcessArgs())
                    {
                        GlobalSettings.Message.Add(new DisplayedMessage()
                        {
                            Start = DateTime.Now,
                            Cmd = cmd,
                            MessageId = Guid.NewGuid().ToString(),
                            State = CmdRunState.EXITWITHERROR,
                            MessageList = cm.GetHelp(true)
                        });
                        isContinue = false;
                    }
                    if ( !isContinue)
                    {
                        GlobalSettings.AppState.isProcessingCommand = false;
                        GlobalSettings.AppState.NotifyObserver();
                        GlobalSettings.CmdQueue.Clear();
                    }
                    else
                    {
                        GlobalSettings.AppState.isProcessingCommand = true;
                        var msgid = Guid.NewGuid().ToString();
                        MessageStructure tmpms = null;
                        if (cm.Paras!=null)
                            tmpms= (cm.Paras as CmdOption).HtmlName;
                        var mlist = new List<MessageStructure>();
                        if (tmpms != null)
                        {
                            tmpms.Message = "Working on " + tmpms.Message + "..........";
                            mlist.Add(tmpms);
                        }
                        GlobalSettings.Message.Add(new DisplayedMessage()
                        {
                            Start = DateTime.Now,
                            Cmd = cmd,
                            MessageId = msgid,
                            State = CmdRunState.STARTING,
                            MessageList = mlist
                        });
                        GlobalSettings.AppState.NotifyObserver();
                  
                        var m = cm.Assembly.CreateInstance(GlobalSettings.MouldeNameSpace + "." + cm.ModuleName);
                        Type thisType = m.GetType();
                        MethodInfo theMethod = thisType.GetMethod(cm.CommandName);
                        CmdInput ci = new CmdInput(MessageHandler, AddorUpdateVariable,null,Common.CommFuns.SaveDataToFile)
                        {
                            Args = cm.Args?.ToList(),
                            options = cm.Paras,                           
                            MessageId = msgid,
                            ResourceFolder = GlobalSettings.ToolResourceDir,
                        };
                        var co = theMethod.Invoke(m, new object[] { ci }) as CmdOutput;

                        if (!co.Result)
                        {
                            //GlobalSettings.CmdQueue.Clear();
                        }

                        MessageHandler(co);
                    }
                }
            }
            catch (ThreadAbortException abortException)
            {
                Console.WriteLine((string)abortException.ExceptionState);
            }
        }
        public static void AddorUpdateVariable(string key,string value)
        {
            GlobalSettings.UserVariables.AddOrUpdate(key?.ToLower(), value, (k, o) => value);
        }
        public static void MessageHandler(CmdOutput co)
        {
            if (co == null) return;
            var dm = GlobalSettings.Message.Where(a => a.MessageId == co.MessageId).Select(b => b).FirstOrDefault();
            if (dm == null) return;
            if (co.Message != null)
            {
                dm.MessageList.AddRange(co.Message);
                dm.MessageList = (from m in dm.MessageList group m by m.MessageId into gm select gm.OrderByDescending(a => a.TimeStamp).FirstOrDefault()).ToList();
            }
            dm.State = co.State;
            if (dm.State == CmdRunState.EXITWITHERROR || dm.State == CmdRunState.FINISHED || dm.State == CmdRunState.FINISHEDWITHWARNING || dm.State == CmdRunState.INVALID)
            {
                var tmpms = dm.MessageList.Where(a => a.MessageType == (int)MessageType.JOBNAME).FirstOrDefault();
                if (tmpms != null)
                {
                    tmpms.Message = tmpms.Message.Substring("Working on ".Length);
                    tmpms.Message = tmpms.Message.Substring(0, tmpms.Message.Length - 10);
                }
                GlobalSettings.AppState.isProcessingCommand = false;
                dm.End = DateTime.Now;
            }
            GlobalSettings.AppState.NotifyObserver();
        }

        public static List<CmdStructure> GetAllModuleCmd()
        {
            var ret = new List<CmdStructure>();
            var assemblylist = new List<Assembly>();
            assemblylist.Add(Assembly.GetExecutingAssembly());
            DirectoryInfo di = new DirectoryInfo(GlobalSettings.ModulesDir);
            var dlls = di.GetFiles("*.dll");
            foreach (var fi in dlls)
            {
                assemblylist.Add(Assembly.LoadFrom(fi.FullName));
            }
            foreach(var a in assemblylist)
            {
                var modules = a.GetTypes().Where(t => string.Equals(t.Namespace, GlobalSettings.MouldeNameSpace, StringComparison.Ordinal)).ToArray();
                foreach (var m in modules)
                {
                    if(m.Name=="LogoImage")
                    {
                        try
                        {
                            var instance = a.CreateInstance(GlobalSettings.MouldeNameSpace + "." + m.Name);
                            Type thisType = instance.GetType();
                            MethodInfo theMethod = thisType.GetMethod("Get");
                            GlobalSettings.LogoImage = theMethod.Invoke(instance, null) as System.Drawing.Image;
                        }
                        catch
                        {
                            GlobalSettings.LogoImage = null;
                        }
                    }

                    if (m.Name.EndsWith("_Options") || m.Name.StartsWith("<>") || typeof(CmdOption).IsAssignableFrom(m))
                        continue;
                    foreach (var method in m.GetMethods())
                    {
                        var tmp = new CmdStructure() { ModuleName = m.Name };
                        var parameters = method.GetParameters();
                        if (parameters.Length == 1 && parameters[0].ParameterType == typeof(CmdInput) && method.ReturnType == typeof(CmdOutput))
                            tmp.CommandName = method.Name;
                        else
                            continue;
                        tmp.Attr = method.GetCustomAttribute(typeof(CommandOptionAttribute)) as CommandOptionAttribute;
                        tmp.Assembly = a;
                        ret.Add(tmp);
                    }
                }
            }
            return ret;

        }

        public static string GetNextCmdFromHistory()
        {
            if (GlobalSettings.CmdHistory.Count == 0) return null;
            if (GlobalSettings.CmdCursor > GlobalSettings.CmdHistory.Count - 1)
                GlobalSettings.CmdCursor = GlobalSettings.CmdHistory.Count - 1;
            if (GlobalSettings.CmdCursor < 0)
                GlobalSettings.CmdCursor = 0;
            return GlobalSettings.CmdHistory[GlobalSettings.CmdCursor++];
        }
        public static string GetPreviousCmdFromHistory()
        {
            if (GlobalSettings.CmdHistory.Count == 0) return null;
            if (GlobalSettings.CmdCursor > GlobalSettings.CmdHistory.Count - 1)
                GlobalSettings.CmdCursor = GlobalSettings.CmdHistory.Count - 1;
            if (GlobalSettings.CmdCursor < 0)
                GlobalSettings.CmdCursor = 0;
            return GlobalSettings.CmdHistory[GlobalSettings.CmdCursor--];
        }
        public static void SetCmdCursor()
        {
            GlobalSettings.CmdCursor = GlobalSettings.CmdHistory.Count;
        }
    }
}
