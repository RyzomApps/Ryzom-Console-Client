///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using Client.Network;
using Client.Stream;

namespace Client.Property
{
    internal class VisualPropertyNodeClient : VisualPropertyNode
    {
        /// <summary>Static data</summary>
        internal static readonly SlotContext SlotContext = new SlotContext();

        internal VisualPropertyNodeClient A() { return (VisualPropertyNodeClient)VpA; }

        internal VisualPropertyNodeClient B() { return (VisualPropertyNodeClient)VpB; }

        internal VisualPropertyNodeClient Parent() { return (VisualPropertyNodeClient)VpParent; }

        internal void DecodeDiscreteProperties(BitMemoryStream msgin)
        {
            msgin.Serial(ref BranchHasPayload);

            if (!BranchHasPayload) return;

            if (IsLeaf())
            {
                SlotContext.NetworkConnection.DecodeDiscreteProperty(msgin, PropIndex);
            }
            else
            {
                if (A() != null) A().DecodeDiscreteProperties(msgin);
                if (B() != null) B().DecodeDiscreteProperties(msgin);
            }
        }

        private void DeleteBranches()
        {
            DeleteA();
            DeleteB();
        }

        private void DeleteA()
        {
            if (A() == null) return;

            A().DeleteBranches();
            VpA = null;
        }

        private void DeleteB()
        {
            if (B() == null) return;

            B().DeleteBranches();
            VpB = null;
        }
    }
}
