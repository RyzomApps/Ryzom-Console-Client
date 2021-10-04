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
        internal VpNodeBase VpParent;
        internal VpNodeBase VpA;
        internal VpNodeBase VpB;
        internal byte PropIndex;
        internal bool BranchHasPayload;

        /// <summary>
        /// Constructor
        /// </summary>
        internal VpNodeBase()
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

        // TODO: is VpNodeBase.PropertyType the same as Entity.Prop?
        public enum PropertyType : byte
        {
            // main root
            Position = 0,
            Orientation = 3,
            // discrete root
            Sheet = 4,
            Behaviour = 5,
            NameStringID = 6,
            TargetID = 7,
            Mode = 8,
            Vpa = 9,
            Vpb = 10,
            Vpc = 11,
            EntityMountedID = 12,
            RiderEntityID = 13,
            Contextual = 14,
            Bars = 15,
            TargetList = 16,
            GuildSymbol = 20,
            GuildNameID = 21,
            VisualFx = 22,
            EventFactionID = 23,
            PvpMode = 24,
            PvpClan = 25,
            OwnerPeople = 26,
            OutpostInfos = 27,
        }

        // From the main root
        private VpNodeBase GetPositionNode() { return VpA; }
        private VpNodeBase GetOrientationNode() { return VpB.VpA; }

        // From the discrete root (mainroot.VPB.VPB)
        private VpNodeBase GetSheetNode() { return VpA.VpA.VpA; }
        private VpNodeBase GetBehaviourNode() { return VpA.VpA.VpB.VpA; }
        private VpNodeBase GetOwnerPeopleNode() { return VpA.VpA.VpB.VpB; }
        private VpNodeBase GetNameStringIdNode() { return VpA.VpB.VpA.VpA; }
        private VpNodeBase GetContextualNode() { return VpA.VpB.VpA.VpB; }
        private VpNodeBase GetTargetListNode() { return VpA.VpB.VpB.VpA; }
        private VpNodeBase GetTargetIdNode() { return VpA.VpB.VpB.VpB; }
        private VpNodeBase GetModeNode() { return VpB.VpA.VpA.VpA; }
        private VpNodeBase GetVpaNode() { return VpB.VpA.VpA.VpB; }
        private VpNodeBase GetBarsNode() { return VpB.VpA.VpB.VpA; }
        private VpNodeBase GetVisualFxNode() { return VpB.VpA.VpB.VpB; }
        private VpNodeBase GetVpbNode() { return VpB.VpB.VpA.VpA; }
        private VpNodeBase GetVpcNode() { return VpB.VpB.VpA.VpB.VpA; }
        private VpNodeBase GetEventFactionInNode() { return VpB.VpB.VpA.VpB.VpB.VpA; }
        private VpNodeBase GetPvpModeNode() { return VpB.VpB.VpA.VpB.VpB.VpB.VpA; }
        private VpNodeBase GetPvpClanNode() { return VpB.VpB.VpA.VpB.VpB.VpB.VpB; }
        private VpNodeBase GetEntityMountedIdNode() { return VpB.VpB.VpB.VpA.VpA; }
        private VpNodeBase GetRiderEntityIdNode() { return VpB.VpB.VpB.VpA.VpB; }
        private VpNodeBase GetOutpostInfosNode() { return VpB.VpB.VpB.VpB.VpA; }
        private VpNodeBase GetGuildSymbolNode() { return VpB.VpB.VpB.VpB.VpB.VpA; }
        private VpNodeBase GetGuildNameIdNode() { return VpB.VpB.VpB.VpB.VpB.VpB; }

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

        private static void SetNodePropIndex(VpNodeBase node, byte property)
        {
            node.PropIndex = property;
        }

        internal virtual void MakeChildren()
        {
            VpA = new VpNodeClient { VpParent = this };
            VpB = new VpNodeClient { VpParent = this };
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