﻿///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Minecraft Console Client'
// https://github.com/ORelio/Minecraft-Console-Client
// which is released under CDDL-1.0 License
// http://opensource.org/licenses/CDDL-1.0
// Copyright 2021 ORelio and Contributers
///////////////////////////////////////////////////////////////////

using API;
using API.Chat;
using API.Client;
using API.Commands;
using API.Database;
using API.Helper;
using API.Helper.Tasks;
using API.Logger;
using API.Network;
using API.Plugins;
using API.Plugins.Interfaces;
using Client.ActionHandler;
using Client.Chat;
using Client.Client;
using Client.Config;
using Client.Database;
using Client.Helper;
using Client.Logger;
using Client.Network;
using Client.Plugins;
using Client.Property;
using Client.Sheet;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using Client.Brick;
using Client.Phrase;
using Client.Skill;
using Client.Stream;
using API.Entity;
using API.Inventory;
using API.Network.Web;
using API.Sheet;
using Client.Strings;
using Client.Inventory;
using Client.Interface;
using Client.Network.Web;
using Client.Network.Proxy;
using Client.WinAPI;
using static System.Threading.Thread;

namespace Client
{
    /// <summary>
    /// The main client class, used to connect to a Ryzom server
    /// </summary>
    public class RyzomClient : IAutoComplete, IChatDisplayer, IClient
    {
        private const string HelpCommand = "help";

        #region Variables

        private static RyzomClient _instance;
        private readonly NetworkConnection _networkConnection;
        private readonly NetworkManager _networkManager;
        private readonly StringManager _stringManager;
        private readonly DatabaseManager _databaseManager;
        private readonly InterfaceManager _interfaceManager;
        private readonly SheetManager _sheetManager;
        private readonly PhraseManager _phraseManager;
        private readonly InventoryManager _inventoryManager;
        private readonly SkillManager _skillManager;
        private readonly BrickManager _brickManager;
        private readonly SheetIdFactory _sheetIdFactory;
        private readonly ActionHandlerManager _actionHandlerManager;
        private readonly WebTransfer _webTransfer;

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
        public uint CharacterHomeSessionId = 0;

        public ILogger Log;

        /// <summary>
        /// Plugins class that holds the loaded plugins from the Plugins directory
        /// </summary>
        public PluginManager Plugins { get; }

        public bool ReadyToPing = true;

        #endregion

        #region Properties

        /// <summary>
        /// Current in-game chat channel of the client used for outgoing chat messages
        /// </summary>
        public ChatGroupType Channel
        {
            get => _channel;
            set
            {
                if (value == _channel)
                    return;

                Log?.Info($"Chat channel changed to {value}");
                _channel = value;
            }
        }

        public SessionData SessionData;

        /// <summary>
        /// Logger instance attached to the Client
        /// </summary>
        public ILogger GetLogger() { return Log; }

        /// <inheritdoc />
        public bool IsInGame() => _networkConnection.ConnectionState == ConnectionState.Connected;

        public NetworkManager GetNetworkManager() { return _networkManager; }

        /// <inheritdoc />
        public INetworkManager GetApiNetworkManager() { return _networkManager; }

        public StringManager GetStringManager() { return _stringManager; }

        /// <inheritdoc />
        public IStringManager GetApiStringManager() { return _stringManager; }

        public DatabaseManager GetDatabaseManager() { return _databaseManager; }

        /// <inheritdoc />
        public IDatabaseManager GetApiDatabaseManager() { return _databaseManager; }

        public NetworkConnection GetNetworkConnection() { return _networkConnection; }

        public SheetManager GetSheetManager() { return _sheetManager; }

        public ISheetManager GetApiSheetManager() { return _sheetManager; }

        public IPluginManager GetPluginManager() { return Plugins; }

        public PhraseManager GetPhraseManager() { return _phraseManager; }

        public InterfaceManager GetInterfaceManager() { return _interfaceManager; }

        public SheetIdFactory GetSheetIdFactory() { return _sheetIdFactory; }

        public ISheetIdFactory GetApiSheetIdFactory() { return _sheetIdFactory; }

        public BrickManager GetBrickManager() { return _brickManager; }

        public SkillManager GetSkillManager() { return _skillManager; }

        public InventoryManager GetInventoryManager() { return _inventoryManager; }

        public IInventoryManager GetApiInventoryManager() { return _inventoryManager; }

        public IWebTransfer GetWebTransfer() { return _webTransfer; }

