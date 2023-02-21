///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

namespace Client.Network.Action
{
    /// <summary>
    /// Action Nature
    /// </summary>
    public enum TargettingType
    {
        None = 0,
        Lootable = 1,
        Harvestable = 2,
        LootableHarvestable = 3
    }
}