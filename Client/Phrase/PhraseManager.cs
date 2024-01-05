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
using System.IO;
using System.Linq;
using API.Sheet;
using Client.Brick;
using Client.Client;
using Client.Database;
using Client.Sheet;
using Client.Stream;
using Client.Strings;

namespace Client.Phrase
{
    /// <summary>
    /// Singleton to Get/Set Sabrina Phrase in SpellBook/Memory.
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
        private readonly DatabaseManager _databaseManager;
        private readonly SheetIdFactory _sheetIdFactory;

        // Shortcut To Phrases Leaves
        private DatabaseNodeLeaf[] _bookDbLeaves;
        private DatabaseNodeLeaf[] _memoryDbLeaves;
        //private DatabaseNodeLeaf[] _memoryAltDbLeaves; not using second memory bar in rcc

        // Shortcut To PhraseSheets Leaves in BotChat
        private DatabaseNodeLeaf[] _botChatPhraseSheetLeaves;
        private DatabaseNodeLeaf[] _botChatPhrasePriceLeaves;

        // Phrase sheet progression
        private readonly DatabaseNodeLeaf[][] _progressionDbSheets = new DatabaseNodeLeaf[(int)ProgressType.NumProgressType][];
        private readonly DatabaseNodeLeaf[][] _progressionDbLevels = new DatabaseNodeLeaf[(int)ProgressType.NumProgressType][];
        private readonly DatabaseNodeLeaf[][] _progressionDbLocks = new DatabaseNodeLeaf[(int)ProgressType.NumProgressType][];

        private int _maxSlotSet;

        private bool _initInGameDone;

        /// <summary>The number of entries that are not set to 0</summary>
        private uint _lastBookNumDbFill;

        /// <summary>For phrase compatibility with enchant weapon special power</summary>
        private SheetId _enchantWeaponMainBrick;

        /// <summary>Map of All Phrase. Contains the Book + some system phrase (1: the Edition Phrase)</summary>
        private readonly Dictionary<int, PhraseCom> _phraseMap = new Dictionary<int, PhraseCom>();

        /// <summary>map each phrase to its sheet id</summary>
        private readonly Dictionary<PhraseCom, int> _phraseToSheet = new Dictionary<PhraseCom, int>();

        /// <summary>extra client data</summary>
        private readonly List<PhraseClient> _phraseClient = new List<PhraseClient>();

        private readonly List<MemoryLine> _memories = new List<MemoryLine>();

        private readonly RyzomClient _client;

        int _selectedMemoryDB = -1;

        //static bool _registerClassDone;

        /// <summary>
        /// Constructor
        /// </summary>
        public PhraseManager(RyzomClient client)
        {
            _client = client;
            _sheetManager = client.GetSheetManager();
            _stringManager = client.GetStringManager();
            _databaseManager = client.GetDatabaseManager();
            _sheetIdFactory = client.GetSheetIdFactory();

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
            _initInGameDone = false;
            _selectedMemoryDB = -1;
        }

        /// <remarks>
        /// Only one memory line is displayed in the Memory DB. if -1, erased.
        /// </remarks>
        internal void SelectMemoryLineDb(int memoryLine)
        {
            if (memoryLine < 0)
                memoryLine = -1;

            if (_selectedMemoryDB == memoryLine)
                return;

            _selectedMemoryDB = memoryLine;

            // since memory selection changes then must update all the DB and the Ctrl states
            UpdateMemoryDbAll();
            UpdateAllMemoryCtrlState();
            // TODO: UpdateAllMemoryCtrlRegenTickRange();

            // must update also the execution views
            // TODO: UpdateExecutionDisplay();
        }

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

            // Initialize Database values
            uint i;
            _bookDbLeaves = new DatabaseNodeLeaf[PHRASE_MAX_BOOK_SLOT];

            for (i = 0; i < PHRASE_MAX_BOOK_SLOT; i++)
            {
                var node = _databaseManager.GetDbProp(PHRASE_DB_BOOK + ":" + i + ":PHRASE");
                node.SetValue32(0);
                _bookDbLeaves[i] = node;
            }

            _memoryDbLeaves = new DatabaseNodeLeaf[PHRASE_MAX_MEMORY_SLOT];
            //_memoryAltDbLeaves = new DatabaseNodeLeaf[PHRASE_MAX_MEMORY_SLOT];

