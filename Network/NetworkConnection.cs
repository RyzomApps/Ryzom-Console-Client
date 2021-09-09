using System;

namespace RCC
{
    class NetworkConnection
    {
        private static int _UserAddr;
        private static int _UserKey;
        private static int _UserId;
        private static int _Valid;

        private static int _CurrentSendNumber;
        private static int _LastReceivedNumber;
        private static long _LastReceivedTime;
        private static int _LastReceivedNormalTime;
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

        public static ConnectionState _ConnectionState = ConnectionState.NotInitialised;

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

        private static object _UpdateTicks;
        private static bool _ReceivedSync;
        private static int _NormalPacketsReceived;
        private static int _TotalMessages;

        private static int _CurrentClientTick;
        private static int _CurrentServerTick;

        private static byte[] _ReceiveBuffer;
        private static bool _DecodedHeader;

        private static bool _SystemMode;
        private static int _CurrentReceivedNumber;

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
            object _MachineTimeAtTick = ryzomGetLocalTime();
            object _MachineTicksAtTick = ryzomGetPerformanceTime();

            _LastSentSync = 0;
            _LatestSync = 0;

            _PropertyDecoder.init(256);

            _DummySend = 0;
            _LongAckBitField.resize(1024);
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

            ConsoleIO.WriteLineFormatted(
                $"Network initialisation with front end '{_FrontendAddress}' and cookie {cookie}");

            _ConnectionState = ConnectionState.NotConnected;
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
                    
                       }

                    ConsoleIO.WriteLine(msgin.ToString());
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
                    throw new Exception("CNET: Too many LOGIN attempts, connection problem");
                    // exit now from loop, don't expect a new state
                }
                else
                {
                    ++m_LoginAttempts;
                }
            }

            return false;
        }

        /// <summary>
        /// Receive available data and convert it to a bitmemstream
        /// </summary>
        private static bool buildStream(CBitMemStream msgin)
        {
            var len = 65536;

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

            msgin.Serial(ref _CurrentReceivedNumber);
            msgin.Serial(ref _SystemMode);

            return true;
        }


        private static void disconnect()
        {
            _Connection.disconnect();
        }

        /// <summary>
        /// sends system login cookie
        /// </summary>
        public static void sendSystemLogin()
        {
            var message = new CBitMemStream();

            message.BuildSystemHeader(ref _CurrentSendNumber);

            byte login = SYSTEM_LOGIN_CODE;
            message.Serial(ref login);

            //message.serial(Cookie);
            message.Serial(ref _UserAddr);
            message.Serial(ref _UserKey);
            message.Serial(ref _UserId);

            // todo: find out why 2 xD
            int unf = 2;
            message.Serial(ref unf);

            message.Serial(ref ClientCfg.LanguageCode);

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

            return (_TotalMessages != 0);
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

            message.Serial(ref _CurrentSendNumber);
            message.Serial(ref systemMode);
            message.Serial(ref _LastReceivedNumber);
            message.Serial(ref _AckBitMask);

            // TODO: Implementation of CActionBlock code

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

        private static bool stateSynchronize()
        {
            return false;
        }

        private static bool stateConnected()
        {
            return false;
        }

        private static bool stateProbe()
        {
            return false;
        }

        private static bool stateStalled()
        {
            return false;
        }

        private static bool stateQuit()
        {
            return false;
        }

        private static void updateSmoothServerTick()
        {

        }

        internal class _MeanLoss
        {
            public static int MeanPeriod { get; set; }
        }

        internal class _MeanPackets
        {
            public static int MeanPeriod { get; set; }
        }

        internal static class _LongAckBitField
        {
            public static void resize(int i)
            {
            }
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