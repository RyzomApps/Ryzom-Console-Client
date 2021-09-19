using System;
using RCC.Messages;
using RCC.Network;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static RCC.Client.DynamicStringInfo;
using System.Linq;

namespace RCC.Client
{
    public class StringWaiter
    {
        /// Pointer to the ucstring to fill
        public string Result;
        /// Pointer to the remover that contains this string reference
        public object Remover;
    };

    public class CacheString
    {
        public uint StringId;
        public string String;
    };

    /// <summary>
    /// Implement this class if you want to wait for
    /// string to be delivered.
    /// </summary>
    public abstract class StringWaitCallback
    {
        /// Overide this method to receive callback for string.
        public abstract void OnStringAvailable(uint stringId, string value);

        /// Overide this method to receive callback for dynamic string.
        public abstract void OnDynStringAvailable(uint stringId, string value);

        ~StringWaitCallback()
        {
            // signal the string manager that this waiter is destroyed
            StringManagerClient.RemoveStringWaiter(this);
        }
    };

    /// <summary>
    ///     Management for dynamically generated text from servers
    /// </summary>
    internal static class StringManagerClient
    {
        static readonly Dictionary<uint, string> ReceivedStrings = new Dictionary<uint, string>();
        private static readonly HashSet<uint> WaitingStrings = new HashSet<uint>();

        static readonly Dictionary<uint, DynamicStringInfo> ReceivedDynStrings = new Dictionary<uint, DynamicStringInfo>();
        static readonly Dictionary<uint, DynamicStringInfo> WaitingDynStrings = new Dictionary<uint, DynamicStringInfo>();

        /// <summary>
        /// String waiting the string value from the server.
        /// </summary>
        static readonly Dictionary<uint, StringWaiter> StringsWaiters = new Dictionary<uint, StringWaiter>();

        /// <summary>
        /// String waiting the dyn string value from the server.
        /// </summary>
        static readonly Dictionary<uint, StringWaiter> DynStringsWaiters = new Dictionary<uint, StringWaiter>();

        /// <summary>
        /// Callback for string value from the server
        /// </summary>
        static readonly Dictionary<uint, StringWaitCallback> StringsCallbacks = new Dictionary<uint, StringWaitCallback>();
        /// <summary>
        /// Callback for dyn string value from the server
        /// </summary>
        static readonly Dictionary<uint, StringWaitCallback> DynStringsCallbacks = new Dictionary<uint, StringWaitCallback>();

        static readonly Dictionary<string, string> DynStrings = new Dictionary<string, string>();

        private static readonly List<CacheString> CacheStringToSave = new List<CacheString>();

        private static string _ShardId;
        private static string _LanguageCode;
        private static bool _CacheInited;
        private static string _CacheFilename;
        private static uint _Timestamp;
        private static bool _CacheLoaded;

        public static void InitCache(string shardId, string languageCode)
        {
            _ShardId = shardId;
            _LanguageCode = languageCode;

            // to be inited, shard id and language code must be filled
            if (_ShardId != string.Empty && _LanguageCode != string.Empty)
                _CacheInited = true;
            else
                _CacheInited = false;
        }

        public static void LoadCache(in int timestamp)
        {
            if (!_CacheInited) return;

            try
            {
                _CacheFilename = "save/" + _ShardId.Split(":")[0] + ".string_cache";

                RyzomClient.Log.Info($"SM : Try to open the string cache : {_CacheFilename}");

                if (File.Exists(_CacheFilename))
                {
                    // there is a cache file, check date reset it if needed
                    using (var fileStream = new FileStream(_CacheFilename, FileMode.Open))
                    {
                        var timeBytes = new byte[4];

                        fileStream.Read(timeBytes, 0, 4);

                        _Timestamp = BitConverter.ToUInt32(timeBytes);
                    }

                    if (_Timestamp != timestamp)
                    {
                        RyzomClient.Log.Info("SM: Clearing string cache : outofdate");

                        // the cache is not sync, reset it TODO this is not working correctly
                        using var fileStream = new FileStream(_CacheFilename, FileMode.Open);

                        var timeBytes = BitConverter.GetBytes(timestamp);

                        fileStream.Write(timeBytes, 0, 4);
                    }
                    else
                    {
                        RyzomClient.Log.Info("SM : string cache in sync. cool");
                    }
                }
                else
                {
                    RyzomClient.Log.Info("SM: Creating string cache");

                    // cache file don't exist, create it with the timestamp
                    using var fileStream = new FileStream(_CacheFilename, FileMode.OpenOrCreate);

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
                using var fileStream2 = new FileStream(_CacheFilename, FileMode.Open);

                var timeBytes2 = new byte[4];

                fileStream2.Read(timeBytes2, 0, 4);

                _Timestamp = BitConverter.ToUInt32(timeBytes2);
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

                    //RyzomClient.Log.Debug($"SM : loading string [{id}] as [{str}] in cache");

                    if (!ReceivedStrings.ContainsKey(id))
                        ReceivedStrings.Add(id, str);
                }

                _CacheLoaded = true;
            }
            catch (Exception e)
            {
                RyzomClient.Log.Warn($"SM : loadCache failed, exception : {e.GetType().Name} {e.Message}");
                RyzomClient.Log.Warn("SM : cache deactivated");

                // deactivated cache.
                _CacheFilename = "";
            }
        }

