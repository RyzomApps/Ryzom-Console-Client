using System;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using API;
using API.Logger;
using API.Network.Web;
using Client.Config;

namespace Client.Network.Web
{
    public class WebBrowserProxyThread
    {
        private static string _url;

        private static HttpListener _listener;
        private readonly ILogger _logger;

        private readonly IWebTransfer _transfer;

        private Thread _thread;
        private static WebBrowserProxyThread _instance;

        internal static void StartThread(RyzomClient client, IWebTransfer transfer)
        {
            _instance ??= new WebBrowserProxyThread(client, transfer);
            _instance.StartThread();
        }

        private WebBrowserProxyThread(IClient client, IWebTransfer transfer)
        {
            _transfer = transfer;
            _logger = client.GetLogger();

            _listener = null;
            _url = ClientConfig.BrowserProxyUrl;
        }

        private void StartThread()
        {
            // initialize outside thread
            if (_thread == null)
            {
                _thread = new Thread(Init);
                Debug.Assert(_thread != null);
                _thread.Start();
                _logger.Debug("HttpProxyServerThread thread started");
            }
            else
            {
                _logger.Warn("HttpProxyServerThread thread already started");
            }
        }

        private void Init()
        {
            // Create a Http server and start listening for incoming connections
            _listener = new HttpListener();
            _listener.Prefixes.Add(_url);
            _listener.Start();

            _logger.Info($"Listening for connections on {_url}");

            // Handle requests
            var listenTask = HandleIncomingConnections();
            listenTask.GetAwaiter().GetResult();

            // Close the listener
            _listener.Close();
        }

