///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using RCC.Network;
using static RCC.Client.DynamicStringInfo;

namespace RCC.Client
{
    /// <summary>
    ///     Management for dynamically generated text from servers
    /// </summary>
    public class StringManager
    {
        private readonly Dictionary<uint, string> ReceivedStrings = new Dictionary<uint, string>();
        private readonly HashSet<uint> WaitingStrings = new HashSet<uint>();

        private readonly Dictionary<uint, DynamicStringInfo> ReceivedDynStrings =
            new Dictionary<uint, DynamicStringInfo>();

        private readonly Dictionary<uint, DynamicStringInfo> WaitingDynStrings =
            new Dictionary<uint, DynamicStringInfo>();

        /// <summary>
        ///     String waiting the string value from the server.
        /// </summary>
        private readonly Dictionary<uint, StringWaiter> StringsWaiters = new Dictionary<uint, StringWaiter>();

        /// <summary>
        ///     String waiting the dyn string value from the server.
        /// </summary>
        private readonly Dictionary<uint, StringWaiter> DynStringsWaiters = new Dictionary<uint, StringWaiter>();

        /// <summary>
        ///     Callback for string value from the server
        /// </summary>
        private readonly Dictionary<uint, StringWaitCallback> StringsCallbacks =
            new Dictionary<uint, StringWaitCallback>();

        /// <summary>
        ///     Callback for dyn string value from the server
        /// </summary>
        private readonly Dictionary<uint, StringWaitCallback> DynStringsCallbacks =
            new Dictionary<uint, StringWaitCallback>();

        private readonly Dictionary<string, string> DynStrings = new Dictionary<string, string>();

        private readonly List<CachedString> CacheStringToSave = new List<CachedString>();

        private string _shardId;
        private string _languageCode;
        private bool _cacheInited;
        private string _cacheFilename;
        private uint _timestamp;

        private readonly RyzomClient _client;

        public StringManager(RyzomClient client)
        {
            _client = client;
        }

        public void InitCache(string shardId, string languageCode)
        {
            _shardId = shardId;
            _languageCode = languageCode;

            // to be inited, shard id and language code must be filled
            if (_shardId != string.Empty && _languageCode != string.Empty)
                _cacheInited = true;
            else
                _cacheInited = false;
        }

        public void LoadCache(in int timestamp)
        {
            if (!_cacheInited) return;

            try
            {
                _cacheFilename = "save/" + _shardId.Split(":")[0] + ".string_cache";

                _client.GetLogger().Info($"Loading string cache from {Path.GetFullPath(_cacheFilename)}");

                if (File.Exists(_cacheFilename))
                {
                    // there is a cache file, check date reset it if needed
                    using (var fileStream = new FileStream(_cacheFilename, FileMode.Open))
                    {
                        var timeBytes = new byte[4];

                        fileStream.Read(timeBytes, 0, 4);

                        _timestamp = BitConverter.ToUInt32(timeBytes);
                    }

                    if (_timestamp != timestamp)
                    {
                        _client.GetLogger().Debug("SM: Clearing string cache : outofdate");

                        // the cache is not sync, reset it TODO this is not working correctly
                        using var fileStream = new FileStream(_cacheFilename, FileMode.Create);

                        var timeBytes = BitConverter.GetBytes(timestamp);

                        fileStream.Write(timeBytes, 0, 4);
                    }
                    else
                    {
                        _client.GetLogger().Debug("SM : string cache in sync. cool");
                    }
                }
                else
                {
                    _client.GetLogger().Debug("SM: Creating string cache");

                    // cache file don't exist, create it with the timestamp
                    using var fileStream = new FileStream(_cacheFilename, FileMode.Create);

                    var timeBytes = BitConverter.GetBytes(timestamp);

                    fileStream.Write(timeBytes, 0, 4);
                }

                // clear all current data.
                ReceivedStrings.Clear();
                ReceivedDynStrings.Clear();
                // NB : we keep the waiting strings and dyn strings

                // insert the empty string.
                ReceivedStrings.Add(0, "");

                // load the cache file
                using var fileStream2 = new FileStream(_cacheFilename, FileMode.Open);

                var timeBytes2 = new byte[4];

                fileStream2.Read(timeBytes2, 0, 4);

                _timestamp = BitConverter.ToUInt32(timeBytes2);
                //Debug.Assert(_Timestamp == timestamp);

                while (fileStream2.Position < fileStream2.Length)
                {
                    var idBytes = new byte[4];

                    fileStream2.Read(idBytes);

                    var lenBytes = new byte[4];

                    fileStream2.Read(lenBytes);

                    var len = BitConverter.ToUInt32(lenBytes);

                    var strBytes = new byte[len * 2];

                    fileStream2.Read(strBytes);

                    var id = BitConverter.ToUInt32(idBytes);
                    var str = Encoding.UTF8.GetString(strBytes).Replace("\0", "");

                    //_client.GetLogger().Debug($"SM : loading string [{id}] as [{str}] in cache");

                    if (!ReceivedStrings.ContainsKey(id))
                        ReceivedStrings.Add(id, str);
                }
            }
            catch (Exception e)
            {
                _client.GetLogger().Warn($"SM : loadCache failed, cache deactivated, exception : {e.GetType().Name} {e.Message}");

                // deactivated cache.
                _cacheFilename = "";
            }
        }

