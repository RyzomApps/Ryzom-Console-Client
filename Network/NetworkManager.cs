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
using RCC.Chat;
using RCC.Client;
using RCC.Database;
using RCC.Helper;
using RCC.Messages;

namespace RCC.Network
{
    /// <summary>
    /// used to control the connection and implements the impulse callbacks from the connection
    /// </summary>
    public class NetworkManager
    {
        public bool ServerReceivedReady;

        public byte PlayerSelectedSlot = 0;
        public byte ServerPeopleActive = 255;
        public byte ServerCareerActive = 255;

        public List<CharacterSummary> CharacterSummaries = new List<CharacterSummary>();
        public bool WaitServerAnswer;

        public string PlayerSelectedHomeShardName = "";
        public string PlayerSelectedHomeShardNameWithParenthesis = "";

        public bool GameExit;

        public bool UserChar;
        public bool NoUserChar;
        public bool ConnectInterf;
        public bool CreateInterf;
        public bool CharacterInterf;

        // This must be changed when cdbank bits in the client change
        private const int FillNbitsWithNbBitsForCdbbank = 3;

        // non ryzom variables (for workarounds)

        /// <summary>
        /// client is ready for the selection of the character
        /// </summary>
        public bool CanSendCharSelection;

        private readonly NetworkConnection _networkConnection;
        private readonly StringManager _stringManager;
        private readonly DatabaseManager _databaseManager;
        private DatabaseNodeBranch _dataBase;
        private readonly RyzomClient _client;

        private readonly GenericMessageHeaderManager _messageHeaderManager;

        private readonly ChatManager _chatManager;

        public GenericMessageHeaderManager GetMessageHeaderManager() => _messageHeaderManager;

        /// <summary>
        /// Constructor
        /// </summary>
        public NetworkManager(RyzomClient client, NetworkConnection networkConnection, StringManager stringManager, Database.DatabaseManager databaseManager)
        {
            _messageHeaderManager = new GenericMessageHeaderManager();
            _chatManager = new ChatManager(this, stringManager);

            _networkConnection = networkConnection;
            _stringManager = stringManager;
            _databaseManager = databaseManager;
            _client = client;
        }

        /// <summary>
        /// Set database entry point
        /// </summary>
        internal void SetDataBase(DatabaseNodeBranch database)
        {
            _dataBase = database;
        }

        /// <summary>
        ///     Send - updates when packets were received
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
        ///     Buffers a bitmemstream, that will be converted into a generic action, to be sent later to the server (at next
        ///     update).
        /// </summary>
        public void Push(BitMemoryStream msg)
        {
            _networkConnection.Push(msg);
        }

        /// <summary>
        ///     Updates the whole connection with the frontend.
        ///     Call this method evently.
        /// </summary>
        /// <returns>'true' if data were sent/received.</returns>
        public bool Update()
        {
            //ConsoleIO.WriteLineFormatted($"§e{GetType().Name}.{MethodBase.GetCurrentMethod()?.Name}() {DateTimeOffset.Now.ToUnixTimeMilliseconds()}");

            // Update the base class.
            var result = _networkConnection.Update();

            // TODO: Get and manage changes with the netmgr update
            // 	const vector<CChange> &changes = NetMngr.getChanges();

            _chatManager.FlushBuffer(_client);

            // TODO: update everyting with the netmgr update

            return result;
        }

        /// <summary>
        ///     Send updates
        /// </summary>
        public void Send()
        {
            _networkConnection.Send();
        }

        /// <summary>
        ///     ImpulseCallBack :
        ///     The Impulse callback to receive all msg from the frontend.
        /// </summary>
        public void ImpulseCallBack(BitMemoryStream impulse)
        {
            _messageHeaderManager.Execute(impulse);
        }

        /// <summary>
        ///     initializeNetwork :
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

            _client.Automata.OnUserBars(msgNumber, hp, sap, sta, focus);
        }

