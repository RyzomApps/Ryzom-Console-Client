using System;
using System.Numerics;
using System.Reflection;
using System.Threading;
using RCC.Client;
using RCC.Helper;
using RCC.Messages;

namespace RCC.Network
{
    /// <summary>
    ///     used to control the connection and implements the impulse callbacks from the connection
    /// </summary>
    public static class NetworkManager
    {
        public static bool ServerReceivedReady;

        /// <summary>
        ///     Send - updates when packets were received
        /// </summary>
        public static void Send(uint gameCycle)
        {
            // wait till next server is received
            if (NetworkConnection.LastSentCycle >= gameCycle)
            {
                //nlinfo ("Try to CNetManager::send(%d) _LastSentCycle=%d more than one time with the same game cycle, so we wait new game cycle to send", gameCycle, _LastSentCycle);
                while (NetworkConnection.LastSentCycle >= gameCycle)
                {
                    // Update network.
                    Update();

                    // Send dummy info
                    Send();

                    // Do not take all the CPU.
                    Thread.Sleep(100);

                    gameCycle = NetworkConnection.GetCurrentServerTick();
                }
            }

            NetworkConnection.Send(gameCycle);
        }

        /// <summary>
        ///     Buffers a bitmemstream, that will be converted into a generic action, to be sent later to the server (at next
        ///     update).
        /// </summary>
        public static void Push(BitMemoryStream msg)
        {
            //if (PermanentlyBanned) return; LOL

            NetworkConnection.Push(msg);
        }


        /// <summary>
        ///     Reset data and init the socket
        /// </summary>
        public static void ReInit()
        {
            //IngameDbMngr.resetInitState();
            NetworkConnection.ReInit();
        }


        /// <summary>
        ///     Updates the whole connection with the frontend.
        ///     Call this method evently.
        /// </summary>
        /// <returns>'true' if data were sent/received.</returns>
        public static bool Update()
        {
            // Update the base class.
            NetworkConnection.Update();

            // TODO:  Get changes with the update.
            // 	const vector<CChange> &changes = NetMngr.getChanges();

            // TODO:  Manage changes

            // TODO: update everyting

            return true;
        }

        /// <summary>
        ///     Send updates
        /// </summary>
        public static void Send()
        {
            NetworkConnection.Send();
        }

        /// <summary>
        ///     ImpulseCallBack :
        ///     The Impulse callback to receive all msg from the frontend.
        /// </summary>
        public static void ImpulseCallBack(BitMemoryStream impulse)
        {
            GenericMessageHeaderManager.Execute(impulse);
        }

