using System;
using System.Collections.Generic;
using System.Diagnostics;
using RCC.NetworkAction;
using RCC.Network;
using System.Reflection;

namespace RCC
{
    partial class NetworkConnection
    {
        private static int _UserAddr;
        private static int _UserKey;
        private static int _UserId;
        private static int _Valid;

        private static int _CurrentSendNumber;
        private static int _LastReceivedNumber;
        private static long _LastReceivedTime;
        private static long _LastReceivedNormalTime;
        private static int _AckBitMask;
        private static int _LastAckBit;
        private static int _Synchronize;
        private static int _InstantPing;
        private static int _BestPing;
        private static int _LCT;
        private static int _LastSentSync;
        private static int _LatestSync;
        private static int _DummySend;
        private static int _LastAckInLongAck;
        private static int _LastSentCycle;
        private static int _TotalReceivedBytes;
        private static int _PartialReceivedBytes;
        private static int _TotalSentBytes;

        private static int _PartialSentBytes;
        private static int _LastReceivedPacketInBothModes;
        private static int _TotalLostPackets;
        private static bool _ConnectionQuality;
        private static int _CurrentSmoothServerTick;
        private static int _SSTLastLocalTime;
        private static string _FrontendAddress;

        public static ConnectionState _ConnectionState
        {
            get => _connectionState;
            set
            {
                ConsoleIO.WriteLine("Connection state changed to " + value.ToString());
                _connectionState = value;
            }
        }

        private static ConnectionState _connectionState = ConnectionState.NotInitialised;

        private static long _LatestLoginTime;
        private static long _LatestSyncTime;
        private static long _LatestProbeTime;
        private static long _UpdateTime;
        private static long _LastSendTime;

        private static int m_LoginAttempts;


        private const int SYSTEM_LOGIN_CODE = 0; // From client
        private const int SYSTEM_SYNC_CODE = 1; // From server
        private const int SYSTEM_ACK_SYNC_CODE = 2; // From client
        private const int SYSTEM_PROBE_CODE = 3; // From server
        private const int SYSTEM_ACK_PROBE_CODE = 4; // From client
        private const int SYSTEM_DISCONNECTION_CODE = 5; // From client
        private const int SYSTEM_STALLED_CODE = 6; // From server
        private const int SYSTEM_SERVER_DOWN_CODE = 7; // From server
        private const int SYSTEM_QUIT_CODE = 8; // From client
        private const int SYSTEM_ACK_QUIT_CODE = 9; // From server

        static UdpSimSock _Connection = new UdpSimSock();

        private static long _UpdateTicks;
        private static bool _ReceivedSync;
        private static int _NormalPacketsReceived;
        private static int _TotalMessages;

        private static int _CurrentClientTick;
        private static int _CurrentServerTick;

        private static byte[] _ReceiveBuffer;
        private static bool _DecodedHeader;

        private static bool _SystemMode;
        private static int _CurrentReceivedNumber;
        private static int _LastReceivedAck;
        private static int _MsPerTick;
        private static long _CurrentClientTime;

        internal static byte[] _LongAckBitField = new byte[1024 / 8];
        private static Action<CBitMemStream, int, object> _ImpulseCallback;
        private static object _ImpulseArg;
        private static object _DataBase;
        private static bool _Registered;
        private static long _MachineTimeAtTick;
        private static long _MachineTicksAtTick;

