// This code is a modified version of a file from the 'Ryzom - MMORPG Framework'
// <http://dev.ryzom.com/projects/ryzom/>,
// which is released under GNU Affero General Public License.
// <http://www.gnu.org/licenses/>
// Original Copyright 2010 by Winch Gate Property Limited

using System.Collections.Generic;
using RCC.Network;

namespace RCC.Client
{
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