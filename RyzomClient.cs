﻿///////////////////////////////////////////////////////////////////
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
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using RCC.Chat;
using RCC.Client;
using RCC.Config;
using RCC.Helper;
using RCC.Logger;
using RCC.Messages;
using RCC.Network;

namespace RCC
{
    /// <summary>
    ///     The main client class, used to connect to a Ryzom server.
    /// </summary>
    public class RyzomClient : IChatDisplayer
    {
        private static bool _firstConnection = true;

        private static readonly List<string> CmdNames = new List<string>();
        private static readonly Dictionary<string, Command> Cmds = new Dictionary<string, Command>();

        private static bool _commandsLoaded;

        private static DateTime _nextMessageSendTime = DateTime.MinValue;

        private readonly List<ChatBot> _bots = new List<ChatBot>();
        private static readonly List<ChatBot> BotsOnHold = new List<ChatBot>();

        private static Thread _clientThread;

        private ChatGroupType _channel = ChatGroupType.Around;

        private static uint _lastGameCycle;

        private static RyzomClient _instance;

        private readonly Queue<KeyValuePair<ChatGroupType, string>> _chatQueue = new Queue<KeyValuePair<ChatGroupType, string>>();

        private readonly Queue<Action> _threadTasks = new Queue<Action>();
        private readonly object _threadTasksLock = new object();

        private Thread _cmdprompt;
        private Thread _timeoutdetector;

        public static ILogger Log;
        public static bool UserCharPosReceived = false;

        public ChatGroupType Channel
        {
            get => _channel;
            set
            {
                Log?.Info($"Channel changed to {value}");
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

        #region Initialisation

        /// <summary>
        ///     Starts the main chat client
        /// </summary>
        public RyzomClient()
        {
            _instance = this;
            _clientThread = Thread.CurrentThread;

            StartClient();

            Program.Exit();
        }

        /// <summary>
        ///     Starts the main chat client, wich will login to the server.
        /// </summary>
        private void StartClient()
        {
            Log = ClientConfig.LogToFile
                ? new FileLogLogger(ClientConfig.LogFile, ClientConfig.PrependTimestamp)
                : new FilteredLogger();

            /* Load commands from Commands namespace */
            LoadCommands();

            if (BotsOnHold.Count == 0)
            {
                //Add your ChatBot here by uncommenting and adapting
                if (ClientConfig.OnlinePlayersLogger_Enabled) { BotLoad(new Bots.OnlinePlayersLogger()); }
            }

            foreach (var bot in BotsOnHold)
                BotLoad(bot, false);

            BotsOnHold.Clear();

            _timeoutdetector = new Thread(TimeoutDetector) { Name = "RCC Connection timeout detector" };
            _timeoutdetector.Start();

            _cmdprompt = new Thread(CommandPrompt) { Name = "RCC Command prompt" };
            _cmdprompt.Start();

            Main();
        }

        #endregion

        #region IChatDisplayer
        /// <summary>
        /// Returns the client as chat displayer instance (singleton pattern)
        /// </summary>
        /// <returns></returns>
        internal static IChatDisplayer GetInstance()
        {
            return _instance;
        }

        public void DisplayChat(uint compressedSenderIndex, string ucstr, string rawMessage, ChatGroupType mode,
            uint dynChatId, string senderName, uint bubbleTimer = 0)
        {
            var color = Misc.GetMinecraftColorForChatGroupType(mode);

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
                        Console.ForegroundColor = Misc.FromColor(col);
                    }
                }
            }

            Log.Chat(
                $"[{mode}]{(stringCategory.Length > 0 ? $"[{stringCategory.ToUpper()}]" : "")}{color} {finalString}");

            OnChat(compressedSenderIndex, ucstr, rawMessage, mode, dynChatId, senderName, bubbleTimer);
        }

        public void DisplayTell(string ucstr, string senderName)
        {
            Log.Chat(ucstr);

            OnTell(ucstr, senderName);
        }

        public void ClearChannel(ChatGroupType mode, uint dynChatDbIndex)
        {
            _chatQueue.Clear();
        }
        #endregion IChatDisplayer

        #region Ryzom Game Loop

