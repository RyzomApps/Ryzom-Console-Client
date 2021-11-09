﻿using RCC.Automata.Internal;
using RCC.Chat;
using RCC.Commands;
using RCC.Config;
using RCC.Helper;
using RCC.Network;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;

namespace RCC.Automata
{
    public class OnlinePlayersLogger : AutomatonBase
    {
        readonly Mutex mutex = new Mutex();

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

        /// <inheritdoc />
        public override void OnInitialize()
        {
            Handler.GetLogger().Info("Automaton 'OnlinePlayersLogger' initialized.");
            RegisterAutomatonCommand("list", "Lists all online players in the friend list.", "", Command);
            RegisterAutomatonCommand("importfriends", "Imports a newline separated text file of player names to the friends list.", "<filename>", Command);
            RegisterAutomatonCommand("exportfriends", "Exports a newline separated text file of player names from the friends list.", "<filename>", Command);
            RegisterAutomatonCommand("sendAPI", "Sending Player Array manually to API Server", "", Command);
            if (ClientConfig.OnlinePlayersApi.Trim().Equals(string.Empty))
                Handler.GetLogger().Info("No server for player online status updates set: Not using this feature.");
        }

        /// <inheritdoc />
        /// <remarks>cancel after each action, because there is still a problem with multiple actions in one action block when sending -> disco</remarks>
        public override void OnUpdate()
        {
            if (!_initialized)
                return;

            // Get missing names from server
            foreach (var id in _friendNames.Keys.Where(id => _friendNames[id] == string.Empty))
            {
                if (Handler.GetStringManager().GetString(id, out var name, Handler.GetNetworkManager()))
                {
                    _friendNames[id] = Entity.Entity.RemoveTitleAndShardFromName(name).ToLower();
                }

                return;
            }

            // Add new names to the friends list
            if (!_friendNames.ContainsValue(string.Empty) && _namesToAdd.Count > 0)
            {
                var name = _namesToAdd.Dequeue();

                if (_playerName == name)
                    return;

                if (_friendNames.ContainsValue(name))
                    return;

                Handler.GetLogger().Info($"Trying to add {name} to the friend list. {_namesToAdd.Count} left.");
                new AddFriend().Run(Handler, "addfriend " + name, null);

                return;
            }

            // Do the who
            if (!_friendNames.ContainsValue(string.Empty) && DateTime.Now > _lastWhoCommand + _intervalWhoCommand)
            {
                _lastWhoCommand = DateTime.Now + _intervalWhoCommand;

                new Who().Run(Handler, "who ", null);
            }

            // Update the API
            if (!_friendNames.ContainsValue(string.Empty) && !ClientConfig.OnlinePlayersApi.Trim().Equals(string.Empty))
            {
                if (DateTime.Now > _lastApiServerUpdate + _intervalApiServer)
                {
                    _lastApiServerUpdate = DateTime.Now + _intervalApiServer;

                    Task.Factory.StartNew(SendApiUpdate);
                }
            }
        }

        /// <inheritdoc />
        public override void OnTeamContactStatus(uint contactListIndex, CharConnectionState online)
        {
            var (key, _) = _friendOnline.ElementAt((int)contactListIndex);

            // if friend already has that status return
            if (_friendOnline[key] == online)
                return;

            _friendOnline[key] = online;
            var name = _friendNames[key] != string.Empty ? _friendNames[key] : $"{contactListIndex}";


            Handler.GetLogger().Info($"{name} is now {(online == CharConnectionState.CcsOnline ? "online" : "offline")}.");
        }

        /// <inheritdoc />
        public override void OnTeamContactRemove(uint contactListIndex, byte nList)
        {
            // 0 is friendlist - 1 ignore list
            if (nList != 0) return;

            var (key, _) = _friendOnline.ElementAt((int)contactListIndex);

            Handler.GetLogger().Info($"Removing {(_friendNames[key] != string.Empty ? _friendNames[key] : $"{key}")} from the friend list.");

            mutex.WaitOne();
            _friendOnline.Remove(key);
            _friendNames.Remove(key);
            mutex.ReleaseMutex();
        }

        /// <inheritdoc />
        public override void OnTeamContactInit(List<uint> friendListNames, List<CharConnectionState> friendListOnline, List<string> ignoreListNames)
        {
            for (var i = 0; i < friendListNames.Count; i++)
            {
                var id = friendListNames[i];
                var state = friendListOnline[i];

                mutex.WaitOne();
                _friendOnline.Add(id, state);
                _friendNames.Add(id, /*Handler.GetStringManager().GetString(id, out string name, Handler.GetNetworkManager()) ? name :*/ string.Empty);
                mutex.ReleaseMutex();
            }

            Handler.GetLogger().Info($"Initialized friend list with {friendListNames.Count} contacts.");

            _playerName = Entity.Entity.RemoveTitleAndShardFromName(Handler.GetNetworkManager().PlayerSelectedHomeShardName).ToLower();

            _initialized = true;
        }

