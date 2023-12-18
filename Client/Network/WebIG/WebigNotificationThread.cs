﻿///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using API.Logger;
using Client.Config;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client.Network.WebIG
{
    public class WebigNotificationThread /*: NLMISC.IRunnable*/
    {
        private HttpClient _curl;
        private bool _running;
        private Thread _thread;
        private readonly ILogger _logger;
        private readonly RyzomClient _client;

        private static WebigNotificationThread _webigThread;

        internal static void StartWebIgNotificationThread(RyzomClient client)
        {
            _webigThread ??= new WebigNotificationThread(client);

            if (!_webigThread.IsRunning())
            {
                _webigThread.StartThread();
            }
        }

        internal static void StopWebIgNotificationThread()
        {
            if (_webigThread.IsRunning())
            {
                _webigThread.StopThread();
            }
        }

        public WebigNotificationThread(RyzomClient client)
        {
            _client = client;
            _logger = _client.GetLogger();
            _running = false;
            _thread = null;

            _curl = null;
        }

        public void Init()
        {
            if (_curl != null)
            {
                return;
            }

            var handler = new HttpClientHandler()
            {
                AllowAutoRedirect = true,
                CookieContainer = new CookieContainer(),
                UseCookies = true,

                // TODO: Add Proxy here
            };

            handler.CookieContainer.Add(new Uri(ClientConfig.WebIgMainDomain), new Cookie("ryzomId", _client?.SessionData?.Cookie ?? ""));

            _curl = new HttpClient(handler);

            if (_curl == null)
            {
                return;
            }

            _curl.DefaultRequestHeaders.TryAddWithoutValidation("user-agent", "Ryzom/Omega / v23.12.346 #adddfe118-windows-x64" /*UserAgent.GetUserAgent()*/);

            _curl.DefaultRequestHeaders.Accept.Clear();
            _curl.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));

            //_curl.DefaultRequestHeaders.TE.Clear();
            //_curl.DefaultRequestHeaders.TE.Add(new TransferCodingWithQualityHeaderValue("trailers"));

            _curl.DefaultRequestHeaders.AcceptLanguage.Clear();
            _curl.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue("en"));

            _curl.DefaultRequestHeaders.AcceptCharset.Clear();
            _curl.DefaultRequestHeaders.AcceptCharset.Add(new StringWithQualityHeaderValue("utf-8"));

            //_curl.DefaultRequestHeaders.Referrer = new Uri(ClientConfig.WebIgMainDomain);

            //NLWEB.CCurlCertificates.useCertificates(_curl);
        }

        public void Dispose()
        {
            if (_curl != null)
            {
                //curl_easy_cleanup(_curl);
                _curl.Dispose();
                _curl = null;
            }

            if (_thread == null)
                return;

            _thread.Join();
            _thread?.Abort();
            _thread = null;
        }

        private static string _curlresult;

        public void Get(string url)
        {
            if (_curl == null)
            {
                return;
            }

            _curlresult = "";

            var task = Task.Run(() => _curl.GetAsync(new Uri(url)));
            task.Wait();
            var res = task.Result;

            var task2 = Task.Run(() => res.Content.ReadAsStringAsync());
            task2.Wait();
            _curlresult = task2.Result;

            var contentType = "";

            File.WriteAllText("webresponse.html", _curlresult);

            //res = curl_easy_getinfo(_curl, CURLINFO_CONTENT_TYPE, ch);
            try
            {
                if (res.IsSuccessStatusCode && res.Content.Headers.Contains("Content-Type"))
                {
                    contentType = string.Join(' ', res.Content.Headers.GetValues("Content-Type"));
                }
            }
            catch (Exception e)
            {
                _logger.Info(e.Message);
                return;
            }

            // "text/lua; charset=utf8"
            if (contentType.IndexOf("text/lua", StringComparison.Ordinal) == 0)
            {
                var script = "";
                script = "\nlocal __WEBIG_NOTIF__= true\n" + _curlresult;
                //CInterfaceManager.getInstance().queueLuaScript(script);
                _logger.Info(script);
            }
            else
            {
                _logger.Warn($"Invalid content-type '{contentType}', expected 'text/lua'");
            }
        }

        private static readonly Random Random = new Random();

        public string RandomString()
        {
            const string chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            var s = new char[32];

            for (var i = 0; i < s.Length; i++)
            {
                s[i] = chars[Random.Next(chars.Length)];
            }

            return new string(s);
        }

        /// <summary>
        /// Called when a thread is run.
        /// </summary>
        public void Run()
        {
            if (ClientConfig.WebIgNotifInterval == 0)
            {
                _running = false;
                _logger.Warn("ClientCfg.WebIgNotifInterval == 0, notification thread not running");
                return;
            }

            var domain = ClientConfig.WebIgMainDomain;
            var ms = (uint)(ClientConfig.WebIgNotifInterval * 60 * 1000);

            _running = true;

            // first time, we wait a small amount of time to be sure everything is initialized
            Thread.Sleep(30 * 1000);

            while (_running)
            {
                //var url = "http://bierdosenhalter.de/_ryzom/title.php";
                var url = domain + "/index.php?app=notif&format=lua&rnd=" + RandomString();
                url = AddWebIgParams(url, true);
                Get(url);

                SleepLoop(ms);
            }
        }

        public void SleepLoop(uint ms)
        {
            // use smaller sleep time so stopThread() will not block too long
            // tick == 100ms
            var ticks = ms / 100;

            while (_running && ticks > 0)
            {
                Thread.Sleep(100);
                ticks--;
            }
        }

        public void StartThread()
        {
            // initialize curl outside thread
            Init();

            if (_thread == null)
            {
                _thread = new Thread(Run);
                Debug.Assert(_thread != null);
                _thread.Start();
                _logger.Warn("WebIgNotification thread started");
            }
            else
            {
                _logger.Warn("WebIgNotification thread already started");
            }
        }

        public void StopThread()
        {
            _running = false;

            if (_thread != null)
            {
                _thread.Join();
                _thread = null;

                _logger.Warn("WebIgNotification thread stopped");
            }
            else
            {
                _logger.Warn("WebIgNotification thread already stopped");
            }
        }

        public bool IsRunning()
        {
            return _running;
        }

        private string AddWebIgParams(string url, bool trustedDomain)
        {
            // no extras parameters added to url if not in trusted domains list
            if (!trustedDomain)
                return url;

            if (_client.GetApiNetworkManager().GetApiEntityManager().GetApiUserEntity() == null || !_client.SessionData.CookieValid)
                return url;

            // Workaround for user entity
            var userEntity = _client.GetApiNetworkManager().GetApiEntityManager().GetApiUserEntity();

            url += $"{(url.IndexOf('?') != -1 ? "&" : "?")}shardid={_client.CharacterHomeSessionId}" +
                   $"&name={userEntity.GetDisplayName()}" +
                   $"&lang={ClientConfig.LanguageCode}" +
                   $"&datasetid={userEntity.DataSetId()}" +
                   "&ig=1";

            if (!trustedDomain)
                return url;

            var cid = _client.SessionData.CookieUserId * 16 + _client.GetNetworkManager().PlayerSelectedSlot;
            url += $"&cid={cid}&authkey={GetWebAuthKey(_client)}";

            url += $"&ryzomId={ _client.SessionData.Cookie}";

            //    if (url.IndexOf('$') != -1)
            //    {
            //        url.Replace("$gender$", GSGENDER.UserEntity.getGender()).ToString();
            //        url.Replace("$displayName$", UserEntity.getDisplayName()); // FIXME: UrlEncode...
            //        url.Replace("$posx$", UserEntity.pos().x).ToString();
            //        url.Replace("$posy$", UserEntity.pos().y).ToString();
            //        url.Replace("$posz$", UserEntity.pos().z).ToString();
            //        url, "$post$".Replace(Math.Atan2(UserEntity.front(.ToString().y, UserEntity.front().x)));
            //
            //        // Target fields
            //        string dbPath = "UI:VARIABLES:TARGET:SLOT";
            //
            //        CInterfaceManager im = CInterfaceManager.getInstance();
            //        CCDBNodeLeaf node = NLGUI.CDBManager.getInstance().getDbProp(dbPath, false);
            //
            //        if (node != null && (byte)node.getValue32() != (byte)CLFECOMMON.INVALID_SLOT)
            //        {
            //            CEntityCL target = EntitiesMngr.entity((uint)node.getValue32());
            //
            //            if (target != null)
            //            {
            //                url.Replace("$tdatasetid$", target.dataSetId()).ToString();
            //                url.Replace("$tdisplayName$", target.getDisplayName()); // FIXME: UrlEncode...
            //                url.Replace("$tposx$", target.pos().x).ToString();
            //                url.Replace("$tposy$", target.pos().y).ToString();
            //                url.Replace("$tposz$", target.pos().z).ToString();
            //                url.Replace("$tpost$", Math.Atan2(target.front().y, target.front().x)));
            //                url.Replace("$tsheet$", target.sheetId()).ToString();
            //
            //                string type = "";
            //
            //                if (target.isFauna())
            //                {
            //                    type = "fauna";
            //                }
            //                else if (target.isNPC())
            //                {
            //                    type = "npc";
            //                }
            //                else if (target.isPlayer())
            //                {
            //                    type = "player";
            //                }
            //                else if (target.isUser())
            //                {
            //                    type = "user";
            //                }
            //
            //                url.Replace("$ttype$", target.sheetId()).ToString();
            //            }
            //            else
            //            {
            //                url.Replace("$tdatasetid$", "");
            //                url.Replace("$tdisplayName$", "");
            //                url.Replace("$tposx$", "");
            //                url.Replace("$tposy$", "");
            //                url.Replace("$tposz$", "");
            //                url.Replace("$tpost$", "");
            //                url.Replace("$tsheet$", "");
            //                url.Replace("$ttype$", "");
            //            }
            //        }
            //    }

            return url;
        }

        private static string GetWebAuthKey(RyzomClient _client)
        {
            if (_client.GetApiNetworkManager().GetApiEntityManager().GetApiUserEntity() == null || !_client.SessionData.CookieValid)
                return "";

            // authkey = <sharid><name><cid><cookie>
            var cid = _client.SessionData.CookieUserId * 16 + _client.GetNetworkManager().PlayerSelectedSlot;

            // Workaround for user entity
            var userEntity = _client.GetApiNetworkManager().GetApiEntityManager().GetApiUserEntity();


            var rawKey = _client.CharacterHomeSessionId +
                         userEntity.GetDisplayName() +
                         cid +
                         _client.SessionData.Cookie;

            var key = GetMd5(rawKey).ToLower();

            //nlinfo("rawkey = '%s'", rawKey.c_str());
            //nlinfo("authkey = %s", key.c_str());

            return key;
        }

        public static string GetMd5(string input)
        {
            // Use input string to calculate MD5 hash
            using var md5 = MD5.Create();
            var inputBytes = Encoding.ASCII.GetBytes(input);

            var hashBytes = md5.ComputeHash(inputBytes);


            // Convert the byte array to hexadecimal string prior to .NET 5
            var sb = new StringBuilder();

            foreach (var t in hashBytes)
            {
                sb.Append(t.ToString("X2"));
            }

            return sb.ToString();
        }
    }
}