        /// <summary>
        /// flush the server string cache
        /// </summary>
        public static void FlushStringCache()
        {
            if (CacheStringToSave.Count > 0)
            {
                using var fileStream = new FileStream(_CacheFilename, FileMode.Open);

                foreach (var cacheString in CacheStringToSave)
                {
                    var idBytes = BitConverter.GetBytes(cacheString.StringId);
                    var strBytes = Encoding.UTF8.GetBytes(cacheString.String); // todo check that
                    var lenBytes = BitConverter.GetBytes(strBytes.Length);

                    fileStream.Write(idBytes);
                    fileStream.Write(lenBytes);
                    fileStream.Write(strBytes);
                }

                CacheStringToSave.Clear();
            }
        }

        /// <summary>
        ///     extract the dynamic string from the stream and check if it is complete
        /// </summary>
        public static void ReceiveDynString(BitMemoryStream bms)
        {
            var dynInfo = new DynamicStringInfo { Status = TStatus.Received };

            // read the dynamic string Id
            uint dynId = 0;
            bms.Serial(ref dynId);

            // read the base string Id
            uint stringId = 0;
            bms.Serial( /*dynInfo.*/ref stringId);
            dynInfo.StringId = stringId;

            // try to build the string
            dynInfo.Message = bms;
            BuildDynString(dynInfo);

            if (dynInfo.Status == TStatus.Complete)
            {
                RyzomClient.Log?.Debug($"DynString {dynId} available : [{dynInfo.String}]");

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
                WaitingDynStrings.Add(dynId, dynInfo);
        }

        /// <summary>
        ///     assemble the dynamic string from DynamicStringInfo
        /// </summary>
        private static bool BuildDynString(DynamicStringInfo dynInfo)
        {
            if (dynInfo.Status == TStatus.Received)
            {
                if (!GetString(dynInfo.StringId, out dynInfo.String))
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
                                    catch (Exception) { }
                                    break;

                                case 'i':
                                    param.Type = ParamType.Integer;
                                    try
                                    {
                                        param.Integer = 0;
                                        dynInfo.Message.Serial(ref param.Integer);
                                    }
                                    catch (Exception) { }
                                    break;

                                case 't':
                                    param.Type = ParamType.Time;
                                    try
                                    {
                                        param.Time = 0;
                                        dynInfo.Message.Serial(ref param.Time);
                                    }
                                    catch (Exception) { }
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
                                    catch (Exception) { }
                                    break;

                                case 'm':
                                    param.Type = ParamType.DynStringID;
                                    try
                                    {
                                        param.DynStringId = 0;
                                        dynInfo.Message.Serial(ref param.DynStringId);
                                    }
                                    catch (Exception) { }
                                    break;

                                default:
                                    RyzomClient.Log.Warn("Error: unknown replacement tag %%%c", (char)character);
                                    return false;
                            }

                            dynInfo.Params.Add(param);
                        }
                    }
                }
                dynInfo.Status = TStatus.Serialized;
            }

            if (dynInfo.Status == TStatus.Serialized)
            {
                // try to retreive all string parameter to build the string.
                StringBuilder temp = new StringBuilder();

                int move = 0;

                for (int i = 0; i < dynInfo.Params.Count; i++)
                {
                    var param = dynInfo.Params[i];

                    switch (param.Type)
                    {
                        case ParamType.StringID:
                            {
                                if (!GetString(param.StringId, out string str))
                                    return false;

                                int p1 = str.IndexOf('[');

                                if (p1 != -1)
                                {
                                    // TODO GetLocalizedName 
                                    //str = str.Substring(0, p1) + StringManagerClient.GetLocalizedName(str.Substring(p1));
                                }

                                // If the string is a player name, we may have to remove the shard name (if the string looks like a player name)
                                if (str.Length != 0 && Connection.PlayerSelectedHomeShardNameWithParenthesis.Length > 0)
                                {
                                    // fast pre-test
                                    if (str[^1] == ')')
                                    {
                                        // the player name must be at least bigger than the string with ()
                                        if (str.Length > Connection.PlayerSelectedHomeShardNameWithParenthesis.Length)
                                        {
                                            // If the shard name is the same as the player home shard name, remove it
                                            uint len = (uint)Connection.PlayerSelectedHomeShardNameWithParenthesis.Length;
                                            uint start = (uint)str.Length - len;

                                            // TODO remove the shard from player name
                                            //if (ucstrnicmp(str, start, len, Connection.PlayerSelectedHomeShardNameWithParenthesis) == 0)
                                            //	str.resize(start);
                                        }
                                    }
                                }

                                // If the string contains a title, then remove it
                                int pos = str.IndexOf('$');

                                if (str.Length > 0 && pos != -1)
                                {
                                    // todo remove title from name
                                    //str = CEntityCL.removeTitleFromName(str);
                                }

                                // if the string contains a special rename of creature, remove it
                                if (str.Length > 2 && str[0] == '<' && str[1] == '#')
                                {
                                    str = char.ToUpper(str[2]) + str.Substring(3);
                                }

                                // append this string
                                temp.Append(dynInfo.String.Substring(move, param.ReplacementPoint - move));
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
                                temp.Append(dynInfo.String.Substring(move, param.ReplacementPoint - move));
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
                                temp.Append(dynInfo.String.Substring(move, param.ReplacementPoint - move));
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
                                temp.Append(dynInfo.String.Substring(move, param.ReplacementPoint - move));
                                temp.Append(str);
                                move = param.ReplacementPoint + 2;
                            }
                            // TODO (from ryzom code)
                            //					temp.append(move, src+param.ReplacementPoint);
                            //					move = dynInfo.String.begin()+param.ReplacementPoint+2;
                            break;

                        case ParamType.DynStringID:
                            {
                                if (!GetDynString(param.DynStringId, out string dynStr))
                                    return false;
                                //temp.append(move, src + param.ReplacementPoint);
                                //temp += dynStr;
                                //move = dynInfo.String.begin() + param.ReplacementPoint + 2;

                                var str = $"{dynStr}";
                                temp.Append(dynInfo.String.Substring(move, param.ReplacementPoint - move));
                                temp.Append(str);
                                move = param.ReplacementPoint + 2;
                            }
                            break;

                        default:
                            RyzomClient.Log.Warn("Unknown parameter type.");
                            break;
                    }
                }
                // append the rest of the string
                temp.Append(dynInfo.String.Substring(move, dynInfo.String.Length - move));

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

                dynInfo.Status = TStatus.Complete;
                dynInfo.Message = null;
                dynInfo.String = temp.ToString();
                return true;
            }

            if (dynInfo.Status == TStatus.Complete)
                return true;

            RyzomClient.Log?.Warn($"Inconsistent dyn string status : {dynInfo.Status}");
            return false;
        }

        public static bool GetDynString(uint dynStringId, out string result)
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
                    RyzomClient.Log?.Warn($"CStringManager::getDynString : the string {dynStringId} is received but still in _WaintingDynStrings !");
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

                if (BuildDynString(WaitingDynStrings[dynStringId]))
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
        /// request the stringId from the local cache or if missing ask the server
        /// </summary>
        public static bool GetString(uint stringId, out string result)
        {
            if (stringId == 93402 || stringId == 93403) // TODO: why do this ids lead to a disconnect?
            {
                result = string.Empty;
                return false;
            }

            if (!ReceivedStrings.ContainsKey(stringId))
            {
                if (!WaitingStrings.Contains(stringId))
                {
                    WaitingStrings.Add(stringId);

                    // need to ask for this string.
                    var bms = new BitMemoryStream();
                    const string msgType = "STRING_MANAGER:STRING_RQ";

                    if (GenericMessageHeaderManager.PushNameToStream(msgType, bms))
                    {
                            bms.Serial(ref stringId);
                            NetworkManager.Push(bms);
                            RyzomClient.Log?.Debug($"<CStringManagerClient.getString> sending 'STRING_MANAGER:STRING_RQ' message to server for stringId {stringId}");
                    }
                    else
                    {
                        RyzomClient.Log?.Warn(
                            "<CStringManagerClient.getString> unknown message name 'STRING_MANAGER:STRING_RQ'");
                    }
                }

                // result.erase(); // = _WaitString;
                result = string.Empty;

                return false;
            }

            result = ReceivedStrings[stringId];

            if (result.Length <= 9 || result.Substring(0, 9) != "<missing:") return true;

            if (DynStrings.ContainsKey(result.Substring(9, result.Length - 10)))
            {
                result = DynStrings[result.Substring(9, result.Length - 10)];
            }

            return true;
        }

        internal static void ReceiveString(uint stringId, string str)
        {
            //H_AUTO(CStringManagerClient_receiveString)

            if (WaitingStrings.Contains(stringId))
            {
                WaitingStrings.Remove(stringId);
            }

            var updateCache = true;

            if (ReceivedStrings.ContainsKey(stringId))
            {
                RyzomClient.Log.Warn($"Receiving stringID {stringId} ({str}), already in received string ({ReceivedStrings[stringId]}), replacing with new value.");

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
                if (_CacheInited && _CacheFilename.Length > 0)
                {
                    var cs = new CacheString
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

            // try to complete any pending dyn string
            bool restartLoop = false;

            while (restartLoop)
            {
                foreach (var dynString in WaitingDynStrings)
                {
                    /// Warning: if getDynString() return true, 'first' is erased => don't use it after in this loop
                    if (GetDynString(dynString.Key, out string value))
                    {
                        var number = dynString.Key;

                        RyzomClient.Log.Info($"DynString {number} available : [{value}]");

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

        internal static void RemoveStringWaiter(StringWaitCallback stringWaitCallback)
        {
            //throw new NotImplementedException();
        }
    }
}