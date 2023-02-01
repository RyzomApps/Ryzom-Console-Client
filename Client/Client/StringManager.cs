///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using API.Client;
using API.Entity;
using API.Network;
using Client.Network;
using Client.Sheet;
using Client.Stream;
using static Client.Client.DynamicStringInfo;

namespace Client.Client
{
    /// <summary>
    /// Management for dynamically generated text from servers
    /// </summary>
    public class StringManager : IStringManager
    {
        private readonly Dictionary<uint, string> _receivedStrings = new Dictionary<uint, string>();
        private readonly HashSet<uint> _waitingStrings = new HashSet<uint>();

        private readonly Dictionary<uint, DynamicStringInfo> _receivedDynStrings = new Dictionary<uint, DynamicStringInfo>();

        private readonly Dictionary<uint, DynamicStringInfo> _waitingDynStrings = new Dictionary<uint, DynamicStringInfo>();

        /// <summary>
        /// String waiting the string value from the server.
        /// </summary>
        private readonly Dictionary<uint, StringWaiter> _stringsWaiters = new Dictionary<uint, StringWaiter>();

        /// <summary>
        /// String waiting the dynamic string value from the server.
        /// </summary>
        private readonly Dictionary<uint, StringWaiter> _dynStringsWaiters = new Dictionary<uint, StringWaiter>();

        /// <summary>
        /// Callback for string value from the server
        /// </summary>
        private readonly Dictionary<uint, StringWaitCallback> _stringsCallbacks = new Dictionary<uint, StringWaitCallback>();

        /// <summary>
        /// Callback for string value from the server as Actions
        /// </summary>
        private readonly Dictionary<uint, Action<uint, string>> _stringsActions = new Dictionary<uint, Action<uint, string>>();

        /// <summary>
        /// Callback for dynamic string value from the server
        /// </summary>
        private readonly Dictionary<uint, StringWaitCallback> _dynStringsCallbacks = new Dictionary<uint, StringWaitCallback>();

        SortedDictionary<string, SpecialWord> _SpecItem_TempMap;

        private readonly Dictionary<string, string> _dynStrings = new Dictionary<string, string>();

        private readonly List<CachedString> _cacheStringToSave = new List<CachedString>();

        private string _shardId;
        private string _languageCode;
        private bool _cacheInited;
        private string _cacheFilename;
        private uint _timestamp;

        private readonly RyzomClient _client;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="client"></param>
        public StringManager(RyzomClient client)
        {
            _client = client;
        }
        /// <summary>
        /// Prepare the string manager to use a persistent string cache.
        /// There is one cache file for each language and for each encountered shard.
        /// </summary>
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

        /// <summary>
        /// Clear the current string table and load the content of the cache file.
	    /// This method is called after receiving the impulse RELOAD_CACHE from IOS.
	    /// If the received timestamp and the file timestamp differ, the file cache is reseted.
        /// </summary>
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
                _receivedStrings.Clear();
                _receivedDynStrings.Clear();

                // NB : we keep the waiting strings and dyn strings

                // insert the empty string.
                _receivedStrings.Add(0, "");

                // load the cache file
                using var fileStream2 = new FileStream(_cacheFilename, FileMode.Open);

                var timeBytes2 = new byte[4];

                fileStream2.Read(timeBytes2, 0, 4);

                _timestamp = BitConverter.ToUInt32(timeBytes2);

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

                    _client.GetLogger().Debug($"SM : loading string [{id}] as [{str}] in cache");

                    if (!_receivedStrings.ContainsKey(id))
                        _receivedStrings.Add(id, str);
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
        /// Force the cache to be saved
        /// </summary>
        public void FlushStringCache()
        {
            if (_cacheStringToSave.Count <= 0) return;

            using var fileStream = new FileStream(_cacheFilename, FileMode.Append);

            foreach (var cacheString in _cacheStringToSave)
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

            _cacheStringToSave.Clear();
        }