        /// <inheritdoc />
        public override void OnTeamContactCreate(in uint contactId, in uint nameId, CharConnectionState online, in byte nList)
        {
            if (nList != 0) return;

            if (_friendOnline.ContainsKey(nameId) || _friendNames.ContainsKey(nameId)) return;

            mutex.WaitOne();
            _friendOnline.Add(nameId, online);
            _friendNames.Add(nameId, Handler.GetStringManager().GetString(nameId, out var name, Handler.GetNetworkManager()) ? Entity.Entity.RemoveTitleAndShardFromName(name).ToLower() : string.Empty);
            mutex.ReleaseMutex();

            Handler.GetLogger().Info($"Added {(_friendNames[nameId] != string.Empty ? _friendNames[nameId] : $"{nameId}")} to the friend list.");
        }

        /// <inheritdoc />
        public override void OnChat(in uint compressedSenderIndex, string ucstr, string rawMessage, ChatGroupType mode, in uint dynChatId, string senderName, in uint bubbleTimer)
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

                        if (whoName.Trim() != string.Empty && !_friendNames.ContainsValue(whoName) && !_namesToAdd.Contains(whoName))
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

            // Try to add players from the chat
            var name = Entity.Entity.RemoveTitleAndShardFromName(senderName).ToLower();

            if (name.StartsWith("~"))
                name = name[1..];

            if (name.Trim().Equals(string.Empty))
                return;

            if (name.ToLower().Equals(Entity.Entity.RemoveTitleAndShardFromName(Handler.GetNetworkManager().PlayerSelectedHomeShardName).ToLower()))
                return;

            if (_friendNames.ContainsValue(name)) return;

            Handler.GetLogger().Info($"{name} will be added to the friends list.");
            new AddFriend().Run(Handler, "addfriend " + name, null);
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

                    online.Sort();

                    var ret = $"There are {online.Count}/{_friendOnline.Count} players online:\r\n";
                    ret += string.Join(", ", online);

                    return ret;

                case "importfriends" when args.Length != 1:
                    Handler.GetLogger()?.Error("Please specify a file for the players to import.");
                    return "";

                case "importfriends":
                    var pathR = args[0];

                    if (!File.Exists(pathR))
                    {
                        Handler.GetLogger()?.Error("File does not exist.");
                        return "";
                    }

                    // Open the file to read from
                    var readText = File.ReadAllLines(pathR);

                    foreach (var name in readText)
                    {
                        if (name.Trim() != string.Empty && !_friendNames.ContainsValue(name) && !_namesToAdd.Contains(name))
                            _namesToAdd.Enqueue(name);
                    }

                    return "";

                case "exportfriends" when args.Length != 1:
                    Handler.GetLogger()?.Error("Please specify a file for the players to export.");
                    return "";

                case "exportfriends":
                    var pathW = args[0];

                    if (_friendNames.ContainsValue(string.Empty))
                    {
                        Handler.GetLogger()?.Error("There are unloaded playernames. Please wait before all names are known by the client.");
                        return "";
                    }

                    var writeText = _friendNames.Values.Aggregate("", (current, name) => current + (name + "\r\n"));

                    // Write names to the file
                    File.WriteAllText(pathW, writeText);

                    return "";

                case "sendapi":
                    Handler.GetLogger()?.Info("Sending Player Array to API");
                    _lastApiServerUpdate = DateTime.Now + _intervalApiServer;

                    Task.Factory.StartNew(SendApiUpdate);
                    return "";

                default:
                    Handler.GetLogger()?.Warn("CommandBase unknown: " + cmd);
                    return "";
            }
        }

        /// <summary>
        /// Sends a list of online players to the API
        /// </summary>
        public void SendApiUpdate()
        {
            // there are friends that have not received a name
            if (_friendNames.Keys.Any(id => _friendNames[id] == string.Empty))
                return;

            try
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(ClientConfig.OnlinePlayersApi);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";
                httpWebRequest.AllowWriteStreamBuffering = false;

                var hash = Misc.GetMD5(DateTime.Now.ToString("ddMMyyyy")).ToLower();

                var json = $"[{{\"auth\":\"{hash}\",\"players\":[";

                foreach (var id in _friendNames.Keys.Where(id => _friendNames[id] != string.Empty))
                {
                    json += $"{{\"name\":\"{_friendNames[id]}\",\"status\":\"{(_friendOnline[id] == CharConnectionState.CcsOnline ? "1" : "0")}\"}},";
                }

                json = json[..^1];

                json += $"]}}]";

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    streamWriter.Write(json);
                }

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                using var streamReader = new StreamReader(httpResponse.GetResponseStream() ?? throw new NullReferenceException());

                var response = streamReader.ReadToEnd().Trim();

                if (!int.TryParse(response, out var result) || result != 1)
                {
                    Handler.GetLogger().Error("Error API responded with: " + response);
                }
            }
            catch (Exception e)
            {
                Handler.GetLogger().Error("Error while posting to the players API: " + e.Message);
            }
        }
    }
}