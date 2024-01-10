using System;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using API;
using API.Logger;
using Client.Config;

namespace Client.Network.WebIG
{
    public class HttpProxyServerThread
    {
        public static string Url = "http://localhost:8000/";

        private static HttpListener _curl;
        private readonly ILogger _logger;

        private static WebigNotificationThread _webigNotificationThread;

        public HttpProxyServerThread(IClient client, WebigNotificationThread webigNotificationThread)
        {
            _webigNotificationThread = webigNotificationThread;
            _logger = client.GetLogger();

            _curl = null;
        }

        public void Init()
        {
            // Create a Http server and start listening for incoming connections
            _curl = new HttpListener();
            _curl.Prefixes.Add(Url);
            _curl.Start();

            _logger.Info($"Listening for connections on {Url}");

            // Handle requests
            var listenTask = HandleIncomingConnections();
            listenTask.GetAwaiter().GetResult();

            // Close the listener
            _curl.Close();
        }

        public async Task HandleIncomingConnections()
        {
            // While a user hasn't visited the `shutdown` url, keep on handling requests
            while (true)
            {
                // Will wait here until we hear from a connection
                var ctx = await _curl.GetContextAsync();

                // Peel out the requests and response objects
                var req = ctx.Request;
                var resp = ctx.Response;

                try
                {
                    // If `shutdown` url requested w/ POST, then shutdown the server after serving the page
                    if (req.HttpMethod == "GET" && req.Url.AbsolutePath == "/shutdown")
                    {
                        _logger.Info("Shutdown requested");
                        return;
                    }

                    var realDomain = req.Url.AbsolutePath;
                    string realPath;

                    if (req.Url.Authority.Trim() == "")
                    {
                        realDomain = ClientConfig.WebIgMainDomain + realDomain;
                    }

                    if (realDomain.StartsWith("/"))
                        realDomain = realDomain[1..];

                    if (realDomain.IndexOf("/", StringComparison.Ordinal) != -1)
                    {
                        realPath = realDomain[realDomain.IndexOf("/", StringComparison.Ordinal)..];
                        realDomain = "http://" + realDomain[..realDomain.IndexOf("/", StringComparison.Ordinal)];
                    }
                    else if (realDomain.Trim() == "")
                    {
                        realDomain = ClientConfig.WebIgMainDomain;
                        realPath = req.Url.AbsolutePath;
                    }
                    else
                    {
                        realPath = "";
                    }

                    //_webigNotificationThread.Get(realDomain + realPath + req.Url.Query, out var response, out var content);

                    string htmlData = _webigNotificationThread.Get3(realDomain + realPath + req.Url.Query);

                    if (htmlData == null)
                    {
                        resp.Close();
                        continue;
                    }

                    //string contentType;

                    //if (response.IsSuccessStatusCode && response.Content.Headers.Contains("Content-Type"))
                    //{
                    //    contentType = string.Join(' ', response.Content.Headers.GetValues("Content-Type"));
                    //
                    //    if (contentType.IndexOf("text/html", StringComparison.Ordinal) == 0)
                    //    {
                    //var htmlData = Encoding.UTF8.GetString(content);

                    htmlData = htmlData.Replace("http://", "/");
                    htmlData = htmlData.Replace("https://", "/");

                    const string pattern = "bgcolor=\"#([0-9a-f]{2})([0-9a-f]{2})([0-9a-f]{2})([0-9a-f]{2})\"";
                    const string replacement = "style=\"background-color:#$1$2$3$4\"";

                    htmlData = Regex.Replace(htmlData, pattern, replacement);

                    htmlData = htmlData.Replace("<lua>", "<!--lua>");
                    htmlData = htmlData.Replace("</lua>", "</lua--!>");

                    var content = Encoding.UTF8.GetBytes(htmlData);
                    //    }
                    //}
                    //else
                    //{
                    //    contentType = "";
                    //}

                    //resp.ContentType = contentType;
                    resp.ContentEncoding = Encoding.UTF8;
                    resp.ContentLength64 = content.LongLength;

                    // Write out to the response stream (asynchronously), then close it
                    await resp.OutputStream.WriteAsync(content, 0, content.Length);

                    resp.Close();
                }
                catch (Exception e)
                {

                }
                finally
                {
                    resp.Close();
                }
            }
        }
    }
}
