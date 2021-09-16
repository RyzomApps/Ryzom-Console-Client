using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using RCC.Network;

namespace RCC.Client
{
    internal class TDynStringInfo
    {
        internal enum TStatus : byte
        {
            received,
            serialized,
            complete
        };

        internal enum TParamType : byte
        {
            string_id,
            integer,
            time,
            money,
            dyn_string_id,
            sheet_id
        };

        struct TParamValue
        {
            TParamType Type;
            string ReplacementPoint;

            uint StringId;
            int Integer;
            uint Time;
            ulong Money;
            uint DynStringId;
        };

        public TStatus Status;

        public CBitMemStream Message;
        public uint StringId;
        List<TParamValue> Params;
        public string String;
    }
}