        /// <summary>
        /// extract the dynamic string from the stream and check if it is complete
        /// </summary>
        public void ReceiveDynString(BitMemoryStream bms, NetworkManager networkManager)
        {
            var dynInfo = new DynamicStringInfo { Status = StringStatus.Received };

            // read the dynamic string Id
            uint dynId = 0;
            bms.Serial(ref dynId);

            // read the base string Id
            uint stringId = 0;
            bms.Serial(ref stringId);
            dynInfo.StringId = stringId;

            // try to build the string
            dynInfo.Message = bms;
            BuildDynString(dynInfo, networkManager);

            if (dynInfo.Status == StringStatus.Complete)
            {
                _client.GetLogger()?.Debug($"DynString {dynId} available : [{dynInfo.String}]");

                if (_receivedDynStrings.ContainsKey(dynId))
                    _receivedDynStrings[dynId] = dynInfo;
                else
                    _receivedDynStrings.Add(dynId, dynInfo);

                // security, if dynstring Message received twice, it is possible that the dynstring is still in waiting list
                _waitingDynStrings.Remove(dynId);

                // update the waiting dyn strings
                if (_dynStringsWaiters.ContainsKey(dynId))
                {
                    _dynStringsWaiters[dynId].Result = dynInfo.String;
                    _dynStringsWaiters.Remove(dynId);
                }

                // callback the waiting dyn strings
                if (_dynStringsCallbacks.ContainsKey(dynId))
                {
                    _dynStringsCallbacks[dynId].OnDynStringAvailable(dynId, dynInfo.String);
                    _dynStringsCallbacks.Remove(dynId);
                }
            }
            else
                if (!_waitingDynStrings.ContainsKey(dynId)) _waitingDynStrings.Add(dynId, dynInfo);

            // Fire an Event
            _client.Plugins.OnPhraseSend(dynInfo);
        }

        /// <summary>
        /// assemble the dynamic string from DynamicStringInfo
        /// </summary>
        private bool BuildDynString(DynamicStringInfo dynInfo, NetworkManager networkManager)
        {
            if (dynInfo.Status == StringStatus.Received)
            {
                if (!GetString(dynInfo.StringId, out dynInfo.String, ((RyzomClient)_client).GetNetworkManager()))
                {
                    // can't continue now, need the base string.
                    return false;
                }

                // ok, we have the base string, we can serial the parameters
                for (var i = 0; i < dynInfo.String.Length - 1; i++)
                {
                    var character = dynInfo.String[i];

                    if (character != '%') continue;

                    i++;
                    character = dynInfo.String[i];

                    if (character == '%') continue;

                    // we have a replacement point.
                    var param = new ParamValue
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
                                // ignored
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
                                // ignored
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
                                // ignored
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
                                // ignored
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
                                // ignored
                            }

                            break;

                        default:
                            _client.GetLogger().Warn($"Error: unknown replacement tag {character}");
                            return false;
                    }

                    dynInfo.Params.Add(param);
                }

                dynInfo.Status = StringStatus.Serialized;
            }

            switch (dynInfo.Status)
            {
                case StringStatus.Serialized:
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
                                        if (str.Length != 0 && networkManager.PlayerSelectedHomeShardNameWithParenthesis.Length > 0)
                                        {
                                            // fast pre-test
                                            if (str[^1] == ')')
                                            {
                                                // the player name must be at least bigger than the string with ()
                                                if (str.Length > networkManager.PlayerSelectedHomeShardNameWithParenthesis.Length)
                                                {
                                                    // If the shard name is the same as the player home shard name, remove it
                                                    var len = networkManager.PlayerSelectedHomeShardNameWithParenthesis.Length;
                                                    var start = str.Length - len;

                                                    // Unicode case-insensitive compare two same-sized strings
                                                    // compare 2 ucstring s0 and s1, without regard to case. give start and size for sequence p0 // OLD
                                                    // const ucstring &s0, uint p0, uint n0, const ucstring &s1
                                                    // return -1 if s1>s0, 1 if s0>s1, or 0 if equals
                                                    if (string.Compare(str.Substring(start, len), networkManager.PlayerSelectedHomeShardNameWithParenthesis, StringComparison.OrdinalIgnoreCase) == 0)
                                                        str = str[start..];
                                                }
                                            }
                                        }

                                        // If the string contains a title, then remove it
                                        var pos = str.IndexOf('$');

                                        if (str.Length > 0 && pos != -1)
                                        {
                                            str = EntityHelper.RemoveTitleFromName(str);
                                        }

                                        // if the string contains a special rename of creature, remove it
                                        if (str.Length > 2 && str[0] == '<' && str[1] == '#')
                                        {
                                            str = char.ToUpper(str[2]) + str[3..];
                                        }

                                        // append this string
                                        temp.Append(dynInfo.String[move..param.ReplacementPoint]);
                                        temp.Append(str);
                                        move = param.ReplacementPoint + 2;
                                    }
                                    break;

                                case ParamType.Integer:
                                    {
                                        var str = $"{param.Integer}";
                                        temp.Append(dynInfo.String[move..param.ReplacementPoint]);
                                        temp.Append(str);
                                        move = param.ReplacementPoint + 2;
                                    }
                                    break;

