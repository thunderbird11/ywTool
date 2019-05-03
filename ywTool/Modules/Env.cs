using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ywTool.BaseObjects;
using ywTool.BaseObjects.Common;

namespace ywTool.Modules
{

    public class Env_AddVar_Options : CmdOption
    {
        [Option('k', "key", Required = true, HelpText = "variable name")]
        public string VarName { get; set; }

        [Option('v', "value", Required = true, HelpText = "variable value")]
        public string VarValue { get; set; }

        [Option('e', "encrypted", Required = false, HelpText = "value is encrypted", DefaultValue = false)]
        public bool Encrypted { get; set; }
    }

    public class Env
    {
        [CommandOption("Add variable in the current context", false, "Env_AddVar_Options")]
        public CmdOutput AddVar(CmdInput ci)
        {
            Env_AddVar_Options options = ci.options as Env_AddVar_Options;
            var v = options.VarValue;
            if (options.Encrypted)
            {
                try
                {
                    v = Encryption.Decrypt(v, "jweoir2389kjhlksd920394hf");
                }
                catch
                {
                    return new CmdOutput(false, CmdRunState.EXITWITHERROR, ci.MessageId, "Invalid encrypted value");
                }
            }
            GlobalSettings.UserVariables.AddOrUpdate(options.VarName?.ToLower(), v, (k, o) => v);
            return new CmdOutput(true, CmdRunState.FINISHED, ci.MessageId, $"Add variable {options.VarName} successfully");
        }

        [CommandOption("List all variables in the current context", false)]
        public CmdOutput ListVar(CmdInput ci)
        {
            return new CmdOutput(true, CmdRunState.FINISHED, ci.MessageId, Common.CommFuns.Dictionary2HTML(GlobalSettings.UserVariables, "Variable Name", "Variable Value"));
        }
    }
}