        public ActionHandlerManager GetActionHandlerManager() { return _actionHandlerManager; }
        #endregion

        #region Console Client - Initialization

        /// <summary>
        /// Starts the main client
        /// </summary>
        public RyzomClient(bool autoStart = true)
        {
            // Setup exit with the quit command
            ExitCleanUp.Add(delegate { PerformInternalCommand("quit", out _); });
            ExitCleanUp.Add(OnDisconnect);

            _instance = this;
            _clientThread = CurrentThread;

            Plugins = new PluginManager(this);
            Plugins.RegisterInterface(typeof(PluginLoader));

            if (ClientConfig.UseDatabase)
                _databaseManager = new DatabaseManager(this);

            _stringManager = new StringManager(this);
            _sheetManager = new SheetManager(this);
            _skillManager = new SkillManager(this);
            _brickManager = new BrickManager(this);
            _sheetIdFactory = new SheetIdFactory(this);
            _networkConnection = new NetworkConnection(this);
            _phraseManager = new PhraseManager(this);

            if (ClientConfig.UseInventory)
                _inventoryManager = new InventoryManager(this);

            _actionHandlerManager = new ActionHandlerManager(this);
            _webTransfer = new WebTransfer(this);
            _interfaceManager = new InterfaceManager(this);
            _networkManager = new NetworkManager(this);

            // Essential action handlers
            _actionHandlerManager.RegisterActionHandler(new ActionHandlerBrowse(this), "browse");
            _actionHandlerManager.RegisterActionHandler(new ActionHandlerCreateChar(this), "ask_create_char");
            _actionHandlerManager.RegisterActionHandler(new ActionHandlerDeleteChar(this), "ask_delete_char");
            _actionHandlerManager.RegisterActionHandler(new ActionHandlerRenameChar(this), "ask_rename_char");
            _actionHandlerManager.RegisterActionHandler(new ActionHandlerAskValidName(this), "ask_valid_name");
            _actionHandlerManager.RegisterActionHandler(new ActionHandlerLaunchGame(this, _networkManager), "launch_game");

            // create the data dir
            if (!Directory.Exists("data")) Directory.CreateDirectory("data");

            // create the plugins dir
            if (!Directory.Exists("plugins")) Directory.CreateDirectory("plugins");

            // copy msg.xml from resources
            if (!File.Exists("./data/msg.xml")) ResourceHelper.WriteResourceToFile("msg", "./data/msg.xml");

            // copy database.xml from resources
            if (!File.Exists("./data/database.xml")) ResourceHelper.WriteResourceToFile("database", "./data/database.xml");

            // copy local_database.xml from resources
            if (!File.Exists("./data/local_database.xml")) ResourceHelper.WriteResourceToFile("local_database", "./data/local_database.xml");

            // copy sheet_id.bin from resources
            if (!File.Exists("./data/sheet_id.bin")) ResourceHelper.WriteResourceToFile("sheet_id", "./data/sheet_id.bin");

            // copy proxies.txt from resources
            if (!File.Exists("./data/proxies.txt")) ResourceHelper.WriteResourceToFile("proxies", "./data/proxies.txt");

            // Start the main client
            if (autoStart)
            {
                StartConsoleClient();
                Program.Exit();
            }
            else
            {
                Log = new DebugLogger();
            }
        }

        /// <summary>
        /// Log system (all chat/tell)
        /// </summary>
        internal bool LogState
        {
            get => Log is FileLogLogger;
            set => Log = value
                ? new FileLogLogger($"save/log_{GetNetworkManager().PlayerSelectedHomeShardName}.txt", ClientConfig.PrependTimestamp)
                : new FilteredLogger();
        }

