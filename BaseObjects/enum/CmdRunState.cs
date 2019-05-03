using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ywTool.BaseObjects
{
    public enum CmdRunState
    {
        INVALID,
        STARTING,
        RUNNING,
        KILLING,
        FINISHED,
        EXITWITHERROR,
        FINISHEDWITHWARNING
    }
}
