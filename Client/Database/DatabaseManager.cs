﻿///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using RCC.Network;
using System;
using System.IO;
using System.Xml;

namespace RCC.Database
{
    /// <summary>
	/// Class to manage a database of properties
    /// </summary>
    /// <author>Stephane Coutelas</author>
    /// <author>Nevrax France</author>
    /// <date>2002</date>
    public class DatabaseManager
    {
        /// <summary>
        /// Database bank identifiers (please change BankNames in cpp accordingly)
        /// </summary>
        public enum BankIdentifiers { CDBPlayer = 0, CDBGuild = 1, /* CDBContinent, */ CDBOutpost = 2, /* CDBGlobal, */ NB_CDB_BANKS = 3, INVALID_CDB_BANK = 4 };

        /// <summary>
        /// Names of the bank identifiers
        /// </summary>
        private static readonly string[] BankNames = { "PLR", "GUILD", /* "CONTINENT", */ "OUTPOST", /* "GLOBAL", */ "<NB>", "INVALID" };

        /// <summary>True while the first database packet has not been completely processed (including branch observers)</summary>
        private bool _initInProgress;

        /// <summary>The number of "init database packet" received</summary>
        private byte _initDeltaReceived;

        /// <summary>
        /// Manages the bank names and mappings
        /// </summary>
        protected BankHandler BankHandler;

        private DatabaseNodeBranch _serverDatabase;

        private static DatabaseNodeBranch _database;

        /// <summary>Pointer to the ryzom client</summary>
        private readonly RyzomClient _client;

        /// <summary>
        /// Default constructor
        /// </summary>
        public DatabaseManager(RyzomClient client)
        {
            _client = client;

            //_database = new DatabaseNodeBranch("ROOT");
            _serverDatabase = new DatabaseNodeBranch("SERVER");

            BankHandler = new BankHandler((uint)BankIdentifiers.NB_CDB_BANKS);

            _initInProgress = true;
            _initDeltaReceived = 0;
        }

        /// <summary>
        /// Return a ptr on the database node
        /// </summary>
        /// <returns>ptr on the database node</returns>
        public DatabaseNodeBranch GetNodePtr() { return _serverDatabase; }

        /// <summary>
        ///  Returns the root branch of the database.
        /// </summary>
        /// <returns></returns>
        public static DatabaseNodeBranch GetDb()
        {
            return _database ??= new DatabaseNodeBranch("ROOT");
        }

        /// <summary>
        /// Build the structure of the database from a file
        /// </summary>
        /// <params name="fileName">is the name of file containing the database structure</params>
        public void Init(string fileName, Action progressCallBack)
        {
            try
            {
                var file = new XmlDocument();

                // Init an xml stream
                file.Load(fileName);

                // Parse the parser output!!!
                BankHandler.ResetNodeBankMapping(); // in case the game is restarted from start
                BankHandler.FillBankNames(BankNames, (uint)(BankIdentifiers.INVALID_CDB_BANK + 1));

                _serverDatabase ??= new DatabaseNodeBranch("SERVER"); // redundant?

                _serverDatabase.Init(file.DocumentElement, progressCallBack, true, BankHandler);
            }
            catch (Exception e)
            {
                // Output error
                _client.GetLogger().Warn($"CFormLoader: Error while loading the form {fileName}: {e.Message}");
            }
        }

        /// <summary>
        /// Save a backup of the database
        /// </summary>
        /// <params name="fileName">is the name of the backup file</params>
        public void Write(string fileName)
        {
            if (_database != null)
            {
                var f = new StreamWriter(fileName, false);

                _database.Write(_database.GetName(), f);
                f.Close();
            }
            else
            {
                RyzomClient.GetInstance().GetLogger().Warn($"<CCDBSynchronised::write> can't write {fileName} : the database has not been initialized");
            }
        }

        /// <summary>
        /// Update the database from a stream coming from the FE
        /// </summary>
        /// <params name="f">the stream</params>
        public void ReadDelta(uint gc, BitMemoryStream s, uint bank)
        {
            _client.GetLogger().Debug("Update DB");

            if (_serverDatabase == null)
            {
                _client.GetLogger().Warn("<CCDBSynchronised::readDelta> the database has not been initialized");
                return;
            }

            uint propertyCount = 0;
            s.Serial(ref propertyCount, 16);

            if (Constants.VerboseDatabase)
                _client.GetLogger().Debug($"CDB: Reading delta ({propertyCount} changes)");


            for (uint i = 0; i != propertyCount; ++i)
            {
                _serverDatabase.ReadAndMapDelta(gc, s, bank, BankHandler);
            }
        }

        /// <summary>Return true while the first database packet has not been completely received</summary>
        public bool InitInProgress() { return _initInProgress; }

        /// <summary>Reset the init state (if you relauch the game from scratch)</summary>
        public void ResetInitState() { _initDeltaReceived = 0; _initInProgress = true; WriteInitInProgressIntoUIDB(); }

        /// <summary>Called after flushObserversCalls() as it calls the observers for branches</summary>
        public void SetChangesProcessed()
        {
            if (AllInitPacketReceived())
            {
                _initInProgress = false;
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

        public void SetInitPacketReceived() { ++_initDeltaReceived; }

        private bool AllInitPacketReceived() { return _initDeltaReceived == 2; } // Classic database + inventory

        private void WriteInitInProgressIntoUIDB()
        {
            //TODO: implementation of WriteInitInProgressIntoUIDB -> UI:VARIABLES:CDB_INIT_IN_PROGRESS
        }

        /// <summary>
		/// Resets the specified bank.
		/// </summary>
		/// <param name="gc"> GameCycle ( no idea what it is exactly, probably some time value)</param>
		/// <param name="bank"> The banks we want to reset</param>
        public void ResetBank(in uint gc, in uint bank)
        {
            _database.ResetNode(gc, BankHandler.GetUidForBank(bank));
        }

        /// <summary>
	    /// Return the value of a property (the update flag is set to false)
	    /// </summary>
	    /// <param name="name">is the name of the property</param>
	    /// <returns>the value of the property</returns>
        public long GetProp(string name)
        {
            if (_database != null)
            {
                var txtId = new TextId(name);
                return _database.GetProp(txtId);
            }

            throw new Exception("EDBNotInit");
        }

        /// <summary>
        /// Retrieves a leaf node from the database.
        /// </summary>
        /// <param name="name">name of the data leaf node we are querying.</param>
        /// <param name="create">when true if a node cannot be found it is created.</param>
        public DatabaseNodeLeaf GetDbProp(string name, bool create)
        {
            return GetDbLeaf(name, create);
        }

        /// <summary>
        /// Returns the specified leaf node from the database.
        /// </summary>
        /// <param name="name">The name of the leaf node.</param>
        /// <param name="create">Specifies if the node should be created if it doesn't exist yet.</param>
        public DatabaseNodeLeaf GetDbLeaf(string name, bool create)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            DatabaseNodeLeaf leaf = GetNodePtr().GetNode(new TextId(name), create) as DatabaseNodeLeaf;
            return leaf;
        }
    }
}