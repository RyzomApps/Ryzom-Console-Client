///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using RCC.Config;
using RCC.Helper;
using RCC.Network.Action;

namespace RCC.Network
{
    /// <summary>
    ///     handles incoming and outgoing messages from the game server and keeps track of the current state of the connection
    /// </summary>
    public class NetworkConnection
    {
        public uint LastSentCycle;
        public int LastReceivedNumber;
        public Action<BitMemoryStream> ImpulseCallback;
        public object ImpulseArg;

        internal byte[] LongAckBitField = new byte[1024 / 8];

        private const byte InvalidSlot = 0xFF;

        private readonly UdpSocket _connection = new UdpSocket();

        private ConnectionState _connectionState = ConnectionState.NotInitialised;

        private int _userAddr;
        private int _userKey;
        private int _userId;

        private int _currentSendNumber;
        private int _ackBitMask;
        private uint _synchronize;
        private int _lct;
        private int _latestSync;
        private int _lastAckInLongAck;

        private string _frontendAddress;

        private long _latestLoginTime;
        private long _latestSyncTime;
        private long _latestProbeTime;
        private long _updateTime;
        private long _lastSendTime;

        private int _mLoginAttempts;

        private int _totalMessages;

        private uint _currentClientTick;
        private uint _currentServerTick;

        private byte[] _receiveBuffer;
        private bool _decodedHeader;

        private bool _systemMode;
        private int _currentReceivedNumber;
        private int _lastReceivedAck;
        private int _msPerTick;
        private long _currentClientTime;

        private bool _registered;
        private readonly List<int> _latestProbes = new List<int>();
        private int _latestProbe;

        private readonly List<GenericMultiPartTemp> _genericMultiPartTemp = new List<GenericMultiPartTemp>();
        private readonly List<ActionBlock> _actions = new List<ActionBlock>();
        private byte[] _msgXmlMD5;
        private byte[] _databaseXmlMD5;

        private bool _alreadyWarned;

        private long _previousTime;

        private readonly RyzomClient _client;

        public NetworkConnection(RyzomClient client)
        {
            _client = client;

            Reset();
        }

        /// <summary>
        ///     state of the connection
        /// </summary>
        public ConnectionState ConnectionState
        {
            get => _connectionState;
            set
            {
                Console.Title = $"[RCC] {Program.Version} - {value}";
                _client.GetLogger().Debug($"Connection state changed to {value}");
                _connectionState = value;
            }
        }

        public uint GetCurrentServerTick() => _currentServerTick;

        /// <summary>
        ///     Reset of the packet data counters and times
        /// </summary>
        public void Reset()
        {
            _currentSendNumber = 0;
            LastReceivedNumber = 0;
            _ackBitMask = 0;

            _synchronize = 0;
            _lct = 100;

            _latestSync = 0;

            LongAckBitField = new byte[1024 / 8];

            _lastAckInLongAck = 0;
            LastSentCycle = 0;
        }

        /// <summary>
        ///     datetime milliseconds
        /// </summary>
        public long RyzomGetLocalTime()
        {
            return DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        }

        /// <summary>
        ///     Start the connection state machine - Udp socket will connect at this point
        /// </summary>
        internal void Connect()
        {
            if (ConnectionState != ConnectionState.NotConnected)
            {
                throw new Exception(
                    "Unable to connect(): connection not properly initialised (maybe connection not closed).");
            }

            // S12: connect to the FES. Note: In UDP mode, it's the user that have to send the cookie to the front end
            try
            {
                _connection.Connect(_frontendAddress);
            }
            catch (Exception e)
            {
                throw new Exception("FS refused the connection (" + e.Message + ")");
            }

            ConnectionState = ConnectionState.Login;

            _latestLoginTime = RyzomGetLocalTime();
            _latestSyncTime = _latestLoginTime;
            _latestProbeTime = _latestLoginTime;
            _mLoginAttempts = 0;
        }

