///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Client.Client;
using Client.Database;
using Client.Sheet;

namespace Client.Phrase
{
    /// <summary>
    /// Singleton to Get/Set Sabrina Phrase in SpellBook / Memory.
    /// </summary>
    /// <remarks>NB: you MUST create it (getInstance()), before loading of ingame.xmls.</remarks>
    /// <author>Lionel Berenguier</author>
    /// <author>Nevrax France</author>
    /// <date>2003</date>
    public class PhraseManager
    {
        #region const

        // number of items in a trade page
        const uint TRADE_PAGE_NUM_ITEMS = 8;
        const uint TRADE_MAX_NUM_PAGES = 128;
        const uint TRADE_MAX_ENTRIES = TRADE_MAX_NUM_PAGES * TRADE_PAGE_NUM_ITEMS;

        const string PHRASE_DB_BOOK = "UI:PHRASE:BOOK";

        public static readonly string[] PHRASE_DB_PROGRESSION =
            {"UI:PHRASE:PROGRESS_ACTIONS", "UI:PHRASE:PROGRESS_UPGRADES"};

        const string PHRASE_DB_MEMORY = "UI:PHRASE:MEMORY";
        const string PHRASE_DB_MEMORY_ALT = "UI:PHRASE:MEMORY_ALT";
        const string PHRASE_DB_EXECUTE_NEXT = "UI:PHRASE:EXECUTE_NEXT:PHRASE";
        const string PHRASE_DB_EXECUTE_NEXT_IS_CYCLIC = "UI:PHRASE:EXECUTE_NEXT:ISCYCLIC";
        const string PHRASE_DB_BOTCHAT = "LOCAL:TRADING";

        private const uint PHRASE_MAX_BOOK_SLOT = 512;
        private const uint PHRASE_MAX_PROGRESSION_SLOT = 512;
        private const uint PHRASE_MAX_MEMORY_SLOT = 20;

        private const uint PHRASE_MAX_BOTCHAT_SLOT = TRADE_MAX_ENTRIES;

        // u16 only for client/server com.
        private const uint PHRASE_MAX_ID = 65535;

        // For phrase execution counter
        private const int PHRASE_EXECUTE_COUNTER_MASK = 255;
        const string PHRASE_DB_COUNTER_NEXT = "SERVER:EXECUTE_PHRASE:NEXT_COUNTER";
        const string PHRASE_DB_COUNTER_CYCLE = "SERVER:EXECUTE_PHRASE:CYCLE_COUNTER";

        #endregion

        private readonly SheetManager _sheetManager;
        private readonly StringManager _stringManager;
        private readonly InterfaceManager _interfaceManager;
        private readonly DatabaseManager _databaseManager;

        // Shortcut To Phrases Leaves
        DatabaseNodeLeaf[] _BookDbLeaves;
        DatabaseNodeLeaf[] _MemoryDbLeaves;
        DatabaseNodeLeaf[] _MemoryAltDbLeaves;
        DatabaseNodeLeaf _NextExecuteLeaf;
        DatabaseNodeLeaf _NextExecuteIsCyclicLeaf;

        // Shortcut To PhraseSheets Leaves in BotChat
        DatabaseNodeLeaf[] _BotChatPhraseSheetLeaves;
        DatabaseNodeLeaf[] _BotChatPhrasePriceLeaves;

        // Phrase sheet progression
        DatabaseNodeLeaf[][] _ProgressionDbSheets = new DatabaseNodeLeaf[(int)TProgressType.NumProgressType][];
        DatabaseNodeLeaf[][] _ProgressionDbLevels = new DatabaseNodeLeaf[(int)TProgressType.NumProgressType][];
        DatabaseNodeLeaf[][] _ProgressionDbLocks = new DatabaseNodeLeaf[(int)TProgressType.NumProgressType][];

        // For phrase compatibility with enchant weapon special power
        SheetId _EnchantWeaponMainBrick;

