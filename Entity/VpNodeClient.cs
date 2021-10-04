///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using RCC.Network;

namespace RCC.Entity
{
    internal class VpNodeClient : VpNodeBase
    {
        internal class TSlotContext
        {
            internal NetworkConnection NetworkConnection;
            internal byte Slot;
            internal uint Timestamp;
        }

        // Static data
        internal static readonly TSlotContext SlotContext = new TSlotContext();

        internal VpNodeClient() : base() { }

        internal VpNodeClient A() { return (VpNodeClient)VpA; }
        internal VpNodeClient B() { return (VpNodeClient)VpB; }
        internal VpNodeClient Parent() { return (VpNodeClient)VpParent; }

        internal void DecodeDiscreetProperties(BitMemoryStream msgin)
        {
            msgin.Serial(ref BranchHasPayload);

            if (!BranchHasPayload) return;

            if (IsLeaf())
            {
                SlotContext.NetworkConnection.DecodeDiscreetProperty(msgin, PropIndex);
            }
            else
            {
                if (A() != null) A().DecodeDiscreetProperties(msgin);
                if (B() != null) B().DecodeDiscreetProperties(msgin);
            }
        }

        private void DeleteBranches()
        {
            DeleteA();
            DeleteB();
        }

        void DeleteA()
        {
            if (A() != null)
            {
                A().DeleteBranches();
                VpA = null;
                //delete a();
            }
        }

        void DeleteB()
        {
            if (B() != null)
            {
                B().DeleteBranches();
                VpB = null;
                //delete b();
            }
        }
    }
}