        public static void reset()
        {
            _CurrentSendNumber = 0;
            _LastReceivedNumber = 0;
            _LastReceivedTime = 0;
            _LastReceivedNormalTime = 0;
            _AckBitMask = 0;
            _LastAckBit = 0;

            _Synchronize = 0;
            _InstantPing = 10000;
            _BestPing = 10000;
            _LCT = 100;
            _MachineTimeAtTick = ryzomGetLocalTime();
            _MachineTicksAtTick = ryzomGetPerformanceTime();

            _LastSentSync = 0;
            _LatestSync = 0;

            _PropertyDecoder.init(256);

            _DummySend = 0;
            //_LongAckBitField.resize(1024);
            _LongAckBitField = new byte[1024 / 8];

            _LastAckInLongAck = 0;
            _LastSentCycle = 0;

            _TotalReceivedBytes = 0;
            _PartialReceivedBytes = 0;
            _TotalSentBytes = 0;
            _PartialSentBytes = 0;
            _MeanPackets.MeanPeriod = 5000;
            _MeanLoss.MeanPeriod = 5000;

            _LastReceivedPacketInBothModes = 0;
            _TotalLostPackets = 0;
            _ConnectionQuality = false;

            _CurrentSmoothServerTick = 0;
            _SSTLastLocalTime = 0;
        }

        public static long ryzomGetPerformanceTime()
        {
            return DateTime.Now.Ticks;
        }

        public static long ryzomGetLocalTime()
        {
            return DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        }

        internal static void connect()
        {

            if (_ConnectionState != ConnectionState.NotConnected)
            {
                throw new Exception(
                    "Unable to connect(): connection not properly initialised (maybe connection not closed).");
            }

            // S12: connect to the FES. Note: In UDP mode, it's the user that have to send the cookie to the front end
            try
            {
                _Connection.connect(_FrontendAddress);
            }
            catch (Exception e)
            {
                throw new Exception("FS refused the connection (" + e.Message + ")");
            }

            _ConnectionState = ConnectionState.Login;

            _LatestLoginTime = ryzomGetLocalTime();
            _LatestSyncTime = _LatestLoginTime;
            _LatestProbeTime = _LatestLoginTime;
            m_LoginAttempts = 0;
        }

        /// <summary>
        /// Init
        /// </summary>
        public static void init(string cookie, string addr)
        {
            if (_ConnectionState != ConnectionState.NotInitialised &&
                _ConnectionState != ConnectionState.Disconnect)
            {
                throw new Exception("Unable to init(): connection not properly closed yet.");
            }

            if (!_Registered)
            {
                //CActionFactory.registerAction(TActionCode.ACTION_POSITION_CODE, CActionPosition);
                //CActionFactory.registerAction(TActionCode.ACTION_SYNC_CODE, CActionSync);
                //CActionFactory.registerAction(TActionCode.ACTION_DISCONNECTION_CODE, CActionDisconnection);
                //CActionFactory.registerAction(TActionCode.ACTION_ASSOCIATION_CODE, CActionAssociation);
                //CActionFactory.registerAction(TActionCode.ACTION_DUMMY_CODE, CActionDummy);
                //CActionFactory.registerAction(TActionCode.ACTION_LOGIN_CODE, CActionLogin);
                //CActionFactory.registerAction(TActionCode.ACTION_TARGET_SLOT_CODE, CActionTargetSlot);
                CActionFactory.registerAction(TActionCode.ACTION_GENERIC_CODE, typeof(CActionGeneric));
                CActionFactory.registerAction(TActionCode.ACTION_GENERIC_MULTI_PART_CODE, typeof(CActionGenericMultiPart));
                //CActionFactory.registerAction(TActionCode.ACTION_SINT64, CActionSint64);
                _Registered = true;
            }

            initCookie(cookie, addr);

            // Register property nbbits
            // CActionSint64::registerNumericPropertiesRyzom();

            // Init visual property tree
            //_VisualPropertyTreeRoot = new TVPNodeClient();
            //_VisualPropertyTreeRoot->buildTree();

            // If the server run on window, those are the one to test
            //_AltMsgXmlMD5 = NLMISC::getMD5("msg.xml");
            //_AltDatabaseXmlMD5 = NLMISC::getMD5("database.xml");
        }

