///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using API.BotChat;
using Client.Database;
using System;
using System.Collections.Generic;

namespace Client.BotChat
{
    /// <summary>
    /// This manager gives plugins access to the bot chat trading session:
    /// it sends the BOTCHAT impulses (start trade, next page, buy, ...)
    /// and reads the trade list from the client database branch SERVER:TRADING.
    /// </summary>
    public class BotChatManager : IBotChatManager
    {
        private readonly RyzomClient _client;
        private readonly DatabaseManager _databaseManager;

        /// <summary>
        /// Number of trade list entries the server displays at once
        /// (see NB_SLOT_PER_PAGE in game_share/bot_chat_page.h).
        /// </summary>
        public const byte NbSlotPerPage = 8;

        /// <summary>
        /// Client side trade session counter. The server increments its own
        /// counter on each trade session start; both start at 0 and are
        /// incremented to 1 on the first session.
        /// </summary>
        private ushort _tradeSessionCounter = 0;

        /// <summary>
        /// Id of the current trade session as sent by the client in the last
        /// StartTrade* call (0 when no trade session is running).
        /// </summary>
        public ushort CurrentSessionId { get; private set; } = 0;

        /// <summary>
        /// Client side mission session counter (same scheme as the trade
        /// session counter).
        /// </summary>
        private ushort _missionSessionCounter = 0;

        /// <summary>
        /// Id of the current mission session as sent by the client in the last
        /// StartChooseMission/NextMissionPage call (0 when no mission session
        /// is running).
        /// </summary>
        public ushort CurrentMissionSessionId { get; private set; } = 0;

        private List<MissionEntry> _lastMissionList = new List<MissionEntry>();

        /// <inheritdoc/>
        public event Action<uint> OnDynChatOpen;

        /// <inheritdoc/>
        public event Action<uint> OnDynChatClose;

        /// <inheritdoc/>
        public event Action OnSessionForceEnd;

        /// <inheritdoc/>
        public event Action OnTradeListUpdated;

        public BotChatManager(RyzomClient client)
        {
            _client = client;
            _databaseManager = client.GetDatabaseManager();
        }

        #region Notifications (called by NetworkManager)

        /// <summary>
        /// Called by NetworkManager when the server opened a dynamic chat.
        /// </summary>
        /// <param name="botUid">Entity id of the bot</param>
        public void NotifyDynChatOpen(uint botUid)
        {
            OnDynChatOpen?.Invoke(botUid);
        }

        /// <summary>
        /// Called by NetworkManager when the server closed a dynamic chat.
        /// </summary>
        /// <param name="botUid">Entity id of the bot</param>
        public void NotifyDynChatClose(uint botUid)
        {
            OnDynChatClose?.Invoke(botUid);
        }

        /// <summary>
        /// Called by NetworkManager when the server force-ends the bot chat.
        /// </summary>
        public void NotifySessionForceEnd()
        {
            CurrentSessionId = 0;
            CurrentMissionSessionId = 0;
            OnSessionForceEnd?.Invoke();
        }

        /// <summary>
        /// Called by NetworkManager after impulses were processed; raises
        /// OnTradeListUpdated when the trade list content changed.
        /// </summary>
        public void CheckTradeListChanged()
        {
            var list = ReadTradeList();
            if (list.Count != _lastTradeList.Count)
            {
                _lastTradeList = list;
                _pendingStartTrade = null;
                _client.GetLogger().Info($"BotChat: trade list arrived with {list.Count} entries.");
                OnTradeListUpdated?.Invoke();
                return;
            }

            CheckPendingStartTradeTimeout();

            for (var i = 0; i < list.Count; i++)
            {
                if (!SameEntry(list[i], _lastTradeList[i]))
                {
                    _lastTradeList = list;
                    OnTradeListUpdated?.Invoke();
                    return;
                }
            }
        }

        private static bool SameEntry(TradeEntry a, TradeEntry b)
        {
            return a.SheetId == b.SheetId && a.Quality == b.Quality && a.Quantity == b.Quantity &&
                   a.Price == b.Price && a.Currency == b.Currency && a.SlotType == b.SlotType &&
                   a.SellerType == b.SellerType && a.FactionType == b.FactionType &&
                   a.NameId == b.NameId && a.VendorNameId == b.VendorNameId &&
                   a.PrerequisitValid == b.PrerequisitValid;
        }

        private List<TradeEntry> _lastTradeList = new List<TradeEntry>();

        /// <summary>
        /// Trade request awaiting the server answer: impulse name + sent time.
        /// </summary>
        private (string Impulse, DateTime SentAt)? _pendingStartTrade;

        /// <summary>Seconds without an answer before we warn the user.</summary>
        private const double StartTradeTimeoutSeconds = 5;

