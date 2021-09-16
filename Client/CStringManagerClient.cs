using System;
using System.Collections.Generic;
using RCC.Config;
using RCC.Helper;
using RCC.Msg;
using RCC.Network;

namespace RCC.Client
{
    internal static class CStringManagerClient
    {
        public static void receiveDynString(CBitMemStream bms)
        {
            //H_AUTO(CStringManagerClient_receiveDynString)
            //
            TDynStringInfo dynInfo = new TDynStringInfo();
            dynInfo.Status = TDynStringInfo.TStatus.received;
            //// read the dynamic string Id
            uint dynId = 0;
            bms.serial(ref dynId);
            //
            ///// read the base string Id
            uint stringId = 0;
            bms.serial(/*dynInfo.*/ref stringId);

            ConsoleIO.WriteLine($"Received DynString with dynID {dynId} and StringID {stringId}. This is not implemented yet!");

            //// try to build the string
            dynInfo.Message = bms;
            buildDynString(dynInfo);

            //if (dynInfo.Status == TDynStringInfo.complete)
            //{
            //    if (!ClientCfg.Light)
            //    {
            //        //nlinfo("DynString %u available : [%s]", dynId, dynInfo.String.toString().c_str());
            //    }
            //
            //    _ReceivedDynStrings.insert(std.make_pair(dynId, dynInfo));
            //    // security, if dynstring Message received twice, it is possible that the dynstring is still in waiting list
            //    _WaitingDynStrings.erase(dynId);
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
            //}
            //else
            //    _WaitingDynStrings.insert(std.make_pair(dynId, dynInfo));
        }

        private static bool buildDynString(TDynStringInfo dynInfo)
        {
            if (dynInfo.Status == TDynStringInfo.TStatus.received)
            {
                if (!getString(dynInfo.StringId, out dynInfo.String))
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
                dynInfo.Status = TDynStringInfo.TStatus.serialized;
            }

            if (dynInfo.Status == TDynStringInfo.TStatus.serialized)
            {
                //// try to retreive all string parameter to build the string.
                //string temp;
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

                dynInfo.Status = TDynStringInfo.TStatus.complete;
                //dynInfo.Message.clear();
                //dynInfo.String = temp;
                return true;
            }
            if (dynInfo.Status == TDynStringInfo.TStatus.complete)
                return true;

            ConsoleIO.WriteLineFormatted("§eInconsistent dyn string status : " + dynInfo.Status);
            return false;
        }

        static Dictionary<uint, string> _ReceivedStrings = new Dictionary<uint, string>();
        static HashSet<uint> _WaitingStrings = new HashSet<uint>();

        static bool getString(uint stringId, out string result)
        {
            //TStringsContainer::iterator it(_ReceivedStrings.find(stringId));
            if (!_ReceivedStrings.ContainsKey(stringId))
            {
                //CHashSet<uint>::iterator it(_WaitingStrings.find(stringId));
                if (!_WaitingStrings.Contains(stringId))
                {
                    _WaitingStrings.Add(stringId);
                    // need to ask for this string.
                    CBitMemStream bms = new CBitMemStream();
                    const string msgType = "STRING_MANAGER:STRING_RQ";
                    if (GenericMsgHeaderMngr.pushNameToStream(msgType, bms))
                    {
                        bms.serial(ref stringId);
                        NetworkManager.push(bms);
                        ConsoleIO.WriteLineFormatted("§e<CStringManagerClient::getString> sending 'STRING_MANAGER:STRING_RQ' message to server");
                    }
                    else
                    {
                        ConsoleIO.WriteLineFormatted("§e<CStringManagerClient::getString> unknown message name 'STRING_MANAGER:STRING_RQ'");
                    }
                }

                // result.erase(); // = _WaitString;
                result = String.Empty;

                return false;
            }

            result = _ReceivedStrings[stringId];

            if (result.Length > 9 && result.Substring(0, 9) == "<missing:")
            {
                //map<ucstring, ucstring>::iterator itds = _DynStrings.find(result.substr(9, result.size() - 10));
                //if (itds != _DynStrings.end())
                //    result = itds->second;
            }

            return true;
        }
    }
}