using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using System.Runtime.Intrinsics;
using System.Threading;
using RCC.Client;
using RCC.Config;
using RCC.Helper;
using RCC.Msg;

namespace RCC.Network
{
    public static class NetworkManager
    {
        public static bool serverReceivedReady = false;

        /// <summary>
        /// Send
        /// </summary>
        public static void send(uint gameCycle)
        {
            // wait till next server is received
            if (NetworkConnection._LastSentCycle >= gameCycle)
            {
                //nlinfo ("Try to CNetManager::send(%d) _LastSentCycle=%d more than one time with the same game cycle, so we wait new game cycle to send", gameCycle, _LastSentCycle);
                while (NetworkConnection._LastSentCycle >= gameCycle)
                {
                    // Update network.
                    update();

                    // Send dummy info
                    send();

                    // Do not take all the CPU.
                    Thread.Sleep(100);

                    gameCycle = NetworkConnection.getCurrentServerTick();
                }
            }

            NetworkConnection.send(gameCycle);
        }

        /// <summary>
        /// Buffers a bitmemstream, that will be converted into a generic action, to be sent later to the server (at next update).
        /// </summary>
        public static void push(CBitMemStream msg)
        {
            //if (PermanentlyBanned) return; LOL

            NetworkConnection.push(msg);
        }


        /// <summary>
        ///  Reset data and init the socket
        /// </summary>
        public static void reinit()
        {
            //IngameDbMngr.resetInitState();
            NetworkConnection.reinit();
        }


        /// <summary>
        /// Updates the whole connection with the frontend.
        /// Call this method evently.
        /// </summary>
        /// <returns>'true' if data were sent/received.</returns>
        public static bool update()
        {
            // Update the base class.
            bool result = NetworkConnection.update();


            // TODO:  Get changes with the update.
            // 	const vector<CChange> &changes = NetMngr.getChanges();

            // TODO:  Manage changes

            // TODO: update everyting

            return true;
        }

        /// <summary>
        /// Send
        /// </summary>
        public static void send()
        {
            NetworkConnection.send();
        }

        /// <summary>
        /// impulseCallBack :
        /// The impulse callback to receive all msg from the frontend.
        /// </summary>
        public static void impulseCallBack(CBitMemStream impulse, int packet, object arg)
        {
            GenericMsgHeaderMngr.execute(impulse);
        }