        /// <summary>
        ///     Init the connection with the cookie from the login server and game server address - registers all action codes
        /// </summary>
        public void Init(string cookie, string addr)
        {
            if (ConnectionState != ConnectionState.NotInitialised &&
                ConnectionState != ConnectionState.Disconnect)
            {
                throw new Exception("Unable to init(): connection not properly closed yet.");
            }

            if (!_registered)
            {
                ActionFactory.RegisterAction(ActionCode.ActionGenericCode, typeof(ActionGeneric));
                ActionFactory.RegisterAction(ActionCode.ActionGenericMultiPartCode, typeof(ActionGenericMultiPart));

                _registered = true;
            }

            InitCookie(cookie, addr);

            // get md5 hashes
            _msgXmlMD5 = Misc.GetFileMD5("data\\msg.xml");
            _databaseXmlMD5 = Misc.GetFileMD5("data\\database.xml");
        }

        /// <summary>
        ///     Sets the cookie and front-end address, resets the connection state.
        /// </summary>
        public void InitCookie(string cookie, string addr)
        {
            _frontendAddress = addr;

            SetLoginCookieFromString(cookie);

            _client.GetLogger().Info($"Network initialisation with front end '{_frontendAddress}'");
            _client.GetLogger().Debug($"cookie {cookie}");

            ConnectionState = ConnectionState.NotConnected;
        }

        /// <summary>
        ///     set the impulse callback method for all stream actions
        /// </summary>
        public void SetImpulseCallback(Action<BitMemoryStream> impulseCallBack)
        {
            ImpulseCallback = impulseCallBack;
            ImpulseArg = null;
        }

        /// <summary>
        ///     Connection state machine - Login state
        ///     if receives System SYNC
        ///     immediate state Synchronize
        ///     else
        ///     sends System LoginCookie
        /// </summary>
        private bool StateLogin()
        {
            while (_connection.IsDataAvailable())
            {
                _decodedHeader = false;
                var msgin = new BitMemoryStream(true);

                if (!BuildStream(msgin) || !DecodeHeader(msgin)) continue;

                if (_systemMode)
                {
                    byte message = 0;
                    msgin.Serial(ref message);

                    switch ((SystemMessageType)message)
                    {
                        case SystemMessageType.SystemSyncCode:
                            // receive sync, decode sync
                            ConnectionState = ConnectionState.Synchronize;
                            _client.GetLogger().Debug("CNET: login->synchronize");
                            ReceiveSystemSync(msgin);
                            return true;

                        case SystemMessageType.SystemStalledCode:
                            // receive stalled, decode stalled and state stalled
                            ConnectionState = ConnectionState.Stalled;
                            _client.GetLogger().Debug("CNET: login->stalled");
                            ReceiveSystemStalled(msgin);
                            return true;

                        case SystemMessageType.SystemProbeCode:
                            // receive probe, decode probe and state probe
                            ConnectionState = ConnectionState.Probe;
                            // TODO _Changes.push_back(CChange(0, ProbeReceived));
                            _client.GetLogger().Debug("CNET: login->probe");
                            ReceiveSystemProbe(msgin);
                            return true;

                        case SystemMessageType.SystemServerDownCode:
                            Disconnect(); // will send disconnection message
                            _client.GetLogger().Error("BACK-END DOWN");
                            return false; // exit now from loop, don't expect a new state

                        default:
                            _client.GetLogger().Warn($"CNET: received system {message} in state Login");
                            break;
                    }
                }
                else
                {
                    _client.GetLogger().Warn("CNET: received normal in state Login");
                }
            }


            // send ack sync if received sync or last sync timed out
            if (_updateTime - _latestLoginTime > 300)
            {
                SendSystemLogin();
                _latestLoginTime = _updateTime;

                if (_mLoginAttempts > 24)
                {
                    _mLoginAttempts = 0;
                    // will send disconnection message
                    Disconnect();
                    _client.GetLogger().Warn("CNET: Too many LOGIN attempts, connection problem");
                    // exit now from loop, don't expect a new state
                    return true;
                }

                ++_mLoginAttempts;
            }

            return false;
        }

