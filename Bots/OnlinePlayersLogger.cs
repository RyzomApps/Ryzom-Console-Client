using MinecraftClient;
using RCC.Chat;
using RCC.Client;
using RCC.Commands;
using RCC.Helper;
using RCC.Network;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using RCC.Config;

namespace RCC.Bots
{
    public class OnlinePlayersLogger : ChatBot
    {
        readonly Dictionary<uint, string> _friendNames = new Dictionary<uint, string>();
        readonly Dictionary<uint, CharConnectionState> _friendOnline = new Dictionary<uint, CharConnectionState>();
        private bool _initialized;

        private DateTime _lastUpdateToServer = DateTime.MinValue;
        readonly TimeSpan _intervalIntervalUpdateToServer = TimeSpan.FromSeconds(60);

        Queue<string> namesToAdd = new Queue<string>();

        public override void Initialize()
        {
            RyzomClient.Log.Info("Bot 'OnlinePlayersLogger' initialized.");
            RegisterChatBotCommand("list", "Lists all online players in the friend list.", "", Command);
            RegisterChatBotCommand("addfriendfile", "Adds a newline separated text file of player names to the friends list.", "", Command);
            if (ClientConfig.OnlinePlayersApi.Trim().Equals(string.Empty))
                RyzomClient.Log.Info("No server for player online status updates set: Not using this feature.");
        }

        Random r = new Random();

        public override void Update()
        {
            if (!_initialized)
                return;

            if (r.NextDouble() < 0.66)
                return;

            if (namesToAdd.Count > 0)
            {
                var name = namesToAdd.Dequeue();

                if (_friendNames.ContainsValue(name))
                    return;

                RyzomClient.Log.Info($"Trying to add {name} to the friend list.");
                new AddFriend().Run((RyzomClient)RyzomClient.GetInstance(), "addfriend " + name, null);

                return;
            }

            foreach (var id in _friendNames.Keys)
            {
                if (_friendNames[id] != string.Empty)
                    continue;

                if (StringManagerClient.GetString(id, out var name))
                {
                    _friendNames[id] = Entity.RemoveTitleAndShardFromName(name).ToLower();
                }

                return; // nach einem namen müssen wir leider schluss machen, da noch ein problem mit mehreren actions in einem action block beim senden besteht -> disco
            }

            if (ClientConfig.OnlinePlayersApi.Trim().Equals(string.Empty))
                return;

            if (DateTime.Now <= _lastUpdateToServer + _intervalIntervalUpdateToServer)
                return;

            _lastUpdateToServer = DateTime.Now + _intervalIntervalUpdateToServer;

            Task.Factory.StartNew(() =>
            {
                foreach (var id in _friendNames.Keys)
                {
                    if (_friendNames[id] == string.Empty)
                        continue;

                    SendUpdate(_friendNames[id], _friendOnline[id]);
                }
            });
        }

        public override void OnGameTeamContactStatus(uint contactId, CharConnectionState online)
        {
            var friend = _friendOnline.ElementAt((int)contactId);

            _friendOnline[friend.Key] = online;
            var name = _friendNames[friend.Key] != string.Empty ? _friendNames[friend.Key] : $"{contactId}";

            RyzomClient.Log.Info($"{name} is now {(online == CharConnectionState.CcsOnline ? "online" : "offline")}.");
        }

        public override void OnGameTeamContactInit(List<uint> vFriendListName, List<CharConnectionState> vFriendListOnline, List<string> vIgnoreListName)
        {
            for (var i = 0; i < vFriendListName.Count; i++)
            {
                var id = vFriendListName[i];
                var state = vFriendListOnline[i];

                _friendOnline.Add(id, state);

                _friendNames.Add(id, /*StringManagerClient.GetString(id, out string name) ? name :*/ string.Empty);
            }

            RyzomClient.Log.Info($"Initialised friend list with {vFriendListName.Count} contacts.");

            _initialized = true;
        }

        public override void OnTeamContactCreate(in uint contactId, in uint nameId, CharConnectionState online, in byte nList)
        {
            _friendOnline.Add(nameId, online);
            _friendNames.Add(nameId, StringManagerClient.GetString(nameId, out var name) ? Entity.RemoveTitleAndShardFromName(name).ToLower() : string.Empty);

            RyzomClient.Log.Info($"Added {(_friendNames[nameId] != string.Empty ? _friendNames[nameId] : $"{nameId}")} to the friend list.");
        }

        public override bool OnDisconnect(DisconnectReason reason, string message)
        {
            return false;
        }

        public override void OnChat(in uint compressedSenderIndex, string ucstr, string rawMessage, ChatGroupType mode, in uint dynChatId,
            string senderName, in uint bubbleTimer)
        {
            var name = Entity.RemoveTitleAndShardFromName(senderName).ToLower();

            if (name.Trim().Equals(string.Empty))
                return;

            if (name.ToLower().Equals(Entity.RemoveTitleAndShardFromName(Connection.PlayerSelectedHomeShardName).ToLower()))
                return;

            if (_friendNames.ContainsValue(name)) return;

            if (name.StartsWith("~"))
                name = name[1..];

            RyzomClient.Log.Info($"{name} will be added to the friends list.");
            new AddFriend().Run((RyzomClient)RyzomClient.GetInstance(), "addfriend " + name, null);
        }

        public string Command(string cmd, string[] args)
        {
            if (cmd.IndexOf(" ") != -1)
            {
                cmd = cmd.Substring(0, cmd.IndexOf(" "));
            }

            if (cmd.ToLower() == "list")
            {
                var online = new List<string>();

                foreach (var id in _friendOnline.Keys)
                {
                    var state = _friendOnline[id];

                    if (state != CharConnectionState.CcsOnline) continue;

                    var name = !_friendNames[id].Equals(string.Empty) ? _friendNames[id] : $"{id}";
                    online.Add(name);
                }

                var ret = $"There are {online.Count}/{_friendOnline.Count} players online:\r\n";
                ret += string.Join(", ", online);

                return ret;
            }
            else if (cmd.ToLower() == "addfriendfile")
            {
                if (args.Length != 1)
                {
                    RyzomClient.Log?.Warn($"Please specify a file of players to add.");
                    return "";
                }

                var path = args[0];

                if (!File.Exists(path))
                {
                    RyzomClient.Log?.Warn($"File does not exist.");
                    return "";
                }

                // Open the file to read from.
                string[] readText = File.ReadAllLines(path);
                foreach (string name in readText)
                {
                    //if (name.Trim() != string.Empty)
                    //    new AddFriend().Run((RyzomClient)RyzomClient.GetInstance(), "addfriend " + name, null);

                    namesToAdd.Enqueue(name);
                }

                return "";
            }

            RyzomClient.Log?.Warn($"Command unknown: " + cmd);
            return "";
        }

        public static void SendUpdate(string name, CharConnectionState status)
        {
            try
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(ClientConfig.OnlinePlayersApi);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";

                //using var md5 = MD5.Create();
                var hash = Misc.CreateMD5(DateTime.Now.ToString("ddMMyyyy")).ToLower();

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    var json =
                        $"[{{\"auth\":\"{hash}\",\"name\":\"{name}\",\"status\":\"{(status == CharConnectionState.CcsOnline ? "online" : "offline")}\"}}]";

                    //Debug.Print(json);

                    streamWriter.Write(json);
                }

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                using var streamReader = new StreamReader(httpResponse.GetResponseStream());

                var result = streamReader.ReadToEnd();

                //Debug.Print(result);
            }
            catch (Exception e)
            {
                RyzomClient.Log.Error("Error while posting to the players API: " + e.Message);
            }
        }
    }
}