        private bool _InitInGameDone;

        /// <summary>
        /// Constructor
        /// </summary>
        public PhraseManager(SheetManager sheetManager, StringManager stringManager, InterfaceManager interfaceManager, DatabaseManager databaseManager)
        {
            _sheetManager = sheetManager;
            _stringManager = stringManager;
            _interfaceManager = interfaceManager;
            _databaseManager = databaseManager;

            Reset();

            //for(uint i=0;i<NumSuccessTable;i++)
            //    _SuccessTableSheet[i]= NULL;

            //_RegenTickRangeTouched = true;
        }

        /// <summary>
        /// Reset
        /// </summary>
        private void Reset()
        {

        }

        //static bool _registerClassDone;

        /// <summary>
        /// To call when The DB inGame is setup. Else, no write is made to it before. (NB: DB is updated here)
        /// </summary>
        public void InitInGame()
        {
            Debug.Print("TODO");
            return;

            if (_InitInGameDone)
            {
                return;
            }

            //_registerClassDone = false;

            _InitInGameDone = true;

            // Init Database values.
            var pIm = _interfaceManager;

            uint i;
            _BookDbLeaves = new DatabaseNodeLeaf[PHRASE_MAX_BOOK_SLOT];
            for (i = 0; i < PHRASE_MAX_BOOK_SLOT; i++)
            {
                var node = _databaseManager.GetDbProp(PHRASE_DB_BOOK + ":" + i + ":PHRASE");
                node.SetValue32(0);
                _BookDbLeaves[i] = node;
            }
            _MemoryDbLeaves = new DatabaseNodeLeaf[PHRASE_MAX_MEMORY_SLOT];
            _MemoryAltDbLeaves = new DatabaseNodeLeaf[PHRASE_MAX_MEMORY_SLOT];

            for (i = 0; i < PHRASE_MAX_MEMORY_SLOT; i++)
            {
                var node = _databaseManager.GetDbProp(PHRASE_DB_MEMORY + ":" + i + ":PHRASE");
                node.SetValue32(0);
                _MemoryDbLeaves[i] = node;
                DatabaseNodeLeaf node_alt = _databaseManager.GetDbProp(PHRASE_DB_MEMORY_ALT + ":" + i + ":PHRASE");
                node_alt.SetValue32(0);
                _MemoryAltDbLeaves[i] = node_alt;
            }

            // Progression Db leaves
            Debug.Assert((int)TProgressType.NumProgressType == PHRASE_DB_PROGRESSION.Length);

            for (uint j = 0; j < (int)TProgressType.NumProgressType; j++)
            {
                _ProgressionDbSheets[j] = new DatabaseNodeLeaf[PHRASE_MAX_PROGRESSION_SLOT];
                _ProgressionDbLocks[j] = new DatabaseNodeLeaf[PHRASE_MAX_PROGRESSION_SLOT];
                _ProgressionDbLevels[j] = new DatabaseNodeLeaf[PHRASE_MAX_PROGRESSION_SLOT];
            }

            for (i = 0; i < PHRASE_MAX_PROGRESSION_SLOT; i++)
            {
                for (uint j = 0; j < (int)TProgressType.NumProgressType; j++)
                {
                    // SHEET
                    DatabaseNodeLeaf node = _databaseManager.GetDbProp(PHRASE_DB_PROGRESSION[j] + ":" + i + ":SHEET");
                    node.SetValue32(0);
                    _ProgressionDbSheets[j][i] = node;
                    // LEVEL
                    node = _databaseManager.GetDbProp(PHRASE_DB_PROGRESSION[j] + ":" + i + ":LEVEL");
                    node.SetValue32(0);
                    _ProgressionDbLevels[j][i] = node;
                    // LOCKED
                    node = _databaseManager.GetDbProp(PHRASE_DB_PROGRESSION[j] + ":" + i + ":LOCKED");
                    node.SetValue32(0);
                    _ProgressionDbLocks[j][i] = node;
                }
            }

            {
                // init the UI Next Execute slot
                var node = _databaseManager.GetDbProp(PHRASE_DB_EXECUTE_NEXT);
                node.SetValue32(0);
                _NextExecuteLeaf = node;
                node = _databaseManager.GetDbProp(PHRASE_DB_EXECUTE_NEXT_IS_CYCLIC);
                node.SetValue32(0);
                _NextExecuteIsCyclicLeaf = node;
            }
            // Init BotChat leaves
            _BotChatPhraseSheetLeaves = new DatabaseNodeLeaf[PHRASE_MAX_BOTCHAT_SLOT];
            _BotChatPhrasePriceLeaves = new DatabaseNodeLeaf[PHRASE_MAX_BOTCHAT_SLOT];
            for (i = 0; i < PHRASE_MAX_BOTCHAT_SLOT; i++)
            {
                DatabaseNodeLeaf nodeSheet = _databaseManager.GetDbProp(PHRASE_DB_BOTCHAT + ":" + i + ":SHEET");
                DatabaseNodeLeaf nodePrice = _databaseManager.GetDbProp(PHRASE_DB_BOTCHAT + ":" + i + ":PRICE");
                _BotChatPhraseSheetLeaves[i] = nodeSheet;
                _BotChatPhrasePriceLeaves[i] = nodePrice;
            }

            // TODO: below

            //// and so update book and memory db
            //updateBookDB();
            //updateMemoryDBAll();
            //
            //// Load the success table here
            //loadSuccessTable();
            //
            //// compute and the progression phrase, and update DB
            //computePhraseProgression();

            _EnchantWeaponMainBrick = new SheetId("bsxea10.sbrick");

            //// build map that gives its description for each built-in phrase
            //// slow test on all sheets here ...
            //var sm = _sheetManager.GetSheets();
            //int result = 0;
            //var tmpPhrase = new PhraseCom();
            //
            //for (_sheetManager.EntitySheetMap.const_iterator it = sm.begin(); it != sm.end(); ++it)
            //{
            //    if (it.second.EntitySheet && it.second.EntitySheet.Type == EntitySheet.TType.SPHRASE)
            //    {
            //        //C++ TO C# CONVERTER TODO TASK: There is no equivalent to 'const_cast' in C#:
            //        const_cast<PhraseManager>(this).buildPhraseFromSheet(tmpPhrase, it.first.asInt());
            //        _PhraseToSheet[tmpPhrase] = it.first.asInt();
            //    }
            //}
        }

