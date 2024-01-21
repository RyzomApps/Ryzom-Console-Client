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
using System.IO;
using System.Xml;
using Client.Interface;
using Client.Stream;

namespace Client.Database
{
    public class DatabaseNodeLeaf : DatabaseNode
    {
        /// <summary>property type</summary>
        private EPropType _type;

        /// <summary>
        /// gamecycle (servertick) of the last change for this value.
        /// change are made in readDelta only for change >= _LastChangeGC
        /// </summary>
        private uint _lastChangeGc;

        /// <summary>property value</summary>
        private long _property;
        private long _oldProperty;

        /// <summary>true if this value has changed</summary>
        bool _changed;
        private DatabaseNodeBranch _Parent;

        /// <summary>
        /// bservers to call when the value really change
        /// </summary>
        readonly List<IPropertyObserver> _Observers = new List<IPropertyObserver>();

        /// <summary>
        /// constructor
        /// </summary>
        public DatabaseNodeLeaf(string name)
        {
            _name = name;
        }

        /// <inheritdoc />
        internal override void Init(XmlElement node, Action progressCallBack, bool mapBanks = false, BankHandler bankHandler = null)
        {
            var type = node.GetAttribute("type");

            switch (type[0])
            {
                // IF type is an INT with n bits [1,64].
                case 'I':
                case 'U':
                    {
                        var nbBit = uint.Parse(type[1..]);

                        if (nbBit >= 1 && nbBit <= 64)
                        {
                            _type = (EPropType)nbBit;
                        }
                        else
                        {
                            RyzomClient.GetInstance().GetLogger().Warn("CCDBNodeLeaf::init : property is an INT and should be between [1,64] but it is " + nbBit + " bit(s).");
                            _type = EPropType.UNKNOWN;
                        }

                        break;
                    }
                // ELSE
                case 'S':
                    {
                        var nbBit = uint.Parse(type[1..]);

                        if (nbBit >= 1 && nbBit <= 64)
                        {
                            _type = (EPropType)(nbBit + 64);
                        }
                        else
                        {
                            RyzomClient.GetInstance().GetLogger().Warn(
                                $"CCDBNodeLeaf::init : property is an SINT and should be between [1,64] but it is {nbBit} bit(s).");
                            _type = EPropType.UNKNOWN;
                        }

                        break;
                    }
                default:
                    {
                        // IF it is a TEXT.
                        if (type == "TEXT")
                        {
                            _type = EPropType.TEXT;
                        }
                        // ELSE type unknown.
                        else
                        {
                            RyzomClient.GetInstance().GetLogger().Warn($"CCDBNodeLeaf::init : type '{type}' is unknown.");
                            _type = EPropType.UNKNOWN;
                        }

                        break;
                    }
            }
        }

        /// <inheritdoc />
        internal override void ReadDelta(uint gc, BitMemoryStream f)
        {
            // If the property Type is valid.
            if (_type > EPropType.UNKNOWN && _type < EPropType.Nb_Prop_Type)
            {
                // Read the Property Value according to the Property Type.
                ulong recvd = 0;
                uint bits;

                if (_type == EPropType.TEXT)
                {
                    bits = 32;
                }
                else if (_type <= EPropType.I64)
                {
                    bits = (uint)_type;
                }
                else
                {
                    bits = (uint)(_type - 64);
                }

                f.Serial(ref recvd, (int)bits);

                // if the DB update is older than last DB update, abort (but after the read!!)
                if (gc < _lastChangeGc)
                {
                    return;
                }

                // bkup _oldProperty

                // setup new one
                _property = (long)recvd;

                // if signed
                if (!(_type == EPropType.TEXT || _type <= EPropType.I64))
                {
                    // extend bit sign
                    var mask = ((long)1 << (int)bits) - 1;

                    if (_property >> (int)(bits - 1) == 1)
                    {
                        _property |= ~mask;
                    }
                }

                // bkup the date of change
                _lastChangeGc = gc;

                NotifyObservers();

            }
            else
            {
                RyzomClient.GetInstance().GetLogger().Warn($"CCDBNodeLeaf::readDelta : Property Type Unknown ('{(uint)_type}') -> not serialized.");
            }
        }

