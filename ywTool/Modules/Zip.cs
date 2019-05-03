using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ywTool.BaseObjects;
using ywTool.BaseObjects.Common;

namespace ywTool.Modules
{
    public class Zip_Unzip_Options : CmdOption
    {
        [Option('f', "filename", Required = true, HelpText = "File in tmp folder to unzip")]
        public string FileName { get; set; }

        [Option('d', "destination", Required = true, HelpText = "Folder to save files")]
        public string Destination { get; set; }
    }
    public class Zip_AddFiles_Options : CmdOption
    {
        [Option('f', "filename", Required = true, HelpText = "Files in tmp folder to add into Zip")]
        public string FileName { get; set; }

        [Option('z', "zipfile", Required = true, HelpText = "zip file")]
        public string ZipFile { get; set; }

    }
    public class Zip
    {
        [CommandOption("Unzip", false,  "Zip_Unzip_Options")]
        public CmdOutput Unzip(CmdInput ci)
        {
            Zip_Unzip_Options options = ci.options as Zip_Unzip_Options;
            StringBuilder sb = new StringBuilder();
            try
            {
                var zfiles = Common.CommFuns.ListTmpFiles(options.FileName);
                if (zfiles.Count == 0)
                    return new CmdOutput(false, CmdRunState.FINISHEDWITHWARNING, ci.MessageId, "No file to unzip");
                foreach (var zf in zfiles)
                {
                    sb.AppendLine($"Unzip {zf}:</br>");
                    var files = ZipUtil.UnzipFiles(File.ReadAllBytes(zf));
                    foreach (var f in files)
                    {
                        var d = Path.Combine(options.Destination, f.Key);
                        var fd = Path.GetDirectoryName(d);
                        if (!Directory.Exists(fd))
                            Directory.CreateDirectory(fd);
                        File.WriteAllBytes(d, f.Value);
                        sb.AppendLine($"&nbsp;&nbsp;{d}</br>");
                    }
                }
            }
            catch (Exception e)
            {
                sb.AppendLine($"Failed to unzip." + e.Message);
                return new CmdOutput(false, CmdRunState.EXITWITHERROR, ci.MessageId, sb.ToString());
            }

            return new CmdOutput(true, CmdRunState.FINISHED, ci.MessageId, sb.ToString());
        }

        [CommandOption("Add Files into Zip", false,  "Zip_AddFiles_Options")]
        public CmdOutput AddFiles(CmdInput ci)
        {
            Zip_AddFiles_Options options = ci.options as  Zip_AddFiles_Options;
            StringBuilder sb = new StringBuilder();
            try
            {
                var zfiles = Common.CommFuns.ListTmpFiles(options.FileName);
                if (zfiles.Count == 0)
                    return new CmdOutput(false, CmdRunState.FINISHEDWITHWARNING, ci.MessageId, "No file to add");
                byte[] zip = File.ReadAllBytes(options.ZipFile);
                Dictionary<string, byte[]> data = new Dictionary<string, byte[]>();
                foreach (var f in zfiles)
                {
                    data.Add(Path.GetFileName(f), File.ReadAllBytes(f));
                    sb.AppendLine($"Added {f} into zipfile<br/>");
                }
                var ret = ZipUtil.AddFilesIntoZip(zip, data);
                File.WriteAllBytes(options.ZipFile, ret);

            }
            catch (Exception e)
            {
                sb.AppendLine($"Failed to add." + e.Message);
                return new CmdOutput(false, CmdRunState.EXITWITHERROR, ci.MessageId, sb.ToString());
            }

            return new CmdOutput(true, CmdRunState.FINISHED, ci.MessageId, sb.ToString());
        }
    }
}
