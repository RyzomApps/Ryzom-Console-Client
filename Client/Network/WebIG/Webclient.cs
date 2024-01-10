using System;
using System.Diagnostics;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using Client.Config;

namespace Client.Network.WebIG
{
    public static class Webclient
    {
        private static CookieContainer CookieContainer = new CookieContainer();
        private static string _referer = ClientConfig.WebIgMainDomain;

        public static string GetHtmlSource(string url, bool simulateIngame = false, CookieContainer cookies = null)
        {
            return GetHtmlSource(url, "", null, "post", simulateIngame, cookies);
        }

        public static string GetHtmlSource(string url, string proxyAdress, string[] gets = null, string mType = "post", bool simulateIngame = false, CookieContainer cookies = null)
        {
            if (cookies != null)
                CookieContainer = cookies;

            string response = null;
            var postData = "";

            if (gets != null)
            {
                postData = gets.Aggregate("", (current, key) => current + (key + "&"));
                postData = System.Web.HttpUtility.UrlEncode(postData);
            }

            if (!(WebRequest.Create(url) is HttpWebRequest httpWebRequest)) 
                return null;

            if (simulateIngame) SetHeadersAndProxy(url, proxyAdress, httpWebRequest);

            if (gets != null)
            {
                try
                {
                    if (mType == "post")
                    {
                        httpWebRequest.Method = "POST";

                        httpWebRequest.ContentType = "application/x-www-form-urlencoded";
                        httpWebRequest.ContentLength = postData.Length;

                        var bytes = Encoding.UTF8.GetBytes(postData);
                        var requestStream = httpWebRequest.GetRequestStream();

                        requestStream.Write(bytes, 0, bytes.Length);
                        requestStream.Close();
                    }
                    else
                    {
                        httpWebRequest.Method = "GET";
                    }
                }
                catch (Exception exception)
                {
                    Console.WriteLine("GetHtmlSource Error: " + exception.Message + " " + url);
                }
            }

            // RESPONSE
            try
            {
                using (var webResponse = (HttpWebResponse)httpWebRequest.GetResponse())
                {
                    var enc = webResponse.ContentEncoding ?? "";
                    
                    System.IO.Stream stream = enc.ToUpperInvariant() switch
                    {
                        "GZIP" => new GZipStream(webResponse.GetResponseStream(), CompressionMode.Decompress),
                        "DEFLATE" => new DeflateStream(webResponse.GetResponseStream(), CompressionMode.Decompress),
                        _ => webResponse.GetResponseStream(),
                    };

                    if (stream != null)
                    {
                        var sb = new StringBuilder();
                        var buf = new byte[8192];
                        var resStream = stream;
                        int count;

                        do
                        {
                            count = resStream.Read(buf, 0, buf.Length);

                            if (count != 0)
                            {
                                if (webResponse.CharacterSet != null)
                                {
                                    sb.Append(Encoding.GetEncoding(webResponse.CharacterSet).GetString(buf, 0, count));

                                }
                                else
                                {
                                    // TODO: use dynamic encoding instead of UTF8 here
                                    sb.Append(Encoding.UTF8.GetString(buf, 0, count));
                                }
                            }

                            count--;

                        } while (count > 0);

                        response = sb.ToString();
                    }
                }

                _referer = url;
            }
            catch (Exception exception)
            {
                Console.WriteLine("GetHtmlSource Error: " + exception.Message + " " + url);
            }

            return response;
        }

        //public static HttpWebResponse GetImageFromUrl(string url, string proxyAdress = "")
        //{
        //    var ryzomWebIgUri = new Uri(ClientConfig.WebIgMainDomain);
        //
        //    if (url.Substring(0, 1) == "/")
        //    {
        //        url = "http://" + Config.Instance.RyzomWebIgIP + url;
        //    }
        //
        //    url = url.Replace(ryzomWebIgUri.Host, Config.Instance.RyzomWebIgIP);
        //
        //
        //    if (WebRequest.Create(url) is HttpWebRequest httpWebRequest)
        //    {
        //        SetHeadersAndProxy(url, proxyAdress, httpWebRequest);
        //
        //        try
        //        {
        //            var response = (HttpWebResponse)httpWebRequest.GetResponse();
        //
        //            return response;
        //        }
        //        catch (Exception exception)
        //        {
        //            Console.WriteLine("GetImageFromUrl Error: " + exception.Message);
        //        }
        //    }
        //
        //    return null;
        //}

        /// <summary>
        ///x Accept=*/*;q=0.3,text/html
        ///x TE=trailers
        ///x Accept-Language=de
        ///x Host=atys.ryzom.com
        ///x User-Agent=Ryzom/1.12.1 libwww/5.3.1
        ///  Cookie=ryzomId=1199DA5D|67595880|0008A2F5
        ///  Connection=TE,Keep-Alive -> couldnt do so cause we are using a close-con-server
        /// </summary>
        private static void SetHeadersAndProxy(string url, string proxyAdress, HttpWebRequest httpWebRequest)
        {
            var ryzomWebIgUri = new Uri(ClientConfig.WebIgMainDomain);

            // Change host if we connect to the app server via IP
            if (url.Contains(ryzomWebIgUri.Host) || url.StartsWith("/") || url.StartsWith("?"))
            {
                var headersFieldInfo = httpWebRequest.GetType().GetField("_HttpRequestHeaders", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);

                var wssHeaders = new CusteredHeaderCollection(ryzomWebIgUri.Host);

                if (headersFieldInfo != null) headersFieldInfo.SetValue(httpWebRequest, wssHeaders);
            }

            httpWebRequest.CookieContainer = CookieContainer;
            httpWebRequest.UserAgent = "Ryzom/Omega / v23.07.329 #b2e801f6-windows-x64";
            httpWebRequest.Accept = "*/*";

            httpWebRequest.Timeout = 1000 * 5;
            httpWebRequest.ReadWriteTimeout = 1000 * 5;

            //httpWebRequest.Headers[HttpRequestHeader.Te] = "trailers";
            httpWebRequest.Headers[HttpRequestHeader.AcceptLanguage] = "en";
            httpWebRequest.Headers[HttpRequestHeader.AcceptCharset] = "utf-8";

            httpWebRequest.UnsafeAuthenticatedConnectionSharing = true;
            //httpWebRequest.Referer = _referer;

            if (proxyAdress == "") return;

            var proxy = new WebProxy(proxyAdress);
            httpWebRequest.Proxy = proxy;
        }

        public class CusteredHeaderCollection : WebHeaderCollection
        {
            public bool HostHeaderValueReplaced { get; private set; }

            public string ClusterUrl { get; private set; }

            public CusteredHeaderCollection(string commonClusterUrl)
            {
                if (string.IsNullOrEmpty("commonClusterUrl"))
                {
                    throw new ArgumentNullException("commonClusterUrl");
                }

                ClusterUrl = commonClusterUrl;
            }

            public override string ToString()
            {
                this["Host"] = ClusterUrl;
                var tmp = base.ToString();
                HostHeaderValueReplaced = true;

                return tmp;
            }
        }
    }
}