        /// <summary>
        ///     initializeNetwork :
        /// </summary>
        public static void InitializeNetwork()
        {
            GenericMessageHeaderManager.SetCallback("DB_UPD_PLR", ImpulseDatabaseUpdatePlayer);
            GenericMessageHeaderManager.SetCallback("DB_INIT:PLR", ImpulseDatabaseInitPlayer);
            GenericMessageHeaderManager.SetCallback("DB_UPD_INV", ImpulseUpdateInventory);
            GenericMessageHeaderManager.SetCallback("DB_INIT:INV", ImpulseInitInventory);
            GenericMessageHeaderManager.SetCallback("DB_GROUP:UPDATE_BANK", ImpulseDatabaseUpdateBank);
            GenericMessageHeaderManager.SetCallback("DB_GROUP:INIT_BANK", ImpulseDatabaseInitBank);
            GenericMessageHeaderManager.SetCallback("DB_GROUP:RESET_BANK", ImpulseDatabaseResetBank);
            GenericMessageHeaderManager.SetCallback("CONNECTION:NO_USER_CHAR", ImpulseNoUserChar);
            GenericMessageHeaderManager.SetCallback("CONNECTION:USER_CHARS", ImpulseUserChars);
            GenericMessageHeaderManager.SetCallback("CONNECTION:USER_CHAR", ImpulseUserChar);
            GenericMessageHeaderManager.SetCallback("CONNECTION:FAR_TP", ImpulseFarTp);
            GenericMessageHeaderManager.SetCallback("CONNECTION:READY", ImpulseServerReady);
            GenericMessageHeaderManager.SetCallback("CONNECTION:VALID_NAME", ImpulseCharNameValid);
            GenericMessageHeaderManager.SetCallback("CONNECTION:SHARD_ID", ImpulseShardId);
            GenericMessageHeaderManager.SetCallback("CONNECTION:SERVER_QUIT_OK", ImpulseServerQuitOk);
            GenericMessageHeaderManager.SetCallback("CONNECTION:SERVER_QUIT_ABORT", ImpulseServerQuitAbort);
            GenericMessageHeaderManager.SetCallback("CONNECTION:MAIL_AVAILABLE", ImpulseMailNotification);
            GenericMessageHeaderManager.SetCallback("CONNECTION:GUILD_MESSAGE_AVAILABLE", ImpulseForumNotification);
            GenericMessageHeaderManager.SetCallback("CONNECTION:PERMANENT_BAN", ImpulsePermanentBan);
            GenericMessageHeaderManager.SetCallback("CONNECTION:UNBAN", ImpulsePermanentUnban);

            GenericMessageHeaderManager.SetCallback("STRING:CHAT", ImpulseChat);
            GenericMessageHeaderManager.SetCallback("STRING:TELL", ImpulseTell);
            GenericMessageHeaderManager.SetCallback("STRING:FAR_TELL", ImpulseFarTell);
            GenericMessageHeaderManager.SetCallback("STRING:CHAT2", ImpulseChat2);
            GenericMessageHeaderManager.SetCallback("STRING:DYN_STRING", ImpulseDynString);
            GenericMessageHeaderManager.SetCallback("STRING:DYN_STRING_GROUP", ImpulseDynStringInChatGroup);
            GenericMessageHeaderManager.SetCallback("STRING:TELL2", ImpulseTell2);
            //	GenericMsgHeaderMngr.setCallback("STRING:ADD_DYN_STR",		ImpulseAddDynStr);
            GenericMessageHeaderManager.SetCallback("TP:DEST", ImpulseTp);
            GenericMessageHeaderManager.SetCallback("TP:DEST_WITH_SEASON", ImpulseTpWithSeason);
            GenericMessageHeaderManager.SetCallback("TP:CORRECT", ImpulseCorrectPos);
            GenericMessageHeaderManager.SetCallback("COMBAT:ENGAGE_FAILED", ImpulseCombatEngageFailed);
            GenericMessageHeaderManager.SetCallback("BOTCHAT:DYNCHAT_OPEN", ImpulseDynChatOpen);
            GenericMessageHeaderManager.SetCallback("BOTCHAT:DYNCHAT_CLOSE", ImpulseDynChatClose);

            GenericMessageHeaderManager.SetCallback("CASTING:BEGIN", ImpulseBeginCast);
            GenericMessageHeaderManager.SetCallback("TEAM:INVITATION", ImpulseTeamInvitation);
            GenericMessageHeaderManager.SetCallback("TEAM:SHARE_OPEN", ImpulseTeamShareOpen);
            GenericMessageHeaderManager.SetCallback("TEAM:SHARE_INVALID", ImpulseTeamShareInvalid);
            GenericMessageHeaderManager.SetCallback("TEAM:SHARE_CLOSE", ImpulseTeamShareClose);
            GenericMessageHeaderManager.SetCallback("TEAM:CONTACT_INIT", ImpulseTeamContactInit);
            GenericMessageHeaderManager.SetCallback("TEAM:CONTACT_CREATE", ImpulseTeamContactCreate);
            GenericMessageHeaderManager.SetCallback("TEAM:CONTACT_STATUS", ImpulseTeamContactStatus);
            GenericMessageHeaderManager.SetCallback("TEAM:CONTACT_REMOVE", ImpulseTeamContactRemove);

            GenericMessageHeaderManager.SetCallback("EXCHANGE:INVITATION", ImpulseExchangeInvitation);
            GenericMessageHeaderManager.SetCallback("EXCHANGE:CLOSE_INVITATION", ImpulseExchangeCloseInvitation);
            GenericMessageHeaderManager.SetCallback("ANIMALS:MOUNT_ABORT", ImpulseMountAbort);

            GenericMessageHeaderManager.SetCallback("DEBUG:REPLY_WHERE", ImpulseWhere);
            GenericMessageHeaderManager.SetCallback("DEBUG:COUNTER", ImpulseCounter);

            //
            GenericMessageHeaderManager.SetCallback("STRING_MANAGER:PHRASE_SEND", ImpulsePhraseSend);
            GenericMessageHeaderManager.SetCallback("STRING_MANAGER:STRING_RESP", ImpulseStringResp);
            GenericMessageHeaderManager.SetCallback("STRING_MANAGER:RELOAD_CACHE", ImpulseReloadCache);
            //
            GenericMessageHeaderManager.SetCallback("BOTCHAT:FORCE_END", ImpulseBotChatForceEnd);

            GenericMessageHeaderManager.SetCallback("JOURNAL:INIT_COMPLETED_MISSIONS",
                ImpulseJournalInitCompletedMissions);
            GenericMessageHeaderManager.SetCallback("JOURNAL:UPDATE_COMPLETED_MISSIONS",
                ImpulseJournalUpdateCompletedMissions);
            //	GenericMsgHeaderMngr.setCallback("JOURNAL:CANT_ABANDON",				ImpulseJournalCantAbandon);

            GenericMessageHeaderManager.SetCallback("JOURNAL:ADD_COMPASS", ImpulseJournalAddCompass);
            GenericMessageHeaderManager.SetCallback("JOURNAL:REMOVE_COMPASS", ImpulseJournalRemoveCompass);


            //GenericMsgHeaderMngr.setCallback("GUILD:SET_MEMBER_INFO",	ImpulseGuildSetMemberInfo);
            //GenericMsgHeaderMngr.setCallback("GUILD:INIT_MEMBER_INFO",	ImpulseGuildInitMemberInfo);

            GenericMessageHeaderManager.SetCallback("GUILD:JOIN_PROPOSAL", ImpulseGuildJoinProposal);

            GenericMessageHeaderManager.SetCallback("GUILD:ASCENSOR", ImpulseGuildAscensor);
            GenericMessageHeaderManager.SetCallback("GUILD:LEAVE_ASCENSOR", ImpulseGuildLeaveAscensor);
            GenericMessageHeaderManager.SetCallback("GUILD:ABORT_CREATION", ImpulseGuildAbortCreation);
            GenericMessageHeaderManager.SetCallback("GUILD:OPEN_GUILD_WINDOW", ImpulseGuildOpenGuildWindow);

            GenericMessageHeaderManager.SetCallback("GUILD:OPEN_INVENTORY", ImpulseGuildOpenInventory);
            GenericMessageHeaderManager.SetCallback("GUILD:CLOSE_INVENTORY", ImpulseGuildCloseInventory);

            GenericMessageHeaderManager.SetCallback("GUILD:UPDATE_PLAYER_TITLE", ImpulseGuildUpdatePlayerTitle);
            GenericMessageHeaderManager.SetCallback("GUILD:USE_FEMALE_TITLES", ImpulseGuildUseFemaleTitles);
            //GenericMsgHeaderMngr.setCallback("GUILD:INVITATION", ImpulseGuildInvitation);

            GenericMessageHeaderManager.SetCallback("HARVEST:CLOSE_TEMP_INVENTORY", ImpulseCloseTempInv);

            GenericMessageHeaderManager.SetCallback("COMMAND:REMOTE_ADMIN", ImpulseRemoteAdmin);

            GenericMessageHeaderManager.SetCallback("PHRASE:DOWNLOAD", ImpulsePhraseDownLoad);
            GenericMessageHeaderManager.SetCallback("PHRASE:CONFIRM_BUY", ImpulsePhraseConfirmBuy);
            GenericMessageHeaderManager.SetCallback("PHRASE:EXEC_CYCLIC_ACK", ImpulsePhraseAckExecuteCyclic);
            GenericMessageHeaderManager.SetCallback("PHRASE:EXEC_NEXT_ACK", ImpulsePhraseAckExecuteNext);

            GenericMessageHeaderManager.SetCallback("ITEM_INFO:SET", ImpulseItemInfoSet);
            GenericMessageHeaderManager.SetCallback("ITEM_INFO:REFRESH_VERSION", ImpulseItemInfoRefreshVersion);
            GenericMessageHeaderManager.SetCallback("MISSION_PREREQ:SET", ImpulsePrereqInfoSet);
            GenericMessageHeaderManager.SetCallback("ITEM:OPEN_ROOM_INVENTORY", ImpulseItemOpenRoomInventory);
            GenericMessageHeaderManager.SetCallback("ITEM:CLOSE_ROOM_INVENTORY", ImpulseItemCloseRoomInventory);

            GenericMessageHeaderManager.SetCallback("DEATH:RESPAWN_POINT", ImpulseDeathRespawnPoint);
            GenericMessageHeaderManager.SetCallback("DEATH:RESPAWN", ImpulseDeathRespawn);

            GenericMessageHeaderManager.SetCallback("DUEL:INVITATION", ImpulseDuelInvitation);
            GenericMessageHeaderManager.SetCallback("DUEL:CANCEL_INVITATION", ImpulseDuelCancelInvitation);

            GenericMessageHeaderManager.SetCallback("PVP_CHALLENGE:INVITATION", ImpulsePvpChallengeInvitation);
            GenericMessageHeaderManager.SetCallback("PVP_CHALLENGE:CANCEL_INVITATION",
                ImpulsePvpChallengeCancelInvitation);

            GenericMessageHeaderManager.SetCallback("PVP_FACTION:PUSH_FACTION_WAR", ImpulsePvpFactionPushFactionWar);
            GenericMessageHeaderManager.SetCallback("PVP_FACTION:POP_FACTION_WAR", ImpulsePvpFactionPopFactionWar);
            GenericMessageHeaderManager.SetCallback("PVP_FACTION:FACTION_WARS", ImpulsePvpFactionFactionWars);


            //	GenericMsgHeaderMngr.setCallback("PVP_VERSUS:CHOOSE_CLAN",	ImpulsePVPChooseClan);

            GenericMessageHeaderManager.SetCallback("ENCYCLOPEDIA:UPDATE", ImpulseEncyclopediaUpdate);
            GenericMessageHeaderManager.SetCallback("ENCYCLOPEDIA:INIT", ImpulseEncyclopediaInit);

            GenericMessageHeaderManager.SetCallback("USER:BARS", ImpulseUserBars);
            GenericMessageHeaderManager.SetCallback("USER:POPUP", ImpulseUserPopup);


            GenericMessageHeaderManager.SetCallback("MISSION:ASK_ENTER_CRITICAL", ImpulseEnterCrZoneProposal);
            GenericMessageHeaderManager.SetCallback("MISSION:CLOSE_ENTER_CRITICAL", ImpulseCloseEnterCrZoneProposal);

            // Module gateway message
            GenericMessageHeaderManager.SetCallback("MODULE_GATEWAY:FEOPEN", CbImpulsionGatewayOpen);
            GenericMessageHeaderManager.SetCallback("MODULE_GATEWAY:GATEWAY_MSG", CbImpulsionGatewayMessage);
            GenericMessageHeaderManager.SetCallback("MODULE_GATEWAY:FECLOSE", CbImpulsionGatewayClose);

            GenericMessageHeaderManager.SetCallback("OUTPOST:CHOOSE_SIDE", ImpulseOutpostChooseSide);
            GenericMessageHeaderManager.SetCallback("OUTPOST:DECLARE_WAR_ACK", ImpulseOutpostDeclareWarAck);

            GenericMessageHeaderManager.SetCallback("COMBAT:FLYING_HP_DELTA", ImpulseCombatFlyingHpDelta);
            GenericMessageHeaderManager.SetCallback("COMBAT:FLYING_TEXT_ISE",
                ImpulseCombatFlyingTextItemSpecialEffectProc);
            GenericMessageHeaderManager.SetCallback("COMBAT:FLYING_TEXT", ImpulseCombatFlyingText);

            GenericMessageHeaderManager.SetCallback("SEASON:SET", ImpulseSetSeason);
            GenericMessageHeaderManager.SetCallback("RING_MISSION:DSS_DOWN", ImpulseDssDown);

            GenericMessageHeaderManager.SetCallback("NPC_ICON:SET_DESC", ImpulseSetNpcIconDesc);
            GenericMessageHeaderManager.SetCallback("NPC_ICON:SVR_EVENT_MIS_AVL",
                ImpulseServerEventForMissionAvailability);
            GenericMessageHeaderManager.SetCallback("NPC_ICON:SET_TIMER", ImpulseSetNpcIconTimer);
        }

