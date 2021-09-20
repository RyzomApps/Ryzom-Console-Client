using System.Collections.Generic;
using MinecraftClient;
using RCC.Client;
using RCC.Network;

namespace RCC.Bots
{
    public class OnlinePlayersLogger : ChatBot
    {
        readonly Dictionary<uint, string> _friendNames = new Dictionary<uint, string>();
        readonly Dictionary<uint, CharConnectionState> _friendOnline = new Dictionary<uint, CharConnectionState>();
        private bool _initialized;

        public override void Initialize()
        {
            RyzomClient.Log.Info("Bot 'OnlinePlayersLogger' initialized.");
            RegisterChatBotCommand("list", "Lists players in the friend list.", "", Command);
        }

        public override void Update()
        {
            if (!_initialized)
                return;

            foreach (var id in _friendNames.Keys)
            {
                if (_friendNames[id] != string.Empty) continue;

                if (StringManagerClient.GetString(id, out var name))
                {
                    _friendNames[id] = name;
                }

                break; // nach einem namen müssen wir leider schluss machen, da noch ein problem mit mehreren actions in einem action block beim senden besteht -> disco
            }
        }

        public override void OnGameTeamContactStatus(uint contactId, CharConnectionState online)
        {
            _friendOnline[contactId] = online;
            var name = _friendNames[contactId] != string.Empty ? _friendNames[contactId] : "{contactId}";

            RyzomClient.Log.Info($"{name} is now {(online == CharConnectionState.CcsOnline ? "online" : "offline")}.");
        }

        public override void OnGameTeamContactInit(List<uint> vFriendListName, List<CharConnectionState> vFriendListOnline, List<string> vIgnoreListName)
        {
            for (int i = 0; i < vFriendListName.Count; i++)
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
            _friendNames.Add(nameId, StringManagerClient.GetString(nameId, out var name) ? name : string.Empty);

            RyzomClient.Log.Info($"Added {(_friendNames[nameId] != string.Empty ? _friendNames[nameId] : $"{nameId}")} to the friend list.");
        }

        public override bool OnDisconnect(DisconnectReason reason, string message)
        {
            return false;
        }

        public string Command(string cmd, string[] args)
        {
            var online = new List<string>();

            foreach (var id in _friendOnline.Keys)
            {
                var state = _friendOnline[id];

                if (state != CharConnectionState.CcsOnline) continue;

                var name = _friendNames[id];
                online.Add(name);
            }

            var ret = $"There are {online.Count}/{_friendOnline.Count} players online:\r\n";
            ret += string.Join(", ", online);

            return ret;
        }
    }
}