        /// <summary>
        ///     flush the server string cache
        /// </summary>
        public void FlushStringCache()
        {
            if (CacheStringToSave.Count <= 0) return;

            using var fileStream = new FileStream(_cacheFilename, FileMode.Append);

            foreach (var cacheString in CacheStringToSave)
            {
                var idBytes = BitConverter.GetBytes(cacheString.StringId);
                var lenBytes = BitConverter.GetBytes(cacheString.String.Length);
                var strChars = cacheString.String.ToCharArray();

                fileStream.Write(idBytes);
                fileStream.Write(lenBytes);

                foreach (var c in strChars)
                {
                    var bytes = BitConverter.GetBytes(c);
                    fileStream.Write(bytes);
                }
            }

            CacheStringToSave.Clear();
        }

        /// <summary>
        ///     extract the dynamic string from the stream and check if it is complete
        /// </summary>
        public void ReceiveDynString(BitMemoryStream bms, NetworkManager _networkManager)
        {
            var dynInfo = new DynamicStringInfo { Status = StringStatus.Received };

            // read the dynamic string Id
            uint dynId = 0;
            bms.Serial(ref dynId);

            // read the base string Id
            uint stringId = 0;
            bms.Serial( /*dynInfo.*/ref stringId);
            dynInfo.StringId = stringId;

            // try to build the string
            dynInfo.Message = bms;
            BuildDynString(dynInfo, _networkManager);

            if (dynInfo.Status == StringStatus.Complete)
            {
                _client.GetLogger()?.Debug($"DynString {dynId} available : [{dynInfo.String}]");

                if (ReceivedDynStrings.ContainsKey(dynId))
                    ReceivedDynStrings[dynId] = dynInfo;
                else
                    ReceivedDynStrings.Add(dynId, dynInfo);

                // security, if dynstring Message received twice, it is possible that the dynstring is still in waiting list
                WaitingDynStrings.Remove(dynId);

                // update the waiting dyn strings
                if (DynStringsWaiters.ContainsKey(dynId))
                {
                    DynStringsWaiters[dynId].Result = dynInfo.String;
                    DynStringsWaiters.Remove(dynId);
                }

                // callback the waiting dyn strings
                if (DynStringsCallbacks.ContainsKey(dynId))
                {
                    DynStringsCallbacks[dynId].OnDynStringAvailable(dynId, dynInfo.String);
                    DynStringsCallbacks.Remove(dynId);
                }
            }
            else
                if(!WaitingDynStrings.ContainsKey(dynId)) WaitingDynStrings.Add(dynId, dynInfo);

            // Fire an Event
            _client.Automata.OnPhraseSend(dynInfo);
        }

