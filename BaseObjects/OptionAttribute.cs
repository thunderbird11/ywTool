using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ywTool.BaseObjects
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false)]
    public class OptionAttribute : Attribute
    {
        public string HelpText { get; set; }
        public bool Required { get; set; }
        public object DefaultValue { get; set; }
        public char? ShortName { get; set; }
        public string LongName { get; set; }


        public OptionAttribute(char sn,string ln)
        {
            ShortName = sn;
            LongName = ln;
        }
        public OptionAttribute(string ln)
        {
            ShortName = null;
            LongName = ln;
        }
        public OptionAttribute(char sn)
        {
            ShortName = sn;
        }
    }
}
