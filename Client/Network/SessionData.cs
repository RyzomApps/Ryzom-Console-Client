using System.IO;
using System.Text.Json;

namespace Client.Network
{
    public class SessionData
    {
        public string Cookie { get; set; }

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
