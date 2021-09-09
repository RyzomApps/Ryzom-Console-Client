using RCC.Logger;
using System;
using System.Threading;

namespace RCC
{
    /// <summary>
    /// The main client class, used to connect to a Ryzom server.
    /// </summary>
    public class RyzomClient
    {
        public ILogger Log;

        public string Cookie { get; set; }

        public string FsAddr { get; set; }

        public string RingMainURL { get; set; }

        public string FarTpUrlBase { get; set; }

        public bool StartStat { get; set; }

        public string R2ServerVersion { get; set; }

        public string R2BackupPatchURL { get; set; }

        public string[] R2PatchUrLs { get; set; }

        bool WaitServerAnswer;

        bool game_exit = false;

        bool userChar;
        bool noUserChar;
        bool ConnectInterf;
        bool CreateInterf;
        bool CharacterInterf;

        bool CharNameValidArrived;

        /// <summary>
        /// Login to the server using the RyzomCom class.
        /// </summary>
        /// TODO: Implementation
        // Initializing XML Database
        // IngameDbMngr.init(CPath::lookup("database.xml"), ProgressBar);
        public void Connect()
        {
            /////////////////////////////////
            // Initialize the application. //

            // prelogInit();

            login();

            postlogInit();

            //////////////////////////////////////////
            // The real main loop

            bool ok = true;

            while (ok)
            {
                //////////////////////////////////////////
                // Manage the connection to the server. //

                // If the connection return false we just want to quit the game
                if (!connection(Cookie, FsAddr))
                {
                    //releaseOutGame();
                    break;
                }

                ///////////////////////////////
                // Initialize the main loop. //

                //initMainLoop();

                //////////////////////////////////////////////////
                // Main loop (biggest part of the application). //

                ok = !mainLoop();

                /////////////////////////////
                // Release all the memory. //
                //if (!FarTP.isReselectingChar())
                //{
                //    releaseMainLoop(!ok);
                //}
            }

            // Final release
            // release();
        }

        private void postlogInit()
        {
            NetworkManager.initializeNetwork();
        }

        /// <summary>
        /// New version of the menu after the server connection
        ///
        /// If you add something in this function, check CFarTP,
        /// some kind of reinitialization might be useful over there.
        /// </summary>
        private bool connection(string cookie, string fsaddr)
        {
            game_exit = false;

            // Init global variables
            userChar = false;
            noUserChar = false;
            ConnectInterf = true;
            CreateInterf = true;
            CharacterInterf = true;
            WaitServerAnswer = false;

            //FarTP.setOutgame();

            bool firstConnection = true;
            TInterfaceState InterfaceState = TInterfaceState.AUTO_LOGIN;

            while ((InterfaceState != TInterfaceState.GOGOGO_IN_THE_GAME) && (InterfaceState != TInterfaceState.QUIT_THE_GAME))
            {
                switch (InterfaceState)
                {
                    case TInterfaceState.AUTO_LOGIN:
                        InterfaceState = autoLogin(cookie, fsaddr, firstConnection);
                        break;

                    case TInterfaceState.GLOBAL_MENU:

                        //if (ClientCfg.SelectCharacter == -1)
                        //{
                        //    NLGUI::CDBManager::getInstance()->getDbProp("UI:CURRENT_SCREEN")->setValue32(0); // 0 == select
                        //}

                        // Interface to choose a char
                        InterfaceState = globalMenu();
                        break;

                    case TInterfaceState.GOGOGO_IN_THE_GAME:
                        break;

                    case TInterfaceState.QUIT_THE_GAME:
                        break;
                }
            }

            // GOGOGO_IN_THE_GAME
            return (InterfaceState == TInterfaceState.GOGOGO_IN_THE_GAME);
        }

        private TInterfaceState autoLogin(string cookie, string fsaddr, in bool firstConnection)
        {
            if (firstConnection)
                NetworkConnection.init(cookie, fsaddr);

            if (firstConnection)
            {
                try
                {
                    NetworkConnection.connect();
                }
                catch (Exception e)
                {
                    ConsoleIO.WriteLine("connection : " + e.Message + ".");
                    return TInterfaceState.QUIT_THE_GAME;
                }

                // Ok the client is connected

                // TODO callback and database set impl
                // Set the impulse callback.
                //NetworkConnection.setImpulseCallback(impulseCallBack);

                // Set the database.
                //NetworkConnection.setDataBase(IngameDbMngr.getNodePtr());

                // init the string manager cache.
                //STRING_MANAGER::CStringManagerClient::instance()->initCache(UsedFSAddr, ClientCfg.LanguageCode);
            }

            WaitServerAnswer = true;

            return TInterfaceState.GLOBAL_MENU;
        }

