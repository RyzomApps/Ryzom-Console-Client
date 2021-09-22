using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using RCC.Chat;
using RCC.Client;
using RCC.Commands.Internal;
using RCC.Config;
using RCC.Network;

namespace RCC.Automata.Internal
{
    /// <summary>
    /// Class that handles all automata events
    /// </summary>
    public class Automata
    {
        private readonly RyzomClient _handler;

        private readonly List<AutomatonBase> _automata = new List<AutomatonBase>();

        private readonly List<AutomatonBase> _automataOnHold = new List<AutomatonBase>();

        public List<AutomatonBase> GetLoadedAutomata() { return new List<AutomatonBase>(_automata); }

        /// <summary>
        /// Load the class and set the handler
        /// </summary>
        public Automata(RyzomClient handler) { _handler = handler; }

        public void LoadAutomata()
        {
            if (_automataOnHold.Count == 0)
            {
                //Add your AutomatonBase here by uncommenting and adapting
                if (ClientConfig.OnlinePlayersLoggerEnabled) { LoadAutomaton(new OnlinePlayersLogger()); }
                if (ClientConfig.AutoJoinTeamEnabled) { LoadAutomaton(new AutoTeamJoiner()); }
                if (ClientConfig.ImpulseDebuggerEnabled) { LoadAutomaton(new ImpulseDebugger()); }
            }

            foreach (var automata in _automataOnHold)
                LoadAutomaton(automata, false);

            _automataOnHold.Clear();
        }

        /// <summary>
        /// Load a new automaton
        /// </summary>
        public void LoadAutomaton(AutomatonBase b, bool init = true)
        {
            if (_handler.InvokeRequired)
            {
                _handler.InvokeOnMainThread(() => LoadAutomaton(b, init));
                return;
            }

            b.SetHandler(_handler);
            _automata.Add(b);

            if (init)
                DispatchAutomatonEvent(automaton => automaton.Initialize(), new[] { b });
            if (_handler.IsInGame())
                DispatchAutomatonEvent(automaton => automaton.OnGameJoined(), new[] { b });
        }

        /// <summary>
        /// Unload a automaton
        /// </summary>
        public void UnloadAutomaton(AutomatonBase b)
        {
            if (_handler.InvokeRequired)
            {
                _handler.InvokeOnMainThread(() => UnloadAutomaton(b));
                return;
            }

            _automata.RemoveAll(item => ReferenceEquals(item, b));
        }

        /// <summary>
        /// Dispatch a AutomatonBase event with automatic exception handling
        /// </summary>
        /// <example>
        /// Example for calling SomeEvent() on all automata at once:
        /// DispatchAutomatonEvent(automaton => automaton.SomeEvent());
        /// </example>
        /// <param name="action">ActionBase to execute on each automaton</param>
        /// <param name="automataList">Only fire the event for the specified automaton list (default: all automata)</param>
        private void DispatchAutomatonEvent(Action<AutomatonBase> action, IEnumerable<AutomatonBase> automataList = null)
        {
            var selectedAutomata = automataList != null ? automataList.ToArray() : _automata.ToArray();

            foreach (var automaton in selectedAutomata)
            {
                try
                {
                    action(automaton);
                }
                catch (Exception e)
                {
                    if (!(e is ThreadAbortException))
                    {
                        //Retrieve parent method name to determine which event caused the exception
                        var frame = new System.Diagnostics.StackFrame(1);
                        var method = frame.GetMethod();
                        var parentMethodName = method?.Name;

                        //Display a meaningful error message to help debugging the AutomatonBase
                        _handler.GetLogger().Error($"{parentMethodName}: Got error from {automaton}: {e}");
                    }
                    //ThreadAbortException should not be caught here as in can happen when disconnecting from server
                    else throw;
                }
            }
        }

        #region Event API

        /// <summary>
        /// Called when the client disconnects from the server
        /// </summary>
        public void OnDisconnect()
        {
            DispatchAutomatonEvent(automaton => automaton.OnDisconnect(AutomatonBase.DisconnectReason.UserLogout, ""));

            _automataOnHold.Clear();
            _automataOnHold.AddRange(_automata);
        }

