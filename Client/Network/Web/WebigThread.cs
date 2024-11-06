///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using System;
using System.Diagnostics;
using System.Threading;
using API.Logger;
using API.Network.Web;
using Client.Config;

namespace Client.Network.Web
{
    public class WebigThread
    {
        private bool _running;

        private Thread _thread;
        public static WebigThread Instance;

        private readonly ILogger _logger;
        private readonly RyzomClient _client;
        private readonly IWebTransfer _transfer;

        private static readonly Random Random = new Random();

        internal static void StartThread(RyzomClient client, IWebTransfer transfer)
        {
            Instance ??= new WebigThread(client, transfer);

            if (Instance.IsRunning())
                return;

            Instance.StartThread();
        }

        private WebigThread(RyzomClient client, IWebTransfer transfer)
        {
            _client = client;
            _transfer = transfer;
            _logger = _client.GetLogger();
            _running = false;
            _thread = null;
        }

        /// <summary>
        /// Called when a thread is run.
        /// </summary>
        private void Run()
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
                var url = $"{domain}/index.php?app=notif&format=lua&rnd={RandomString()}";
                var result = _transfer.Get(url);

                if (!result.IsSuccessStatusCode)
                {
                    _client.Log.Warn($"Webig update returned '{result.ReasonPhrase}'");
                }
                else
                {
                    _client.Log.Info("Webig update successful.");
                }

                SleepLoop(ms);
            }
        }

        private void SleepLoop(uint ms)
        {
            // use smaller sleep time so stopThread() will not block too long - tick is 100ms
            var ticks = ms / 100;

            while (_running && ticks > 0)
            {
                Thread.Sleep(100);
                ticks--;
            }
        }

        private void StartThread()
        {
            // initialize curl outside thread
            if (_thread == null)
            {
                _thread = new Thread(Run);
                Debug.Assert(_thread != null);
                _thread.Start();
                _logger.Debug("WebIgNotification thread started");
            }
            else
            {
                _logger.Warn("WebIgNotification thread already started");
            }
        }

        private bool IsRunning()
        {
            return _running;
        }

        private static string RandomString()
        {
            const string chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            var s = new char[32];

            for (var i = 0; i < s.Length; i++)
            {
                s[i] = chars[Random.Next(chars.Length)];
            }

            return new string(s);
        }
    }
}