        /// <summary>
        ///     Connection state machine - Probe State
        ///     if receives System SYNC
        ///     immediate state SYNC
        ///     else if receives System PROBE
        ///     decode PROBE
        ///     sends System ACK_PROBE
        /// </summary>
        private bool StateProbe()
        {
            while (_connection.IsDataAvailable())
            {
                _decodedHeader = false;
                var msgin = new BitMemoryStream(true);

                if (BuildStream(msgin) && DecodeHeader(msgin))
                {
                    if (_systemMode)
                    {
                        byte message = 0;
                        msgin.Serial(ref message);

                        switch ((SystemMessageType)message)
                        {
                            case SystemMessageType.SystemSyncCode:
                                // receive sync, decode sync and state synchronize
                                ConnectionState = ConnectionState.Synchronize;
                                ReceiveSystemSync(msgin);
                                return true;

                            case SystemMessageType.SystemStalledCode:
                                // receive sync, decode sync and state synchronize
                                ConnectionState = ConnectionState.Stalled;
                                ReceiveSystemStalled(msgin);
                                return true;

                            case SystemMessageType.SystemProbeCode:
                                // receive sync, decode sync
                                ReceiveSystemProbe(msgin);
                                break;

                            case SystemMessageType.SystemServerDownCode:
                                // will send disconnection message
                                Disconnect();
                                _client.GetLogger().Error("BACK-END DOWN");
                                // exit now from loop, don't expect a new state
                                return false;

                            default:
                                _client.GetLogger().Warn($"CNET: received system {message} in state Probe");
                                break;
                        }
                    }
                    else
                    {
                        _client.GetLogger().Warn("CNET: received normal in state Probe");
                        _latestProbeTime = _updateTime;
                    }
                }
            }

            // send ack sync if received sync or last sync timed out
            if (_latestProbes.Count != 0 || _updateTime - _latestProbeTime > 300)
            {
                SendSystemAckProbe();
                _latestProbeTime = _updateTime;
            }
            else
                Thread.Sleep(10);

            return false;
        }

        /// <summary>
        ///     Connection state machine - Synchronize State
        ///     if receives System PROBE
        ///     immediate state Probe
        ///     else if receives Normal
        ///     immediate state Connected
        ///     sends System ACK_SYNC
        /// </summary>
        private bool StateSynchronize()
        {
            while (_connection.IsDataAvailable())
            {
                _decodedHeader = false;
                var msgin = new BitMemoryStream(true);

                if (BuildStream(msgin) && DecodeHeader(msgin))
                {
                    if (_systemMode)
                    {
                        byte message = 0;
                        msgin.Serial(ref message);

                        switch ((SystemMessageType)message)
                        {
                            case SystemMessageType.SystemProbeCode:
                                // receive probe, decode probe and state probe
                                ConnectionState = ConnectionState.Probe;
                                // TODO _Changes.push_back(CChange(0, ProbeReceived)); 
                                ReceiveSystemProbe(msgin);
                                return true;

                            case SystemMessageType.SystemStalledCode:
                                // receive stalled, decode stalled and state stalled
                                ConnectionState = ConnectionState.Stalled;
                                ReceiveSystemStalled(msgin);
                                return true;

                            case SystemMessageType.SystemSyncCode:
                                // receive sync, decode sync
                                ReceiveSystemSync(msgin);
                                break;

                            case SystemMessageType.SystemServerDownCode:
                                // will send disconnection message
                                Disconnect();
                                _client.GetLogger().Error("BACK-END DOWN");
                                // exit now from loop, don't expect a new state
                                return false;

                            default:
                                _client.GetLogger().Warn($"CNET: received system {message} in state Synchronize");
                                break;
                        }
                    }
                    else
                    {
                        ConnectionState = ConnectionState.Connected;
                        // TODO _Changes.push_back(CChange(0, ConnectionReady));
                        ImpulseDecoder.Reset();
                        ReceiveNormalMessage(msgin);
                        return true;
                    }
                }
            }

            // send ack sync if received sync or last sync timed out
            if (_updateTime - _latestSyncTime > 300)
                SendSystemAckSync();

            return false;
        }

