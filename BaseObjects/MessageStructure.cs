using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ywTool.BaseObjects
{
    public class MessageStructure
    {
        public string MessageId { get; }
        public string Message { get; set; }
        public int MessageType { get; set; }
        public DateTime TimeStamp { get; set; }
        public MessageFormat MessageFormat { get; set; }

        public MessageStructure(string message, MessageType mt,MessageFormat mf=MessageFormat.STRING)
        {
            MessageId = Guid.NewGuid().ToString();
            Message = message;
            MessageType =(int)mt;
            TimeStamp = DateTime.Now;
            MessageFormat = mf;
        }
        public MessageStructure(string message, int type=1, MessageFormat mf = MessageFormat.STRING)
        {
            MessageId = Guid.NewGuid().ToString();
            Message = message;
            MessageType = type;
            TimeStamp = DateTime.Now;
            MessageFormat = mf;
        }
        public MessageStructure(string message, DateTime dt, int type = 1, MessageFormat mf = MessageFormat.STRING)
        {
            MessageId = Guid.NewGuid().ToString();
            MessageType = type;
            Message = message;
            TimeStamp = dt;
            MessageFormat = mf;
        }

        public override string ToString()
        {
            return Message;
        }
    }
}
