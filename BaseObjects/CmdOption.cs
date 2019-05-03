using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ywTool.BaseObjects
{
    public class CmdOption
    {
        public MessageStructure GetUsage()
        {
            var ret = "<table border=1><tr><th>Parameter</th><th>Long Parameter</th><th>Required</th><th>Description</th>";
            PropertyInfo[] props = this.GetType().GetProperties();
            foreach (PropertyInfo prop in props)
            {
                object[] attrs = prop.GetCustomAttributes(true);
                foreach (object attr in attrs)
                {
                    OptionAttribute oAttr = attr as OptionAttribute;
                    if (oAttr != null)
                    {
                        var required = oAttr.Required ? "YES" : "NO";
                        ret += $"<tr><td>-{oAttr.ShortName}</td><td>--{oAttr.LongName}</td><td>{required}</td><td>{oAttr.HelpText}</td></tr>";
                    }
                }
            }
            ret += "</table>";
            return new MessageStructure(ret, MessageType.INFO);
        }

        [Option('n', "name", Required = false, HelpText = "Friendly name")]
        public string Name { get; set; }

        public MessageStructure HtmlName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Name)) return null;
                return new MessageStructure(Name, MessageType.JOBNAME);
            }
        }

        public CmdOption()
        { }
    }
}