        private bool mainLoop()
        {
            // TODO: mainLoopImp
            ConsoleIO.WriteLine("mainLoop");
            return true;
        }

        private bool login()
        {
            // run the main loop

            // start the login state machine
            // ev_init_done -> st_auto_login
            // username and password set

            // onlogin -> main menu page for r2mode -> ev_login_ok
            // string res = checkLogin(LoginLogin, LoginPassword, ClientApp, LoginCustomParameters);
            Login.checkLogin(this, "betaem1", "mozyr", "ryzom_live", "");

            // ev_login_ok -> ... -> st_connect
            ConnectToShard();

            //st_reconnect_fs
            //ConnectToNewShard();

            // -> st_ingame

            return true;
        }

        private void ConnectToShard()
        {
            ConsoleIO.WriteLine("ConnectToShard");
        }

        /// <summary>
        /// Launch the interface to choose a character
        /// </summary>
        public TInterfaceState globalMenu()
        {
            int serverTick = NetworkConnection.getCurrentServerTick();
            bool PlayerWantToGoInGame = false;
            bool firewallTimeout = false;

            while (PlayerWantToGoInGame == false)
            {
                try
                {
                    if (!firewallTimeout)
                        NetworkManager.update();
                }
                catch
                {
                    if (NetworkConnection._ConnectionState == ConnectionState.Disconnect)
                    {
                        firewallTimeout = true;
                    }
                    else
                    {
                        Thread.Sleep(30);
                    }
                }

                // IngameDbMngr.flushObserverCalls();
                // NLGUI::CDBManager::getInstance()->flushObserverCalls();

                // check if we can send another dated block
                if (NetworkConnection.getCurrentServerTick() != serverTick)
                {
                    //
                    serverTick = NetworkConnection.getCurrentServerTick();
                    NetworkConnection.send(serverTick);
                }
                else
                {
                    // Send dummy info
                    NetworkConnection.send();
                }


                // SERVER INTERACTIONS WITH INTERFACE
                if (WaitServerAnswer)
                {
                    if (noUserChar || userChar)
                    {
                        noUserChar = userChar = false;

                        // Clear sending buffer that may contain prevous QUIT_GAME when getting back to the char selection screen
                        NetworkConnection.flushSendBuffer();
                    }

                    if (CharNameValidArrived)
                    {
                        CharNameValidArrived = false;
                        WaitServerAnswer = false;

                    }

                    if (NetworkManager.serverReceivedReady)
                    {
                        //nlinfo("impulseCallBack : received serverReceivedReady");
                        NetworkManager.serverReceivedReady = false;
                        WaitServerAnswer = false;
                        PlayerWantToGoInGame = true;
                    }
                }
                else
                {
                    noUserChar = false;
                    userChar = false;
                    CharNameValidArrived = false;
                    NetworkManager.serverReceivedReady = false;
                }

                if (NetworkConnection._ConnectionState == ConnectionState.Disconnect)
                {
                    // Display the connection failure screen
                    // TODO Display the connection failure screen

                    if (firewallTimeout)
                    {
                        // Display the firewall error string instead of the normal failure string

                        ConsoleIO.WriteLine("uiFirewallFail");
                    }
                }

                if (game_exit)
                    return TInterfaceState.QUIT_THE_GAME;
            }

            //if (ClientCfg.SelectCharacter != -1)
            //    PlayerSelectedSlot = ClientCfg.SelectCharacter;

            // -> ev_global_menu_exited

            //  Init the current Player Name (for interface.cfg and sentence.name save). Make a good File Name.

            //	return SELECT_CHARACTER;
            return TInterfaceState.GOGOGO_IN_THE_GAME;

        }

        // TODO impulseServerReady

        private void ConnectToNewShard()
        {
            // Connect to the next FS
            NetworkConnection.initCookie(Cookie, FsAddr);

            //// connect the session browser to the new shard
            //NLNET::CInetAddress sbsAddress(CSessionBrowserImpl::getInstance().getFrontEndAddress());
            //sbsAddress.setPort(sbsAddress.port() + SBSPortOffset);
            //CSessionBrowserImpl::getInstance().connectItf(sbsAddress);

            try
            {
                NetworkConnection.connect();
            }
            catch
            {
                // TODO: Connection refused or other error - Retry?
                //_Reason = new string(result);
                //LoginSM.pushEvent(CLoginStateMachine::ev_conn_failed);
            }

            // Reinit the string manager cache.
            //STRING_MANAGER::CStringManagerClient::instance()->initCache(FSAddr, ClientCfg.LanguageCode);

            // reset the chat mode
            //ChatMngr.resetChatMode();

            // The next step will be triggered by the  msg from the server
        }

        /// <summary>
        /// Disconnect the client from the server (initiated from MCC)
        /// </summary>
        public void Disconnect()
        {

        }
    }
}
