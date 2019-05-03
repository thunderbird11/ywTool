using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ywTool.BaseObjects;
using ywTool.BaseObjects.Common;
using System.IO;
using ywTool.Objects;
using Newtonsoft.Json;

namespace ywTool.Modules
{
    public class SQL_Select_Options : CmdOption
    {
        [Option('c', "connectionstring", Required = true, HelpText = "database connection string")]
        public string ConnectionString { get; set; }

        [Option('i', "input", Required = true, HelpText = "sql select statement filename")]
        public string Input { get; set; }

        [Option("parameter", Required = false, HelpText = "use ---TYPE---name value format to provide parameters, e.g. [---INT---clientid 2008]")]
        public string Parameter { get; set; }

        [Option('o', "output", Required = false, HelpText = "output filename, extension must be csv or xlsx, e.g. [Data_{{yyyyddmmHHmmss}}.xlsx]")]
        public string Output { get; set; }

        [Option('s', "show", Required = false, HelpText = "show result directly", DefaultValue = false)]
        public bool Show { get; set; }

        [Option('t', "showheader", Required = false, HelpText = "include column header", DefaultValue = false)]
        public bool ShowHeader { get; set; }

        [Option('f', "format", Required = false, HelpText = "Format of result,  Table/Chart", DefaultValue = "Table")]
        public string Format { get; set; }

        [Option("chartoptions", Required = false, HelpText = "use name|value format to provide options, e.g. [type|line|height|600|width|600]")]
        public string ChartOptions { get; set; }

        [Option("chartfilename", Required = false, HelpText = "save image as a file")]
        public string ChartFileName { get; set; }

    }
    public class SQL
    {
        /*[CommandOption("Run the select sql statement and show result", false, "SQL_Select_Options")]
        public CmdOutput Select(CmdInput ci)
        {
            SQL_Select_Options options = ci.options as SQL_Select_Options;
            var para = new Dictionary<string, object>();
            try
            {
                for (int i = 0; i < ci.Args.Count; i++)
                    if (ci.Args[i].StartsWith("---"))
                    {
                        var tmp = ci.Args[i].Substring(3).Split(new string[] { "---"},StringSplitOptions.RemoveEmptyEntries);
                        if (tmp[0].ToUpper() == "INT")
                            para.Add(tmp[1], Convert.ToInt32(ci.Args[i + 1]));
                        else if (tmp[0].ToUpper() == "STR")
                            para.Add(tmp[1], ci.Args[i + 1]);
                        else if (tmp[0].ToUpper() == "DATE")
                            para.Add(tmp[1], Convert.ToDateTime(ci.Args[i + 1]));
                        else
                            throw new Exception("Unknown type");
                    }
            }
            catch(Exception e)
            {
                return new CmdOutput(false, CmdRunState.INVALID, ci.MessageId, "input parameters are not correct:" + e.Message);
            }
            var sql = Common.CommFuns.GetSQLStatement(options.Input);
            if(string.IsNullOrWhiteSpace(sql))
                return new CmdOutput(false, CmdRunState.INVALID, ci.MessageId, "Invalid input file");
            DataSet ds = null;
            try {
                ds=DBUtil.GetDataSet(options.ConnectionString, sql, para);
            }
            catch (Exception e)
            {
                return new CmdOutput(false, CmdRunState.INVALID, ci.MessageId, "Error to execute:" + e.Message);
            }


            if (!string.IsNullOrWhiteSpace(options.Output) && !options.Show)
            {
                if (Path.GetExtension(options.Output).ToLower() != ".csv" && Path.GetExtension(options.Output).ToLower() != ".xlsx")
                    return new CmdOutput(false, CmdRunState.INVALID, ci.MessageId, "output filename extension must be csv or xlsx");

                int j = 1;
                foreach (DataTable dt in ds.Tables)
                    ci.ProcessAnOutput(true, CmdRunState.RUNNING, $"The {j++} query has <b>{dt.Rows.Count}</b> records");

                ci.ProcessAnOutput(true, CmdRunState.RUNNING, "Saving data into file");

                string filename = "";
                if (Path.GetExtension(options.Output).ToLower() == ".csv")
                    filename = Common.CommFuns.SaveDataTableToCSVFile(ds.Tables[0], options.Output);
                else
                    filename = Common.CommFuns.SaveDataSetToExcel(ds, options.Output);

                if (!string.IsNullOrWhiteSpace(filename))
                    return new CmdOutput(true, CmdRunState.FINISHED, ci.MessageId, $"Saved as {filename}");
                else
                    return new CmdOutput(false, CmdRunState.EXITWITHERROR, ci.MessageId, "command fail");
            }
            else if(options.Format.ToUpper()=="TABLE")
            {
                return new CmdOutput(true, CmdRunState.FINISHED, ci.MessageId, DataTableToHTML.Convert(options.Name,ds.Tables[0],options.ShowHeader,options.Format));
            }
            else
            {
                DSToChartPng.DToCOptions chartoptions = new DSToChartPng.DToCOptions();
                if (!string.IsNullOrWhiteSpace(options.ChartOptions))
                {
                    chartoptions= JsonConvert.DeserializeObject<DSToChartPng.DToCOptions>(options.ChartOptions);
                    chartoptions.EnginePath = GlobalSettings.NodeEngineDir;
                }
                try {
                    var pngdata = DSToChartPng.DToCFactory.Convert(ds.Tables[0], chartoptions).data;
                    var messagelist = new List<MessageStructure>();
                    if (pngdata == null) throw new Exception("Fail to generate png.");
                    if (!string.IsNullOrWhiteSpace(options.ChartFileName))
                        File.WriteAllBytes(options.ChartFileName, pngdata);
                    var message = Convert.ToBase64String(pngdata);
                    messagelist.Add(new MessageStructure(message,(int)MessageType.NONE,MessageFormat.IMAGE));                     
                    return new CmdOutput(true, CmdRunState.FINISHED, ci.MessageId,messagelist);
                }
                catch(Exception e)
                {
                    return new CmdOutput(true, CmdRunState.EXITWITHERROR, ci.MessageId, e.Message);
                }
            }
        }*/
    }
}