        /// <summary>
        /// Called after an internal command has been performed
        /// </summary>
        public void OnInternalCommand(string commandName, string command, string responseMsg)
        {
            foreach (var automaton in _automata.ToArray())
            {
                try
                {
                    automaton.OnInternalCommand(commandName, string.Join(" ", CommandBase.GetArgs(command)), responseMsg);
                }
                catch (Exception e)
                {
                    //ThreadAbortException should not be caught
                    if (!(e is ThreadAbortException))
                    {
                        _handler.GetLogger().Warn("icmd.error " + automaton + " " + e);
                    }
                    else throw;
                }
            }
        }

        /// <summary>
        /// Called from the main instance to update all the automata every some ticks
        /// </summary>
        public void OnUpdate()
        {
            foreach (var automaton in _automata.ToArray())
            {
                try
                {
                    automaton.Update();
                    automaton.UpdateInternal();
                }
                catch (Exception e)
                {
                    if (!(e is ThreadAbortException))
                    {
                        _handler.GetLogger().Warn($"Update: Got error from {automaton}: {e}");
                    }
                    else throw; //ThreadAbortException should not be caught
                }
            }
        }

        /// <summary>
        /// Called when a server was successfully joined
        /// </summary>
        public void OnGameJoined()
        {
            DispatchAutomatonEvent(automaton => automaton.OnGameJoined());
        }

        /// <summary>
        /// Called when one of the characters from the friend list updates
        /// </summary>
        /// <param name="contactId">id</param>
        /// <param name="online">new status</param>
        public void OnTeamContactStatus(uint contactId, CharConnectionState online)
        {
            DispatchAutomatonEvent(automaton => automaton.OnTeamContactStatus(contactId, online));
        }

        /// <summary>
        /// Called when friend list and ignore list from the contact list are initialized
        /// </summary>
        internal void OnTeamContactInit(List<uint> vFriendListName, List<CharConnectionState> vFriendListOnline, List<string> vIgnoreListName)
        {
            DispatchAutomatonEvent(automaton => automaton.OnTeamContactInit(vFriendListName, vFriendListOnline, vIgnoreListName));
        }

        /// <summary>
        /// Called when one character from the friend or ignore list is created
        /// </summary>
        internal void OnTeamContactCreate(uint contactId, uint nameId, CharConnectionState online, byte nList)
        {
            DispatchAutomatonEvent(automaton => automaton.OnTeamContactCreate(contactId, nameId, online, nList));
        }

        /// <summary>
        /// Any chat will arrive here 
        /// </summary>
        internal void OnChat(uint compressedSenderIndex, string ucstr, string rawMessage, ChatGroupType mode, uint dynChatId, string senderName, uint bubbleTimer)
        {
            DispatchAutomatonEvent(automaton => automaton.OnChat(compressedSenderIndex, ucstr, rawMessage, mode, dynChatId, senderName, bubbleTimer));
        }

        /// <summary>
        /// Any tells will arrive here 
        /// </summary>
        internal void OnTell(string ucstr, string senderName)
        {
            DispatchAutomatonEvent(automaton => automaton.OnTell(ucstr, senderName));
        }

        /// <summary>
        /// called when the server activates/deactivates use of female titles
        /// </summary>
        public void OnGuildUseFemaleTitles(bool useFemaleTitles)
        {
            DispatchAutomatonEvent(automaton => automaton.OnGuildUseFemaleTitles(useFemaleTitles));
        }

        /// <summary>
        /// called when the server upload the phrases.
        /// </summary>
        public void OnPhraseDownLoad()
        {
            DispatchAutomatonEvent(automaton => automaton.OnPhraseDownLoad());
        }