        /// <summary>
        ///     Connection state machine - Connected State
        ///     if receives System PROBE
        ///     immediate state Probe
        ///     else if receives Normal
        ///     sends Normal data
        /// </summary>
        private bool StateConnected()
        {
            // Prevent to increment the client time when the front-end does not respond
            var now = RyzomGetLocalTime();
            var diff = now - _previousTime;
            _previousTime = RyzomGetLocalTime();

            if (diff > 3000 && !_connection.IsDataAvailable())
            {
                return false;
            }

            // update the current time;
            while (_currentClientTime < _updateTime - _msPerTick - _lct &&
                   _currentClientTick < _currentServerTick)
            {
                _currentClientTime += _msPerTick;

                _currentClientTick++;
            }

            if (_currentClientTick >= _currentServerTick && !_connection.IsDataAvailable())
            {
                return false;
            }

            while (_connection.IsDataAvailable())
            {
                _decodedHeader = false;
                var msgin = new BitMemoryStream(true);

                if (BuildStream(msgin) && DecodeHeader(msgin))
                {
                    if (_systemMode)
                    {
                        byte message = 0;
                        msgin.Serial(ref message);

                        switch ((SystemMessageType)message)
                        {
                            case SystemMessageType.SystemProbeCode:
                                // receive probe, and goto state probe
                                ConnectionState = ConnectionState.Probe;
                                // TODO _Changes.Add(CChange(0, ProbeReceived));
                                ReceiveSystemProbe(msgin);
                                return true;

                            case SystemMessageType.SystemSyncCode:
                                // receive stalled, decode stalled and state stalled
                                ConnectionState = ConnectionState.Synchronize;
                                ReceiveSystemSync(msgin);
                                return true;

                            case SystemMessageType.SystemStalledCode:
                                // receive stalled, decode stalled and state stalled
                                ConnectionState = ConnectionState.Stalled;
                                ReceiveSystemStalled(msgin);
                                return true;

                            case SystemMessageType.SystemServerDownCode:
                                // will send disconnection message
                                Disconnect();
                                _client.GetLogger().Error("BACK-END DOWN");
                                // exit now from loop, don't expect a new state
                                return false;

                            default:
                                _client.GetLogger().Warn($"CNET: received system {message} in state Connected");
                                break;
                        }
                    }
                    else
                    {
                        ReceiveNormalMessage(msgin);
                    }
                }
            }

            return false;
        }


        /// <summary>
        ///     Connection state machine - Stalled State
        ///     TODO: Connection state machine - Stalled State
        /// </summary>
        private bool StateStalled()
        {
            _client.GetLogger().Error($"{MethodBase.GetCurrentMethod()?.Name} called, but not implemented");
            return false;
        }

        /// <summary>
        ///     Connection state machine - Quit State
        ///     TODO: Connection state machine - Quit State
        /// </summary>
        private bool StateQuit()
        {
            _client.GetLogger().Error($"{MethodBase.GetCurrentMethod()?.Name} called, but not implemented");
            return false;
        }

        /// <summary>
        ///     Receive and extract a normal (non system) message from a stream
        /// </summary>
        private void ReceiveNormalMessage(BitMemoryStream msgin)
        {
            _client.GetLogger().Debug($"CNET: received normal message Packet={LastReceivedNumber} Ack={_lastReceivedAck}");
            _client.GetLogger().Debug($"{msgin}");

            var actions = new List<Action.ActionBase>();
            ImpulseDecoder.Decode(msgin, _currentReceivedNumber, _lastReceivedAck, _currentSendNumber, actions);

            // we can now remove all old action that are acked
            while (_actions.Count != 0 && _actions[0].FirstPacket != 0 && _actions[0].FirstPacket <= _lastReceivedAck)
            {
                // CActionBlock automatically remove() actions when deleted

                _client.GetLogger().Debug($"removed action {_actions[0]}");
                _actions.RemoveAt(0);
            }

            Debug.Assert(_currentReceivedNumber * 2 + _synchronize > _currentServerTick);
            _currentServerTick = (uint)(_currentReceivedNumber * 2 + _synchronize);

            // TODO: receiveNormalMessage PacketStamps implementation

            // Decode the actions received in the impulsions
            foreach (var action in actions)
            {
                switch (action.Code)
                {
                    case ActionCode.ActionDisconnectionCode:
                        // Self disconnection
                        _client.GetLogger().Info("You were disconnected by the server");
                        // will send disconnection message
                        Disconnect();
                        // TODO LoginSM.pushEvent(CLoginStateMachine::ev_conn_dropped);

                        break;
                    case ActionCode.ActionGenericCode:
                        GenericAction((ActionGeneric)action);
                        break;

                    case ActionCode.ActionGenericMultiPartCode:
                        GenericAction((ActionGenericMultiPart)action);
                        break;

                    case ActionCode.ActionDummyCode:
                        _client.GetLogger().Debug($"CNET Received Dummy {action}");
                        // Nothing to do
                        break;
                }

                ActionFactory.Remove(action);
            }

            // Decode the visual properties
            DecodeVisualProperties(msgin);
        }

