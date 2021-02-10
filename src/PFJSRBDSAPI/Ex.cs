using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PFJSRBDSAPI
{
    public static class Ex
    {
        //public static string Localrequest(string u, string m, string p)
        //{
        //    string mode = m == "POST" ? m : "GET";
        //    string url = u;
        //    if (mode == "GET")
        //    {
        //        if (!string.IsNullOrEmpty(p))
        //        {
        //            url = url + "?" + p;
        //        }
        //    }
        //    var req = System.Net.WebRequest.Create(url);
        //    req.Method = mode;
        //    if (mode == "POST")
        //    {
        //        req.ContentType = "application/x-www-form-urlencoded";
        //        if (!string.IsNullOrEmpty(p))
        //        {
        //            byte[] payload = Encoding.UTF8.GetBytes(p);
        //            req.ContentLength = payload.Length;
        //            var writer = req.GetRequestStream();
        //            writer.Write(payload, 0, payload.Length);
        //            writer.Dispose();
        //            writer.Close();
        //        }
        //    }
        //    var resp = req.GetResponse();
        //    var stream = resp.GetResponseStream();
        //    var reader = new System.IO.StreamReader(stream); // 初始化读取器
        //    string result = reader.ReadToEnd();    // 读取，存储结果
        //    reader.Close(); // 关闭读取器，释放资源
        //    stream.Close();	// 关闭流，释放资源
        //    return result;
        //}
    }
}