        /// <summary>
        /// initializeNetwork :
        /// </summary>
        public static void initializeNetwork()
        {
            GenericMsgHeaderMngr.setCallback("DB_UPD_PLR", impulseDatabaseUpdatePlayer);
            GenericMsgHeaderMngr.setCallback("DB_INIT:PLR", impulseDatabaseInitPlayer);
            GenericMsgHeaderMngr.setCallback("DB_UPD_INV", impulseUpdateInventory);
            GenericMsgHeaderMngr.setCallback("DB_INIT:INV", impulseInitInventory);
            GenericMsgHeaderMngr.setCallback("DB_GROUP:UPDATE_BANK", impulseDatabaseUpdateBank);
            GenericMsgHeaderMngr.setCallback("DB_GROUP:INIT_BANK", impulseDatabaseInitBank);
            GenericMsgHeaderMngr.setCallback("DB_GROUP:RESET_BANK", impulseDatabaseResetBank);
            GenericMsgHeaderMngr.setCallback("CONNECTION:NO_USER_CHAR", impulseNoUserChar);
            GenericMsgHeaderMngr.setCallback("CONNECTION:USER_CHARS", impulseUserChars);
            GenericMsgHeaderMngr.setCallback("CONNECTION:USER_CHAR", impulseUserChar);
            GenericMsgHeaderMngr.setCallback("CONNECTION:FAR_TP", impulseFarTP);
            GenericMsgHeaderMngr.setCallback("CONNECTION:READY", impulseServerReady);
            GenericMsgHeaderMngr.setCallback("CONNECTION:VALID_NAME", impulseCharNameValid);
            GenericMsgHeaderMngr.setCallback("CONNECTION:SHARD_ID", impulseShardId);
            GenericMsgHeaderMngr.setCallback("CONNECTION:SERVER_QUIT_OK", impulseServerQuitOk);
            GenericMsgHeaderMngr.setCallback("CONNECTION:SERVER_QUIT_ABORT", impulseServerQuitAbort);
            GenericMsgHeaderMngr.setCallback("CONNECTION:MAIL_AVAILABLE", impulseMailNotification);
            GenericMsgHeaderMngr.setCallback("CONNECTION:GUILD_MESSAGE_AVAILABLE", impulseForumNotification);
            GenericMsgHeaderMngr.setCallback("CONNECTION:PERMANENT_BAN", impulsePermanentBan);
            GenericMsgHeaderMngr.setCallback("CONNECTION:UNBAN", impulsePermanentUnban);

            GenericMsgHeaderMngr.setCallback("STRING:CHAT", impulseChat);
            GenericMsgHeaderMngr.setCallback("STRING:TELL", impulseTell);
            GenericMsgHeaderMngr.setCallback("STRING:FAR_TELL", impulseFarTell);
            GenericMsgHeaderMngr.setCallback("STRING:CHAT2", impulseChat2);
            GenericMsgHeaderMngr.setCallback("STRING:DYN_STRING", impulseDynString);
            GenericMsgHeaderMngr.setCallback("STRING:DYN_STRING_GROUP", inpulseDynStringInChatGroup);
            GenericMsgHeaderMngr.setCallback("STRING:TELL2", impulseTell2);
            //	GenericMsgHeaderMngr.setCallback("STRING:ADD_DYN_STR",		impulseAddDynStr);
            GenericMsgHeaderMngr.setCallback("TP:DEST", impulseTP);
            GenericMsgHeaderMngr.setCallback("TP:DEST_WITH_SEASON", impulseTPWithSeason);
            GenericMsgHeaderMngr.setCallback("TP:CORRECT", impulseCorrectPos);
            GenericMsgHeaderMngr.setCallback("COMBAT:ENGAGE_FAILED", impulseCombatEngageFailed);
            GenericMsgHeaderMngr.setCallback("BOTCHAT:DYNCHAT_OPEN", impulseDynChatOpen);
            GenericMsgHeaderMngr.setCallback("BOTCHAT:DYNCHAT_CLOSE", impulseDynChatClose);

            GenericMsgHeaderMngr.setCallback("CASTING:BEGIN", impulseBeginCast);
            GenericMsgHeaderMngr.setCallback("TEAM:INVITATION", impulseTeamInvitation);
            GenericMsgHeaderMngr.setCallback("TEAM:SHARE_OPEN", impulseTeamShareOpen);
            GenericMsgHeaderMngr.setCallback("TEAM:SHARE_INVALID", impulseTeamShareInvalid);
            GenericMsgHeaderMngr.setCallback("TEAM:SHARE_CLOSE", impulseTeamShareClose);
            GenericMsgHeaderMngr.setCallback("TEAM:CONTACT_INIT", impulseTeamContactInit);
            GenericMsgHeaderMngr.setCallback("TEAM:CONTACT_CREATE", impulseTeamContactCreate);
            GenericMsgHeaderMngr.setCallback("TEAM:CONTACT_STATUS", impulseTeamContactStatus);
            GenericMsgHeaderMngr.setCallback("TEAM:CONTACT_REMOVE", impulseTeamContactRemove);

            GenericMsgHeaderMngr.setCallback("EXCHANGE:INVITATION", impulseExchangeInvitation);
            GenericMsgHeaderMngr.setCallback("EXCHANGE:CLOSE_INVITATION", impulseExchangeCloseInvitation);
            GenericMsgHeaderMngr.setCallback("ANIMALS:MOUNT_ABORT", impulseMountAbort);

            GenericMsgHeaderMngr.setCallback("DEBUG:REPLY_WHERE", impulseWhere);
            GenericMsgHeaderMngr.setCallback("DEBUG:COUNTER", impulseCounter);

            //
            GenericMsgHeaderMngr.setCallback("STRING_MANAGER:PHRASE_SEND", impulsePhraseSend);
            GenericMsgHeaderMngr.setCallback("STRING_MANAGER:STRING_RESP", impulseStringResp);
            GenericMsgHeaderMngr.setCallback("STRING_MANAGER:RELOAD_CACHE", impulseReloadCache);
            //
            GenericMsgHeaderMngr.setCallback("BOTCHAT:FORCE_END", impulseBotChatForceEnd);

            GenericMsgHeaderMngr.setCallback("JOURNAL:INIT_COMPLETED_MISSIONS", impulseJournalInitCompletedMissions);
            GenericMsgHeaderMngr.setCallback("JOURNAL:UPDATE_COMPLETED_MISSIONS", impulseJournalUpdateCompletedMissions);
            //	GenericMsgHeaderMngr.setCallback("JOURNAL:CANT_ABANDON",				impulseJournalCantAbandon);

            GenericMsgHeaderMngr.setCallback("JOURNAL:ADD_COMPASS", impulseJournalAddCompass);
            GenericMsgHeaderMngr.setCallback("JOURNAL:REMOVE_COMPASS", impulseJournalRemoveCompass);


            //GenericMsgHeaderMngr.setCallback("GUILD:SET_MEMBER_INFO",	impulseGuildSetMemberInfo);
            //GenericMsgHeaderMngr.setCallback("GUILD:INIT_MEMBER_INFO",	impulseGuildInitMemberInfo);

            GenericMsgHeaderMngr.setCallback("GUILD:JOIN_PROPOSAL", impulseGuildJoinProposal);

            GenericMsgHeaderMngr.setCallback("GUILD:ASCENSOR", impulseGuildAscensor);
            GenericMsgHeaderMngr.setCallback("GUILD:LEAVE_ASCENSOR", impulseGuildLeaveAscensor);
            GenericMsgHeaderMngr.setCallback("GUILD:ABORT_CREATION", impulseGuildAbortCreation);
            GenericMsgHeaderMngr.setCallback("GUILD:OPEN_GUILD_WINDOW", impulseGuildOpenGuildWindow);

            GenericMsgHeaderMngr.setCallback("GUILD:OPEN_INVENTORY", impulseGuildOpenInventory);
            GenericMsgHeaderMngr.setCallback("GUILD:CLOSE_INVENTORY", impulseGuildCloseInventory);

            GenericMsgHeaderMngr.setCallback("GUILD:UPDATE_PLAYER_TITLE", impulseGuildUpdatePlayerTitle);
            GenericMsgHeaderMngr.setCallback("GUILD:USE_FEMALE_TITLES", impulseGuildUseFemaleTitles);
            //GenericMsgHeaderMngr.setCallback("GUILD:INVITATION", impulseGuildInvitation);

            GenericMsgHeaderMngr.setCallback("HARVEST:CLOSE_TEMP_INVENTORY", impulseCloseTempInv);

            GenericMsgHeaderMngr.setCallback("COMMAND:REMOTE_ADMIN", impulseRemoteAdmin);

            GenericMsgHeaderMngr.setCallback("PHRASE:DOWNLOAD", impulsePhraseDownLoad);
            GenericMsgHeaderMngr.setCallback("PHRASE:CONFIRM_BUY", impulsePhraseConfirmBuy);
            GenericMsgHeaderMngr.setCallback("PHRASE:EXEC_CYCLIC_ACK", impulsePhraseAckExecuteCyclic);
            GenericMsgHeaderMngr.setCallback("PHRASE:EXEC_NEXT_ACK", impulsePhraseAckExecuteNext);

            GenericMsgHeaderMngr.setCallback("ITEM_INFO:SET", impulseItemInfoSet);
            GenericMsgHeaderMngr.setCallback("ITEM_INFO:REFRESH_VERSION", impulseItemInfoRefreshVersion);
            GenericMsgHeaderMngr.setCallback("MISSION_PREREQ:SET", impulsePrereqInfoSet);
            GenericMsgHeaderMngr.setCallback("ITEM:OPEN_ROOM_INVENTORY", impulseItemOpenRoomInventory);
            GenericMsgHeaderMngr.setCallback("ITEM:CLOSE_ROOM_INVENTORY", impulseItemCloseRoomInventory);

            GenericMsgHeaderMngr.setCallback("DEATH:RESPAWN_POINT", impulseDeathRespawnPoint);
            GenericMsgHeaderMngr.setCallback("DEATH:RESPAWN", impulseDeathRespawn);

            GenericMsgHeaderMngr.setCallback("DUEL:INVITATION", impulseDuelInvitation);
            GenericMsgHeaderMngr.setCallback("DUEL:CANCEL_INVITATION", impulseDuelCancelInvitation);

            GenericMsgHeaderMngr.setCallback("PVP_CHALLENGE:INVITATION", impulsePVPChallengeInvitation);
            GenericMsgHeaderMngr.setCallback("PVP_CHALLENGE:CANCEL_INVITATION", impulsePVPChallengeCancelInvitation);

            GenericMsgHeaderMngr.setCallback("PVP_FACTION:PUSH_FACTION_WAR", impulsePVPFactionPushFactionWar);
            GenericMsgHeaderMngr.setCallback("PVP_FACTION:POP_FACTION_WAR", impulsePVPFactionPopFactionWar);
            GenericMsgHeaderMngr.setCallback("PVP_FACTION:FACTION_WARS", impulsePVPFactionFactionWars);


            //	GenericMsgHeaderMngr.setCallback("PVP_VERSUS:CHOOSE_CLAN",	impulsePVPChooseClan);

            GenericMsgHeaderMngr.setCallback("ENCYCLOPEDIA:UPDATE", impulseEncyclopediaUpdate);
            GenericMsgHeaderMngr.setCallback("ENCYCLOPEDIA:INIT", impulseEncyclopediaInit);

            GenericMsgHeaderMngr.setCallback("USER:BARS", impulseUserBars);
            GenericMsgHeaderMngr.setCallback("USER:POPUP", impulseUserPopup);


            GenericMsgHeaderMngr.setCallback("MISSION:ASK_ENTER_CRITICAL", impulseEnterCrZoneProposal);
            GenericMsgHeaderMngr.setCallback("MISSION:CLOSE_ENTER_CRITICAL", impulseCloseEnterCrZoneProposal);

            // Module gateway message
            GenericMsgHeaderMngr.setCallback("MODULE_GATEWAY:FEOPEN", cbImpulsionGatewayOpen);
            GenericMsgHeaderMngr.setCallback("MODULE_GATEWAY:GATEWAY_MSG", cbImpulsionGatewayMessage);
            GenericMsgHeaderMngr.setCallback("MODULE_GATEWAY:FECLOSE", cbImpulsionGatewayClose);

            GenericMsgHeaderMngr.setCallback("OUTPOST:CHOOSE_SIDE", impulseOutpostChooseSide);
            GenericMsgHeaderMngr.setCallback("OUTPOST:DECLARE_WAR_ACK", impulseOutpostDeclareWarAck);

            GenericMsgHeaderMngr.setCallback("COMBAT:FLYING_HP_DELTA", impulseCombatFlyingHpDelta);
            GenericMsgHeaderMngr.setCallback("COMBAT:FLYING_TEXT_ISE", impulseCombatFlyingTextItemSpecialEffectProc);
            GenericMsgHeaderMngr.setCallback("COMBAT:FLYING_TEXT", impulseCombatFlyingText);

            GenericMsgHeaderMngr.setCallback("SEASON:SET", impulseSetSeason);
            GenericMsgHeaderMngr.setCallback("RING_MISSION:DSS_DOWN", impulseDssDown);

            GenericMsgHeaderMngr.setCallback("NPC_ICON:SET_DESC", impulseSetNpcIconDesc);
            GenericMsgHeaderMngr.setCallback("NPC_ICON:SVR_EVENT_MIS_AVL", impulseServerEventForMissionAvailability);
            GenericMsgHeaderMngr.setCallback("NPC_ICON:SET_TIMER", impulseSetNpcIconTimer);
        }