                                case ParamType.Time:
                                    {
                                        var str = $"{param.Time}";
                                        temp.Append(dynInfo.String[move..param.ReplacementPoint]);
                                        temp.Append(str);
                                        move = param.ReplacementPoint + 2;
                                    }
                                    break;

                                case ParamType.Money:
                                    {
                                        var str = $"{param.Money}";
                                        temp.Append(dynInfo.String[move..param.ReplacementPoint]);
                                        temp.Append(str);
                                        move = param.ReplacementPoint + 2;
                                    }
                                    break;

                                case ParamType.DynStringID:
                                    {
                                        if (!GetDynString(param.DynStringId, out string dynStr, networkManager))
                                            return false;

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
                        temp = temp.Replace("" + (char)8, "");
                        temp = temp.Replace("%%", "");

                        dynInfo.Status = StringStatus.Complete;
                        dynInfo.Message = null;
                        dynInfo.String = temp.ToString();
                        return true;
                    }

                case StringStatus.Complete:
                    return true;

                default:
                    _client.GetLogger()?.Warn($"Inconsistent dyn string status : {dynInfo.Status}");
                    return false;
            }
        }

        public bool GetDynString(uint dynStringId, out string result, NetworkManager networkManager)
        {
            result = "";

            if (dynStringId == 0)
                return true;

            if (_receivedDynStrings.ContainsKey(dynStringId))
            {
                // ok, we have the string with all the parts.
                result = _receivedDynStrings[dynStringId].String;

                // security/antiloop checking
                if (!_waitingDynStrings.ContainsKey(dynStringId)) return true;

                _client.GetLogger()?.Warn(
                    $"CStringManager::getDynString : the string {dynStringId} is received but still in _WaintingDynStrings !");
                _waitingDynStrings.Remove(dynStringId);

                return true;
            }

            // check to see if the string is available now.
            if (!_waitingDynStrings.ContainsKey(dynStringId))
            {
                result = "";
                return false;
            }

            if (BuildDynString(_waitingDynStrings[dynStringId], networkManager))
            {
                result = _waitingDynStrings[dynStringId].String;
                _receivedDynStrings.Add(dynStringId, _waitingDynStrings[dynStringId]);
                _waitingDynStrings.Remove(dynStringId);

                return true;
            }

            result = "";

            return false;
        }

        /// <inheritdoc cref="GetString(uint, out string, NetworkManager)"/>
        public bool GetString(uint stringId, out string result, INetworkManager networkManager)
        {
            if (networkManager is NetworkManager manager)
                return GetString(stringId, out result, manager);

            throw new NotImplementedException("GetString can only be executed with a Networkmanager.");
        }

        /// <summary>
        /// request the stringId from the local cache or if missing ask the server
        /// </summary>
        public bool GetString(uint stringId, out string result, NetworkManager networkManager)
        {
            if (!_receivedStrings.ContainsKey(stringId))
            {
                if (!_waitingStrings.Contains(stringId))
                {
                    _waitingStrings.Add(stringId);

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

                result = string.Empty;

                return false;
            }

            result = _receivedStrings[stringId];

            if (result.Length <= 9 || result.Substring(0, 9) != "<missing:") return true;

            if (_dynStrings.ContainsKey(result[9..^1]))
            {
                result = _dynStrings[result[9..^1]];
            }

            return true;
        }

        internal void ReceiveString(uint stringId, string str, NetworkManager networkManager)
        {
            if (_waitingStrings.Contains(stringId))
            {
                _waitingStrings.Remove(stringId);
            }

            var updateCache = true;

            if (_receivedStrings.ContainsKey(stringId))
            {
                _client.GetLogger().Warn($"Receiving stringID {stringId} ({str}), already in received string ({_receivedStrings[stringId]}), replacing with new value.");

                if (_receivedStrings[stringId] != str)
                    _receivedStrings[stringId] = str;
                else
                    updateCache = false;
            }
            else
            {
                _receivedStrings.Add(stringId, str);
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
                    _cacheStringToSave.Add(cs);
                }
            }

            // update the waiting strings
            if (_stringsWaiters.ContainsKey(stringId))
            {
                _stringsWaiters[stringId].Result = str;
                _stringsWaiters.Remove(stringId);
            }

            // callback the waiter
            if (_stringsCallbacks.ContainsKey(stringId))
            {
                _stringsCallbacks[stringId].OnStringAvailable(stringId, str);
                _stringsCallbacks.Remove(stringId);
            }

            // callback the waiter
            if (_stringsActions.ContainsKey(stringId))
            {
                _stringsActions[stringId](stringId, str);
                _stringsActions.Remove(stringId);
            }

            // TODO: try to complete any pending dyn string
            var restartLoop = false;

            while (restartLoop)
            {
                foreach (var dynString in _waitingDynStrings)
                {
                    /// Warning: if getDynString() return true, 'first' is erased => don't use it after in this loop
                    if (GetDynString(dynString.Key, out string value, networkManager))
                    {
                        var number = dynString.Key;

                        _client.GetLogger().Info($"DynString {number} available : [{value}]");

                        // this dyn string is now complete !
                        // update the waiting dyn strings
                        _dynStringsWaiters[number].Result = str; // is that correct?
                        _dynStringsWaiters.Remove(number);

                        // callback the waiting dyn strings
                        _dynStringsCallbacks[number].OnDynStringAvailable(number, value);
                        _dynStringsCallbacks.Remove(number);

                        restartLoop = true;
                    }
                }
            }
        }

        /// <summary>
        /// Wait for a string or fire the action if the string is already present
        /// </summary>
        public void WaitString(in uint stringId, Action<uint, string> pcallback, NetworkManager networkManager)
        {
            if (GetString(stringId, out var value, networkManager))
            {
                pcallback(stringId, value);
            }
            else
            {
                // wait for the string
                if (_stringsCallbacks.ContainsKey(stringId))
                {
                    _stringsActions[stringId] = pcallback;
                }
                else
                {
                    _stringsActions.Add(stringId, pcallback);
                }
            }
        }

        /// <summary>
        /// Wait for a string or fire the action if the string is already present
        /// </summary>
        public void WaitString(in uint stringId, StringWaitCallback pcallback, NetworkManager networkManager)
        {
            if (GetString(stringId, out var value, networkManager))
            {
                pcallback.OnStringAvailable(stringId, value);
            }
            else
            {
                // wait for the string
                if (_stringsCallbacks.ContainsKey(stringId))
                {
                    _stringsCallbacks[stringId] = pcallback;
                }
                else
                {
                    _stringsCallbacks.Add(stringId, pcallback);
                }
            }
        }

        /// <summary>
        /// Get the Localized Name of the SPhrase.
        /// </summary>
        public string GetSPhraseLocalizedName(SheetId id)
        {
            return GetSpecialWord(id.ToString());
        }

        /// <summary>
        /// Get the Localized name
        /// </summary>
        public string GetLocalizedName(in string uctext)
        {
            var text = uctext;
            var defaultText = "";

            if (text[0] != '[') 
                return uctext;

            var textLocalizations = text[1..].Split('[');

            if (textLocalizations.Length <= 0) 
                return !string.IsNullOrEmpty(defaultText) ? defaultText : uctext;

            for (uint i = 0; i < textLocalizations.Length; i++)
            {
                if (textLocalizations[i].Substring(0, 3) == CultureInfo.CurrentCulture.TwoLetterISOLanguageName + "]")
                {
                    defaultText = textLocalizations[i].Substring(3);
                    return defaultText;
                }

                if (textLocalizations[i].Substring(0, 3) == "wk]")
                {
                    defaultText = textLocalizations[i].Substring(3);
                }
            }

            return !string.IsNullOrEmpty(defaultText) ? defaultText : uctext;
        }


        public string GetSpecialWord(string label, bool women = false)
        {
            if (label.Length == 0)
            {
                return "";
            }

            if (label[0] == '#')
            {
                return GetLocalizedName(label[1..]);
            }

            // avoid case problems
            var lwrLabel = label.ToLower();

            //if (_SpecItem_MemoryCompressed)
            //{
            //    CItemLight tmp = new CItemLight();
            //    tmp.Label = (string)lwrLabel.c_str();
            //    List<CItemLight>.Enumerator it = lower_bound(_SpecItems.begin(), _SpecItems.end(), tmp, CItemLightComp());
            //
            //    if (it != _SpecItems.end())
            //    {
            //        if (string.Compare(it.Label, lwrLabel.c_str()) == 0)
            //        {
            //            if (UseFemaleTitles && women)
            //            {
            //                if (!it.WomenName[0])
            //                {
            //                    return it.WomenName;
            //                }
            //            }
            //            return it.Name;
            //        }
            //    }
            //}
            //else
            //{
            //    SortedDictionary<string,CItem>.Enumerator it = _SpecItem_TempMap.find(lwrLabel);
            //
            //    if (it != _SpecItem_TempMap.end())
            //    {
            //        if (UseFemaleTitles && women)
            //        {
            //            if (!it.second.WomenName.empty())
            //            {
            //                return it.second.WomenName.c_str();
            //            }
            //        }
            //
            //        return it.second.Name.c_str();
            //    }
            //}

            var badString = "<NotExist:" + lwrLabel + ">";
            return badString;
        }
    }
}