        private static void ImpulseSetNpcIconTimer(BitMemoryStream impulse)
        {
            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private static void ImpulseServerEventForMissionAvailability(BitMemoryStream impulse)
        {
            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private static void ImpulseSetNpcIconDesc(BitMemoryStream impulse)
        {
            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private static void ImpulseDssDown(BitMemoryStream impulse)
        {
            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private static void ImpulseSetSeason(BitMemoryStream impulse)
        {
            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private static void ImpulseCombatFlyingText(BitMemoryStream impulse)
        {
            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private static void ImpulseCombatFlyingTextItemSpecialEffectProc(BitMemoryStream impulse)
        {
            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private static void ImpulseCombatFlyingHpDelta(BitMemoryStream impulse)
        {
            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private static void CbImpulsionGatewayMessage(BitMemoryStream impulse)
        {
            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private static void ImpulseOutpostDeclareWarAck(BitMemoryStream impulse)
        {
            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private static void ImpulseOutpostChooseSide(BitMemoryStream impulse)
        {
            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private static void CbImpulsionGatewayClose(BitMemoryStream impulse)
        {
            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private static void CbImpulsionGatewayOpen(BitMemoryStream impulse)
        {
            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private static void ImpulseCloseEnterCrZoneProposal(BitMemoryStream impulse)
        {
            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private static void ImpulseEnterCrZoneProposal(BitMemoryStream impulse)
        {
            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private static void ImpulseUserPopup(BitMemoryStream impulse)
        {
            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private static void ImpulseUserBars(BitMemoryStream impulse)
        {
            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private static void ImpulseEncyclopediaInit(BitMemoryStream impulse)
        {
            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private static void ImpulseEncyclopediaUpdate(BitMemoryStream impulse)
        {
            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private static void ImpulsePvpFactionFactionWars(BitMemoryStream impulse)
        {
            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private static void ImpulsePvpFactionPopFactionWar(BitMemoryStream impulse)
        {
            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private static void ImpulsePvpFactionPushFactionWar(BitMemoryStream impulse)
        {
            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private static void ImpulsePvpChallengeCancelInvitation(BitMemoryStream impulse)
        {
            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private static void ImpulsePvpChallengeInvitation(BitMemoryStream impulse)
        {
            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private static void ImpulseDuelCancelInvitation(BitMemoryStream impulse)
        {
            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private static void ImpulseDuelInvitation(BitMemoryStream impulse)
        {
            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private static void ImpulseDeathRespawn(BitMemoryStream impulse)
        {
            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private static void ImpulseDeathRespawnPoint(BitMemoryStream impulse)
        {
            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private static void ImpulseItemCloseRoomInventory(BitMemoryStream impulse)
        {
            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private static void ImpulseItemOpenRoomInventory(BitMemoryStream impulse)
        {
            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private static void ImpulseItemInfoRefreshVersion(BitMemoryStream impulse)
        {
            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private static void ImpulsePrereqInfoSet(BitMemoryStream impulse)
        {
            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private static void ImpulseItemInfoSet(BitMemoryStream impulse)
        {
            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private static void ImpulsePhraseAckExecuteNext(BitMemoryStream impulse)
        {
            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private static void ImpulsePhraseAckExecuteCyclic(BitMemoryStream impulse)
        {
            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private static void ImpulsePhraseConfirmBuy(BitMemoryStream impulse)
        {
            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private static void ImpulsePhraseDownLoad(BitMemoryStream impulse)
        {
            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private static void ImpulseRemoteAdmin(BitMemoryStream impulse)
        {
            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private static void ImpulseCloseTempInv(BitMemoryStream impulse)
        {
            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private static void ImpulseGuildUseFemaleTitles(BitMemoryStream impulse)
        {
            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private static void ImpulseGuildUpdatePlayerTitle(BitMemoryStream impulse)
        {
            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private static void ImpulseGuildCloseInventory(BitMemoryStream impulse)
        {
            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private static void ImpulseGuildOpenInventory(BitMemoryStream impulse)
        {
            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private static void ImpulseGuildOpenGuildWindow(BitMemoryStream impulse)
        {
            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private static void ImpulseGuildAbortCreation(BitMemoryStream impulse)
        {
            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private static void ImpulseGuildLeaveAscensor(BitMemoryStream impulse)
        {
            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private static void ImpulseGuildAscensor(BitMemoryStream impulse)
        {
            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private static void ImpulseGuildJoinProposal(BitMemoryStream impulse)
        {
            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private static void ImpulseJournalRemoveCompass(BitMemoryStream impulse)
        {
            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private static void ImpulseJournalAddCompass(BitMemoryStream impulse)
        {
            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private static void ImpulseJournalUpdateCompletedMissions(BitMemoryStream impulse)
        {
            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private static void ImpulseJournalInitCompletedMissions(BitMemoryStream impulse)
        {
            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private static void ImpulseBotChatForceEnd(BitMemoryStream impulse)
        {
            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        /// <summary>
        ///     reload the string cache
        /// </summary>
        private static void ImpulseReloadCache(BitMemoryStream impulse)
        {
            var timestamp = 0;
            impulse.Serial(ref timestamp);

            //if (PermanentlyBanned) return; <- haha
            //CStringManagerClient.loadCache(timestamp);

            // todo: CStringManagerClient.loadCache(timestamp)

            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name} with timestamp {timestamp}");
        }

        private static void ImpulseStringResp(BitMemoryStream impulse)
        {
            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        /// <summary>
        ///     A dyn string (or phrase) is send (so, we receive it)
        /// </summary>
        private static void ImpulsePhraseSend(BitMemoryStream impulse)
        {
            StringManagerClient.ReceiveDynString(impulse);
        }

        private static void ImpulseCounter(BitMemoryStream impulse)
        {
            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private static void ImpulseWhere(BitMemoryStream impulse)
        {
            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private static void ImpulseMountAbort(BitMemoryStream impulse)
        {
            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private static void ImpulseExchangeCloseInvitation(BitMemoryStream impulse)
        {
            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private static void ImpulseExchangeInvitation(BitMemoryStream impulse)
        {
            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private static void ImpulseTeamContactRemove(BitMemoryStream impulse)
        {
            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private static void ImpulseTeamContactStatus(BitMemoryStream impulse)
        {
            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private static void ImpulseTeamContactCreate(BitMemoryStream impulse)
        {
            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private static void ImpulseTeamContactInit(BitMemoryStream impulse)
        {
            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private static void ImpulseTeamShareClose(BitMemoryStream impulse)
        {
            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private static void ImpulseTeamShareInvalid(BitMemoryStream impulse)
        {
            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private static void ImpulseTeamShareOpen(BitMemoryStream impulse)
        {
            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private static void ImpulseTeamInvitation(BitMemoryStream impulse)
        {
            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");

            GenericMessageHeaderManager.SendMsgToServer("TEAM:JOIN");
        }

        private static void ImpulseBeginCast(BitMemoryStream impulse)
        {
            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private static void ImpulseDynChatClose(BitMemoryStream impulse)
        {
            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private static void ImpulseDynChatOpen(BitMemoryStream impulse)
        {
            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private static void ImpulseCombatEngageFailed(BitMemoryStream impulse)
        {
            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private static void ImpulseCorrectPos(BitMemoryStream impulse)
        {
            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private static void ImpulseTpWithSeason(BitMemoryStream impulse)
        {
            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private static void ImpulseTp(BitMemoryStream impulse)
        {
            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private static void ImpulseTell2(BitMemoryStream impulse)
        {
            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private static void ImpulseDynStringInChatGroup(BitMemoryStream impulse)
        {
            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private static void ImpulseDynString(BitMemoryStream impulse)
        {
            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private static void ImpulseChat2(BitMemoryStream impulse)
        {
            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private static void ImpulseFarTell(BitMemoryStream impulse)
        {
            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private static void ImpulseTell(BitMemoryStream impulse)
        {
            ChatManager.ProcessTellString(impulse, null);
        }

        private static void ImpulseChat(BitMemoryStream impulse)
        {
            ChatManager.ProcessChatString(impulse, null);
        }

        private static void ImpulsePermanentUnban(BitMemoryStream impulse)
        {
            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private static void ImpulsePermanentBan(BitMemoryStream impulse)
        {
            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private static void ImpulseForumNotification(BitMemoryStream impulse)
        {
            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private static void ImpulseMailNotification(BitMemoryStream impulse)
        {
            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private static void ImpulseServerQuitAbort(BitMemoryStream impulse)
        {
            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private static void ImpulseServerQuitOk(BitMemoryStream impulse)
        {
            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private static void ImpulseShardId(BitMemoryStream impulse)
        {
            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private static void ImpulseCharNameValid(BitMemoryStream impulse)
        {
            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private static void ImpulseServerReady(BitMemoryStream impulse)
        {
            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");

            ServerReceivedReady = true;

            CheckHandshake(impulse);

            //LoginSM.pushEvent(CLoginStateMachine::ev_ready_received);
        }

        private static void ImpulseFarTp(BitMemoryStream impulse)
        {
            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }


        private static void ImpulseUserChar(BitMemoryStream impulse)
        {
            //// received USER_CHAR
            RyzomClient.Log?.Info("ImpulseCallBack : Received CONNECTION:USER_CHAR");
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
            //    NetMngr.setReferencePosition(UserEntity->pos());
            //}
            //else
            //{
            var userEntityInitPos = new Vector3((float) x / 1000.0f, (float) y / 1000.0f, (float) z / 1000.0f);
            var userEntityInitFront = new Vector3((float) Math.Cos(heading), (float) Math.Sin(heading), 0f);

            RyzomClient.Log?.Info($"<ImpulseUserChar> pos : {userEntityInitPos}  heading : {heading}");

            // Update the position for the vision.
            //NetworkManager.setReferencePosition(UserEntityInitPos);
            //}

            RyzomClient.UserCharPosReceived = true;

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
        }

        private static void ImpulseUserChars(BitMemoryStream impulse)
        {
            // received USER_CHARS
            RyzomClient.Log?.Info("Received user characters");

            impulse.Serial(ref Connection.ServerPeopleActive);
            impulse.Serial(ref Connection.ServerCareerActive);
            // read characters summary
            Connection.CharacterSummaries.Clear();

            // START WORKAROUND workaround for serialVector(T &cont) in stream.h TODO
            int len = 0;
            impulse.Serial(ref len);

            for (var i = 0; i < len; i++)
            {
                var cs = new CharacterSummary();
                cs.Serial(impulse);
                RyzomClient.Log?.Info($"Found character {cs.Name} from shard {cs.Mainland} in slot {i}");
                Connection.CharacterSummaries.Add(cs);
            }
            // END WORKAROUND


            //LoginSM.pushEvent(CLoginStateMachine::ev_chars_received);
            RyzomClient.Log?.Info("st_ingame->st_select_char");
            Connection.AutoSendCharSelection = true;

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
            //    ConsoleIO.WriteLineFormatted("ImpulseCallBack : CONNECTION:SELECT_CHAR sent");
            //}
            //else
            //{
            //    ConsoleIO.WriteLineFormatted("§cImpulseCallBack : unknown message name : 'CONNECTION:SELECT_CHAR'.");
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

        private static void ImpulseNoUserChar(BitMemoryStream impulse)
        {
            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private static void ImpulseDatabaseResetBank(BitMemoryStream impulse)
        {
            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private static void ImpulseDatabaseInitBank(BitMemoryStream impulse)
        {
            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private static void ImpulseDatabaseUpdateBank(BitMemoryStream impulse)
        {
            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private static void ImpulseInitInventory(BitMemoryStream impulse)
        {
            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private static void ImpulseUpdateInventory(BitMemoryStream impulse)
        {
            RyzomClient.Log?.Info($"Impulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        private static void ImpulseDatabaseInitPlayer(BitMemoryStream impulse)
        {
            int p = impulse.Pos;

            // get the egs tick of this change
            int serverTick = 0;
            impulse.Serial(ref serverTick);

            // read delta
            // TODO: IngameDbMngr.readDelta + setInitPacketReceived
            //IngameDbMngr.readDelta(serverTick, Impulse, TCDBBank.CDBPlayer);
            //IngameDbMngr.setInitPacketReceived();
            RyzomClient.Log?.Info($"DB_INIT:PLR done ({impulse.Pos - p} bytes)");
        }

        private static void ImpulseDatabaseUpdatePlayer(BitMemoryStream impulse)
        {
            //ConsoleIO.WriteLine("Impulse on " + MethodBase.GetCurrentMethod()?.Name);
        }

        static void CheckHandshake(BitMemoryStream impulse)
        {
            // Decode handshake to check versions
            uint handshakeVersion = 0;
            uint itemSlotVersion = 0;
            impulse.Serial(ref handshakeVersion, 2);
            if (handshakeVersion > 0)
                RyzomClient.Log?.Warn("Server handshake version is more recent than client one");
            impulse.Serial(ref itemSlotVersion, 2);
            RyzomClient.Log?.Info($"Item slot version: {itemSlotVersion}");
            //if (itemSlotVersion != INVENTORIES::CItemSlot::getVersion())
            //    nlerror("Handshake: itemSlotVersion mismatch (S:%hu C:%hu)", itemSlotVersion, INVENTORIES::CItemSlot::getVersion());
        }

        enum TCDBBank
        {
            CDBPlayer,
            CDBGuild, /* CDBContinent, */
            CDBOutpost, /* CDBGlobal, */
            NB_CDB_BANKS,
            INVALID_CDB_BANK
        };
    }
}