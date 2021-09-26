///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Minecraft Console Client'
// https://github.com/ORelio/Minecraft-Console-Client
// which is released under CDDL-1.0 License
// http://opensource.org/licenses/CDDL-1.0
// Copyright 2021 ORelio and Contributers
///////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;

namespace RCC.Database
{

    /// <summary>
	/// Database Node which contains a set of properties
	/// \author Stephane Coutelas
	/// \author Nevrax France
	/// \date 2002
	/// </summary>
    public partial class CDBNodeBranch : ICDBNode
    {

        // default ructor
        CDBNodeBranch(string name) //: ICDBNode(name)
        {
            _Parent = null;
            _IdBits = 0;
            _Sorted = false;
        }

        /// <summary>
        /// Build the structure of the database from a file
        /// \param f is the stream
        /// </summary>
        //void init(xmlNodePtr node, IProgressCallback progressCallBack, bool mapBanks = false, CCDBBankHandler bankHandler = null );

        /// <summary>
        /// Add a new sub node
        /// \param node is the new subnode
        /// \param nodeName is the name of the node
        /// </summary>
        void attachChild(ICDBNode node, string nodeName) { }

        /// <summary>
        /// Get a node . Create it if it does not exist yet
        /// \param id : the CTextId identifying the node
        /// </summary>
        //ICDBNode getNode(CTextId id, bool bCreate = true) { return null; }

        /// <summary>
        /// Get a node. Return null if out of bounds (no warning)
        /// \param idx is the node index
        /// </summary>
        ICDBNode getNode(ushort idx) { return null; }

        /// <summary>
        /// Get a node index
        /// \param node is a pointer to the node
        /// </summary>
        public virtual bool getNodeIndex(ICDBNode node, uint index)
        {
            //index = 0;
            //for (List<ICDBNode> _iterator it = _Nodes.begin(); it != _Nodes.end(); it++)
            //{
            //    if (it == node)
            //        return true;
            //    index++;
            //}
            return false;
        }

        // return the child with the given node id, creating it if requested
        public CCDBNodeLeaf getLeaf(char id, bool bCreate) { return null; }

        public CCDBNodeLeaf getLeaf(string id, bool bCreate) { return getLeaf(id[0], bCreate); }

        /// <summary>
        /// Save a backup of the database
        /// \param id is the text id of the property/grp
        /// \param f is the stream
        /// </summary>
        //void write(CTextId id, FILE f);

        /// Update the database from the delta, but map the first level with the bank mapping (see _CDBBankToUnifiedIndexMapping)
        //void readAndMapDelta(TGameCycle gc, CBitMemStream s, uint bank, CCDBBankHandler bankHandler);

        /// Update the database from a stream coming from the FE
        //void readDelta(TGameCycle gc, CBitMemStream f);

        /// <summary>
        /// Return the value of a property (the update flag is set to false)
        /// \param id is the text id of the property/grp
        /// \param name is the name of the property
        /// \return the value of the property
        /// </summary>
        //long getProp(CTextId id);

        /// <summary>
        /// Set the value of a property (the update flag is set to true)
        /// \param id is the text id of the property/grp
        /// \param name is the name of the property
        /// \param value is the value of the property
        /// \return bool : 'true' if property found.
        /// </summary>
        //bool setProp(CTextId id, long value);

        /// Clear the node and his children
        void clear() { }

        //void resetNode(TGameCycle gc, uint node)
        //{
        //    if (node > _Nodes.size())
        //        return;
        //
        //    _Nodes[node]->resetData(gc);
        //}

        /// Reset all leaf data from this point
        //void resetData(TGameCycle gc, bool forceReset = false)
        //{
        //    for (uint i = 0; i != _Nodes.size(); ++i)
        //    {
        //        _Nodes[i]->resetData(gc, forceReset);
        //    }
        //}

        /// <summary>
        ///	Destructor
        /// </summary>
        ~CDBNodeBranch() { clear(); }

        // the parent node for a branch (null by default)
        public virtual void setParent(CDBNodeBranch parent) { _Parent = parent; }

        public virtual CDBNodeBranch getParent()
        {
            return _Parent;
        }

        //get the number of nodes
        public ushort getNbNodes()
        {
            return (ushort)_Nodes.Count;
        }

        /// Count the leaves
        public virtual uint countLeaves() { return 0; }

        /// Find the leaf which count is specified (if found, the returned value is non-null and count is 0)
        public virtual CCDBNodeLeaf findLeafAtCount(uint count) { return null; }

        public virtual void display(string prefix) { }

        //void removeNode(CTextId id);

        /// <summary>
        /// add an observer to a property
        /// \param observer : pointer to an observer
        /// \param id text id identifying the property
        /// \return false if the node doen t exist
        /// </summary>
        //virtual bool addObserver(IPropertyObserver observer, CTextId id);

        /// <summary> remove an obsever
        /// \param observer : pointer to an observer
        /// \return false if the node or observer doesn t exist
        /// </summary>
        //virtual bool removeObserver(IPropertyObserver observer, CTextId id);

        // Add an observer to this branch. It will be notified of any change in the sub-leaves

        /// <summary>
        /// Add observer to all sub-leaves, except if a positive filter is set:
        /// If positiveLeafNameFilter is non-empty, only changes to leaves having names found in it
        /// will be notified (this is equivalent to creating a sub-branch containing only the specified leaves
        /// and setting a branch observer on it, except you don't need to change your database paths
        /// and update large amounts of code!).
        /// </summary>
        //void addBranchObserver(ICDBDBBranchObserverHandle handle, List<string> positiveLeafNameFilter = List<string>());

        /// <summary>
        /// Easy version of addBranchObserver() (see above).
        /// Examples of dbPathFromThisNode:
        /// "" -> this node
        /// "FOO:BAR" ->  sub-branch "BAR" of "FOO" which is a sub-branch of this node
        /// </summary>
        //void addBranchObserver(ICDBDBBranchObserverHandle handle, char dbPathFromThisNode, char positiveLeafNameFilter = null, uint positiveLeafNameFilterSize = 0);

        // Remove observer from all sub-leaves
        //bool removeBranchObserver(IPropertyObserver observer);

        /// Easy version of removeBranchObserver() (see above and see easy version of addBranchObserver())
        //void removeBranchObserver(char dbPathFromThisNode, ICDBNodeIPropertyObserver observer);

        public virtual bool isLeaf() { return false; }

        // mark this branch and parent branch as 'modified'. This is usually called by sub-leaves
        //void onLeafChanged(TStringId leafName);

        /// Find a subnode at this level
        //ICDBNode find(string nodeName);

        //typedef List<ICDBDBBranchObserverHandle > TObserverHandleList;

        CDBNodeBranch _Parent;

        /// database subnodes not sorted
        List<ICDBNode> _Nodes;

        /// subnodes sorted by name
        List<ICDBNode> _NodesByName;

        // number of bits required to stock my children's ids
        byte _IdBits = 7;
        bool _Sorted = true;

        // observers for this node or branch
        //TObserverHandleList observerHandles;

        /// called by clear
        void removeAllBranchObserver() { }

        //#if NL_CDB_OPTIMIZE_PREDICT
        //		CRefPtr<ICDBNode>		_PredictNode;
        //#endif
    }
}
