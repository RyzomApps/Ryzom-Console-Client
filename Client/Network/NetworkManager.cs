﻿///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using API.Chat;
using API.Entity;
using API.Network;
using Client.Chat;
using Client.Client;
using Client.Database;
using Client.Entity;
using Client.Messages;
using Client.Network.Action;
using Client.Phrase;
using Client.Property;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using System.Threading;
using Client.Config;
using Client.Sheet;
using Client.Stream;
using Client.Strings;
using Client.Inventory;
using System.Drawing;

namespace Client.Network
{
    /// <summary>
    /// used to control the connection and implements the impulse callbacks from the connection
    /// </summary>
    public class NetworkManager : INetworkManager
    {
        public bool ServerReceivedReady;

        public byte PlayerSelectedSlot = 0;
        private byte _serverPeopleActive = 255;
        private byte _serverCareerActive = 255;

        public readonly List<CharacterSummary> CharacterSummaries = [];
        public bool WaitServerAnswer;

        public string PlayerSelectedHomeShardName { get; set; } = "";
        public string PlayerSelectedHomeShardNameWithParenthesis = "";

        public bool GameExit;

        public bool FreeTrial { get; set; }

        public List<string> ShardNames { get; set; } = [];

        public List<MainlandSummary> Mainlands { get; set; } = [];

        public string UserPrivileges { get; set; } = "";

        public bool CharNameValid { get; set; }

        public int MainlandSelected { get; set; }

        public bool CharNameValidArrived { get; set; }

        /// <summary>
        /// client is ready for the selection of the character - non ryzom variable (for workarounds)
        /// </summary>
        public bool CanSendCharSelection;

        private readonly NetworkConnection _networkConnection;
        private readonly StringManager _stringManager;
        private readonly DatabaseManager _databaseManager;
        private readonly RyzomClient _client;
        private readonly GenericMessageHeaderManager _messageHeaderManager;
        private readonly ChatManager _chatManager;
        private readonly PhraseManager _phraseManager;
        private readonly SheetIdFactory _sheetIdFactory;
        private readonly InventoryManager _inventoryManager;

        /// <summary>
        /// season
        /// </summary>
        private byte _serverSeasonValue;

        /// <summary>
        /// manages entities and shapes instances
        /// </summary>
        private readonly EntityManager _entitiesManager;

        /// <summary>
        /// char play time in seconds
        /// </summary>
        public int CharPlayedTime;

        public GenericMessageHeaderManager GetMessageHeaderManager() => _messageHeaderManager;

        public EntityManager GetEntityManager() => _entitiesManager;

        /// <inheritdoc />
        public IEntityManager GetApiEntityManager() => _entitiesManager;

        public ChatManager GetChatManager() => _chatManager;

        public NetworkConnection GetNetworkConnection() => _networkConnection;

        public StringManager GetStringManager() => _stringManager;

        public DatabaseManager GetDatabaseManager() => _databaseManager;

        private InventoryManager GetInventoryManager() => _inventoryManager;

        /// <inheritdoc />
        public uint GetCurrentServerTick() => _networkConnection.GetCurrentServerTick();

        /// <inheritdoc />
        public double[] GetTps() => _networkConnection.GetTps();

        private readonly Random _random = new Random();

        /// <summary>
        /// Constructor
        /// </summary>
        public NetworkManager(RyzomClient client)
        {
            _networkConnection = client.GetNetworkConnection();
            _stringManager = client.GetStringManager();
            _databaseManager = client.GetDatabaseManager();
            _phraseManager = client.GetPhraseManager();
            _sheetIdFactory = client.GetSheetIdFactory();
            _inventoryManager = client.GetInventoryManager();

            _messageHeaderManager = new GenericMessageHeaderManager();
            _chatManager = new ChatManager(this);
            _entitiesManager = new EntityManager(client);

            _client = client;
        }

        /// <summary>
        /// Send - updates when packets were received
        /// </summary>
        public void Send(uint gameCycle)
        {
            // wait till next server is received
            if (_networkConnection.LastSentCycle >= gameCycle)
            {
                while (_networkConnection.LastSentCycle >= gameCycle)
                {
                    // Update network.
                    Update();

                    // Send dummy info
                    Send();

                    // Do not take all the CPU.
                    Thread.Sleep(10);

                    gameCycle = _networkConnection.GetCurrentServerTick();
                }
            }

            _networkConnection.Send(gameCycle);
        }

        /// <summary>
        /// Buffers a BitMemoryStream, that will be converted into a generic action, to be sent later to the server (at next update).
        /// </summary>
        public void Push(BitMemoryStream msg)
        {
            _networkConnection.Push(msg);
        }

        /// <summary>
        /// Buffers a pick-up action
        /// </summary>
        public void PushPickup(byte slot, TargettingType lootOrHarvest)
        {
            _networkConnection.PushTarget(slot, lootOrHarvest);
        }

        /// <summary>
        /// Updates the whole connection with the front end. Call this method periodically.
        /// </summary>
        /// <returns>'true' if data were sent/received.</returns>
        public bool Update()
        {
            // Update the base class
            var result = _networkConnection.Update();

            // Get changes with the update
            var changes = _networkConnection.GetChanges();

            // Manage changes
            foreach (var change in changes)
            {
                // Update a property
                if (change.Property < (byte)PropertyType.AddNewEntity)
                {
                    // Update the visual property for the slot
                    _entitiesManager.UpdateVisualProperty(change.GameCycle, change.ShortId, change.Property, change.PositionInfo.PredictedInterval);
                }
                else
                {
                    switch (change.Property)
                    {
                        // Add New Entity (and remove the old one in the slot)
                        case (byte)PropertyType.AddNewEntity:
                            // Remove the old entity
                            _entitiesManager.Remove(change.ShortId, false);

                            // Create the new entity
                            if (_entitiesManager.Create(change.ShortId, _networkConnection.GetPropertyDecoder().GetSheetFromEntity(change.ShortId), change.NewEntityInfo) == null)
                            {
                                _client.GetLogger().Warn($"CNetManager::update : entity in the slot '{change.ShortId}' has not been created.");
                            }

                            break;

                        // Delete an entity
                        case (byte)PropertyType.RemoveOldEntity:
                            _client.GetLogger().Debug($"CNetManager::remove old entity : {(_entitiesManager.GetEntity(change.ShortId) != null ? _entitiesManager.GetEntity(change.ShortId).GetDisplayName().Trim() : "unnamed")} id {change.ShortId}");
                            _entitiesManager.Remove(change.ShortId, true);
                            break;

                        // Lag detected
                        case (byte)PropertyType.LagDetected:
                            _client.GetLogger().Debug("CNetManager::update : Lag detected.");
                            break;

                        // Probe received
                        case (byte)PropertyType.ProbeReceived:
                            _client.GetLogger().Debug("CNetManager::update : Probe Received.");
                            break;

                        // Connection ready
                        case (byte)PropertyType.ConnectionReady:
                            _client.GetLogger().Debug("CNetManager::update : Connection Ready.");
                            break;

                        // Property unknown
                        default:
                            _client.GetLogger().Warn($"CNetManager::update : The property '{change.Property}' is unknown.");
                            break;
                    }
                }
            }

            // Clear all changes.
            _networkConnection.ClearChanges();

            _chatManager.FlushBuffer(_client);

            return result;
        }

        /// <summary>
        /// Send updates
        /// </summary>
        public void Send()
        {
            _networkConnection.Send();
        }

        /// <summary>
        /// The Impulse callback to receive all messages from the front end.
        /// </summary>
        public void ImpulseCallBack(BitMemoryStream impulse)
        {
            _messageHeaderManager.Execute(impulse);
        }