        private static void impulseSetNpcIconTimer(CBitMemStream impulse)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseServerEventForMissionAvailability(CBitMemStream impulse)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseSetNpcIconDesc(CBitMemStream impulse)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseDssDown(CBitMemStream impulse)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseSetSeason(CBitMemStream impulse)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseCombatFlyingText(CBitMemStream impulse)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseCombatFlyingTextItemSpecialEffectProc(CBitMemStream impulse)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseCombatFlyingHpDelta(CBitMemStream impulse)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void cbImpulsionGatewayMessage(CBitMemStream impulse)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseOutpostDeclareWarAck(CBitMemStream impulse)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseOutpostChooseSide(CBitMemStream impulse)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void cbImpulsionGatewayClose(CBitMemStream impulse)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void cbImpulsionGatewayOpen(CBitMemStream impulse)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseCloseEnterCrZoneProposal(CBitMemStream impulse)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseEnterCrZoneProposal(CBitMemStream impulse)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseUserPopup(CBitMemStream impulse)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseUserBars(CBitMemStream impulse)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseEncyclopediaInit(CBitMemStream impulse)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseEncyclopediaUpdate(CBitMemStream impulse)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulsePVPFactionFactionWars(CBitMemStream impulse)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulsePVPFactionPopFactionWar(CBitMemStream impulse)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulsePVPFactionPushFactionWar(CBitMemStream impulse)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulsePVPChallengeCancelInvitation(CBitMemStream impulse)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulsePVPChallengeInvitation(CBitMemStream impulse)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseDuelCancelInvitation(CBitMemStream impulse)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseDuelInvitation(CBitMemStream impulse)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseDeathRespawn(CBitMemStream impulse)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseDeathRespawnPoint(CBitMemStream impulse)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseItemCloseRoomInventory(CBitMemStream impulse)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseItemOpenRoomInventory(CBitMemStream impulse)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseItemInfoRefreshVersion(CBitMemStream impulse)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulsePrereqInfoSet(CBitMemStream impulse)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseItemInfoSet(CBitMemStream impulse)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulsePhraseAckExecuteNext(CBitMemStream impulse)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulsePhraseAckExecuteCyclic(CBitMemStream impulse)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulsePhraseConfirmBuy(CBitMemStream impulse)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulsePhraseDownLoad(CBitMemStream impulse)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseRemoteAdmin(CBitMemStream impulse)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseCloseTempInv(CBitMemStream impulse)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseGuildUseFemaleTitles(CBitMemStream impulse)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseGuildUpdatePlayerTitle(CBitMemStream impulse)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseGuildCloseInventory(CBitMemStream impulse)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseGuildOpenInventory(CBitMemStream impulse)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseGuildOpenGuildWindow(CBitMemStream impulse)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseGuildAbortCreation(CBitMemStream impulse)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseGuildLeaveAscensor(CBitMemStream impulse)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseGuildAscensor(CBitMemStream impulse)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseGuildJoinProposal(CBitMemStream impulse)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseJournalRemoveCompass(CBitMemStream impulse)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseJournalAddCompass(CBitMemStream impulse)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseJournalUpdateCompletedMissions(CBitMemStream impulse)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseJournalInitCompletedMissions(CBitMemStream impulse)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseBotChatForceEnd(CBitMemStream impulse)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        /// <summary>
        /// reload the string cache
        /// </summary>
        private static void impulseReloadCache(CBitMemStream impulse)
        {
            int timestamp = 0;
            impulse.serial(ref timestamp);

            //if (PermanentlyBanned) return; <- haha
            //CStringManagerClient.loadCache(timestamp);

            // todo: CStringManagerClient.loadCache(timestamp)

            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name + " with timestamp " + timestamp);
        }

