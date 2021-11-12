///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using System.Threading;
using API.Chat;
using API.Helper;
using API.Network;
using Client.Chat;
using Client.Client;
using Client.Database;
using Client.Entity;
using Client.Helper;
using Client.Messages;
using Client.Network.Action;
using Client.Phrase;
using Client.Property;

namespace Client.Network
{

    /// <summary>
    /// used to control the connection and implements the impulse callbacks from the connection
    /// </summary>
    public class NetworkManager : INetworkManager
    {
        public bool ServerReceivedReady;

        public byte PlayerSelectedSlot = 0;
        public byte ServerPeopleActive = 255;
        public byte ServerCareerActive = 255;

        public List<CharacterSummary> CharacterSummaries = new List<CharacterSummary>();
        public bool WaitServerAnswer;

        public string PlayerSelectedHomeShardName { get; set; } = "";
        public string PlayerSelectedHomeShardNameWithParenthesis = "";

        public bool GameExit;

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

        private readonly PhraseManager _phraseManager = new PhraseManager();

        /// <summary>
        /// was the inital server season received
        /// </summary>
        public bool ServerSeasonReceived;

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

        public ChatManager GetChatManager() => _chatManager;

        /// <summary>
        /// Constructor
        /// </summary>
        public NetworkManager(RyzomClient client, NetworkConnection networkConnection, StringManager stringManager, DatabaseManager databaseManager)
        {
            _messageHeaderManager = new GenericMessageHeaderManager();
            _chatManager = new ChatManager(this, stringManager, databaseManager);
            _entitiesManager = new EntityManager(client);

            _networkConnection = networkConnection;
            _stringManager = stringManager;
            _databaseManager = databaseManager;
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
                    Thread.Sleep(100);

                    gameCycle = _networkConnection.GetCurrentServerTick();
                }
            }