        /// <summary>
        ///     assemble the dynamic string from DynamicStringInfo
        /// </summary>
        private bool BuildDynString(DynamicStringInfo dynInfo, NetworkManager _networkManager)
        {
            if (dynInfo.Status == StringStatus.Received)
            {
                if (!GetString(dynInfo.StringId, out dynInfo.String, ((RyzomClient)_client).GetNetworkManager()))
                {
                    // can't continue now, need the base string.
                    return false;
                }

                // ok, we have the base string, we can serial the parameters
                for (int i = 0; i < dynInfo.String.Length - 1; i++)
                {
                    var character = dynInfo.String[i];

                    if (character == '%')
                    {
                        i++;
                        character = dynInfo.String[i];

                        if (character != '%')
                        {
                            // we have a replacement point.
                            ParamValue param = new ParamValue
                            {
                                ReplacementPoint = i - 1 //(character - 1) - dynInfo.String.begin();
                            };

                            switch (character)
                            {
                                case 's':
                                    param.Type = ParamType.StringID;
                                    try
                                    {
                                        param.StringId = 0;
                                        dynInfo.Message.Serial(ref param.StringId);
                                    }
                                    catch (Exception)
                                    {
                                    }

                                    break;

                                case 'i':
                                    param.Type = ParamType.Integer;
                                    try
                                    {
                                        param.Integer = 0;
                                        dynInfo.Message.Serial(ref param.Integer);
                                    }
                                    catch (Exception)
                                    {
                                    }

                                    break;

                                case 't':
                                    param.Type = ParamType.Time;
                                    try
                                    {
                                        param.Time = 0;
                                        dynInfo.Message.Serial(ref param.Time);
                                    }
                                    catch (Exception)
                                    {
                                    }

                                    break;

                                case '$':
                                    param.Type = ParamType.Money;
                                    try
                                    {
                                        param.Money = 0;
                                        byte[] moneyArr = new byte[64];
                                        dynInfo.Message.Serial(ref moneyArr);
                                        param.Money = BitConverter.ToUInt64(moneyArr);
                                    }
                                    catch (Exception)
                                    {
                                    }

                                    break;

                                case 'm':
                                    param.Type = ParamType.DynStringID;
                                    try
                                    {
                                        param.DynStringId = 0;
                                        dynInfo.Message.Serial(ref param.DynStringId);
                                    }
                                    catch (Exception)
                                    {
                                    }

                                    break;

                                default:
                                    _client.GetLogger().Warn("Error: unknown replacement tag %%%c", (char)character);
                                    return false;
                            }

                            dynInfo.Params.Add(param);
                        }
                    }
                }

                dynInfo.Status = StringStatus.Serialized;
            }

            if (dynInfo.Status == StringStatus.Serialized)
            {
                // try to retreive all string parameter to build the string.
                var temp = new StringBuilder();

                var move = 0;

                foreach (var param in dynInfo.Params)
                {
                    switch (param.Type)
                    {
                        case ParamType.StringID:
                            {
                                if (!GetString(param.StringId, out string str, ((RyzomClient)_client).GetNetworkManager()))
                                    return false;

                                var p1 = str.IndexOf('[');

                                if (p1 != -1)
                                {
                                    // TODO GetLocalizedName 
                                    //str = str.Substring(0, p1) + StringManager.GetLocalizedName(str.Substring(p1));
                                }

                                // If the string is a player name, we may have to remove the shard name (if the string looks like a player name)
                                if (str.Length != 0 && _networkManager.PlayerSelectedHomeShardNameWithParenthesis.Length > 0)
                                {
                                    // fast pre-test
                                    if (str[^1] == ')')
                                    {
                                        // the player name must be at least bigger than the string with ()
                                        if (str.Length > _networkManager.PlayerSelectedHomeShardNameWithParenthesis.Length)
                                        {
                                            // If the shard name is the same as the player home shard name, remove it
                                            uint len = (uint)_networkManager.PlayerSelectedHomeShardNameWithParenthesis.Length;
                                            uint start = (uint)str.Length - len;

                                            // TODO remove the shard from player name
                                            //if (ucstrnicmp(str, start, len, _networkManager.PlayerSelectedHomeShardNameWithParenthesis) == 0)
                                            //	str.resize(start);
                                        }
                                    }
                                }

                                // If the string contains a title, then remove it
                                var pos = str.IndexOf('$');

                                if (str.Length > 0 && pos != -1)
                                {
                                    str = Entity.RemoveTitleFromName(str);
                                }

                                // if the string contains a special rename of creature, remove it
                                if (str.Length > 2 && str[0] == '<' && str[1] == '#')
                                {
                                    str = char.ToUpper(str[2]) + str.Substring(3);
                                }

                                // append this string
                                temp.Append(dynInfo.String[move..param.ReplacementPoint]);
                                temp.Append(str);
                                move = param.ReplacementPoint + 2;
                            }
                            break;

                        case ParamType.Integer:
                            {
                                //char value[1024];
                                //sprintf(value, "%d", param.Integer);
                                //temp.append(move, src + param.ReplacementPoint);
                                //temp += string(value);
                                //move = dynInfo.String.begin() + param.ReplacementPoint + 2;
                                var str = $"{param.Integer}";
                                temp.Append(dynInfo.String[move..param.ReplacementPoint]);
                                temp.Append(str);
                                move = param.ReplacementPoint + 2;
                            }
                            break;

                        case ParamType.Time:
                            {
                                //string value;
                                //uint time = (uint)param.Time;
                                //if (time >= (10 * 60 * 60))
                                //{
                                //	uint nbHours = time / (10 * 60 * 60);
                                //	time -= nbHours * 10 * 60 * 60;
                                //	value = toString("%d ", nbHours) + CI18N.get("uiMissionTimerHour") + " ";
                                //
                                //	uint nbMinutes = time / (10 * 60);
                                //	time -= nbMinutes * 10 * 60;
                                //	value = value + toString("%d ", nbMinutes) + CI18N.get("uiMissionTimerMinute") + " ";
                                //}
                                //else if (time >= (10 * 60))
                                //{
                                //	uint nbMinutes = time / (10 * 60);
                                //	time -= nbMinutes * 10 * 60;
                                //	value = value + toString("%d ", nbMinutes) + CI18N.get("uiMissionTimerMinute") + " ";
                                //}
                                //uint nbSeconds = time / 10;
                                //value = value + toString("%d", nbSeconds) + CI18N.get("uiMissionTimerSecond");
                                //temp.append(move, src + param.ReplacementPoint);
                                //temp += value;
                                //move = dynInfo.String.begin() + param.ReplacementPoint + 2;
                                var str = $"{param.Time}";
                                temp.Append(dynInfo.String[move..param.ReplacementPoint]);
                                temp.Append(str);
                                move = param.ReplacementPoint + 2;
                            }
                            break;

                        case ParamType.Money:
                            ///\todo nicoB/Boris : this is a temp patch that display money as integers
                            {
                                //char value[1024];
                                //sprintf(value, "%u", (uint)param.Money);
                                //temp.append(move, src + param.ReplacementPoint);
                                //temp += string(value);
                                //move = dynInfo.String.begin() + param.ReplacementPoint + 2;
                                var str = $"{param.Money}";
                                temp.Append(dynInfo.String[move..param.ReplacementPoint]);
                                temp.Append(str);
                                move = param.ReplacementPoint + 2;
                            }
                            // TODO (from ryzom code)
                            //					temp.append(move, src+param.ReplacementPoint);
                            //					move = dynInfo.String.begin()+param.ReplacementPoint+2;
                            break;

                        case ParamType.DynStringID:
                            {
                                if (!GetDynString(param.DynStringId, out string dynStr, _networkManager))
                                    return false;
                                //temp.append(move, src + param.ReplacementPoint);
                                //temp += dynStr;
                                //move = dynInfo.String.begin() + param.ReplacementPoint + 2;

                                var str = $"{dynStr}";
                                temp.Append(dynInfo.String[move..param.ReplacementPoint]);
                                temp.Append(str);
                                move = param.ReplacementPoint + 2;
                            }
                            break;

                        default:
                            _client.GetLogger().Warn("Unknown parameter type.");
                            break;
                    }
                }

                // append the rest of the string
                temp.Append(dynInfo.String[move..]);

                // apply any 'delete' character in the string and replace double '%'
                //{
                //	uint i = 0;
                //	while (i < temp.size())
                //	{
                //		if (temp[i] == 8)
                //		{
                //			// remove the 'delete' char AND the next char
                //			temp.erase(i, 2);
                //		}
                //		else if (temp[i] == '%' && i < temp.size() - 1 && temp[i + 1] == '%')
                //		{
                //			temp.erase(i, 1);
                //		}
                //		else
                //			++i;
                //	}
                //}

                dynInfo.Status = StringStatus.Complete;
                dynInfo.Message = null;
                dynInfo.String = temp.ToString();
                return true;
            }

            if (dynInfo.Status == StringStatus.Complete)
                return true;

            _client.GetLogger()?.Warn($"Inconsistent dyn string status : {dynInfo.Status}");
            return false;
        }

