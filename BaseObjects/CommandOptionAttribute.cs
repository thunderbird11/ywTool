using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ywTool.BaseObjects
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class CommandOptionAttribute : Attribute
    {
        private string _helptext;
        private bool _logonrequired;
        private bool _allowcancel;
        private string _parameteroptionclass;

        public CommandOptionAttribute(string HelpText, bool LogonRequired = true, string ParameterOptionClass = "", bool AllowCancel = false)
        {
            _helptext = HelpText;
            _logonrequired = LogonRequired;
            _parameteroptionclass = ParameterOptionClass;
            _allowcancel = AllowCancel;
        }

        public string HelpText { get { return _helptext; } }
        public bool LogonRequired { get { return _logonrequired; } }
        public bool AllowCancel { get { return _allowcancel; } }
        public string ParameterOptionClass { get { return _parameteroptionclass; } }
    }
}
