using System.Collections.Generic;
using RCC.Network;

namespace RCC.Client
{
    /// <summary>
    ///     Info about a dynamically generated string from the server
    /// </summary>
    internal class DynamicStringInfo
    {
        public BitMemoryStream Message;
        public List<ParamValue> Params;

        public TStatus Status;
        public string String;
        public uint StringId;

        internal enum TStatus : byte
        {
            Received,
            Serialized,
            Complete
        };

        internal enum TParamType : byte
        {
            StringID,
            Integer,
            Time,
            Money,
            DynStringID,
            SheetID
        };

        internal struct ParamValue
        {
            public TParamType Type;
            public string ReplacementPoint;

            public uint StringId;
            public int Integer;
            public uint Time;
            public ulong Money;
            public uint DynStringId;
        };
    }
}