        private void ImpulseEncyclopediaInit(BitMemoryStream impulse)
        {
            _client.GetLogger().Debug($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");

            _client.Automata.OnEncyclopediaInit();
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

            _client.Automata.OnDeathRespawnPoint(x, y);
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
            _client.GetLogger().Debug($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");

            _client.Automata.OnPhraseDownLoad();
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

            _client.Automata.OnGuildUseFemaleTitles(useFemaleTitles);
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

            _client.Automata.OnGuildUpdatePlayerTitle(bUnblock, len, vTitles);
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
        ///     reload the string cache
        /// </summary>
        private void ImpulseReloadCache(BitMemoryStream impulse)
        {
            var timestamp = 0;
            impulse.Serial(ref timestamp);

            _client.GetLogger().Debug($"Impulse on {MethodBase.GetCurrentMethod()?.Name} with timestamp {timestamp}");

            _stringManager.LoadCache(timestamp);

            _client.Automata.OnReloadCache(timestamp);
        }

        /// <summary>
        ///     Update the local string set
        /// </summary>
        private void ImpulseStringResp(BitMemoryStream impulse)
        {
            uint stringId = 0;
            string strUtf8 = "";
            impulse.Serial(ref stringId);
            impulse.Serial(ref strUtf8, false);

            _client.GetLogger().Debug($"Impulse on {MethodBase.GetCurrentMethod()?.Name} with stringId {stringId}");

            _stringManager.ReceiveString(stringId, strUtf8, this);

            _client.Automata.OnStringResp(stringId, strUtf8);
        }

        /// <summary>
        ///     A dyn string (or phrase) is send (so, we receive it)
        /// </summary>
        /// <remarks>Automaton Event is fired inside the string manager</remarks>
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

        private void ImpulseTeamContactRemove(BitMemoryStream impulse)
        {
            _client.GetLogger().Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
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

            _client.Automata.OnTeamContactStatus(contactId, online);
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

            _client.Automata.OnTeamContactCreate(contactId, nameId, online, nList);
        }

        /// <summary>
        /// initialize friend list and ignore list from the contact list
        /// </summary>
        private void ImpulseTeamContactInit(BitMemoryStream impulse)
        {
            var vIgnoreListName = new List<string>();

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

            //impulse.SerialCont(vIgnoreListName); TODO: ignore list - do we need them?

            _client.Automata.OnTeamContactInit(vFriendListName, vFriendListOnline, vIgnoreListName);
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

            _client.Automata.OnTeamInvitation(textID);
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

        private void ImpulseCorrectPos(BitMemoryStream impulse)
        {
            _client.GetLogger().Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
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

            _client.Automata.OnDisconnect();
        }

        /// <summary>
        /// received SHARD_ID
        /// </summary>
        /// <param name="impulse"></param>
        private void ImpulseShardId(BitMemoryStream impulse)
        {
            _client.GetLogger().Debug($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");

            uint shardId = 0;
            impulse.Serial(ref shardId);

            var webHost = "";
            impulse.Serial(ref webHost, false);

            _client.Automata.OnShardID(shardId, webHost);
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

            _client.Automata.OnGameJoined();
        }

        private void ImpulseFarTp(BitMemoryStream impulse)
        {
            _client.GetLogger().Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        /// <summary>
        /// TODO properly received USER_CHAR impulse
        /// </summary>
        private void ImpulseUserChar(BitMemoryStream impulse)
        {
            //// received USER_CHAR
            _client.GetLogger().Debug("ImpulseCallBack : Received CONNECTION:USER_CHAR");
            //
            //// Serialize the message
            //COfflineEntityState posState;
            //extern uint8 ServerSeasonValue;
            //extern bool ServerSeasonReceived;
            //uint32 userRole;

            int x = 0;
            int y = 0;
            int z = 0;

            int headingI = 0;

            var s = impulse;
            var f = s;

            f.Serial(ref x);
            f.Serial(ref y);
            f.Serial(ref z);
            f.Serial(ref headingI);

            float heading = Misc.Int32BitsToSingle(headingI);

            short v = 0;
            s.Serial(ref v, 3);
            var season = v;
            v = 0;
            s.Serial(ref v, 3);
            var userRole = v & 0x3; // bits 0-1
            var isInRingSession = (v & 0x4) != 0; // bit 2

            int highestMainlandSessionId = 0;
            int firstConnectedTime = 0;
            int playedTime = 0;

            s.Serial(ref highestMainlandSessionId);
            s.Serial(ref firstConnectedTime);
            s.Serial(ref playedTime);

            //ServerSeasonReceived = true; // set the season that will be used when selecting the continent from the position
            //
            //if (UserEntity)
            //{
            //    UserEntity->pos(CVectorD((float)posState.X / 1000.0f, (float)posState.Y / 1000.0f, (float)posState.Z / 1000.0f));
            //    UserEntity->front(CVector((float)cos(posState.Heading), (float)sin(posState.Heading), 0.f));
            //    UserEntity->dir(UserEntity->front());
            //    UserEntity->setHeadPitch(0);
            //    UserControls.resetCameraDeltaYaw();
            //    //nldebug("<ImpulseUserChar> pos : %f %f %f  heading : %f",UserEntity->pos().x,UserEntity->pos().y,UserEntity->pos().z,posState.Heading);
            //
            //    // Update the position for the vision.
            //    TODO NetMngr.setReferencePosition(UserEntity->pos());
            //}
            //else
            //{
            var userEntityInitPos = new Vector3(x / 1000.0f, y / 1000.0f, z / 1000.0f);
            var userEntityInitFront = new Vector3((float)Math.Cos(heading), (float)Math.Sin(heading), 0f);

            _client.GetLogger().Info($"Received Char Position: {userEntityInitPos} Heading: {heading} Front: {userEntityInitFront}");

            // Update the position for the vision.
            //NetworkManager.setReferencePosition(UserEntityInitPos);
            //}

            _client.UserCharPosReceived = true;

            //// Configure the ring editor
            //extern R2::TUserRole UserRoleInSession;
            //UserRoleInSession = R2::TUserRole::TValues(userRole);
            //ClientConfig.R2EDEnabled = IsInRingSession /*&& (UserRoleInSession.getValue() != R2::TUserRole::ur_player)*/;
            //// !!!Do NOT uncomment the following line  do the  ClientConfig.R2EDEnabled = IsInRingSession && (UserRoleInSession != R2::TUserRole::ur_player);
            //// even with UserRoleInSession R2::TUserRole::ur_player the ring features must be activated
            //// because if the ring is not activated the dss do not know the existence of the player
            //// So we can not kick him, tp to him, tp in to next act ....
            //nldebug("EnableR2Ed = %u, IsInRingSession = %u, UserRoleInSession = %u", (uint)ClientConfig.R2EDEnabled, (uint)IsInRingSession, userRole);

            // updatePatcherPriorityBasedOnCharacters();

            _client.Automata.OnUserChar(highestMainlandSessionId, firstConnectedTime, playedTime, userEntityInitPos, userEntityInitFront, season, userRole, isInRingSession);
        }

        private void ImpulseUserChars(BitMemoryStream impulse)
        {
            // received USER_CHARS
            _client.GetLogger().Info("Received user characters from the server:");

            impulse.Serial(ref ServerPeopleActive);
            impulse.Serial(ref ServerCareerActive);
            // read characters summary
            CharacterSummaries.Clear();

            // START WORKAROUND workaround for serialVector(T &cont) in stream.h
            int len = 0;
            impulse.Serial(ref len);

            for (var i = 0; i < len; i++)
            {
                var cs = new CharacterSummary();
                cs.Serial(impulse);
                if ((PeopleType)cs.People != PeopleType.Unknown)
                    _client.GetLogger().Info($"Character {cs.Name} from shard {cs.Mainland} in slot {i}");
                CharacterSummaries.Add(cs);
            }
            // END WORKAROUND

            //LoginSM.pushEvent(CLoginStateMachine::ev_chars_received);
            _client.GetLogger().Debug("st_ingame->st_select_char");
            CanSendCharSelection = true;

            //if (!NewKeysCharNameValidated.empty())
            //{
            //    // if there's a new char for which a key set was wanted, create it now
            //    for (uint k = 0; k < CharacterSummaries.size(); ++k)
            //    {
            //        if (toLower(CharacterSummaries[k].Name) == toLower(NewKeysCharNameValidated))
            //        {
            //            // first, stripes server name
            //            copyKeySet(lookupSrcKeyFile(GameKeySet), "save/keys_" + buildPlayerNameForSaveFile(NewKeysCharNameValidated) + ".xml");
            //            copyKeySet(lookupSrcKeyFile(RingEditorKeySet), "save/keys_r2ed_" + buildPlayerNameForSaveFile(NewKeysCharNameValidated) + ".xml");
            //            break;
            //        }
            //    }
            //}
            //updatePatcherPriorityBasedOnCharacters();

            _client.Automata.OnUserChars();
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
                const int nbits = FillNbitsWithNbBitsForCdbbank;
                impulse.Serial(ref bank, nbits);

                // reset the bank
                _databaseManager.ResetBank(serverTick, bank);

                //_client.Automata.OnDatabaseResetBank(serverTick, bank);
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
                const int nbits = FillNbitsWithNbBitsForCdbbank;
                impulse.Serial(ref bank, nbits);

                // read delta
                _databaseManager.ReadDelta(serverTick, impulse, bank);

                _client.Automata.OnDatabaseInitBank(serverTick, bank);
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
                const int nbits = FillNbitsWithNbBitsForCdbbank;
                impulse.Serial(ref bank, nbits);

                // read delta
                _databaseManager.ReadDelta(serverTick, impulse, bank);

                //_client.Automata.OnDatabaseUpdateBank(serverTick, bank);
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

            _client.Automata.OnInitInventory(serverTick);
        }

        private void ImpulseUpdateInventory(BitMemoryStream impulse)
        {
            _client.GetLogger().Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        const int CDBPlayer = 0;

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
                _databaseManager.ReadDelta(serverTick, impulse, CDBPlayer);
                _databaseManager.SetInitPacketReceived();
                _client.GetLogger().Debug($"DB_INIT:PLR done ({impulse.Pos - p} bytes)");

                _client.Automata.OnDatabaseInitPlayer(serverTick);
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
                _databaseManager.ReadDelta(serverTick, impulse, CDBPlayer); // unlike on the server, here there is only one unified CCDBSynchronized object

                _client.Automata.OnDatabaseUpdatePlayer(serverTick);
            }
            catch (Exception e)
            {
                _client.GetLogger().Error($"Problem while decoding a DB_UPDATE_PLR msg, skipped: {e.Message}");
            }
        }

        /// <summary>
        ///     Decode handshake to check versions
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
        ///     sendMsgToServer Helper
        ///     selects the message by its name and pushes it to the connection
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
    }
}