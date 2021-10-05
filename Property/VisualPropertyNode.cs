///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

namespace RCC.Property
{
    internal class VisualPropertyNode
    {
        internal VisualPropertyNode VpParent;
        internal VisualPropertyNode VpA;
        internal VisualPropertyNode VpB;
        internal byte PropIndex;
        internal bool BranchHasPayload;

        /// <summary>
        /// Constructor
        /// </summary>
        internal VisualPropertyNode()
        {
            VpParent = null;
            VpA = null;
            VpB = null;
            PropIndex = byte.MaxValue;
            BranchHasPayload = false;
        }

        /// <summary>
        /// Return true if the node is leaf of a tree
        /// </summary>
        internal virtual bool IsLeaf()
        {
            return PropIndex != byte.MaxValue;
        }

        /// <summary>
        /// Number of visual properties
        /// </summary>
        public const uint NbVisualProperties = 28;

        // From the main root
        private VisualPropertyNode GetPositionNode() { return VpA; }
        private VisualPropertyNode GetOrientationNode() { return VpB.VpA; }

        // From the discrete root (mainroot.VPB.VPB)
        private VisualPropertyNode GetSheetNode() { return VpA.VpA.VpA; }
        private VisualPropertyNode GetBehaviourNode() { return VpA.VpA.VpB.VpA; }
        private VisualPropertyNode GetOwnerPeopleNode() { return VpA.VpA.VpB.VpB; }
        private VisualPropertyNode GetNameStringIdNode() { return VpA.VpB.VpA.VpA; }
        private VisualPropertyNode GetContextualNode() { return VpA.VpB.VpA.VpB; }
        private VisualPropertyNode GetTargetListNode() { return VpA.VpB.VpB.VpA; }
        private VisualPropertyNode GetTargetIdNode() { return VpA.VpB.VpB.VpB; }
        private VisualPropertyNode GetModeNode() { return VpB.VpA.VpA.VpA; }
        private VisualPropertyNode GetVpaNode() { return VpB.VpA.VpA.VpB; }
        private VisualPropertyNode GetBarsNode() { return VpB.VpA.VpB.VpA; }
        private VisualPropertyNode GetVisualFxNode() { return VpB.VpA.VpB.VpB; }
        private VisualPropertyNode GetVpbNode() { return VpB.VpB.VpA.VpA; }
        private VisualPropertyNode GetVpcNode() { return VpB.VpB.VpA.VpB.VpA; }
        private VisualPropertyNode GetEventFactionInNode() { return VpB.VpB.VpA.VpB.VpB.VpA; }
        private VisualPropertyNode GetPvpModeNode() { return VpB.VpB.VpA.VpB.VpB.VpB.VpA; }
        private VisualPropertyNode GetPvpClanNode() { return VpB.VpB.VpA.VpB.VpB.VpB.VpB; }
        private VisualPropertyNode GetEntityMountedIdNode() { return VpB.VpB.VpB.VpA.VpA; }
        private VisualPropertyNode GetRiderEntityIdNode() { return VpB.VpB.VpB.VpA.VpB; }
        private VisualPropertyNode GetOutpostInfosNode() { return VpB.VpB.VpB.VpB.VpA; }
        private VisualPropertyNode GetGuildSymbolNode() { return VpB.VpB.VpB.VpB.VpB.VpA; }
        private VisualPropertyNode GetGuildNameIdNode() { return VpB.VpB.VpB.VpB.VpB.VpB; }