        /// <summary>
        /// Sets the cookie and front-end address, resets the connection state.
        /// </summary>
        public static void initCookie(string cookie, string addr)
        {
            _FrontendAddress = addr;

            SetLoginCookieFromString(cookie);

            ConsoleIO.WriteLine(
                $"Network initialisation with front end '{_FrontendAddress}' and cookie {cookie}");

            _ConnectionState = ConnectionState.NotConnected;
        }

        public static void setImpulseCallback(Action<CBitMemStream, int, object> impulseCallBack)
        {
            _ImpulseCallback = impulseCallBack;
            _ImpulseArg = null;
        }

        public static void setDataBase(object database)
        {
            _DataBase = database;
        }

        private static bool stateLogin()
        {
            // if receives System SYNC
            //    immediate state Synchronize
            // else
            //    sends System LoginCookie


            // CASES AND GET DATA

            while (_Connection.dataAvailable())
            {
                _DecodedHeader = false;
                var msgin = new CBitMemStream(true);

                if (buildStream(msgin) && decodeHeader(msgin))
                {
                    if (_SystemMode)
                    {
                        byte message = 0;
                        msgin.serial(ref message);

                        switch (message)
                        {
                            case SYSTEM_SYNC_CODE:
                                // receive sync, decode sync
                                _ConnectionState = ConnectionState.Synchronize;
                                ConsoleIO.WriteLine("CNET: login->synchronize");
                                receiveSystemSync(msgin);
                                return true;

                            case SYSTEM_STALLED_CODE:
                                // receive stalled, decode stalled and state stalled
                                _ConnectionState = ConnectionState.Stalled;
                                ConsoleIO.WriteLine("CNET: login->stalled");
                                receiveSystemStalled(msgin);
                                return true;

                            case SYSTEM_PROBE_CODE:
                                // receive probe, decode probe and state probe
                                _ConnectionState = ConnectionState.Probe;
                                //_Changes.push_back(CChange(0, ProbeReceived));
                                ConsoleIO.WriteLine("CNET: login->probe");
                                receiveSystemProbe(msgin);
                                return true;

                            case SYSTEM_SERVER_DOWN_CODE:
                                disconnect(); // will send disconnection message
                                ConsoleIO.WriteLine("BACK-END DOWN");
                                return false; // exit now from loop, don't expect a new state

                            default:
                                //msgin.displayStream("DBG:BEN:stateLogin:msgin");
                                ConsoleIO.WriteLine("CNET: received system " + message + " in state Login");
                                break;
                        }
                    }
                    else
                    {
                        //msgin.displayStream("DBG:BEN:stateLogin:msgin");
                        ConsoleIO.WriteLine("CNET: received normal in state Login");
                    }
                }
            }


            // send ack sync if received sync or last sync timed out
            if (_UpdateTime - _LatestLoginTime > 300)
            {
                sendSystemLogin();
                _LatestLoginTime = _UpdateTime;
                if (m_LoginAttempts > 24)
                {
                    m_LoginAttempts = 0;
                    disconnect(); // will send disconnection message
                    ConsoleIO.WriteLine("CNET: Too many LOGIN attempts, connection problem");
                    // exit now from loop, don't expect a new state
                }
                else
                {
                    ++m_LoginAttempts;
                }
            }

            return false;
        }

