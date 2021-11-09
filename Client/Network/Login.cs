///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Net;
using System.Text;
using Client.Config;
using Client.Helper.Crypter;

namespace Client.Network
{
    /// <summary>
    /// http login process prior to the udp connection to the ryzom server
    /// </summary>
    public class Login
    {
        /// <summary>
        /// getting server information and try to login the client with the given credentials
        /// </summary>
        public static void CheckLogin(RyzomClient client, string login, string password, string clientApp, string customParameters)
        {
            var url = "";

            if (!ClientConfig.StartupHost.Contains("http://") && !ClientConfig.StartupHost.Contains("https://"))
            {
                url = ClientConfig.StartupHost.Contains(":80") || ClientConfig.StartupHost.Contains(":40916") ? "http://" : "https://";
            }

            url += ClientConfig.StartupHost + ClientConfig.StartupPage;

            var salt = GetServerSalt(login, url);

            var cryptedPassword = Crypt(password, salt);

            var urlLogin =
                $"{url}?cmd=login&login={login}&password={cryptedPassword}&clientApplication={clientApp}&cp=2&lg={ClientConfig.LanguageCode}{customParameters}";

            var request = WebRequest.CreateHttp(urlLogin);
            request.Method = "GET";

            using var response = (HttpWebResponse)request.GetResponse();

            using var reader =
                new StreamReader(
                    response.GetResponseStream() ?? throw new InvalidOperationException("Can't send (error code 2)"),
                    Encoding.UTF8);

            // Read stream content as string
            var responseString = reader.ReadToEnd();

            var first = responseString.IndexOf("\n\n", StringComparison.Ordinal);

            if (first == -1)
            {
                first = responseString.IndexOf("\r\r", StringComparison.Ordinal);

                if (first == -1)
                {
                    first = responseString.IndexOf("\r\n\r\n", StringComparison.Ordinal);

                    if (first != -1)
                    {
                        responseString = responseString[(first + 4)..];
                    }
                }

                else
                {
                    responseString = responseString[(first + 2)..];
                }
            }
            else
            {
                responseString = responseString[(first + 2)..];
            }

            switch (responseString[0])
            {
                case 'H':
                    throw new InvalidOperationException("missing response body (error code 65)");

                case '0':
                    // server returns an error
                    throw new InvalidOperationException($"server error: {responseString[2..]}");

                case '1':
                    var lines = responseString.Split('\n');

                    if (lines.Length != 2)
                        throw new InvalidOperationException(
                            $"Invalid server return, found {lines.Length} lines, want 2");

                    var parts = lines[0].Split('#');

                    if (parts.Length < 5)
                        throw new InvalidOperationException("Invalid server return, missing cookie and/or Ring URLs");

                    // server returns ok, we have the cookie

                    // store the cookie value and FS address for next page request
                    var currentCookie = parts[1];
                    var fsAddr = parts[2];

                    // store the ring startup page
                    var ringMainURL = parts[3];
                    var farTpUrlBase = parts[4];
                    var startStat = parts.Length >= 6 && parts[5] == "1";

                    // parse the second line (contains the domain info)
                    parts = lines[1].Split('#');

                    if (parts.Length < 3)
                        throw new InvalidOperationException("Invalid server return, missing patch URLs");

                    var r2ServerVersion = parts[0];
                    var r2BackupPatchURL = parts[1];

                    var r2PatchUrLs = parts[2].Split(' ');

                    client.Cookie = currentCookie;
                    client.FsAddr = fsAddr;
                    client.RingMainURL = ringMainURL;
                    client.FarTpUrlBase = farTpUrlBase;
                    client.StartStat = startStat;
                    client.R2ServerVersion = r2ServerVersion;
                    client.R2BackupPatchURL = r2BackupPatchURL;
                    client.R2PatchUrLs = r2PatchUrLs;
                    break;
            }
        }

        /// <summary>
        /// ask ryzom login server for password salt
        /// </summary>
        private static string GetServerSalt(string login, string url)
        {
            var urlSalt = $"{url}?cmd=ask&cp=2&login={login}&lg={ClientConfig.LanguageCode}";

            var requestSalt = WebRequest.CreateHttp(urlSalt);
            requestSalt.Method = "GET";

            using var responseSalt = (HttpWebResponse)requestSalt.GetResponse();

            using var readerSalt =
                new StreamReader(
                    responseSalt.GetResponseStream() ??
                    throw new InvalidOperationException("Can't send (error code 60)"), Encoding.UTF8);

            // Read stream content as string
            var res = readerSalt.ReadToEnd();

            if (res.Length == 0)
            {
                throw new InvalidOperationException("Empty answer from server (error code 62)");
            }

            var salt = res[0] switch
            {
                'H' => throw new InvalidOperationException("missing response body (error code 64)"),
                '0' => throw new InvalidOperationException($"server error: {res[2..]}"),
                '1' => res[2..],
                _ => throw new InvalidOperationException(res)
            };

            return salt;
        }

        /// <summary>
        /// Return a pointer to static data consisting of the "salt"
        /// followed by an encryption produced by the "key" and "salt".
        /// </summary>
        protected static string Crypt(string password, string salt)
        {
            if (salt.Length < 2 || salt[0] != '$' || salt[1] != '6') return DesCrypter.Crypt(salt, password);

            if (salt.StartsWith("$6$") && !salt.Contains("$rounds="))
            {
                salt = "$6$rounds=5000$" + salt[3..];
            }

            if (salt.EndsWith("$"))
            {
                salt = salt[..^1];
            }

            var sha512Crypter = new Sha512CrypterBase();

            return sha512Crypter.Crypt(password, salt).Replace("$rounds=5000", "");
        }
    }
}