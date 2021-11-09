///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using static System.String;

namespace Client.Database
{
    /// <summary>
    /// Manages the bank names and mappings of the CDB it's associated with
    /// 
    /// Banks are numeric identifiers for the top-level branches of the CDB.
    /// They are used for saving bandwidth, because the local CDBs are updated with deltas,
    /// that identify the updatable top-level branch with this id.
    /// The CCDBBankHandler manages the mapping of banks to their names, unified (databaseNode) index,
    /// and the other way around.
    /// </summary>
    public class BankHandler
    {
        /// <summary>
        /// Mapping from server database index to client database index (first-level nodes)
        /// </summary>
        private readonly List<List<uint>> _cdbBankToUnifiedIndexMapping;

        /// <summary>
        /// Mapping from client database index to bank IDs (first-level nodes)
        /// </summary>
        private readonly List<uint> _unifiedIndexToBank = new List<uint>();

        /// <summary>
        /// Last index mapped
        /// </summary>
        private uint _cdbLastUnifiedIndex;

        /// <summary>
        /// Number of bits for first-level branches, by bank
        /// </summary>
        private readonly List<uint> _firstLevelIdBitsByBank;

        /// <summary>
        /// Names of the CDB banks
        /// </summary>
        private List<string> _cdbBankNames;

        /// <summary>
        /// The number of banks used
        /// </summary>
        private readonly uint _maxBanks;

        /// <summary>
        /// The class' constructor
        /// </summary>
        /// <param name="maxbanks">the maximum number of banks we need to handle</param>
        internal BankHandler(uint maxbanks)
        {
            _cdbBankToUnifiedIndexMapping = new List<List<uint>>(new List<uint>[maxbanks]);
            _firstLevelIdBitsByBank = new List<uint>(new uint[maxbanks]);

            _maxBanks = maxbanks;
        }

        /// <summary>
        /// Resets the databaseNode to bank mapping vector
        /// </summary>
        public void ResetNodeBankMapping()
        {
            _unifiedIndexToBank.Clear();
        }

        /// <summary>
        /// Loads the known bank names from an array ( the order decides the bank Id ).
        /// </summary>
        /// <param name="strings">The array of the banks names.</param>
        /// <param name="size">The size of the array.</param>
        public void FillBankNames(string[] strings, uint size)
        {
            _cdbBankNames = new List<string>();

            for (uint i = 0; i < size; i++)
                _cdbBankNames.Add(strings[i]);
        }

        /// <summary>
        /// Maps the specified bank name to a unified (databaseNode) index and vica versa.
        /// </summary>
        /// <param name="bankName">Name of the bank to map.</param>
        public void MapNodeByBank(string bankName)
        {
            var b = GetBankByName(bankName);

            // no such bank
            if (b == unchecked((uint)-1))
                return;

            _cdbBankToUnifiedIndexMapping[(int)b] ??= new List<uint>();

            _cdbBankToUnifiedIndexMapping[(int)b].Add(_cdbLastUnifiedIndex);

            ++_cdbLastUnifiedIndex;
            _unifiedIndexToBank.Add(b);
        }

        /// <summary>
        /// Looks up the bank Id of the bank name specified.
        /// </summary>
        /// <param name="name">The name of the bank whose Id we need.</param>
        /// <returns>Returns the id of the bank, or static_cast(uint)(-1) on fail.</returns>
        public uint GetBankByName(string name)
        {
            const uint b = unchecked((uint)-1);

            for (var i = 0; i < _cdbBankNames.Count; i++)
                if (Compare(_cdbBankNames[i], name, StringComparison.Ordinal) == 0)
                    return (uint)i;

            return b;
        }

        /// <summary>
        /// Calculates the number of bits used to store the number of nodes that belong to the banks.
        /// </summary>
        public void CalcIdBitsByBank()
        {
            for (uint bank = 0; bank < _maxBanks; bank++)
            {
                var nbNodesOfBank = (uint)_cdbBankToUnifiedIndexMapping[(int)bank].Count;

                uint idb = 0;

                if (nbNodesOfBank > 0)
                    for (idb = 1; nbNodesOfBank > 1 << (int)idb; idb++) { }

                _firstLevelIdBitsByBank[(int)bank] = idb;
            }
        }

        internal int GetUnifiedIndexToBankSize()
        {
            return _unifiedIndexToBank.Count;
        }

        /// <summary>
        /// Returns the number of bits used to store the number of nodes that belong to this bank.
        /// </summary>
        /// <param name="bank">The banks whose id bits we need.</param>
        /// <returns>Returns the number of bits used to store the number of nodes that belong to this bank.</returns>
        public uint GetFirstLevelIdBits(in int bank)
        {
            return _firstLevelIdBitsByBank[bank];
        }

        /// <summary>
        /// Looks up the unified (databaseNode) index of a bank databaseNode.
        /// </summary>
        /// <param name="bank">The bank id of the databaseNode we are looking up.</param>
        /// <param name="index">The index of the databaseNode within the bank.</param>
        /// <returns>Returns the unified (databaseNode) index of the specified bank databaseNode.</returns>
        public uint GetServerToClientUidMapping(in int bank, in int index)
        {
            return _cdbBankToUnifiedIndexMapping[bank][index];
        }

        /// <summary>
        /// Returns the unified(node) index for the specified bank Id.
        /// </summary>
        /// <param name="bank">The bank whose uid we need.</param>
        /// <returns>Returns an uid or static_cast(uint)(-1) on failure.</returns>
        public int GetUidForBank(uint bank)
        {
            var uid = unchecked((uint)-1);

            for (var i = 0; i < _unifiedIndexToBank.Count; i++)
            {
                if (_unifiedIndexToBank[i] == bank)
                {
                    return i;
                }
            }

            return (int)uid;
        }
    }
}