        /// <summary>
        /// Initialize Network
        /// </summary>
        public void InitializeNetwork()
        {
            _messageHeaderManager.SetCallback("DB_UPD_PLR", ImpulseDatabaseUpdatePlayer);
            _messageHeaderManager.SetCallback("DB_INIT:PLR", ImpulseDatabaseInitPlayer);
            _messageHeaderManager.SetCallback("DB_UPD_INV", ImpulseUpdateInventory);
            _messageHeaderManager.SetCallback("DB_INIT:INV", ImpulseInitInventory);

            _messageHeaderManager.SetCallback("DB_GROUP:UPDATE_BANK", ImpulseDatabaseUpdateBank);
            _messageHeaderManager.SetCallback("DB_GROUP:INIT_BANK", ImpulseDatabaseInitBank);
            _messageHeaderManager.SetCallback("DB_GROUP:RESET_BANK", ImpulseDatabaseResetBank);

            _messageHeaderManager.SetCallback("CONNECTION:NO_USER_CHAR", ImpulseNoUserChar);
            _messageHeaderManager.SetCallback("CONNECTION:USER_CHARS", ImpulseUserChars);
            _messageHeaderManager.SetCallback("CONNECTION:USER_CHAR", ImpulseUserChar);
            _messageHeaderManager.SetCallback("CONNECTION:FAR_TP", ImpulseFarTp);
            _messageHeaderManager.SetCallback("CONNECTION:READY", ImpulseServerReady);
            _messageHeaderManager.SetCallback("CONNECTION:VALID_NAME", ImpulseCharNameValid);
            _messageHeaderManager.SetCallback("CONNECTION:SHARD_ID", ImpulseShardId);
            _messageHeaderManager.SetCallback("CONNECTION:SERVER_QUIT_OK", ImpulseServerQuitOk);
            _messageHeaderManager.SetCallback("CONNECTION:SERVER_QUIT_ABORT", ImpulseServerQuitAbort);
            _messageHeaderManager.SetCallback("CONNECTION:MAIL_AVAILABLE", ImpulseMailNotification);
            _messageHeaderManager.SetCallback("CONNECTION:GUILD_MESSAGE_AVAILABLE", ImpulseForumNotification);
            _messageHeaderManager.SetCallback("CONNECTION:PERMANENT_BAN", ImpulsePermanentBan);
            _messageHeaderManager.SetCallback("CONNECTION:UNBAN", ImpulsePermanentUnban);

            _messageHeaderManager.SetCallback("STRING:CHAT", ImpulseChat);
            _messageHeaderManager.SetCallback("STRING:TELL", ImpulseTell);
            _messageHeaderManager.SetCallback("STRING:FAR_TELL", ImpulseFarTell);
            _messageHeaderManager.SetCallback("STRING:CHAT2", ImpulseChat2);
            _messageHeaderManager.SetCallback("STRING:DYN_STRING", ImpulseDynString);
            _messageHeaderManager.SetCallback("STRING:DYN_STRING_GROUP", ImpulseDynStringInChatGroup);
            _messageHeaderManager.SetCallback("STRING:TELL2", ImpulseTell2);

            _messageHeaderManager.SetCallback("TP:DEST", ImpulseTp);
            _messageHeaderManager.SetCallback("TP:DEST_WITH_SEASON", ImpulseTpWithSeason);
            _messageHeaderManager.SetCallback("TP:CORRECT", ImpulseCorrectPos);

            _messageHeaderManager.SetCallback("COMBAT:ENGAGE_FAILED", ImpulseCombatEngageFailed);

            _messageHeaderManager.SetCallback("BOTCHAT:DYNCHAT_OPEN", ImpulseDynChatOpen);
            _messageHeaderManager.SetCallback("BOTCHAT:DYNCHAT_CLOSE", ImpulseDynChatClose);

            _messageHeaderManager.SetCallback("CASTING:BEGIN", ImpulseBeginCast);

            _messageHeaderManager.SetCallback("TEAM:INVITATION", ImpulseTeamInvitation);
            _messageHeaderManager.SetCallback("TEAM:SHARE_OPEN", ImpulseTeamShareOpen);
            _messageHeaderManager.SetCallback("TEAM:SHARE_INVALID", ImpulseTeamShareInvalid);
            _messageHeaderManager.SetCallback("TEAM:SHARE_CLOSE", ImpulseTeamShareClose);
            _messageHeaderManager.SetCallback("TEAM:CONTACT_INIT", ImpulseTeamContactInit);
            _messageHeaderManager.SetCallback("TEAM:CONTACT_CREATE", ImpulseTeamContactCreate);
            _messageHeaderManager.SetCallback("TEAM:CONTACT_STATUS", ImpulseTeamContactStatus);
            _messageHeaderManager.SetCallback("TEAM:CONTACT_REMOVE", ImpulseTeamContactRemove);

            _messageHeaderManager.SetCallback("EXCHANGE:INVITATION", ImpulseExchangeInvitation);
            _messageHeaderManager.SetCallback("EXCHANGE:CLOSE_INVITATION", ImpulseExchangeCloseInvitation);

            _messageHeaderManager.SetCallback("ANIMALS:MOUNT_ABORT", ImpulseMountAbort);

            _messageHeaderManager.SetCallback("DEBUG:REPLY_WHERE", ImpulseWhere);
            _messageHeaderManager.SetCallback("DEBUG:COUNTER", ImpulseCounter);

            _messageHeaderManager.SetCallback("STRING_MANAGER:PHRASE_SEND", ImpulsePhraseSend);
            _messageHeaderManager.SetCallback("STRING_MANAGER:STRING_RESP", ImpulseStringResp);
            _messageHeaderManager.SetCallback("STRING_MANAGER:RELOAD_CACHE", ImpulseReloadCache);

            _messageHeaderManager.SetCallback("BOTCHAT:FORCE_END", ImpulseBotChatForceEnd);

            _messageHeaderManager.SetCallback("JOURNAL:INIT_COMPLETED_MISSIONS", ImpulseJournalInitCompletedMissions);
            _messageHeaderManager.SetCallback("JOURNAL:UPDATE_COMPLETED_MISSIONS", ImpulseJournalUpdateCompletedMissions);
            _messageHeaderManager.SetCallback("JOURNAL:ADD_COMPASS", ImpulseJournalAddCompass);
            _messageHeaderManager.SetCallback("JOURNAL:REMOVE_COMPASS", ImpulseJournalRemoveCompass);

            _messageHeaderManager.SetCallback("GUILD:JOIN_PROPOSAL", ImpulseGuildJoinProposal);
            _messageHeaderManager.SetCallback("GUILD:ASCENSOR", ImpulseGuildAscensor);
            _messageHeaderManager.SetCallback("GUILD:LEAVE_ASCENSOR", ImpulseGuildLeaveAscensor);
            _messageHeaderManager.SetCallback("GUILD:ABORT_CREATION", ImpulseGuildAbortCreation);
            _messageHeaderManager.SetCallback("GUILD:OPEN_GUILD_WINDOW", ImpulseGuildOpenGuildWindow);
            _messageHeaderManager.SetCallback("GUILD:OPEN_INVENTORY", ImpulseGuildOpenInventory);
            _messageHeaderManager.SetCallback("GUILD:CLOSE_INVENTORY", ImpulseGuildCloseInventory);
            _messageHeaderManager.SetCallback("GUILD:UPDATE_PLAYER_TITLE", ImpulseGuildUpdatePlayerTitle);
            _messageHeaderManager.SetCallback("GUILD:USE_FEMALE_TITLES", ImpulseGuildUseFemaleTitles);

            _messageHeaderManager.SetCallback("HARVEST:CLOSE_TEMP_INVENTORY", ImpulseCloseTempInv);

            _messageHeaderManager.SetCallback("COMMAND:REMOTE_ADMIN", ImpulseRemoteAdmin);

            _messageHeaderManager.SetCallback("PHRASE:DOWNLOAD", ImpulsePhraseDownload);
            _messageHeaderManager.SetCallback("PHRASE:CONFIRM_BUY", ImpulsePhraseConfirmBuy);
            _messageHeaderManager.SetCallback("PHRASE:EXEC_CYCLIC_ACK", ImpulsePhraseAckExecuteCyclic);
            _messageHeaderManager.SetCallback("PHRASE:EXEC_NEXT_ACK", ImpulsePhraseAckExecuteNext);

            _messageHeaderManager.SetCallback("ITEM_INFO:SET", ImpulseItemInfoSet);
            _messageHeaderManager.SetCallback("ITEM_INFO:REFRESH_VERSION", ImpulseItemInfoRefreshVersion);
            _messageHeaderManager.SetCallback("ITEM:OPEN_ROOM_INVENTORY", ImpulseItemOpenRoomInventory);
            _messageHeaderManager.SetCallback("ITEM:CLOSE_ROOM_INVENTORY", ImpulseItemCloseRoomInventory);

            _messageHeaderManager.SetCallback("MISSION_PREREQ:SET", ImpulsePrereqInfoSet);

            _messageHeaderManager.SetCallback("DEATH:RESPAWN_POINT", ImpulseDeathRespawnPoint);
            _messageHeaderManager.SetCallback("DEATH:RESPAWN", ImpulseDeathRespawn);

            _messageHeaderManager.SetCallback("DUEL:INVITATION", ImpulseDuelInvitation);
            _messageHeaderManager.SetCallback("DUEL:CANCEL_INVITATION", ImpulseDuelCancelInvitation);

            _messageHeaderManager.SetCallback("PVP_CHALLENGE:INVITATION", ImpulsePvpChallengeInvitation);
            _messageHeaderManager.SetCallback("PVP_CHALLENGE:CANCEL_INVITATION", ImpulsePvpChallengeCancelInvitation);

            _messageHeaderManager.SetCallback("PVP_FACTION:PUSH_FACTION_WAR", ImpulsePvpFactionPushFactionWar);
            _messageHeaderManager.SetCallback("PVP_FACTION:POP_FACTION_WAR", ImpulsePvpFactionPopFactionWar);
            _messageHeaderManager.SetCallback("PVP_FACTION:FACTION_WARS", ImpulsePvpFactionFactionWars);

            _messageHeaderManager.SetCallback("ENCYCLOPEDIA:UPDATE", ImpulseEncyclopediaUpdate);
            _messageHeaderManager.SetCallback("ENCYCLOPEDIA:INIT", ImpulseEncyclopediaInit);

            _messageHeaderManager.SetCallback("USER:BARS", ImpulseUserBars);
            _messageHeaderManager.SetCallback("USER:POPUP", ImpulseUserPopup);

            _messageHeaderManager.SetCallback("MISSION:ASK_ENTER_CRITICAL", ImpulseEnterCrZoneProposal);
            _messageHeaderManager.SetCallback("MISSION:CLOSE_ENTER_CRITICAL", ImpulseCloseEnterCrZoneProposal);

            // Module gateway message
            _messageHeaderManager.SetCallback("MODULE_GATEWAY:FEOPEN", CbImpulsionGatewayOpen);
            _messageHeaderManager.SetCallback("MODULE_GATEWAY:GATEWAY_MSG", CbImpulsionGatewayMessage);
            _messageHeaderManager.SetCallback("MODULE_GATEWAY:FECLOSE", CbImpulsionGatewayClose);

            _messageHeaderManager.SetCallback("OUTPOST:CHOOSE_SIDE", ImpulseOutpostChooseSide);
            _messageHeaderManager.SetCallback("OUTPOST:DECLARE_WAR_ACK", ImpulseOutpostDeclareWarAck);

            _messageHeaderManager.SetCallback("COMBAT:FLYING_HP_DELTA", ImpulseCombatFlyingHpDelta);
            _messageHeaderManager.SetCallback("COMBAT:FLYING_TEXT_ISE", ImpulseCombatFlyingTextItemSpecialEffectProc);
            _messageHeaderManager.SetCallback("COMBAT:FLYING_TEXT", ImpulseCombatFlyingText);

            _messageHeaderManager.SetCallback("SEASON:SET", ImpulseSetSeason);

            _messageHeaderManager.SetCallback("RING_MISSION:DSS_DOWN", ImpulseDssDown);

            _messageHeaderManager.SetCallback("NPC_ICON:SET_DESC", ImpulseSetNpcIconDesc);
            _messageHeaderManager.SetCallback("NPC_ICON:SVR_EVENT_MIS_AVL", ImpulseServerEventForMissionAvailability);
            _messageHeaderManager.SetCallback("NPC_ICON:SET_TIMER", ImpulseSetNpcIconTimer);
        }