            for (i = 0; i < PHRASE_MAX_MEMORY_SLOT; i++)
            {
                var node = _databaseManager.GetDbProp(PHRASE_DB_MEMORY + ":" + i + ":PHRASE");
                node.SetValue32(0);
                _memoryDbLeaves[i] = node;

                //var nodeAlt = _databaseManager.GetDbProp(PHRASE_DB_MEMORY_ALT + ":" + i + ":PHRASE");
                //nodeAlt.SetValue32(0);
                //_memoryAltDbLeaves[i] = nodeAlt;
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

            // Initialize the UI Next Execute slot
            var node2 = _databaseManager.GetDbProp(PHRASE_DB_EXECUTE_NEXT);
            node2.SetValue32(0);

            node2 = _databaseManager.GetDbProp(PHRASE_DB_EXECUTE_NEXT_IS_CYCLIC);
            node2.SetValue32(0);

            // Initialize BotChat leaves
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

            _enchantWeaponMainBrick = (SheetId)_sheetIdFactory.SheetId("bsxea10.sbrick");

            // build map that gives its description for each built-in phrase
            // slow test on all sheets here ...
            var sm = _sheetManager.GetSheets();
            var tmpPhrase = new PhraseCom();

            foreach (var (key, value) in sm)
            {
                if (!(value.EntitySheet is { Type: SheetType.SPHRASE }))
                    continue;

                BuildPhraseFromSheet(ref tmpPhrase, key.AsInt());
                _phraseToSheet[tmpPhrase] = (int)key.AsInt();
            }
        }

        /// <summary>
        /// return getMemorizedPhrase(memoryLine, memoryIndex), only if this phrase is only used in this memory slot
     	/// else return allocatePhraseSlot()
        /// </summary>
        public uint AllocatePhraseSlot()
        {
            //if (_freeSlots.empty())
            //{
            // if too big, fail.
            if (_maxSlotSet >= PHRASE_MAX_ID)
            {
                return 0;
            }

            // get a free slot
            return (uint)(_maxSlotSet + 1);
            //}
            //else
            //{
            //    uint val = _FreeSlots.back();
            //    _freeSlots.pop_back();
            //    return val;
            //}
        }