            _networkConnection.Send(gameCycle);
        }

        /// <summary>
        /// Buffers a bitmemstream, that will be converted into a generic action, to be sent later to the server (at next
        /// update).
        /// </summary>
        public void Push(BitMemoryStream msg)
        {
            _networkConnection.Push(msg);
        }

        /// <summary>
        /// Updates the whole connection with the frontend.
        /// Call this method evently.
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
                // Add New Entity (and remove the old one in the slot)
                else if (change.Property == (byte)PropertyType.AddNewEntity)
                {
                    // Remove the old entity
                    _entitiesManager.Remove(change.ShortId, false);

                    // Create the new entity
                    if (_entitiesManager.Create(change.ShortId, _networkConnection.GetPropertyDecoder().GetSheetFromEntity(change.ShortId), change.NewEntityInfo) == null)
                    {
                        _client.GetLogger().Warn($"CNetManager::update : entity in the slot '{change.ShortId}' has not been created.");
                    }
                }
                // Delete an entity
                else if (change.Property == (byte)PropertyType.RemoveOldEntity)
                {
                    // Remove the old entity
                    _entitiesManager.Remove(change.ShortId, true);
                }
                // Lag detected
                else if (change.Property == (byte)PropertyType.LagDetected)
                {
                    _client.GetLogger().Debug("CNetManager::update : Lag detected.");
                }
                // Probe received
                else if (change.Property == (byte)PropertyType.ProbeReceived)
                {
                    _client.GetLogger().Debug("CNetManager::update : Probe Received.");
                }
                // Connection ready
                else if (change.Property == (byte)PropertyType.ConnectionReady)
                {
                    _client.GetLogger().Debug("CNetManager::update : Connection Ready.");
                }
                // Property unknown
                else
                {
                    _client.GetLogger().Warn("CNetManager::update : The property '" + change.Property + "' is unknown.");
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
        /// ImpulseCallBack :
        /// The Impulse callback to receive all msg from the frontend.
        /// </summary>
        public void ImpulseCallBack(BitMemoryStream impulse)
        {
            _messageHeaderManager.Execute(impulse);
        }

        /// <summary>
        /// initializeNetwork :
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

            _messageHeaderManager.SetCallback("PHRASE:DOWNLOAD", ImpulsePhraseDownLoad);
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

        private void ImpulseCombatFlyingText(BitMemoryStream impulse)
        {
            _client.GetLogger().Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private void ImpulseCombatFlyingTextItemSpecialEffectProc(BitMemoryStream impulse)
        {
            _client.GetLogger().Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private void ImpulseCombatFlyingHpDelta(BitMemoryStream impulse)
        {
            _client.GetLogger().Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
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
            _client.GetLogger().Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
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
        /// TODO: Respawn point set
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

        private void ImpulsePhraseAckExecuteNext(BitMemoryStream impulse)
        {
            _client.GetLogger().Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private void ImpulsePhraseAckExecuteCyclic(BitMemoryStream impulse)
        {
            _client.GetLogger().Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private void ImpulsePhraseConfirmBuy(BitMemoryStream impulse)
        {
            _client.GetLogger().Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        /// <summary>
        /// TODO server sends the phrases
        /// </summary>
        private void ImpulsePhraseDownLoad(BitMemoryStream impulse)
        {
            return;

            // Read Known Phrases
            //impulse.SerialCont(phrases);

            int len = 0;
            impulse.Serial(ref len);
            var phrases = new List<PhraseSlot>(len);

            for (var i = 0; i < len; i++)
            {
                var value = PhraseSlot.Serial(impulse);
                phrases.Add(value);
            }

            //PhraseManager pPM = PhraseManager.getInstance();

            for (var i = 0; i < phrases.Count; ++i)
            {
            //    if (phrases[i].PhraseSheetId != CSheetId.Unknown)
            //    {
            //        PhraseCom phraseCom = new PhraseCom();
            //        _phraseManager.buildPhraseFromSheet(phraseCom, phrases[i].PhraseSheetId.AsInt());
            //        _phraseManager.setPhraseNoUpdateDB(phrases[i].KnownSlot, phraseCom);
            //    }
            //    else
            //    {
            //        _phraseManager.setPhraseNoUpdateDB(phrases[i].KnownSlot, phrases[i].Phrase);
            //    }
            }
            
            // must update the DB (NB: if initInGameDone) after all phrase set.
            //pPM.updateBookDB();

            // Then Read Memorized Phrases
            //impulse.SerialCont(memorizedPhrases);
            len = 0;
            impulse.Serial(ref len);
            var memorizedPhrases = new List<PhraseMemorySlot>(len);

            for (var i = 0; i < len; i++)
            {
                var value = PhraseMemorySlot.Serial(impulse);
                memorizedPhrases.Add(value);
            }

            for (var i = 0; i < memorizedPhrases.Count; ++i)
            {
            //    pPM.memorizePhrase(memorizedPhrases[i].MemoryLineId, memorizedPhrases[i].MemorySlotId, memorizedPhrases[i].PhraseId);
            }
            
            // OK.
            _client.SabrinaPhraseBookLoaded = true;
            
            // update gray state, if game inited.
            //pPM.updateMemoryBar();

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

            bool useFemaleTitles = false;
            impulse.Serial(ref useFemaleTitles);

            _client.Plugins.OnGuildUseFemaleTitles(useFemaleTitles);
        }

        /// <summary>
        /// server block/unblock some reserved titles
        /// </summary>
        private void ImpulseGuildUpdatePlayerTitle(BitMemoryStream impulse)
        {
            _client.GetLogger().Debug($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");

            bool bUnblock = false;
            impulse.Serial(ref bUnblock);

            int len = 0;
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

            uint textID = uint.MinValue;
            impulse.Serial(ref textID);

            _client.Plugins.OnTeamInvitation(textID);
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
            _client.GetLogger().Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private void ImpulseCombatEngageFailed(BitMemoryStream impulse)
        {
            _client.GetLogger().Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        /// <summary>
        /// Message from the server to correct the user position because he is not at the same position on the server..
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

                _client.GetLogger().Warn($"impulseCorrectPos: SetSpeedServerAdjust");
            }
            else
            {
                // Compute the destination.
                var dest = new Vector3(x / 1000.0f, y / 1000.0f, z / 1000.0f);

                _client.GetLogger().Warn($"impulseCorrectPos: new user position {dest}");

                // Update the position for the vision.
                _client.GetNetworkManager().SetReferencePosition(dest);

                // Change the user poisition.
                _entitiesManager.UserEntity.CorrectPos(dest);
            }
            //}
        }

        private void SetReferencePosition(Vector3 dest)
        {
            _networkConnection.GetPropertyDecoder().SetReferencePosition(dest);
        }

        private void ImpulseTpWithSeason(BitMemoryStream impulse)
        {
            _client.GetLogger().Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private void ImpulseTp(BitMemoryStream impulse)
        {
            _client.GetLogger().Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private void ImpulseTell2(BitMemoryStream impulse)
        {
            _client.GetLogger().Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private void ImpulseDynStringInChatGroup(BitMemoryStream impulse)
        {
            _client.GetLogger().Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private void ImpulseDynString(BitMemoryStream impulse)
        {
            _client.GetLogger().Debug($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");

            _chatManager.ProcessChatStringWithNoSender(impulse, ChatGroupType.System, _client);
        }

        private void ImpulseChat2(BitMemoryStream impulse)
        {
            _client.GetLogger().Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
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
            _client.GetLogger().Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private void ImpulsePermanentBan(BitMemoryStream impulse)
        {
            _client.GetLogger().Info($"§cImpulse on {MethodBase.GetCurrentMethod()?.Name}");

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
            _client.GetLogger().Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
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
            _client.GetLogger().Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private void ImpulseServerReady(BitMemoryStream impulse)
        {
            _client.GetLogger().Info("Server is ready! Joining game...");

            ServerReceivedReady = true;

            CheckHandshake(impulse);

            _client.Plugins.OnGameJoined();
        }

        private void ImpulseFarTp(BitMemoryStream impulse)
        {
            _client.GetLogger().Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        /// <summary>
        /// server client message for the character information at the beginning
        /// </summary>
        private void ImpulseUserChar(BitMemoryStream impulse)
        {
            // received USER_CHAR
            _client.GetLogger().Debug("ImpulseCallBack : Received CONNECTION:USER_CHAR");

            UserCharMsgRead(impulse, out var x, out var y, out var z, out var heading, out var season, out var userRole, out var isInRingSession, out var highestMainlandSessionId, out var firstConnectedTime, out CharPlayedTime);

            ServerSeasonReceived = true; // set the season that will be used when selecting the continent from the position

            if (_entitiesManager.UserEntity != null)
            {
                _entitiesManager.UserEntity.Pos = new Vector3(x / 1000.0f, y / 1000.0f, z / 1000.0f);
                _entitiesManager.UserEntity.Front = new Vector3((float)Math.Cos(heading), (float)Math.Sin(heading), 0f);
                _entitiesManager.UserEntity.Dir = _entitiesManager.UserEntity.Front;
                _entitiesManager.UserEntity.SetHeadPitch(0);

                _client.GetLogger().Info($"Received Char Position: {_entitiesManager.UserEntity.Pos} Heading: {heading:0.000} Front: {_entitiesManager.UserEntity.Front:0.000}");

                // Update the position for the vision.
                _networkConnection.SetReferencePosition(_entitiesManager.UserEntity.Pos);

                _client.Plugins.OnUserChar(highestMainlandSessionId, firstConnectedTime, CharPlayedTime, _entitiesManager.UserEntity.Pos, _entitiesManager.UserEntity.Front, season, userRole, isInRingSession);
            }
            else
            {
                var userEntityInitPos = new Vector3(x / 1000.0f, y / 1000.0f, z / 1000.0f);
                var userEntityInitFront = new Vector3((float)Math.Cos(heading), (float)Math.Sin(heading), 0f);

                _client.GetLogger().Info($"Received Char Position: {userEntityInitPos} Heading: {heading} Front: {userEntityInitFront}");

                // Update the position for the vision.
                _networkConnection.SetReferencePosition(userEntityInitPos);

                _client.Plugins.OnUserChar(highestMainlandSessionId, firstConnectedTime, CharPlayedTime, userEntityInitPos, userEntityInitFront, season, userRole, isInRingSession);
            }

            _client.UserCharPosReceived = true;
        }

        /// <summary>
        /// decode server client message for the character information at the beginning
        /// </summary>
        /// <author>PUZIN Guillaume (GUIGUI)</author>
        /// <author>Nevrax France</author>
        /// <date>2002</date>
        private static void UserCharMsgRead(BitMemoryStream impulse, out int x, out int y, out int z, out float heading, out short season, out int userRole, out bool isInRingSession, out int highestMainlandSessionId, out int firstConnectedTime, out int playedTime)
        {
            x = 0;
            y = 0;
            z = 0;

            var headingI = 0;

            var s = impulse;
            var f = s;

            f.Serial(ref x);
            f.Serial(ref y);
            f.Serial(ref z);
            f.Serial(ref headingI);

            heading = Misc.Int32BitsToSingle(headingI);

            short v = 0;
            s.Serial(ref v, 3);
            season = v;
            v = 0;
            s.Serial(ref v, 3);
            userRole = v & 0x3;
            isInRingSession = (v & 0x4) != 0;

            highestMainlandSessionId = 0;
            firstConnectedTime = 0;
            playedTime = 0;

            s.Serial(ref highestMainlandSessionId);
            s.Serial(ref firstConnectedTime);
            s.Serial(ref playedTime);
        }

        private void ImpulseUserChars(BitMemoryStream impulse)
        {
            // received USER_CHARS
            _client.GetLogger().Info("Received user characters from the server:");

            impulse.Serial(ref ServerPeopleActive);
            impulse.Serial(ref ServerCareerActive);

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
                    _client.GetLogger().Info($"Character {cs.Name} from shard {cs.Mainland} in slot {i}");
                CharacterSummaries.Add(cs);
            }
            #endregion

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
        /// TODO: impulseDatabaseInitBank
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

        /// <summary>
        /// TODO: ImpulseInitInventory
        /// </summary>
        private void ImpulseInitInventory(BitMemoryStream impulse)
        {
            _client.GetLogger().Debug($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");

            // get the egs tick of this change
            uint serverTick = 0;
            impulse.Serial(ref serverTick);

            _client.Plugins.OnInitInventory(serverTick);
        }

        private void ImpulseUpdateInventory(BitMemoryStream impulse)
        {
            _client.GetLogger().Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
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

                _databaseManager.ReadDelta(serverTick, impulse, Constants.CdbPlayer); // unlike on the server, here there is only one unified CCDBSynchronized object

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