        /// <summary>
        /// Starts the main client, wich will login to the server.
        /// </summary>
        private void StartConsoleClient()
        {
            Log = ClientConfig.DiscordWebhook.Trim() != "" ? new DiscordLogger(ClientConfig.DiscordWebhook) : new FilteredLogger();

            Log.DebugEnabled = ClientConfig.DebugEnabled;

            Log.Debug("StartConsoleClient()");

            // Load commands from Commands name space
            LoadCommands();

            // Load action handlers from action handler name space
            _actionHandlerManager.LoadActionHandlers();

            // Load plugin manager
            Plugins.LoadPlugins(new DirectoryInfo(@"./plugins/"));

            _timeoutdetector = new Thread(TimeoutDetector) { Name = "RCC Connection timeout detector" };
            _timeoutdetector.Start();

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

        public void DisplayChat(uint compressedSenderIndex, string ucstr, string rawMessage, ChatGroupType mode, uint dynChatId, string senderName, uint bubbleTimer = 0)
        {
            var color = ChatColor.GetMinecraftColorForChatGroupType(mode);

            var stringCategory = ChatManagerHelper.GetStringCategory(ucstr, out var finalString).ToUpper();

            // Override color if the string contains the color
            if (stringCategory.Length > 0 && stringCategory != "SYS")
            {
                if (ClientConfig.SystemInfoColors.TryGetValue(stringCategory, out var infoColor))
                {
                    while (infoColor.Contains("  ")) infoColor = infoColor.Replace("  ", " ");

                    var paramStringSplit = infoColor.Split(" ");

                    if (paramStringSplit.Length >= 3)
                    {
                        var col = Color.FromArgb(int.Parse(paramStringSplit[0]), int.Parse(paramStringSplit[1]), int.Parse(paramStringSplit[2]));

                        color = "";
                        Console.ForegroundColor = ChatColor.FromColor(col);
                    }
                }
            }

            // translation
            finalString = ChatManagerHelper.GetVerbatim(finalString, ClientConfig.TranslateChat);

            Log.Chat($"[{mode}]{(stringCategory.Length > 0 ? $"[{stringCategory.ToUpper()}]" : "")}{color} {finalString}");

            Plugins.OnChat(compressedSenderIndex, ucstr, rawMessage, mode, dynChatId, senderName, bubbleTimer);
        }

        public void DisplayTell(string formattedMessage, string senderName, string rawMessage)
        {
            Log.Chat(formattedMessage);

            Plugins.OnTell(rawMessage, senderName);
        }

        #endregion IChatDisplayer

        #region Ryzom Client - Initialization and Game Loop

        /// <summary>
        /// Initialize the application. Login to the server. Main loop.
        /// </summary>
        private void StartRyzomClient()
        {
            Log?.Debug("StartRyzomClient()");

            // Initialize the application
            PreLoginInit();

            // Log the client and choose from shard
            if (!Login())
            {
                Log?.Error("Could not log in!");
                return;
            }

            // initialize the message database
            PostLoginInit();

            var ok = true;

            while (ok)
            {
                // If the connection return false we just want to quit the game
                if (!Connection(SessionData.Cookie, SessionData.FsAddr))
                {
                    break;
                }

                // sends 'connection ready' to the server
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
        /// Initialize the application before the login
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

            if (ClientConfig.UseProxy)
                Log?.Info("§eTrying to find a working TCP proxy. This could take a moment...");

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

                    if (e.Message.Contains("already online") && firstRetry)
                    {
                        Log?.Info("Retrying in 1 second...");
                        Sleep(1000);
                        firstRetry = false;
                    }
                    else if (e.Message.Contains("already online"))
                    {
                        Log?.Info("Retrying in 30 seconds...");
                        Sleep(30000);
                    }
                    else if (!ClientConfig.UseProxy)
                    {
                        return false;
                    }
                }
            }

            return SessionData?.Cookie != null;
        }

        /// <summary>
        /// Initialize the application after login
        /// </summary>
        /// <remarks>if the initialization fails, call NLERROR</remarks>
        private void PostLoginInit()
        {
            _networkManager.GetMessageHeaderManager().Init(Constants.MsgXmlPath);

            // OnInitialize the Generic Message Header Manager.
            _networkManager.InitializeNetwork();

            // TODO: Initialize Chat Manager
            // ChatManager.init(CPath::lookup("chat_static.cdb"));

            // TODO: Read the LIGO primitive class file

            // Initialize Sheet IDs
            _sheetIdFactory.Init(ClientConfig.UpdatePackedSheet);

            // Initialize Packed Sheets
            _sheetManager.SetOutputDataPath("../../client/data");
            _sheetManager.Load(null, ClientConfig.UpdatePackedSheet, ClientConfig.NeedComputeVs, ClientConfig.DumpVsIndex, _sheetIdFactory);

            // TODO: Initialize bricks

            // TODO: Initialize primitives
        }

