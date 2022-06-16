///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Xml;
using API.Database;
using Client.Network;

namespace Client.Database
{
    /// <summary>
	/// Class to manage a database of properties
    /// </summary>
    /// <author>Stephane Coutelas</author>
    /// <author>Nevrax France</author>
    /// <date>2002</date>
    public class DatabaseManager : IDatabaseManager
    {
        /// <summary>
        /// Verbose Mode
        /// </summary>
        public static bool VerboseDatabase = true;

        /// <summary>
        /// Database bank identifiers (please change BankNames in cpp accordingly)
        /// </summary>
        public enum BankIdentifiers { CdbPlayer = 0, CdbGuild = 1, /* CDBContinent, */ CdbOutpost = 2, /* CDBGlobal, */ NbCdbBanks = 3, InvalidCdbBank = 4 };

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

        // TODO: check server and client database structures
        private DatabaseNodeBranch _serverDatabase;

        // TODO: non static database
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

            BankHandler = new BankHandler((uint)BankIdentifiers.NbCdbBanks);

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
        public DatabaseNodeBranch GetDb()
        {
            return _database ??= new DatabaseNodeBranch("ROOT");
        }

        /// <summary>
        /// Build the structure of the database from a file
        /// </summary>
        /// <param name="fileName">is the name of file containing the database structure</param>
        /// <param name="progressCallBack">progress callback</param>
        public void Init(string fileName, Action progressCallBack)
        {
            try
            {
                var file = new XmlDocument();

                // Init an xml stream
                file.Load(fileName);

                // Parse the parser output!!!
                BankHandler.ResetNodeBankMapping(); // in case the game is restarted from start
                BankHandler.FillBankNames(BankNames, (uint)(BankIdentifiers.InvalidCdbBank + 1));

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
        /// <param name="fileName">is the name of the backup file</param>
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
        /// <param name="gc">game cycle</param>
        /// <param name="s">the stream</param>
        /// <param name="bank">The banks we want to update</param>
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

            if (VerboseDatabase)
                _client.GetLogger().Debug($"CDB: Reading delta ({propertyCount} changes)");


            for (uint i = 0; i != propertyCount; ++i)
            {
                _serverDatabase.ReadAndMapDelta(gc, s, bank, BankHandler);
            }
        }

        /// <summary>
        /// Return true while the first database packet has not been completely received
        /// </summary>
        public bool InitInProgress() { return _initInProgress; }

        /// <summary>
        /// Reset the init state (if you relauch the game from scratch)
        /// </summary>
        public void ResetInitState() { _initDeltaReceived = 0; _initInProgress = true; WriteInitInProgressIntoUiDb(); }

        /// <summary>
        /// Called after flushObserversCalls() as it calls the observers for branches
        /// </summary>
        public void SetChangesProcessed()
        {
            if (!AllInitPacketReceived())
                return;

            _initInProgress = false;
            WriteInitInProgressIntoUiDb(); // replaced by DECLARE_INTERFACE_USER_FCT(isDBInitInProgress)
        }

        /// <summary>
        /// Notifies the observers whose observed branches were updated.
        /// </summary>
        internal void FlushObserverCalls()
        {
            // TODO FlushObserverCalls: branchObservingHandler.flushObserverCalls();
        }

        /// <summary>
        /// Increment the number of "init database packet" received
        /// </summary>
        public void SetInitPacketReceived() { ++_initDeltaReceived; }

        /// <summary>
        /// Classic database + inventory
        /// </summary>
        private bool AllInitPacketReceived() { return _initDeltaReceived == 2; }

        /// <summary>
        /// replaced by DECLARE_INTERFACE_USER_FCT(isDBInitInProgress)
        /// </summary>
        private void WriteInitInProgressIntoUiDb()
        {
            //TODO: implementation of WriteInitInProgressIntoUIDB -> UI:VARIABLES:CDB_INIT_IN_PROGRESS
        }

        /// <inheritdoc />
        public void ResetBank(in uint gc, in uint bank)
        {
            //_database.ResetNode(gc, BankHandler.GetUidForBank(bank));
            _serverDatabase.ResetNode(gc, BankHandler.GetUidForBank(bank));
        }

        /// <inheritdoc />
        public long GetProp(string name)
        {
            if (_database == null)
                throw new Exception("EDBNotInit");

            var txtId = new TextId(name);
            return _database.GetProp(txtId);
        }

        /// <summary>
        /// Retrieves a leaf node from the database.
        /// </summary>
        /// <param name="name">name of the data leaf node we are querying.</param>
        /// <param name="create">when true if a node cannot be found it is created.</param>
        public DatabaseNodeLeaf GetDbProp(string name, bool create = true)
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

            var leaf = GetNodePtr().GetNode(new TextId(name), create) as DatabaseNodeLeaf;
            return leaf;
        }
    }
}