        internal void SetValue8(byte uc)
        {
            throw new NotImplementedException();
        }

        internal void SetValue16(short quality)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Set the value of the property
        /// </summary>
        internal void SetValue32(int prop)
        {
            var newVal = (long)prop;
            SetValue64(newVal);
        }

        /// <summary>
        /// Set the value of the property (set '_Changed' flag with 'true').
        /// </summary>
        public void SetValue64(long prop)
        {
            if (_property == prop) return;

            if (!_changed)
            {
                _changed = true;
            }

            _oldProperty = _property;
            _property = prop;

            // notify observer
            NotifyObservers();
        }

        /// <inheritdoc />
        internal override DatabaseNode GetNode(ushort idx)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        internal override long GetProp(TextId id)
        {
            // assert that there are no lines left in the textid
            Debug.Assert(id.GetCurrentIndex() == id.Size());

            // Return the property value.
            return GetValue64();
        }

        /// <summary>
        /// Find the leaf which count is specified (if found, the returned value is non-null and count is 0)
        /// </summary>
        internal override DatabaseNodeLeaf FindLeafAtCount(ref uint count)
        {
            if (count == 0)
                return this;

            --count;
            return null;
        }

        /// <inheritdoc />
        internal override DatabaseNode GetNode(TextId id, bool bCreate = true)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        internal override void ResetData(uint gc, bool forceReset = false)
        {
            if (forceReset)
            {
                _lastChangeGc = 0;
                SetValue64(0);
            }
            else if (gc >= _lastChangeGc)
            {
                // apply only if happens after the DB change
                _lastChangeGc = gc;
                SetValue64(0);
            }
        }

        /// <summary>
        /// Count the leaves
        /// </summary>
        /// <returns>1</returns>
        internal override uint CountLeaves()
        {
            return 1;
        }

        /// <summary>
        /// Return the value of the property
        /// </summary>
        public long GetValue64() { return _property; }

        /// <summary>
        /// Return the value of the property
        /// </summary>
        public int GetValue32()
        {
            return (int)(_property & 0xffffffff);
        }

        /// <summary>
        /// Return the value of the property
        /// </summary>
        public short GetValue16()
        {
            return (short)(_property & 0xffff);
        }

        /// <summary>
        /// Return the value of the property
        /// </summary>
        public byte GetValue8()
        {
            return (byte)(_property & 0xff);
        }

        /// <summary>
        /// Return the value of the property
        /// </summary>
        public bool GetValueBool()
        {
            return (_property != 0);
        }

        /// <inheritdoc />
        internal override void Write(string id, StreamWriter f)
        {
            f.WriteLine($"{_property}\t{id}");
        }

        /// <summary>
        /// Set the value of a property, only if gc>=_LastChangeGC
        /// </summary>
        internal bool SetPropCheckGC(uint gc, long value)
        {
            // Apply only if happens after the DB change
            if (gc >= _lastChangeGc)
            {
                // new recent date
                _lastChangeGc = gc;

                // Set the property value (and set "_Changed" flag with 'true');
                SetValue64(value);

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <inheritdoc/>
        public override bool AddObserver(IPropertyObserver observer, TextId id)
        {
            _Observers.Add(observer);
            return true;
        }

        /// <inheritdoc/>
        public override bool RemoveObserver(IPropertyObserver observer, TextId UnnamedParameter1)
        {
            if (!_Observers.Contains(observer))
                // no observer has been removed..
                return false;

            _Observers.Remove(observer);
            return true;
        }

        /// <inheritdoc/>
        public void NotifyObservers()
        {
            List<IPropertyObserver> obs = _Observers;

            // notify observer
            foreach (IPropertyObserver it in obs)
            {
                it.Update(this);
            }

            // mark parent branchs
            if (_parent != null)
            {
                _parent.OnLeafChanged(_name);
            }
        }
    }
}