        public bool GetDynString(uint dynStringId, out string result, NetworkManager _networkManager)
        {
            result = "";

            if (dynStringId == 0)
                return true;

            if (ReceivedDynStrings.ContainsKey(dynStringId))
            {
                // ok, we have the string with all the parts.
                result = ReceivedDynStrings[dynStringId].String;

                // security/antiloop checking
                if (WaitingDynStrings.ContainsKey(dynStringId))
                {
                    _client.GetLogger()?.Warn(
                        $"CStringManager::getDynString : the string {dynStringId} is received but still in _WaintingDynStrings !");
                    WaitingDynStrings.Remove(dynStringId);
                }

                return true;
            }
            else
            {
                // check to see if the string is available now.
                if (!WaitingDynStrings.ContainsKey(dynStringId))
                {
                    result = "";
                    return false;
                }

                if (BuildDynString(WaitingDynStrings[dynStringId], _networkManager))
                {
                    result = WaitingDynStrings[dynStringId].String;
                    ReceivedDynStrings.Add(dynStringId, WaitingDynStrings[dynStringId]);
                    WaitingDynStrings.Remove(dynStringId);

                    return true;
                }

                result = "";

                return false;
            }
        }

        /// <summary>
        ///     request the stringId from the local cache or if missing ask the server
        /// </summary>
        public bool GetString(uint stringId, out string result, NetworkManager networkManager)
        {
            //if (stringId == 93402 || stringId == 93403) // TODO: why do this ids lead to a disconnect?
            //{
            //    result = string.Empty;
            //    return false;
            //}

            if (!ReceivedStrings.ContainsKey(stringId))
            {
                if (!WaitingStrings.Contains(stringId))
                {
                    WaitingStrings.Add(stringId);

                    // need to ask for this string.
                    var bms = new BitMemoryStream();
                    const string msgType = "STRING_MANAGER:STRING_RQ";

                    if (networkManager.GetMessageHeaderManager().PushNameToStream(msgType, bms))
                    {
                        bms.Serial(ref stringId);
                        networkManager.Push(bms);
                        _client.GetLogger()?.Debug(
                            $"<CStringManagerClient.getString> sending 'STRING_MANAGER:STRING_RQ' message to server for stringId {stringId}");
                    }
                    else
                    {
                        _client.GetLogger()?.Warn(
                            "<CStringManagerClient.getString> unknown message name 'STRING_MANAGER:STRING_RQ'");
                    }
                }

                // result.erase(); // = _WaitString;
                result = string.Empty;

                return false;
            }

            result = ReceivedStrings[stringId];

            if (result.Length <= 9 || result.Substring(0, 9) != "<missing:") return true;

            if (DynStrings.ContainsKey(result[9..^1]))
            {
                result = DynStrings[result[9..^1]];
            }

            return true;
        }