        /// <summary>
        /// Warn once when no trade list arrived in time. The server silently
        /// ignores START_TRADE_* on bots without the matching trade flag, so
        /// silence is the only failure signal.
        /// </summary>
        private void CheckPendingStartTradeTimeout()
        {
            if (_pendingStartTrade == null)
                return;

            if ((DateTime.UtcNow - _pendingStartTrade.Value.SentAt).TotalSeconds < StartTradeTimeoutSeconds)
                return;

            _client.GetLogger().Warn(
                $"BotChat: no trade list {StartTradeTimeoutSeconds:0}s after {_pendingStartTrade.Value.Impulse} — " +
                "this bot probably has no matching trade flag (not a trader / wrong shop type). Ending session.");

            _pendingStartTrade = null;
            SendEndTrade();
        }

        #endregion

        #region Trade session impulses

        /// <summary>
        /// Bump the session counter and send BOTCHAT:START_TRADE_*.
        /// </summary>
        /// <param name="startImpulseName">Impulse name from msg.xml</param>
        private bool StartTrade(string startImpulseName)
        {
            if (CurrentSessionId != 0)
            {
                // The server keeps one bot chat session per client; end the
                // previous session before starting a new one.
                if (!SendEndTrade())
                    return false;
            }

            // The server increments its own session counter on each session
            // start, then checks that the session id the client sends with
            // each impulse matches (see fillTradePage in phrase_manager.cpp:
            // session != _CurrentTradeSession aborts). Both counters start at
            // 0 and are incremented to 1 on the first session.
            // Clear stale SERVER:TRADING / SERVER:CHOOSE_MISSIONS content left
            // over from the previous session; the server only fills the slots of
            // the current page, so old entries would otherwise be misread as
            // part of the new bot's list (page id / has-next included).
            ClearTradeAndMissionLists();

            _tradeSessionCounter++;
            if (_tradeSessionCounter == 0)
                _tradeSessionCounter = 1;
            CurrentSessionId = _tradeSessionCounter;
            _pendingStartTrade = (startImpulseName, DateTime.UtcNow);

            if (!_client.GetNetworkManager().SendImpulse(startImpulseName, w => w.U16(CurrentSessionId)))
            {
                CurrentSessionId = 0;
                return false;
            }

            return true;
        }

        /// <inheritdoc/>
        public bool SendStartTradeItem()
        {
            return StartTrade("BOTCHAT:START_TRADE_ITEM");
        }

        /// <inheritdoc/>
        public bool SendStartTradeTeleport()
        {
            return StartTrade("BOTCHAT:START_TRADE_TELEPORT");
        }

        /// <inheritdoc/>
        public bool SendStartTradeFaction()
        {
            return StartTrade("BOTCHAT:START_TRADE_FACTION");
        }

        /// <inheritdoc/>
        public bool SendStartTradeSkill()
        {
            return StartTrade("BOTCHAT:START_TRADE_SKILL");
        }

        /// <inheritdoc/>
        public bool SendStartTradePact()
        {
            return StartTrade("BOTCHAT:START_TRADE_PACT");
        }

        /// <inheritdoc/>
        public bool SendStartTradeAction()
        {
            return StartTrade("BOTCHAT:START_TRADE_ACTION");
        }

        /// <inheritdoc/>
        public bool SendNextTradePage()
        {
            if (CurrentSessionId == 0)
            {
                _client.GetLogger().Warn("BotChat: SendNextTradePage failed, no trade session is running.");
                return false;
            }

            return _client.GetNetworkManager().SendImpulse("BOTCHAT:NEXT_PAGE_ITEM", w => w.U16(CurrentSessionId));
        }

        /// <inheritdoc/>
        public bool SendRefreshTradeList()
        {
            return _client.GetNetworkManager().SendImpulse("BOTCHAT:REFRESH_TRADE_LIST");
        }

        /// <inheritdoc/>
        public bool SendSetFilters(uint minQuality, uint maxQuality, uint minPrice, uint maxPrice, byte minClass, byte maxClass, byte itemPart, byte itemType)
        {
            return _client.GetNetworkManager().SendImpulse("BOTCHAT:SET_FILTERS", w =>
            {
                w.U32(minQuality);
                w.U32(maxQuality);
                w.U32(minPrice);
                w.U32(maxPrice);
                w.U8(minClass);
                w.U8(maxClass);
                w.U8(itemPart);
                w.U8(itemType);
            });
        }