        /// <summary>
        ///     extract properties (database, sheets, ...) from a stream
        /// </summary>
        private static void DecodeVisualProperties(BitMemoryStream _)
        {
            // TODO decodeVisualProperties -> adding _Changes
        }

        /// <summary>
        ///     manage a generic action - invoke the impulse callback for the action
        /// </summary>
        private void GenericAction(ActionGeneric ag)
        {
            var bms = ag.Get();

            ImpulseCallback?.Invoke(bms);
        }

        /// <summary>
        ///     manage a generic multi part action - generate temporary multipart holder until the action is complete
        /// </summary>
        /// <param name="agmp"></param>
        private void GenericAction(ActionGenericMultiPart agmp)
        {
            while (_genericMultiPartTemp.Count <= agmp.Number)
            {
                _genericMultiPartTemp.Add(new GenericMultiPartTemp());
            }

            _genericMultiPartTemp[agmp.Number].Set(agmp, this);
        }

        /// <summary>
        ///     Probe state - deserialise info from stream
        /// </summary>
        private void ReceiveSystemProbe(BitMemoryStream msgin)
        {
            _latestProbeTime = _updateTime;
            msgin.Serial(ref _latestProbe);
            _latestProbes.Add(_latestProbe);
        }

        /// <summary>
        ///     TODO Stalled state - deserialise info from stream
        /// </summary>
        private void ReceiveSystemStalled(BitMemoryStream msgin)
        {
            _client.GetLogger().Info("CNET: received STALLED but not implemented " + msgin);
        }

        /// <summary>
        ///     Sync state - deserialise info from stream - send ack
        /// </summary>
        private void ReceiveSystemSync(BitMemoryStream msgin)
        {
            _latestSyncTime = _updateTime;
            long stime = 0;
            msgin.Serial(ref _synchronize);
            msgin.Serial(ref stime);
            msgin.Serial(ref _latestSync);

            _client.GetLogger().Debug($"receiveSystemSync {msgin}");

            var checkMsgXml = new byte[16];
            var checkDatabaseXml = new byte[16];

            msgin.Serial(ref checkMsgXml);
            msgin.Serial(ref checkDatabaseXml);

            var xmlInvalid = !checkMsgXml.SequenceEqual(_msgXmlMD5) ||
                             !checkDatabaseXml.SequenceEqual(_databaseXmlMD5);

            if (xmlInvalid && !_alreadyWarned)
            {
                _alreadyWarned = true;
                _client.GetLogger().Warn("XML files invalid: msg.xml and database.xml files are invalid (server version signature is different)");

                _client.GetLogger().Debug($"msg.xml client:{Misc.ByteArrToString(_msgXmlMD5)} server:{Misc.ByteArrToString(checkMsgXml)}");
                _client.GetLogger().Debug($"database.xml client:{Misc.ByteArrToString(_databaseXmlMD5)} server:{Misc.ByteArrToString(checkDatabaseXml)}");
            }

            _msPerTick = 100;

            _currentServerTick = (uint)(_synchronize + _currentReceivedNumber * 2);

            _currentClientTick = (uint)(_currentServerTick - (_lct + _msPerTick) / _msPerTick);
            _currentClientTime = _updateTime - (_lct + _msPerTick);

            SendSystemAckSync();
        }

