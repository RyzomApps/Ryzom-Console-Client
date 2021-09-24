///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

namespace RCC.Entity
{
    internal class VpNodeBase
    {
        internal VpNodeBase VPParent;
        internal VpNodeBase VPA;
        internal VpNodeBase VPB;
        internal byte PropIndex;
        internal bool BranchHasPayload;

        /// <summary>
        /// Constructor
        /// </summary>
        internal VpNodeBase()
        {
            VPParent = null;
            VPA = null;
            VPB = null;
            PropIndex = (byte.MaxValue);
            BranchHasPayload = (false);
        }

        /// <summary>
        /// Return true if the node is leaf of a tree
        /// </summary>
        internal virtual bool IsLeaf() { return PropIndex != byte.MaxValue; }

        /// <summary>
        /// Number of visual properties
        /// </summary>
        const uint NB_VISUAL_PROPERTIES = 28;

        /// <summary>
        /// Return the number of visual properties
        /// </summary>
        internal uint BuildTree()
        {
            MakeChildren();
            //setNodePropIndex(POSITION);
            //VPB.makeChildren();
            //setNodePropIndex(ORIENTATION);
            //
            //TVPNodeBase discreetRoot = VPB.VPB;
            //discreetRoot.makeDescendants(3); // 8 leaves + those created by additional makeChildren()
            //discreetRoot.setNodePropIndex(SHEET);
            //discreetRoot.VPA.VPA.VPB.makeChildren();
            //discreetRoot.setNodePropIndex(BEHAVIOUR);
            //discreetRoot.setNodePropIndex(OWNER_PEOPLE);
            //discreetRoot.VPA.VPB.VPA.makeChildren();
            //discreetRoot.setNodePropIndex(NAME_STRING_ID);
            //discreetRoot.setNodePropIndex(CONTEXTUAL);
            //discreetRoot.VPA.VPB.VPB.makeChildren();
            //discreetRoot.setNodePropIndex(TARGET_LIST);
            //discreetRoot.setNodePropIndex(TARGET_ID);
            //discreetRoot.VPB.VPA.VPA.makeChildren();
            //discreetRoot.setNodePropIndex(MODE);
            //discreetRoot.setNodePropIndex(VPA);
            //discreetRoot.VPB.VPA.VPB.makeChildren();
            //discreetRoot.setNodePropIndex(BARS);
            //discreetRoot.setNodePropIndex(VISUAL_FX);
            //discreetRoot.VPB.VPB.VPA.makeChildren();
            //discreetRoot.setNodePropIndex(VPB);
            //discreetRoot.VPB.VPB.VPA.VPB.makeChildren();
            //discreetRoot.setNodePropIndex(VPC);
            //discreetRoot.VPB.VPB.VPA.VPB.VPB.makeChildren();
            //discreetRoot.setNodePropIndex(EVENT_FACTION_ID);
            //discreetRoot.VPB.VPB.VPA.VPB.VPB.VPB.makeChildren();
            //discreetRoot.setNodePropIndex(PVP_MODE);
            //discreetRoot.setNodePropIndex(PVP_CLAN);
            //discreetRoot.VPB.VPB.VPB.makeChildren();
            //discreetRoot.VPB.VPB.VPB.VPA.makeChildren();
            //discreetRoot.setNodePropIndex(ENTITY_MOUNTED_ID);
            //discreetRoot.setNodePropIndex(RIDER_ENTITY_ID);
            //discreetRoot.VPB.VPB.VPB.VPB.makeChildren();
            //discreetRoot.setNodePropIndex(OUTPOST_INFOS);
            //discreetRoot.VPB.VPB.VPB.VPB.VPB.makeChildren();
            //discreetRoot.setNodePropIndex(GUILD_SYMBOL);
            //discreetRoot.setNodePropIndex(GUILD_NAME_ID);

            return NB_VISUAL_PROPERTIES;
        }

        internal virtual void MakeChildren()
        {
            VPA = new VpNodeClient { VPParent = this };
            VPB = new VpNodeClient { VPParent = this };
        }

        internal virtual void MakeDescendants(uint nbLevels)
        {
            MakeChildren();

            if (nbLevels > 1)
            {
                VPA.MakeDescendants(nbLevels - 1);
                VPB.MakeDescendants(nbLevels - 1);
            }
        }
    }
}