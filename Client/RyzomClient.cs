///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Minecraft Console Client'
// https://github.com/ORelio/Minecraft-Console-Client
// which is released under CDDL-1.0 License
// http://opensource.org/licenses/CDDL-1.0
// Copyright 2021 ORelio and Contributers
///////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using API;
using API.Chat;
using API.Client;
using API.Commands;
using API.Helper;
using API.Helper.Tasks;
using API.Logger;
using API.Network;
using API.Plugins;
using API.Plugins.Interfaces;
using Client.Chat;
using Client.Client;
using Client.Config;
using Client.Database;
using Client.Helper;
using Client.Logger;
using Client.Network;
using Client.Plugins;
using Client.Property;

namespace Client
{
    /// <summary>
    /// The main client class, used to connect to a Ryzom server
    /// </summary>
    public class RyzomClient : IChatDisplayer, IClient
    {
        #region Variables

        private static RyzomClient _instance;
        private readonly NetworkConnection _networkConnection;
        private readonly NetworkManager _networkManager;
        private readonly StringManager _stringManager;
        private readonly DatabaseManager _databaseManager;
        private readonly InterfaceManager _interfaceManager;

        /// <summary>
        /// ryzom client thread to determine if other threads need to invoke
        /// </summary>
        private static Thread _clientThread;

        /// <summary>
        /// thread that handles console input
        /// </summary>
        private Thread _cmdprompt;

        /// <summary>
        /// thread used to detect if the connection has a timeout
        /// </summary>
        private Thread _timeoutdetector;

        private readonly List<string> _cmdNames = new List<string>();
        private readonly Dictionary<string, CommandBase> _cmds = new Dictionary<string, CommandBase>();
        private bool _commandsLoaded;

        private readonly Queue<KeyValuePair<ChatGroupType, string>> _chatQueue = new Queue<KeyValuePair<ChatGroupType, string>>();

        private readonly Queue<Action> _threadTasks = new Queue<Action>();
        private readonly object _threadTasksLocks = new object();

        /// <summary>
        /// when was the last Update call of the RyzomClient
        /// </summary>
        private long _lastClientUpdate = DateTimeOffset.Now.ToUnixTimeMilliseconds();

        private bool _firstConnection = true;
        private DateTime _nextMessageSendTime = DateTime.MinValue;
        private ChatGroupType _channel = ChatGroupType.Around;
        private uint _lastGameCycle;

        // Network Walls
        public bool UserCharPosReceived = false;
        public bool SabrinaPhraseBookLoaded = false;

        public ILogger Log;

        /// <summary>
        /// Plugins class that holds the loaded plugins from the Plugins directory
        /// </summary>
        public PluginManager Plugins { get; }

        public bool ReadyToPing = true;

        #endregion

        #region Properties

        public ChatGroupType Channel
        {
            get => _channel;
            set
            {
                Log?.Info($"Chat channel changed to {value}");
                _channel = value;
            }
        }

        public string Cookie { get; set; }

        public string FsAddr { get; set; }
        public string RingMainURL { get; set; }
        public string FarTpUrlBase { get; set; }
        public bool StartStat { get; set; }
        public string R2ServerVersion { get; set; }
        public string R2BackupPatchURL { get; set; }
        public string[] R2PatchUrLs { get; set; }

        public ILogger GetLogger() { return Log; }

        /// <inheritdoc />
        public bool IsInGame() => _networkConnection.ConnectionState == ConnectionState.Connected;

        public NetworkManager GetNetworkManager() { return _networkManager; }

        /// <inheritdoc />
        public INetworkManager GetINetworkManager() { return _networkManager; }

        public StringManager GetStringManager() { return _stringManager; }

        public IStringManager GetIStringManager() { return _stringManager; }

        public DatabaseManager GetDatabaseManager() { return _databaseManager; }

        public NetworkConnection GetNetworkConnection() { return _networkConnection; }

        public IPluginManager GetPluginManager() { return Plugins; }

        #endregion

        #region Console Client - Initialization

