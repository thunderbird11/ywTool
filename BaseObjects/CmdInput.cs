using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ywTool.BaseObjects
{
    public class CmdInput
    {
        public List<string> Args;
        public object options;
        public string MessageId;
        public string ResourceFolder;
        private Action<CmdOutput> MessageProcess;
        private Action<string, string> AddOrUpdateVariable;
        private Action<string, LogLevel> LogWritter;
        private Func<byte[], string, bool, string> DataWriter;
        public CmdInput(Action<CmdOutput> funMessageProcess=null, Action<string, string> funVarProcess=null, Action<string, LogLevel> funLogProcess=null, Func<byte[], string, bool, string> funDataProcess=null)
        {
            MessageProcess = funMessageProcess;
            AddOrUpdateVariable = funVarProcess;
            LogWritter = funLogProcess;
            DataWriter = funDataProcess;
        }

        public CmdOutput CreateAnOutput(bool result, CmdRunState cs, string message, Dictionary<string, object> ei = null)
        {
            return new CmdOutput(result, cs, MessageId, message, ei);
        }
        public void ProcessAnOutput(bool result, CmdRunState cs, string message, Dictionary<string, object> ei = null)
        {
            if(MessageProcess!=null)
                MessageProcess(CreateAnOutput(result, cs, message, ei));
        }

        public void ProcessVariable(string key,string value)
        {
            if (AddOrUpdateVariable != null)
                AddOrUpdateVariable(key, value);
        }

        public void ProcessLog(string message,LogLevel lg=LogLevel.INFO)
        {
            if (LogWritter != null)
                ProcessLog(message, lg);
        }

        public string SaveData(byte[] data,string filename, bool writeToTmpFolder=false)
        {
            if (DataWriter != null)
                return DataWriter(data, filename, writeToTmpFolder);
            return null;
        }
    }
}
