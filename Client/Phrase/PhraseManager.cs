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
    /// <remarks>There is a Network BUG which prevents us from simply doing {Forget(), Delete()} Old, then {Learn(), Memorize()} New (Messages are shuffled).</remarks>
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
        const string PHRASE_DB_BOTCHAT = "SERVER:TRADING";

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
        private readonly Dictionary<int, PhraseCom> _phraseMap = [];

        /// <summary>map each phrase to its sheet id</summary>
        private readonly Dictionary<PhraseCom, int> _phraseToSheet = [];

        /// <summary>extra client data</summary>
        private readonly List<PhraseClient> _phraseClient = [];

        private readonly List<MemoryLine> _memories = [];

        private readonly RyzomClient _client;

        int _selectedMemoryDb = -1;

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
            _selectedMemoryDb = -1;
        }

        /// <remarks>
        /// Only one memory line is displayed in the Memory DB. if -1, erased.
        /// </remarks>
        internal void SelectMemoryLineDb(int memoryLine)
        {
            if (memoryLine < 0)
                memoryLine = -1;

            if (_selectedMemoryDb == memoryLine)
                return;

            _selectedMemoryDb = memoryLine;

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
                var node = _databaseManager.GetServerNode($"{PHRASE_DB_BOOK}:{i}:PHRASE");
                node.SetValue32(0);
                _bookDbLeaves[i] = node;
            }

            _memoryDbLeaves = new DatabaseNodeLeaf[PHRASE_MAX_MEMORY_SLOT];
            //_memoryAltDbLeaves = new DatabaseNodeLeaf[PHRASE_MAX_MEMORY_SLOT];

            for (i = 0; i < PHRASE_MAX_MEMORY_SLOT; i++)
            {
                var node = _databaseManager.GetServerNode($"{PHRASE_DB_MEMORY}:{i}:PHRASE");
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
                    var node1 = _databaseManager.GetServerNode($"{PHRASE_DB_PROGRESSION[j]}:{i}:SHEET");
                    node1.SetValue32(0);
                    _progressionDbSheets[j][i] = node1;

                    // LEVEL
                    node1 = _databaseManager.GetServerNode($"{PHRASE_DB_PROGRESSION[j]}:{i}:LEVEL");
                    node1.SetValue32(0);
                    _progressionDbLevels[j][i] = node1;

                    // LOCKED
                    node1 = _databaseManager.GetServerNode($"{PHRASE_DB_PROGRESSION[j]}:{i}:LOCKED");
                    node1.SetValue32(0);
                    _progressionDbLocks[j][i] = node1;
                }
            }

            // Initialize the UI Next Execute slot
            var node2 = _databaseManager.GetServerNode(PHRASE_DB_EXECUTE_NEXT);
            node2.SetValue32(0);

            node2 = _databaseManager.GetServerNode(PHRASE_DB_EXECUTE_NEXT_IS_CYCLIC);
            node2.SetValue32(0);

            // Initialize BotChat leaves
            _botChatPhraseSheetLeaves = new DatabaseNodeLeaf[PHRASE_MAX_BOTCHAT_SLOT];
            _botChatPhrasePriceLeaves = new DatabaseNodeLeaf[PHRASE_MAX_BOTCHAT_SLOT];

            for (i = 0; i < PHRASE_MAX_BOTCHAT_SLOT; i++)
            {
                var nodeSheet = _databaseManager.GetServerNode($"{PHRASE_DB_BOTCHAT}:{i}:SHEET");
                var nodePrice = _databaseManager.GetServerNode($"{PHRASE_DB_BOTCHAT}:{i}:PRICE");

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
                if (value.Sheet is not { Type: SheetType.SPHRASE })
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
            if (memoryLine == _selectedMemoryDb /*||  memoryLine == _selectedMemoryDBalt*/)
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

            if (_selectedMemoryDb == -1 || _selectedMemoryDb >= _memories.Count)
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
                    var slot = _memories[_selectedMemoryDb].Slot[i];

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

        private void UpdateMemoryDbSlot(uint memorySlot)
        {
            // If DB not inited, no-op
            if (!_initInGameDone)
                return;

            if (memorySlot >= PHRASE_MAX_MEMORY_SLOT)
                return;

            if (_selectedMemoryDb == -1 || _selectedMemoryDb >= _memories.Count)
                return;

            var slot = _memories[_selectedMemoryDb].Slot[memorySlot];

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

                f.WriteLine("#Line:Slot\tId\tIsMacro\tIsMacroVisualDirty\tName\tBricksCount\tcount");

                foreach (var memory in _memories)
                {
                    var s = 0;

                    foreach (var slot in memory.Slot)
                    {
                        var phrase = GetPhrase(slot.Id);

                        f.WriteLine($"{m}:{s}\t{slot.Id}\t{slot.IsMacro}\t{slot.IsMacroVisualDirty}\t{phrase.Name}\t{phrase.Bricks?.Count}\t{CountAllThatUsePhrase(slot.Id)}");

                        if (phrase.Bricks != null)
                        {
                            if (phrase.Bricks.Count > 0)
                                f.WriteLine("#\tId\tShortId\tSheetType\tIdIcon\tName");

                            foreach (var brick in phrase.Bricks)
                            {
                                var bs = (BrickSheet)_sheetManager.Get(brick);

                                f.WriteLine($"\t{brick.AsInt()}\t{brick.GetShortId()}\t{brick.GetSheetType()}\t{bs?.IdIcon}\t{brick.Name}");
                            }

                            if (phrase.Bricks.Count > 0)
                                f.WriteLine("#Line:Slot\tId\tIsMacro\tIsMacroVisualDirty\tName\tBricksCount\tcount");
                        }

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
                _client.GetLogger().Debug($"{sMsg} {memoryId} {slotId} {pid} (phrase) sent");
            }
            else
            {
                _client.GetLogger().Warn($"Unknown message name : '{sMsg}'.");
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
                _client.GetLogger().Debug($"{sMsg} {memoryId} {slotId} sent");
            }
            else
            {
                _client.GetLogger().Warn($"Unknown message name: '{sMsg}'.");
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
            const string sMsg = "PHRASE:LEARN";

            if (_client.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(sMsg, @out))
            {
                var slotId = (ushort)phraseId;
                @out.Serial(ref slotId);
                PhraseCom.Serial(ref phrase, @out, _sheetIdFactory);
                _client.GetNetworkManager().Push(@out);
                _client.GetLogger().Debug($"{sMsg} {slotId} sent");
            }
            else
            {
                _client.GetLogger().Warn($"Unknown message name: '{sMsg}'.");
            }
        }

        /// <summary>
        /// Common Method to send the execution msg to server
        /// </summary>
        internal void SendExecuteToServer(uint memoryLine, uint memorySlot, bool cyclic)
        {
            var @out = new BitMemoryStream();
            var msgName = cyclic ? "PHRASE:EXECUTE_CYCLIC" : "PHRASE:EXECUTE";

            if (_client.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(msgName, @out))
            {
                //serial the sentence memorized index
                var memoryId = (byte)memoryLine;
                var slotId = (byte)memorySlot;
                @out.Serial(ref memoryId);
                @out.Serial(ref slotId);
                _client.GetNetworkManager().Push(@out);

                _client.GetLogger().Info($"{msgName} {memoryLine} {memorySlot} {cyclic} sent");
            }
            else
            {
                _client.GetLogger().Warn($"Unknown message named '{msgName}'.");
            }
        }

        /// <summary>
        /// Execute a cristalize action on both client and server side
        /// </summary>
        public void ExecuteCristalize(uint memoryLine, uint memorySlot)
        {
            // Increment counter client side (like an execution) (no cyclic)
            SendExecuteToServer((byte)memoryLine, (byte)memorySlot, false);

            // Special sendExecuteToServer
            // Removed ClientCfg.Local check and related code.
            // Instead, directly push the message to the server.
            var @out = new BitMemoryStream();
            const string sMsg = "PHRASE:CRISTALIZE";

            if (_client.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(sMsg, @out))
            {
                // serial the sentence memorized index
                var memoryId = (byte)memoryLine;
                var slotId = (byte)memorySlot;
                @out.Serial(ref memoryId);
                @out.Serial(ref slotId);
                _client.GetNetworkManager().Push(@out);
            }
            else
            {
                _client.GetLogger().Warn($"unknown message {sMsg}");
            }
        }

        /// <summary>
        /// Execute a default attack, on both client and server side.
        /// </summary>
        /// <remarks>Try to launch a standard action "default attack" if found.</remarks>
        public void ExecuteDefaultAttack()
        {
            // Try to find a "default attack" in memories
            if (FindDefaultAttack(out var memoryLine, out var memorySlot))
            {
                SendExecuteToServer((byte)memoryLine, (byte)memorySlot, true);
            }
            else
            {
                // Removed ClientCfg.Local check and related code.
                // Directly send the attack message to the server.
                const string msgName = "COMBAT:DEFAULT_ATTACK";
                var @out = new BitMemoryStream();
                if (_client.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(msgName, @out))
                {
                    _client.GetNetworkManager().Push(@out);
                }
                else
                {
                    _client.GetLogger().Warn($"unknown message named '{msgName}'.");
                }
            }
        }

        /// <summary>
        /// Send the PHRASE:DELETE message to the server
        /// </summary>
        /// <param name="slot">The slot to delete</param>
        public void SendDeleteToServer(uint slot)
        {
            var @out = new BitMemoryStream();
            if (_client.GetNetworkManager().GetMessageHeaderManager().PushNameToStream("PHRASE:DELETE", @out))
            {
                var slotId = (ushort)slot;
                @out.Serial(ref slotId);
                _client.GetNetworkManager().Push(@out);
            }
            else
            {
                _client.GetLogger().Warn("unknown message name 'PHRASE:DELETE'");
            }
        }

        /// <summary>
        /// Rememorize all memories that use this slot
        /// </summary>
        /// <param name="slot">The phrase slot to re-memorize</param>
        public void RememorizeAllThatUsePhrase(uint slot)
        {
            var someMemorized = false;

            // For all memory slot
            for (var i = 0; i < _memories.Count; i++)
            {
                for (var j = 0; j < PHRASE_MAX_MEMORY_SLOT; j++)
                {
                    if (!_memories[i].Slot[j].IsPhrase() || _memories[i].Slot[j].Id != slot)
                        continue;

                    // Re-memorize the phrase, need server only
                    SendMemorizeToServer((uint)i, (uint)j, slot);
                    someMemorized = true;
                }
            }

            // Update all memory ctrl states if some re-learned
            if (someMemorized)
            {
                UpdateAllMemoryCtrlState();
            }
        }

        /// <summary>
        /// Count all memories that use this phrase.
        /// </summary>
        /// <param name="slot">The phrase slot to count.</param>
        /// <returns>The count of memories using the given phrase slot.</returns>
        public uint CountAllThatUsePhrase(uint slot)
        {
            if (slot == 0)
                return 0;

            // For all memory slots
            uint count = 0;
            for (uint i = 0; i < _memories.Count; i++)
            {
                for (uint j = 0; j < PHRASE_MAX_MEMORY_SLOT; j++)
                {
                    if (_memories[(int)i].Slot[j].IsPhrase() &&
                        _memories[(int)i].Slot[j].Id == slot)
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        /// <summary>
        /// Search the default attack like on client. Returns false if not found. 
        /// Prefers to find in the currently selected memory line.
        /// </summary>
        /// <param name="memoryLine">Output parameter for the found memory line index.</param>
        /// <param name="memorySlot">Output parameter for the found memory slot index.</param>
        /// <returns>True if a default attack is found, otherwise false.</returns>
        private bool FindDefaultAttack(out uint memoryLine, out uint memorySlot)
        {
            var brickManager = _client.GetBrickManager();

            memoryLine = 0;
            memorySlot = 0;

            if (_memories.Count == 0)
                return false;

            // Create a sorted list of memory lines, prioritizing the selected one
            var memoryLineSort = Enumerable.Range(0, _memories.Count).ToList();

            // Remove and push the selected memory line to the front
            if (_selectedMemoryDb >= 0 && _selectedMemoryDb < memoryLineSort.Count)
            {
                memoryLineSort.RemoveAt(_selectedMemoryDb);
                memoryLineSort.Insert(0, _selectedMemoryDb);
            }

            // Parse all memories
            foreach (var lineIndex in memoryLineSort)
            {
                var memLine = _memories[lineIndex];
                for (uint j = 0; j < PHRASE_MAX_MEMORY_SLOT; j++)
                {
                    if (!memLine.Slot[j].IsPhrase())
                        continue;

                    var phrase = GetPhrase(memLine.Slot[j].Id);

                    // The phrase must have only one brick: the default attack brick
                    if (phrase.Bricks.Count != 1)
                        continue;

                    var brick = brickManager.GetBrick(phrase.Bricks[0]);

                    // Check if it's a combat root brick
                    if (brick == null || !brick.IsRoot() || !brick.IsCombat())
                        continue;

                    // Found default attack
                    memoryLine = (uint)lineIndex;
                    memorySlot = j;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Retrieves the phrase ID from the specified memory line and index.
        /// </summary>
        /// <param name="memoryLine">The memory line index.</param>
        /// <param name="memoryIndex">The slot index in the memory line.</param>
        /// <returns>The phrase ID if found; otherwise, 0.</returns>
        public uint GetPhraseIdFromMemory(uint memoryLine, uint memoryIndex)
        {
            if (memoryLine >= _memories.Count || memoryIndex >= PHRASE_MAX_MEMORY_SLOT)
            {
                return 0; // Invalid memory line or index
            }

            var slot = _memories[(int)memoryLine].Slot[memoryIndex];
            return slot.IsPhrase() ? slot.Id : 0; // Return the ID if it's a valid phrase
        }

        public void SetPhraseInternal(uint slot, PhraseCom phrase, bool lockPhrase, bool updateDb)
        {
            // Don't allow slot too big. Don't allow to set the 0 slot.
            if (slot > PHRASE_MAX_ID || slot == 0)
                return;

            // Enlarge _phraseClient if needed
            //while (slot >= _phraseClient.Count)
            //{
            //    _phraseClient.Add(new PhraseClient());
            //}

            // Set the phrase
            _phraseMap[(int)slot] = phrase;

            // Increment the phrase version
            _phraseClient[(int)slot].Version++;

            // BotChat lock?
            _phraseClient[(int)slot].Lock = lockPhrase;

            // For Free Slot Management
            _maxSlotSet = Math.Max(_maxSlotSet, (int)slot);

            // Update the book if necessary
            if (updateDb && !lockPhrase && slot >= (uint)SlotType.BookStartSlot)
            {
                UpdateBookDb();
            }
        }

        public void ErasePhrase(uint slot)
        {
            if (slot >= _phraseClient.Count)
                return;

            _phraseMap.Remove((int)slot);
            _phraseClient[(int)slot].Version = -1;
            _phraseClient[(int)slot].Lock = false;

            // Make this slot available for allocation
            //_freeSlots.Add((int)slot);

            // Update the book.
            UpdateBookDb();

            // Ignore: If the phrase erased was currently executed, must stop it
        }
    }
}