        /// <summary>
        /// called when the server block/unblock some reserved titles
        /// </summary>
        public void OnGuildUpdatePlayerTitle(bool unblock, int len, List<ushort> titles)
        {
            DispatchAutomatonEvent(automaton => automaton.OnGuildUpdatePlayerTitle(unblock, len, titles));
        }

        /// <summary>
        /// called when the server sends a new respawn point
        /// </summary>
        public void OnDeathRespawnPoint(int x, int y)
        {
            DispatchAutomatonEvent(automaton => automaton.OnDeathRespawnPoint(x, y));
        }

        /// <summary>
        /// called when the server sends the encyclopedia initialisation
        /// </summary>
        public void OnEncyclopediaInit()
        {
            DispatchAutomatonEvent(automaton => automaton.OnEncyclopediaInit());
        }

        /// <summary>
        /// called when the server sends the inventory initialisation
        /// </summary>
        public void OnInitInventory(uint serverTick)
        {
            DispatchAutomatonEvent(automaton => automaton.OnInitInventory(serverTick));
        }

        /// <summary>
        /// called when the server sends the database initialisation
        /// </summary>
        public void OnDatabaseInitPlayer(uint serverTick)
        {
            DispatchAutomatonEvent(automaton => automaton.OnDatabaseInitPlayer(serverTick));
        }

        /// <summary>
        /// called when the server sends the database updates
        /// </summary>
        public void OnDatabaseUpdatePlayer(uint serverTick)
        {
            DispatchAutomatonEvent(automaton => automaton.OnDatabaseUpdatePlayer(serverTick));
        }

        /// <summary>
        /// called when the server updates the user hp, sap, stamina and focus bars/stats
        /// </summary>
        public void OnUserBars(byte msgNumber, int hp, int sap, int sta, int focus)
        {
            DispatchAutomatonEvent(automaton => automaton.OnUserBars(msgNumber, hp, sap, sta, focus));
        }

        /// <summary>
        /// called when a database bank gets initialised
        /// </summary>
        public void OnDatabaseInitBank(uint serverTick, uint bank)
        {
            DispatchAutomatonEvent(automaton => automaton.OnDatabaseInitBank(serverTick, bank));
        }

        /// <summary>
        /// called when the string cache reloads
        /// </summary>
        public void OnReloadCache(int timestamp)
        {
            DispatchAutomatonEvent(automaton => automaton.OnReloadCache(timestamp));
        }

        /// <summary>
        /// called when the local string set updates
        /// </summary>
        public void OnStringResp(uint stringId, string strUtf8)
        {
            DispatchAutomatonEvent(automaton => automaton.OnStringResp(stringId, strUtf8));
        }

        /// <summary>
        /// called on local string set updates
        /// </summary>
        public void OnPhraseSend(DynamicStringInfo dynInfo)
        {
            DispatchAutomatonEvent(automaton => automaton.OnPhraseSend(dynInfo));
        }

        /// <summary>
        /// called when the player gets invited to a team
        /// </summary>
        public void OnTeamInvitation(uint textID)
        {
            DispatchAutomatonEvent(automaton => automaton.OnTeamInvitation(textID));
        }

        /// <summary>
        /// called when the server sends information about the user char after the login
        /// </summary>
        public void OnUserChar(int highestMainlandSessionId, int firstConnectedTime, int playedTime, Vector3 initPos, Vector3 initFront, short season, int role, bool isInRingSession)
        {
            DispatchAutomatonEvent(automaton => automaton.OnUserChar(highestMainlandSessionId, firstConnectedTime, playedTime, initPos, initFront, season, role, isInRingSession));
        }

        /// <summary>
        /// called when the server sends information about the all the user chars
        /// </summary>
        internal void OnUserChars()
        {
            DispatchAutomatonEvent(automaton => automaton.OnUserChars());
        }

        /// <summary>
        /// called when the client receives the shard id and the webhost from the server
        /// </summary>
        public void OnShardID(uint shardId, string webHost)
        {
            DispatchAutomatonEvent(automaton => automaton.OnShardID(shardId, webHost));
        }

        #endregion
    }
}