        private static bool stateSynchronize()
        {
            // if receives System PROBE
            //    immediate state Probe
            // else if receives Normal
            //    immediate state Connected
            // sends System ACK_SYNC

            while (_Connection.dataAvailable()) // && _TotalMessages<5)

            {
                _DecodedHeader = false;
                var msgin = new CBitMemStream(true);

                if (buildStream(msgin) && decodeHeader(msgin))
                {
                    if (_SystemMode)
                    {
                        byte message = 0;
                        msgin.serial(ref message);

                        switch (message)
                        {
                            case SYSTEM_PROBE_CODE:
                                // receive probe, decode probe and state probe
                                _ConnectionState = ConnectionState.Probe;
                                //nldebug("CNET[%p]: synchronize->probe", this);
                                //_Changes.push_back(CChange(0, ProbeReceived)); TODO
                                receiveSystemProbe(msgin);
                                return true;

                            case SYSTEM_STALLED_CODE:
                                // receive stalled, decode stalled and state stalled
                                _ConnectionState = ConnectionState.Stalled;
                                //nldebug("CNET[%p]: synchronize->stalled", this);
                                receiveSystemStalled(msgin);
                                return true;

                            case SYSTEM_SYNC_CODE:
                                // receive sync, decode sync
                                receiveSystemSync(msgin);
                                break;

                            case SYSTEM_SERVER_DOWN_CODE:
                                disconnect(); // will send disconnection message
                                ConsoleIO.WriteLine("BACK-END DOWN");
                                return false; // exit now from loop, don't expect a new state

                            default:
                                ConsoleIO.WriteLine("CNET: received system " + message + " in state Synchronize");
                                break;
                        }
                    }
                    else
                    {
                        _ConnectionState = ConnectionState.Connected;
                        //nlwarning("CNET[%p]: synchronize->connected", this);
                        //_Changes.push_back(CChange(0, ConnectionReady)); TODO
                        //_ImpulseDecoder.reset();
                        receiveNormalMessage(msgin);
                        return true;
                    }
                }
            }

            // send ack sync if received sync or last sync timed out
            if (_UpdateTime - _LatestSyncTime > 300)
                sendSystemAckSync();

            return false;
        }

        private static void receiveNormalMessage(CBitMemStream msgin)
        {
            ConsoleIO.WriteLine("CNET: received normal message Packet=" + _LastReceivedNumber + " Ack=" + _LastReceivedAck);

            var actions = new List<CAction>();
            CImpulseDecoder.decode(msgin, _CurrentReceivedNumber, _LastReceivedAck, _CurrentSendNumber, actions);

            ++_NormalPacketsReceived;

            // we can now remove all old action that are acked
            //while (!_Actions.empty() && _Actions.front().FirstPacket != 0 && _Actions.front().FirstPacket <= _LastReceivedAck)
            //{
            //    // warning, CActionBlock automatically remove() actions when deleted
            //    _Actions.pop_front();
            //}

            _CurrentServerTick = _CurrentReceivedNumber * 2 + _Synchronize;

            // TODO: PacketStamps stuff

            // Decode the actions received in the impulsions
            for (int i = 0; i < actions.Count; i++)
            {
                switch (actions[i].Code)
                {
                    case TActionCode.ACTION_DISCONNECTION_CODE:
                        {
                            // Self disconnection
                            ConsoleIO.WriteLine("You were disconnected by the server");
                            disconnect(); // will send disconnection message
                            // TODO LoginSM.pushEvent(CLoginStateMachine::ev_conn_dropped);
                        }
                        break;
                    case TActionCode.ACTION_GENERIC_CODE:
                        {
                            genericAction((CActionGeneric)actions[i]);
                        }
                        break;
                    case TActionCode.ACTION_GENERIC_MULTI_PART_CODE:
                        {
                            //genericAction((CActionGenericMultiPart)actions[i]); // TODO: why this cast?
                        }
                        break;
                    case TActionCode.ACTION_DUMMY_CODE:
                        {
                            //CActionDummy* dummy = ((CActionDummy*)actions[i]);
                            ConsoleIO.WriteLine("CNET Received Dummy" + actions[i]);
                            // Nothing to do
                        }
                        break;
                }

                CActionFactory.remove(actions[i]);
            }

            // Decode the visual properties
            // TODO decodeVisualProperties(msgin);

            _LastReceivedNormalTime = _UpdateTime;
        }

        static void genericAction(CActionGeneric ag)
        {
            // manage a generic action
            // TODO: get memory stream -> CImpulseDecoder.decode to action?
            CBitMemStream bms = ag.get();

            //nldebug("CNET: Calling impulsion callback (size %u) :'%s'", this, bms.length(), toHexaString(bms.bufferAsVector()).c_str());
            //nldebug("CNET[%p]: Calling impulsion callback (size %u)", this, bms.length());

            if (_ImpulseCallback != null)
                _ImpulseCallback.Invoke(bms, _LastReceivedNumber, _ImpulseArg);
        }