        /// <summary>
        /// Starts the main chat client
        /// </summary>
        public RyzomClient(bool autoStart = true)
        {
            _instance = this;
            _clientThread = Thread.CurrentThread;
            
            Plugins = new PluginManager(this);
            Plugins.RegisterInterface(typeof(PluginLoader));

            if (ClientConfig.UseDatabase)
                _databaseManager = new DatabaseManager(this);
            _networkConnection = new NetworkConnection(this, _databaseManager);
            _stringManager = new StringManager(this);
            _networkManager = new NetworkManager(this, _networkConnection, _stringManager, _databaseManager);
            _interfaceManager = new InterfaceManager();

            // create the data dir
            if (!Directory.Exists("data")) Directory.CreateDirectory("data");

            // create the plugins dir
            if (!Directory.Exists("plugins")) Directory.CreateDirectory("plugins");

            // copy msg.xml from resources
            if (!File.Exists("./data/msg.xml")) ResourceHelper.WriteResourceToFile("msg", "./data/msg.xml");

            // copy database.xml from resources
            if (!File.Exists("./data/database.xml")) ResourceHelper.WriteResourceToFile("database", "./data/database.xml");

            // Start the main client
            if (autoStart)
            {
                StartConsoleClient();
                Program.Exit();
            }
            else
            {
                Log = new FilteredLogger();
            }
        }