        private static void impulseStringResp(CBitMemStream impulse)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        /// <summary>
        /// A dyn string (or phrase) is send (so, we receive it)
        /// </summary>
        private static void impulsePhraseSend(CBitMemStream impulse)
        {
            CStringManagerClient.receiveDynString(impulse);
        }

        private static void impulseCounter(CBitMemStream impulse)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseWhere(CBitMemStream impulse)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseMountAbort(CBitMemStream impulse)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseExchangeCloseInvitation(CBitMemStream impulse)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseExchangeInvitation(CBitMemStream impulse)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseTeamContactRemove(CBitMemStream impulse)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseTeamContactStatus(CBitMemStream impulse)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseTeamContactCreate(CBitMemStream impulse)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseTeamContactInit(CBitMemStream impulse)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseTeamShareClose(CBitMemStream impulse)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseTeamShareInvalid(CBitMemStream impulse)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseTeamShareOpen(CBitMemStream impulse)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseTeamInvitation(CBitMemStream impulse)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseBeginCast(CBitMemStream impulse)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseDynChatClose(CBitMemStream impulse)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseDynChatOpen(CBitMemStream impulse)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseCombatEngageFailed(CBitMemStream impulse)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseCorrectPos(CBitMemStream impulse)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseTPWithSeason(CBitMemStream impulse)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseTP(CBitMemStream impulse)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseTell2(CBitMemStream impulse)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void inpulseDynStringInChatGroup(CBitMemStream impulse)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseDynString(CBitMemStream impulse)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseChat2(CBitMemStream impulse)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseFarTell(CBitMemStream impulse)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseTell(CBitMemStream impulse)
        {
            ChatMngr.processTellString(impulse, null);
        }

