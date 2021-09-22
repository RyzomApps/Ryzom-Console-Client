using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using RCC.Automata.Internal;
using RCC.Chat;
using RCC.Client;
using RCC.Commands;
using RCC.Config;
using RCC.Helper;
using RCC.Network;

namespace RCC.Automata
{
    public class OnlinePlayersLogger : AutomatonBase
    {
        private readonly Dictionary<uint, string> _friendNames = new Dictionary<uint, string>();
        private readonly Dictionary<uint, CharConnectionState> _friendOnline = new Dictionary<uint, CharConnectionState>();
        private bool _initialized;

        private DateTime _lastApiServerUpdate = DateTime.MinValue;
        private DateTime _lastWhoCommand = DateTime.MinValue;

        private readonly TimeSpan _intervalApiServer = TimeSpan.FromSeconds(60);
        private readonly TimeSpan _intervalWhoCommand = TimeSpan.FromSeconds(60 * 10);

        private readonly Queue<string> _namesToAdd = new Queue<string>();

        private bool _whoChatMode;
        private const string WhoPlayerPattern = @"^\&SYS\&(?<name>[a-zA-Z].*)\(Atys\)\.$";
        private readonly Regex _whoRegex = new Regex(WhoPlayerPattern);

        private string _playerName;

        public override void Initialize()
        {
            RyzomClient.GetInstance().GetLogger().Info("Bot 'OnlinePlayersLogger' initialized.");
            RegisterAutomatonCommand("list", "Lists all online players in the friend list.", "", Command);
            RegisterAutomatonCommand("importfriends", "Imports a newline separated text file of player names to the friends list.", "<filename>", Command);
            RegisterAutomatonCommand("exportfriends", "Exports a newline separated text file of player names from the friends list.", "<filename>", Command);
            if (ClientConfig.OnlinePlayersApi.Trim().Equals(string.Empty))
                RyzomClient.GetInstance().GetLogger().Info("No server for player online status updates set: Not using this feature.");
        }

        /// <remarks>nach einer aktion jeweils abbrechen, da noch ein problem mit mehreren actions in einem action block beim senden besteht -> disco</remarks>
        public override void Update()
        {
            if (!_initialized)
                return;

            // Get missing names from server
            foreach (var id in _friendNames.Keys.Where(id => _friendNames[id] == string.Empty))
            {
                if (Handler.GetStringManager().GetString(id, out var name, Handler.GetNetworkManager()))
                {
                    _friendNames[id] = Entity.RemoveTitleAndShardFromName(name).ToLower();
                }

                return;
            }

            // add new names to the friends list
            if (!_friendNames.ContainsValue(string.Empty) && _namesToAdd.Count > 0)
            {
                var name = _namesToAdd.Dequeue();

                if (_playerName == name)
                    return;

                if (_friendNames.ContainsValue(name))
                    return;

                RyzomClient.GetInstance().GetLogger().Info($"Trying to add {name} to the friend list. {_namesToAdd.Count} left.");
                new AddFriend().Run((RyzomClient)RyzomClient.GetInstance(), "addfriend " + name, null);

                return;
            }

            // do the who
            if (!_friendNames.ContainsValue(string.Empty) && DateTime.Now > _lastWhoCommand + _intervalWhoCommand)
            {
                _lastWhoCommand = DateTime.Now + _intervalWhoCommand;

                new Who().Run((RyzomClient)RyzomClient.GetInstance(), "who ", null);
            }

            // update the api
            if (!_friendNames.ContainsValue(string.Empty) && !ClientConfig.OnlinePlayersApi.Trim().Equals(string.Empty))
            {
                if (DateTime.Now > _lastApiServerUpdate + _intervalApiServer)
                {
                    _lastApiServerUpdate = DateTime.Now + _intervalApiServer;

                    Task.Factory.StartNew(() =>
                    {
                        foreach (var id in _friendNames.Keys.Where(id => _friendNames[id] != string.Empty))
                        {
                            SendUpdate(_friendNames[id], _friendOnline[id]);
                        }
                    });
                }
            }
        }

        public override void OnGameTeamContactStatus(uint contactId, CharConnectionState online)
        {
            var (key, _) = _friendOnline.ElementAt((int)contactId);

            _friendOnline[key] = online;
            var name = _friendNames[key] != string.Empty ? _friendNames[key] : $"{contactId}";

            RyzomClient.GetInstance().GetLogger().Info($"{name} is now {(online == CharConnectionState.CcsOnline ? "online" : "offline")}.");
        }

        public override void OnGameTeamContactInit(List<uint> vFriendListName, List<CharConnectionState> vFriendListOnline, List<string> vIgnoreListName)
        {
            for (var i = 0; i < vFriendListName.Count; i++)
            {
                var id = vFriendListName[i];
                var state = vFriendListOnline[i];

                _friendOnline.Add(id, state);

                _friendNames.Add(id, /*StringManager.GetString(id, out string name) ? name :*/ string.Empty);
            }

            RyzomClient.GetInstance().GetLogger().Info($"Initialised friend list with {vFriendListName.Count} contacts.");

            _playerName = Entity.RemoveTitleAndShardFromName(Handler.GetNetworkManager().PlayerSelectedHomeShardName).ToLower();

            _initialized = true;
        }

