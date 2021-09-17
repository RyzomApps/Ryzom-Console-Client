// This code is a modified version of a file from the 'Ryzom - MMORPG Framework'
// <http://dev.ryzom.com/projects/ryzom/>,
// which is released under GNU Affero General Public License.
// <http://www.gnu.org/licenses/>
// Original Copyright 2010 by Winch Gate Property Limited

using RCC.Network;

namespace RCC.NetworkAction
{
    public abstract class Action
    {
        public ActionCode Code { get; internal set; }
        public ActionCode PropertyCode { get; internal set; }
        public byte Slot { get; internal set; }

        public abstract void unpack(BitMemoryStream message);

        public abstract int size();

        public abstract void reset();

        public abstract void pack(BitMemoryStream message);
    }
}