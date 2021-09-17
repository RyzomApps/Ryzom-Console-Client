// This code is a modified version of a file from the 'Ryzom - MMORPG Framework'
// <http://dev.ryzom.com/projects/ryzom/>,
// which is released under GNU Affero General Public License.
// <http://www.gnu.org/licenses/>
// Original Copyright 2010 by Winch Gate Property Limited

using System.Collections.Generic;
using RCC.Helper;
using RCC.Messages;
using RCC.Network;

namespace RCC.Client
{
    internal static class StringManagerClient
    {
        static readonly Dictionary<string, string> DynStrings = new Dictionary<string, string>();

        static readonly Dictionary<uint, DynamicStringInfo> ReceivedDynStrings =
            new Dictionary<uint, DynamicStringInfo>();

        static readonly Dictionary<uint, DynamicStringInfo> WaitingDynStrings =
            new Dictionary<uint, DynamicStringInfo>();

        static readonly Dictionary<uint, string> ReceivedStrings = new Dictionary<uint, string>();
        private static readonly HashSet<uint> WaitingStrings = new HashSet<uint>();

        public static void ReceiveDynString(BitMemoryStream bms)
        {
            //H_AUTO(CStringManagerClient_receiveDynString)

            var dynInfo = new DynamicStringInfo {Status = DynamicStringInfo.TStatus.Received};

            // read the dynamic string Id
            uint dynId = 0;
            bms.Serial(ref dynId);

            // read the base string Id
            uint stringId = 0;
            bms.Serial( /*dynInfo.*/ref stringId);

            // try to build the string
            dynInfo.Message = bms;
            BuildDynString(dynInfo);

            ConsoleIO.WriteLine($"Received DynString with dynID {dynId} and StringID {stringId}: " + dynInfo.String);

            if (dynInfo.Status == DynamicStringInfo.TStatus.Complete)
            {
                //    if (!ClientConfig.Light)
                //    {
                //        //nlinfo("DynString %u available : [%s]", dynId, dynInfo.String.toString().c_str());
                //    }
                //
                ReceivedDynStrings.Add(dynId, dynInfo);
                //    // security, if dynstring Message received twice, it is possible that the dynstring is still in waiting list
                WaitingDynStrings.Remove(dynId);
                //
                //    // update the waiting dyn strings
                //    {
                //        std.pair<TStringWaitersContainer.iterator, TStringWaitersContainer.iterator> range =
                //            _DynStringsWaiters.equal_range(dynId);
                //
                //        if (range.first != range.second)
                //        {
                //            for (; range.first != range.second; ++range.first)
                //            {
                //                TStringWaiter & sw = range.first->second;
                //                *(sw.Result) = dynInfo.String;
                //            }
                //            _DynStringsWaiters.erase(dynId);
                //        }
                //    }
                //    // callback the waiting dyn strings
                //    {
                //        std.pair<TStringCallbacksContainer.iterator, TStringCallbacksContainer.iterator> range =
                //            _DynStringsCallbacks.equal_range(dynId);
                //
                //        if (range.first != range.second)
                //        {
                //            for (; range.first != range.second; ++range.first)
                //            {
                //                range.first->second->onDynStringAvailable(dynId, dynInfo.String);
                //            }
                //            _DynStringsCallbacks.erase(dynId);
                //        }
                //    }
            }
            else
                WaitingDynStrings.Add(dynId, dynInfo);
        }

