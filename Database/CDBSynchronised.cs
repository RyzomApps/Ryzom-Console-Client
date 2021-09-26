///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Minecraft Console Client'
// https://github.com/ORelio/Minecraft-Console-Client
// which is released under CDDL-1.0 License
// http://opensource.org/licenses/CDDL-1.0
// Copyright 2021 ORelio and Contributers
///////////////////////////////////////////////////////////////////

using RCC.Network;
using System;
using System.Collections.Generic;

namespace RCC.Database
{
    /// <summary>
	/// Class to manage a database of properties
    /// </summary>
    /// <author>Stephane Coutelas</author>
    /// <author>Nevrax France</author>
    /// <date>2002</date>
    public class CDBSynchronised //: public CCDBManager
    {
        /// <summary>string associations</summary>
        private readonly Dictionary<uint, string> _Strings;

        /// <summary>True while the first database packet has not been completely processed (including branch observers)</summary>
        private bool _InitInProgress;

        /// <summary>The number of "init database packet" received</summary>
        private byte _InitDeltaReceived;

        //protected CCDBBankHandler bankHandler;
        //protected CCDBBranchObservingHandler branchObservingHandler;
        protected /*CRefPtr<CCDBNodeBranch>*/ object _database;

        private RyzomClient _client;

        //CRefPtr<CCDBNodeLeaf> m_CDBInitInProgressDB { }

        /// <summary>exception thrown when database is not initialized</summary>
        public class EDBNotInit : Exception
        {
            public EDBNotInit() : base("Property Database not initialized", null) { }
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public CDBSynchronised(RyzomClient client)
        {
            _client = client;
        }

        /// <summary>
        /// Return a ptr on the node
        /// </summary>
        /// <returns>ptr on the node</returns>
        public object GetNodePtr() { return _database; }

        /// <summary>
        /// Build the structure of the database from a file
        /// </summary>
        /// <params name="fileName">is the name of file containing the database structure</params>
        public void Init(string fileName, Action progressCallBack) { }

        /// <summary>
        /// Load a backup of the database
        /// </summary>
        /// <params name="fileName">is the name of the backup file</params>
        public void Read(string fileName) { }

        /// <summary>
        /// Save a backup of the database
        /// </summary>
        /// <params name="fileName">is the name of the backup file</params>
        public void Write(string fileName) { }

        /// <summary>
        /// Update the database from a stream coming from the FE
        /// </summary>
        /// <params name="f">the stream</params>
        public void ReadDelta(uint gc, BitMemoryStream s, uint bank)
        {
            _client.GetLogger().Debug("Update DB");

            if (_database == null)
            {
                _client.GetLogger().Warn("<CCDBSynchronised::readDelta> the database has not been initialized");
                //return;
            }

            //displayBitStream2( f, f.getPosInBit(), f.getPosInBit() + 64 );
            uint propertyCount = 0;
            s.Serial(ref propertyCount, 16);


            //if (NLMISC::ICDBNode::isDatabaseVerbose())
            _client.GetLogger().Info($"CDB: Reading delta ({propertyCount} changes)");
            //NbDatabaseChanges += propertyCount;
            //
            //for (uint i = 0; i != propertyCount; ++i)
            //{
            //    _database.readAndMapDelta(gc, s, bank, &bankHandler);
            //}
        }

        /// <summary>
        /// Return the value of a property (the update flag is set to false)
        /// </summary>
        /// <params name="name">is the name of the property</params>
        /// <returns>the value of the property</returns>
        public long GetProp(string name) { return 0; }

        /// <summary>
        /// Set the value of a property (the update flag is set to true)
        /// </summary>
        /// <params name="name">is the name of the property</params>
        /// <params name="value">is the value of the property</params>
        /// <returns>bool : 'true' if the property was found.</returns>
        public bool SetProp(string name, long value) { return false; }

        /// <summary>
        /// Return the string associated with id
        /// </summary>
        /// <params name="id">is the string id</params>
        /// <returns>the string</returns>
        public string GetString(uint id) { return null; }

        /// <summary>
        /// Set a new string association
        /// </summary>
        /// <params name="id">is the string id</params>
        /// <params name="str">is the new string</params>
        public void SetString(uint id, string str) { }

        /// <summary>
        /// Clear the database
        /// </summary>
        public void Clear() { }

        /// <summary>
        /// Destructor
        /// </summary>
        ~CDBSynchronised() { Clear(); }

        /// <summary>Return true while the first database packet has not been completely received</summary>
        public bool InitInProgress() { return _InitInProgress; }

        /// <summary>tests</summary>
        public void Test() { }

        /// <summary>Reset the init state (if you relauch the game from scratch)</summary>
        public void ResetInitState() { _InitDeltaReceived = 0; _InitInProgress = true; WriteInitInProgressIntoUIDB(); }

        /// <summary>Called after flushObserversCalls() as it calls the observers for branches</summary>
        public void SetChangesProcessed()
        {
            if (AllInitPacketReceived())
            {
                _InitInProgress = false;
                WriteInitInProgressIntoUIDB(); // replaced by DECLARE_INTERFACE_USER_FCT(isDBInitInProgress)
            }
        }

        /// <summary>
        /// Notifies the observers whose observed branches were updated.
        /// </summary>
        internal void FlushObserverCalls()
        {
            // TODO FlushObserverCalls: branchObservingHandler.flushObserverCalls();
        }

        private void ImpulseDatabaseInitPlayer(BitMemoryStream impulse) { }
        private void ImpulseInitInventory(BitMemoryStream impulse) { }

        public void SetInitPacketReceived() { ++_InitDeltaReceived; }
        private bool AllInitPacketReceived() { return _InitDeltaReceived == 2; } // Classic database + inventory

        private void WriteInitInProgressIntoUIDB() { }


    }
}
