using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using ywTool.BaseObjects;
using ywTool.BaseObjects.Common;
using ywTool.Objects;

namespace ywTool.Modules
{
    public class Email_Report_Options : CmdOption
    {
        [Option('s', "stmp", Required = true, HelpText = "STMP server")]
        public string STMP { get; set; }

        [Option('p', "port", Required = true, HelpText = "Port")]
        public int Port { get; set; }

        [Option('u', "user", Required = true, HelpText = "User Name")]
        public string User { get; set; }

        [Option('w', "password", Required = true, HelpText = "Password")]
        public string Password { get; set; }

        [Option('t', "to", Required = true, HelpText = "To list" )]
        public string To { get; set; }

        [Option('b', "subject", Required = true, HelpText = "Subject")]
        public string Subject { get; set; }

        [Option('f', "from", Required = true, HelpText = "From")]
        public string From { get; set; }

        [Option('c', "includecmd", Required = false, HelpText = "Include Cmd in report", DefaultValue = false )]
        public bool IncludeCmd { get; set; }

        [Option('d', "includetimestamp", Required = false, HelpText = "Include TimeStamp in report", DefaultValue = false)]
        public bool IncludeTimeStamp { get; set; }

        [Option('i', "includetaskname", Required = false, HelpText = "Include Task name in report", DefaultValue = false)]
        public bool IncludeTaskName { get; set; }
    }
    public class Email
    {
        [CommandOption("Send Batch job report", false,  "Email_Report_Options")]
        public CmdOutput Report(CmdInput ci)
        {
            var options = ci.options as Email_Report_Options;
            try
            {
                List<Attachment> attachments = new List<Attachment>();
                string prefix = Guid.NewGuid().ToString().Substring(0, 8);
                string body = DisplayedMessage.GetMessage(new DisplayedMessageOption() {
                    SkipHead =1,
                    RemoveTail =1,
                    IncludeStatusImage =false,
                    IncludeTaskName = options.IncludeTaskName,
                    IncludeTimeStamp = options.IncludeTimeStamp ,
                    IncludeCmd = options.IncludeCmd },true, prefix, attachments);
                List<string> to = options.To.Split(';').ToList();
                EmailUtil.SendEmail(to, options.Subject, body, options.STMP, options.Port, options.User, options.Password,options.From,null,"HTML",null,attachments);
            }
            catch (Exception ex)
            {
                return new CmdOutput(false, CmdRunState.EXITWITHERROR, ci.MessageId, ex.Message);
            }
            return new CmdOutput(true, CmdRunState.FINISHED, ci.MessageId, "Sent report successfully");
        }

    }
}
