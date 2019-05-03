using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ywTool.BaseObjects;
using ywTool.Objects;

namespace ywTool.Modules
{
    public class Command_Save_Options : CmdOption
    {
        [Option('o', "output", Required = false, HelpText = "Include Output", DefaultValue = false)]
        public bool IncludeOutput { get; set; }

        [Option('v', "view", Required = false, HelpText = "view Output immeditely", DefaultValue = false)]
        public bool ViewOutput { get; set; }
    }

    public class Command_Echo_Options : CmdOption
    {
        [Option('s', "echostring", Required = false, HelpText = "Echo String", DefaultValue = "Done")]
        public string EchoString { get; set; }

    }

    public class Command_RunBatch_Options : CmdOption
    {
        [Option('i', "input", Required = true, HelpText = "batch file, include full path")]
        public string BatchFile { get; set; }
    }

    public class Command
    {
        [CommandOption("List all available modules or commands in the tool", false)]
        public CmdOutput List(CmdInput ci)
        {
            string cmd = "";
            if (ci.Args != null && ci.Args.Count == 1)
                cmd = ci.Args[0];
            cmd = GlobalSettings.AvailableCmd.Where(a => a.ModuleName.ToLower() == cmd.ToLower()).Select(a => a.ModuleName).FirstOrDefault();
            List<string> tmp;
            if (cmd == null)
                tmp = GlobalSettings.AvailableCmd.Where(a => a.Attr != null).Select(a => a.ModuleName).Distinct().ToList();
            else
                tmp = GlobalSettings.AvailableCmd.Where(a => a.Attr != null && a.ModuleName == cmd).Select(a => a.CommandName).Distinct().ToList();
            var msg = "";
            if (cmd == null)
            {
                msg += "Availble module name:<li>";
                msg += string.Join("</li><li>", tmp) + "</li>";
            }
            else
            {
                msg += $"Availble command for module {cmd}:<li>";
                msg += string.Join($"</li><li>", tmp) + "</li>";
            }
            return new CmdOutput(true, CmdRunState.FINISHED, ci.MessageId, msg);
        }
        [CommandOption("Save command histroy to file w/o command output", false, "Command_Save_Options")]
        public CmdOutput Save(CmdInput ci)
        {
            var options = ci.options as Command_Save_Options;
            string prefix = "cmdhistory";
            string suffix = "txt";
            if (options.IncludeOutput)
            {
                prefix = "cmdwithoutput";
                suffix = "html";
            }
            StringBuilder sb = new StringBuilder();
            if (options.IncludeOutput)
            {
                sb.Append(DisplayedMessage.GetMessage(new DisplayedMessageOption() { RemoveTail = 1}));
            }
            else
            {
                foreach (var c in GlobalSettings.CmdHistory)
                    sb.AppendLine(c);
            }

            var f = Common.CommFuns.SaveStringToFile(sb.ToString(), prefix, suffix);
            if (options.ViewOutput)
                System.Diagnostics.Process.Start(f);
            return new CmdOutput(true, CmdRunState.FINISHED, ci.MessageId, $"Saved as {f}");
        }
        [CommandOption("clear screen", false)]
        public CmdOutput Clear(CmdInput ci)
        {
            GlobalSettings.CmdHistory.Clear();
            GlobalSettings.Message = GlobalSettings.Message.Where(a => a.MessageId == ci.MessageId).Select(a => a).ToList();
            return new CmdOutput(true, CmdRunState.FINISHED, ci.MessageId, $"Cleared");

        }
        [CommandOption("Run the command one by one in the batch file", false, "Command_RunBatch_Options")]
        public CmdOutput RunBatch(CmdInput ci)
        {
            var options = ci.options as Command_RunBatch_Options;
            var cmds = Common.CommFuns.ReadAllLinesFromBatchFile(options.BatchFile);
            if (cmds == null || cmds.Length == 0)
                return new CmdOutput(false, CmdRunState.INVALID, ci.MessageId, "Invalid batch file");
            foreach (var c in cmds)
                GlobalSettings.CmdQueue.Enqueue(c);
            return new CmdOutput(true, CmdRunState.FINISHED, ci.MessageId, $"Loaded batch successfully");
        }

        [CommandOption("Echo a string", false, "Command_Echo_Options")]
        public CmdOutput Echo(CmdInput ci)
        {
            var options = ci.options as Command_Echo_Options;
            return new CmdOutput(true, CmdRunState.FINISHED, ci.MessageId, options.EchoString);
        }

        [CommandOption("Delete Temp files", false)]
        public CmdOutput ClearTmpFiles(CmdInput ci)
        {
            Common.CommFuns.ClearTmpFile();
            return new CmdOutput(true, CmdRunState.FINISHED, ci.MessageId, "Cleared temp files");
        }
    }

}