        /// <summary>
        ///     Receive available data and convert it to a bitmemstream
        /// </summary>
        private bool BuildStream(BitMemoryStream msgin)
        {
            if (_connection.Receive(ref _receiveBuffer, false))
            {
                msgin.MemCpy(_receiveBuffer);

                return true;
            }

            // A receiving error means the front-end is down
            ConnectionState = ConnectionState.Disconnect;

            // won't send a disconnection msg because state is already Disconnect
            Disconnect();
            _client.GetLogger().Warn("DISCONNECTION");
            return false;
        }

        /// <summary>
        ///     decode the message header from the stream
        /// </summary>
        private bool DecodeHeader(BitMemoryStream msgin)
        {
            if (_decodedHeader)
                return true;

            ++_totalMessages;

            msgin.Serial(ref _currentReceivedNumber);
            msgin.Serial(ref _systemMode);

            if (_systemMode)
            {
            }
            else
            {
                msgin.Serial(ref _lastReceivedAck);
            }

            LastReceivedNumber = _currentReceivedNumber;
            _decodedHeader = true;
            return true;
        }

        /// <summary>
        ///     Disconnects the Client from the server sending a disconnection packet
        /// </summary>
        public void Disconnect()
        {
            if (ConnectionState == ConnectionState.NotInitialised ||
                ConnectionState == ConnectionState.NotConnected ||
                ConnectionState == ConnectionState.Authenticate ||
                ConnectionState == ConnectionState.Disconnect)
            {
                _client.GetLogger().Warn("Unable to disconnect(): not connected yet, or already disconnected.");
                return;
            }

            SendSystemDisconnection();
            _connection.Close();
            ConnectionState = ConnectionState.Disconnect;
        }

        /// <summary>
        ///     sends system sync acknowledge
        /// </summary>
        private void SendSystemAckSync()
        {
            var message = new BitMemoryStream();

            message.BuildSystemHeader(ref _currentSendNumber);

            var sync = (byte)SystemMessageType.SystemAckSyncCode;
            message.Serial(ref sync);
            message.Serial(ref LastReceivedNumber);
            message.Serial(ref _lastAckInLongAck);

            foreach (var ack in LongAckBitField)
            {
                var b = ack;
                message.Serial(ref b);
            }

            message.Serial(ref _latestSync);

            _client.GetLogger().Debug($"sendSystemAckSync {message}");

            var length = message.Length;
            _connection.Send(message.Buffer(), length);

            _latestSyncTime = _updateTime;
        }

        /// <summary>
        ///     sends system Probe acknowledge
        /// </summary>
        private void SendSystemAckProbe()
        {
            var message = new BitMemoryStream();

            message.BuildSystemHeader(ref _currentSendNumber);

            var probe = (byte)SystemMessageType.SystemAckProbeCode;
            var numprobes = _latestProbes.Count;

            message.Serial(ref probe);
            message.Serial(ref numprobes);

            for (var i = 0; i < numprobes; ++i)
            {
                var val = _latestProbes[i];
                message.Serial(ref val);
            }

            _latestProbes.Clear();

            var length = message.Length;
            _connection.Send(message.Buffer(), length);
        }

        /// <summary>
        ///     sends system Disconnection acknowledge
        /// </summary>
        private void SendSystemDisconnection()
        {
            var message = new BitMemoryStream();

            message.BuildSystemHeader(ref _currentSendNumber);

            byte disconnection = (byte)SystemMessageType.SystemDisconnectionCode;

            message.Serial(ref disconnection);

            int length = message.Length;

            if (_connection.Connected())
            {
                try
                {
                    _client.GetLogger().Debug($"sendSystemDisconnection {message}");
                    _connection.Send(message.Buffer(), length);
                }
                catch (Exception e)
                {
                    _client.GetLogger().Error($"Socket exception: {e.Message}");
                }
            }
        }

