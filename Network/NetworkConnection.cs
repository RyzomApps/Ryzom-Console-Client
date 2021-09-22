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

        private UdpSocket _connection = new UdpSocket();

        private ConnectionState _connectionState = ConnectionState.NotInitialised;

        private int _userAddr;
        private int _userKey;
        private int _userId;
        private int _valid;

        private int _currentSendNumber;
        private long _lastReceivedTime;
        private long _lastReceivedNormalTime;
        private int _ackBitMask;
        private int _lastAckBit;
        private uint _synchronize;
        private int _instantPing;
        private int _bestPing;
        private int _lct;
        private int _lastSentSync;
        private int _latestSync;
        private int _dummySend;
        private int _lastAckInLongAck;

        private int _totalReceivedBytes;
        private int _partialReceivedBytes;
        private int _totalSentBytes;

        private int _partialSentBytes;
        private int _lastReceivedPacketInBothModes;
        private int _totalLostPackets;
        private bool _connectionQuality;
        private int _currentSmoothServerTick;
        private int _sstLastLocalTime;
        private string _frontendAddress;

        private long _latestLoginTime;
        private long _latestSyncTime;
        private long _latestProbeTime;
        private long _updateTime;
        private long _lastSendTime;

        private int _mLoginAttempts;

        private long _updateTicks;
        private bool _receivedSync;
        private int _normalPacketsReceived;
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

        private object _dataBase;
        private bool _registered;
        private long _machineTimeAtTick;
        private long _machineTicksAtTick;
        private readonly List<int> _latestProbes = new List<int>();
        private int _latestProbe;

        private readonly List<GenericMultiPartTemp> _genericMultiPartTemp = new List<GenericMultiPartTemp>();
        private readonly List<ActionBlock> _actions = new List<ActionBlock>();
        private byte[] _msgXmlMD5;
        private byte[] _databaseXmlMD5;

        private bool _alreadyWarned;
        private byte _impulseMultiPartNumber = 0;

        private readonly RyzomClient _handler;

        public NetworkConnection(RyzomClient handler)
        {
            _handler = handler;
        }

        /// <summary>
        ///     state of the connection
        /// </summary>
        public ConnectionState ConnectionState
        {
            get => _connectionState;
            set
            {
                _handler.GetLogger().Info($"Connection state changed to {value}");
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
            _lastReceivedTime = 0;
            _lastReceivedNormalTime = 0;
            _ackBitMask = 0;
            _lastAckBit = 0;

            _synchronize = 0;
            _instantPing = 10000;
            _bestPing = 10000;
            _lct = 100;
            _machineTimeAtTick = RyzomGetLocalTime();
            _machineTicksAtTick = RyzomGetPerformanceTime();

            _lastSentSync = 0;
            _latestSync = 0;

            //_PropertyDecoder.init(256);

            _dummySend = 0;
            //_LongAckBitField.resize(1024);
            LongAckBitField = new byte[1024 / 8];

            _lastAckInLongAck = 0;
            LastSentCycle = 0;

            _totalReceivedBytes = 0;
            _partialReceivedBytes = 0;
            _totalSentBytes = 0;
            _partialSentBytes = 0;
            //_MeanPackets.MeanPeriod = 5000;
            //_MeanLoss.MeanPeriod = 5000;

            _lastReceivedPacketInBothModes = 0;
            _totalLostPackets = 0;
            _connectionQuality = false;

            _currentSmoothServerTick = 0;
            _sstLastLocalTime = 0;
        }

        /// <summary>
        ///     reset the client and server ticks
        /// </summary>
        void InitTicks()
        {
            _currentClientTick = 0;
            _currentServerTick = 0;
            _msPerTick = 100;
            //_LCT = LCT;
        }

        /// <summary>
        ///     reinitialisation of the connection
        /// </summary>
        public void ReInit()
        {
            // TODO reinitialisation of the connection -> some methods missing
            ImpulseDecoder.Reset();
            //if (_DataBase)
            //    _DataBase->resetData(_CurrentServerTick, true);
            LongAckBitField = new byte[0];
            //_PacketStamps.clear();
            _actions.Clear();
            //_Changes.clear();
            _genericMultiPartTemp.Clear();
            //_IdMap.clear();
            Reset();
            InitTicks();

            // Reuse the udp socket
            _connection = new UdpSocket();
        }

        /// <summary>
        ///     datetime ticks
        /// </summary>
        public long RyzomGetPerformanceTime()
        {
            return DateTime.Now.Ticks;
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
                //CActionFactory.registerAction(TActionCode.ACTION_POSITION_CODE, CActionPosition);
                //CActionFactory.registerAction(TActionCode.ACTION_SYNC_CODE, CActionSync);
                //CActionFactory.registerAction(TActionCode.ACTION_DISCONNECTION_CODE, CActionDisconnection);
                //CActionFactory.registerAction(TActionCode.ACTION_ASSOCIATION_CODE, CActionAssociation);
                //CActionFactory.registerAction(TActionCode.ACTION_DUMMY_CODE, CActionDummy);
                //CActionFactory.registerAction(TActionCode.ACTION_LOGIN_CODE, CActionLogin);
                //CActionFactory.registerAction(TActionCode.ACTION_TARGET_SLOT_CODE, CActionTargetSlot);
                ActionFactory.RegisterAction(ActionCode.ActionGenericCode, typeof(ActionGeneric));
                ActionFactory.RegisterAction(ActionCode.ActionGenericMultiPartCode, typeof(ActionGenericMultiPart));
                //CActionFactory.registerAction(TActionCode.ACTION_SINT64, CActionSint64);
                _registered = true;
            }

            InitCookie(cookie, addr);

            // Register property nbbits
            // CActionSint64::registerNumericPropertiesRyzom();

            // Init visual property tree
            //_VisualPropertyTreeRoot = new TVPNodeClient();
            //_VisualPropertyTreeRoot->buildTree();

            // If the server run on window, those are the one to test
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

            _handler.GetLogger().Info($"Network initialisation with front end '{_frontendAddress}' and cookie {cookie}");

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
        ///     set the client database manager - TODO: client database manager
        /// </summary>
        public void SetDataBase(object database)
        {
            _dataBase = database;
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
                            _handler.GetLogger().Debug("CNET: login->synchronize");
                            ReceiveSystemSync(msgin);
                            return true;

                        case SystemMessageType.SystemStalledCode:
                            // receive stalled, decode stalled and state stalled
                            ConnectionState = ConnectionState.Stalled;
                            _handler.GetLogger().Debug("CNET: login->stalled");
                            ReceiveSystemStalled(msgin);
                            return true;

                        case SystemMessageType.SystemProbeCode:
                            // receive probe, decode probe and state probe
                            ConnectionState = ConnectionState.Probe;
                            //_Changes.push_back(CChange(0, ProbeReceived));
                            _handler.GetLogger().Debug("CNET: login->probe");
                            ReceiveSystemProbe(msgin);
                            return true;

                        case SystemMessageType.SystemServerDownCode:
                            Disconnect(); // will send disconnection message
                            _handler.GetLogger().Error("BACK-END DOWN");
                            return false; // exit now from loop, don't expect a new state

                        default:
                            //msgin.displayStream("DBG:BEN:stateLogin:msgin");
                            _handler.GetLogger().Warn($"CNET: received system {message} in state Login");
                            break;
                    }
                }
                else
                {
                    //msgin.displayStream("DBG:BEN:stateLogin:msgin");
                    _handler.GetLogger().Warn($"CNET: received normal in state Login");
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
                    Disconnect(); // will send disconnection message
                    _handler.GetLogger().Warn("CNET: Too many LOGIN attempts, connection problem");
                    return true; // exit now from loop, don't expect a new state
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
                                //nldebug("CNET[%p]: probe->synchronize", this);
                                ReceiveSystemSync(msgin);
                                return true;

                            case SystemMessageType.SystemStalledCode:
                                // receive sync, decode sync and state synchronize
                                ConnectionState = ConnectionState.Stalled;
                                //nldebug("CNET[%p]: probe->stalled", this);
                                ReceiveSystemStalled(msgin);
                                return true;

                            case SystemMessageType.SystemProbeCode:
                                // receive sync, decode sync
                                ReceiveSystemProbe(msgin);
                                break;

                            case SystemMessageType.SystemServerDownCode:
                                Disconnect(); // will send disconnection message
                                _handler.GetLogger().Error("BACK-END DOWN");
                                return false; // exit now from loop, don't expect a new state

                            default:
                                _handler.GetLogger().Warn($"CNET: received system {message} in state Probe");
                                break;
                        }
                    }
                    else
                    {
                        _handler.GetLogger().Warn("CNET: received normal in state Probe");
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
                                //nldebug("CNET[%p]: synchronize->probe", this);
                                //_Changes.push_back(CChange(0, ProbeReceived)); TODO
                                ReceiveSystemProbe(msgin);
                                return true;

                            case SystemMessageType.SystemStalledCode:
                                // receive stalled, decode stalled and state stalled
                                ConnectionState = ConnectionState.Stalled;
                                //nldebug("CNET[%p]: synchronize->stalled", this);
                                ReceiveSystemStalled(msgin);
                                return true;

                            case SystemMessageType.SystemSyncCode:
                                // receive sync, decode sync
                                ReceiveSystemSync(msgin);
                                break;

                            case SystemMessageType.SystemServerDownCode:
                                Disconnect(); // will send disconnection message
                                _handler.GetLogger().Error("BACK-END DOWN");
                                return false; // exit now from loop, don't expect a new state

                            default:
                                _handler.GetLogger().Warn($"CNET: received system {message} in state Synchronize");
                                break;
                        }
                    }
                    else
                    {
                        ConnectionState = ConnectionState.Connected;
                        //nlwarning("CNET[%p]: synchronize->connected", this);
                        //_Changes.push_back(CChange(0, ConnectionReady)); TODO _Changes  synchronize->connected
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

        private long _previousTime = 0;

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

                _machineTimeAtTick = _updateTime;
                _machineTicksAtTick = _updateTicks;
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
                                //_Changes.Add(CChange(0, ProbeReceived));
                                ReceiveSystemProbe(msgin);
                                return true;

                            case SystemMessageType.SystemSyncCode:
                                // receive stalled, decode stalled and state stalled
                                ConnectionState = ConnectionState.Synchronize;
                                //nldebug("CNET[%p]: connected->synchronize", this);
                                ReceiveSystemSync(msgin);
                                return true;

                            case SystemMessageType.SystemStalledCode:
                                // receive stalled, decode stalled and state stalled
                                ConnectionState = ConnectionState.Stalled;
                                //nldebug("CNET[%p]: connected->stalled", this);
                                ReceiveSystemStalled(msgin);
                                return true;

                            case SystemMessageType.SystemServerDownCode:
                                Disconnect(); // will send disconnection message
                                _handler.GetLogger().Error("BACK-END DOWN");
                                return false; // exit now from loop, don't expect a new state

                            default:
                                _handler.GetLogger().Warn($"CNET: received system {message} in state Connected");
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
            _handler.GetLogger().Error($"{MethodBase.GetCurrentMethod()?.Name} called, but not implemented");
            return false;
        }

        /// <summary>
        ///     Connection state machine - Quit State
        ///     TODO: Connection state machine - Quit State
        /// </summary>
        private bool StateQuit()
        {
            _handler.GetLogger().Error($"{MethodBase.GetCurrentMethod()?.Name} called, but not implemented");
            return false;
        }

        /// <summary>
        ///     Receive and extract a normal (non system) message from a stream
        /// </summary>
        private void ReceiveNormalMessage(BitMemoryStream msgin)
        {
            _handler.GetLogger().Debug($"CNET: received normal message Packet={LastReceivedNumber} Ack={_lastReceivedAck}");

            var actions = new List<Action.Action>();
            ImpulseDecoder.Decode(msgin, _currentReceivedNumber, _lastReceivedAck, _currentSendNumber, actions);

            ++_normalPacketsReceived;

            // we can now remove all old action that are acked
            while (_actions.Count != 0 && _actions[0].FirstPacket != 0 && _actions[0].FirstPacket <= _lastReceivedAck)
            {
                // CActionBlock automatically remove() actions when deleted

                _handler.GetLogger().Debug($"removed action {_actions[0]}");
                _actions.RemoveAt(0);
            }

            Debug.Assert(_currentReceivedNumber * 2 + _synchronize > _currentServerTick);
            _currentServerTick = (uint)(_currentReceivedNumber * 2 + _synchronize);

            // TODO: receiveNormalMessage PacketStamps stuff

            // Decode the actions received in the impulsions
            foreach (var action in actions)
            {
                switch (action.Code)
                {
                    case ActionCode.ActionDisconnectionCode:
                        // Self disconnection
                        _handler.GetLogger().Info("You were disconnected by the server");
                        Disconnect(); // will send disconnection message
                        // TODO LoginSM.pushEvent(CLoginStateMachine::ev_conn_dropped);

                        break;
                    case ActionCode.ActionGenericCode:
                        GenericAction((ActionGeneric)action);
                        break;

                    case ActionCode.ActionGenericMultiPartCode:
                        GenericAction((ActionGenericMultiPart)action);
                        break;

                    case ActionCode.ActionDummyCode:
                        //CActionDummy* dummy = ((CActionDummy*)actions[i]);
                        _handler.GetLogger().Debug($"CNET Received Dummy{action}");
                        // Nothing to do
                        break;
                }

                ActionFactory.Remove(action);
            }

            // Decode the visual properties
            DecodeVisualProperties(msgin);

            _lastReceivedNormalTime = _updateTime;
        }

        /// <summary>
        ///     extract properties (database, sheets, ...) from a stream
        ///     TODO decodeVisualProperties -> adding _Changes
        /// </summary>
        void DecodeVisualProperties(BitMemoryStream msgin)
        {
            try
            {
                //nldebug( "pos: %d  len: %u", msgin.getPos(), msgin.length() );
                //while (false)
                //{
                ////nlinfo( "Reading pass %u, BEFORE HEADER: pos: %d  len: %u", ++i, msgin.getPosInBit(), msgin.length() * 8 );
                //
                //// Check if there is a new block to read
                if (msgin.GetPosInBit() + sizeof(byte) * 8 > msgin.Length * 8)
                    return;
                //
                //
                //
                //// Header
                //TCLEntityId slot;
                //msgin.serialAndLog1(slot);
                //
                //uint associationBits;
                //msgin.serialAndLog2(associationBits, 2);
                ////nlinfo( "slot %hu AB: %u", (uint16)slot, associationBits );
                //if (associationBitsHaveChanged(slot, associationBits) && (!IgnoreEntityDbUpdates || slot == 0))
                //{
                //    //displayBitStream( msgin, beginbitpos, msgin.getPosInBit() );
                //    //			   nlinfo ("Disassociating S%hu (AB %u)", (uint16)slot, associationBits );
                //    if (_PropertyDecoder.isUsed(slot))
                //    {
                //        TSheetId sheet = _PropertyDecoder.sheet(slot);
                //        TIdMap::iterator it = _IdMap.find(sheet);
                //        if (it != _IdMap.end())
                //            _IdMap.erase(it);
                //        _PropertyDecoder.removeEntity(slot);
                //
                //        CChange theChange(slot, RemoveOldEntity );
                //        _Changes.push_back(theChange);
                //    }
                //    else
                //    {
                //        //					nlinfo( "Cannot disassociate slot %hu: sheet not received yet", (uint16)slot );
                //    }
                //}
                //
                //// Read the timestamp delta if there's one (otherwise take _CurrentServerTick)
                //TGameCycle timestamp;
                //bool timestampIsThere;
                //msgin.serialBitAndLog(timestampIsThere);
                //if (timestampIsThere)
                //{
                //    uint timestampDelta;
                //    msgin.serialAndLog2(timestampDelta, 4);
                //    timestamp = _CurrentServerTick - timestampDelta;
                //    //nldebug( "TD: %u (S%hu)", timestampDelta, (uint16)slot );
                //}
                //else
                //{
                //    timestamp = _CurrentServerTick;
                //}
                //
                //// Tree
                ////nlinfo( "AFTER HEADER: posBit: %d pos: %d  len: %u", msgin.getPosInBit(), msgin.getPos(), msgin.length() );
                //
                //TVPNodeClient* currentNode = _VisualPropertyTreeRoot;
                //msgin.serialBitAndLog(currentNode->a()->BranchHasPayload);
                //if (currentNode->a()->BranchHasPayload)
                //{
                //    CActionPosition* ap = (CActionPosition*)CActionFactory::getInstance()->create(slot, ACTION_POSITION_CODE);
                //    ap->unpack(msgin);
                //    _PropertyDecoder.receive(_CurrentReceivedNumber, ap);
                //
                //    /*
                //     * Set into property database
                //     */
                //
                //    // TEMP
                //    if (ap->Position[0] == 0 || ap->Position[1] == 0)
                //        nlwarning("S%hu: Receiving an invalid position", (uint16)slot);
                //
                //    if (_DataBase != NULL && (!IgnoreEntityDbUpdates || slot == 0))
                //    {
                //        CCDBNodeBranch* nodeRoot;
                //        nodeRoot = dynamic_cast<CCDBNodeBranch*>(_DataBase->getNode((uint16)0));
                //        if (nodeRoot)
                //        {
                //            CCDBNodeLeaf* node;
                //            node = dynamic_cast<CCDBNodeLeaf*>(nodeRoot->getNode(slot)->getNode(0));
                //            nlassert(node != NULL);
                //            node->setValue64(ap->Position[0]);
                //            node = dynamic_cast<CCDBNodeLeaf*>(nodeRoot->getNode(slot)->getNode(1));
                //            nlassert(node != NULL);
                //            node->setValue64(ap->Position[1]);
                //            node = dynamic_cast<CCDBNodeLeaf*>(nodeRoot->getNode(slot)->getNode(2));
                //            nlassert(node != NULL);
                //            node->setValue64(ap->Position[2]);
                //
                //            if (LoggingMode)
                //            {
                //                nlinfo("recvd position (%d,%d) for slot %hu, date %u", (sint32)(ap->Position[0]), (sint32)(ap->Position[1]), (uint16)slot, timestamp);
                //            }
                //        }
                //    }
                //
                //    bool interior = ap->Interior;
                //
                //    CActionFactory::getInstance()->remove((Action * &)ap);
                //
                //
                //    /*
                //     * Statistical prediction of time before next position update: set PredictedInterval
                //     */
                //
                //    //nlassert( MAX_POSUPDATETICKQUEUE_SIZE > 1 );
                //    deque<TGameCycle> & puTicks = _PosUpdateTicks[slot];
                //    multiset<TGameCycle> & puIntervals = _PosUpdateIntervals[slot];
                //
                //    // Flush the old element of tick queue and of the interval sorted set
                //    if (puTicks.size() == MAX_POSUPDATETICKQUEUE_SIZE)
                //    {
                //        puIntervals.erase(puIntervals.find(puTicks[1] - puTicks[0])); // erase only one element, not all corresponding to the value
                //        puTicks.pop_front();
                //    }
                //
                //    // Add a new element to the tick queue and possibly to the interval sorted set
                //    // Still to choose: _CurrentServerTick or timestamp ?
                //    TGameCycle latestInterval = 0;
                //    if (!puTicks.empty())
                //    {
                //        latestInterval = timestamp - puTicks.back();
                //        puIntervals.insert(latestInterval);
                //
                //        if (PosUpdateIntervalGraph)
                //            PosUpdateIntervalGraph->addOneValue(slot, (float)latestInterval);
                //    }
                //    puTicks.push_back(timestamp);
                //
                //    nlassert(puTicks.size() == puIntervals.size() + 1);
                //
                //    // Prediction function : Percentile(25 last, 0.8) + 1
                //    TGameCycle predictedInterval;
                //    if (puIntervals.empty())
                //    {
                //        predictedInterval = 0;
                //    }
                //    else
                //    {
                //        predictedInterval = (TGameCycle)(percentileRev(puIntervals, PREDICTION_REV_PERCENTILE) + 1);
                //
                //        //if ( predictedInterval > 100 )
                //        //	nlwarning( "Slot %hu: Predicted interval %u exceeds 100 ticks", (uint16)slot, predictedInterval );
                //
                //        if (PosUpdatePredictionGraph)
                //            PosUpdatePredictionGraph->addOneValue(slot, (float)predictedInterval);
                //    }
                //
                //    //nlinfo( "Slot %hu: Interval=%u Predicted=%u", (uint16)slot, latestInterval, predictedInterval );
                //
                //    /*
                //     * Add into the changes vector
                //     */
                //    CChange thechange(slot, PROPERTY_POSITION, timestamp );
                //    thechange.PositionInfo.PredictedInterval = predictedInterval;
                //    thechange.PositionInfo.IsInterior = interior;
                //    _Changes.push_back(thechange);
                //}
                //
                //currentNode = currentNode->b();
                //msgin.serialBitAndLog(currentNode->BranchHasPayload);
                //if (currentNode->BranchHasPayload)
                //{
                //    msgin.serialBitAndLog(currentNode->a()->BranchHasPayload);
                //    if (currentNode->a()->BranchHasPayload)
                //    {
                //        CActionSint64* ac = (CActionSint64*)CActionFactory::getInstance()->createByPropIndex(slot, PROPERTY_ORIENTATION);
                //        ac->unpack(msgin);
                //
                //        // Process orientation
                //        CChange thechange(slot, PROPERTY_ORIENTATION, timestamp);
                //        _Changes.push_back(thechange);
                //
                //        if (_DataBase != NULL && (!IgnoreEntityDbUpdates || slot == 0))
                //        {
                //            CCDBNodeBranch* nodeRoot;
                //            nodeRoot = dynamic_cast<CCDBNodeBranch*>(_DataBase->getNode(0));
                //            if (nodeRoot)
                //            {
                //                CCDBNodeLeaf* node = dynamic_cast<CCDBNodeLeaf*>(nodeRoot->getNode(slot)->getNode(PROPERTY_ORIENTATION));
                //                nlassert(node != NULL);
                //                node->setValue64(ac->getValue());
                //                if (LoggingMode)
                //                {
                //                    nlinfo("CLIENT: recvd property %hu (%s) for slot %hu, date %u", (uint16)PROPERTY_ORIENTATION, getPropText(PROPERTY_ORIENTATION), (uint16)slot, timestamp);
                //                }
                //                //nldebug("CLPROPNET[%p]: received property %d for entity %d: %" NL_I64 "u", this, action->PropIndex, action->CLEntityId, action->getValue());
                //            }
                //        }
                //
                //        CActionFactory::getInstance()->remove((Action * &)ac);
                //    }
                //
                //    TVPNodeClient::SlotContext.NetworkConnection = this;
                //    TVPNodeClient::SlotContext.Slot = slot;
                //    TVPNodeClient::SlotContext.Timestamp = timestamp;
                //
                //    // Discreet properties
                //    currentNode->b()->decodeDiscreetProperties(msgin);
                //}
                //}
            }
            catch
            {
                // End of stream (saves useless bits)
            }
        }

        /// <summary>
        ///     manage a generic action - invoke the impulse callback for the action
        ///     TODO: get memory stream -> CImpulseDecoder.decode to action?
        /// </summary>
        private void GenericAction(ActionGeneric ag)
        {
            var bms = ag.Get();

            //nldebug("CNET: Calling impulsion callback (size %u) :'%s'", this, bms.length(), toHexaString(bms.bufferAsVector()).c_str());
            //nldebug("CNET[%p]: Calling impulsion callback (size %u)", this, bms.length());

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

            //nldebug("CNET[%p]: received PROBE %d", this, _LatestProbe);
        }

        /// <summary>
        ///     TODO Stalled state - deserialise info from stream
        /// </summary>
        private void ReceiveSystemStalled(BitMemoryStream msgin)
        {
            _handler.GetLogger().Info("CNET: received STALLED but not implemented");
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

            _handler.GetLogger().Debug($"receiveSystemSync {msgin}");

            //return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            byte[] checkMsgXml = new byte[16];
            byte[] checkDatabaseXml = new byte[16];

            msgin.Serial(ref checkMsgXml);
            msgin.Serial(ref checkDatabaseXml);

            var xmlInvalid = !checkMsgXml.SequenceEqual(_msgXmlMD5) ||
                             !checkDatabaseXml.SequenceEqual(_databaseXmlMD5);

            if (xmlInvalid && !_alreadyWarned)
            {
                _alreadyWarned = true;
                _handler.GetLogger().Warn(
                    $"XML files invalid: msg.xml and database.xml files are invalid (server version signature is different)");

                _handler.GetLogger().Warn(
                    $"msg.xml client:{Misc.ByteArrToString(_msgXmlMD5)} server:{Misc.ByteArrToString(checkMsgXml)}");
                _handler.GetLogger().Warn(
                    $"database.xml client:{Misc.ByteArrToString(_databaseXmlMD5)} server:{Misc.ByteArrToString(checkDatabaseXml)}");
            }

            _receivedSync = true;

            _msPerTick = 100; // initial values

            //#pragma message ("HALF_FREQUENCY_SENDING_TO_CLIENT")
            _currentServerTick = (uint)(_synchronize + _currentReceivedNumber * 2);

            _currentClientTick = (uint)(_currentServerTick - (_lct + _msPerTick) / _msPerTick);
            _currentClientTime = _updateTime - (_lct + _msPerTick);

            //nlinfo( "CNET[%p]: received SYNC %" NL_I64 "u %" NL_I64 "u - _CurrentReceivedNumber=%d _CurrentServerTick=%d", this, (uint64)_Synchronize, (uint64)stime, _CurrentReceivedNumber, _CurrentServerTick );

            SendSystemAckSync();
        }

        /// <summary>
        ///     Receive available data and convert it to a bitmemstream
        /// </summary>
        private bool BuildStream(BitMemoryStream msgin)
        {
            const int len = 65536;

            if (_connection.Receive(ref _receiveBuffer, len, false))
            {
                // Fill the message
                //msgin.clear();
                msgin.MemCpy(_receiveBuffer);

                return true;
            }
            else
            {
                // A receiving error means the front-end is down
                ConnectionState = ConnectionState.Disconnect;
                Disconnect(); // won't send a disconnection msg because state is already Disconnect
                _handler.GetLogger().Warn($"DISCONNECTION");
                return false;
            }
        }

        /// <summary>
        ///     decode the message header from the stream
        /// </summary>
        private bool DecodeHeader(BitMemoryStream msgin)
        {
            if (_decodedHeader)
                return true;

            ++_totalMessages;

            _lastReceivedTime = _updateTime;

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
        private void Disconnect()
        {
            if (ConnectionState == ConnectionState.NotInitialised ||
                ConnectionState == ConnectionState.NotConnected ||
                ConnectionState == ConnectionState.Authenticate ||
                ConnectionState == ConnectionState.Disconnect)
            {
                _handler.GetLogger().Warn("Unable to disconnect(): not connected yet, or already disconnected.");
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
            // message.serial(_LongAckBitField); todo

            foreach (var ack in LongAckBitField)
            {
                var b = ack;
                message.Serial(ref b);
            }

            message.Serial(ref _latestSync);

            _handler.GetLogger().Debug($"sendSystemAckSync {message}");

            var length = message.Length;
            _connection.Send(message.Buffer(), length);
            //sendUDP (&(_Connection), message.buffer(), length);
            //statsSend(length); todo stats

            _latestSyncTime = _updateTime;
        }

        /// <summary>
        ///     sends system Probe acknowledge
        /// </summary>
        void SendSystemAckProbe()
        {
            var message = new BitMemoryStream();

            message.BuildSystemHeader(ref _currentSendNumber);

            byte probe = (byte)SystemMessageType.SystemAckProbeCode;
            int numprobes = _latestProbes.Count;

            message.Serial(ref probe);
            message.Serial(ref numprobes);

            int i;
            for (i = 0; i < numprobes; ++i)
            {
                var val = _latestProbes[i];
                message.Serial(ref val);
            }

            _latestProbes.Clear();

            var length = message.Length;
            _connection.Send(message.Buffer(), length);
            //sendUDP (&(_Connection), message.buffer(), length);
            //statsSend(length);

            //nlinfo("CNET[%p]: sent ACK_PROBE (%d probes)", this, numprobes);
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
                    _handler.GetLogger().Debug($"sendSystemDisconnection {message}");
                    _connection.Send(message.Buffer(), length);
                }
                catch (Exception e)
                {
                    _handler.GetLogger().Error($"Socket exception: " + e.Message);
                }
            }

            //sendUDP (&(_Connection), message.buffer(), length);
            //statsSend(length); TODO stats

            //updateBufferizedPackets(); TODO updateBufferizedPackets
            //nlinfo("CNET[%p]: sent DISCONNECTION", this);
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

            //message.serial(Cookie);
            message.Serial(ref _userAddr);
            message.Serial(ref _userKey);
            message.Serial(ref _userId);

            // todo: find out why 2 xD - string terminator?
            var unf = 2;
            message.Serial(ref unf);

            message.Serial(ref ClientConfig.LanguageCode);

            _handler.GetLogger().Debug($"sendSystemLogin {message}");
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

            if (_userAddr == 0 && _userKey == 0 && _userId == 0)
                _valid = 0;
            else
                _valid = 1;
        }

        /// <summary>
        ///     update connection info - tests the connection state and inits the state machine
        /// </summary>
        public bool Update()
        {
            _updateTime = RyzomGetLocalTime();
            _updateTicks = RyzomGetPerformanceTime();
            _receivedSync = false;
            _normalPacketsReceived = 0;
            _totalMessages = 0;

            // If we are disconnected, bypass the real network update
            if (ConnectionState == ConnectionState.Disconnect)
            {
                _connectionQuality = false; // to block the user entity
                return false;
            }

            // Yoyo. Update the Smooth ServerTick.
            UpdateSmoothServerTick();

            if (!_connection.Connected())
            {
                _handler.GetLogger().Warn("CNET: update() attempted whereas socket is not connected !");
                return false;
            }

            try
            {
                // State automaton
                bool stateBroke;

                do
                {
                    switch (ConnectionState)
                    {
                        case ConnectionState.Login:
                            // if receives System SYNC
                            //    immediate state Synchronize
                            // else
                            //    sends System LoginCookie
                            stateBroke = StateLogin();
                            break;

                        case ConnectionState.Synchronize:
                            // if receives System PROBE
                            //    immediate state Probe
                            // else if receives Normal
                            //    immediate state Connected
                            // else
                            //    sends System ACK_SYNC
                            stateBroke = StateSynchronize();
                            break;

                        case ConnectionState.Connected:
                            // if receives System PROBE
                            //    immediate state Probe
                            // else if receives Normal
                            //	   sends Normal data
                            stateBroke = StateConnected();
                            break;

                        case ConnectionState.Probe:
                            // if receives System SYNC
                            //    immediate state SYNC
                            // else if receives System PROBE
                            //    decode PROBE
                            // sends System ACK_PROBE
                            stateBroke = StateProbe();
                            break;

                        case ConnectionState.Stalled:
                            // if receives System SYNC
                            //    immediate state SYNC
                            // else if receives System STALLED
                            //    decode STALLED (nothing to do)
                            // else if receives System PROBE
                            //    immediate state PROBE
                            stateBroke = StateStalled();
                            break;

                        case ConnectionState.Quit:
                            // if receives System SYNC
                            //    immediate state Synchronize
                            // else
                            //    sends System LoginCookie
                            stateBroke = StateQuit();
                            break;

                        default:
                            // Nothing here !
                            stateBroke = false; // will come here if a disconnection action is received inside a method that returns true
                            break;
                    }
                } while (stateBroke); // && _TotalMessages<5);
            }
            catch (Exception)
            {
                _connectionState = ConnectionState.Disconnect;
            }

            _connectionQuality = ConnectionState == ConnectionState.Connected &&
                                 _updateTime - _lastReceivedNormalTime < 2000 &&
                                 _currentClientTick < _currentServerTick;

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
                    int bitSize = 32 + 8; // block size is 32 (cycle) + 8 (number of actions

                    for (i = 0; i < block.Actions.Count; ++i)
                    {
                        bitSize += ActionFactory.Size(block.Actions[i]);
                        if (bitSize >= 480 * 8)
                            break;
                    }

                    if (i < block.Actions.Count)
                    {
                        throw new NotImplementedException();
                    }

                    //nlinfo("-BEEN- setcycle [size=%d, cycle=%d]", _Actions.size(), _Actions.empty() ? 0 : _Actions.back().Cycle);
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

            uint numPacked = 0;

            foreach (var block in _actions)
            {
                //for (var itblock = _Actions.Begin(); itblock != _Actions.end(); ++itblock)
                //{
                //    CActionBlock & block = *itblock;

                // if block contains action that are not already stamped, don't send it now
                if (block.Cycle == 0)
                    break;

                // Prevent to send a message too big
                //if (message.getPosInBit() + (*itblock).bitSize() > FrontEndInputBufferSize) // hard version
                //	break;

                if (block.FirstPacket == 0)
                    block.FirstPacket = _currentSendNumber;

                //nlassertex((*itblock).Cycle > lastPackedCycle, ("(*itblock).Cycle=%d lastPackedCycle=%d", (*itblock).Cycle, lastPackedCycle));

                //		lastPackedCycle = block.Cycle;

                block.Serial(message);
                ++numPacked;

                //nldebug("CNET[%p]: packed block %d, message is currently %d bits long", this, block.Cycle, message.getPosInBit());

                // Prevent to send a message too big
                //if (message.getPosInBit() + (*itblock).bitSize() > FrontEndInputBufferSize) // hard version
                //if (message.GetPosInBit() > 480 * 8) // easy version TODO: GetPosInBit does not return the right size i guess -> so we send only 1 block at a time (but do not get disconnected)
                break;
            }

            _handler.GetLogger().Debug($"sendNormalMessage {message}");
            _connection.Send(message.Buffer(), message.Length);

            _lastSendTime = RyzomGetLocalTime();

            //_PacketStamps.push_back(make_pair(_CurrentSendNumber, _UpdateTime));

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
            //_Actions.clear();
        }

        /// <summary>
        ///     pushes an action to be sent by the client to the send queue
        /// </summary>
        void Push(Action.Action action)
        {
            if (_actions.Count == 0 || _actions[^1].Cycle != 0)
            {
                //nlinfo("-BEEN- push back 2 [size=%d, cycle=%d]", _Actions.size(), _Actions.empty() ? 0 : _Actions.back().Cycle);
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

            int bytelen = msg.Length;
            int impulseMinBitSize = (int)ActionFactory.Size(ag);
            int impulseBitSize = impulseMinBitSize + (4 + bytelen) * 8;

            if (impulseBitSize < maxImpulseBitSize)
            {
                ag.Set(msg);
                Push(ag);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}