        /// <inheritdoc/>
        public bool SendBuyItem(byte index, ushort quantity)
        {
            if (CurrentSessionId == 0)
            {
                _client.GetLogger().Warn("BotChat: SendBuyItem failed, no trade session is running.");
                return false;
            }

            if (index >= NbSlotPerPage)
            {
                _client.GetLogger().Warn($"BotChat: SendBuyItem failed, index {index} out of range.");
                return false;
            }

            return _client.GetNetworkManager().SendImpulse("BOTCHAT:BUY", w =>
            {
                w.U16(index);
                w.U16(quantity);
            });
        }

        /// <inheritdoc/>
        public bool SendBuyPhrase(byte index, ushort phraseId)
        {
            if (CurrentSessionId == 0)
            {
                _client.GetLogger().Warn("BotChat: SendBuyPhrase failed, no trade session is running.");
                return false;
            }

            if (index >= NbSlotPerPage)
            {
                _client.GetLogger().Warn($"BotChat: SendBuyPhrase failed, index {index} out of range.");
                return false;
            }

            // PHRASE:BUY: u8 index, u16 phraseId (0 for not castable phrases)
            return _client.GetNetworkManager().SendImpulse("PHRASE:BUY", w =>
            {
                w.U8(index);
                w.U16(phraseId);
            });
        }

        /// <inheritdoc/>
        public bool SendSell(byte inventoryId, ushort slot, ushort quantity, uint price)
        {
            if (quantity == 0)
            {
                _client.GetLogger().Warn("BotChat: SendSell failed, quantity is zero.");
                return false;
            }

            return _client.GetNetworkManager().SendImpulse("BOTCHAT:SELL", w =>
            {
                w.U8(inventoryId);
                w.U16(slot);
                w.U16(quantity);
                w.U32(price);
            });
        }

        /// <inheritdoc/>
        public bool SendEndTrade()
        {
            if (CurrentSessionId == 0)
                return false;

            var sent = _client.GetNetworkManager().SendImpulse("BOTCHAT:END");
            if (sent)
            {
                CurrentSessionId = 0;
                CurrentMissionSessionId = 0;
                ClearTradeAndMissionLists();
            }
            return sent;
        }

        #endregion

        /// <summary>
        /// Wipe all locally cached trade/mission list leaves (slot contents,
        /// page id, has-next). Called when a session ends or a new one starts
        /// so that leftover values from a previous bot are never returned by
        /// GetTradeList()/GetMissionList(). Zeroing SHEET marks a slot as
        /// empty (GetTradeList skips sheet id 0).
        /// </summary>
        private void ClearTradeAndMissionLists()
        {
            if (_databaseManager == null)
                return;

            for (byte i = 0; i < NbSlotPerPage; i++)
            {
                ClearDatabaseLeaf($"SERVER:TRADING:{i}:SHEET");
                ClearDatabaseLeaf($"SERVER:CHOOSE_MISSIONS:{i}:TEXT");
            }
            ClearDatabaseLeaf("SERVER:TRADING:PAGE_ID");
            ClearDatabaseLeaf("SERVER:TRADING:HAS_NEXT");
            ClearDatabaseLeaf("SERVER:CHOOSE_MISSIONS:PAGE_ID");
            ClearDatabaseLeaf("SERVER:CHOOSE_MISSIONS:HAS_NEXT");
        }

        /// <summary>
        /// Set a database leaf to 0 if it exists. Missing nodes are ignored:
        /// they were never received from the server, so there is nothing stale
        /// to clear.
        /// </summary>
        private void ClearDatabaseLeaf(string name)
        {
            if (_databaseManager.GetNode(name, false) is DatabaseNodeLeaf leaf)
                leaf.SetValue64(0);
        }

        #region Trade list (database branch SERVER:TRADING)

        /// <inheritdoc/>
        public uint PageId
        {
            get { return _databaseManager != null && TryGetProp("SERVER:TRADING:PAGE_ID", out var v) ? (uint)v : 0u; }
        }

        /// <inheritdoc/>
        public bool HasNextPage
        {
            get
            {
                return _databaseManager != null
                    && TryGetProp("SERVER:TRADING:HAS_NEXT", out var v) && v != 0;
            }
        }

        /// <inheritdoc/>
        public List<TradeEntry> GetTradeList()
        {
            return ReadTradeList();
        }

        /// <summary>
        /// Safe property read: returns false when the node does not exist yet
        /// (e.g. the SERVER:TRADING branch has not been received from the server).
        /// </summary>
        private bool TryGetProp(string name, out long value)
        {
            value = 0;

            var node = _databaseManager.GetNode(name, false);
            if (node == null)
                return false;

            value = node.GetValue32();
            return true;
        }