        private static bool BuildDynString(DynamicStringInfo dynInfo)
        {
            if (dynInfo.Status == DynamicStringInfo.TStatus.Received)
            {
                if (!GetString(dynInfo.StringId, out dynInfo.String))
                {
                    // can't continue now, need the base string.
                    return false;
                }

                //// ok, we have the base string, we can serial the parameters
                //string.iterator first(dynInfo.String.begin()), last(dynInfo.String.end());
                //for (; first != last; ++first)
                //{
                //	if (*first == '%')
                //	{
                //		first++;
                //		if (first != last && *first != '%')
                //		{
                //			// we have a replacement point.
                //			TParamValue param;
                //			param.ReplacementPoint = (first - 1) - dynInfo.String.begin();
                //			switch (*first)
                //			{
                //				case 's':
                //					param.Type = string_id;
                //					try
                //					{
                //						dynInfo.Message.serial(param.StringId);
                //					}
                //					catch (const Exception &)
                //			{
                //	param.StringId = EmptyStringId;
                //}
                //break;
                //		case 'i':
                //			param.Type = integer;
                //try
                //{
                //	dynInfo.Message.serial(param.Integer);
                //}
                //catch (const Exception &)
                //			{
                //	param.Integer = 0;
                //}
                //break;
                //		case 't':
                //			param.Type = time;
                //try
                //{
                //	dynInfo.Message.serial(param.Time);
                //}
                //catch (const Exception &)
                //			{
                //	param.Time = 0;
                //}
                //break;
                //		case '$':
                //			param.Type = money;
                //try
                //{
                //	dynInfo.Message.serial(param.Money);
                //}
                //catch (const Exception &)
                //			{
                //	param.Money = 0;
                //}
                //break;
                //		case 'm':
                //			param.Type = dyn_string_id;
                //try
                //{
                //	dynInfo.Message.serial(param.DynStringId);
                //}
                //catch (const Exception &)
                //			{
                //	param.DynStringId = EmptyDynStringId;
                //}
                //break;
                //default:
                //			nlwarning("Error: unknown replacement tag %%%c", (char)*first);
                //return false;
                //}
                //
                //dynInfo.Params.push_back(param);
                //}
                //}
                //}
                dynInfo.Status = DynamicStringInfo.TStatus.Serialized;
            }

            if (dynInfo.Status == DynamicStringInfo.TStatus.Serialized)
            {
                //// try to retreive all string parameter to build the string.
                //string temp = "";
                //temp.reserve(dynInfo.String.size() * 2);
                //string.iterator src(dynInfo.String.begin());
                //string.iterator move = src;
                //
                //std.vector<TParamValue>.iterator first(dynInfo.Params.begin()), last(dynInfo.Params.end());
                //for (; first != last; ++first)
                //{
                //	TParamValue & param = *first;
                //	switch (param.Type)
                //	{
                //		case string_id:
                //			{
                //				string str;
                //				if (!getString(param.StringId, str))
                //					return false;
                //
                //				string.size_type p1 = str.find('[');
                //				if (p1 != string.npos)
                //				{
                //					str = str.substr(0, p1) + STRING_MANAGER.CStringManagerClient.getLocalizedName(str.substr(p1));
                //				}
                //
                //				// If the string is a player name, we may have to remove the shard name (if the string looks like a player name)
                //				if (!str.empty() && !PlayerSelectedHomeShardNameWithParenthesis.empty())
                //				{
                //					// fast pre-test
                //					if (str[str.size() - 1] == ')')
                //					{
                //						// the player name must be at least bigger than the string with ()
                //						if (str.size() > PlayerSelectedHomeShardNameWithParenthesis.size())
                //						{
                //							// If the shard name is the same as the player home shard name, remove it
                //							uint len = (uint)PlayerSelectedHomeShardNameWithParenthesis.size();
                //							uint start = (uint)str.size() - len;
                //							if (ucstrnicmp(str, start, len, PlayerSelectedHomeShardNameWithParenthesis) == 0)
                //								str.resize(start);
                //						}
                //					}
                //				}
                //
                //				// If the string contains a title, then remove it
                //				string.size_type pos = str.find('$');
                //				if (!str.empty() && pos != string.npos)
                //				{
                //					str = CEntityCL.removeTitleFromName(str);
                //				}
                //
                //				// if the string contains a special rename of creature, remove it
                //				if (str.size() > 2 && str[0] == '<' && str[1] == '#')
                //				{
                //					str = toUpper(str[2]) + str.substr(3);
                //				}
                //
                //				// append this string
                //				temp.append(move, src + param.ReplacementPoint);
                //				temp += str;
                //				move = dynInfo.String.begin() + param.ReplacementPoint + 2;
                //			}
                //
                //			break;
                //		case integer:
                //			{
                //				char value[1024];
                //				sprintf(value, "%d", param.Integer);
                //				temp.append(move, src + param.ReplacementPoint);
                //				temp += string(value);
                //				move = dynInfo.String.begin() + param.ReplacementPoint + 2;
                //			}
                //			break;
                //		case time:
                //			{
                //				string value;
                //				uint time = (uint)param.Time;
                //				if (time >= (10 * 60 * 60))
                //				{
                //					uint nbHours = time / (10 * 60 * 60);
                //					time -= nbHours * 10 * 60 * 60;
                //					value = toString("%d ", nbHours) + CI18N.get("uiMissionTimerHour") + " ";
                //
                //					uint nbMinutes = time / (10 * 60);
                //					time -= nbMinutes * 10 * 60;
                //					value = value + toString("%d ", nbMinutes) + CI18N.get("uiMissionTimerMinute") + " ";
                //				}
                //				else if (time >= (10 * 60))
                //				{
                //					uint nbMinutes = time / (10 * 60);
                //					time -= nbMinutes * 10 * 60;
                //					value = value + toString("%d ", nbMinutes) + CI18N.get("uiMissionTimerMinute") + " ";
                //				}
                //				uint nbSeconds = time / 10;
                //				value = value + toString("%d", nbSeconds) + CI18N.get("uiMissionTimerSecond");
                //				temp.append(move, src + param.ReplacementPoint);
                //				temp += value;
                //				move = dynInfo.String.begin() + param.ReplacementPoint + 2;
                //			}
                //			break;
                //		case money:
                //			///\todo nicoB/Boris : this is a temp patch that display money as integers
                //			{
                //				char value[1024];
                //				sprintf(value, "%u", (uint)param.Money);
                //				temp.append(move, src + param.ReplacementPoint);
                //				temp += string(value);
                //				move = dynInfo.String.begin() + param.ReplacementPoint + 2;
                //			}
                //			// TODO (from ryzom code)
                //			//					temp.append(move, src+param.ReplacementPoint);
                //			//					move = dynInfo.String.begin()+param.ReplacementPoint+2;
                //			break;
                //		case dyn_string_id:
                //			{
                //				string dynStr;
                //				if (!getDynString(param.DynStringId, dynStr))
                //					return false;
                //				temp.append(move, src + param.ReplacementPoint);
                //				temp += dynStr;
                //				move = dynInfo.String.begin() + param.ReplacementPoint + 2;
                //			}
                //			break;
                //		default:
                //			nlwarning("Unknown parameter type.");
                //			break;
                //	}
                //}
                //// append the rest of the string
                //temp.append(move, dynInfo.String.end());
                //
                //// apply any 'delete' character in the string and replace double '%'
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

                dynInfo.Status = DynamicStringInfo.TStatus.Complete;
                dynInfo.Message = null;
                //dynInfo.String = temp;
                return true;
            }

            if (dynInfo.Status == DynamicStringInfo.TStatus.Complete)
                return true;

            ConsoleIO.WriteLineFormatted("§eInconsistent dyn string status : " + dynInfo.Status);
            return false;
        }

