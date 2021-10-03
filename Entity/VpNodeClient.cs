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
        static readonly TSlotContext SlotContext;

        internal VpNodeClient() : base() { }

        internal VpNodeClient A() { return (VpNodeClient)VPA; }
        internal VpNodeClient B() { return (VpNodeClient)VPB; }
        internal VpNodeClient Parent() { return (VpNodeClient)VPParent; }

        void DecodeDiscreetProperties(BitMemoryStream msgin)
        {
            msgin.Serial(ref BranchHasPayload);

            if (BranchHasPayload)
            {
                if (IsLeaf())
                {
                    //SlotContext.NetworkConnection.decodeDiscreetProperty(msgin, PropIndex);
                }
                else
                {
                    if (A() != null) A().DecodeDiscreetProperties(msgin);
                    if (B() != null) B().DecodeDiscreetProperties(msgin);
                }
            }
        }

        void DeleteBranches()
        {
            if (A() != null)
            {
                A().DeleteBranches();
                VPA = null;
                //delete a();
            }
            if (B() != null)
            {
                B().DeleteBranches();
                VPB = null;
                //delete b();
            }
        }

        void DeleteA()
        {
            if (A() != null)
            {
                A().DeleteBranches();
                VPA = null;
                //delete a();
            }
        }

        void DeleteB()
        {
            if (B() != null)
            {
                B().DeleteBranches();
                VPB = null;
                //delete b();
            }
        }
    }
}