        private static bool stateConnected()
        {
            // if receives System PROBE
            //    immediate state Probe
            // else if receives Normal
            //	   sends Normal data


            // Prevent to increment the client time when the front-end does not respond
            long previousTime = ryzomGetLocalTime();
            long now = ryzomGetLocalTime();
            long diff = now - previousTime;
            previousTime = now;
            if ((diff > 3000) && (!_Connection.dataAvailable()))
            {
                return false;
            }

            // update the current time;
            while (_CurrentClientTime < (long)(_UpdateTime - _MsPerTick - _LCT) && _CurrentClientTick < _CurrentServerTick)
            {
                _CurrentClientTime += _MsPerTick;

                _CurrentClientTick++;

                _MachineTimeAtTick = _UpdateTime;
                _MachineTicksAtTick = _UpdateTicks;
            }

            if (_CurrentClientTick >= _CurrentServerTick && !_Connection.dataAvailable())
            {
                return false;
            }


            while (_Connection.dataAvailable())// && _TotalMessages<5)
            {
                _DecodedHeader = false;
                CBitMemStream msgin = new CBitMemStream(true);

                if (buildStream(msgin) && decodeHeader(msgin))
                {
                    if (_SystemMode)
                    {
                        byte message = 0;
                        msgin.serial(ref message);

                        switch (message)
                        {
                            case SYSTEM_PROBE_CODE:
                                // receive probe, and goto state probe
                                _ConnectionState = ConnectionState.Probe;
                                // reset client impulse & vars
                                /*
                                                    _ImpulseDecoder.reset();
                                                    _PropertyDecoder.clear();
                                                    _PacketStamps.clear();
                                                    // clears sent actions
                                                    while (!_Actions.empty())
                                                        CActionFactory::getInstance()->remove(_Actions.front().Actions),
                                                    _Actions.clear();
                                                    _AckBitMask = 0;
                                                    _LastReceivedNumber = 0xffffffff;
                                */
                                //nldebug("CNET[%p]: connected->probe", this);
                                //_Changes.push_back(CChange(0, ProbeReceived)); TODO: Changes
                                receiveSystemProbe(msgin);
                                return true;

                            case SYSTEM_SYNC_CODE:
                                // receive stalled, decode stalled and state stalled
                                _ConnectionState = ConnectionState.Synchronize;
                                //nldebug("CNET[%p]: connected->synchronize", this);
                                receiveSystemSync(msgin);
                                return true;

                            case SYSTEM_STALLED_CODE:
                                // receive stalled, decode stalled and state stalled
                                _ConnectionState = ConnectionState.Stalled;
                                //nldebug("CNET[%p]: connected->stalled", this);
                                receiveSystemStalled(msgin);
                                return true;

                            case SYSTEM_SERVER_DOWN_CODE:
                                disconnect(); // will send disconnection message
                                ConsoleIO.WriteLine("BACK-END DOWN");
                                return false; // exit now from loop, don't expect a new state

                            default:
                                ConsoleIO.WriteLine("CNET: received system " + message + " in state Connected");
                                break;
                        }
                    }
                    else
                    {
                        receiveNormalMessage(msgin);
                    }

                }
            }

            return false;
        }

        private static bool stateProbe()
        {
            ConsoleIO.WriteLine(MethodBase.GetCurrentMethod().Name + " called, but not implemented");
            return false;
        }

        private static bool stateStalled()
        {
            ConsoleIO.WriteLine(MethodBase.GetCurrentMethod().Name + " called, but not implemented");
            return false;
        }

        private static bool stateQuit()
        {
            ConsoleIO.WriteLine(MethodBase.GetCurrentMethod().Name + " called, but not implemented");
            return false;
        }

