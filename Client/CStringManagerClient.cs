using RCC.Helper;
using RCC.Network;

namespace RCC.Client
{
    internal static class CStringManagerClient
    {
        public static void receiveDynString(CBitMemStream bms)
        {
            //H_AUTO(CStringManagerClient_receiveDynString)
            //
            //TDynStringInfo dynInfo;
            //dynInfo.Status = TDynStringInfo::received;
            //// read the dynamic string Id
            uint dynId = 0;
            bms.serial(ref dynId);
            //
            ///// read the base string Id
            uint StringId = 0;
            bms.serial(/*dynInfo.*/ref StringId);

            ConsoleIO.WriteLine($"Received DynString with dynID {dynId} and StringID {StringId}. This is not implemented yet!");

            //
            //// try to build the string
            //dynInfo.Message = bms;
            //buildDynString(dynInfo);
            //
            //if (dynInfo.Status == TDynStringInfo::complete)
            //{
            //    if (!ClientCfg.Light)
            //    {
            //        //nlinfo("DynString %u available : [%s]", dynId, dynInfo.String.toString().c_str());
            //    }
            //
            //    _ReceivedDynStrings.insert(std::make_pair(dynId, dynInfo));
            //    // security, if dynstring Message received twice, it is possible that the dynstring is still in waiting list
            //    _WaitingDynStrings.erase(dynId);
            //
            //    // update the waiting dyn strings
            //    {
            //        std::pair<TStringWaitersContainer::iterator, TStringWaitersContainer::iterator> range =
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
            //        std::pair<TStringCallbacksContainer::iterator, TStringCallbacksContainer::iterator> range =
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
            //    _WaitingDynStrings.insert(std::make_pair(dynId, dynInfo));
        }
    }
}