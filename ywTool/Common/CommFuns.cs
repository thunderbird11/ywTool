using OfficeOpenXml;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using ywTool.BaseObjects;

namespace ywTool.Common
{
    public class CommFuns
    {
        [DllImport("shell32.dll", SetLastError = true)]
        static extern IntPtr CommandLineToArgvW([MarshalAs(UnmanagedType.LPWStr)] string lpCmdLine, out int pNumArgs);
        public static string[] CommandLineToArgs(string commandLine)
        {
            int argc;
            var argv = CommandLineToArgvW(commandLine, out argc);
            if (argv == IntPtr.Zero)
                throw new System.ComponentModel.Win32Exception();
            try
            {
                var args = new string[argc];
                for (var i = 0; i < args.Length; i++)
                {
                    var p = Marshal.ReadIntPtr(argv, i * IntPtr.Size);
                    args[i] = Marshal.PtrToStringUni(p);
                }

                return args;
            }
            finally
            {
                Marshal.FreeHGlobal(argv);
            }
        }

        public static string TranslateTimeVariable(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return "";
            DateTime t = DateTime.Now;
            if (GlobalSettings.UserVariables.ContainsKey(InternalVariable.CurrentTimeStamp))
            {
                DateTime setTime = DateTime.Now;
                if (DateTime.TryParse(GlobalSettings.UserVariables[InternalVariable.CurrentTimeStamp], out setTime))
                    t = setTime;
            }
            System.Text.RegularExpressions.Regex rgx = new System.Text.RegularExpressions.Regex("{{[YyMdHhms._-]+}}");
            var tmp = rgx.Matches(s);
            foreach (System.Text.RegularExpressions.Match v in tmp)
                s = s.Replace(v.Value, t.ToString(v.Value.Replace("{{", "").Replace("}}", "")));
            System.Text.RegularExpressions.Regex rgx1 = new System.Text.RegularExpressions.Regex("{{(-{0,1}\\d+\\|){6}[YyMdHhms._-]+}}");
            var tmp1 = rgx1.Matches(s);
            foreach (System.Text.RegularExpressions.Match v in tmp1)
            {

                var num = v.Value.Replace("{{", "").Replace("}}", "").Split('|');
                var offset = num.Take(6).Select(a => Convert.ToInt32(a)).ToList();
                var tmpdate = t;
                tmpdate = tmpdate.AddYears(offset[0]).AddMonths(offset[1]).AddDays(offset[2]).AddHours(offset[3]).AddMinutes(offset[4]).AddSeconds(offset[5]);
                s = s.Replace(v.Value, tmpdate.ToString(num[6]));
            }
            return s;
        }

        public static void WriteLog(string message)
        {
            var filename = Path.Combine(GlobalSettings.OutputFolder, DateTime.Now.ToString("yyyyMM") + ".log");
            message = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "--------" + message;
            File.AppendAllLines(filename, new List<string>() { message });
        }

        public static bool ClearTmpFile(string pattern="")
        {
            DirectoryInfo di = new DirectoryInfo(GlobalSettings.OutputTmp);
            FileInfo[] files = null;
            if (string.IsNullOrWhiteSpace(pattern))
                files = di.GetFiles();
            else
                files = di.GetFiles(pattern, SearchOption.TopDirectoryOnly);
            foreach (FileInfo file in files)
            {
                file.Delete();
            }
            if(string.IsNullOrWhiteSpace(pattern))
                foreach (DirectoryInfo dir in di.GetDirectories())
                {
                    dir.Delete(true);
                }
            return true;
        }

        public static string[] ReadAllLinesFromBatchFile(string filename)
        {
            if (!File.Exists(filename))
            {
                filename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BatchConfiguration", filename);
                if (!File.Exists(filename))
                    return null;
            }
            string[] tmp = File.ReadAllLines(filename);
            tmp = tmp.Select(a => a.Trim()).Where(a => !a.StartsWith("#") && !string.IsNullOrWhiteSpace(a)).Select(a => a).ToArray();
            return tmp;
        }

        public static string SaveStringToFile(string content, string prefix = "Output", string suffix = "txt", string filename = null)
        {
            if (string.IsNullOrWhiteSpace(filename))
                filename = $"{prefix}_" + "{{yyyyMMddHHmmss}}" + $".{suffix}";
            filename = TranslateTimeVariable(filename);
            filename = Path.Combine(GlobalSettings.OutputFolder, filename);
            File.WriteAllText(filename, content);
            return filename;
        }

        public static string Dictionary2HTML(ConcurrentDictionary<string, string> d, string keyname, string valuename)
        {
            StringBuilder sb = new StringBuilder();
            if (d == null) return sb.ToString();
            sb.AppendLine($"<table border=1><tr><th>{keyname}</th><th>{valuename}</th></tr>");
            foreach (var r in d)
            {
                sb.AppendLine($"<tr><td>{r.Key}</td><td>{r.Value}</td></tr>");
            }
            sb.AppendLine("</table>");
            return sb.ToString();
        }

        public static List<string> ListTmpFiles(string pattern, string folder = null)
        {
            List<string> ret = new List<string>();
            System.Text.RegularExpressions.Regex rgx = new System.Text.RegularExpressions.Regex(pattern);
            DirectoryInfo di = new DirectoryInfo(string.IsNullOrWhiteSpace(folder) ? GlobalSettings.OutputTmp : folder);

            foreach (FileInfo file in di.GetFiles())
            {
                if (rgx.IsMatch(file.Name))
                    ret.Add(file.FullName);
            }
            return ret;
        }

        public static string SaveDataToFile(byte[] content, string filename, bool toTmpFolder = false)
        {
            filename = Path.Combine(toTmpFolder ? GlobalSettings.OutputTmp : GlobalSettings.OutputData, filename);
            File.WriteAllBytes(filename, content);
            return filename;
        }

        public static string GetSQLStatement(string filename)
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SQL", filename);
            if (File.Exists(path))
                return File.ReadAllText(path);
            else
                return "";
        }

        public static string SaveDataTableToCSVFile(DataTable dt, string filename)
        {
            filename = Path.Combine(GlobalSettings.OutputFolder, filename);

            StringBuilder sb = new StringBuilder();

            IEnumerable<string> columnNames = dt.Columns.Cast<DataColumn>().Select(column => column.ColumnName);
            sb.AppendLine(string.Join(",", columnNames));

            foreach (DataRow row in dt.Rows)
            {
                IEnumerable<string> fields = row.ItemArray.Select(field => string.Concat("\"", field.ToString().Replace("\"", "\"\""), "\""));
                sb.AppendLine(string.Join(",", fields));
            }

            File.WriteAllText(filename, sb.ToString());
            return filename;
        }

        public static string SaveDataSetToExcel(DataSet ds, string filename)
        {
            filename = Path.Combine(GlobalSettings.OutputFolder, filename);
            FileInfo fi = new FileInfo(filename);
            using (ExcelPackage pck = new ExcelPackage(fi))
            {
                int i = 0;
                foreach (DataTable dt in ds.Tables)
                {
                    i++;
                    ExcelWorksheet ws = pck.Workbook.Worksheets.Add($"DataResult {i.ToString()}");
                    ws.Cells["A1"].LoadFromDataTable(dt, true);
                }
                pck.Save();
            }
            return filename;
        }
    }
}
