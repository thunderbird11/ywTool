using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ywTool.BaseObjects
{
    public enum MessageType
    {
        JOBNAME=0,

        NONE =1,
        HEADER=2,
        TITLE=4,

        INFO=1024,
        SUCCESS=2048,
        WARNING = 4096,
        ERROR =8192
    }

    public enum MessageFormat
    {
        STRING=0,
        HTML=1,
        IMAGE=2,
        FILE=4,

    }
}