        /// <summary>
        /// For BotChat learning, build a phrase from .sphrase sheetId
        /// </summary>
        internal void BuildPhraseFromSheet(ref PhraseCom phrase, uint sheetId)
        {
            if (_sheetManager.Get(new SheetId(sheetId)) is PhraseSheet phraseSheet)
            {
                // get localized Name
                phrase.Name = _stringManager.GetSPhraseLocalizedName(new SheetId(sheetId));

                // Build bricks
                phrase.Bricks.Clear();

                for (uint i = 0; i < phraseSheet.Bricks.Length; i++)
                {
                    phrase.Bricks.Add(phraseSheet.Bricks[i]);
                }
            }
            else
            {
                phrase = PhraseCom.EmptyPhrase;
            }
        }

        internal void SetPhraseNoUpdateDB(ushort knownSlot, PhraseCom phraseCom)
        {

        }

        internal void UpdateBookDB()
        {

        }

        internal void MemorizePhrase(byte memoryLineId, byte memorySlotId, ushort phraseId)
        {

        }

        internal void UpdateMemoryBar()
        {

        }

        internal void UpdateEquipInvalidation(uint v)
        {

        }

        internal void UpdateAllActionRegen()
        {

        }

        enum TProgressType
        {
            ActionProgress = 0,
            UpgradeProgress,

            NumProgressType
        };
    }
}