        private List<TradeEntry> ReadTradeList()
        {
            var list = new List<TradeEntry>();

            if (_databaseManager == null)
                return list;

            for (byte i = 0; i < NbSlotPerPage; i++)
            {
                var prefix = $"SERVER:TRADING:{i}:";

                if (!TryGetProp(prefix + "SHEET", out var sheetIdLong))
                    continue;

                var sheetId = (uint)sheetIdLong;
                if (sheetId == 0)
                    continue;

                TryGetProp(prefix + "QUALITY", out var quality);
                TryGetProp(prefix + "QUANTITY", out var quantity);
                TryGetProp(prefix + "PRICE", out var price);
                TryGetProp(prefix + "CURRENCY", out var currency);
                TryGetProp(prefix + "SLOT_TYPE", out var slotType);
                TryGetProp(prefix + "SELLER_TYPE", out var sellerType);
                TryGetProp(prefix + "FACTION_TYPE", out var factionType);
                TryGetProp(prefix + "NAME_ID", out var nameId);
                TryGetProp(prefix + "VENDOR_NAME_ID", out var vendorNameId);
                TryGetProp(prefix + "PREREQUISIT_VALID", out var prerequisitValid);

                var entry = new TradeEntry
                {
                    Index = i,
                    SheetId = sheetId,
                    Quality = (uint)quality,
                    Quantity = (uint)quantity,
                    Price = (uint)price,
                    Currency = (uint)currency,
                    SlotType = (uint)slotType,
                    SellerType = (uint)sellerType,
                    FactionType = (uint)factionType,
                    NameId = (uint)nameId,
                    VendorNameId = (uint)vendorNameId,
                    PrerequisitValid = prerequisitValid != 0
                };

                list.Add(entry);
            }

            return list;
        }

        #endregion

        #region Mission list (database branch SERVER:CHOOSE_MISSIONS)

        /// <inheritdoc/>
        public uint MissionPageId
        {
            get { return _databaseManager != null && TryGetProp("SERVER:CHOOSE_MISSIONS:PAGE_ID", out var v) ? (uint)v : 0u; }
        }

        /// <inheritdoc/>
        public bool MissionHasNextPage
        {
            get
            {
                return _databaseManager != null
                    && TryGetProp("SERVER:CHOOSE_MISSIONS:HAS_NEXT", out var v) && v != 0;
            }
        }

        /// <inheritdoc/>
        public List<MissionEntry> GetMissionList()
        {
            return ReadMissionList();
        }

        /// <inheritdoc/>
        public bool SendStartChooseMission()
        {
            // Same session scheme as the trade session: the counter is
            // incremented on each session start and sent to the server, which
            // echoes it into SERVER:CHOOSE_MISSIONS:SESSION.
            _missionSessionCounter++;
            CurrentMissionSessionId = _missionSessionCounter;

            // BOTCHAT:START_CHOOSE_MISSION: u16 session id
            return _client.GetNetworkManager().SendImpulse("BOTCHAT:START_CHOOSE_MISSION", w =>
            {
                w.U16(CurrentMissionSessionId);
            });
        }

        /// <inheritdoc/>
        public bool SendNextMissionPage()
        {
            if (CurrentMissionSessionId == 0)
                return false;

            // BOTCHAT:NEXT_PAGE_MISSION: u16 session id
            return _client.GetNetworkManager().SendImpulse("BOTCHAT:NEXT_PAGE_MISSION", w =>
            {
                w.U16(CurrentMissionSessionId);
            });
        }

        /// <inheritdoc/>
        public bool SendPickMission(byte index)
        {
            if (CurrentMissionSessionId == 0)
                return false;

            if (index >= NbSlotPerPage)
            {
                _client.GetLogger().Warn($"BotChat: SendPickMission failed, index {index} out of range.");
                return false;
            }

            // BOTCHAT:PICK_MISSION: u8 index (no session id on the wire)
            return _client.GetNetworkManager().SendImpulse("BOTCHAT:PICK_MISSION", w =>
            {
                w.U8(index);
            });
        }

        private List<MissionEntry> ReadMissionList()
        {
            var list = new List<MissionEntry>();

            if (_databaseManager == null)
                return list;

            for (byte i = 0; i < NbSlotPerPage; i++)
            {
                var prefix = $"SERVER:CHOOSE_MISSIONS:{i}:";

                if (!TryGetProp(prefix + "TEXT", out var textIdLong))
                    continue;

                var textId = (uint)textIdLong;
                if (textId == 0)
                    continue;

                TryGetProp(prefix + "ICON", out var iconId);
                TryGetProp(prefix + "DETAIL_TEXT", out var detailTextId);
                TryGetProp(prefix + "PREREQ_STATE", out var prereqState);

                var entry = new MissionEntry
                {
                    Index = i,
                    IconId = (uint)iconId,
                    TextId = textId,
                    DetailTextId = (uint)detailTextId,
                    PrereqState = (byte)prereqState
                };

                list.Add(entry);
            }

            return list;
        }

        #endregion
    }
}