        /// <summary>
        ///     Initialize the application. Login to the server. Main loop.
        /// </summary>
        private void Main()
        {
            // Login State Machine
            if (!Login())
            {
                Log?.Error("Could not login!");
                return;
            }

            // init message database
            PostlogInit();

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
                StringManagerClient.FlushStringCache();
            }

            Log?.Info("EXIT of the Application.");
        }

        /// <summary>
        ///     Initialize the main loop.
        ///     If you add something in this function, check CFarTP,
        ///     some kind of reinitialization might be useful over there.
        /// </summary>
        private static void InitMainLoop()
        {
            // Update Network till current tick increase.
            _lastGameCycle = NetworkConnection.GetCurrentServerTick();

            while (_lastGameCycle == NetworkConnection.GetCurrentServerTick())
            {
                // Update Network.
                NetworkManager.Update();
            }

            // Create the message for the server to create the character.
            var out2 = new BitMemoryStream();

            if (GenericMessageHeaderManager.PushNameToStream("CONNECTION:READY", out2))
            {
                out2.Serial(ref ClientConfig.LanguageCode);
                NetworkManager.Push(out2);
                NetworkManager.Send(NetworkConnection.GetCurrentServerTick());
            }
            else
            {
                Log?.Info("initMainLoop : unknown message name : 'CONNECTION:READY'.");
            }
        }

        /// <summary>
        ///     Called from client.cpp
        ///     start the login state machine
        /// </summary>
        private bool Login()
        {
            var loggedIn = false;
            var firstRetry = true;

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
        ///     Initialize the application after login
        ///     if the init fails, call nlerror
        /// </summary>
        private static void PostlogInit()
        {
            //std::string msgXMLPath = CPath::lookup("msg.xml");
            const string msgXmlPath = "./data/msg.xml";
            GenericMessageHeaderManager.Init(msgXmlPath);

            // Initialize the Generic Message Header Manager.
            NetworkManager.InitializeNetwork();

            // todo: init the chat manager
            // ChatManager.init(CPath::lookup("chat_static.cdb"));
        }

        /// <summary>
        ///     New version of the menu after the server connection
        ///     If you add something in this function, check CFarTP,
        ///     some kind of reinitialization might be useful over there.
        /// </summary>
        private bool Connection(string cookie, string fsaddr)
        {
            Network.Connection.GameExit = false;

            // Init global variables
            Network.Connection.UserChar = false;
            Network.Connection.NoUserChar = false;
            Network.Connection.ConnectInterf = true;
            Network.Connection.CreateInterf = true;
            Network.Connection.CharacterInterf = true;
            Network.Connection.WaitServerAnswer = false;

            // Start the finite state machine
            var interfaceState = InterfaceState.AutoLogin;

            while (interfaceState != InterfaceState.GoInTheGame &&
                   interfaceState != InterfaceState.QuitTheGame)
            {
                switch (interfaceState)
                {
                    case InterfaceState.AutoLogin:
                        interfaceState = AutoLogin(cookie, fsaddr, _firstConnection);
                        break;

                    case InterfaceState.GlobalMenu:
                        // Interface to choose a char
                        interfaceState = GlobalMenu();
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            _firstConnection = false;

            // GOGOGO_IN_THE_GAME
            return interfaceState == InterfaceState.GoInTheGame;
        }

        /// <summary>
        /// Establish network connection, set callbacks for server messages and init the string manager
        /// </summary>
        private static InterfaceState AutoLogin(string cookie, string fsaddr, in bool firstConnection)
        {
            if (firstConnection)
                NetworkConnection.Init(cookie, fsaddr);

            if (firstConnection)
            {
                try
                {
                    NetworkConnection.Connect();
                }
                catch (Exception e)
                {
                    Log?.Error($"Auto Login error: {e.Message}.");
                    return InterfaceState.QuitTheGame;
                }

                // Ok the client is connected
                // Set the impulse callback.
                NetworkConnection.SetImpulseCallback(NetworkManager.ImpulseCallBack);

                // Set the database.
                //TODO NetworkConnection.setDataBase(IngameDbMngr.getNodePtr());

                // init the string manager cache.
                StringManagerClient.InitCache(fsaddr, ClientConfig.LanguageCode);
            }

            Network.Connection.WaitServerAnswer = true;

            return InterfaceState.GlobalMenu;
        }

        /// <summary>
        ///     Launch the interface to choose a character
        /// </summary>
        public InterfaceState GlobalMenu()
        {
            var serverTick = NetworkConnection.GetCurrentServerTick();
            var playerWantToGoInGame = false;
            var firewallTimeout = false;

            while (!playerWantToGoInGame)
            {
                // Update network.
                try
                {
                    if (!firewallTimeout)
                        NetworkManager.Update();
                }
                catch
                {
                    if (NetworkConnection.ConnectionState == ConnectionState.Disconnect)
                    {
                        firewallTimeout = true;
                    }
                    else
                    {
                        Thread.Sleep(30);
                    }
                }

                // TODO: IngameDbMngr.flushObserverCalls();

                // check if we can send another dated block
                if (NetworkConnection.GetCurrentServerTick() != serverTick)
                {
                    serverTick = NetworkConnection.GetCurrentServerTick();
                    NetworkConnection.Send(serverTick);
                }
                else
                {
                    // Send dummy info
                    NetworkConnection.Send();
                }

                // TODO: updateClientTime();
                // TODO: IngameDbMngr.flushObserverCalls();

                // SERVER INTERACTIONS WITH INTERFACE
                if (Network.Connection.WaitServerAnswer)
                {
                    // Should we send the char selection without any user interaction?
                    if (Network.Connection.AutoSendCharSelection)
                    {
                        var charSelect = -1;

                        if (ClientConfig.SelectCharacter != -1)
                            charSelect = ClientConfig.SelectCharacter;

                        Network.Connection.WaitServerAnswer = false;

                        // check that the pre selected character is available
                        if (Network.Connection.CharacterSummaries[charSelect].People == (int)PeopleType.Unknown || charSelect > 4)
                        {
                            // BAD ! preselected char does not exist
                            throw new InvalidOperationException("preselected char does not exist");
                        }

                        // Auto-selection for fast launching (dev only)
                        Network.Connection.AutoSendCharSelection = false;
                        ActionHandlerLaunchGame.Execute(charSelect.ToString());
                    }

                    // Clear sending buffer that may contain prevous QUIT_GAME when getting back to the char selection screen
                    NetworkConnection.FlushSendBuffer();
                }

                if (NetworkManager.ServerReceivedReady)
                {
                    //nlinfo("impulseCallBack : received serverReceivedReady");
                    NetworkManager.ServerReceivedReady = false;
                    Network.Connection.WaitServerAnswer = false;
                    playerWantToGoInGame = true;
                }

                if (NetworkConnection.ConnectionState == ConnectionState.Disconnect)
                {
                    // Display the connection failure screen
                    if (firewallTimeout)
                    {
                        // Display the firewall error string instead of the normal failure string
                        Log?.Error("Firewall Fail (Timeout)");
                    }
                }

                if (Network.Connection.GameExit)
                    return InterfaceState.QuitTheGame;
            }

            //  Init the current Player Name
            var playerName = Network.Connection.CharacterSummaries[Network.Connection.PlayerSelectedSlot].Name;

            // Init the current Player name
            Network.Connection.PlayerSelectedHomeShardName = playerName;
            Network.Connection.PlayerSelectedHomeShardNameWithParenthesis = '(' + playerName + ')';

            return InterfaceState.GoInTheGame;
        }

        /// <summary>
        /// Main ryzom game loop
        /// </summary>
        private bool MainLoop()
        {
            Log?.Debug("mainLoop");

            // Main loop. If the window is no more Active -> Exit.
            while (!Network.Connection.GameExit)
            {
                // Do not eat up all the processor
                Thread.Sleep(10);

                // Update Ryzom Client stuff -> Execute Tasks (like commands and bot stuff)
                OnUpdate();

                // NetWork Update.
                NetworkManager.Update();

                // Send new data Only when server tick changed.
                if (NetworkConnection.GetCurrentServerTick() > _lastGameCycle)
                {
                    // Send the Packet.
                    NetworkManager.Send(NetworkConnection.GetCurrentServerTick());

                    // Update the Last tick received from the server.
                    _lastGameCycle = NetworkConnection.GetCurrentServerTick();
                }

                // Get the Connection State.
                var connectionState = NetworkConnection.ConnectionState;

                if (connectionState == ConnectionState.Disconnect || connectionState == ConnectionState.Quit)
                {
                    Network.Connection.GameExit = true;
                    break;
                }

            } // end of main loop

            return true;
        }

        #endregion

        #region Concurrent Threads

        /// <summary>
        ///     Periodically checks for server keepalives and consider that connection has been lost if the last received keepalive
        ///     is too old.
        /// </summary>
        private void TimeoutDetector()
        {
            // TODO: TimeoutDetector
            //UpdateKeepAlive();
            //do
            //{
            //    Thread.Sleep(TimeSpan.FromSeconds(15));
            //    lock (lastKeepAliveLock)
            //    {
            //        if (lastKeepAlive.AddSeconds(30) < DateTime.Now)
            //        {
            //            OnConnectionLost(ChatBot.DisconnectReason.ConnectionLost, Translations.Get("error.timeout"));
            //            return;
            //        }
            //    }
            //}
            //while (true);
        }

        /// <summary>
        ///     Allows the user to send chat messages, commands, and leave the server.
        /// </summary>
        private void CommandPrompt()
        {
            try
            {
                Thread.Sleep(500);
                while (true /*NetworkConnection._ConnectionState == ConnectionState.Connected*/)
                {
                    var text = ConsoleIO.ReadLine();
                    InvokeOnMainThread(() => HandleCommandPromptText(text));
                }
            }
            catch (IOException) { }
            catch (NullReferenceException) { }
        }

        #endregion

        #region Console Client Methods 

        /// <summary>
        /// Load a new bot
        /// </summary>
        public void BotLoad(ChatBot b, bool init = true)
        {
            if (InvokeRequired)
            {
                InvokeOnMainThread(() => BotLoad(b, init));
                return;
            }

            b.SetHandler(this);
            _bots.Add(b);

            if (init)
                DispatchBotEvent(bot => bot.Initialize(), new[] { b });
            if (NetworkConnection.ConnectionState == ConnectionState.Connected)
                DispatchBotEvent(bot => bot.OnGameJoined(), new[] { b });
        }

        /// <summary>
        /// Unload a bot
        /// </summary>
        public void BotUnLoad(ChatBot b)
        {
            if (InvokeRequired)
            {
                InvokeOnMainThread(() => BotUnLoad(b));
                return;
            }

            _bots.RemoveAll(item => ReferenceEquals(item, b));
        }

        /// <summary>
        /// Called ~10 times per second by the protocol handler
        /// </summary>
        public void OnUpdate()
        {
            foreach (var bot in _bots.ToArray())
            {
                try
                {
                    bot.Update();
                    bot.UpdateInternal();
                }
                catch (Exception e)
                {
                    if (!(e is ThreadAbortException))
                    {
                        Log.Warn($"Update: Got error from {bot}: {e}");
                    }
                    else throw; //ThreadAbortException should not be caught
                }
            }

            lock (_chatQueue)
            {
                if (_chatQueue.Count > 0 && _nextMessageSendTime < DateTime.Now)
                {
                    var (key, value) = _chatQueue.Dequeue();
                    SendChatMessage(value, key);
                    _nextMessageSendTime = DateTime.Now + TimeSpan.FromSeconds(2);
                }
            }

            lock (_threadTasksLock)
            {
                while (_threadTasks.Count > 0)
                {
                    Action taskToRun = _threadTasks.Dequeue();
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

                if (GenericMessageHeaderManager.PushNameToStream(msgType, bms))
                {
                    bms.Serial(ref mode);
                    bms.Serial(ref dynamicChannelId);
                    NetworkManager.Push(bms);
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
                if (GenericMessageHeaderManager.PushNameToStream(msgType, out2))
                {
                    out2.Serial(ref message);
                    NetworkManager.Push(out2);
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
        ///     Load commands from the 'Commands' namespace
        /// </summary>
        public void LoadCommands()
        {
            if (_commandsLoaded) return;

            var cmdsClasses = Program.GetTypesInNamespace("RCC.Commands");
            foreach (var type in cmdsClasses)
            {
                if (!type.IsSubclassOf(typeof(Command))) continue;

                try
                {
                    var cmd = (Command)Activator.CreateInstance(type);

                    if (cmd != null)
                    {
                        Cmds[cmd.CmdName.ToLower()] = cmd;
                        CmdNames.Add(cmd.CmdName.ToLower());
                        foreach (var alias in cmd.getCMDAliases())
                            Cmds[alias.ToLower()] = cmd;
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
        ///     Allows the user to send chat messages, commands, and leave the server.
        ///     Process message from the RCC command prompt on the main thread.
        /// </summary>
        private void HandleCommandPromptText(string text)
        {
            text = text.Trim();
            if (text.Length <= 0) return;

            if (ClientConfig.InternalCmdChar == ' ' || text[0] == ClientConfig.InternalCmdChar)
            {
                var responseMsg = "";
                var command = ClientConfig.InternalCmdChar == ' ' ? text : text.Substring(1);

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
        ///     Send a chat message or command to the server (Enqueues messages)
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
                            text = text.Substring(maxLength, text.Length - maxLength);
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
        public bool RegisterCommand(string cmdName, string cmdDesc, string cmdUsage, ChatBot.CommandRunner callback)
        {
            if (Cmds.ContainsKey(cmdName.ToLower()))
            {
                return false;
            }

            Command cmd = new ChatBot.ChatBotCommand(cmdName, cmdDesc, cmdUsage, callback);
            Cmds.Add(cmdName.ToLower(), cmd);
            CmdNames.Add(cmdName.ToLower());
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
            if (!Cmds.ContainsKey(cmdName.ToLower())) return false;

            Cmds.Remove(cmdName.ToLower());
            CmdNames.Remove(cmdName.ToLower());
            return true;

        }

        /// <summary>
        ///     Perform an internal RCC command (not a server command, use SendText() instead for that!)
        /// </summary>
        /// <param name="command">The command</param>
        /// <param name="responseMsg">May contain a confirmation or error message after processing the command, or "" otherwise.</param>
        /// <param name="localVars">Local variables passed along with the command</param>
        /// <returns>TRUE if the command was indeed an internal RCC command</returns>
        public bool PerformInternalCommand(string command, ref string responseMsg,
            Dictionary<string, object> localVars = null)
        {
            if (responseMsg == null) throw new ArgumentNullException(nameof(responseMsg));

            // Process the provided command
            var commandName = command.Split(' ')[0].ToLower();

            if (commandName == "help")
            {
                if (Command.hasArg(command))
                {
                    var helpCmdname = Command.getArgs(command)[0].ToLower();

                    if (helpCmdname == "help")
                    {
                        responseMsg = "help <cmdname>: show brief help about a command.";
                    }
                    else if (Cmds.ContainsKey(helpCmdname))
                    {
                        responseMsg = Cmds[helpCmdname].GetCmdDescTranslated();
                    }
                    else responseMsg = $"Unknown command '{commandName}'. Use 'help' for command list.";
                }
                else
                    responseMsg =
                        $"help <cmdname>. Available commands: {string.Join(", ", CmdNames.ToArray())}. For server help, use '{ClientConfig.InternalCmdChar}send /help' instead.";
            }
            else if (Cmds.ContainsKey(commandName))
            {
                responseMsg = Cmds[commandName].Run(this, command, localVars);

                foreach (var bot in _bots.ToArray())
                {
                    try
                    {
                        bot.OnInternalCommand(commandName, string.Join(" ", Command.getArgs(command)), responseMsg);
                    }
                    catch (Exception e)
                    {
                        //ThreadAbortException should not be caught
                        if (!(e is ThreadAbortException))
                        {
                            Log.Warn("icmd.error " + bot + " " + e);
                        }
                        else throw;
                    }
                }
            }
            else
            {
                responseMsg = "Unknown command '{command_name}'. Use 'help' for command list.";
                return false;
            }

            return true;
        }

        /// <summary>
        ///     Disconnect the client from the server (initiated from RCC)
        /// </summary>
        public void Disconnect()
        {
            DispatchBotEvent(bot => bot.OnDisconnect(ChatBot.DisconnectReason.UserLogout, ""));

            BotsOnHold.Clear();
            BotsOnHold.AddRange(_bots);

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

        /// <summary>
        /// Dispatch a ChatBot event with automatic exception handling
        /// </summary>
        /// <example>
        /// Example for calling SomeEvent() on all bots at once:
        /// DispatchBotEvent(bot => bot.SomeEvent());
        /// </example>
        /// <param name="action">Action to execute on each bot</param>
        /// <param name="botList">Only fire the event for the specified bot list (default: all bots)</param>
        private void DispatchBotEvent(Action<ChatBot> action, IEnumerable<ChatBot> botList = null)
        {
            var selectedBots = botList != null ? botList.ToArray() : _bots.ToArray();

            foreach (var bot in selectedBots)
            {
                try
                {
                    action(bot);
                }
                catch (Exception e)
                {
                    if (!(e is ThreadAbortException))
                    {
                        //Retrieve parent method name to determine which event caused the exception
                        var frame = new System.Diagnostics.StackFrame(1);
                        var method = frame.GetMethod();
                        var parentMethodName = method?.Name;

                        //Display a meaningful error message to help debugging the ChatBot
                        Log.Error($"{parentMethodName}: Got error from {bot}: {e}");
                    }
                    else throw; //ThreadAbortException should not be caught here as in can happen when disconnecting from server
                }
            }
        }

        #endregion

        #region Thread-Invoke: Cross-thread method calls

        /// <summary>
        ///     Invoke a task on the main thread, wait for completion and retrieve return value.
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

            lock (_threadTasksLock)
            {
                _threadTasks.Enqueue(taskWithResult.ExecuteSynchronously);
            }

            return taskWithResult.WaitGetResult();
        }

        /// <summary>
        ///     Invoke a task on the main thread and wait for completion
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
        ///     Check if running on a different thread and InvokeOnMainThread is required
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

        #region Event API

        /// <summary>
        /// Called when a server was successfully joined
        /// </summary>
        public void OnGameJoined()
        {
            DispatchBotEvent(bot => bot.OnGameJoined());
        }

        /// <summary>
        /// Called when one of the characters from the friend list updates
        /// </summary>
        /// <param name="contactId">id</param>
        /// <param name="online">new status</param>
        public void OnGameTeamContactStatus(uint contactId, CharConnectionState online)
        {
            DispatchBotEvent(bot => bot.OnGameTeamContactStatus(contactId, online));
        }

        /// <summary>
        /// Called when friend list and ignore list from the contact list are initialized
        /// </summary>
        internal void OnGameTeamContactInit(List<uint> vFriendListName, List<CharConnectionState> vFriendListOnline, List<string> vIgnoreListName)
        {
            DispatchBotEvent(bot => bot.OnGameTeamContactInit(vFriendListName, vFriendListOnline, vIgnoreListName));
        }

        /// <summary>
        /// Called when one character from the friend or ignore list is created
        /// </summary>
        internal void OnTeamContactCreate(uint contactId, uint nameId, CharConnectionState online, byte nList)
        {
            DispatchBotEvent(bot => bot.OnTeamContactCreate(contactId, nameId, online, nList));
        }

        /// <summary>
        /// Any chat will arrive here 
        /// </summary>
        internal void OnChat(uint compressedSenderIndex, string ucstr, string rawMessage, ChatGroupType mode, uint dynChatId, string senderName, uint bubbleTimer)
        {
            DispatchBotEvent(bot => bot.OnChat(compressedSenderIndex, ucstr, rawMessage, mode, dynChatId, senderName, bubbleTimer));
        }

        /// <summary>
        /// Any tells will arrive here 
        /// </summary>
        internal void OnTell(string ucstr, string senderName)
        {
            DispatchBotEvent(bot => bot.OnTell(ucstr, senderName));
        }

        #endregion
    }
}