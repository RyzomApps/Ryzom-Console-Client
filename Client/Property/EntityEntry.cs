///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

namespace RCC.Property
{
    /// <summary>
    /// An entity entry, containing properties for this entity
    /// </summary>
    public class EntityEntry
    {
        public uint Sheet = new uint();
        public ushort AssociationBits;
        public bool EntryUsed;
        public bool PosIsRelative;
        public bool PosIsInterior;

        public EntityEntry()
        {
            AssociationBits = 0;
            EntryUsed = false;
            PosIsRelative = false;
            PosIsInterior = false;
        }
    }
}