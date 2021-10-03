///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using RCC.Network;

namespace RCC.Client
{
    /// <summary>
    /// Info about a dynamically generated string from the server
    /// </summary>
    public class DynamicStringInfo
    {
        public BitMemoryStream Message;
        public List<ParamValue> Params = new List<ParamValue>();

        public StringStatus Status;
        public string String;
        public uint StringId;

        public enum StringStatus : byte
        {
            Received,
            Serialized,
            Complete
        }

        public enum ParamType : byte
        {
            StringID,
            Integer,
            Time,
            Money,
            DynStringID,
            SheetID
        }

        public struct ParamValue
        {
            public ParamType Type;
            public int ReplacementPoint;

            public uint StringId;
            public int Integer;
            public uint Time;
            public ulong Money;
            public uint DynStringId;
        }
    }
}