        /// <summary>
        /// New version of the menu after the server connection
        /// If you add something in this function, check CFarTP,
        /// some kind of reinitialization might be useful over there.
        /// </summary>
        private bool Connection(string cookie, string fsaddr)
        {
            _networkManager.GameExit = false;

            // Initialize global variables
            _networkManager.WaitServerAnswer = false;

            // Start the finite state machine
            var interfaceState = InterfaceState.AutoLogin;

            var loginRetries = 0;

            while (interfaceState != InterfaceState.GoInTheGame &&
                   interfaceState != InterfaceState.QuitTheGame)
            {
                try
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
                catch (NetworkLoginException e)
                {
                    GetLogger().Error(e.Message);

                    if (ClientConfig.UseProxy)
                        ProxyManager.SetProxyBrokenFlag(true);

                    loginRetries++;

                    // login exception
                    if (loginRetries < ClientConfig.ProxyLoginRetries && ClientConfig.UseProxy)
                    {
                        // udp proxy may be bad - try another one
                        GetLogger().Warn($"Login retry #{loginRetries}...");
                        interfaceState = InterfaceState.AutoLogin;
                    }
                    else
                    {
                        // connection may be bad - quit
                        interfaceState = InterfaceState.QuitTheGame;
                    }
                }
            }

            _firstConnection = false;

            // GOGOGO_IN_THE_GAME
            return interfaceState == InterfaceState.GoInTheGame;
        }

        /// <summary>
        /// Establish network connection, set callbacks for server messages and initialize the string manager
        /// </summary>
        private InterfaceState AutoLogin(string cookie, string fsaddr, in bool firstConnection)
        {
            if (firstConnection)
                _networkConnection.Init(cookie, fsaddr);

            if (firstConnection)
            {
                try
                {
                    _networkConnection.Connect(ClientConfig.UseProxy);
                }
                catch (Exception e)
                {
                    Log?.Error($"Auto Login error: {e.Message}.");
                    return InterfaceState.QuitTheGame;
                }

                // OK, the client is connected
                // Set the impulse callback.
                _networkConnection.SetImpulseCallback(_networkManager.ImpulseCallBack);

                // Set the database. - maybe not needed for console client
                // _networkConnection.SetDataBase(_databaseManager.GetNodePtr());

                // Initialize the string manager cache.
                _stringManager.InitCache(fsaddr, ClientConfig.LanguageCode);
            }

            _networkManager.WaitServerAnswer = true;

            return InterfaceState.GlobalMenu;
        }

        /// <summary>
        /// Launch the interface to choose a character
        /// </summary>
        private InterfaceState GlobalMenu()
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
                catch (NetworkLoginException)
                {
                    throw;
                }
                catch
                {
                    if (_networkConnection.ConnectionState == ConnectionState.Disconnect)
                    {
                        firewallTimeout = true;
                    }
                    else
                    {
                        Sleep(10);
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

                        // Display character selection menu
                        if (charSelect == -1)
                        {
                            ConsoleIO.WriteLineFormatted("§dPlease enter your character slot [§b0§d-§b4§d] or action [§bR§d]ename, [§bD§d]elete, or [§bE§d]xit:");
                            var input = Console.ReadLine();

                            if (input != null)
                            {
                                // Handle exit action
                                if (input.Equals("E", StringComparison.OrdinalIgnoreCase))
                                {
                                    return InterfaceState.QuitTheGame;
                                }

                                // Handle character action
                                if (input.Equals("R", StringComparison.OrdinalIgnoreCase) || input.Equals("D", StringComparison.OrdinalIgnoreCase))
                                {
                                    ProcessCharacterAction(input, charSelect);
                                    return InterfaceState.GlobalMenu;
                                }

                                // Handle 0-4 selection
                                charSelect = int.Parse(input);
                            }
                        }

                        if (charSelect is < 0 or > 4)
                        {
                            Log?.Error("Invalid slot.");
                            return InterfaceState.GlobalMenu;
                        }

                        // check that the preselected character is available
                        if (_networkManager.CharacterSummaries[charSelect].People == (int)PeopleType.Unknown)
                        {
                            // Launch the character creation process if the slot is empty
                            ProcessCharacterAction("N", charSelect);
                        }
                        else
                        {
                            _networkManager.WaitServerAnswer = false;

                            // Clear sending buffer that may contain previous QUIT_GAME when getting back to the char selection screen
                            _networkConnection.FlushSendBuffer();

                            // Auto-selection for fast launching (developer only)
                            _networkManager.CanSendCharSelection = false;

                            GetActionHandlerManager().RunActionHandler("launch_game", null, $"slot={charSelect}|edit_mode=0");
                        }
                    }
                }

                if (_networkManager.CharNameValidArrived)
                {
                    Log?.Debug("impulseCallBack : received charNameValidArrived");

                    //_networkManager.CharNameValidArrived = false;
                    _networkManager.WaitServerAnswer = false;
                }