        private static void receiveSystemProbe(CBitMemStream msgin)
        {
            ConsoleIO.WriteLine(MethodBase.GetCurrentMethod().Name + " called, but not implemented");
            // TODO receiveSystemProbe
        }

        private static void receiveSystemStalled(CBitMemStream msgin)
        {
            ConsoleIO.WriteLine(MethodBase.GetCurrentMethod().Name + " called, but not implemented");
            // TODO receiveSystemStalled
        }

        private static void receiveSystemSync(CBitMemStream msgin)
        {
            _LatestSyncTime = _UpdateTime;
            long stime = 0;
            msgin.serial(ref _Synchronize);
            msgin.serial(ref stime);
            msgin.serial(ref _LatestSync);

            Debug.Print("receiveSystemSync " + msgin);

            _ReceivedSync = true;

            _MsPerTick = 100;           // initial values

            //#pragma message ("HALF_FREQUENCY_SENDING_TO_CLIENT")
            _CurrentServerTick = _Synchronize + _CurrentReceivedNumber * 2;

            _CurrentClientTick = (int)(_CurrentServerTick - (_LCT + _MsPerTick) / _MsPerTick);
            _CurrentClientTime = _UpdateTime - (_LCT + _MsPerTick);

            //nlinfo( "CNET[%p]: received SYNC %" NL_I64 "u %" NL_I64 "u - _CurrentReceivedNumber=%d _CurrentServerTick=%d", this, (uint64)_Synchronize, (uint64)stime, _CurrentReceivedNumber, _CurrentServerTick );

            sendSystemAckSync();
        }

        /// <summary>
        /// Receive available data and convert it to a bitmemstream
        /// </summary>
        private static bool buildStream(CBitMemStream msgin)
        {
            const int len = 65536;

            if (_Connection.receive(ref _ReceiveBuffer, len, false))
            {
                // Fill the message
                //msgin.clear();
                msgin.memcpy(_ReceiveBuffer);

                return true;
            }
            else
            {
                // A receiving error means the front-end is down
                _ConnectionState = ConnectionState.Disconnect;
                disconnect(); // won't send a disconnection msg because state is already Disconnect
                ConsoleIO.WriteLine("DISCONNECTION");
                return false;
            }
        }


        private static bool decodeHeader(CBitMemStream msgin)
        {
            if (_DecodedHeader)
                return true;

            ++_TotalMessages;

            _LastReceivedTime = _UpdateTime;

            msgin.serial(ref _CurrentReceivedNumber);
            msgin.serial(ref _SystemMode);

            if (_SystemMode)
            {
            }
            else
            {
                msgin.serial(ref _LastReceivedAck);
            }

            Debug.Print("decodeHeader " + msgin);

            _LastReceivedNumber = _CurrentReceivedNumber;
            _DecodedHeader = true;
            return true;
        }


        private static void disconnect()
        {
            if (_ConnectionState == ConnectionState.NotInitialised ||
                _ConnectionState == ConnectionState.NotConnected ||
                _ConnectionState == ConnectionState.Authenticate ||
                _ConnectionState == ConnectionState.Disconnect)
            {
                // ConsoleIO.WriteLine("Unable to disconnect(): not connected yet, or already disconnected.");
                return;
            }

            sendSystemDisconnection();
            _Connection.close();
            _ConnectionState = ConnectionState.Disconnect;
        }

        private static void sendSystemAckSync()
        {
            CBitMemStream message = new CBitMemStream();

            message.BuildSystemHeader(ref _CurrentSendNumber);

            byte sync = SYSTEM_ACK_SYNC_CODE;
            message.serial(ref sync);
            message.serial(ref _LastReceivedNumber);
            message.serial(ref _LastAckInLongAck);
            // message.serial(_LongAckBitField); todo
            for (var index = 0; index < _LongAckBitField.Length; index++)
            {
                var b = _LongAckBitField[index];
                message.serial(ref b);
            }
            message.serial(ref _LatestSync);

            Debug.WriteLine("sendSystemAckSync " + message);

            var length = message.Length;
            _Connection.send(message.Buffer(), length);
            //sendUDP (&(_Connection), message.buffer(), length);
            //statsSend(length); todo stats

            _LatestSyncTime = _UpdateTime;
        }