        /// <summary>
        /// For BotChat learning, build a phrase from .sphrase sheetId
        /// </summary>
        internal void BuildPhraseFromSheet(ref PhraseCom phrase, uint sheetId)
        {
            if (_sheetManager.Get(_sheetIdFactory.SheetId(sheetId)) is PhraseSheet phraseSheet)
            {
                // get localized Name
                phrase.Name = _stringManager.GetSPhraseLocalizedName((SheetId)_sheetIdFactory.SheetId(sheetId));

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
            // If DB not initialized, no-op
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
        /// true if the phrase is "compatible" with _BookSkillFitler
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
            // TODO: ForgetPhrase(memoryLine, memorySlot);

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

            // must update DB?
            if (memoryLine == _selectedMemoryDB /*||  memoryLine == _selectedMemoryDBalt*/)
            {
                // update the DB
                UpdateMemoryDbSlot((uint)memorySlot);

                // update the ctrl state
                //UpdateMemoryCtrlState(memorySlot);

                // If there is an execution running with this action, maybe re-display it
                //if (_CurrentExecutePhraseIdNext == slot || _CurrentExecutePhraseIdCycle == slot)
                //{
                //    UpdateExecutionDisplay();
                //}
            }
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
            // TODO: Implementation
        }

        private void LoadSuccessTable()
        {
            // TODO: Implementation
        }

        private void UpdateAllMemoryCtrlState()
        {
            // TODO: Implementation
        }

        private void UpdateMemoryDbAll()
        {
            // If DB not inited, no-op
            if (!_initInGameDone)
                return;

            if (_selectedMemoryDB == -1 || _selectedMemoryDB >= _memories.Count)
            {
                for (uint i = 0; i < PHRASE_MAX_MEMORY_SLOT; i++)
                {
                    _memoryDbLeaves[i].SetValue32(0);
                }
            }
            else
            {
                for (uint i = 0; i < PHRASE_MAX_MEMORY_SLOT; i++)
                {
                    var slot = _memories[_selectedMemoryDB].Slot[i];

                    if (!slot.IsPhrase())
                    {
                        _memoryDbLeaves[i].SetValue32(0);
                    }
                    else
                    {
                        _memoryDbLeaves[i].SetValue32((int)slot.Id);
                    }
                }
            }
        }

        public void UpdateMemoryDbSlot(uint memorySlot)
        {
            // If DB not inited, no-op
            if (!_initInGameDone)
                return;

            if (memorySlot >= PHRASE_MAX_MEMORY_SLOT)
                return;

            if (_selectedMemoryDB == -1 || _selectedMemoryDB >= _memories.Count)
                return;

            var slot = _memories[_selectedMemoryDB].Slot[memorySlot];

            if (!slot.IsPhrase())
            {
                _memoryDbLeaves[memorySlot].SetValue32(0);
            }
            else
            {
                _memoryDbLeaves[memorySlot].SetValue32((int)slot.Id);
            }
        }

        internal void UpdateEquipInvalidation(uint v)
        {
            // TODO: Implementation
        }

        internal void UpdateAllActionRegen()
        {
            // TODO: Implementation
        }

        /// <summary>
        /// Dump all phrases to a file.
        /// </summary>
        public void Write(string fileName)
        {
            if (_memories != null)
            {
                var f = new StreamWriter(fileName, false);

                var m = 0;

                foreach (var memory in _memories)
                {
                    var s = 0;

                    foreach (var slot in memory.Slot)
                    {
                        var phrase = GetPhrase(slot.Id);

                        //if (phrase != PhraseCom.EmptyPhrase)
                        //{
                        f.WriteLine($"{m}:{s}\t{slot.Id}\t{slot.IsMacro}\t{slot.IsMacroVisualDirty}\t{phrase.Name}\t{phrase.Bricks?.Count}");

                        if (phrase.Bricks != null)

                            foreach (var brick in phrase?.Bricks)
                            {
                                var bs = (BrickSheet)_sheetManager.Get(brick);

                                f.WriteLine($"\t{brick.AsInt()}\t{brick.GetShortId()}\t{brick.GetSheetType()}\t{bs?.IdIcon}");
                            }
                        //}

                        s++;
                    }

                    m++;
                }

                f.Close();
            }
            else
            {
                RyzomClient.GetInstance().GetLogger().Warn($"<CCDBSynchronised::write> can't write {fileName} : the database has not been initialized");
            }
        }

        /// <summary>
        /// Get a phrase for a slot. Empty Phrase returned if don't exist.
        /// </summary>
        public PhraseCom GetPhrase(uint slot)
        {
            return !_phraseMap.ContainsKey((int)slot) ? PhraseCom.EmptyPhrase : _phraseMap[(int)slot];
        }

        /// <summary>
        /// Common Method to send the Memorize msg to server
        /// </summary>
        public void SendMemorizeToServer(uint memoryLine, uint memorySlot, uint phraseId)
        {
            var @out = new BitMemoryStream();
            const string sMsg = "PHRASE:MEMORIZE";

            if (_client.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(sMsg, @out))
            {
                var phrase = GetPhrase(phraseId);

                // free Band; don't send name
                phrase.Name = "";

                var memoryId = (byte)memoryLine; // action group
                var slotId = (byte)memorySlot; // action slot
                var pid = (ushort)phraseId; // phrase

                @out.Serial(ref memoryId);
                @out.Serial(ref slotId);
                @out.Serial(ref pid);
                PhraseCom.Serial(ref phrase, @out, _sheetIdFactory);

                _client.GetNetworkManager().Push(@out);
                _client.GetLogger().Info($"impulseCallBack : {sMsg} {memoryId} {slotId} {pid} (phrase) sent");
            }
            else
            {
                _client.GetLogger().Warn($"impulseCallBack : unknown message name : '{sMsg}'.");
            }
        }

        /// <summary>
        /// Common Method to send the Forget msg to server
        /// </summary>
        public void SendForgetToServer(uint memoryLine, uint memoryIndex)
        {
            var @out = new BitMemoryStream();
            const string sMsg = "PHRASE:FORGET";

            if (_client.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(sMsg, @out))
            {
                //serial the sentence memorized index
                var memoryId = (byte)memoryLine;
                var slotId = (byte)memoryIndex;

                @out.Serial(ref memoryId);
                @out.Serial(ref slotId);

                _client.GetNetworkManager().Push(@out);
                _client.GetLogger().Info($"impulseCallBack : {sMsg} {memoryId} {slotId} sent");
            }
            else
            {
                _client.GetLogger().Warn($"impulseCallBack : unknown message name : '{sMsg}'.");
            }
        }

        /// <summary>
        /// Send the PHRASE:LEARN message to the server
        /// </summary>
        internal void SendLearnToServer(uint phraseId)
        {
            var phrase = GetPhrase(phraseId);

            if (phrase == PhraseCom.EmptyPhrase)
                return;

            var @out = new BitMemoryStream();
            if (_client.GetNetworkManager().GetMessageHeaderManager().PushNameToStream("PHRASE:LEARN", @out))
            {
                var slotId = (ushort)phraseId;
                @out.Serial(ref slotId);
                PhraseCom.Serial(ref phrase, @out, _sheetIdFactory);
                _client.GetNetworkManager().Push(@out);
                _client.GetLogger().Info($"impulseCallBack : PHRASE:LEARN {slotId} (phrase) sent");
            }
            else
            {
                _client.GetLogger().Warn(" unknown message name 'PHRASE:LEARN");
            }
        }
    }
}