                if (_networkManager.ServerReceivedReady)
                {
                    Log?.Debug("impulseCallBack : received serverReceivedReady");
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

            // Initialize the current Player Name
            var playerName = _networkManager.CharacterSummaries[_networkManager.PlayerSelectedSlot].Name;
            _networkManager.PlayerSelectedHomeShardName = playerName;

            // Initialize the file Logger if possilbe
            if (!LogState && ClientConfig.LogToFile)
            {
                LogState = true;
            }

            // Init the current Player Home shard Id and name
            CharacterHomeSessionId = _networkManager.CharacterSummaries[_networkManager.PlayerSelectedSlot].Mainland;
            _networkManager.PlayerSelectedHomeShardNameWithParenthesis = $"({playerName})";

            return InterfaceState.GoInTheGame;
        }

        /// <summary>
        /// Handles character actions such as renaming, creating, and deleting characters.
        /// </summary>
        /// <param name="action">The action to perform: 'R' for rename, 'N' for create, 'D' for delete.</param>
        /// <param name="slot">The index of the character slot.</param>
        /// <returns>If the action was successful.</returns>
        private bool ProcessCharacterAction(string action, int slot = -1)
        {
            if (slot == -1)
            {
                ConsoleIO.WriteLineFormatted("§dPlease enter your character slot [§b0§d-§b4§d]:");
                var input = Console.ReadLine();

                if (input != null)
                    slot = int.Parse(input);
                else
                    throw new Exception("Invalid slot.");

                if (slot is < 0 or > 4)
                    throw new Exception("Invalid slot.");
            }

            if (action.Equals("R", StringComparison.OrdinalIgnoreCase))
            {
                // Renaming character
                if (_networkManager.CharacterSummaries[slot].People != (int)PeopleType.Unknown)
                {
                    ConsoleIO.WriteLineFormatted("§dPlease enter a new name for your character:");
                    var newName = Console.ReadLine();

                    // Validate the character name
                    if (ValidateCharacterName(newName, slot, "rename"))
                    {
                        GetActionHandlerManager().RunActionHandler("ask_rename_char", null, $"name={newName}|slot={slot}");
                        return true;
                    }

                    ConsoleIO.WriteLineFormatted("§cInvalid name. Please try again.");
                }
                else
                {
                    ConsoleIO.WriteLineFormatted("§cYou cannot rename a character that does not exist.");
                }
            }

            if (action.Equals("N", StringComparison.OrdinalIgnoreCase))
            {
                // Creating character
                if (_networkManager.CharacterSummaries[slot].People == (int)PeopleType.Unknown)
                {
                    ConsoleIO.WriteLineFormatted("§dPlease enter a character name:");
                    var selectedName = Console.ReadLine();

                    // Validate the character name
                    if (ValidateCharacterName(selectedName, slot, "create"))
                    {
                        GetActionHandlerManager().RunActionHandler("ask_create_char", null, $"name={selectedName}|slot={slot}");
                        return true;
                    }

                    ConsoleIO.WriteLineFormatted("§cInvalid name. Please try again.");
                }
                else
                {
                    ConsoleIO.WriteLineFormatted("§cYou cannot create a character in a slot that already has a character.");
                }
            }

            if (action.Equals("D", StringComparison.OrdinalIgnoreCase))
            {
                // Deleting character
                if (_networkManager.CharacterSummaries[slot].People != (int)PeopleType.Unknown)
                {
                    ConsoleIO.WriteLineFormatted($"§dAre you sure you want to delete the character '{_networkManager.CharacterSummaries[slot].Name}'? (Y/N)");
                    var confirm = Console.ReadLine();

                    if (confirm != null && confirm.Equals("Y", StringComparison.OrdinalIgnoreCase))
                    {
                        GetActionHandlerManager().RunActionHandler("ask_delete_char", null, $"slot={slot}");
                        return true;
                    }
                }
                else
                {
                    ConsoleIO.WriteLineFormatted("§cYou cannot delete a character that does not exist.");
                    return false;
                }
            }

            // In case none of the actions were matched
            return false;
        }