        private static void impulseChat(CBitMemStream impulse)
        {
            ChatMngr.processChatString(impulse, null);
        }

        private static void impulsePermanentUnban(CBitMemStream impulse)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulsePermanentBan(CBitMemStream impulse)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseForumNotification(CBitMemStream impulse)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseMailNotification(CBitMemStream impulse)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseServerQuitAbort(CBitMemStream impulse)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseServerQuitOk(CBitMemStream impulse)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseShardId(CBitMemStream impulse)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseCharNameValid(CBitMemStream impulse)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        static void checkHandshake(CBitMemStream impulse)
        {
            // Decode handshake to check versions
            uint handshakeVersion = 0;
            uint itemSlotVersion = 0;
            impulse.serial(ref handshakeVersion, 2);
            //if (handshakeVersion > 0)
            //    nlerror("Server handshake version is more recent than client one");
            impulse.serial(ref itemSlotVersion, 2);
            //if (itemSlotVersion != INVENTORIES::CItemSlot::getVersion())
            //    nlerror("Handshake: itemSlotVersion mismatch (S:%hu C:%hu)", itemSlotVersion, INVENTORIES::CItemSlot::getVersion());
        }


        private static void impulseServerReady(CBitMemStream impulse)
        {
            ConsoleIO.WriteLineFormatted("§eimpulse on " + MethodBase.GetCurrentMethod().Name);

            serverReceivedReady = true;

            checkHandshake(impulse);

            //LoginSM.pushEvent(CLoginStateMachine::ev_ready_received);
        }