        private async Task HandleIncomingConnections()
        {
            // While a user hasn't visited the `shutdown` url, keep on handling requests
            while (true)
            {
                // Will wait here until we hear from a connection
                var ctx = await _listener.GetContextAsync();

                // Peel out the requests and response objects
                var req = ctx.Request;
                var resp = ctx.Response;

                try
                {
                    // If `shutdown` url requested w/ POST, then shutdown the server after serving the page
                    if (req.HttpMethod == "GET" && req.Url.AbsolutePath == "/shutdown")
                    {
                        _logger.Info("Shutdown requested");
                        // TODO: shutdown client
                        return;
                    }

                    // TODO: implement other commands and things like that

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
                        realPath = "/" + realDomain;
                        realDomain = ClientConfig.WebIgMainDomain;
                    }

                    var url = $"{realDomain}{realPath}{req.Url.Query}";

                    var result = _transfer.Get(url);

                    if (!result.IsSuccessStatusCode)
                    {
                        _logger.Warn(url + " returned " + result.ReasonPhrase);
                        resp.Close();
                        continue;
                    }
                    byte[] content;

                    resp.ContentType = result.Content.Headers.ContentType.ToString();

                    if (result.Content.Headers.ContentType.MediaType.ToLower().Contains("text"))
                    {
                        var htmlData = result.Content.ReadAsStringAsync().Result;

                        // Replace lua tags
                        htmlData = htmlData.Replace("<lua>", "<pre><code class=\"language-clike\">");
                        htmlData = htmlData.Replace("</lua>", "</code></pre>");

                        // Replace ryzom alpha channel in html colors
                        htmlData = Regex.Replace(htmlData, "#([0-9a-f]{2})([0-9a-f]{2})([0-9a-f]{2})([0-9a-f]{2})", "#$1$2$3");

                        // Replace all non image links with proxy links
                        htmlData = Regex.Replace(htmlData, "https?://((?![^\" ]*(?:jpg|png|gif|tga))[^\" ]+)", "/$1");

                        // Remove size tags
                        htmlData = Regex.Replace(htmlData, "<([^>]*?)(size=\"(.*?)\")(.*?)>", "<$1$4>");

                        // Remove bgcolor tags
                        //htmlData = Regex.Replace(htmlData, "<([^>]*?)(bgcolor=\"(.*?)\")(.*?)>", "<$1$4>");

                        // Add some style informations
                        htmlData = htmlData.Replace("</head>",
                            "<link href=\"https://prismjs.com/themes/prism.css\" rel=\"stylesheet\" />\r\n" +
                            "<style type=\"text/css\">\r\n" +
                            "html {\r\n    color: rgba(210, 210, 210, 1.0);\r\n    font-size: 12px;\r\n    font-style: normal;\r\n    font-weight: normal;\r\n    text-shadow: none;\r\n}\r\n\r\naddress, article, aside, blockquote, details, dialog, dd, div, dl, dt,\r\nfieldset, figcaption, figure, footer, form, h1, h2, h3, h4, h5, h6,\r\nheader, hgroup, hr, li, main, nav, ol, p, pre, section, table, ul {\r\n    display: block;\r\n}\r\n\r\ntable {\r\n    display: table;\r\n}\r\n\r\ninput,. table, tr, td, th, textarea {\r\n    color: inherit;\r\n    font-weight: inherit;\r\n    font-style: inherit;\r\n    font-size: inherit;\r\n    text-shadow: inherit;\r\n}\r\n\r\ninput, textarea {\r\n    text-shadow: 1px 1px #000;\r\n}\r\n\r\na {\r\n    color: rgba(240, 155, 100, 1.0);\r\n    text-decoration: underline;\r\n}\r\n\r\nh1, h2, h3, h4, h5, h6 {\r\n    color: rgba(255, 255, 255, 1.0);\r\n}\r\n\r\nh1 {\r\n    font-size: 2em;\r\n}\r\n\r\nh2 {\r\n    font-size: 1.5em;\r\n}\r\n\r\nh3 {\r\n    font-size: 1.17em;\r\n}\r\n\r\nh4 {\r\n    font-size: 1.0em;\r\n}\r\n\r\nh5 {\r\n    font-size: 0.83em;\r\n}\r\n\r\nh6 {\r\n    font-size: 0.67em;\r\n}\r\n\r\npre {\r\n    font-family: monospace;\r\n}\r\n\r\n/* th { text-align:center; }      overrides <td align=\"..\"> property */\r\nth {\r\n    font-weight: bold;\r\n}\r\n\r\ndel {\r\n    text-decoration: line-through;\r\n}\r\n\r\nu {\r\n    text-decoration: underline;\r\n}\r\n\r\nem {\r\n    font-style: italic;\r\n}\r\n\r\nstrong {\r\n    font-weight: bold;\r\n}\r\n\r\nsmall {\r\n    font-size: smaller;\r\n}\r\n\r\ndt {\r\n    font-weight: bold;\r\n}\r\n\r\nhr {\r\n    color: rgb(120, 120, 120);\r\n}\r\n\r\n/* td { padding: 1px; }           overrides <td cellpadding=\"..\"> attribute */\r\n/* table { border-spacing: 2px; } overrides <td cellspacing=\"..\"> attribute */\r\ntable {\r\n    border-collapse: separate;\r\n}\r\n\r\nmeter::-webkit-meter-bar,\r\nmeter::-webkit-optimum-value,\r\nmeter::-webkit-suboptimum-value,\r\nmeter::-webkit-even-less-good-value {\r\n    background: none;\r\n}\r\n\r\nmeter::-webkit-meter-bar {\r\n    background-color: rgb(100, 100, 100);\r\n    width: 5em;\r\n    height: 1em;\r\n}\r\n\r\nmeter::-webkit-meter-optimum-value {\r\n    background-color: rgb(80, 220, 80);\r\n}\r\n\r\nmeter::-webkit-meter-suboptimum-value {\r\n    background-color: rgb(220, 220, 80);\r\n}\r\n\r\nmeter::-webkit-meter-even-less-good-value {\r\n    background-color: rgb(220, 80, 80);\r\n}\r\n\r\nprogress::-webkit-progress-bar,\r\nprogress::-webkit-progress-value {\r\n    background: none;\r\n}\r\n\r\nprogress::-webkit-progress-bar {\r\n    background-color: rgb(230, 230, 230);\r\n    width: 10em;\r\n    height: 1em;\r\n}\r\n\r\nprogress::-webkit-progress-value {\r\n    background-color: rgb(0, 100, 180);\r\n}\r\n\r\n* {\r\n    background-repeat: no-repeat;\r\n}\r\n" +
                            "</style>\r\n" +
                            "</head>");

                        htmlData = htmlData.Replace("</body>",
                            "<script src=\"https://prismjs.com/prism.js\"></script>\r\n" +
                            "</body>");

                        content = Encoding.UTF8.GetBytes(htmlData);
                    }
                    else
                    {
                        content = result.Content.ReadAsByteArrayAsync().Result;
                    }

                    resp.ContentEncoding = Encoding.UTF8;
                    resp.ContentLength64 = content.LongLength;

                    // Write out to the response stream (asynchronously), then close it
                    await resp.OutputStream.WriteAsync(content, 0, content.Length);
                }
                catch
                {
                    // ignored
                }
                finally
                {
                    resp.Close();
                }
            }
        }
    }
}