        static bool GetString(uint stringId, out string result)
        {
            //TStringsContainer::iterator it(_ReceivedStrings.find(stringId));
            if (!ReceivedStrings.ContainsKey(stringId))
            {
                //CHashSet<uint>::iterator it(_WaitingStrings.find(stringId));
                if (!WaitingStrings.Contains(stringId))
                {
                    WaitingStrings.Add(stringId);
                    // need to ask for this string.
                    BitMemoryStream bms = new BitMemoryStream();
                    const string msgType = "STRING_MANAGER:STRING_RQ";
                    if (GenericMessageHeaderManager.PushNameToStream(msgType, bms))
                    {
                        bms.Serial(ref stringId);
                        NetworkManager.Push(bms);
                        ConsoleIO.WriteLineFormatted(
                            "§e<CStringManagerClient::getString> sending 'STRING_MANAGER:STRING_RQ' message to server");
                    }
                    else
                    {
                        ConsoleIO.WriteLineFormatted(
                            "§e<CStringManagerClient::getString> unknown message name 'STRING_MANAGER:STRING_RQ'");
                    }
                }

                // result.erase(); // = _WaitString;
                result = string.Empty;

                return false;
            }

            result = ReceivedStrings[stringId];

            if (result.Length > 9 && result.Substring(0, 9) == "<missing:")
            {
                if (DynStrings.ContainsKey(result.Substring(9, result.Length - 10)))
                {
                    result = DynStrings[result.Substring(9, result.Length - 10)];
                }
            }

            return true;
        }
    }
}