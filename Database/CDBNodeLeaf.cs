///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Xml;
using RCC.Network;

namespace RCC.Database
{
    enum EPropType
    {
        UNKNOWN = 0,
        // Unsigned
        I1, I2, I3, I4, I5, I6, I7, I8, I9, I10, I11, I12, I13, I14, I15, I16,
        I17, I18, I19, I20, I21, I22, I23, I24, I25, I26, I27, I28, I29, I30, I31, I32,
        I33, I34, I35, I36, I37, I38, I39, I40, I41, I42, I43, I44, I45, I46, I47, I48,
        I49, I50, I51, I52, I53, I54, I55, I56, I57, I58, I59, I60, I61, I62, I63, I64,
        // Signed
        S1, S2, S3, S4, S5, S6, S7, S8, S9, S10, S11, S12, S13, S14, S15, S16,
        S17, S18, S19, S20, S21, S22, S23, S24, S25, S26, S27, S28, S29, S30, S31, S32,
        S33, S34, S35, S36, S37, S38, S39, S40, S41, S42, S43, S44, S45, S46, S47, S48,
        S49, S50, S51, S52, S53, S54, S55, S56, S57, S58, S59, S60, S61, S62, S63, S64,
        TEXT, Nb_Prop_Type
    };

    public class CDBNodeLeaf : ICDBNode
    {
        /// property type
        EPropType _Type;

        /// gamecycle (servertick) of the last change for this value.
        /// change are made in readDelta only for change >= _LastChangeGC
        uint _LastChangeGC;

        /// property value
        long _Property;
        long _oldProperty;

        public CDBNodeLeaf(string name)
        {
            this.name = name;
        }

        internal override void SetAtomic(bool atomBranch)
        {

        }

        internal override void Init(XmlElement child, Action progressCallBack)
        {

        }

        /// <summary>
        /// readDelta
        /// </summary>
        internal override void readDelta(uint gc, BitMemoryStream f)
        {
            // If the property Type is valid.
            if (_Type > EPropType.UNKNOWN && _Type < EPropType.Nb_Prop_Type)
            {
                // Read the Property Value according to the Property Type.
                ulong recvd = 0;
                uint bits;
                if (_Type == EPropType.TEXT)
                {
                    bits = 32;
                }
                else if (_Type <= EPropType.I64)
                {
                    bits = (uint)_Type;
                }
                else
                {
                    bits = (uint)(_Type - 64);
                }

                f.Serial(ref recvd, (int)bits);


                // if the DB update is older than last DB update, abort (but after the read!!)
                if (gc < _LastChangeGC)
                {
                    return;
                }

                // bkup _oldProperty
                _oldProperty = _Property;

                // setup new one
                _Property = (long)recvd;

                // if signed
                if (!((_Type == EPropType.TEXT) || (_Type <= EPropType.I64)))
                {
                    // extend bit sign
                    long mask = (((long)1) << (int)bits) - (long)1;

                    if ((_Property >> (int)(bits - 1)) == 1)
                    {
                        _Property |= ~mask;
                    }
                }

                //if (verboseDatabase)
                //{
                //    nlinfo("CDB: Read value (%u bits) %" NL_I64 "d", bits, _Property);
                //}

                // bkup the date of change
                _LastChangeGC = gc;

                notifyObservers();

            }
            else
            {
                RyzomClient.GetInstance().GetLogger().Warn("CCDBNodeLeaf::readDelta : Property Type Unknown ('" + (uint)_Type + "') -> not serialized.");
            }

        }

        /// Find the leaf which count is specified (if found, the returned value is non-null and count is 0)
        internal override CDBNodeLeaf findLeafAtCount(uint count)
        {
            if (count == 0)
                return this;
            else
            {
                --count;
                return null;
            }
        }

        public void notifyObservers()
        {
            //// notify observer
            //foreach (IPropertyObserver it in _Observers)
            //{
            //    it.update(this);
            //}
            //
            //// mark parent branchs
            //if (_Parent)
            //{
            //    _Parent.onLeafChanged(_Name);
            //}
        }
    }
}