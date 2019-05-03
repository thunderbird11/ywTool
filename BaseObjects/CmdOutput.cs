using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ywTool.BaseObjects
{
    public class CmdOutput
    {
        public bool Result;
        public CmdRunState State;
        public string MessageId;
        public List<MessageStructure> Message;
        public Dictionary<string, object> ExtraInfo;
        public CmdOutput(bool result, CmdRunState cs, string msgid, List<MessageStructure> msg, Dictionary<string, object> ei = null)
        {
            Result = result;
            State = cs;
            MessageId = msgid;
            Message = msg;
            ExtraInfo = ei;
        }
        public CmdOutput(bool result, CmdRunState cs, string msgid, string msg, Dictionary<string, object> ei = null)
        {
            Result = result;
            State = cs;
            MessageId = msgid;
            Message = new List<MessageStructure>() { new MessageStructure(msg) };
            ExtraInfo = ei;
        }
        public CmdOutput()
        { }
    }
}
