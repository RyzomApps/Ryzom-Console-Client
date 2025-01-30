using System;
using System.IO;
using System.Text.Json;

namespace Client.Network
{
    public class SessionData
    {
        private string _cookie;

        public string Cookie
        {
            get => _cookie;
            set
            {
                _cookie = value;

                try
                {
                    var parts = _cookie.Split('|');

                    CookieUserAddr = Convert.ToInt32($"0x{parts[0]}", 16);
                    CookieUserKey = Convert.ToInt32($"0x{parts[1]}", 16);
                    CookieUserId = Convert.ToInt32($"0x{parts[2]}", 16);
                    CookieValid = !(CookieUserAddr == 0 && CookieUserKey == 0 && CookieUserId == 0);
                }
                catch (Exception)
                {
                    CookieUserAddr = 0;
                    CookieUserKey = 0;
                    CookieUserId = 0;
                    CookieValid = false;
                }
            }
        }

        public bool CookieValid { get; internal set; }
        public int CookieUserAddr { get; internal set; }
        public int CookieUserKey { get; internal set; }
        public int CookieUserId { get; internal set; }

        public string FsAddr { get; set; }

        public string RingMainURL { get; set; }

        public string FarTpUrlBase { get; set; }

        public bool StartStat { get; set; }

        /// <summary>
        /// domain server version for patch
        /// </summary>
        public string R2ServerVersion { get; set; }

        /// <summary>
        /// Backup patch server to use in case of failure of all other servers
        /// </summary>
        public string R2BackupPatchURL { get; set; }

        /// <summary>
        /// a list of patch server to use randomly
        /// </summary>
        public string[] R2PatchUrLs { get; set; }

        /// <summary>
        /// Saves the session data to a file in JSON format
        /// </summary>
        /// <param name="fileName">name of the session file</param>
        public void Save(string fileName)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var jsonString = JsonSerializer.Serialize(this, options);
            File.WriteAllText(fileName, jsonString);
        }

        /// <summary>
        /// Loads the session data from a file in JSON format
        /// </summary>
        /// <param name="fileName">name of the session file</param>
        public static SessionData Load(string fileName)
        {
            var jsonString = File.ReadAllText(fileName);
            return JsonSerializer.Deserialize<SessionData>(jsonString);
        }
    }
}
