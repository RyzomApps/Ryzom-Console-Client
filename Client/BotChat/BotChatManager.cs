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
                OnTradeListUpdated?.Invoke();
                return;
            }

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
            _tradeSessionCounter++;
            if (_tradeSessionCounter == 0)
                _tradeSessionCounter = 1;
            CurrentSessionId = _tradeSessionCounter;

            // The server does not clear the whole SERVER:TRADING branch on a
            // new session; slots not refilled would keep stale entries from
            // the previous session (e.g. when switching to a non-vendor bot
            // or to a shop with fewer items). Clear it before requesting.
            ClearTradeList();

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
                CurrentSessionId = 0;
            return sent;
        }

        #endregion

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
        /// Reset all SERVER:TRADING slots (SHEET = 0) so ReadTradeList does
        /// not report stale entries from a previous trade session.
        /// </summary>
        public void ClearTradeList()
        {
            var db = _client.GetDatabaseManager();
            if (db == null)
                return;

            for (byte i = 0; i < NbSlotPerPage; i++)
            {
                var leaf = db.GetServerNode($"SERVER:TRADING:{i}:SHEET", true);
                leaf?.SetValue64(0);
            }
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
    }
}