        public override void OnTeamContactCreate(in uint contactId, in uint nameId, CharConnectionState online, in byte nList)
        {
            _friendOnline.Add(nameId, online);
            _friendNames.Add(nameId, Handler.GetStringManager().GetString(nameId, out var name, Handler.GetNetworkManager()) ? Entity.RemoveTitleAndShardFromName(name).ToLower() : string.Empty);

            RyzomClient.GetInstance().GetLogger().Info($"Added {(_friendNames[nameId] != string.Empty ? _friendNames[nameId] : $"{nameId}")} to the friend list.");
        }

        public override bool OnDisconnect(DisconnectReason reason, string message)
        {
            return false;
        }

        public override void OnChat(in uint compressedSenderIndex, string ucstr, string rawMessage, ChatGroupType mode, in uint dynChatId,
            string senderName, in uint bubbleTimer)
        {
            // Try to add player from who command
            if (mode == ChatGroupType.System)
            {
                if (_whoChatMode)
                {
                    var match = _whoRegex.Match(ucstr);

                    if (match.Success)
                    {
                        var whoName = match.Groups["name"].Value.ToLower();

                        if (!_friendNames.ContainsValue(whoName) && !_namesToAdd.Contains(whoName))
                            _namesToAdd.Enqueue(whoName);
                    }
                    else
                    {
                        _whoChatMode = false;
                    }
                }

                if (ucstr.StartsWith(@"&SYS&Online characters in region"))
                {
                    _whoChatMode = true;
                }
            }

            // try to add players from the chat
            var name = Entity.RemoveTitleAndShardFromName(senderName).ToLower();

            if (name.Trim().Equals(string.Empty))
                return;

            if (name.ToLower().Equals(Entity.RemoveTitleAndShardFromName(Handler.GetNetworkManager().PlayerSelectedHomeShardName).ToLower()))
                return;

            if (_friendNames.ContainsValue(name)) return;

            if (name.StartsWith("~"))
                name = name[1..];

            RyzomClient.GetInstance().GetLogger().Info($"{name} will be added to the friends list.");
            new AddFriend().Run((RyzomClient)RyzomClient.GetInstance(), "addfriend " + name, null);
        }

        public string Command(string cmd, string[] args)
        {
            if (cmd.IndexOf(" ", StringComparison.Ordinal) != -1)
            {
                cmd = cmd.Substring(0, cmd.IndexOf(" ", StringComparison.Ordinal));
            }

            switch (cmd.ToLower())
            {
                case "list":
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

                case "importfriends" when args.Length != 1:
                    RyzomClient.GetInstance().GetLogger()?.Error("Please specify a file for the players to import.");
                    return "";

                case "importfriends":
                    var pathR = args[0];

                    if (!File.Exists(pathR))
                    {
                        RyzomClient.GetInstance().GetLogger()?.Error("File does not exist.");
                        return "";
                    }

                    // Open the file to read from
                    var readText = File.ReadAllLines(pathR);

                    foreach (var name in readText)
                    {
                        _namesToAdd.Enqueue(name);
                    }

                    return "";

                case "exportfriends" when args.Length != 1:
                    RyzomClient.GetInstance().GetLogger()?.Error("Please specify a file for the players to export.");
                    return "";

                case "exportfriends":
                    var pathW = args[0];

                    if (_friendNames.ContainsValue(string.Empty))
                    {
                        RyzomClient.GetInstance().GetLogger()?.Error("There are unloaded playernames. Please wait before all names are known by the client.");
                        return "";
                    }

                    var writeText = _friendNames.Values.Aggregate("", (current, name) => current + (name + "\r\n"));

                    // Write names to the file
                    File.WriteAllText(pathW, writeText);

                    return "";


                default:
                    RyzomClient.GetInstance().GetLogger()?.Warn("CommandBase unknown: " + cmd);
                    return "";
            }
        }

        public static void SendUpdate(string name, CharConnectionState status)
        {
            try
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(ClientConfig.OnlinePlayersApi);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";

                //using var md5 = MD5.Create();
                var hash = Misc.GetMD5(DateTime.Now.ToString("ddMMyyyy")).ToLower();

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    var json =
                        $"[{{\"auth\":\"{hash}\",\"name\":\"{name}\",\"status\":\"{(status == CharConnectionState.CcsOnline ? "online" : "offline")}\"}}]";

                    //Debug.Print(json);

                    streamWriter.Write(json);
                }

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                using var streamReader = new StreamReader(httpResponse.GetResponseStream() ?? throw new NullReferenceException());
            }
            catch (Exception e)
            {
                RyzomClient.GetInstance().GetLogger().Error("Error while posting to the players API: " + e.Message);
            }
        }
    }
}