        private static void sendSystemDisconnection()
        {
            var message = new CBitMemStream();

            message.BuildSystemHeader(ref _CurrentSendNumber);

            byte disconnection = SYSTEM_DISCONNECTION_CODE;

            message.serial(ref disconnection);

            int length = message.Length;

            if (_Connection.connected())
            {
                try
                {
                    Debug.WriteLine("sendSystemDisconnection " + message);
                    _Connection.send(message.Buffer(), length);
                }
                catch (Exception e)
                {
                    ConsoleIO.WriteLine("Socket exception: " + e.Message);
                }
            }

            //sendUDP (&(_Connection), message.buffer(), length);
            //statsSend(length); TODO stats

            //updateBufferizedPackets(); TODO updateBufferizedPackets
            //nlinfo("CNET[%p]: sent DISCONNECTION", this);
        }

        /// <summary>
        /// sends system login cookie
        /// </summary>
        public static void sendSystemLogin()
        {
            var message = new CBitMemStream();

            message.BuildSystemHeader(ref _CurrentSendNumber);

            byte login = SYSTEM_LOGIN_CODE;
            message.serial(ref login);

            //message.serial(Cookie);
            message.serial(ref _UserAddr);
            message.serial(ref _UserKey);
            message.serial(ref _UserId);

            // todo: find out why 2 xD - string terminator?
            int unf = 2;
            message.serial(ref unf);

            message.serial(ref ClientCfg.LanguageCode);

            Debug.WriteLine("sendSystemLogin " + message);
            _Connection.send(message.Buffer(), message.Length);
        }

        /// <summary>
        /// Sets the cookie
        /// </summary>
        /// <param name="str"></param>
        private static void SetLoginCookieFromString(string str)
        {
            var parts = str.Split('|');

            _UserAddr = int.Parse(parts[0], System.Globalization.NumberStyles.HexNumber);
            _UserKey = int.Parse(parts[1], System.Globalization.NumberStyles.HexNumber);
            _UserId = int.Parse(parts[2], System.Globalization.NumberStyles.HexNumber);

            if (_UserAddr == 0 && _UserKey == 0 && _UserId == 0)
                _Valid = 0;
            else
                _Valid = 1;
        }