        internal void ReceiveString(uint stringId, string str, NetworkManager _networkManager)
        {
            //H_AUTO(CStringManagerClient_receiveString)

            if (WaitingStrings.Contains(stringId))
            {
                WaitingStrings.Remove(stringId);
            }

            var updateCache = true;

            if (ReceivedStrings.ContainsKey(stringId))
            {
                _client.GetLogger().Warn(
                    $"Receiving stringID {stringId} ({str}), already in received string ({ReceivedStrings[stringId]}), replacing with new value.");

                if (ReceivedStrings[stringId] != str)
                    ReceivedStrings[stringId] = str;
                else
                    updateCache = false;
            }
            else
            {
                ReceivedStrings.Add(stringId, str);
            }

            if (updateCache)
            {
                // update the string cache. DON'T SAVE now cause
                if (_cacheInited && _cacheFilename.Length > 0)
                {
                    var cs = new CachedString
                    {
                        StringId = stringId,
                        String = str
                    };
                    CacheStringToSave.Add(cs);
                }
            }

            // update the waiting strings
            if (StringsWaiters.ContainsKey(stringId))
            {
                StringsWaiters[stringId].Result = str;
                StringsWaiters.Remove(stringId);
            }

            // callback the waiter
            if (StringsCallbacks.ContainsKey(stringId))
            {
                StringsCallbacks[stringId].OnStringAvailable(stringId, str);
                StringsCallbacks.Remove(stringId);
            }

            // todo: try to complete any pending dyn string
            bool restartLoop = false;

            while (restartLoop)
            {
                foreach (var dynString in WaitingDynStrings)
                {
                    /// Warning: if getDynString() return true, 'first' is erased => don't use it after in this loop
                    if (GetDynString(dynString.Key, out string value, _networkManager))
                    {
                        var number = dynString.Key;

                        _client.GetLogger().Info($"DynString {number} available : [{value}]");

                        // this dyn string is now complete !
                        // update the waiting dyn strings
                        DynStringsWaiters[number].Result = str; // is that correct?
                        DynStringsWaiters.Remove(number);

                        // callback the waiting dyn strings
                        DynStringsCallbacks[number].OnDynStringAvailable(number, value);
                        DynStringsCallbacks.Remove(number);

                        restartLoop = true;
                    }
                }
            }
        }
    }
}