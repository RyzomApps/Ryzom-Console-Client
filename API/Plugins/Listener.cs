using System.Collections.Generic;
using System.Numerics;
using API.Plugins.Interfaces;

namespace API.Plugins
{
    /// <summary>
    /// Simple class for tagging all EventListeners
    /// </summary>
    public abstract class Listener : IListener
    {
        /// <summary>
        /// Anything you want to initialize your automaton, will be called on load
        /// This method is called only once, whereas OnGameJoined() is called once per server join.
        ///
        /// NOTE: Chat messages cannot be sent at this point in the login process.
        /// If you want to send a message when the automaton is loaded, use OnGameJoined.
        /// </summary>
        public virtual void OnInitialize() { }

        /// <summary>
        /// Will be called every ~100ms (10fps)
        /// </summary>
        public virtual void OnUpdate() { }

        /// <summary>
        /// Called after the server has been joined successfully and chat messages are able to be sent.
        /// This method is called again after reconnecting to the server, whereas OnInitialize() is called only once.
        ///
        /// NOTE: This is not always right after joining the server - if the automaton was loaded after logging
        /// in this is still called.
        /// </summary>
        public virtual void OnGameJoined() { }

        /// <summary>
        /// Called after an internal RCC command has been performed
        /// </summary>
        /// <param name="commandName">RCC CommandBase Name</param>
        /// <param name="commandParams">RCC CommandBase Parameters</param>
        /// <param name="result">RCC command result</param>
        public virtual void OnInternalCommand(string commandName, string commandParams, string result) { }

        /// <summary>
        /// called when an entity health, sap, stamina or focus value changes
        /// </summary>
        public virtual void OnEntityUpdateBars(uint gameCycle, long prop, byte slot, byte hitPoints, byte stamina, byte sap, byte focus) { }

        /// <summary>
        /// Remove a contact by the server
        /// </summary>
        public virtual void OnTeamContactRemove(uint contactId, byte nList) { }

        /// <summary>
        /// Called when a tell arrives
        /// </summary>
        public virtual void OnTell(string ucstr, string senderName) { }

        /// <summary>
        /// called when the server activates/deactivates use of female titles
        /// </summary>
        public virtual void OnGuildUseFemaleTitles(bool useFemaleTitles) { }

        /// <summary>
        /// called when the server block/unblock some reserved titles
        /// </summary>
        public virtual void OnGuildUpdatePlayerTitle(bool unblock, int len, List<ushort> titles) { }

        /// <summary>
        /// called when the server sends a new respawn point
        /// </summary>
        public virtual void OnDeathRespawnPoint(int x, int y) { }

        /// <summary>
        /// called when the server sends the encyclopedia initialization
        /// </summary>
        public virtual void OnEncyclopediaInit() { }

        /// <summary>
        /// called when the server sends the inventory initialization
        /// </summary>
        public virtual void OnInitInventory(uint serverTick) { }

        /// <summary>
        /// called when the server sends the database initialization
        /// </summary>
        public virtual void OnDatabaseInitPlayer(uint serverTick) { }

        /// <summary>
        /// called when the server updates the user hp, sap, stamina and focus bars/stats
        /// </summary>
        public virtual void OnUserBars(byte msgNumber, int hp, int sap, int sta, int focus) { }

        /// <summary>
        /// called when the string cache reloads
        /// </summary>
        public virtual void OnReloadCache(in int timestamp) { }

        /// <summary>
        /// called when the local string set updates
        /// </summary>
        public virtual void OnStringResp(in uint stringId, string strUtf8) { }

        /// <summary>
        /// called when the player gets invited to a team
        /// </summary>
        public virtual void OnTeamInvitation(in uint textID) { }

        /// <summary>
        /// called when the server sends information about the user char after the login
        /// </summary>
        public virtual void OnUserChar(int highestMainlandSessionId, int firstConnectedTime, int playedTime, Vector3 initPos, Vector3 initFront, short season, int role, bool isInRingSession) { }

        /// <summary>
        /// called when the server sends information about the all the user chars
        /// </summary>
        /// <remarks>
        /// character summaries are updated in the network manager before the Event fires
        /// </remarks>
        public virtual void OnUserChars() { }

        /// <summary>
        /// called when the server sends the database updates
        /// </summary>
        public virtual void OnDatabaseUpdatePlayer(uint serverTick) { }

        /// <summary>
        /// called when the client receives the shard id and the webhost from the server
        /// </summary>
        public virtual void OnShardID(in uint shardId, string webHost) { }

        /// <summary>
        /// Calles when the ingame database was received
        /// </summary>
        public virtual void OnIngameDatabaseInitialized() { }

        /// <summary>
        /// called when an entity gets removed
        /// </summary>
        public virtual void OnEntityRemove(in byte slot) { }

        /// <summary>
        /// called when visual property is updated
        /// </summary>
        public virtual void OnEntityUpdateVisualProperty(in uint gameCycle, in byte slot, in byte prop, in uint predictedInterval) { }
    }
}
