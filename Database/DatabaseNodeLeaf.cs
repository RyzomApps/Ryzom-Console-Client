///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using System;
using System.Diagnostics;
using System.Xml;
using RCC.Network;

namespace RCC.Database
{
    public class DatabaseNodeLeaf : DatabaseNodeBase
    {
        private const bool VerboseDatabase = false;

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

        public DatabaseNodeLeaf(string name)
        {
            Name = name;
        }

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

        /// <summary>
        /// readDelta
        /// </summary>
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

                if (VerboseDatabase)
                {
                    RyzomClient.GetInstance().GetLogger().Info($"CDB: Read value ({bits} bits) {_property}");
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

        internal override DatabaseNodeBase GetNode(ushort idx)
        {
            throw new NotImplementedException();
        }

        internal override bool GetNodeIndex(DatabaseNodeBase databaseNode, ref uint index)
        {
            throw new NotImplementedException();
        }

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

        internal override DatabaseNodeBase GetNode(TextId id, bool bCreate = true)
        {
            throw new NotImplementedException();
        }

        internal override void AttachChild(DatabaseNodeBase node, string nodeName)
        {
            throw new NotImplementedException();
        }

        internal override void ResetData(uint gc, bool forceReset = false)
        {
            if (forceReset)
            {
                _lastChangeGc = 0;
                SetValue64(0);
            }
            else if (gc >= _lastChangeGc)   // apply only if happens after the DB change
            {
                _lastChangeGc = gc;
                SetValue64(0);
            }
        }

        public void SetValue64(long prop)
        {
            if (_property != prop)
            {
                if (!_changed)
                {
                    _changed = true;
                }

                _oldProperty = _property;
                _property = prop;

                // notify observer
                NotifyObservers();
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

        /// <summary>Return the value of the property.</summary>
        private long GetValue64() { return _property; }

        public void NotifyObservers()
        {
            //Debug.Print("NotifyObservers");
            //throw new NotImplementedException();
        }
    }
}