        /// <summary>
        ///     sends system login cookie
        /// </summary>
        public void SendSystemLogin()
        {
            var message = new BitMemoryStream();

            message.BuildSystemHeader(ref _currentSendNumber);

            byte login = (byte)SystemMessageType.SystemLoginCode;
            message.Serial(ref login);

            // Cookie
            message.Serial(ref _userAddr);
            message.Serial(ref _userKey);
            message.Serial(ref _userId);

            // todo: find out why 2 xD - string terminator?
            var unf = 2;
            message.Serial(ref unf);

            message.Serial(ref ClientConfig.LanguageCode);

            _client.GetLogger().Debug($"sendSystemLogin {message}");
            _connection.Send(message.Buffer(), message.Length);
        }

        /// <summary>
        ///     Sets the cookie for the connection
        /// </summary>
        private void SetLoginCookieFromString(string str)
        {
            var parts = str.Split('|');

            _userAddr = int.Parse(parts[0], NumberStyles.HexNumber);
            _userKey = int.Parse(parts[1], NumberStyles.HexNumber);
            _userId = int.Parse(parts[2], NumberStyles.HexNumber);
        }

        /// <summary>
        ///     update connection info - tests the connection state and inits the state machine
        /// </summary>
        public bool Update()
        {
            _updateTime = RyzomGetLocalTime();
            _totalMessages = 0;

            // If we are disconnected, bypass the real network update
            if (ConnectionState == ConnectionState.Disconnect)
            {
                return false;
            }

            // Yoyo. Update the Smooth ServerTick.
            UpdateSmoothServerTick();

            if (!_connection.Connected())
            {
                _client.GetLogger().Warn("CNET: update() attempted whereas socket is not connected !");
                return false;
            }

            try
            {
                // State automaton
                bool stateBroke;

                do
                {
                    stateBroke = ConnectionState switch
                    {
                        ConnectionState.Login =>
                            // if receives System SYNC
                            //    immediate state Synchronize
                            // else
                            //    sends System LoginCookie
                            StateLogin(),
                        ConnectionState.Synchronize =>
                            // if receives System PROBE
                            //    immediate state Probe
                            // else if receives Normal
                            //    immediate state Connected
                            // else
                            //    sends System ACK_SYNC
                            StateSynchronize(),
                        ConnectionState.Connected =>
                            // if receives System PROBE
                            //    immediate state Probe
                            // else if receives Normal
                            //	   sends Normal data
                            StateConnected(),
                        ConnectionState.Probe =>
                            // if receives System SYNC
                            //    immediate state SYNC
                            // else if receives System PROBE
                            //    decode PROBE
                            // sends System ACK_PROBE
                            StateProbe(),
                        ConnectionState.Stalled =>
                            // if receives System SYNC
                            //    immediate state SYNC
                            // else if receives System STALLED
                            //    decode STALLED (nothing to do)
                            // else if receives System PROBE
                            //    immediate state PROBE
                            StateStalled(),
                        ConnectionState.Quit =>
                            // if receives System SYNC
                            //    immediate state Synchronize
                            // else
                            //    sends System LoginCookie
                            StateQuit(),
                        _ => false
                    };
                } while (stateBroke); // && _TotalMessages<5);
            }
            catch (Exception)
            {
                _connectionState = ConnectionState.Disconnect;
            }

            return _totalMessages != 0;
        }

        /// <summary>
        ///     sends normal messages to the server including action blocks with actions if there are any to send
        /// </summary>
        public void Send(in uint cycle)
        {
            try
            {
                LastSentCycle = cycle;

                // if no actions were sent at this cyle, create a new block
                if (_actions.Count != 0 && _actions[^1].Cycle == 0)
                {
                    var block = _actions[^1];

                    block.Cycle = cycle;

                    // check last block isn't bigger than maximum allowed
                    int i;
                    var bitSize = 32 + 8; // block size is 32 (cycle) + 8 (number of actions

                    for (i = 0; i < block.Actions.Count; ++i)
                    {
                        bitSize += ActionFactory.Size(block.Actions[i]);

                        if (bitSize >= 480 * 8)
                            break;
                    }

                    if (i < block.Actions.Count)
                    {
                        throw new NotImplementedException("Send: ActionBlock size is bigger than 480 bit. Thats not implemented yet.");
                    }
                }

                if (ConnectionState == ConnectionState.Connected)
                {
                    SendNormalMessage();
                }
            }
            catch (Exception)
            {
                ConnectionState = ConnectionState.Disconnect;
                Disconnect();
            }
        }