        /// <summary>
        /// Validates a character name by sending a request to the server and waiting for a response.
        /// </summary>
        private bool ValidateCharacterName(string name, int slot, string action)
        {
            var ryzomClient = RyzomClient.GetInstance();

            ryzomClient.GetNetworkManager().CharNameValidArrived = false;

            // Execute the action handler to validate the name
            GetActionHandlerManager().RunActionHandler("ask_valid_name", null, $"name={name}");

            // Wait for the validation response
            if (!ryzomClient.GetNetworkManager().CharNameValidArrived)
            {
                ryzomClient.GetNetworkManager().Update(); // keep updating until we get the response
            }

            // Check if the name is valid
            return ryzomClient.GetNetworkManager().CharNameValid;
        }

        /// <summary>
        /// Initialize the main loop.
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
                _databaseManager.Init(@"data/database.xml", null);

                var textId = new TextId("SERVER");

                if (_databaseManager.GetRootDb().GetNode(textId, false) != null)
                {
                    _databaseManager.GetRootDb().RemoveNode(textId);
                }

                _databaseManager.GetRootDb().AttachChild(_databaseManager.GetServerDb(), "SERVER");

                // Set the database
                //_networkManager.SetDataBase(_databaseManager.GetNodePtr());

                // Create interface database
                Log.Info("Initializing Interface Database ...");

                // Add the LOCAL branch
                textId = new TextId("LOCAL");

                if (_databaseManager.GetRootDb().GetNode(textId) != null)
                {
                    _databaseManager.GetRootDb().RemoveNode(textId);
                }

                _interfaceManager.CreateLocalBranch(@"data/local_database.xml");
            }

            // Initialize Sound System

            // Initializing Entities Manager
            _networkManager.GetEntityManager().Initialize(256);

            // Creating Scene.

            // Initialize automatic animation

            // Initialize the timer.

            // Parse the interface InGame
            _interfaceManager.InitInGame(); // must be called after waitForUserCharReceived() because Ring information is used by initInGame()

            // Start the command promt
            _cmdprompt = new Thread(CommandPrompt) { Name = "RCC CommandBase prompt" };
            _cmdprompt.Start();

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
            var userSheet = uint.TryParse(ClientConfig.UserSheet, out var userSheetId) ? GetSheetIdFactory().SheetId(userSheetId) : GetSheetIdFactory().SheetId(ClientConfig.UserSheet);
            var emptyEntityInfo = new PropertyChange.TNewEntityInfo();
            emptyEntityInfo.Reset();
            _networkManager.GetEntityManager().Create(0, userSheet.AsInt(), emptyEntityInfo);
            Log.Info($"Created user entity with the sheet {userSheet}");

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

                // Update the position for the vision.
                if (_networkManager?.GetEntityManager()?.UserEntity != null)
                    _networkManager.SetReferencePosition(_networkManager.GetEntityManager().UserEntity.Pos);

                // Send new data Only when server tick changed.
                if (_networkConnection.GetCurrentServerTick() > _lastGameCycle && _networkManager != null)
                {
                    BitMemoryStream out2;

                    if (ClientConfig.SendPosition)
                    {
                        // Update the server with our position and orientation.
                        out2 = new BitMemoryStream();

                        if (_networkManager.GetEntityManager().UserEntity.SendToServer(out2, _networkManager.GetMessageHeaderManager(), this))
                        {
                            _networkManager.Push(out2);
                        }

                        // Give information to the server about the combat position (ability to strike).
                        out2 = new BitMemoryStream();
                        if (_networkManager.GetEntityManager().UserEntity.MsgForCombatPos(out2, _networkManager.GetMessageHeaderManager()))
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
                            GetLogger().Warn("main loop: unknown message named 'DEBUG:PING'.");
                        }
                    }

                    // Send the Packet.
                    _networkManager.Send(_networkConnection.GetCurrentServerTick());

