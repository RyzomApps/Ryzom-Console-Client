///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using RCC.Helper;
using RCC.Network.Action;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace RCC.Network
{
    /// <summary>An engine that allows to encode/decode continuous properties using delta values.</summary>
    /// <author>Benjamin Legros</author>
    /// <author>Nevrax France</author>
    /// <date>2001</date>
    internal class PropertyDecoder
    {
        /// <summary>The entity entries</summary>
        readonly List<EntityEntry> _Entities = new List<EntityEntry>();

        readonly ushort _RefBitsX;
        readonly ushort _RefBitsY;
        readonly ushort _RefBitsZ;

        readonly int _RefPosX = new int();
        readonly int _RefPosY = new int();

        internal void Init(uint maximum)
        {
            SetMaximumEntities(maximum);
            Clear();
        }

        private void Clear()
        {
            uint sz = (uint)_Entities.Count;
            _Entities.Clear();
            _Entities.Resize((int)sz);
        }

        private void SetMaximumEntities(uint maximum)
        {
            _Entities.Resize((int)maximum);
        }

        internal ushort GetAssociationBits(byte entity)
        {
            return _Entities[entity].AssociationBits;
        }

        internal bool IsUsed(byte entity)
        {
            return _Entities[entity].EntryUsed;
        }

        internal uint Sheet(byte entity)
        {
            return _Entities[entity].Sheet;
        }

        internal void SetAssociationBits(byte slot, ushort associationBits)
        {
            _Entities[slot].AssociationBits = associationBits;
        }

        internal bool RemoveEntity(byte entity)
        {
            Debug.Assert(entity < _Entities.Count, "entity=" + (ushort)entity + "u size=" + _Entities.Count);

            //Workaround: assert converted to test when failure in vision from the server
            if (_Entities[entity].EntryUsed)
            {
                _Entities[entity].EntryUsed = false;
                _Entities[entity].Sheet = 0xffff;
            }

            return true;
        }

        internal void Receive(int _, ActionPosition action)
        {
            if (!action.IsContinuous())
                return;

            if (action.Code == ActionCode.ActionPositionCode)
            {
                ActionPosition act = (ActionPosition)(action);

                if (act.IsRelative)
                {
                    // Relative position (entity in a ferry...)
                    act.Position[0] = act.Position16[0];
                    act.Position[1] = act.Position16[1];
                    act.Position[2] = act.Position16[2];
                    _Entities[act.Slot].PosIsRelative = true;
                    _Entities[act.Slot].PosIsInterior = false;
                    RyzomClient.GetInstance().GetLogger().Info($"Relative Pos: {act.Position[0]} {act.Position[1]} {act.Position[2]} Pos16: {act.Position16[0]} {act.Position16[1]} {act.Position16[2]} Date {act.GameCycle}");
                }
                else
                {
                    // Absolute position
                    //nlinfo( "RefPos: %d %d %d RefBits: %hd %hd %hd", _RefPosX, _RefPosY, _RefPosZ, _RefBitsX, _RefBitsY, _RefBitsZ );
                    DecodeAbsPos2D(ref act.Position[0], ref act.Position[1], act.Position16[0], act.Position16[1]);
                    act.Position[2] = (short)act.Position16[2] << 4;
                    if (act.Interior)
                    {
                        act.Position[2] += 2;
                    }
                    //act->Position[2] = _RefPosZ + (((int)((sint16)(act->Position16[2] - _RefBitsZ))) << 4);
                    //nlinfo( "Pos16: %hd %hd %hd => Pos: %d %d %d", act->Position16[0], act->Position16[1], act->Position16[2], (int)px.LastReceived, (int)py.LastReceived, (int)pz.LastReceived );
                    _Entities[act.Slot].PosIsRelative = false;
                    _Entities[act.Slot].PosIsInterior = act.Interior;
                    //nlinfo( "Slot %hu: Absolute, Pos: %d %d %d, Pos16: %hu %hu %hu, Date %u", (uint16)act->CLEntityId, (int)act->Position[0], (int)act->Position[1], (int)act->Position[2], act->Position16[0], act->Position16[1], act->Position16[2], act->GameCycle );
                    //nlinfo( "            RefPos: %d %d %d, RefBits: %hu %hu %hu", _RefPosX, _RefPosY, _RefPosZ, _RefBitsX, _RefBitsY, _RefBitsZ );
                }
            }
        }

        /// <summary>
        /// Decode x and y
        /// </summary>
        void DecodeAbsPos2D(ref int x, ref int y, ushort x16, ushort y16)
        {
            x = _RefPosX + ((short)(x16 - _RefBitsX) << 4);
            y = _RefPosY + ((short)(y16 - _RefBitsY) << 4);
        }
    }
}