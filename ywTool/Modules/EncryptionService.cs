using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ywTool.BaseObjects;
using ywTool.BaseObjects.Common;

namespace ywTool.Modules
{
    public class EncryptionService
    {
        public CmdOutput PasswordEncrypt(CmdInput ci)
        {
            if (ci == null || ci.Args == null || ci.Args.Count != 1)
                return new CmdOutput(false, CmdRunState.EXITWITHERROR, ci.MessageId, "Invalid command" ) ;
            return new CmdOutput(true,CmdRunState.FINISHED,ci.MessageId,"Encrypted Password: " + Encryption.EncryptPassword(ci.Args[0]));
        }

        public CmdOutput Decrypt(CmdInput ci)
        {
            if (ci == null || ci.Args == null || ci.Args.Count != 1)
                return new CmdOutput(false, CmdRunState.EXITWITHERROR, ci.MessageId, "Invalid command");
            try
            {
                var d = Encryption.Decrypt(ci.Args[0], "jweoir2389kjhlksd920394hf");
                return new CmdOutput( true,CmdRunState.FINISHED,ci.MessageId,"Decrypted string: " + d);
            }
            catch
            {
                return new CmdOutput(false, CmdRunState.EXITWITHERROR, ci.MessageId, "Invalid string");
            }
        }

        public CmdOutput Encrypt(CmdInput ci)
        {
            if (ci == null || ci.Args == null || ci.Args.Count != 1)
                return new CmdOutput(false, CmdRunState.EXITWITHERROR, ci.MessageId, "Invalid command");
            return new CmdOutput(true, CmdRunState.FINISHED, ci.MessageId, "Encrypted string: " + Encryption.Encrypt(ci.Args[0], "jweoir2389kjhlksd920394hf"));
        }
    }
}
