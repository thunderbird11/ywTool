using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ywTool.BaseObjects;

namespace ywTool.Objects
{
    class CmdBindClass
    {
        public PropertyInfo PI { get; set; }
        public OptionAttribute OA { get; set; }
        public bool IsBinded { get; set; }
        public string value { get; set; }
    }

    public class CmdOptionsBind
    {
        public static bool Bind(IEnumerable<string> args, object options)
        {
            if (args == null) args = new List<string>();
            var pros = options.GetType().GetProperties();
            List<CmdBindClass> bind = new List<CmdBindClass>();
            foreach(var p in pros)
            {
                var attr=p.GetCustomAttribute(typeof(OptionAttribute)) as OptionAttribute;
                if (attr != null)
                    bind.Add(new CmdBindClass() { PI = p, OA = attr, IsBinded = false });
            }
            int i = 0;
            var arglist = args.ToList();
            for(;i<arglist.Count;i++)
            {
                CmdBindClass b = null;
                if(arglist[i].StartsWith("-") && arglist[i].Length==2 && arglist[i]!="--")
                    b = bind.Where(a => a.OA.ShortName.HasValue && Char.ToLower(a.OA.ShortName.Value) == (arglist[i].ToLower())[1]).FirstOrDefault();
                if (arglist[i].StartsWith("--") && arglist[i].Length > 3)
                    b = bind.Where(a => a.OA.LongName?.ToString() == arglist[i].ToLower().Substring(2)).FirstOrDefault();
                if(b!=null)
                {
                    if (b.IsBinded) return false; //already binded
                    b.IsBinded = true;
                    if (b.PI.PropertyType == typeof(bool))
                        b.value = "true";
                    else
                    {
                        if (i == arglist.Count - 1) return false;
                        i++;
                        b.value = arglist[i];
                    } 
                }
            }
            if (bind.Where(a => !a.IsBinded && a.OA.Required).Count() > 0) return false;
            foreach(var b in bind)
            {
                PropertyInfo p = options.GetType().GetProperty(b.PI.Name);
                if (!b.IsBinded && b.OA.DefaultValue != null)
                    p.SetValue(options, b.OA.DefaultValue, null);
                else if(b.IsBinded)
                {
                    if (p.PropertyType == typeof(bool))
                        p.SetValue(options, Convert.ToBoolean(b.value), null);
                    if (p.PropertyType == typeof(string))
                        p.SetValue(options, b.value, null);
                    if (p.PropertyType == typeof(int))
                        p.SetValue(options, Convert.ToInt32(b.value), null);
                }
            }
            return true;
        }
    }
}
