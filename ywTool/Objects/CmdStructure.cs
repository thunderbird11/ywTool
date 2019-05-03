using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ywTool.BaseObjects;

namespace ywTool.Objects
{
    public class CmdStructure
    {
        private string _originalCmd { get; }
        private bool _moduleisvalid { get; set; }
        private bool _commandisvalid { get; set; }

        public string OriginalCmd { get { return _originalCmd; } }
        public string ModuleName { get; set; }
        public string CommandName { get; set; }
        public string[] Args { get; set; }
        public Assembly Assembly { get; set; }
        public  object Paras { get; set; }
        public CommandOptionAttribute Attr { get; set; }
        public bool ModuleNameIsValid { get { return _moduleisvalid; } }
        public bool CommandNameIsValid { get { return _commandisvalid; } }

        public bool ValidCommand()
        {
            var tmp = GlobalSettings.AvailableCmd.Where(a => a.ModuleName.ToLower() == ModuleName?.ToLower()).Select(a => a.ModuleName).ToList();
            if (tmp.Count > 0)
            {
                ModuleName = tmp[0];
                _moduleisvalid = true;
                var tmp2 = GlobalSettings.AvailableCmd.Where(a => a.ModuleName == ModuleName && a.CommandName.ToLower() == CommandName?.ToLower()).Select(a => a).ToList();
                if (tmp2.Count > 0)
                {
                    CommandName = tmp2[0].CommandName;
                    _commandisvalid = true;
                    Assembly = tmp2[0].Assembly;
                    Attr = tmp2[0].Attr;
                }
            }
            return _moduleisvalid && _commandisvalid;
        }
        public override string ToString()
        {
            string arg = "";
            if (Args != null && Args.Length > 0)
                arg = string.Join("|", Args);
            return $"Module Name:[{ModuleName}],Command Name:[{CommandName}],Args:[{arg}]";
        }
        public bool CanRun()
        {
            var attr = GlobalSettings.AvailableCmd.Where(a => a.ModuleName == ModuleName && a.CommandName == CommandName).Select(a => a.Attr).FirstOrDefault();
            if (attr == null) return true;
            if (attr.LogonRequired && !GlobalSettings.AppState.isLogon) return false;
            return true;
        }
        public bool ProcessArgs()
        {
            object o = null;
            if (Attr != null && !string.IsNullOrWhiteSpace(Attr.ParameterOptionClass))
            {
                o = Assembly.CreateInstance(GlobalSettings.MouldeNameSpace + "." + Attr.ParameterOptionClass);
                if (o != null)
                {                  
                    //if (!Parser.Default.ParseArguments((Args?.ToArray()) ?? (new List<string>()).ToArray(), o))
                    if (!CmdOptionsBind.Bind(Args,o)) 
                    {
                        return false;
                    }
                }
            }
            Paras = o;
            return true;
        }
        public bool RequiredHelp()
        {
            if (Args != null && Args.Length > 0)
            {
                List<string> helpflg = new List<string> { "-?", "?", "-h", "--help" };
                if (Args.ToList().Select(a => a.ToLower()).ToList().Intersect(helpflg).Count() > 0)
                    return true;
            }
            return false;
        }
        public List<MessageStructure> GetHelp(bool highlight = false )
        {
            var retVal = new List<MessageStructure>();
            var attr = GlobalSettings.AvailableCmd.Where(a => a.ModuleName == ModuleName && a.CommandName == CommandName).Select(a => a.Attr).FirstOrDefault();
            if (attr == null || string.IsNullOrWhiteSpace(attr.HelpText))
                retVal.Add(new MessageStructure("No help information provided.", highlight ? MessageType.ERROR : MessageType.INFO));
            else
                retVal.Add(new MessageStructure(attr.HelpText, highlight ? MessageType.ERROR : MessageType.INFO));
            if (attr != null && !string.IsNullOrWhiteSpace(attr.ParameterOptionClass))
            {
                var module = Assembly.GetTypes().Where(t => string.Equals(t.Namespace, GlobalSettings.MouldeNameSpace, StringComparison.Ordinal) && t.Name == attr.ParameterOptionClass).FirstOrDefault();
                if (module != null)
                {
                    CmdOption instance = (CmdOption)Activator.CreateInstance(module);
                    retVal.Add( instance.GetUsage() );
                }
            }
            return retVal;
        }
        public MessageStructure ReplaceVariable()
        {
            if (Args != null)
            {
                List<string> unreplaced = new List<string>();
                for (int i = 0; i < Args.Length; i++)
                {
                    foreach (var v in GlobalSettings.UserVariables)
                    {
                        Args[i] = Args[i].Replace("{{" + v.Key + "}}", v.Value);
                    }
                    Args[i] = Common.CommFuns.TranslateTimeVariable(Args[i]);
                    System.Text.RegularExpressions.Regex rgx = new System.Text.RegularExpressions.Regex("{{.+}}");
                    var tmp = rgx.Matches(Args[i]);
                    foreach (System.Text.RegularExpressions.Match v in tmp)
                        unreplaced.Add(v.Value.Replace("{{", "").Replace("}}", ""));
                }
                unreplaced = unreplaced.Distinct().ToList();
                if (unreplaced.Count > 0)
                    return new MessageStructure("Cannot find the following variable value:" +string.Join(",", unreplaced) , MessageType.ERROR) ;
                else
                    return null;
            }
            return null;
        }


        public CmdStructure()
        {

        }

        public CmdStructure(string cmd)
        {
            if (string.IsNullOrWhiteSpace(cmd))
                return;
            _originalCmd = cmd.Trim();
            var list = Common.CommFuns.CommandLineToArgs(_originalCmd);
            if (list.Length > 0) ModuleName = list[0];
            if (list.Length > 1) CommandName = list[1];
            if (list.Length > 2) Args = list.Skip(2).ToArray();
        }
    }
}