        public static bool update()
        {
            _UpdateTime = ryzomGetLocalTime();
            _UpdateTicks = ryzomGetPerformanceTime();
            _ReceivedSync = false;
            _NormalPacketsReceived = 0;
            _TotalMessages = 0;

            // If we are disconnected, bypass the real network update
            if (_ConnectionState == ConnectionState.Disconnect)
            {
                _ConnectionQuality = false; // to block the user entity
                return false;
            }

            // Yoyo. Update the Smooth ServerTick.
            updateSmoothServerTick();

            if (!_Connection.connected())
            {
                //if(!ClientCfg.Local)
                //	nlwarning("CNET[%p]: update() attempted whereas socket is not connected !", this);
                return false;
            }

            try
            {
                // State automaton
                bool stateBroke = false;
                do
                {
                    switch (_ConnectionState)
                    {
                        case ConnectionState.Login:
                            // if receives System SYNC
                            //    immediate state Synchronize
                            // else
                            //    sends System LoginCookie
                            stateBroke = stateLogin();
                            break;

                        case ConnectionState.Synchronize:
                            // if receives System PROBE
                            //    immediate state Probe
                            // else if receives Normal
                            //    immediate state Connected
                            // else
                            //    sends System ACK_SYNC
                            stateBroke = stateSynchronize();
                            break;

                        case ConnectionState.Connected:
                            // if receives System PROBE
                            //    immediate state Probe
                            // else if receives Normal
                            //	   sends Normal data
                            stateBroke = stateConnected();
                            break;

                        case ConnectionState.Probe:
                            // if receives System SYNC
                            //    immediate state SYNC
                            // else if receives System PROBE
                            //    decode PROBE
                            // sends System ACK_PROBE
                            stateBroke = stateProbe();
                            break;

                        case ConnectionState.Stalled:
                            // if receives System SYNC
                            //    immediate state SYNC
                            // else if receives System STALLED
                            //    decode STALLED (nothing to do)
                            // else if receives System PROBE
                            //    immediate state PROBE
                            stateBroke = stateStalled();
                            break;

                        case ConnectionState.Quit:
                            // if receives System SYNC
                            //    immediate state Synchronize
                            // else
                            //    sends System LoginCookie
                            stateBroke = stateQuit();
                            break;

                        default:
                            // Nothing here !
                            stateBroke =
                                false; // will come here if a disconnection action is received inside a method that returns true
                            break;
                    }
                } while (stateBroke); // && _TotalMessages<5);
            }
            catch (Exception)
            {
                _ConnectionState = ConnectionState.Disconnect;
            }

            //updateBufferizedPackets (); - unused in Ryzom code

            //PacketLossGraph.addOneValue(getMeanPacketLoss());

            _ConnectionQuality = (_ConnectionState == ConnectionState.Connected &&
                                  _UpdateTime - _LastReceivedNormalTime < 2000 &&
                                  _CurrentClientTick < _CurrentServerTick);

            return _TotalMessages != 0;
        }

        public static int getCurrentServerTick()
        {
            return _CurrentServerTick;
        }

        public static void send(in int cycle)
        {
            try
            {
                _LastSentCycle = cycle;

                // TODO: Implementation of CActionBlock code

                if (_ConnectionState == ConnectionState.Connected)
                {
                    sendNormalMessage();
                }
            }
            catch (Exception)
            {
                _ConnectionState = ConnectionState.Disconnect;
                disconnect();
            }
        }

        private static void sendNormalMessage()
        {
            //
            // Create the message to send to the server
            //

            var message = new CBitMemStream();

            var systemMode = false;

            message.serial(ref _CurrentSendNumber);
            message.serial(ref systemMode);
            message.serial(ref _LastReceivedNumber);
            message.serial(ref _AckBitMask);

            // TODO: Implementation of CActionBlock code

            //Debug.Print("sendNormalMessage " + message);
            _Connection.send(message.Buffer(), message.Length);
        }

        internal static void send()
        {
            try
            {
                // Send is temporised, that is the packet may not be actually sent.
                // We don't care, since:
                // - this packet has no new data (not ticked send)
                // - a next send() will send packet if time elapsed enough
                // - a next send(tick) will really be sent
                // This way, we can say that at most 15 packets will be delivered each second
                // (5 send(tick), and 10 send() -- if you take getLocalTime() inaccuracy into account)
                if (_ConnectionState == ConnectionState.Connected && ryzomGetLocalTime() - _LastSendTime > 100)
                {
                    sendNormalMessage();
                }
            }
            catch (Exception)
            {
                _ConnectionState = ConnectionState.Disconnect;
            }
        }

        private static void updateSmoothServerTick()
        {
            // TODO updateSmoothServerTick
        }

        internal class _MeanLoss
        {
            public static int MeanPeriod { get; set; }
        }

        internal class _MeanPackets
        {
            public static int MeanPeriod { get; set; }
        }

        internal static class _PropertyDecoder
        {
            public static void init(int i)
            {
            }
        }

        /// <summary>
        /// Clear not acknownledged actions in sending buffer
        /// </summary>
        public static void flushSendBuffer()
        {
            //_Actions.clear();
        }
    }
}