        private static void impulseFarTP(CBitMemStream impulse)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }



        private static void impulseUserChar(CBitMemStream impulse)
        {
            //// received USER_CHAR
            ConsoleIO.WriteLineFormatted("impulseCallBack : Received CONNECTION:USER_CHAR");
            //
            //// Serialize the message
            //COfflineEntityState posState;
            //extern uint8 ServerSeasonValue;
            //extern bool ServerSeasonReceived;
            //uint32 userRole;

            int X = 0;
            int Y = 0;
            int Z = 0;

            int HeadingI = 0;

            var s = impulse;
            var f = s;

            f.serial(ref X);
            f.serial(ref Y);
            f.serial(ref Z);
            f.serial(ref HeadingI);

            float Heading = Misc.Int32BitsToSingle(HeadingI);

            short v = 0;
            s.serial(ref v, 3);
            var season = v;
            v = 0;
            s.serial(ref v, 3);
            var userRole = (v & 0x3); // bits 0-1
            var isInRingSession = ((v & 0x4) != 0); // bit 2

            int highestMainlandSessionId = 0;
            int firstConnectedTime = 0;
            int playedTime = 0;

            s.serial(ref highestMainlandSessionId);
            s.serial(ref firstConnectedTime);
            s.serial(ref playedTime);

            //ServerSeasonReceived = true; // set the season that will be used when selecting the continent from the position
            //
            //if (UserEntity)
            //{
            //    UserEntity->pos(CVectorD((float)posState.X / 1000.0f, (float)posState.Y / 1000.0f, (float)posState.Z / 1000.0f));
            //    UserEntity->front(CVector((float)cos(posState.Heading), (float)sin(posState.Heading), 0.f));
            //    UserEntity->dir(UserEntity->front());
            //    UserEntity->setHeadPitch(0);
            //    UserControls.resetCameraDeltaYaw();
            //    //nldebug("<impulseUserChar> pos : %f %f %f  heading : %f",UserEntity->pos().x,UserEntity->pos().y,UserEntity->pos().z,posState.Heading);
            //
            //    // Update the position for the vision.
            //    NetMngr.setReferencePosition(UserEntity->pos());
            //}
            //else
            //{
            var UserEntityInitPos = new Vector3((float)X / 1000.0f, (float)Y / 1000.0f, (float)Z / 1000.0f);
            var UserEntityInitFront = new Vector3((float)Math.Cos(Heading), (float)Math.Sin(Heading), 0f);

            ConsoleIO.WriteLineFormatted($"§d<impulseUserChar> pos : {UserEntityInitPos}  heading : {Heading}");

            // Update the position for the vision.
            //NetworkManager.setReferencePosition(UserEntityInitPos);
            //}

            RyzomClient.UserCharPosReceived = true;

            //// Configure the ring editor
            //extern R2::TUserRole UserRoleInSession;
            //UserRoleInSession = R2::TUserRole::TValues(userRole);
            //ClientCfg.R2EDEnabled = IsInRingSession /*&& (UserRoleInSession.getValue() != R2::TUserRole::ur_player)*/;
            //// !!!Do NOT uncomment the following line  do the  ClientCfg.R2EDEnabled = IsInRingSession && (UserRoleInSession != R2::TUserRole::ur_player);
            //// even with UserRoleInSession R2::TUserRole::ur_player the ring features must be activated
            //// because if the ring is not activated the dss do not know the existence of the player
            //// So we can not kick him, tp to him, tp in to next act ....
            //nldebug("EnableR2Ed = %u, IsInRingSession = %u, UserRoleInSession = %u", (uint)ClientCfg.R2EDEnabled, (uint)IsInRingSession, userRole);

            // updatePatcherPriorityBasedOnCharacters();

        }

        private static void impulseUserChars(CBitMemStream impulse)
        {
            // received USER_CHARS
            ConsoleIO.WriteLine("impulseCallBack : Received CONNECTION:USER_CHARS");

            impulse.serial(ref Connection.ServerPeopleActive);
            impulse.serial(ref Connection.ServerCareerActive);
            // read characters summary
            Connection.CharacterSummaries.Clear();

            // START WORKAROUND workaround for serialVector(T &cont) in stream.h TODO
            int len = 0;
            impulse.serial(ref len);

            for (var i = 0; i < len; i++)
            {
                var cs = new CCharacterSummary();
                cs.serial(impulse);
                ConsoleIO.WriteLineFormatted("§eFound character " + cs.Name + " from shard " + cs.Mainland + " in slot " + i);
                Connection.CharacterSummaries.Add(cs);
            }
            // END WORKAROUND


            //LoginSM.pushEvent(CLoginStateMachine::ev_chars_received);
            ConsoleIO.WriteLineFormatted("st_ingame->st_select_char");
            Connection.SendCharSelection = true;

            //// Create the message for the server to select the first character.
            //var outP = new CBitMemStream(false);
            //
            //if (GenericMsgHeaderMngr.pushNameToStream("CONNECTION:SELECT_CHAR", outP))
            //{
            //    byte c = 0;
            //    outP.serial(ref c);
            //
            //    push(outP);
            //    send(NetworkConnection.getCurrentServerTick());
            //    // send CONNECTION:USER_CHARS
            //    ConsoleIO.WriteLineFormatted("impulseCallBack : CONNECTION:SELECT_CHAR sent");
            //}
            //else
            //{
            //    ConsoleIO.WriteLineFormatted("§cimpulseCallBack : unknown message name : 'CONNECTION:SELECT_CHAR'.");
            //}

            // (FarTP) noUserChar = true; TODO ???

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
        }

        private static void impulseNoUserChar(CBitMemStream impulse)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseDatabaseResetBank(CBitMemStream impulse)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseDatabaseInitBank(CBitMemStream impulse)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseDatabaseUpdateBank(CBitMemStream impulse)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseInitInventory(CBitMemStream impulse)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseUpdateInventory(CBitMemStream impulse)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        enum TCDBBank { CDBPlayer, CDBGuild, /* CDBContinent, */ CDBOutpost, /* CDBGlobal, */ NB_CDB_BANKS, INVALID_CDB_BANK };


        private static void impulseDatabaseInitPlayer(CBitMemStream impulse)
        {
            try
            {
                int p = impulse.Pos;

                // get the egs tick of this change
                int serverTick = 0;
                impulse.serial(ref serverTick);

                // read delta
                // TODO: IngameDbMngr.readDelta + setInitPacketReceived
                //IngameDbMngr.readDelta(serverTick, impulse, TCDBBank.CDBPlayer);
                //IngameDbMngr.setInitPacketReceived();
                ConsoleIO.WriteLine("DB_INIT:PLR done (" + (impulse.Pos - p) + " bytes)");
            }
            catch (Exception e)
            {
                //BOMB(NLMISC::toString("Problem while decoding a DB_INIT:PLR msg, skipped: %s", e.what()), return );
                throw;
            }
        }

        private static void impulseDatabaseUpdatePlayer(CBitMemStream impulse)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }
    }
}