        /// <summary>
        /// Starts the main chat client, wich will login to the server.
        /// </summary>
        private void StartConsoleClient()
        {
            if (ClientConfig.DiscordWebhook.Trim() != "")
            {
                Log = new DiscordLogger(ClientConfig.DiscordWebhook);
            }
            else if (ClientConfig.LogToFile)
            {
                Log = new FileLogLogger(ClientConfig.LogFile, ClientConfig.PrependTimestamp);
            }
            else
            {
                Log = new FilteredLogger();
            }

            Log.DebugEnabled = ClientConfig.DebugEnabled;

            // Load commands from Commands namespace
            LoadCommands();

            // Load plugin manager
            Plugins.LoadPlugins(new DirectoryInfo(@".\plugins\"));

            _timeoutdetector = new Thread(TimeoutDetector) { Name = "RCC Connection timeout detector" };
            _timeoutdetector.Start();

            _cmdprompt = new Thread(CommandPrompt) { Name = "RCC CommandBase prompt" };
            _cmdprompt.Start();

            StartRyzomClient();
        }

        #endregion

        #region Chat
        /// <summary>
        /// Returns the client instance (singleton pattern)
        /// </summary>
        /// <returns></returns>
        internal static RyzomClient GetInstance()
        {
            return _instance;
        }

        public void DisplayChat(uint compressedSenderIndex, string ucstr, string rawMessage, ChatGroupType mode,
            uint dynChatId, string senderName, uint bubbleTimer = 0)
        {
            var color = ChatColor.GetMinecraftColorForChatGroupType(mode);

            var stringCategory = ChatManager.GetStringCategory(ucstr, out var finalString).ToUpper();

            // Override color if the string contains the color
            if (stringCategory.Length > 0 && stringCategory != "SYS")
            {
                if (ClientConfig.SystemInfoColors.ContainsKey(stringCategory))
                {
                    var paramString = ClientConfig.SystemInfoColors[stringCategory];

                    while (paramString.Contains("  ")) paramString = paramString.Replace("  ", " ");

                    var paramStringSplit = paramString.Split(" ");

                    if (paramStringSplit.Length >= 3)
                    {
                        var col = Color.FromArgb(int.Parse(paramStringSplit[0]), int.Parse(paramStringSplit[1]),
                            int.Parse(paramStringSplit[2]));

                        color = "";
                        Console.ForegroundColor = ChatColor.FromColor(col);
                    }
                }
            }

            Log.Chat(
                $"[{mode}]{(stringCategory.Length > 0 ? $"[{stringCategory.ToUpper()}]" : "")}{color} {finalString}");

            Plugins.OnChat(compressedSenderIndex, ucstr, rawMessage, mode, dynChatId, senderName, bubbleTimer);
        }

        public void DisplayTell(string ucstr, string senderName)
        {
            Log.Chat(ucstr);

            Plugins.OnTell(ucstr, senderName);
        }

        #endregion IChatDisplayer

        #region Ryzom Client - Initialization and Game Loop

        /// <summary>
        /// Initialize the application. Login to the server. Main loop.
        /// </summary>
        private void StartRyzomClient()
        {
            // Initialize the application
            PreLoginInit();

            // Log the client and choose from shard
            if (!Login())
            {
                Log?.Error("Could not login!");
                return;
            }

            // init message database
            PostLoginInit();

            var ok = true;

            while (ok)
            {
                // If the connection return false we just want to quit the game
                if (!Connection(Cookie, FsAddr))
                {
                    break;
                }

                // sends connection rdy to the server
                InitMainLoop();

                //////////////////////////////////////////////////
                // Main loop (biggest part of the application). //
                //////////////////////////////////////////////////
                ok = !MainLoop();

                // save the string cache
                _stringManager.FlushStringCache();
            }

            Log?.Info("EXIT of the Application.");
        }

        /// <summary>
        /// Initialize the application before loginmeine
        /// </summary>
        private static void PreLoginInit()
        {
            // create the save dir
            if (!Directory.Exists("save")) Directory.CreateDirectory("save");

            // create the user dir
            if (!Directory.Exists("user")) Directory.CreateDirectory("user");
        }

        /// <summary>
        /// Called from client.cpp
        /// start the login state machine
        /// </summary>
        private bool Login()
        {
            var loggedIn = false;
            var firstRetry = true;

            _databaseManager?.FlushObserverCalls();

            while (!loggedIn)
            {
                try
                {
                    Network.Login.CheckLogin(this, ClientConfig.Username, ClientConfig.Password, ClientConfig.ApplicationServer, "");
                    loggedIn = true;
                }
                catch (Exception e)
                {
                    Log?.Warn(e.Message);

                    if (!e.Message.Contains("already online"))
                        return false;

                    if (firstRetry)
                    {
                        Log?.Info("Retrying in 1 second...");
                        Thread.Sleep(1000);
                        firstRetry = false;
                    }
                    else
                    {
                        Log?.Info("Retrying in 30 seconds...");
                        Thread.Sleep(30000);
                    }
                }
            }

            return Cookie != null;
        }

        /// <summary>
        /// OnInitialize the application after login
        /// </summary>
        private void PostLoginInit()
        {
            _networkManager.GetMessageHeaderManager().Init(Constants.MsgXmlPath);

            // OnInitialize the Generic Message Header Manager.
            _networkManager.InitializeNetwork();

            // TODO: init the chat manager
            // ChatManager.init(CPath::lookup("chat_static.cdb"));
        }

        /// <summary>
        /// New version of the menu after the server connection
        /// If you add something in this function, check CFarTP,
        /// some kind of reinitialization might be useful over there.
        /// </summary>
        private bool Connection(string cookie, string fsaddr)
        {
            _networkManager.GameExit = false;

            // Init global variables
            //_networkManager.UserChar = false;
            //_networkManager.NoUserChar = false;
            //_networkManager.ConnectInterf = true;
            //_networkManager.CreateInterf = true;
            //_networkManager.CharacterInterf = true;
            _networkManager.WaitServerAnswer = false;

            // Start the finite state machine
            var interfaceState = InterfaceState.AutoLogin;

            while (interfaceState != InterfaceState.GoInTheGame &&
                   interfaceState != InterfaceState.QuitTheGame)
            {
                interfaceState = interfaceState switch
                {
                    InterfaceState.AutoLogin => AutoLogin(cookie, fsaddr, _firstConnection),
                    InterfaceState.GlobalMenu =>
                        // Interface to choose a char
                        GlobalMenu(),
                    _ => throw new ArgumentOutOfRangeException()
                };
            }

            _firstConnection = false;

            // GOGOGO_IN_THE_GAME
            return interfaceState == InterfaceState.GoInTheGame;
        }

        /// <summary>
        /// Establish network connection, set callbacks for server messages and init the string manager
        /// </summary>
        private InterfaceState AutoLogin(string cookie, string fsaddr, in bool firstConnection)
        {
            if (firstConnection)
                _networkConnection.Init(cookie, fsaddr);

            if (firstConnection)
            {
                try
                {
                    _networkConnection.Connect();
                }
                catch (Exception e)
                {
                    Log?.Error($"Auto Login error: {e.Message}.");
                    return InterfaceState.QuitTheGame;
                }

                // Ok the client is connected
                // Set the impulse callback.
                _networkConnection.SetImpulseCallback(_networkManager.ImpulseCallBack);

                // Set the database. - maybe not needed for console client
                // _networkConnection.SetDataBase(_databaseManager.GetNodePtr());

                // init the string manager cache.
                _stringManager.InitCache(fsaddr, ClientConfig.LanguageCode);
            }

            _networkManager.WaitServerAnswer = true;

            return InterfaceState.GlobalMenu;
        }

        /// <summary>
        /// Launch the interface to choose a character
        /// </summary>
        public InterfaceState GlobalMenu()
        {
            var serverTick = _networkConnection.GetCurrentServerTick();
            var playerWantToGoInGame = false;
            var firewallTimeout = false;

            while (!playerWantToGoInGame)
            {
                // Update network.
                try
                {
                    if (!firewallTimeout)
                        _networkManager.Update();
                }
                catch
                {
                    if (_networkConnection.ConnectionState == ConnectionState.Disconnect)
                    {
                        firewallTimeout = true;
                    }
                    else
                    {
                        Thread.Sleep(30);
                    }
                }
                _databaseManager?.FlushObserverCalls();

                // check if we can send another dated block
                if (_networkConnection.GetCurrentServerTick() != serverTick)
                {
                    serverTick = _networkConnection.GetCurrentServerTick();
                    _networkConnection.Send(serverTick);
                }
                else
                {
                    // Send dummy info
                    _networkConnection.Send();
                }

                // TODO: updateClientTime();
                _databaseManager?.FlushObserverCalls();

                // SERVER INTERACTIONS WITH INTERFACE
                if (_networkManager.WaitServerAnswer)
                {
                    if (_networkManager.CanSendCharSelection)
                    {
                        var charSelect = ClientConfig.SelectCharacter;

                        _networkManager.WaitServerAnswer = false;

                        // check that the pre selected character is available
                        if (_networkManager.CharacterSummaries[charSelect].People == (int)PeopleType.Unknown ||
                            charSelect > 4)
                        {
                            // BAD ! preselected char does not exist
                            throw new InvalidOperationException("preselected char does not exist");
                        }

                        // Clear sending buffer that may contain previous QUIT_GAME when getting back to the char selection screen
                        _networkConnection.FlushSendBuffer();

                        // Auto-selection for fast launching (dev only)
                        _networkManager.CanSendCharSelection = false;
                        ActionHandlerLaunchGame.Execute(charSelect.ToString(), _networkManager);
                    }
                }

                if (_networkManager.ServerReceivedReady)
                {
                    //nlinfo("impulseCallBack : received serverReceivedReady");
                    _networkManager.ServerReceivedReady = false;
                    _networkManager.WaitServerAnswer = false;
                    playerWantToGoInGame = true;
                }

                if (_networkConnection.ConnectionState == ConnectionState.Disconnect)
                {
                    // Display the connection failure screen
                    if (firewallTimeout)
                    {
                        // Display the firewall error string instead of the normal failure string
                        Log?.Error("Firewall Fail (Timeout)");
                    }
                }

                if (_networkManager.GameExit)
                    return InterfaceState.QuitTheGame;
            }

            //  Init the current Player Name
            var playerName = _networkManager.CharacterSummaries[_networkManager.PlayerSelectedSlot].Name;

            // Init the current Player name
            _networkManager.PlayerSelectedHomeShardName = playerName;
            _networkManager.PlayerSelectedHomeShardNameWithParenthesis = '(' + playerName + ')';

            return InterfaceState.GoInTheGame;
        }

        /// <summary>
        /// OnInitialize the main loop.
        /// If you add something in this function, check CFarTP,
        /// some kind of reinitialization might be useful over there.
        /// </summary>
        private void InitMainLoop()
        {
            // Create the game interface database

            // Initialize the Database.
            if (_databaseManager != null)
            {
                Log.Info("Initializing XML Database ...");
                _databaseManager.Init(@"data\database.xml", null);

                var textId = new TextId("SERVER");

                if (DatabaseManager.GetDb().GetNode(textId, false) != null)
                {
                    DatabaseManager.GetDb().RemoveNode(textId);
                }

                DatabaseManager.GetDb().AttachChild(_databaseManager.GetNodePtr(), "SERVER");

                // Set the database
                //_networkManager.SetDataBase(_databaseManager.GetNodePtr());

                // Create interface database
                Log.Info("Initializing Interface Database ...");

                // Add the LOCAL branch
                textId = new TextId("LOCAL");

                if (DatabaseManager.GetDb().GetNode(textId, false) != null)
                {
                    DatabaseManager.GetDb().RemoveNode(textId);
                }

                _interfaceManager.CreateLocalBranch("local_database.xml");
            }

            // Initialize Sound System

            // Initializing Entities Manager
            _networkManager.GetEntityManager().Initialize(256);

            // Creating Scene.

            // Initialize automatic animation

            // Initialize the timer.

            // Parse the interface InGame
            _interfaceManager.InitInGame(); // must be called after waitForUserCharReceived() because Ring information is used by initInGame()
            _networkManager.GetChatManager().InitInGame();

            // Update Network till current tick increase.
            _lastGameCycle = _networkConnection.GetCurrentServerTick();

            while (_lastGameCycle == _networkConnection.GetCurrentServerTick())
            {
                // Update Network.
                _networkManager.Update();
                _databaseManager?.FlushObserverCalls();
            }

            // Get the sheet for the user from the CFG.
            // Initialize the user and add him into the entity manager.
            // DO IT AFTER: Database, Collision Manager, PACS, scene, animations loaded.

            //CSheetId userSheet = new CSheetId(ClientCfg.UserSheet);
            var emptyEntityInfo = new Change.TNewEntityInfo();
            emptyEntityInfo.Reset();
            _networkManager.GetEntityManager().Create(0, Constants.UserSheetId, emptyEntityInfo);
            Log.Info("Created the user with the sheet " + Constants.UserSheetId);

            // Create the message for the server that the client is ready
            var out2 = new BitMemoryStream();

            if (_networkManager.GetMessageHeaderManager().PushNameToStream("CONNECTION:READY", out2))
            {
                out2.Serial(ref ClientConfig.LanguageCode);
                _networkManager.Push(out2);
                _networkManager.Send(_networkConnection.GetCurrentServerTick());
            }
            else
            {
                Log?.Info("initMainLoop : unknown message name : 'CONNECTION:READY'.");
            }
        }

        /// <summary>
        /// Main ryzom game loop
        /// </summary>
        private bool MainLoop()
        {
            Log?.Debug("mainLoop");

            // Main loop. If the window is no more Active -> Exit.
            while (!_networkManager.GameExit)
            {
                // Network Update.
                _networkManager.Update();

                // Database
                if (_databaseManager != null)
                {
                    _databaseManager.FlushObserverCalls();

                    var prevDatabaseInitStatus = _databaseManager.InitInProgress();
                    _databaseManager.SetChangesProcessed();
                    var newDatabaseInitStatus = _databaseManager.InitInProgress();

                    if (!newDatabaseInitStatus && prevDatabaseInitStatus)
                    {
                        Plugins.OnIngameDatabaseInitialized();
                    }
                }

                // Send new data Only when server tick changed.
                if (_networkConnection.GetCurrentServerTick() > _lastGameCycle)
                {
                    BitMemoryStream out2;

                    if (ClientConfig.SendPosition)
                    {
                        // Update the server with our position and orientation.
                        out2 = new BitMemoryStream();

                        if (_networkManager.GetEntityManager().UserEntity
                            .SendToServer(out2, _networkManager.GetMessageHeaderManager()))
                        {
                            _networkManager.Push(out2);
                        }

                        // Give information to the server about the combat position (ability to strike).
                        out2 = new BitMemoryStream();
                        if (_networkManager.GetEntityManager().UserEntity
                            .MsgForCombatPos(out2, _networkManager.GetMessageHeaderManager()))
                        {
                            _networkManager.Push(out2);
                        }
                    }

                    // Create the message for the server to move the user (except in combat mode).
                    if (ReadyToPing)
                    {
                        out2 = new BitMemoryStream();

                        if (_networkManager.GetMessageHeaderManager().PushNameToStream("DEBUG:PING", out2))
                        {
                            const long mask = 0xFFFFFFFF;
                            var localTime = (uint)(mask & Misc.GetLocalTime());
                            out2.Serial(ref localTime);
                            _networkManager.Push(out2);

                            ReadyToPing = false;
                        }
                        else
                        {
                            GetLogger().Warn("mainloop: unknown message named 'DEBUG:PING'.");
                        }
                    }

                    // Send the Packet.
                    _networkManager.Send(_networkConnection.GetCurrentServerTick());

                    // Update the Last tick received from the server.
                    _lastGameCycle = _networkConnection.GetCurrentServerTick();
                }

                // Stats2Title
                Console.Title = $@"[RCC] Version: {Program.Version} - State: {_networkConnection.ConnectionState} - Down: {_networkConnection.GetMeanDownload():0.00} kbps - Up: {_networkConnection.GetMeanUpload():0.00} kbps - Loss: {_networkConnection.GetMeanPacketLoss():0.00}";

                // Update Ryzom Client stuff ~10 times per second -> Execute Tasks (like commands and listener stuff)
                if (Math.Abs(Misc.GetLocalTime() - _lastClientUpdate) > 100)
                {
                    _lastClientUpdate = Misc.GetLocalTime();
                    OnUpdate();
                }

                // Do not eat up all the processor
                Thread.Sleep(10);

                // Get the Connection State.
                var connectionState = _networkConnection.ConnectionState;

                // loop while connected
                if (connectionState != ConnectionState.Disconnect && connectionState != ConnectionState.Quit) continue;

                _networkManager.GameExit = true;
                break;
            } // end of main loop

            _databaseManager?.ResetInitState();

            return true;
        }

        #endregion

        #region Watchdogs

        /// <summary>
        /// Periodically checks for server keepalives and consider that connection has been lost
        /// if the last received keepalive is too old.
        /// </summary>
        private void TimeoutDetector()
        {
            var lastAck = DateTime.Now;

            while (true)
            {
                Thread.Sleep(TimeSpan.FromSeconds(10));

                if (_networkConnection.ConnectionState == ConnectionState.NotInitialized ||
                    _networkConnection.ConnectionState == ConnectionState.Connected)
                {
                    lastAck = DateTime.Now;
                    continue;
                }

                if (lastAck.AddSeconds(Constants.ConnectionTimeout) >= DateTime.Now)
                    continue;

                GetLogger().Error($"Connection timeout of {Constants.ConnectionTimeout} seconds reached.");
                Plugins.OnConnectionLost(ListenerBase.DisconnectReason.ConnectionLost, "error.timeout");
                _networkManager.GameExit = true;

                return;
            }
        }

        /// <summary>
        /// Allows the user to send chat messages, commands, and leave the server.
        /// </summary>
        private void CommandPrompt()
        {
            Thread.Sleep(500);

            while (true)
            {
                try
                {
                    var text = ConsoleIO.ReadLine();
                    InvokeOnMainThread(() => HandleCommandPromptText(text));
                }
                catch (IOException) { }
                catch (NullReferenceException) { }
            }
        }

        #endregion

        #region Console Client Methods 

        /// <summary>
        /// Called ~10 times per second by the game loop
        /// </summary>
        public void OnUpdate()
        {
            // execute all the pluginmanger listener scripts
            Plugins.OnUpdate();

            // process the queued chat messages
            lock (_chatQueue)
            {
                if (_chatQueue.Count > 0 && _nextMessageSendTime < DateTime.Now)
                {
                    var (key, value) = _chatQueue.Dequeue();
                    SendChatMessage(value, key);
                    _nextMessageSendTime = DateTime.Now + TimeSpan.FromSeconds(2);
                }
            }

            // processing of the pending tasks
            lock (_threadTasksLocks)
            {
                while (_threadTasks.Count > 0)
                {
                    var taskToRun = _threadTasks.Dequeue();
                    taskToRun();
                }
            }
        }

        /// <summary>
        /// Send a chat message to the server
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="channel">Channel (e.g. around, universe, region, ...)</param>
        /// <returns>True if properly sent</returns>
        /// TODO: Move that closer to the chatManager
        public bool SendChatMessage(string message, ChatGroupType channel = ChatGroupType.Around)
        {
            if (string.IsNullOrEmpty(message))
                return true;
            try
            {
                if (message.Length > 255)
                    message = message.Substring(0, 255);

                var bms = new BitMemoryStream();
                var msgType = "STRING:CHAT_MODE";
                byte mode = (byte)channel;
                uint dynamicChannelId = 0;

                if (_networkManager.GetMessageHeaderManager().PushNameToStream(msgType, bms))
                {
                    bms.Serial(ref mode);
                    bms.Serial(ref dynamicChannelId);
                    _networkManager.Push(bms);
                    //nlinfo("impulseCallBack : %s %d sent", msgType.c_str(), mode);
                }
                else
                {
                    Log?.Warn($"Unknown message named '{msgType}'.");
                    return false;
                }

                // send str to IOS
                msgType = "STRING:CHAT";

                var out2 = new BitMemoryStream();
                if (_networkManager.GetMessageHeaderManager().PushNameToStream(msgType, out2))
                {
                    out2.Serial(ref message);
                    _networkManager.Push(out2);
                }
                else
                {
                    Log?.Warn($"Unknown message named '{msgType}'.");
                    return false;
                }

                return true;
            }
            catch (SocketException) { return false; }
            catch (IOException) { return false; }
            catch (ObjectDisposedException) { return false; }
        }

        /// <summary>
        /// Load commands from the 'Commands' namespace
        /// </summary>
        public void LoadCommands()
        {
            if (_commandsLoaded) return;

            var cmdsClasses = Program.GetTypesInNamespace("Client.Commands");

            foreach (var type in cmdsClasses)
            {
                if (!type.IsSubclassOf(typeof(CommandBase))) continue;

                try
                {
                    var cmd = (CommandBase)Activator.CreateInstance(type);

                    if (cmd != null)
                    {
                        _cmds[cmd.CmdName.ToLower()] = cmd;
                        _cmdNames.Add(cmd.CmdName.ToLower());
                        foreach (var alias in cmd.GetCmdAliases())
                            _cmds[alias.ToLower()] = cmd;
                    }
                }
                catch (Exception e)
                {
                    Log.Warn(e.Message);
                }
            }

            _commandsLoaded = true;
        }

        /// <summary>
        /// Allows the user to send chat messages, commands, and leave the server.
        /// Process message from the RCC command prompt on the main thread.
        /// </summary>
        private void HandleCommandPromptText(string text)
        {
            text = text.Trim();
            if (text.Length <= 0) return;

            if (ClientConfig.InternalCmdChar == ' ' || text[0] == ClientConfig.InternalCmdChar)
            {
                var responseMsg = "";
                var command = ClientConfig.InternalCmdChar == ' ' ? text : text[1..];

                if (!PerformInternalCommand( /*ClientConfig.ExpandVars(*/command /*)*/, ref responseMsg) &&
                    ClientConfig.InternalCmdChar == '/')
                {
                    SendText(text);
                }
                else if (responseMsg.Length > 0)
                {
                    Log.Info(responseMsg);
                }
            }
            else SendText(text);
        }

        /// <summary>
        /// Send a chat message or command to the server (Enqueues messages)
        /// </summary>
        /// <param name="text">Text to send to the server</param>
        public void SendText(string text)
        {
            const int maxLength = 255;

            // never send out any commands
            if (text[0] == '/')
                return;

            lock (_chatQueue)
            {
                if (string.IsNullOrEmpty(text))
                    return;
                if (text.Length > maxLength) //Message is too long?
                {
                    if (text[0] == '/')
                    {
                        //Send the first 100/256 chars of the command
                        text = text.Substring(0, maxLength);
                        _chatQueue.Enqueue(new KeyValuePair<ChatGroupType, string>(Channel, text));
                    }
                    else
                    {
                        //Split the message into several messages
                        while (text.Length > maxLength)
                        {
                            _chatQueue.Enqueue(new KeyValuePair<ChatGroupType, string>(Channel, text.Substring(0, maxLength)));
                            text = text[maxLength..];
                        }

                        _chatQueue.Enqueue(new KeyValuePair<ChatGroupType, string>(Channel, text));
                    }
                }
                else _chatQueue.Enqueue(new KeyValuePair<ChatGroupType, string>(Channel, text));
            }
        }

        /// <summary>
        /// Register a custom console command
        /// </summary>
        /// <param name="cmdName">Name of the command</param>
        /// <param name="cmdDesc">Description/usage of the command</param>
        /// <param name="cmdUsage">String containing a usage case</param>
        /// <param name="callback">Method for handling the command</param>
        /// <returns>True if successfully registered</returns>
        public bool RegisterCommand(string cmdName, string cmdDesc, string cmdUsage, IClient.CommandRunner callback)
        {
            if (_cmds.ContainsKey(cmdName.ToLower()))
            {
                return false;
            }

            CommandBase cmd = new GenericCommand(cmdName, cmdDesc, cmdUsage, callback);
            _cmds.Add(cmdName.ToLower(), cmd);
            _cmdNames.Add(cmdName.ToLower());
            return true;
        }

        /// <summary>
        /// Unregister a console command
        /// </summary>
        /// <remarks>
        /// There is no check for the command is registered by above method or is embedded command.
        /// Which mean this can unload any command
        /// </remarks>
        /// <param name="cmdName">The name of command to be unregistered</param>
        /// <returns></returns>
        public bool UnregisterCommand(string cmdName)
        {
            if (!_cmds.ContainsKey(cmdName.ToLower())) return false;

            _cmds.Remove(cmdName.ToLower());
            _cmdNames.Remove(cmdName.ToLower());
            return true;
        }

        /// <inheritdoc />
        public bool PerformInternalCommand(string command, ref string responseMsg, Dictionary<string, object> localVars = null)
        {
            if (responseMsg == null) throw new ArgumentNullException(nameof(responseMsg));

            // Process the provided command
            var commandName = command.Split(' ')[0].ToLower();

            if (commandName == "help")
            {
                if (CommandBase.HasArg(command))
                {
                    var helpCmdname = CommandBase.GetArgs(command)[0].ToLower();

                    if (helpCmdname == "help")
                    {
                        responseMsg = "help <cmdname>: show brief help about a command.";
                    }
                    else if (_cmds.ContainsKey(helpCmdname))
                    {
                        responseMsg = _cmds[helpCmdname].GetCmdDescTranslated();
                    }
                    else responseMsg = $"Unknown command '{commandName}'. Use 'help' for command list.";
                }
                else
                    responseMsg =
                        $"help <cmdname>. Available commands: {string.Join(", ", _cmdNames.ToArray())}. For server help, use '{ClientConfig.InternalCmdChar}send /help' instead.";
            }
            else if (_cmds.ContainsKey(commandName))
            {
                responseMsg = _cmds[commandName].Run(this, command, localVars);

                Plugins.OnInternalCommand(commandName, command, responseMsg);
            }
            else
            {
                responseMsg = "Unknown command '{command_name}'. Use 'help' for command list.";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Disconnect the client from the server (initiated from RCC)
        /// </summary>
        public void Disconnect()
        {
            Plugins.OnDisconnect();

            try
            {
                _cmdprompt?.Abort();
            }
            catch
            {
                // ignored
            }

            if (_timeoutdetector == null) return;

            try
            {
                _timeoutdetector.Abort();
                _timeoutdetector = null;
            }
            catch
            {
                // ignored
            }
        }

        #endregion

        #region Thread-Invoke: Cross-thread method calls

        /// <summary>
        /// Invoke a task on the main thread, wait for completion and retrieve return value.
        /// </summary>
        /// <param name="task">Task to run with any type or return value</param>
        /// <returns>Any result returned from task, result type is inferred from the task</returns>
        /// <example>bool result = InvokeOnMainThread(methodThatReturnsAbool);</example>
        /// <example>bool result = InvokeOnMainThread(() => methodThatReturnsAbool(argument));</example>
        /// <example>int result = InvokeOnMainThread(() => { yourCode(); return 42; });</example>
        /// <typeparam name="T">Type of the return value</typeparam>
        public T InvokeOnMainThread<T>(Func<T> task)
        {
            if (!InvokeRequired)
            {
                return task();
            }

            var taskWithResult = new TaskWithResult<T>(task);

            lock (_threadTasksLocks)
            {
                _threadTasks.Enqueue(taskWithResult.ExecuteSynchronously);
            }

            return taskWithResult.WaitGetResult();
        }

        /// <summary>
        /// Invoke a task on the main thread and wait for completion
        /// </summary>
        /// <param name="task">Task to run without return value</param>
        /// <example>InvokeOnMainThread(methodThatReturnsNothing);</example>
        /// <example>InvokeOnMainThread(() => methodThatReturnsNothing(argument));</example>
        /// <example>InvokeOnMainThread(() => { yourCode(); });</example>
        public void InvokeOnMainThread(Action task)
        {
            InvokeOnMainThread(() =>
            {
                task();
                return true;
            });
        }

        /// <summary>
        /// Check if running on a different thread and InvokeOnMainThread is required
        /// </summary>
        /// <returns>True if calling thread is not the main thread</returns>
        public bool InvokeRequired => GetNetReadThreadId() != Thread.CurrentThread.ManagedThreadId;

        /// <summary>
        /// Get net read thread (main thread) ID
        /// </summary>
        /// <returns>Net read thread ID</returns>
        public int GetNetReadThreadId()
        {
            return _clientThread?.ManagedThreadId ?? -1;
        }

        #endregion
    }
}