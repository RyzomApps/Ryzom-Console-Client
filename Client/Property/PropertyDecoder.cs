///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using System;
using System.Diagnostics;
using System.Numerics;
using Client.Network.Action;

namespace Client.Property
{
    /// <summary>An engine that allows to encode/decode continuous properties using delta values.</summary>
    /// <author>Benjamin Legros</author>
    /// <author>Nevrax France</author>
    /// <date>2001</date>
    internal class PropertyDecoder
    {
        /// <summary>The entity entries</summary>
        private EntityEntry[] _entities = Array.Empty<EntityEntry>();

        private ushort _refBitsX;
        private ushort _refBitsY;

        private int _refPosX;
        private int _refPosY;

        internal void Init(uint maximum)
        {
            SetMaximumEntities(maximum);
            Clear();
        }

        private void Clear()
        {
            for (var i = 0; i < _entities.Length; i++)
            {
                _entities[i] = new EntityEntry();
            }
        }

        private void SetMaximumEntities(uint maximum)
        {
            Array.Resize(ref _entities, (int)maximum);
        }

        internal ushort GetAssociationBits(byte entity)
        {
            //Debug.Assert(_entities[entity].EntryUsed);
            return _entities[entity].AssociationBits;
        }

        internal uint GetSheetFromEntity(byte entity) { return _entities[entity].Sheet; }

        internal bool IsUsed(byte entity)
        {
            return _entities[entity].EntryUsed;
        }

        internal uint Sheet(byte entity)
        {
            Debug.Assert(_entities[entity].EntryUsed);
            return _entities[entity].Sheet;
        }

        internal void SetAssociationBits(byte slot, ushort associationBits)
        {
            _entities[slot].AssociationBits = associationBits;
        }

        internal void AddEntity(byte entity, uint sheet)
        {
            Debug.Assert(entity < _entities.Length && !_entities[entity].EntryUsed);

            _entities[entity].EntryUsed = true;
            _entities[entity].Sheet = sheet;
        }

        internal bool RemoveEntity(byte entity)
        {
            Debug.Assert(entity < _entities.Length, $"entity={(ushort)entity}u size={_entities.Length}");

            //Workaround: assert converted to test when failure in vision from the server
            //if (!_entities[entity].EntryUsed)
            //    return true;

            //_entities[entity].EntryUsed = false;
            //_entities[entity].Sheet = 0xffff;

            _entities[entity] = new EntityEntry();

            return true;
        }

        /// <summary>
        /// Receives single action from the front end. Actually transmits action received
        /// by the client to the property decoder.
        /// </summary>
        /// <param name="_">the number of the packet received</param>
        /// <param name="action">action the action sent to the client by the front end</param>
        internal void Receive(int _, ActionPosition action)
        {
            if (!action.IsContinuous())
                return;

            if (action.Code == ActionCode.ActionPositionCode)
            {
                var act = action;

                if (act.IsRelative)
                {
                    // Relative position (entity in a ferry...)
                    act.Position[0] = act.Position16[0];
                    act.Position[1] = act.Position16[1];
                    act.Position[2] = act.Position16[2];
                    _entities[act.Slot].PosIsRelative = true;
                    _entities[act.Slot].PosIsInterior = false;
                    RyzomClient.GetInstance().GetLogger().Debug($"Relative Pos: {act.Position[0]} {act.Position[1]} {act.Position[2]} Pos16: {act.Position16[0]} {act.Position16[1]} {act.Position16[2]} Date {act.GameCycle}");
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
                    _entities[act.Slot].PosIsRelative = false;
                    _entities[act.Slot].PosIsInterior = act.Interior;
                    //nlinfo( "Slot %hu: Absolute, Pos: %d %d %d, Pos16: %hu %hu %hu, Date %u", (uint16)act->CLEntityId, (int)act->Position[0], (int)act->Position[1], (int)act->Position[2], act->Position16[0], act->Position16[1], act->Position16[2], act->GameCycle );
                    //nlinfo( "            RefPos: %d %d %d, RefBits: %hu %hu %hu", _RefPosX, _RefPosY, _RefPosZ, _RefBitsX, _RefBitsY, _RefBitsZ );
                }
            }
        }

        /// <summary>
        /// Decode x and y
        /// </summary>
        internal void DecodeAbsPos2D(ref int x, ref int y, ushort x16, ushort y16)
        {
            x = _refPosX + ((short)(x16 - _refBitsX) << 4);
            y = _refPosY + ((short)(y16 - _refBitsY) << 4);
        }

        /// <summary>
        /// Set player's reference position
        /// </summary>
        public void SetReferencePosition(Vector3 position)
        {
            _refPosX = (int)(position.X * 1000.0) & ~0xf; // correction here by Sadge: clear low-order to prevent flickering depending on player's reference position
            _refPosY = (int)(position.Y * 1000.0) & ~0xf; // correction here by Sadge
            //_RefPosZ = (sint32)(position.z * 1000.0);

            _refBitsX = (ushort)((_refPosX >> 4) & 0xffff);
            _refBitsY = (ushort)((_refPosY >> 4) & 0xffff);
            //_RefBitsZ = (uint16)(_RefPosZ >> 4);
        }

        public Vector3 GetReferencePosition()
        {
            return new Vector3(_refPosX / 1000f, _refPosY / 1000f, 0);
        }
    }
}