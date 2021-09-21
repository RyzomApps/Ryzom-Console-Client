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
        public static bool UserCharPosReceived = false;

        static bool _firstConnection = true;

        // Control stuff
        private static readonly List<string> CmdNames = new List<string>();
        private static readonly Dictionary<string, Command> Cmds = new Dictionary<string, Command>();

        private static bool _commandsLoaded = false;

        private static DateTime nextMessageSendTime = DateTime.MinValue;

        private readonly List<ChatBot> bots = new List<ChatBot>();
        private static readonly List<ChatBot> botsOnHold = new List<ChatBot>();

        public ChatGroupType Channel
        {
            get => _channel;
            set
            {
                Log?.Info($"Channel changed to {value}");
                _channel = value;
            }
        }

        private ChatGroupType _channel = ChatGroupType.Around;

        static uint _lastGameCycle;

        public static ILogger Log;

        static RyzomClient _instance;

        private readonly Queue<KeyValuePair<ChatGroupType, string>> _chatQueue = new Queue<KeyValuePair<ChatGroupType, string>>();

        private readonly Queue<Action> _threadTasks = new Queue<Action>();
        private readonly object _threadTasksLock = new object();

        Thread _cmdprompt;
        Thread _timeoutdetector;

        /// <summary>
        ///     Starts the main chat client
        /// </summary>
        public RyzomClient()
        {
            _instance = this;
            StartClient();

            Program.Exit(0);
        }

        public string Cookie { get; set; }
        public string FsAddr { get; set; }
        public string RingMainURL { get; set; }
        public string FarTpUrlBase { get; set; }
        public bool StartStat { get; set; }
        public string R2ServerVersion { get; set; }
        public string R2BackupPatchURL { get; set; }
        public string[] R2PatchUrLs { get; set; }

        public List<ChatBot> GetLoadedChatBots() { return new List<ChatBot>(bots); }

        public void DisplayChat(uint compressedSenderIndex, string ucstr, string rawMessage, ChatGroupType mode,
            uint dynChatId, string senderName, uint bubbleTimer = 0)
        {
            var color = "§f";

            switch (mode)
            {
                case ChatGroupType.DynChat:
                    color = "§b";
                    break;
                case ChatGroupType.Shout:
                    color = "§c";
                    break;
                case ChatGroupType.Team:
                    color = "§9";
                    break;
                case ChatGroupType.Guild:
                    color = "§a";
                    break;
                case ChatGroupType.Civilization:
                    color = "§d";
                    break;
                case ChatGroupType.Territory:
                    color = "§d";
                    break;
                case ChatGroupType.Universe:
                    color = "§6";
                    break;
                case ChatGroupType.Region:
                    color = "§7";
                    break;
                case ChatGroupType.Tell:
                    color = "§f";
                    break;
                default:
                    /*nlwarning("unknown group type"); return;*/
                    break;
            }

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
        }

        internal static IChatDisplayer GetInstance()
        {
            return _instance;
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

            if (botsOnHold.Count == 0)
            {
                //if (Settings.AntiAFK_Enabled) { BotLoad(new ChatBots.AntiAFK(Settings.AntiAFK_Delay)); }
                //if (Settings.Hangman_Enabled) { BotLoad(new ChatBots.HangmanGame(Settings.Hangman_English)); }
                //if (Settings.Alerts_Enabled) { BotLoad(new ChatBots.Alerts()); }
                //if (Settings.ChatLog_Enabled) { BotLoad(new ChatBots.ChatLog(Settings.ExpandVars(Settings.ChatLog_File), Settings.ChatLog_Filter, Settings.ChatLog_DateTime)); }
                //if (Settings.PlayerLog_Enabled) { BotLoad(new ChatBots.PlayerListLogger(Settings.PlayerLog_Delay, Settings.ExpandVars(Settings.PlayerLog_File))); }
                //if (Settings.AutoRelog_Enabled) { BotLoad(new ChatBots.AutoRelog(Settings.AutoRelog_Delay_Min, Settings.AutoRelog_Delay_Max, Settings.AutoRelog_Retries)); }
                //if (Settings.ScriptScheduler_Enabled) { BotLoad(new ChatBots.ScriptScheduler(Settings.ExpandVars(Settings.ScriptScheduler_TasksFile))); }
                //if (Settings.RemoteCtrl_Enabled) { BotLoad(new ChatBots.RemoteControl()); }
                //if (Settings.AutoRespond_Enabled) { BotLoad(new ChatBots.AutoRespond(Settings.AutoRespond_Matches)); }
                //if (Settings.AutoAttack_Enabled) { BotLoad(new ChatBots.AutoAttack(Settings.AutoAttack_Mode, Settings.AutoAttack_Priority, Settings.AutoAttack_OverrideAttackSpeed, Settings.AutoAttack_CooldownSeconds)); }
                //if (Settings.AutoFishing_Enabled) { BotLoad(new ChatBots.AutoFishing()); }
                //if (Settings.AutoEat_Enabled) { BotLoad(new ChatBots.AutoEat(Settings.AutoEat_hungerThreshold)); }
                //if (Settings.Mailer_Enabled) { BotLoad(new ChatBots.Mailer()); }
                //if (Settings.AutoCraft_Enabled) { BotLoad(new AutoCraft(Settings.AutoCraft_configFile)); }
                //if (Settings.AutoDrop_Enabled) { BotLoad(new AutoDrop(Settings.AutoDrop_Mode, Settings.AutoDrop_items)); }
                //if (Settings.ReplayMod_Enabled) { BotLoad(new ReplayCapture(Settings.ReplayMod_BackupInterval)); }

                //Add your ChatBot here by uncommenting and adapting
                BotLoad(new Bots.OnlinePlayersLogger());
            }

            //foreach (ChatBot bot in botsOnHold)
            //    BotLoad(bot, false);
            //botsOnHold.Clear();

            _timeoutdetector = new Thread(TimeoutDetector) { Name = "RCC Connection timeout detector" };
            _timeoutdetector.Start();

            foreach (ChatBot bot in botsOnHold)
                BotLoad(bot, false);
            botsOnHold.Clear();

            _cmdprompt = new Thread(new ThreadStart(CommandPrompt));
            _cmdprompt.Name = "RCC Command prompt";
            _cmdprompt.Start();

            Main();
        }

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
            bots.Add(b);
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

            bots.RemoveAll(item => ReferenceEquals(item, b));
        }

        /// <summary>
        /// Called ~10 times per second by the protocol handler
        /// </summary>
        public void OnUpdate()
        {
            foreach (var bot in bots.ToArray())
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
                        Log.Warn("Update: Got error from " + bot.ToString() + ": " + e.ToString());
                    }
                    else throw; //ThreadAbortException should not be caught
                }
            }

            lock (_chatQueue)
            {
                if (_chatQueue.Count <= 0 || nextMessageSendTime >= DateTime.Now) return;

                var message = _chatQueue.Dequeue();
                SendChatMessage(message.Value, message.Key);
                nextMessageSendTime = DateTime.Now + TimeSpan.FromSeconds(2);
            }

            //lock (threadTasksLock)
            //{
            //    while (threadTasks.Count > 0)
            //    {
            //        Action taskToRun = threadTasks.Dequeue();
            //        taskToRun();
            //    }
            //}
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

                BitMemoryStream bms = new BitMemoryStream();
                string msgType = "STRING:CHAT_MODE";
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
            if (!_commandsLoaded)
            {
                Type[] cmdsClasses = Program.GetTypesInNamespace("RCC.Commands");
                foreach (Type type in cmdsClasses)
                {
                    if (!type.IsSubclassOf(typeof(Command))) continue;

                    try
                    {
                        Command cmd = (Command)Activator.CreateInstance(type);
                        Cmds[cmd.CmdName.ToLower()] = cmd;
                        CmdNames.Add(cmd.CmdName.ToLower());
                        foreach (string alias in cmd.getCMDAliases())
                            Cmds[alias.ToLower()] = cmd;
                    }
                    catch (Exception e)
                    {
                        Log.Warn(e.Message);
                    }
                }

                _commandsLoaded = true;
            }
        }

        /// <summary>
        ///     Periodically checks for server keepalives and consider that connection has been lost if the last received keepalive
        ///     is too old.
        /// </summary>
        private void TimeoutDetector()
        {
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
            catch (IOException)
            {
            }
            catch (NullReferenceException)
            {
            }
        }

        /// <summary>
        ///     Allows the user to send chat messages, commands, and leave the server.
        ///     Process message from the RCC command prompt on the main thread.
        /// </summary>
        private void HandleCommandPromptText(string text)
        {
            //if (ConsoleIO.BasicIO && message.Length > 0 && message[0] == (char)0x00)
            //{
            //    //Process a request from the GUI
            //    string[] command = message.Substring(1).Split((char)0x00);
            //    switch (command[0].ToLower())
            //    {
            //        case "autocomplete":
            //            if (command.Length > 1) { ConsoleIO.WriteLine((char)0x00 + "autocomplete" + (char)0x00 + handler.AutoComplete(command[1])); }
            //            else Console.WriteLine((char)0x00 + "autocomplete" + (char)0x00);
            //            break;
            //    }
            //}
            //else
            //{
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

            //}
        }

        /// <summary>
        ///     Send a chat message or command to the server
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
        /// <param name="callback">Method for handling the command</param>
        /// <returns>True if successfully registered</returns>
        public bool RegisterCommand(string cmdName, string cmdDesc, string cmdUsage, ChatBot.CommandRunner callback)
        {
            if (Cmds.ContainsKey(cmdName.ToLower()))
            {
                return false;
            }
            else
            {
                Command cmd = new ChatBot.ChatBotCommand(cmdName, cmdDesc, cmdUsage, callback);
                Cmds.Add(cmdName.ToLower(), cmd);
                CmdNames.Add(cmdName.ToLower());
                return true;
            }
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
            if (Cmds.ContainsKey(cmdName.ToLower()))
            {
                Cmds.Remove(cmdName.ToLower());
                CmdNames.Remove(cmdName.ToLower());
                return true;
            }
            else return false;
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
            /* Process the provided command */

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
                //foreach (ChatBot bot in bots.ToArray())
                //{
                //    try
                //    {
                //        bot.OnInternalCommand(command_name, string.Join(" ", Command.getArgs(command)), response_msg);
                //    }
                //    catch (Exception e)
                //    {
                //        if (!(e is ThreadAbortException))
                //        {
                //            Log.Warn(Translations.Get("icmd.error", bot.ToString(), e.ToString()));
                //        }
                //        else throw; //ThreadAbortException should not be caught
                //    }
                //}
            }
            else
            {
                responseMsg = "Unknown command '{command_name}'. Use 'help' for command list.";
                return false;
            }

            return true;
        }

        /// <summary>
        ///     Login to the server.
        /// </summary>
        private void Main()
        {
            /////////////////////////////////
            // Initialize the application. //
            /////////////////////////////////

            // prelogInit(); <- init config file, 3d driver, display

            // Login State Machine
            if (!Login())
            {
                Log?.Error("Could not login!");
                return;
            }

            PostlogInit(); // <- message database

            ////////////////////////
            // The real main loop //
            ////////////////////////

            var ok = true;

            while (ok)
            {
                //////////////////////////////////////////
                // Manage the connection to the server. //
                //////////////////////////////////////////

                // If the connection return false we just want to quit the game
                if (!Connection(Cookie, FsAddr))
                {
                    //releaseOutGame();
                    break;
                }

                ///////////////////////////////
                // Initialize the main loop. //
                ///////////////////////////////

                InitMainLoop();

                //////////////////////////////////////////////////
                // Main loop (biggest part of the application). //
                //////////////////////////////////////////////////

                ok = !MainLoop();

                /////////////////////////////
                // Release all the memory. //
                /////////////////////////////

                //if (!FarTP.isReselectingChar())
                //{
                //releaseMainLoop(!ok);
                StringManagerClient.FlushStringCache();
                //}
            }

            // Final release
            // release();

            Log?.Info("EXIT of the Application.");
        }

        /// <summary>
        ///     Initialize the main loop.
        ///     If you add something in this function, check CFarTP,
        ///     some kind of reinitialization might be useful over there.
        /// </summary>
        private void InitMainLoop()
        {
            // Create the game interface database

            // Create interface database

            // Ask and receive the user position to start (Olivier: moved here because needed by initInGame())
            //waitForUserCharReceived();

            // Starting to load data.

            // Initializing Entities Manager.

            // Creating Scene.
            // Creating Landscape.
            // Create the cloud scape
            // Initialize World and select the right continent.
            // Initialize the collision manager.
            // Set the Main Camera.

            Thread.Sleep(1000);

            // Network Mode
            //if (UserCharPosReceived && !ConnectionReadySent)?

            // Update Network till current tick increase.
            _lastGameCycle = NetworkConnection.GetCurrentServerTick();
            while (_lastGameCycle == NetworkConnection.GetCurrentServerTick())
            {
                // Event server get events
                //CInputHandlerManager::getInstance()->pumpEventsNoIM();
                // Update Network.
                NetworkManager.Update();
                //IngameDbMngr.flushObserverCalls();
                //NLGUI::CDBManager::getInstance()->flushObserverCalls();
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
            // ev_init_done -> st_auto_login
            // username and password set
            // onlogin -> main menu page for r2mode -> ev_login_ok

            var loggedIn = false;
            var firstRetry = true;

            while (!loggedIn)
            {
                try
                {
                    // string res = checkLogin(LoginLogin, LoginPassword, ClientApp, LoginCustomParameters);
                    Network.Login.CheckLogin(this, ClientConfig.Username, ClientConfig.Password,
                        ClientConfig.ApplicationServer,
                        "");
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

            // ev_login_ok -> ... -> st_connect
            // -> st_ingame

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

            // init the chat manager
            // TODO: ChatManager.init(CPath::lookup("chat_static.cdb"));

            // Read the ligo primitive class file

            // Init the sound manager

            // Initialize Sheet IDs.

            // Initializing bricks

            // Initialize the color slot manager

            // Register the ligo primitives for .primitive sheets
        }

        /// <summary>
        ///     New version of the menu after the server connection
        ///     If you add something in this function, check CFarTP,
        ///     some kind of reinitialization might be useful over there.
        /// </summary>
        private bool Connection(string cookie, string fsaddr)
        {
            Network.Connection.GameExit = false;

            // Preload continents

            // Init out game

            // Init user interface

            // Init global variables
            Network.Connection.UserChar = false;
            Network.Connection.NoUserChar = false;
            Network.Connection.ConnectInterf = true;
            Network.Connection.CreateInterf = true;
            Network.Connection.CharacterInterf = true;
            Network.Connection.WaitServerAnswer = false;

            //FarTP.setOutgame();

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
                }
            }

            _firstConnection = false;

            // GOGOGO_IN_THE_GAME
            return interfaceState == InterfaceState.GoInTheGame;
        }

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
            uint serverTick = NetworkConnection.GetCurrentServerTick();
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
                        if (Network.Connection.CharacterSummaries[charSelect].People == (int)PeopleType.Unknown ||
                            charSelect > 4)
                        {
                            // BAD ! preselected char does not exist
                            throw new InvalidOperationException("preselected char does not exist");
                        }

                        // Auto-selection for fast launching (dev only)
                        //CAHManager::getInstance()->runActionHandler("launch_game", NULL, toString("slot=%d|edit_mode=0", charSelect));
                        Network.Connection.AutoSendCharSelection = false;
                        ActionHandlerLaunchGame.Execute(charSelect.ToString());
                    }

                    // Clear sending buffer that may contain prevous QUIT_GAME when getting back to the char selection screen
                    NetworkConnection.FlushSendBuffer();
                }

                //// Ask the server if the name is not already used -> not used since we do not create chars
                //if (Connection.CharNameValidArrived)
                //{
                //    nlinfo("impulseCallBack : received CharNameValidArrived");
                //    Connection.CharNameValidArrived = false;
                //    Connection.WaitServerAnswer = false;
                //}

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
                    // TODO Display the connection failure screen

                    if (firewallTimeout)
                    {
                        // Display the firewall error string instead of the normal failure string
                        Log?.Warn("Firewall Fail (Timeout)");
                    }
                }

                if (Network.Connection.GameExit)
                    return InterfaceState.QuitTheGame;
            }

            //if (ClientConfig.SelectCharacter != -1)
            //    PlayerSelectedSlot = ClientConfig.SelectCharacter;

            // -> ev_global_menu_exited

            //  Init the current Player Name (for interface.cfg and sentence.name save). Make a good File Name.
            string playerName = Network.Connection.CharacterSummaries[Network.Connection.PlayerSelectedSlot].Name;
            //Client.Connection.PlayerSelectedFileName = buildPlayerNameForSaveFile(playerName);

            // Init the current Player Home shard Id and name
            //Client.Connection.CharacterHomeSessionId = Client.Connection.CharacterSummaries[Client.Connection.PlayerSelectedSlot].Mainland;
            //Client.Connection.PlayerSelectedMainland = Client.Connection.CharacterSummaries[Client.Connection.PlayerSelectedSlot].Mainland;
            //Client.Connection.PlayerSelectedHomeShardName = "";
            //Client.Connection.PlayerSelectedHomeShardNameWithParenthesis = "";

            // workaround
            Network.Connection.PlayerSelectedHomeShardName = playerName;
            Network.Connection.PlayerSelectedHomeShardNameWithParenthesis = '(' + playerName + ')';

            //for (uint i = 0; i < CShardNames::getInstance().getSessionNames().size(); i++)
            //{
            //    const CShardNames::TSessionName &sessionName = CShardNames::getInstance().getSessionNames()[i];
            //    if (PlayerSelectedMainland == sessionName.SessionId)
            //    {
            //        PlayerSelectedHomeShardName = sessionName.DisplayName;
            //        PlayerSelectedHomeShardNameWithParenthesis = '(' + PlayerSelectedHomeShardName + ')';
            //    }
            //}

            //	return SELECT_CHARACTER;
            return InterfaceState.GoInTheGame;
        }

        private bool MainLoop()
        {
            // TODO: mainLoopImp
            Log?.Debug("mainLoop");

            // Main loop. If the window is no more Active -> Exit.
            while ( /*!UserEntity->permanentDeath() &&*/ !Network.Connection.GameExit)
            {
                Thread.Sleep(100);

                // If an action handler execute. NB: MUST BE DONE BEFORE ANY THING ELSE PROFILE CRASH!!!!!!!!!!!!!!!!!

                // Test and may run a VBLock profile (only once)

                // Fast mode.

                OnUpdate();

                //////////////////////////
                // INITIALIZE THE FRAME //
                //////////////////////////

                // ...

                ///////////////////////
                // PROCESS THE FRAME //
                ///////////////////////
                // NetWork Update.

                // NetWork Update.
                NetworkManager.Update();
                //IngameDbMngr.flushObserverCalls();
                //NLGUI::CDBManager::getInstance()->flushObserverCalls();
                //bool prevDatabaseInitStatus = IngameDbMngr.initInProgress();
                //IngameDbMngr.setChangesProcessed();
                //bool newDatabaseInitStatus = IngameDbMngr.initInProgress();
                //if ((!newDatabaseInitStatus) && prevDatabaseInitStatus)
                //{
                //    // When database received, activate allegiance buttons (for neutral state) in fame window
                //    CInterfaceManager* pIM = CInterfaceManager::getInstance();
                //    CInterfaceGroup* group = dynamic_cast<CInterfaceGroup*>(CWidgetManager::getInstance()->getElementFromId("ui:interface:fame:content:you"));
                //    if (group)
                //        group->updateAllLinks();
                //    // send a msg to lua for specific ui update
                //    CLuaManager::getInstance().executeLuaScript("game:onInGameDbInitialized()");
                //}

                // Send new data Only when server tick changed.
                if (NetworkConnection.GetCurrentServerTick() > _lastGameCycle)
                {
                    // Put here things you have to send to the server only once per tick like user position.
                    // UPDATE COMPASS

                    // Update the server with our position and orientation.

                    // Give information to the server about the combat position (ability to strike).

                    // Create the message for the server to move the user (except in combat mode).

                    // Send the Packet.
                    NetworkManager.Send(NetworkConnection.GetCurrentServerTick());
                    // Update the Last tick received from the server.
                    _lastGameCycle = NetworkConnection.GetCurrentServerTick();
                }

                // Get the Connection State.
                //lastConnectionState = connectionState;
                //var connectionState = NetworkConnection._ConnectionState;

                ///////////////
                // FAR_TP -> //
                ///////////////
                // Enter a network loop during the FarTP process, without doing the whole real main loop.
                // This code must remain at the very end of the main loop.
                ///////////////
                // <- FAR_TP //
                ///////////////
            } // end of main loop

            // FAR TP STUFF HERE

            // IngameDbMngr.resetInitState();

            //ryzom_exit = true;

            return true;
        }

        /// <summary>
        ///     Disconnect the client from the server (initiated from RCC)
        /// </summary>
        public void Disconnect()
        {
            DispatchBotEvent(bot => bot.OnDisconnect(ChatBot.DisconnectReason.UserLogout, ""));

            botsOnHold.Clear();
            botsOnHold.AddRange(bots);

            try
            {
                _cmdprompt?.Abort();
            }
            catch { }

            if (_timeoutdetector != null)
            {
                try
                {
                    _timeoutdetector.Abort();
                    _timeoutdetector = null;
                }
                catch { }
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
            ChatBot[] selectedBots;

            if (botList != null)
            {
                selectedBots = botList.ToArray();
            }
            else
            {
                selectedBots = bots.ToArray();
            }

            foreach (ChatBot bot in selectedBots)
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
                        System.Diagnostics.StackFrame frame = new System.Diagnostics.StackFrame(1);
                        System.Reflection.MethodBase method = frame.GetMethod();
                        string parentMethodName = method.Name;

                        //Display a meaningful error message to help debugging the ChatBot
                        Log.Error(parentMethodName + ": Got error from " + bot.ToString() + ": " + e.ToString());
                    }
                    else throw; //ThreadAbortException should not be caught here as in can happen when disconnecting from server
                }
            }
        }

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
            else
            {
                TaskWithResult<T> taskWithResult = new TaskWithResult<T>(task);
                lock (_threadTasksLock)
                {
                    _threadTasks.Enqueue(taskWithResult.ExecuteSynchronously);
                }

                return taskWithResult.WaitGetResult();
            }
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
        public bool InvokeRequired
        {
            get
            {
                int callingThreadId = Thread.CurrentThread.ManagedThreadId;

                //if (this != null)
                //{
                //    return this.GetNetReadThreadId() != callingThreadId;
                //}
                //else
                //{
                //    // net read thread (main thread) not yet ready
                //    return false;
                //}

                return false;
            }
        }

        #endregion

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

    }
}