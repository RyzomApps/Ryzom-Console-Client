///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using System;
using System.Globalization;
using API.Entity;
using Client.Config;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using API.Helper;
using API.Network.Web;

namespace Client.Network.Web
{
    /// <summary>
    /// Requesting websites and images from the web.
    /// </summary>
    public class WebTransfer : IWebTransfer
    {
        private readonly RyzomClient _ryzomClient;
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Contructor for the Transfer class.
        /// </summary>
        /// <param name="ryzomClient">RyzomClient used for session data.</param>
        public WebTransfer(RyzomClient ryzomClient)
        {
            _ryzomClient = ryzomClient;

            // Cookies
            var cookies = new CookieContainer();
            cookies.Add(new Uri(ClientConfig.WebIgMainDomain), new Cookie("ryzomId", _ryzomClient?.SessionData?.Cookie ?? ""));

            // Handler
            var handler = new HttpClientHandler()
            {
                AllowAutoRedirect = true,
                UseCookies = true,
                CookieContainer = cookies,
            };

            // TODO: Add Proxy here
            //if (ClientConfig.UseProxy)
            //    handler.Proxy = new WebProxy();

            _httpClient = new HttpClient(handler);
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("user-agent", Constants.UserAgent);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
            _httpClient.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue(CultureInfo.GetCultureInfo(ClientConfig.LanguageCode).ToString()));
            _httpClient.DefaultRequestHeaders.AcceptCharset.Add(new StringWithQualityHeaderValue("utf-8"));
        }

        /// <summary>
        ///  Send a GET request to the specified Uri including the WebIg parameters.
        /// </summary>
        /// <param name="url">Url the request is sent to.</param>
        /// <returns>Response to the get request.</returns>
        public HttpResponseMessage Get(string url)
        {
            // TODO: Check if trusted
            url = AddWebIgParams(url, true);

            var task = Task.Run(() => _httpClient.GetAsync(new Uri(url)));
            task.Wait();

            return task.Result;
        }

        /// <summary>
        /// Appends special authentication parameters for web integration.
        /// This method enhances the provided URL with necessary authentication details.
        /// </summary>
        /// <param name="url">The original URL to which the authentication parameters will be added.</param>
        /// <param name="trustedDomain">Indicates whether the domain is trusted (e.g. Ryzom App domain).</param>
        /// <returns>A modified URL containing the appended authentication parameters.</returns>
        internal string AddWebIgParams(string url, bool trustedDomain)
        {
            // no extras parameters added to url if not in trusted domains list
            if (!trustedDomain)
                return url;

            if (_ryzomClient.GetApiNetworkManager().GetApiEntityManager().GetApiUserEntity() == null || !_ryzomClient.SessionData.CookieValid)
                return url;

            // Workaround for user entity
            var userEntity = _ryzomClient.GetApiNetworkManager().GetApiEntityManager().GetApiUserEntity();

            var name = _ryzomClient.GetNetworkManager()?.PlayerSelectedHomeShardName;
            name = EntityHelper.RemoveTitleAndShardFromName(name);

            url += $"{(url.IndexOf('?') != -1 ? "&" : "?")}shardid={_ryzomClient.CharacterHomeSessionId}" +
                   $"&name={name}" +
                   $"&lang={ClientConfig.LanguageCode}" +
                   $"&datasetid={userEntity.DataSetId()}" +
            "&ig=1";

            var cid = _ryzomClient.SessionData.CookieUserId * 16 + _ryzomClient.GetNetworkManager().PlayerSelectedSlot;
            url += $"&cid={cid}&authkey={GetWebAuthKey()}";

            if (url.IndexOf('$') == -1)
                return url;

            //url.Replace("$gender$", GSGENDER.UserEntity.getGender()).ToString();
            url = url.Replace("$displayName$", userEntity.GetDisplayName()); // FIXME: UrlEncode...
            url = url.Replace("$posx$", userEntity.Pos.X.ToString(CultureInfo.InvariantCulture));
            url = url.Replace("$posy$", userEntity.Pos.Y.ToString(CultureInfo.InvariantCulture));
            url = url.Replace("$posz$", userEntity.Pos.Z.ToString(CultureInfo.InvariantCulture));
            url = url.Replace("$post$", Math.Atan2(userEntity.Front.Y, userEntity.Front.X).ToString(CultureInfo.InvariantCulture));

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

            return url;
        }

        /// <summary>
        /// Generates a web authentication key
        /// </summary>
        private string GetWebAuthKey()
        {
            if (_ryzomClient.GetApiNetworkManager().GetApiEntityManager().GetApiUserEntity() == null || !_ryzomClient.SessionData.CookieValid)
                return "";

            // authkey = <sharid><name><cid><cookie>
            var cid = _ryzomClient.SessionData.CookieUserId * 16 + _ryzomClient.GetNetworkManager().PlayerSelectedSlot;

            // Workaround for user entity
            var userEntity = _ryzomClient.GetApiNetworkManager().GetApiEntityManager().GetApiUserEntity();

            var rawKey = $"{_ryzomClient.CharacterHomeSessionId}{userEntity.GetDisplayName()}{cid}'{_ryzomClient.SessionData.Cookie}'";

            return Misc.GetMD5(rawKey).ToLower();
        }
    }
}