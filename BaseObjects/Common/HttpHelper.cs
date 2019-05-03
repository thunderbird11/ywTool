using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ywTool.BaseObjects.Common
{
    public class HttpResponse
    {
        public int code;
        public byte[] data;
        public string message;

    }
    public class HttpHelper
    {
        public static HttpResponse GetResponse(string url, byte[] data, string method)
        {
            try
            {
                var http = (HttpWebRequest)WebRequest.Create(new Uri(url));
                http.Timeout = 10 * 60 * 1000;
                if (method.ToLower() == "get")
                {
                    http.Method = "GET";
                }
                else
                {
                    http.Method = "POST";
                    http.ContentType = "application/json";
                    http.ContentLength = data.Length;
                    Stream dataStream = http.GetRequestStream();
                    dataStream.Write(data, 0, data.Length);
                    dataStream.Close();
                }
                HttpResponse ret = new HttpResponse();
                HttpWebResponse response = (HttpWebResponse)http.GetResponse();
                var responseStream = response.GetResponseStream();
                StreamReader sr = new StreamReader(responseStream);

                byte[] buffer = new byte[0x1000];
                int bytes;
                using (MemoryStream ms = new MemoryStream())
                {
                    while ((bytes = responseStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        ms.Write(buffer, 0, bytes);
                    }
                    ret.data = ms.ToArray();
                }
                ret.code = (int)response.StatusCode;
                return ret;

            }
            catch (Exception e)
            {
                return new HttpResponse() { code = -1, message = e.Message };
            }
        }
    }
}
