///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using System;
using System.Xml;
using RCC.Network;

namespace RCC.Database
{
    public abstract class DatabaseNodeBase
    {
        // ReSharper disable InconsistentNaming
        internal enum EPropType
        {
            UNKNOWN = 0,
            // Unsigned
            I1, I2, I3, I4, I5, I6, I7, I8, I9, I10, I11, I12, I13, I14, I15, I16,
            I17, I18, I19, I20, I21, I22, I23, I24, I25, I26, I27, I28, I29, I30, I31, I32,
            I33, I34, I35, I36, I37, I38, I39, I40, I41, I42, I43, I44, I45, I46, I47, I48,
            I49, I50, I51, I52, I53, I54, I55, I56, I57, I58, I59, I60, I61, I62, I63, I64,
            // Signed
            S1, S2, S3, S4, S5, S6, S7, S8, S9, S10, S11, S12, S13, S14, S15, S16,
            S17, S18, S19, S20, S21, S22, S23, S24, S25, S26, S27, S28, S29, S30, S31, S32,
            S33, S34, S35, S36, S37, S38, S39, S40, S41, S42, S43, S44, S45, S46, S47, S48,
            S49, S50, S51, S52, S53, S54, S55, S56, S57, S58, S59, S60, S61, S62, S63, S64,
            TEXT, Nb_Prop_Type
        }

        internal string Name;

        internal DatabaseNodeBranch Parent;

        /// <summary>is the branch an atomic group, or is the leaf a member of an atomic group</summary>
        private bool _atomicFlag;

        /// <summary>
        /// Build the structure of the database from a file
        /// </summary>
        internal abstract void Init(XmlElement child, Action progressCallBack, bool mapBanks = false, BankHandler bankHandler = null);

        /// <summary>
        /// Update the database from a stream coming from the FE
        /// </summary>
        /// <param name="gc">the server gameCycle of this update. Any outdated update are aborted</param>
        /// <param name="f">the stream.</param>
        internal abstract void ReadDelta(uint gc, BitMemoryStream f);

        /// <summary>
        /// Get a databaseNode
        /// </summary>
        /// <returns>idx is the databaseNode index</returns>
        internal abstract DatabaseNodeBase GetNode(ushort idx);

        /// <summary>
        /// Get a databaseNode index
        /// </summary>
        /// <param name="databaseNode">databaseNode is a pointer to the databaseNode</param>
        /// <param name="index">index is a reference that receive the result</param>
        /// <returns>true if the databaseNode was found</returns>
        internal abstract bool GetNodeIndex(DatabaseNodeBase databaseNode, ref uint index);

        /// <summary>
        /// Return the value of a property (the update flag is set to false)
        /// </summary>
        /// <param name="id">is the text id of the property/grp</param>
        /// <returns>the value of the property</returns>
        internal abstract long GetProp(TextId id);

        /// <summary>
        /// get the parent of a databaseNode
        /// </summary>
        internal virtual DatabaseNodeBranch GetParent() { return Parent; }

        /// <summary>
        /// get the name of this databaseNode
        /// </summary>
        internal virtual string GetName() { return Name; }

        /// <summary>
        /// Find the leaf which count is specified (if found, the returned value is non-null and count is 0)
        /// </summary>
        internal abstract DatabaseNodeLeaf FindLeafAtCount(ref uint count);

        /// <summary>
        /// Set the atomic branch flag (when all the modified nodes of a branch should be tranmitted at the same time)
        /// </summary>
        internal virtual void SetAtomic(bool atomicBranch) { _atomicFlag = atomicBranch; }

        /// <summary>
        /// Return true if the branch has the atomic flag
        /// </summary>
        internal virtual bool IsAtomic() { return _atomicFlag; }

        /// <summary>
        /// Inform a databaseNode of its parenthood
        /// </summary>
        internal virtual void SetParent(DatabaseNodeBranch parent) { Parent = parent; }

        /// <summary>
        /// Get a databaseNode . Create it if it does not exist yet
        /// </summary>
        /// <params id="id">the TextId identifying the databaseNode</params>
        internal abstract DatabaseNodeBase GetNode(TextId id, bool bCreate = true);

        /// <summary>
        /// Add a new sub node
        /// </summary>
        /// <params id="node">is the new subnode</params>
        /// <params id="nodeName">is the name of the node</params>
        internal abstract void AttachChild(DatabaseNodeBase node, string nodeName);

        /// <summary>
        /// Reset all leaf data from this point
        /// </summary>
        internal abstract void ResetData(uint gc, bool forceReset = false);

        /// <summary>
        /// Count the leaves
        /// </summary>
        internal abstract uint CountLeaves();
    }
}