        /// <summary>
        /// Return the number of visual properties
        /// </summary>
        internal uint BuildTree()
        {
            MakeChildren();
            SetNodePropIndex(GetPositionNode(), (byte)PropertyType.Position);

            VpB.MakeChildren();
            SetNodePropIndex(GetOrientationNode(), (byte)PropertyType.Orientation);

            var discreetRoot = VpB.VpB;
            discreetRoot.MakeDescendants(3); // 8 leaves + those created by additional MakeChildren()
            SetNodePropIndex(discreetRoot.GetSheetNode(), (byte)PropertyType.Sheet);

            discreetRoot.VpA.VpA.VpB.MakeChildren();
            SetNodePropIndex(discreetRoot.GetBehaviourNode(), (byte)PropertyType.Behaviour);
            SetNodePropIndex(discreetRoot.GetOwnerPeopleNode(), (byte)PropertyType.OwnerPeople);

            discreetRoot.VpA.VpB.VpA.MakeChildren();
            SetNodePropIndex(discreetRoot.GetNameStringIdNode(), (byte)PropertyType.NameStringID);
            SetNodePropIndex(discreetRoot.GetContextualNode(), (byte)PropertyType.Contextual);

            discreetRoot.VpA.VpB.VpB.MakeChildren();
            SetNodePropIndex(discreetRoot.GetTargetListNode(), (byte)PropertyType.TargetList);
            SetNodePropIndex(discreetRoot.GetTargetIdNode(), (byte)PropertyType.TargetID);

            discreetRoot.VpB.VpA.VpA.MakeChildren();
            SetNodePropIndex(discreetRoot.GetModeNode(), (byte)PropertyType.Mode);
            SetNodePropIndex(discreetRoot.GetVpaNode(), (byte)PropertyType.Vpa);

            discreetRoot.VpB.VpA.VpB.MakeChildren();
            SetNodePropIndex(discreetRoot.GetBarsNode(), (byte)PropertyType.Bars);
            SetNodePropIndex(discreetRoot.GetVisualFxNode(), (byte)PropertyType.VisualFx);

            discreetRoot.VpB.VpB.VpA.MakeChildren();
            SetNodePropIndex(discreetRoot.GetVpbNode(), (byte)PropertyType.Vpb);

            discreetRoot.VpB.VpB.VpA.VpB.MakeChildren();
            SetNodePropIndex(discreetRoot.GetVpcNode(), (byte)PropertyType.Vpc);

            discreetRoot.VpB.VpB.VpA.VpB.VpB.MakeChildren();
            SetNodePropIndex(discreetRoot.GetEventFactionInNode(), (byte)PropertyType.EventFactionID);

            discreetRoot.VpB.VpB.VpA.VpB.VpB.VpB.MakeChildren();
            SetNodePropIndex(discreetRoot.GetPvpModeNode(), (byte)PropertyType.PvpMode);
            SetNodePropIndex(discreetRoot.GetPvpClanNode(), (byte)PropertyType.PvpClan);

            discreetRoot.VpB.VpB.VpB.MakeChildren();
            discreetRoot.VpB.VpB.VpB.VpA.MakeChildren();
            SetNodePropIndex(discreetRoot.GetEntityMountedIdNode(), (byte)PropertyType.EntityMountedID);
            SetNodePropIndex(discreetRoot.GetRiderEntityIdNode(), (byte)PropertyType.RiderEntityID);

            discreetRoot.VpB.VpB.VpB.VpB.MakeChildren();
            SetNodePropIndex(discreetRoot.GetOutpostInfosNode(), (byte)PropertyType.OutpostInfos);

            discreetRoot.VpB.VpB.VpB.VpB.VpB.MakeChildren();
            SetNodePropIndex(discreetRoot.GetGuildSymbolNode(), (byte)PropertyType.GuildSymbol);
            SetNodePropIndex(discreetRoot.GetGuildNameIdNode(), (byte)PropertyType.GuildNameID);

            return NbVisualProperties;
        }

        private static void SetNodePropIndex(VisualPropertyNode node, byte property)
        {
            node.PropIndex = property;
        }

        internal virtual void MakeChildren()
        {
            VpA = new VisualPropertyNodeClient { VpParent = this };
            VpB = new VisualPropertyNodeClient { VpParent = this };
        }

        internal virtual void MakeDescendants(uint nbLevels)
        {
            MakeChildren();

            if (nbLevels <= 1) return;

            VpA.MakeDescendants(nbLevels - 1);
            VpB.MakeDescendants(nbLevels - 1);
        }
    }
}