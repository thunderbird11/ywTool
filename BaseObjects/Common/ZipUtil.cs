using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ywTool.BaseObjects.Common
{
    public class ZipUtil
    {
        public static byte[] ZipFiles(Dictionary<string, byte[]> filelist)
        {
            using (var stream = new MemoryStream())
            {
                using (var zipArchive = new ZipArchive(stream, ZipArchiveMode.Create, true))
                {
                    foreach (var v in filelist)
                    {
                        var file = zipArchive.CreateEntry(v.Key);
                        using (var entryStream = file.Open())
                        using (var filestream = new MemoryStream(v.Value))
                        {
                            filestream.CopyTo(entryStream);
                        }
                    }
                }
                stream.Seek(0, SeekOrigin.Begin);
                return stream.ToArray();
            }
        }

        public static Dictionary<string, byte[]> UnzipFiles(byte[] zipfile)
        {
            var ret = new Dictionary<string, byte[]>();
            using (Stream stream = new MemoryStream(zipfile))
            {
                using (ZipArchive archive = new ZipArchive(stream))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        using (Stream rawstream = entry.Open())
                        {
                            using (MemoryStream ms = new MemoryStream())
                            {
                                rawstream.CopyTo(ms);
                                var filedata = ms.ToArray();
                                ret.Add(entry.FullName, filedata);
                            }
                        }
                    }
                }
            }
            return ret;
        }

        public static Dictionary<string, byte[]> SeparateClaimFile(byte[] zipfile)
        {
            var ret = new Dictionary<string, byte[]>();
            var tmp = UnzipFiles(zipfile);
            var claimdata = tmp.Where(a => a.Key.ToLower().EndsWith(".xlsx") || a.Key.ToLower().EndsWith(".xls")).Select(a => a).FirstOrDefault();
            ret.Add(".xlsx", claimdata.Value);
            var tmp2 = tmp.Where(a => !a.Key.ToLower().EndsWith(".xlsx") && !a.Key.ToLower().EndsWith(".xls")).Select(a => a).ToDictionary(a => a.Key, a => a.Value);
            ret.Add(".zip", ZipFiles(tmp2));
            return ret;
        }

        public static byte[] AddFilesIntoZip(byte[] zipfile, Dictionary<string, byte[]> files)
        {
            var tmp = UnzipFiles(zipfile);
            foreach (var f in files)
            {
                tmp.Add(f.Key, f.Value);
            }
            return ZipFiles(tmp);
        }
    }
}