        private void ImpulseSetNpcIconTimer(BitMemoryStream impulse)
        {
            _client.GetLogger().Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private void ImpulseServerEventForMissionAvailability(BitMemoryStream impulse)
        {
            _client.GetLogger().Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private void ImpulseSetNpcIconDesc(BitMemoryStream impulse)
        {
            _client.GetLogger().Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private void ImpulseDssDown(BitMemoryStream impulse)
        {
            _client.GetLogger().Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private void ImpulseSetSeason(BitMemoryStream impulse)
        {
            _client.GetLogger().Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        enum TCombatFlyingText
        {
            None = 0,
            TargetDodge,
            TargetParry,
            TargetEvade,
            SelfEvade,
            TargetResist,
            SelfResist,
            SelfInterrupt,
            SelfFailure
        };

        private void ImpulseCombatFlyingHpDelta(BitMemoryStream impulse)
        {
            uint entityID = 0;
            uint rgba = 0;
            short hpDelta = 0;

            impulse.Serial(ref entityID);
            impulse.Serial(ref rgba);
            impulse.Serial(ref hpDelta);

            var color = Color.FromArgb((byte)(rgba >> 24 & 255), (byte)(rgba >> 16 & 255), (byte)(rgba >> 8 & 255), (byte)(rgba & 255));

            _client.Plugins.OnCombatFlyingHpDelta(entityID, color.ToArgb(), hpDelta);
        }

        private void ImpulseCombatFlyingTextItemSpecialEffectProc(BitMemoryStream impulse)
        {
            uint entityID = 0;
            uint rgba = 0;
            byte effect = 0;
            int param = 0;

            impulse.Serial(ref entityID);
            impulse.Serial(ref rgba);
            impulse.Serial(ref effect);
            impulse.Serial(ref param);

            var color = Color.FromArgb((byte)(rgba >> 24 & 255), (byte)(rgba >> 16 & 255), (byte)(rgba >> 8 & 255), (byte)(rgba & 255));

            _client.Plugins.OnCombatFlyingTextItemSpecialEffectProc(entityID, color.ToArgb(), effect, param);
        }

        private void ImpulseCombatFlyingText(BitMemoryStream impulse)
        {
            uint entityID = 0;
            byte tmp = 0;

            impulse.Serial(ref entityID);
            impulse.Serial(ref tmp);
            var type = (TCombatFlyingText)tmp;

            var color =  Color.FromArgb(255, 255, 255);
            string text = "";
            //float dt = 0.0f;

            switch (type)
            {
                case TCombatFlyingText.TargetDodge:
                    color = Color.FromArgb(255, 128, 64);
                    break;
                case TCombatFlyingText.TargetParry:
                    color = Color.FromArgb(255, 128, 64);
                    break;
                case TCombatFlyingText.TargetEvade:
                    color = Color.FromArgb(255, 128, 64);
                    break;
                case TCombatFlyingText.SelfEvade:
                    color = Color.FromArgb(255, 255, 0);
                    break;
                case TCombatFlyingText.TargetResist:
                    color = Color.FromArgb(255, 128, 64);
                    break;
                case TCombatFlyingText.SelfResist:
                    color = Color.FromArgb(255, 255, 0);
                    break;
                case TCombatFlyingText.SelfInterrupt:
                    color = Color.FromArgb(200, 0, 0);
                    break;
                case TCombatFlyingText.SelfFailure:
                    color = Color.FromArgb(200, 0, 0);
                    break;
                default:
                    _client.GetLogger().Warn("Bad type for COMBAT_FLYING_TEXT:TCombatFlyingText enum");
                    break;
            }

            _client.Plugins.OnCombatFlyingText(entityID, color.ToArgb(), (byte)type);
        }

        private void CbImpulsionGatewayMessage(BitMemoryStream impulse)
        {
            _client.GetLogger().Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private void ImpulseOutpostDeclareWarAck(BitMemoryStream impulse)
        {
            _client.GetLogger().Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private void ImpulseOutpostChooseSide(BitMemoryStream impulse)
        {
            _client.GetLogger().Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private void CbImpulsionGatewayClose(BitMemoryStream impulse)
        {
            _client.GetLogger().Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private void CbImpulsionGatewayOpen(BitMemoryStream impulse)
        {
            _client.GetLogger().Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private void ImpulseCloseEnterCrZoneProposal(BitMemoryStream impulse)
        {
            _client.GetLogger().Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private void ImpulseEnterCrZoneProposal(BitMemoryStream impulse)
        {
            _client.GetLogger().Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private void ImpulseUserPopup(BitMemoryStream impulse)
        {
            uint titleTextId = 0;
            uint docTextId = 0;
            impulse.Serial(ref titleTextId);
            impulse.Serial(ref docTextId);

            // setup TEMP DB for title
            //var pIM = _client.GetInterfaceManager();
            var node = _client.GetDatabaseManager().GetServerNode("UI:TEMP:SERVER_POPUP:TITLE");

            node?.SetValue32((int)titleTextId);

            _client.GetStringManager().WaitDynString(titleTextId, (a, b) => { _client.GetLogger().Info($"{a} {b}"); }, this);
            _client.GetStringManager().WaitDynString(docTextId, (a, b) => { _client.GetLogger().Info($"{a} {b}"); }, this);

            //_client.GetLogger().Info($"titleTextId {titleTextId} docTextId {docTextId}");

            // Open the Popup only when the 2 dyn strings are available
            //ServerMessageBoxOnReceiveTextId.startWaitTexts(titleTextId, docTextId);
        }

        /// <summary>
        /// update user bars (stats)
        /// </summary>
        private void ImpulseUserBars(BitMemoryStream impulse)
        {
            _client.GetLogger().Debug($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");

            byte msgNumber = 0;
            var hp = 0;
            var sap = 0;
            var sta = 0;
            var focus = 0;

            impulse.Serial(ref msgNumber);
            impulse.Serial(ref hp);
            impulse.Serial(ref sap);
            impulse.Serial(ref sta);
            impulse.Serial(ref focus);

            _client.Plugins.OnUserBars(msgNumber, hp, sap, sta, focus);
        }

        private void ImpulseEncyclopediaInit(BitMemoryStream impulse)
        {
            _client.GetLogger().Debug($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");

            _client.Plugins.OnEncyclopediaInit();
        }

        private void ImpulseEncyclopediaUpdate(BitMemoryStream impulse)
        {
            _client.GetLogger().Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private void ImpulsePvpFactionFactionWars(BitMemoryStream impulse)
        {
            _client.GetLogger().Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private void ImpulsePvpFactionPopFactionWar(BitMemoryStream impulse)
        {
            _client.GetLogger().Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private void ImpulsePvpFactionPushFactionWar(BitMemoryStream impulse)
        {
            _client.GetLogger().Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private void ImpulsePvpChallengeCancelInvitation(BitMemoryStream impulse)
        {
            _client.GetLogger().Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private void ImpulsePvpChallengeInvitation(BitMemoryStream impulse)
        {
            _client.GetLogger().Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private void ImpulseDuelCancelInvitation(BitMemoryStream impulse)
        {
            _client.GetLogger().Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private void ImpulseDuelInvitation(BitMemoryStream impulse)
        {
            _client.GetLogger().Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private void ImpulseDeathRespawn(BitMemoryStream impulse)
        {
            _client.GetLogger().Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        /// <summary>
        /// Respawn point set
        /// </summary>
        private void ImpulseDeathRespawnPoint(BitMemoryStream impulse)
        {
            _client.GetLogger().Debug($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");

            short x = 0;
            short y = 0;

            impulse.Serial(ref x);
            impulse.Serial(ref y);

            _client.Plugins.OnDeathRespawnPoint(x, y);
        }

        private void ImpulseItemCloseRoomInventory(BitMemoryStream impulse)
        {
            _client.GetLogger().Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private void ImpulseItemOpenRoomInventory(BitMemoryStream impulse)
        {
            _client.GetLogger().Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private void ImpulseItemInfoRefreshVersion(BitMemoryStream impulse)
        {
            _client.GetLogger().Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private void ImpulsePrereqInfoSet(BitMemoryStream impulse)
        {
            _client.GetLogger().Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }


        private void ImpulseItemInfoSet(BitMemoryStream impulse)
        {
            _client.GetLogger().Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        /// <summary>
        /// server confirm/infirm the execution of a phrase
        /// </summary>
        private void ImpulsePhraseAckExecuteNext(BitMemoryStream impulse)
        {
            _client.GetLogger().Debug($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");

            byte counter = 0;
            var ok = false;

            impulse.Serial(ref ok);
            impulse.Serial(ref counter);

            _client.Plugins.OnPhraseAckExecute(false, counter, ok);
        }

        /// <summary>
        /// server confirm/infirm the cyclic execution of a phrase
        /// </summary>
        private void ImpulsePhraseAckExecuteCyclic(BitMemoryStream impulse)
        {
            _client.GetLogger().Debug($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");

            byte counter = 0;
            var ok = false;

            impulse.Serial(ref ok);
            impulse.Serial(ref counter);

            _client.Plugins.OnPhraseAckExecute(true, counter, ok);
        }

        private void ImpulsePhraseConfirmBuy(BitMemoryStream impulse)
        {
            _client.GetLogger().Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        /// <summary>
        /// server upload the phrases
        /// </summary>
        private void ImpulsePhraseDownload(BitMemoryStream impulse)
        {
            // Read Known Phrases
            #region workaround for: impulse.serialCont(phrases);
            var len = 0;
            impulse.Serial(ref len);

            var phrases = new List<PhraseSlot>(len);

            for (var i = 0; i < len; i++)
            {
                var value = PhraseSlot.Serial(impulse, _sheetIdFactory);
                phrases.Add(value);
            }
            #endregion end workaround

            foreach (var phrase in phrases)
            {
                if (phrase.PhraseSheetId.AsInt() != 0)
                {
                    var phraseCom = new PhraseCom();
                    _phraseManager.BuildPhraseFromSheet(ref phraseCom, phrase.PhraseSheetId.AsInt());
                    _phraseManager.SetPhraseNoUpdateDb(phrase.KnownSlot, phraseCom);
                }
                else
                {
                    _phraseManager.SetPhraseNoUpdateDb(phrase.KnownSlot, phrase.Phrase);
                }
            }

            // must update the DB (NB: if initInGameDone) after all phrase set.
            _phraseManager.UpdateBookDb();

            // Then Read Memorized Phrases
            // workaround for: impulse.SerialCont(memorizedPhrases);
            len = 0;
            impulse.Serial(ref len);
            var memorizedPhrases = new List<PhraseMemorySlot>(len);

            for (var i = 0; i < len; i++)
            {
                var value = PhraseMemorySlot.Serial(impulse);
                memorizedPhrases.Add(value);
            }
            // end workaround

            foreach (var phrase in memorizedPhrases)
            {
                _phraseManager.MemorizePhrase(phrase.MemoryLineId, phrase.MemorySlotId, phrase.PhraseId);
            }

            // OK.
            _client.SabrinaPhraseBookLoaded = true;

            // update gray state, if game inited.
            _phraseManager.UpdateMemoryBar();

            _client.Plugins.OnPhraseDownLoad(phrases, memorizedPhrases);
        }

        private void ImpulseRemoteAdmin(BitMemoryStream impulse)
        {
            _client.GetLogger().Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private void ImpulseCloseTempInv(BitMemoryStream impulse)
        {
            _client.GetLogger().Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        /// <summary>
        /// server activates/deactivates use of female titles
        /// </summary>
        private void ImpulseGuildUseFemaleTitles(BitMemoryStream impulse)
        {
            _client.GetLogger().Debug($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");

            var useFemaleTitles = false;
            impulse.Serial(ref useFemaleTitles);

            _client.Plugins.OnGuildUseFemaleTitles(useFemaleTitles);
        }

        /// <summary>
        /// server block/unblock some reserved titles
        /// </summary>
        private void ImpulseGuildUpdatePlayerTitle(BitMemoryStream impulse)
        {
            _client.GetLogger().Debug($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");

            var bUnblock = false;
            impulse.Serial(ref bUnblock);

            var len = 0;
            impulse.Serial(ref len);

            var vTitles = new List<ushort>(len);

            for (var i = 0; i < len; i++)
            {
                byte value = 0;
                impulse.Serial(ref value);
                vTitles.Add(value);
            }

            _client.Plugins.OnGuildUpdatePlayerTitle(bUnblock, len, vTitles);
        }

        private void ImpulseGuildCloseInventory(BitMemoryStream impulse)
        {
            _client.GetLogger().Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private void ImpulseGuildOpenInventory(BitMemoryStream impulse)
        {
            _client.GetLogger().Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private void ImpulseGuildOpenGuildWindow(BitMemoryStream impulse)
        {
            _client.GetLogger().Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private void ImpulseGuildAbortCreation(BitMemoryStream impulse)
        {
            _client.GetLogger().Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private void ImpulseGuildLeaveAscensor(BitMemoryStream impulse)
        {
            _client.GetLogger().Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private void ImpulseGuildAscensor(BitMemoryStream impulse)
        {
            _client.GetLogger().Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private void ImpulseGuildJoinProposal(BitMemoryStream impulse)
        {
            _client.GetLogger().Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private void ImpulseJournalRemoveCompass(BitMemoryStream impulse)
        {
            _client.GetLogger().Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private void ImpulseJournalAddCompass(BitMemoryStream impulse)
        {
            _client.GetLogger().Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private void ImpulseJournalUpdateCompletedMissions(BitMemoryStream impulse)
        {
            _client.GetLogger().Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private void ImpulseJournalInitCompletedMissions(BitMemoryStream impulse)
        {
            _client.GetLogger().Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private void ImpulseBotChatForceEnd(BitMemoryStream impulse)
        {
            _client.GetLogger().Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        /// <summary>
        /// reload the string cache
        /// </summary>
        private void ImpulseReloadCache(BitMemoryStream impulse)
        {
            var timestamp = 0;
            impulse.Serial(ref timestamp);

            _client.GetLogger().Debug($"Impulse on {MethodBase.GetCurrentMethod()?.Name} with timestamp {timestamp}");

            _stringManager.LoadCache(timestamp);

            _client.Plugins.OnReloadCache(timestamp);
        }

        /// <summary>
        /// Update the local string set
        /// </summary>
        private void ImpulseStringResp(BitMemoryStream impulse)
        {
            uint stringId = 0;
            var strUtf8 = "";
            impulse.Serial(ref stringId);
            impulse.Serial(ref strUtf8, false);

            _client.GetLogger().Debug($"Impulse on {MethodBase.GetCurrentMethod()?.Name} with stringId {stringId}");

            _stringManager.ReceiveString(stringId, strUtf8, this);

            _client.Plugins.OnStringResp(stringId, strUtf8);
        }

        /// <summary>
        /// A dyn string (or phrase) is send (so, we receive it)
        /// </summary>
        /// <remarks>Event is fired inside the string manager</remarks>
        private void ImpulsePhraseSend(BitMemoryStream impulse)
        {
            _stringManager.ReceiveDynString(impulse, this);
        }

        private void ImpulseCounter(BitMemoryStream impulse)
        {
            _client.GetLogger().Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private void ImpulseWhere(BitMemoryStream impulse)
        {
            _client.GetLogger().Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private void ImpulseMountAbort(BitMemoryStream impulse)
        {
            _client.GetLogger().Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private void ImpulseExchangeCloseInvitation(BitMemoryStream impulse)
        {
            _client.GetLogger().Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private void ImpulseExchangeInvitation(BitMemoryStream impulse)
        {
            _client.GetLogger().Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        /// <summary>
        /// Remove a contact by the server
        /// </summary>
        private void ImpulseTeamContactRemove(BitMemoryStream impulse)
        {
            uint contactId = 0;
            byte nList = 0;

            impulse.Serial(ref contactId);
            impulse.Serial(ref nList);

            _client.Plugins.OnTeamContactRemove(contactId, nList);
        }

        /// <summary>
        /// update one of the character from the friend list
        /// </summary>
        private void ImpulseTeamContactStatus(BitMemoryStream impulse)
        {
            uint contactId = 0;
            byte state = 0;

            impulse.Serial(ref contactId);
            impulse.Serial(ref state);

            var online = (CharConnectionState)state;

            _client.Plugins.OnTeamContactStatus(contactId, online);
        }

        private void ImpulseTeamContactCreate(BitMemoryStream impulse)
        {
            uint contactId = 0;
            uint nameId = 0;
            byte state = 0;
            byte nList = 0;

            impulse.Serial(ref contactId);
            impulse.Serial(ref nameId);
            impulse.Serial(ref state);
            impulse.Serial(ref nList);

            var online = (CharConnectionState)state;

            // client patch to resolve bad server response when requesting ignore list contact creation
            if (nList == 1)     // ignore list
            {
                // prevent adding an empty player to ignore list
                if (nameId == 0) return;
            }

            _client.Plugins.OnTeamContactCreate(contactId, nameId, online, nList);
        }

        /// <summary>
        /// initialize friend list and ignore list from the contact list
        /// </summary>
        private void ImpulseTeamContactInit(BitMemoryStream impulse)
        {
            var len = 0;
            impulse.Serial(ref len);

            var vFriendListName = new List<uint>(len);

            for (var i = 0; i < len; i++)
            {
                uint value = 0;
                impulse.Serial(ref value);
                vFriendListName.Add(value);
            }

            uint nbState = 0;
            impulse.Serial(ref nbState);

            var vFriendListOnline = new List<CharConnectionState>((int)nbState);

            for (var i = 0; i < nbState; ++i)
            {
                byte state = 0;
                impulse.Serial(ref state);
                vFriendListOnline.Add((CharConnectionState)state);
            }

            len = 0;
            impulse.Serial(ref len);

            var vIgnoreListName = new List<string>();

            for (var i = 0; i < len; i++)
            {
                var value = "";
                impulse.Serial(ref value);
                vIgnoreListName.Add(value);
            }

            _client.Plugins.OnTeamContactInit(vFriendListName, vFriendListOnline, vIgnoreListName);
        }

        private void ImpulseTeamShareClose(BitMemoryStream impulse)
        {
            _client.GetLogger().Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private void ImpulseTeamShareInvalid(BitMemoryStream impulse)
        {
            _client.GetLogger().Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private void ImpulseTeamShareOpen(BitMemoryStream impulse)
        {
            _client.GetLogger().Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private void ImpulseTeamInvitation(BitMemoryStream impulse)
        {
            _client.GetLogger().Debug($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");

            var textId = uint.MinValue;
            impulse.Serial(ref textId);

            _client.Plugins.OnTeamInvitation(textId);
        }

        private void ImpulseBeginCast(BitMemoryStream impulse)
        {
            _client.GetLogger().Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private void ImpulseDynChatClose(BitMemoryStream impulse)
        {
            _client.GetLogger().Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private void ImpulseDynChatOpen(BitMemoryStream impulse)
        {
            uint botUid = 0; // Compressed Index
            uint botName = 0; // Server string

            impulse.Serial(ref botUid);
            impulse.Serial(ref botName);

            #region workaround for: impulse.SerialCont(ref DynStrs);
            var len = 0;
            impulse.Serial(ref len);

            var dynStrs = new List<uint>(); // 0 - Desc, 1 - Option0, 2 - Option1, etc....

            for (var i = 0; i < len; i++)
            {
                uint value = 0;
                impulse.Serial(ref value);
                dynStrs.Add(value);
            }
            #endregion end workaround

            var sTmp = "impulseCallback : Received BOTCHAT:DYNCHAT_OPEN BotUID:";
            sTmp += $"{botUid} BotName:";
            sTmp += $"{botName} DynStrs:";

            for (var i = 0; i < dynStrs.Count; ++i)
            {
                sTmp += dynStrs[i];
                if (i != dynStrs.Count - 1)
                {
                    sTmp += ",";
                }
            }

            _client.GetLogger().Info(sTmp);
            _client.GetLogger().Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private void ImpulseCombatEngageFailed(BitMemoryStream impulse)
        {
            _client.GetLogger().Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        /// <summary>
        /// Message from the server to correct the user position because (s)he is not at the same position on the server.
        /// </summary>
        private void ImpulseCorrectPos(BitMemoryStream impulse)
        {
            // TP:CORRECT

            var x = new int();
            var y = new int();
            var z = new int();

            impulse.Serial(ref x);
            impulse.Serial(ref y);
            impulse.Serial(ref z);

            //if (UserEntity.mode() != MBEHAV.COMBAT_FLOAT)
            //{
            if (x == 0) // Get SpeedAdjustement
            {
                //UserEntity.SetSpeedServerAdjust(-0.2f);

                _client.GetLogger().Warn("impulseCorrectPos: SetSpeedServerAdjust");
            }
            else
            {
                // Compute the destination.
                var dest = new Vector3(x / 1000.0f, y / 1000.0f, z / 1000.0f);

                _client.GetLogger().Warn($"Position error: Server relocated the user entity to {dest}.");

                if (!GetEntityManager().GetApiUserEntity().IsDead())
                {
                    // Add some Noise to get unstuck ;)
                    var noise = Vector3.Normalize(new Vector3((float)_random.NextDouble() - 0.5f, (float)_random.NextDouble() - 0.5f, 0));
                    dest += noise * 1;
                }

                // Update the position for the vision.
                _client.GetNetworkManager().SetReferencePosition(dest);

                // Change the user poisition.
                _entitiesManager.UserEntity.CorrectPos(dest);
            }
            //}
        }

        internal void SetReferencePosition(Vector3 dest)
        {
            _networkConnection.GetPropertyDecoder().SetReferencePosition(dest);
        }

        private void ImpulseTpWithSeason(BitMemoryStream impulse)
        {
            ImpulseTp(impulse, true);
            _client.GetLogger().Debug($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private void ImpulseTp(BitMemoryStream impulse)
        {
            ImpulseTp(impulse, false);
            _client.GetLogger().Debug($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        /// <summary>
        /// Message from the server to teleport the user.
        /// </summary>
        private void ImpulseTp(BitMemoryStream impulse, bool hasSeason)
        {
            var userEntity = _client.GetNetworkManager().GetEntityManager().GetApiUserEntity();

            var x = new int();
            var y = new int();
            var z = new int();
            var useHeading = new bool();

            impulse.Serial(ref x);
            impulse.Serial(ref y);
            impulse.Serial(ref z);
            impulse.Serial(ref useHeading);

            // Is there an orientation too ?
            if (useHeading)
            {
                var angle = new float();
                impulse.Serial(ref angle);

                _client.GetLogger().Debug($"impulseTP: to {x} {y} {z} {angle}");

                var ori = new Vector3((float)Math.Cos(angle), (float)Math.Sin(angle), 0f);
                Vector3.Normalize(ori);

                userEntity.Dir = ori;
                userEntity.Front = ori;
                //userEntity.SetHeadPitch(0);
            }
            else
            {
                _client.GetLogger().Debug($"impulseTP: to {x} {y} {z}");
            }

            if (hasSeason)
            {
                impulse.Serial(ref _serverSeasonValue);
            }

            // Compute the destination.
            var dest = new Vector3(x / 1000f, y / 1000f, z / 1000f);

            // Update the position for the vision.
            _client.GetNetworkManager().SetReferencePosition(dest);

            // Change the position of the entity and in Pacs.
            userEntity.Pos = dest;

            // Msg Received, send an acknowledge after the landscape has been loaded.
            var @out = new BitMemoryStream();
            if (_client.GetNetworkManager().GetMessageHeaderManager().PushNameToStream("TP:ACK", @out))
            {
                _client.GetNetworkManager().Push(@out);
                _client.GetLogger().Info("Teleport acknowledge sent.");
            }
            else
                _client.GetLogger().Warn("impulseTP: unknown message name : 'TP:ACK'.");

            _client.Plugins.OnTeleport(hasSeason);
        }

        private void ImpulseTell2(BitMemoryStream impulse)
        {
            _client.GetLogger().Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private void ImpulseDynStringInChatGroup(BitMemoryStream impulse)
        {
            _client.GetLogger().Debug($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");

            var type = 0;
            impulse.Serial(ref type);
            _chatManager.ProcessChatStringWithNoSender(impulse, (ChatGroupType)type, _client);
        }

        private void ImpulseDynString(BitMemoryStream impulse)
        {
            _client.GetLogger().Debug($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");

            _chatManager.ProcessChatStringWithNoSender(impulse, ChatGroupType.System, _client);
        }

        private void ImpulseChat2(BitMemoryStream impulse)
        {
            _chatManager.ProcessChatString2(impulse, _client);
        }

        private void ImpulseFarTell(BitMemoryStream impulse)
        {
            _client.GetLogger().Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        /// <summary>
        /// a tell arrives
        /// </summary>
        private void ImpulseTell(BitMemoryStream impulse)
        {
            _chatManager.ProcessTellString(impulse, _client);
        }

        /// <summary>
        /// a chat message arrives
        /// </summary>
        private void ImpulseChat(BitMemoryStream impulse)
        {
            _chatManager.ProcessChatString(impulse, _client);
        }

        private void ImpulsePermanentUnban(BitMemoryStream impulse)
        {
            _client.GetLogger().Warn($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private void ImpulsePermanentBan(BitMemoryStream impulse)
        {
            _client.GetLogger().Warn($"§cImpulse on {MethodBase.GetCurrentMethod()?.Name}");

            // WHAAAAAAAAAAAAAAAAAAAAAAAAAAA!!!
        }

        private void ImpulseForumNotification(BitMemoryStream impulse)
        {
            _client.GetLogger().Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private void ImpulseMailNotification(BitMemoryStream impulse)
        {
            _client.GetLogger().Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private void ImpulseServerQuitAbort(BitMemoryStream impulse)
        {
            _client.Plugins.OnServerQuitAbort();
        }

        private void ImpulseServerQuitOk(BitMemoryStream impulse)
        {
            _client.GetLogger().Debug($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
            GameExit = true;
            _networkConnection.Disconnect();

            _client.Plugins.OnDisconnect();
        }

        /// <summary>
        /// received SHARD_ID
        /// </summary>
        private void ImpulseShardId(BitMemoryStream impulse)
        {
            _client.GetLogger().Debug($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");

            uint shardId = 0;
            impulse.Serial(ref shardId);

            var webHost = "";
            impulse.Serial(ref webHost, false);

            _client.Plugins.OnShardID(shardId, webHost);
        }

        private void ImpulseCharNameValid(BitMemoryStream impulse)
        {
            _client.GetLogger().Debug($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");

            byte nTmp = 0;
            impulse.Serial(ref nTmp);

            CharNameValid = nTmp != 0;
            CharNameValidArrived = true;
        }

        private void ImpulseServerReady(BitMemoryStream impulse)
        {
            _client.GetLogger().Info("Server is ready! Joining game...");

            ServerReceivedReady = true;

            CheckHandshake(impulse);

            _client.Plugins.OnGameJoined();
        }

        /// <summary>
        /// Received FAR_TP
        /// </summary>
        private void ImpulseFarTp(BitMemoryStream impulse)
        {
            uint sessionId = 0;
            impulse.Serial(ref sessionId);

            var bailOutIfSessionVanished = false;
            impulse.Serial(ref bailOutIfSessionVanished);

            //FarTP.requestFarTPToSession(sessionId, PlayerSelectedSlot, CFarTP::JoinSession, bailOutIfSessionVanished);

            _client.GetLogger().Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name} - Session ID: {sessionId} - Bail out, if session vanished: {bailOutIfSessionVanished}");
        }

        /// <summary>
        /// server client message for the character information at the beginning
        /// </summary>
        private void ImpulseUserChar(BitMemoryStream impulse)
        {
            // Received USER_CHAR
            _client.GetLogger().Debug("ImpulseCallBack : Received CONNECTION:USER_CHAR");

            UserCharMsg.Read(impulse, out var x, out var y, out var z, out var heading, out _serverSeasonValue, out var userRole, out var isInRingSession, out var highestMainlandSessionId, out var firstConnectedTime, out CharPlayedTime);

            // Set the season that will be used when selecting the continent from the position

            if (_entitiesManager.UserEntity != null)
            {
                _entitiesManager.UserEntity.Pos = new Vector3(x / 1000.0f, y / 1000.0f, z / 1000.0f);
                _entitiesManager.UserEntity.Front = new Vector3((float)Math.Cos(heading), (float)Math.Sin(heading), 0f);
                _entitiesManager.UserEntity.Dir = _entitiesManager.UserEntity.Front;
                _entitiesManager.UserEntity.SetHeadPitch(0);

                _client.GetLogger().Info($"Received Char Position: {_entitiesManager.UserEntity.Pos} Heading: {heading:0.000} Front: {_entitiesManager.UserEntity.Front:0.000}");

                // Update the position for the vision.
                _networkConnection.SetReferencePosition(_entitiesManager.UserEntity.Pos);

                _client.Plugins.OnUserChar(highestMainlandSessionId, firstConnectedTime, CharPlayedTime, _entitiesManager.UserEntity.Pos, _entitiesManager.UserEntity.Front, _serverSeasonValue, userRole, isInRingSession);
            }
            else
            {
                var userEntityInitPos = new Vector3(x / 1000.0f, y / 1000.0f, z / 1000.0f);
                var userEntityInitFront = new Vector3((float)Math.Cos(heading), (float)Math.Sin(heading), 0f);

                _client.GetLogger().Info($"Received Char Position: {userEntityInitPos} Heading: {heading} Front: {userEntityInitFront}");

                // Update the position for the vision.
                _networkConnection.SetReferencePosition(userEntityInitPos);

                _client.Plugins.OnUserChar(highestMainlandSessionId, firstConnectedTime, CharPlayedTime, userEntityInitPos, userEntityInitFront, _serverSeasonValue, userRole, isInRingSession);
            }

            _client.UserCharPosReceived = true;
        }

        private void ImpulseUserChars(BitMemoryStream impulse)
        {
            // received USER_CHARS
            _client.GetLogger().Info("Received the following characters from the server:");

            impulse.Serial(ref _serverPeopleActive);
            impulse.Serial(ref _serverCareerActive);

            // read characters summary
            CharacterSummaries.Clear();

            #region workaround for serialVector(T &cont) in stream.h
            var len = 0;
            impulse.Serial(ref len);

            for (var i = 0; i < len; i++)
            {
                var cs = new CharacterSummary();
                cs.Serial(impulse);
                if ((PeopleType)cs.People != PeopleType.Unknown)
                    _client.GetLogger().Info($"§d[§b{i}§d] {EntityHelper.RemoveShardFromName(cs.Name)}§r from shard {cs.Mainland} of type {(PeopleType)cs.People}.");
                CharacterSummaries.Add(cs);
            }
            #endregion

            // read shard name summaries
            var shardNamesLength = 0;
            impulse.Serial(ref shardNamesLength);

            ShardNames.Clear();
            for (var i = 0; i < shardNamesLength; i++)
            {
                var shardName = string.Empty;
                impulse.Serial(ref shardName, false);
                ShardNames.Add(shardName);
            }

            // TODO: ShardNames.Instance.LoadShardNames(shardNames);

            // read privileges
            var userPrivileges = "";
            impulse.Serial(ref userPrivileges);
            UserPrivileges = userPrivileges;

            // read FreeTrial status
            var freeTrial = true;
            impulse.Serial(ref freeTrial);
            FreeTrial = freeTrial;

            // read Mainlands
            var mainlandsLength = 0;
            impulse.Serial(ref mainlandsLength);

            Mainlands.Clear();
            for (var i = 0; i < mainlandsLength; i++)
            {
                var mainland = new MainlandSummary();
                mainland.Serial(impulse);
                Mainlands.Add(mainland);
            }

            // Assuming there's only one Mainland
            MainlandSelected = Mainlands[0].Id;

            // TODO: Handle new character keysets if applicable

            // TODO: UpdatePatcherPriorityBasedOnCharacters();

            _client.GetLogger().Debug("st_ingame->st_select_char");
            CanSendCharSelection = true;

            _client.Plugins.OnUserChars();
        }

        private void ImpulseNoUserChar(BitMemoryStream impulse)
        {
            _client.GetLogger().Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private void ImpulseDatabaseResetBank(BitMemoryStream impulse)
        {
            _client.GetLogger().Debug($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");

            uint serverTick = 0;
            uint bank = 0;

            try
            {
                // get the egs tick of this change
                impulse.Serial(ref serverTick);

                // read the bank to reset
                const int nbits = Constants.FillNbitsWithNbBitsForCdbbank;
                impulse.Serial(ref bank, nbits);

                // reset the bank
                if (_databaseManager == null) return;

                _databaseManager.ResetBank(serverTick, bank);

                _client.Plugins.OnDatabaseResetBank(serverTick, bank, _databaseManager);
            }
            catch (Exception e)
            {
                _client.GetLogger().Error($"Problem while decoding a DB_GROUP:RESET_BANK {bank} msg, skipped: {e.Message}");
            }
        }

        /// <summary>
        /// impulseDatabaseInitBank
        /// </summary>
        private void ImpulseDatabaseInitBank(BitMemoryStream impulse)
        {
            _client.GetLogger().Debug($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");

            uint serverTick = 0;
            uint bank = 0;

            try
            {
                // get the egs tick of this change
                impulse.Serial(ref serverTick);

                // decode bank
                const int nbits = Constants.FillNbitsWithNbBitsForCdbbank;
                impulse.Serial(ref bank, nbits);

                // read delta
                if (_databaseManager == null) return;

                _databaseManager.ReadDelta(serverTick, impulse, bank);

                _client.Plugins.OnDatabaseInitBank(serverTick, bank, _databaseManager);
            }
            catch (Exception e)
            {
                _client.GetLogger().Error($"Problem while decoding a DB_GROUP:INIT_BANK {bank} msg, skipped: {e.Message}");
            }
        }

        private void ImpulseDatabaseUpdateBank(BitMemoryStream impulse)
        {
            _client.GetLogger().Debug($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");

            uint serverTick = 0;
            uint bank = 0;

            try
            {
                // get the egs tick of this change
                impulse.Serial(ref serverTick);

                // decode bank
                const int nbits = Constants.FillNbitsWithNbBitsForCdbbank;
                impulse.Serial(ref bank, nbits);

                // read delta
                if (_databaseManager == null) return;

                _databaseManager.ReadDelta(serverTick, impulse, bank);

                _client.Plugins.OnDatabaseUpdateBank(serverTick, bank, _databaseManager);
            }
            catch (Exception e)
            {
                _client.GetLogger().Error($"Problem while decoding a DB_GROUP:INIT_BANK {bank} msg, skipped: {e.Message}");
            }
        }

        private void UpdateInventoryFromStream(BitMemoryStream impulse, InventoryCategoryTemplate templ)
        {
            if (!ClientConfig.UseInventory)
                return;

            try
            {
                // get the egs tick of this change
                var serverTick = new uint();
                impulse.Serial(ref serverTick);

                // For All inventories
                for (uint invId = 0; invId != templ.NbInventoryIds; ++invId)
                {
                    // Presence bit
                    var hasContent = new bool();
                    impulse.Serial(ref hasContent);
                    if (!hasContent)
                    {
                        continue;
                    }

                    // Number field
                    var nbChanges = new uint();
                    impulse.Serial(ref nbChanges, (int)Inventories.LowNumberBits);
                    if (nbChanges == Inventories.LowNumberBound)
                    {
                        impulse.Serial(ref nbChanges, 32);
                    }

                    var invBranchStr = templ.GetDbStr(invId);
                    var textId = new TextId(invBranchStr);
                    var inventoryNode = GetDatabaseManager().GetServerDb().GetNode(textId);
                    if (inventoryNode == null) throw new Exception("Inventory missing in database");

                    // List of updates
                    for (uint c = 0; c != nbChanges; ++c)
                    {
                        // Unpack (the bitmemstream is written from high-order to low-order)
                        var iuInfoVersion = new uint();
                        impulse.Serial(ref iuInfoVersion, 1);
                        if (iuInfoVersion == 1)
                        {
                            var slotIndex = new uint();
                            impulse.Serial(ref slotIndex, (int)templ.SlotBitSize);

                            // Access the database leaf
                            var slotNode = (DatabaseNodeBranch)inventoryNode.GetNode((ushort)slotIndex);
                            var leafNode = (DatabaseNodeLeaf)(slotNode.Find(Inventories.InfoVersionStr));

                            if (leafNode == null)
                            {
                                _client.Log.Error($"Inventory slot property missing in database {slotNode.GetFullName()}:{Inventories.InfoVersionStr}");
                                continue;
                            }

                            // Apply or increment Info Version in database
                            if (templ.NeedPlainInfoVersionTransfer())
                            {
                                var infoVersion = new uint();
                                impulse.Serial(ref infoVersion, (int)Inventories.InfoVersionBitSize);
                                leafNode.SetPropCheckGc(serverTick, infoVersion);
                            }
                            else
                            {
                                // NB: don't need to check GC on a info version upgrade, since this is always a delta of +1
                                // the order of received of this impulse is not important
                                leafNode.SetValue64(leafNode.GetValue64() + 1);
                            }
                        }
                        else
                        {
                            var iuAll = new uint();
                            impulse.Serial(ref iuAll, 1);
                            if (iuAll == 1)
                            {
                                var itemSlot = new ItemSlot();
                                itemSlot.SerialAll(impulse, templ);

                                // Apply all properties to database
                                var slotNode = (DatabaseNodeBranch)inventoryNode.GetNode((ushort)itemSlot.GetSlotIndex());
                                for (uint i = 0; i != (uint)Inventories.ItemPropId.NbItemPropId; ++i)
                                {
                                    var leafNode = (DatabaseNodeLeaf)(slotNode.Find(ItemSlot.ItemPropStr[i]));
                                    if (leafNode == null)
                                    {
                                        _client.Log.Debug($"Inventory slot property missing in database {slotNode.GetFullName()}:{ItemSlot.ItemPropStr[i]}");
                                        continue;
                                    }

                                    leafNode.SetPropCheckGc(serverTick, itemSlot.GetItemProp((Inventories.ItemPropId)i));
                                }
                            }
                            else
                            {
                                var iuOneProp = new uint();
                                impulse.Serial(ref iuOneProp, 1);
                                if (iuOneProp == 1)
                                {
                                    var itemSlot = new ItemSlot();
                                    itemSlot.SerialOneProp(impulse, templ);
                                    //nldebug( "Inv %s Prop %u %s", CInventoryCategoryTemplate::InventoryStr[invId], itemSlot.getSlotIndex(), INVENTORIES::CItemSlot::ItemPropStr[itemSlot.getOneProp().ItemPropId] );

                                    // Apply property to database
                                    var slotNode = (DatabaseNodeBranch)inventoryNode.GetNode((ushort)itemSlot.GetSlotIndex());
                                    var leafNode = (DatabaseNodeLeaf)(slotNode.Find(ItemSlot.ItemPropStr[(int)itemSlot.GetOneProp().ItemPropId]));
                                    if (leafNode == null)
                                    {
                                        _client.Log.Error($"Inventory slot property missing in database {slotNode.GetFullName()}:{ItemSlot.ItemPropStr[(int)itemSlot.GetOneProp().ItemPropId]}");
                                        continue;
                                    }

                                    leafNode.SetPropCheckGc(serverTick, itemSlot.GetOneProp().ItemPropValue);
                                }
                                else // iuReset
                                {
                                    var slotIndex = new uint();
                                    impulse.Serial(ref slotIndex, (int)templ.SlotBitSize);
                                    //nldebug( "Inv %s Reset %u", CInventoryCategoryTemplate::InventoryStr[invId], slotIndex );

                                    // Reset all properties in database
                                    var slotNode = (DatabaseNodeBranch)inventoryNode.GetNode((ushort)slotIndex);
                                    for (uint i = 0; i != (uint)Inventories.ItemPropId.NbItemPropId; ++i)
                                    {
                                        // Instead of clearing all leaves (by index), we must find and clear only the
                                        // properties in TItemPropId, because the actual database leaves may have
                                        // less properties, and because we must not clear the leaf INFO_VERSION.
                                        // NOTE: For example, only player BAG inventory has WORNED leaf.
                                        var leafNode = (DatabaseNodeLeaf)(slotNode.Find(ItemSlot.ItemPropStr[i]));
                                        if (leafNode == null)
                                        {
                                            _client.Log.Error($"Inventory slot property missing in database {slotNode.GetFullName()}:{ItemSlot.ItemPropStr[i]}");
                                            continue;
                                        }
                                        leafNode.SetPropCheckGc(serverTick, 0);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _client.Log.Error($"Problem while decoding a DB_UPD_INV msg, skipped: {e.Message}");
            }
        }

        /// <summary>
        /// ImpulseUpdateInventory
        /// </summary>
        private void ImpulseUpdateInventory(BitMemoryStream impulse)
        {
            UpdateInventoryFromStream(impulse, new InventoryCategoryForCharacter());

            _client.Plugins.OnInitInventory(0);
        }

        /// <summary>
        /// ImpulseInitInventory
        /// </summary>
        private void ImpulseInitInventory(BitMemoryStream impulse)
        {
            ImpulseUpdateInventory(impulse);

            _databaseManager?.SetInitPacketReceived();

            if (ClientConfig.UseInventory)
                GetInventoryManager().OnUpdateEquipHands();
        }

        /// <summary>
        /// ImpulseDatabaseInitPlayer
        /// </summary>
        private void ImpulseDatabaseInitPlayer(BitMemoryStream impulse)
        {
            _client.GetLogger().Debug($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");

            try
            {
                var p = impulse.Pos;

                // get the egs tick of this change
                uint serverTick = 0;
                impulse.Serial(ref serverTick);

                // read delta
                if (_databaseManager == null) return;

                _databaseManager.ReadDelta(serverTick, impulse, Constants.CdbPlayer);
                _databaseManager.SetInitPacketReceived();
                _client.GetLogger().Debug($"DB_INIT:PLR done ({impulse.Pos - p} bytes)");

                _client.Plugins.OnDatabaseInitPlayer(serverTick);
            }
            catch (Exception e)
            {
                _client.GetLogger().Error($"Problem while decoding a DB_INIT:PLR msg, skipped: {e.Message}");
            }
        }

        /// <summary>
        /// player database update
        /// </summary>
        private void ImpulseDatabaseUpdatePlayer(BitMemoryStream impulse)
        {
            _client.GetLogger().Debug($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");

            try
            {
                // get the egs tick of this change
                uint serverTick = 0;
                impulse.Serial(ref serverTick);

                // read delta
                if (_databaseManager == null) return;

                // unlike on the server, here there is only one unified CCDBSynchronized object
                _databaseManager.ReadDelta(serverTick, impulse, Constants.CdbPlayer);

                _client.Plugins.OnDatabaseUpdatePlayer(serverTick);
            }
            catch (Exception e)
            {
                _client.GetLogger().Error($"Problem while decoding a DB_UPDATE_PLR msg, skipped: {e.Message}");
            }
        }

        /// <summary>
        /// Decode handshake to check versions
        /// </summary>
        private void CheckHandshake(BitMemoryStream impulse)
        {
            uint handshakeVersion = 0;
            impulse.Serial(ref handshakeVersion, 2);
            if (handshakeVersion > 0)
                _client.GetLogger().Warn("Server handshake version is more recent than client one");

            uint itemSlotVersion = 0;
            impulse.Serial(ref itemSlotVersion, 2);
            _client.GetLogger().Debug($"Item slot version: {itemSlotVersion}");

            //if (itemSlotVersion != INVENTORIES::CItemSlot::getVersion())
            //    nlerror("Handshake: itemSlotVersion mismatch (S:%hu C:%hu)", itemSlotVersion, INVENTORIES::CItemSlot::getVersion());
        }

        /// <summary>
        /// sendMsgToServer Helper
        /// selects the message by its name and pushes it to the connection
        /// </summary>
        public void SendMsgToServer(string sMsg)
        {
            var out2 = new BitMemoryStream();

            if (_messageHeaderManager.PushNameToStream(sMsg, out2))
            {
                Push(out2);
            }
            else
            {
                _client.GetLogger().Warn($"Unknown message named '{sMsg}'.");
            }
        }

        /// <summary>
        /// Buffers a target action
        /// </summary>
        public void PushTarget(in byte slot)
        {
            _networkConnection.PushTarget(slot, TargettingType.None);
        }
    }
}