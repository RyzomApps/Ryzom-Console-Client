///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using static System.String;

namespace RCC.Database
{
    public class CDBBankHandler
    {
        /// Mapping from server database index to client database index (first-level nodes)
        List<List<uint>> _CDBBankToUnifiedIndexMapping;

        /// Mapping from client database index to bank IDs (first-level nodes)
        List<uint> _UnifiedIndexToBank = new List<uint>();

        /// Last index mapped
        uint _CDBLastUnifiedIndex;

        /// Number of bits for first-level branches, by bank
        List<uint> _FirstLevelIdBitsByBank;

        /// Names of the CDB banks
        List<string> _CDBBankNames;

        /// The number of banks used
        uint _maxBanks;

        internal CDBBankHandler(uint maxbanks)
        {
            _CDBBankToUnifiedIndexMapping = new List<List<uint>>(new List<uint>[maxbanks]);
            _FirstLevelIdBitsByBank = new List<uint>(new uint[maxbanks]);

            _maxBanks = maxbanks;
        }

        public void resetNodeBankMapping()
        {

        }

        public void fillBankNames(string[] cdbBankNames, uint size)
        {
            _CDBBankNames = new List<string>();

            for (uint i = 0; i < size; i++)
                _CDBBankNames.Add(cdbBankNames[i]);
        }

        public void mapNodeByBank(string bankName)
        {
            var b = getBankByName(bankName);

            // no such bank
            if (b == unchecked((uint)-1))
                return;

            if (_CDBBankToUnifiedIndexMapping[(int)b] == null) _CDBBankToUnifiedIndexMapping[(int)b] = new List<uint>();
            _CDBBankToUnifiedIndexMapping[(int)b].Add(_CDBLastUnifiedIndex);
            ++_CDBLastUnifiedIndex;
            _UnifiedIndexToBank.Add(b);
        }

        public uint getBankByName(string name)
        {
            const uint b = unchecked((uint)-1);

            for (var i = 0; i < _CDBBankNames.Count; i++)
                if (Compare(_CDBBankNames[i], name, StringComparison.Ordinal) == 0)
                    return (uint)i;

            return b;
        }

        public void calcIdBitsByBank()
        {
            for (uint bank = 0; bank != _maxBanks; bank++)
            {
                var nbNodesOfBank = (uint)_CDBBankToUnifiedIndexMapping[(int)bank].Count;
                uint idb = 0;

                if (nbNodesOfBank > 0)
                    for (idb = 1; nbNodesOfBank > (1 << (int)idb); idb++) { }

                _FirstLevelIdBitsByBank[(int)bank] = idb;
            }
        }
    }
}