        /// <summary>
        ///     Create the message to send to the server
        /// </summary>
        private void SendNormalMessage()
        {
            var message = new BitMemoryStream();

            var systemMode = false;

            message.Serial(ref _currentSendNumber);
            message.Serial(ref systemMode);
            message.Serial(ref LastReceivedNumber);
            message.Serial(ref _ackBitMask);

            foreach (var block in _actions)
            {
                // if block contains action that are not already stamped, don't send it now
                if (block.Cycle == 0)
                    break;

                if (block.FirstPacket == 0)
                    block.FirstPacket = _currentSendNumber;

                block.Serial(message);

                // Prevent to send a message too big
                //if (message.GetPosInBit() > 480 * 8) // easy version TODO: GetPosInBit does not return the right size i guess

                // TODO fix workaround: send only 1 block at a time (to not get disconnected)
                break;
            }

            _client.GetLogger().Debug($"CNET: send normal message Packet={_currentSendNumber} Ack={_lastReceivedAck}");
            _client.GetLogger().Debug($"{message}");

            _connection.Send(message.Buffer(), message.Length);

            _lastSendTime = RyzomGetLocalTime();

            // TODO _PacketStamps.push_back(make_pair(_CurrentSendNumber, _UpdateTime));

            _currentSendNumber++;
        }

        /// <summary>
        ///     Send is temporised, that is the packet may not be actually sent.
        ///     We don't care, since:
        ///     - this packet has no new data (not ticked send)
        ///     - a next send() will send packet if time elapsed enough
        ///     - a next send(tick) will really be sent
        ///     This way, we can say that at most 15 packets will be delivered each second
        ///     (5 send(tick), and 10 send() -- if you take getLocalTime() inaccuracy into account)
        /// </summary>
        internal void Send()
        {
            try
            {
                if (ConnectionState == ConnectionState.Connected && RyzomGetLocalTime() - _lastSendTime > 100)
                {
                    SendNormalMessage();
                }
            }
            catch (Exception)
            {
                ConnectionState = ConnectionState.Disconnect;
            }
        }

        /// <summary>
        ///     TODO updateSmoothServerTick - not that important
        /// </summary>
        private void UpdateSmoothServerTick()
        {
        }

        /// <summary>
        ///     Clear not acknownledged actions in sending buffer
        /// </summary>
        public void FlushSendBuffer()
        {
            _actions.Clear();
        }

        /// <summary>
        ///     pushes an action to be sent by the client to the send queue
        /// </summary>
        private void Push(Action.ActionBase action)
        {
            if (_actions.Count == 0 || _actions[^1].Cycle != 0)
            {
                _actions.Add(new ActionBlock());
            }

            _actions[^1].Actions.Add(action);
        }

        /// <summary>
        ///     pushes a stream (message) to be sent by the client to the send queue
        /// </summary>
        public void Push(BitMemoryStream msg)
        {
            const int maxImpulseBitSize = 230 * 8;

            var ag = (ActionGeneric)ActionFactory.Create(InvalidSlot, ActionCode.ActionGenericCode);

            if (ag == null) //TODO: see that with oliver...
                return;

            var bytelen = msg.Length;
            var impulseMinBitSize = ActionFactory.Size(ag);
            var impulseBitSize = impulseMinBitSize + (4 + bytelen) * 8;

            if (impulseBitSize < maxImpulseBitSize)
            {
                ag.Set(msg);
                Push(ag);
            }
            else
            {
                throw new NotImplementedException("Generic ActionBase is too big to get pushed to the stream.");
            }
        }
    }
}