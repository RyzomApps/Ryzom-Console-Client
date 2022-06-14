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
using System.Linq;
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
        private enum ProgressType
        {
            ActionProgress = 0,
            UpgradeProgress,

            NumProgressType
        }

        private enum SlotType
        {
            EditionSlot = 1,
            BookStartSlot = 2
        }

        #region const

        // number of items in a trade page
        const uint TRADE_PAGE_NUM_ITEMS = 8;
        const uint TRADE_MAX_NUM_PAGES = 128;
        const uint TRADE_MAX_ENTRIES = TRADE_MAX_NUM_PAGES * TRADE_PAGE_NUM_ITEMS;

        const string PHRASE_DB_BOOK = "UI:PHRASE:BOOK";

        public static readonly string[] PHRASE_DB_PROGRESSION = { "UI:PHRASE:PROGRESS_ACTIONS", "UI:PHRASE:PROGRESS_UPGRADES" };

        const string PHRASE_DB_MEMORY = "UI:PHRASE:MEMORY";
        const string PHRASE_DB_MEMORY_ALT = "UI:PHRASE:MEMORY_ALT";
        const string PHRASE_DB_EXECUTE_NEXT = "UI:PHRASE:EXECUTE_NEXT:PHRASE";
        const string PHRASE_DB_EXECUTE_NEXT_IS_CYCLIC = "UI:PHRASE:EXECUTE_NEXT:ISCYCLIC";
        const string PHRASE_DB_BOTCHAT = "LOCAL:TRADING";

        const uint PHRASE_MAX_BOOK_SLOT = 512;
        const uint PHRASE_MAX_PROGRESSION_SLOT = 512;
        public const uint PHRASE_MAX_MEMORY_SLOT = 20;

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
        private DatabaseNodeLeaf[] _bookDbLeaves;
        private DatabaseNodeLeaf[] _memoryDbLeaves;
        private DatabaseNodeLeaf[] _memoryAltDbLeaves;

        private DatabaseNodeLeaf _nextExecuteLeaf;
        private DatabaseNodeLeaf _nextExecuteIsCyclicLeaf;

        // Shortcut To PhraseSheets Leaves in BotChat
        private DatabaseNodeLeaf[] _botChatPhraseSheetLeaves;
        private DatabaseNodeLeaf[] _botChatPhrasePriceLeaves;

        // Phrase sheet progression
        private readonly DatabaseNodeLeaf[][] _progressionDbSheets = new DatabaseNodeLeaf[(int)ProgressType.NumProgressType][];
        private readonly DatabaseNodeLeaf[][] _progressionDbLevels = new DatabaseNodeLeaf[(int)ProgressType.NumProgressType][];
        private readonly DatabaseNodeLeaf[][] _progressionDbLocks = new DatabaseNodeLeaf[(int)ProgressType.NumProgressType][];

        private int _maxSlotSet;

        private bool _initInGameDone;

        /// <summary> The number of entries setuped to not 0</summary>
        private uint _lastBookNumDbFill;

        /// <summary>For phrase compatibility with enchant weapon special power</summary>
        private SheetId _enchantWeaponMainBrick;

        /// <summary>Map of All Phrase. Contains the Book + some system phrase (1: the Edition Phrase)</summary>
        private readonly Dictionary<int, PhraseCom> _phraseMap = new Dictionary<int, PhraseCom>();

        /// <summary>map each phrase to its sheet id</summary>
        private readonly Dictionary<PhraseCom, int> _phraseToSheet = new Dictionary<PhraseCom, int>();

        /// <summary> extra client data </summary>
        private readonly List<PhraseClient> _phraseClient = new List<PhraseClient>();

        private readonly List<MemoryLine> _memories = new List<MemoryLine>();


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
            if (_initInGameDone || _databaseManager == null)
            {
                return;
            }

            //_registerClassDone = false;

            _initInGameDone = true;

            // Init Database values.
            uint i;
            _bookDbLeaves = new DatabaseNodeLeaf[PHRASE_MAX_BOOK_SLOT];

            for (i = 0; i < PHRASE_MAX_BOOK_SLOT; i++)
            {
                var node = _databaseManager.GetDbProp(PHRASE_DB_BOOK + ":" + i + ":PHRASE");
                node.SetValue32(0);
                _bookDbLeaves[i] = node;
            }

            _memoryDbLeaves = new DatabaseNodeLeaf[PHRASE_MAX_MEMORY_SLOT];
            _memoryAltDbLeaves = new DatabaseNodeLeaf[PHRASE_MAX_MEMORY_SLOT];

            for (i = 0; i < PHRASE_MAX_MEMORY_SLOT; i++)
            {
                var node = _databaseManager.GetDbProp(PHRASE_DB_MEMORY + ":" + i + ":PHRASE");
                node.SetValue32(0);
                _memoryDbLeaves[i] = node;

                var nodeAlt = _databaseManager.GetDbProp(PHRASE_DB_MEMORY_ALT + ":" + i + ":PHRASE");
                nodeAlt.SetValue32(0);
                _memoryAltDbLeaves[i] = nodeAlt;
            }

            // Progression Db leaves
            Debug.Assert((int)ProgressType.NumProgressType == PHRASE_DB_PROGRESSION.Length);

            for (uint j = 0; j < (int)ProgressType.NumProgressType; j++)
            {
                _progressionDbSheets[j] = new DatabaseNodeLeaf[PHRASE_MAX_PROGRESSION_SLOT];
                _progressionDbLocks[j] = new DatabaseNodeLeaf[PHRASE_MAX_PROGRESSION_SLOT];
                _progressionDbLevels[j] = new DatabaseNodeLeaf[PHRASE_MAX_PROGRESSION_SLOT];
            }

            for (i = 0; i < PHRASE_MAX_PROGRESSION_SLOT; i++)
            {
                for (uint j = 0; j < (int)ProgressType.NumProgressType; j++)
                {
                    // SHEET
                    var node1 = _databaseManager.GetDbProp(PHRASE_DB_PROGRESSION[j] + ":" + i + ":SHEET");
                    node1.SetValue32(0);
                    _progressionDbSheets[j][i] = node1;

                    // LEVEL
                    node1 = _databaseManager.GetDbProp(PHRASE_DB_PROGRESSION[j] + ":" + i + ":LEVEL");
                    node1.SetValue32(0);
                    _progressionDbLevels[j][i] = node1;

                    // LOCKED
                    node1 = _databaseManager.GetDbProp(PHRASE_DB_PROGRESSION[j] + ":" + i + ":LOCKED");
                    node1.SetValue32(0);
                    _progressionDbLocks[j][i] = node1;
                }
            }

            // init the UI Next Execute slot
            var node2 = _databaseManager.GetDbProp(PHRASE_DB_EXECUTE_NEXT);
            node2.SetValue32(0);
            _nextExecuteLeaf = node2;

            node2 = _databaseManager.GetDbProp(PHRASE_DB_EXECUTE_NEXT_IS_CYCLIC);
            node2.SetValue32(0);
            _nextExecuteIsCyclicLeaf = node2;

            // Init BotChat leaves
            _botChatPhraseSheetLeaves = new DatabaseNodeLeaf[PHRASE_MAX_BOTCHAT_SLOT];
            _botChatPhrasePriceLeaves = new DatabaseNodeLeaf[PHRASE_MAX_BOTCHAT_SLOT];

            for (i = 0; i < PHRASE_MAX_BOTCHAT_SLOT; i++)
            {
                var nodeSheet = _databaseManager.GetDbProp(PHRASE_DB_BOTCHAT + ":" + i + ":SHEET");
                var nodePrice = _databaseManager.GetDbProp(PHRASE_DB_BOTCHAT + ":" + i + ":PRICE");

                _botChatPhraseSheetLeaves[i] = nodeSheet;
                _botChatPhrasePriceLeaves[i] = nodePrice;
            }

            // and so update book and memory db
            UpdateBookDb();
            UpdateMemoryDbAll();

            // Load the success table here
            LoadSuccessTable();

            // compute and the progression phrase, and update DB
            ComputePhraseProgression();

            _enchantWeaponMainBrick = new SheetId("bsxea10.sbrick");

            // build map that gives its description for each built-in phrase
            // slow test on all sheets here ...
            var sm = _sheetManager.GetSheets();
            var tmpPhrase = new PhraseCom();

            foreach (var (key, value) in sm)
            {
                if (value.EntitySheet == null || value.EntitySheet.Type != EntitySheet.TType.SPHRASE)
                    continue;

                BuildPhraseFromSheet(ref tmpPhrase, key.AsInt());
                _phraseToSheet[tmpPhrase] = (int)key.AsInt();
            }
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

        /// <summary>
        /// Set the phrase but don't update the DB (NB: no phrase lock)
        /// </summary>
        internal void SetPhraseNoUpdateDb(ushort slot, PhraseCom phrase)
        {
            SetPhraseInternal(slot, phrase, false, false);
        }

        /// <summary>
        /// real stuff
        /// </summary>
        private void SetPhraseInternal(in ushort slot, PhraseCom phrase, bool @lock, bool updateDb)
        {
            // don't allow slot too big. don't allow set the 0 slot.
            if (slot > PHRASE_MAX_ID || slot == 0)
            {
                return;
            }

            // enlargePhraseClient
            while (slot >= _phraseClient.Count)
            {
                _phraseClient.Add(new PhraseClient());
            }

            // set the phrase
            _phraseMap[slot] = phrase;

            // increment the phrase version.
            _phraseClient[slot].Version++;

            // BotChat lock?
            _phraseClient[slot].Lock = @lock;

            // For Free Slot Mgt.
            _maxSlotSet = Math.Max(_maxSlotSet, slot);

            // update the book, if necessary
            if (updateDb && !@lock && slot >= (ushort)SlotType.BookStartSlot)
            {
                UpdateBookDb();
            }

        }

        /// <summary>
        /// Update Action Book Database. You need only to use it in conjunction with setPhraseNoUpdateDB()
        /// </summary>
        internal void UpdateBookDb()
        {
            // If DB not inited, no-op
            if (!_initInGameDone)
            {
                return;
            }

            // Fill All the book.
            var numBookFill = _phraseMap.Count;

            var i = 0;

            while (i < numBookFill)
            {
                var (key, value) = _phraseMap.ElementAt(i);

                // if the slot is not from the book, then don't display it
                // if the slot is Locked (BotChat confirm wait), then don't display it too
                // if the slot does not match the filter, then don't display it too
                if (key < (int)SlotType.BookStartSlot || _phraseClient[key].Lock || !MatchBookSkillFilter(value))
                {
                    numBookFill--;
                }
                else
                {
                    // fill with the phrase id
                    _bookDbLeaves[i].SetValue32(key);

                    // next mem fill
                    i++;
                }

                // if no more place on book, stop
                if (i < PHRASE_MAX_BOOK_SLOT)
                    continue;

                numBookFill = (int)PHRASE_MAX_BOOK_SLOT;
                break;
            }

            // reset old no more used to empty
            for (i = numBookFill; i < (int)_lastBookNumDbFill; i++)
            {
                _bookDbLeaves[i].SetValue32(0);
            }

            // update cache
            _lastBookNumDbFill = (uint)numBookFill;
        }

        /// <summary>
        /// true if the phrase is "compatbile" with _BookSkillFitler
        /// </summary>
        private static bool MatchBookSkillFilter(PhraseCom value)
        {
            return false;
        }

        /// <summary>
        /// Memorize a phrase (no MSG send)
        /// </summary>
        internal void MemorizePhrase(int memoryLine, int memorySlot, int slot)
        {
            if (memorySlot >= PHRASE_MAX_MEMORY_SLOT)
            {
                return;
            }

            if (slot >= _phraseClient.Count)
            {
                return;
            }

            // Can memorize only a phrase of the book
            if (slot < (int)SlotType.BookStartSlot)
            {
                return;
            }

            // first force forget old mem slot
            // TODO: forgetPhrase(memoryLine, memorySlot);

            // then memorize new one.
            if (!_phraseMap.ContainsKey(slot))
            {
                // TODO: is that correct?
                return;
            }

            // enlarge memory if needed
            while (memoryLine >= _memories.Count)
            {
                _memories.Add(new MemoryLine());
            }

            // update memory
            _memories[memoryLine].Slot[memorySlot].IsMacro = false;
            _memories[memoryLine].Slot[memorySlot].Id = (uint)slot;

            //// must update DB?
            //if (memoryLine == _selectedMemoryDb || memoryLine == _selectedMemoryDBalt)
            //{
            //    // update the DB
            //    UpdateMemoryDbSlot(memorySlot);
            //
            //    // update the ctrl state
            //    UpdateMemoryCtrlState(memorySlot);
            //
            //    // If there is an execution running with this action, maybe re-display it
            //    if (_CurrentExecutePhraseIdNext == slot || _CurrentExecutePhraseIdCycle == slot)
            //    {
            //        UpdateExecutionDisplay();
            //    }
            //}
        }


        internal void UpdateMemoryBar()
        {
            if (!_initInGameDone)
                return;

            UpdateMemoryDbAll();
            UpdateAllMemoryCtrlState();
        }

        private void ComputePhraseProgression()
        {

        }

        private void LoadSuccessTable()
        {

        }

        private void UpdateAllMemoryCtrlState()
        {

        }

        private void UpdateMemoryDbAll()
        {

        }

        private void UpdateMemoryCtrlState(in int memorySlot)
        {

        }

        private void UpdateMemoryDbSlot(in int memorySlot)
        {

        }

        internal void UpdateEquipInvalidation(uint v)
        {

        }

        internal void UpdateAllActionRegen()
        {

        }
    }
}