                    // Update the Last tick received from the server.
                    _lastGameCycle = _networkConnection.GetCurrentServerTick();
                }

                // Stats2Title
                var name = GetNetworkManager()?.PlayerSelectedHomeShardName;
                name = EntityHelper.RemoveTitleAndShardFromName(name);
                if (name.Trim().Length > 0)
                    name += " - ";

                var country = GetNetworkManager()?.GetNetworkConnection()?.ProxyCountry;
                if (country != null && country.Trim().Length > 0)
                    country += " - ";

                Console.Title = $@"[RCC] {name}{country}{_networkConnection.ConnectionState} - Ping: {_networkConnection.GetPing()} ms - Down: {_networkConnection.GetMeanDownload():0.0} KB/s - Up: {_networkConnection.GetMeanUpload():0.0} KB/s - Loss: {_networkConnection.GetMeanPacketLoss():0} P/s - Version: {Program.Version}";

                // Update Ryzom Client stuff ~10 times per second -> Execute Tasks (like commands and listener stuff)
                if (Math.Abs(Misc.GetLocalTime() - _lastClientUpdate) > 100)
                {
                    _lastClientUpdate = Misc.GetLocalTime();
                    OnUpdate();
                }

                // Do not eat up all the processor
                Sleep(10);

                // Get the Connection State.
                var connectionState = _networkConnection.ConnectionState;

                // Update Phrase Manager
                _phraseManager.UpdateEquipInvalidation(_networkManager.GetCurrentServerTick());
                _phraseManager.UpdateAllActionRegen();

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
        /// Periodically checks for server keep-alive and consider that connection has been lost
        /// if the last received keep-alive is too old.
        /// </summary>
        private void TimeoutDetector()
        {
            var lastAck = DateTime.Now;

            while (true)
            {
                try
                {
                    Sleep(TimeSpan.FromSeconds(10));
                }
                catch
                {
                    return;
                }

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
            Sleep(500);

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
            // execute all the plugin manger listener scripts
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
        /// <param name="str">Message</param>
        /// <param name="channel">Channel (e.g. around, universe, region, ...)</param>
        /// <returns>True if properly sent</returns>
        /// TODO: Move that closer to the chatManager
        public bool SendChatMessage(string str, ChatGroupType channel = ChatGroupType.Around)
        {
            if (string.IsNullOrEmpty(str))
                return true;
            try
            {
                if (str.Length > 255)
                    str = str[..255];

                _networkManager.GetChatManager().SetChatMode(channel);

                // send STR to IOS
                var bms = new BitMemoryStream();
                var msgType = channel == ChatGroupType.Team ? "STRING:CHAT_TEAM" : "STRING:CHAT";

                if (_networkManager.GetMessageHeaderManager().PushNameToStream(msgType, bms))
                {
                    bms.Serial(ref str);
                    _networkManager.Push(bms);
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

            ConsoleIO.SetAutoCompleteEngine(this);
        }

        /// <summary>
        /// Allows the user to send chat messages, commands, and leave the server.
        /// Process message from the RCC command prompt on the main thread.
        /// </summary>
        private void HandleCommandPromptText(string text)
        {
            text = text.Trim();
            if (text.Length <= 0) return;

            // Send command
            if (ClientConfig.InternalCmdChar == ' ' || text[0] == ClientConfig.InternalCmdChar)
            {
                var command = ClientConfig.InternalCmdChar == ' ' ? text : text[1..];

                if (!PerformInternalCommand(command, out var responseMsg) && ClientConfig.InternalCmdChar == '/')
                {
                    // Command not found
                    Log.Info($"\u00a7c{responseMsg}");
                }
                else if (responseMsg.Length > 0)
                {
                    // Command successful with response
                    Log.Info($"\u00a7f{responseMsg}");
                }
            }
            // Send text
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
            if (text[0] == ClientConfig.InternalCmdChar)
                return;

            lock (_chatQueue)
            {
                if (string.IsNullOrEmpty(text))
                    return;

                if (text.Length > maxLength) //Message is too long?
                {
                    if (text[0] == ClientConfig.InternalCmdChar)
                    {
                        //Send the first 100/256 chars of the command
                        text = text[..maxLength];
                    }
                    else
                    {
                        //Split the message into several messages
                        while (text.Length > maxLength)
                        {
                            _chatQueue.Enqueue(new KeyValuePair<ChatGroupType, string>(Channel, text[..maxLength]));
                            text = text[maxLength..];
                        }
                    }
                }

                _chatQueue.Enqueue(new KeyValuePair<ChatGroupType, string>(Channel, text));
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
        public bool PerformInternalCommand(string command, out string responseMsg, Dictionary<string, object> localVars = null)
        {
            // Process the provided command
            var commandName = command.Split(' ')[0].ToLower();

            if (commandName == HelpCommand)
            {
                if (CommandBase.HasArg(command))
                {
                    var arguments = CommandBase.GetArgs(command)[0].ToLower();

                    if (arguments == HelpCommand)
                    {
                        responseMsg = "{helpCommand} <cmdname>: show brief help about a command.";
                    }
                    else if (_cmds.TryGetValue(arguments, out var cmd))
                    {
                        responseMsg = $"\u00a7e{ClientConfig.InternalCmdChar}{cmd.GetCmdDescTranslated()}";
                    }
                    else responseMsg = $"Unknown command '{arguments}'. Use '{HelpCommand}' for command list.";
                }
                else
                {
                    responseMsg = $"§e--- §fCommands §e---§r\r\n{string.Join(", ", _cmdNames.ToArray())}.";
                }
            }
            else if (_cmds.TryGetValue(commandName, out var cmd))
            {
                try
                {
                    responseMsg = cmd.Run(this, command, localVars);

                    Plugins.OnInternalCommand(commandName, command, responseMsg);
                }
                catch (Exception e)
                {
                    responseMsg = $"Command '{commandName}' caused {e}: {e.Message}\r\n{e.StackTrace}";
                }
            }
            else
            {
                responseMsg = $"Unknown command '{commandName}'. Use '{HelpCommand}' for command list.";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Auto complete text while typing command
        /// </summary>
        public IEnumerable<string> AutoComplete(string behindCursor)
        {
            var ret = new List<string>();

            if (string.IsNullOrEmpty(behindCursor))
                return ret;

            if (behindCursor.StartsWith(ClientConfig.InternalCmdChar))
            {
                var args = behindCursor[1..].Split(' ');

                if (_cmdNames.Contains(args[0].ToLower()) && behindCursor.EndsWith(' '))
                {
                    // command is complete, so show the command arguments
                    var argsPossible = _cmds[args[0].ToLower()].CmdUsage.Split(' ');

                    // only if arguments left
                    if (argsPossible.Length > args.Length - 2)
                        ret.Add(argsPossible[args.Length - 2]);
                }
                else if (!behindCursor.Contains(' '))
                {
                    // tab completion on an uncompleted command
                    ret.AddRange(from cmdName in _cmdNames where cmdName.StartsWith(behindCursor[1..], StringComparison.InvariantCultureIgnoreCase) select ClientConfig.InternalCmdChar + cmdName);

                    if (HelpCommand.StartsWith(behindCursor[1..], StringComparison.InvariantCultureIgnoreCase))
                    {
                        ret.Add(ClientConfig.InternalCmdChar + HelpCommand);
                    }
                }
                else
                {
                    // player names in commands
                    foreach (var entity in GetNetworkManager().GetEntityManager().GetApiEntities())
                    {
                        if (entity == null || (entity.GetEntityType() != EntityType.Player && entity.GetEntityType() != EntityType.User) || !entity.GetDisplayName().ToLower().StartsWith(args.Last().ToLower()))
                            continue;

                        if (entity.GetDisplayName().Trim().Length > 0)
                            ret.Add(entity.GetDisplayName());
                    }

                    // player names in team
                    var databaseManager = GetApiDatabaseManager();
                    var stringManager = GetApiStringManager();
                    var networkManager = GetApiNetworkManager();

                    if (databaseManager == null || stringManager == null || networkManager == null)
                        return ret;

                    for (var gm = 0; gm < 7; gm++)
                    {
                        if (databaseManager.GetProp($"SERVER:GROUP:{gm}:PRESENT") == 0)
                            break;

                        var nameId = databaseManager.GetProp($"SERVER:GROUP:{gm}:NAME");
                        stringManager.GetString((uint)nameId, out var name, networkManager);
                        name = EntityHelper.RemoveTitleAndShardFromName(name);
                        if (!ret.Contains(name))
                            ret.Add(name);
                    }
                }
            }

            return ret;
        }

        /// <summary>
        /// Signalling a disconnection of the client from the server (initiated from RCC)
        /// </summary>
        private void OnDisconnect()
        {
            Plugins.OnDisconnect();

            try
            {
                _cmdprompt?.Interrupt();
                _cmdprompt = null;
            }
            catch
            {
                // ignored
            }

            try
            {
                _timeoutdetector?.Interrupt();
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
        /// <example>BOOL result = InvokeOnMainThread(methodThatReturnsAbool);</example>
        /// <example>BOOL result = InvokeOnMainThread(() => methodThatReturnsAbool(argument));</example>
        /// <example>INT result = InvokeOnMainThread(() => { yourCode(); return 42; });</example>
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
        private static bool InvokeRequired => GetNetReadThreadId() != CurrentThread.ManagedThreadId;

        /// <summary>
        /// Get net read thread (main thread) ID
        /// </summary>
        /// <returns>Net read thread ID</returns>
        private static int GetNetReadThreadId()
        {
            return _clientThread?.ManagedThreadId ?? -1;
        }
        #endregion
    }
}