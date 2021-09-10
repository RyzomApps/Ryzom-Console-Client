using System;
using System.Reflection;
using System.Threading;

namespace RCC
{
    public static class NetworkManager
    {
        public static bool serverReceivedReady = false;

        private static int _LastSentCycle;

        /// <summary>
        /// Send
        /// </summary>
        public static void send(int gameCycle)
        {
            // wait till next server is received
            if (_LastSentCycle >= gameCycle)
            {
                //nlinfo ("Try to CNetManager::send(%d) _LastSentCycle=%d more than one time with the same game cycle, so we wait new game cycle to send", gameCycle, _LastSentCycle);
                while (_LastSentCycle >= gameCycle)
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
        /// Updates the whole connection with the frontend.
        /// Call this method evently.
        /// </summary>
        /// <returns>'true' if data were sent/received.</returns>
        public static bool update()
        {
            // Update the base class.
            bool result = NetworkConnection.update();

            // Get changes with the update.
            // 	const vector<CChange> &changes = NetMngr.getChanges();
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

        private static void impulseSetNpcIconTimer(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseServerEventForMissionAvailability(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseSetNpcIconDesc(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseDssDown(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseSetSeason(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseCombatFlyingText(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseCombatFlyingTextItemSpecialEffectProc(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseCombatFlyingHpDelta(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void cbImpulsionGatewayMessage(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseOutpostDeclareWarAck(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseOutpostChooseSide(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void cbImpulsionGatewayClose(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void cbImpulsionGatewayOpen(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseCloseEnterCrZoneProposal(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseEnterCrZoneProposal(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseUserPopup(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseUserBars(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseEncyclopediaInit(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseEncyclopediaUpdate(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulsePVPFactionFactionWars(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulsePVPFactionPopFactionWar(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulsePVPFactionPushFactionWar(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulsePVPChallengeCancelInvitation(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulsePVPChallengeInvitation(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseDuelCancelInvitation(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseDuelInvitation(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseDeathRespawn(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseDeathRespawnPoint(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseItemCloseRoomInventory(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseItemOpenRoomInventory(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseItemInfoRefreshVersion(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulsePrereqInfoSet(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseItemInfoSet(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulsePhraseAckExecuteNext(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulsePhraseAckExecuteCyclic(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulsePhraseConfirmBuy(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulsePhraseDownLoad(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseRemoteAdmin(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseCloseTempInv(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseGuildUseFemaleTitles(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseGuildUpdatePlayerTitle(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseGuildCloseInventory(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseGuildOpenInventory(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseGuildOpenGuildWindow(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseGuildAbortCreation(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseGuildLeaveAscensor(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseGuildAscensor(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseGuildJoinProposal(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseJournalRemoveCompass(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseJournalAddCompass(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseJournalUpdateCompletedMissions(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseJournalInitCompletedMissions(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseBotChatForceEnd(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseReloadCache(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseStringResp(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulsePhraseSend(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseCounter(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseWhere(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseMountAbort(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseExchangeCloseInvitation(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseExchangeInvitation(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseTeamContactRemove(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseTeamContactStatus(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseTeamContactCreate(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseTeamContactInit(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseTeamShareClose(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseTeamShareInvalid(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseTeamShareOpen(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseTeamInvitation(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseBeginCast(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseDynChatClose(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseDynChatOpen(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseCombatEngageFailed(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseCorrectPos(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseTPWithSeason(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseTP(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseTell2(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void inpulseDynStringInChatGroup(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseDynString(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseChat2(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseFarTell(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseTell(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseChat(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulsePermanentUnban(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulsePermanentBan(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseForumNotification(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseMailNotification(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseServerQuitAbort(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseServerQuitOk(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseShardId(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseCharNameValid(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseServerReady(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseFarTP(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseUserChar(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseUserChars(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseNoUserChar(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseDatabaseResetBank(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseDatabaseInitBank(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseDatabaseUpdateBank(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseInitInventory(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseUpdateInventory(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseDatabaseInitPlayer(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }

        private static void impulseDatabaseUpdatePlayer(CBitMemStream obj)
        {
            ConsoleIO.WriteLine("impulse on " + MethodBase.GetCurrentMethod().Name);
        }
    }
}