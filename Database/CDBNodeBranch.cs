﻿///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;

namespace RCC.Database
{
    /// <summary>
    /// Database Node which contains a set of properties
    /// </summary>
    /// <author>Stephane Coutelas</author>
    /// <author>Nevrax France</author>
    /// <date>2002</date>
    public class CDBNodeBranch : ICDBNode
    {
        /// <summary>database subnodes not sorted</summary>
        public List<ICDBNode> _Nodes = new List<ICDBNode>();

        /// <summary>subnodes sorted by name</summary>
        public List<ICDBNode> _NodesByName = new List<ICDBNode>();

        /// <summary>default constructor</summary>
        public CDBNodeBranch(string name)
        {
            base.name = name;
            _Parent = null;
            _IdBits = 0;
            _Sorted = false;
        }

        internal override void Init(XmlElement child, Action progressCallBack)
        {
            Init(child, progressCallBack, false, null);
        }

        /// <summary>
        /// Build the structure of the database from a file
        /// </summary>
        /// <params id="f">is the stream</params>
        public void Init(XmlElement node, Action progressCallBack, bool mapBanks = false, CDBBankHandler bankHandler = null)
        {
            //XmlElement child; // = new XmlElement();

            _Sorted = false;
            // look for other branches within this branch
            //uint countNode = CIXml.countChildren(node, "branch") + CIXml.countChildren(node, "leaf");
            uint nodeId = 0;

            //for (child = CIXml.getFirstChildNode(node, "branch"); child; child = CIXml.getNextChildNode(child, "branch"))
            foreach (var subNode in node.SelectNodes("branch"))
            {
                var child = subNode as XmlElement;

                if (child == null)
                    continue;

                // Progress bar
                //progressCallBack.progress((float)nodeId / (float)countNode);
                //progressCallBack.pushCropedValues((float)nodeId / (float)countNode, (float)(nodeId + 1) / (float)countNode);

                //CXMLAutoPtr name = new CXMLAutoPtr((string)xmlGetProp(child, (xmlChar)"name"));
                //CXMLAutoPtr count = new CXMLAutoPtr((string)xmlGetProp(child, (xmlChar)"count"));
                //CXMLAutoPtr bank = new CXMLAutoPtr((string)xmlGetProp(child, (xmlChar)"bank"));
                //CXMLAutoPtr atom = new CXMLAutoPtr((string)xmlGetProp(child, (xmlChar)"atom"));
                //CXMLAutoPtr clientonly = new CXMLAutoPtr((string)xmlGetProp(child, (xmlChar)"clientonly"));

                string sBank = child.GetAttribute("bank");
                string sAtom = child.GetAttribute("atom");
                string sClientonly = child.GetAttribute("clientonly");
                string count = child.GetAttribute("count");
                string name = child.GetAttribute("name");

                //if (bank != null)
                //{
                //    sBank = bank.getDatas();
                //}
                //if (atom != null)
                //{
                //    sAtom = (string)atom;
                //}
                //if (clientonly != null)
                //{
                //    sClientonly = clientonly.getDatas();
                //}
                Debug.Assert((string)name != null);

                if (count != "")
                {
                    // dealing with an array of entries
                    int countAsInt = int.Parse(count);

                    //fromString((string)count, countAsInt);

                    for (uint i = 0; i < countAsInt; i++)
                    {
                        // Progress bar
                        //progressCallBack.progress((float)i / (float)countAsInt);
                        //progressCallBack.pushCropedValues((float)i / (float)countAsInt, (float)(i + 1) / (float)countAsInt);

                        //				nlinfo("+ %s%d",name,i);
                        string newName = name + i; //new string(name.getDatas()) + toString(i);
                        addNode(new CDBNodeBranch(newName), newName, this, _Nodes, _NodesByName, child, sBank, sAtom == "1", sClientonly == "1", progressCallBack, mapBanks, bankHandler);
                        //				nlinfo("-");

                        // Progress bar
                        //progressCallBack.popCropedValues();
                    }
                }
                else
                {
                    // dealing with a single entry
                    //			nlinfo("+ %s",name);
                    string newName = name; //.getDatas();
                    addNode(new CDBNodeBranch(newName), newName, this, _Nodes, _NodesByName, child, sBank, sAtom == "1", sClientonly == "1", progressCallBack, mapBanks, bankHandler);
                    //			nlinfo("-");
                }

                // Progress bar
                //progressCallBack.popCropedValues();

                nodeId++;
            }

            //// look for leaves of this branch
            //for (child = CIXml.getFirstChildNode(node, "leaf"); child; child = CIXml.getNextChildNode(child, "leaf"))
            //{
            //    // Progress bar
            //    progressCallBack.progress((float)nodeId / (float)countNode);
            //    progressCallBack.pushCropedValues((float)nodeId / (float)countNode, (float)(nodeId + 1) / (float)countNode);
            //
            //    CXMLAutoPtr name = new CXMLAutoPtr((string)xmlGetProp(child, (xmlChar)"name"));
            //    CXMLAutoPtr count = new CXMLAutoPtr((string)xmlGetProp(child, (xmlChar)"count"));
            //    CXMLAutoPtr bank = new CXMLAutoPtr((string)xmlGetProp(child, (xmlChar)"bank"));
            //
            //    string sBank;
            //    if (bank != null)
            //    {
            //        sBank = bank.getDatas();
            //    }
            //    Debug.Assert((string)name != null);
            //    if ((string)count != null)
            //    {
            //        // dealing with an array of entries
            //        uint countAsInt;
            //        fromString((string)count, countAsInt);
            //
            //        for (uint i = 0; i < countAsInt; i++)
            //        {
            //            // Progress bar
            //            progressCallBack.progress((float)i / (float)countAsInt);
            //            progressCallBack.pushCropedValues((float)i / (float)countAsInt, (float)(i + 1) / (float)countAsInt);
            //
            //            //				nlinfo("  %s%d",name,i);
            //            string newName = new string(name.getDatas()) + toString(i);
            //            addNode(new CDBNodeLeaf(newName), newName, this, _Nodes, _NodesByName, child, sBank, false, false, progressCallBack, mapBanks, bankHandler);
            //
            //            // Progress bar
            //            progressCallBack.popCropedValues();
            //        }
            //    }
            //    else
            //    {
            //        //			nlinfo("  %s",name);
            //        string newName = name.getDatas();
            //        addNode(new CDBNodeLeaf(newName), newName, this, _Nodes, _NodesByName, child, sBank, false, false, progressCallBack, mapBanks, bankHandler);
            //    }
            //
            //    // Progress bar
            //    //progressCallBack.popCropedValues();
            //
            //    nodeId++;
            //}
            //
            //// count number of bits required to store the id
            ////C++ TO C# CONVERTER TODO TASK: The following line could not be converted:
            //if ((mapBanks) && (getParent() == null))
            //{
            //    Debug.Assert(bankHandler != null);
            //    //Debug.Assert(bankHandler.getUnifiedIndexToBankSize() == countNode /*, ("Mapped: %u Nodes: %u", bankHandler.getUnifiedIndexToBankSize(), countNode)*/);
            //    bankHandler.calcIdBitsByBank();
            //    _IdBits = 0;
            //}
            //else
            //{
            //    if (_Nodes.Count != 0)
            //    {
            //        for (_IdBits = 1; _Nodes.Count > (uint)(1 << _IdBits); _IdBits++) { }
            //    }
            //    else
            //    {
            //        _IdBits = 0;
            //    }
            //}
            //
            //find(""); // Sort !
        }

        private void addNode(ICDBNode newNode, string newName, CDBNodeBranch parent, List<ICDBNode> nodes, List<ICDBNode> nodesSorted, XmlElement child, string bankName, bool atomBranch, bool clientOnly, Action progressCallBack, bool mapBanks, CDBBankHandler bankHandler = null)
        {
            nodesSorted.Add(newNode);
            nodes.Add(newNode);
            nodes[nodes.Count - 1].SetParent(parent);
            nodes[nodes.Count - 1].SetAtomic(parent.IsAtomic() || atomBranch);
            nodes[nodes.Count - 1].Init(child, progressCallBack);

            // Setup bank mapping for first-level node
            if (mapBanks && (parent.getParent() == null))
            {
                if (!string.IsNullOrEmpty(bankName))
                {
                    bankHandler.mapNodeByBank(bankName);
                    //nldebug( "CDB: Mapping %s for %s (node %u)", newName.c_str(), bankName.c_str(), nodes.size()-1 );
                }
                else
                {
                    RyzomClient.GetInstance().GetLogger().Error("Missing bank for first-level node " + newName);
                }
            }
        }

        private bool IsAtomic()
        {
            return false; //throw new NotImplementedException();
        }

        /// <summary>
        /// Add a new sub node
        /// </summary>
        /// <params id="node">is the new subnode</params>
        /// <params id="nodeName">is the name of the node</params>
        void attachChild(ICDBNode node, string nodeName) { }

        /// <summary>
        /// Get a node . Create it if it does not exist yet
        /// </summary>
        /// <params id="id">the CTextId identifying the node</params>
        //ICDBNode getNode(CTextId id, bool bCreate = true) { return null; }

        /// <summary>
        /// Get a node. Return null if out of bounds (no warning)
        /// </summary>
        /// <params id="idx">is the node index</params>
        ICDBNode getNode(ushort idx) { return null; }

        /// <summary>
        /// Get a node index
        /// </summary>
        /// <params id="node">is a pointer to the node</params>
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

        /// <summary>return the child with the given node id, creating it if requested</summary>
        public CDBNodeLeaf getLeaf(char id, bool bCreate) { return null; }

        public CDBNodeLeaf getLeaf(string id, bool bCreate) { return getLeaf(id[0], bCreate); }

        /// <summary>
        /// Save a backup of the database
        /// </summary>
        /// <params id="id">is the text id of the property/grp</params>
        /// <params id="f">is the stream</params>
        //void write(CTextId id, FILE f);

        /// Update the database from the delta, but map the first level with the bank mapping (see _CDBBankToUnifiedIndexMapping)
        //void readAndMapDelta(TGameCycle gc, CBitMemStream s, uint bank, CDBBankHandler bankHandler);

        /// Update the database from a stream coming from the FE
        //void readDelta(TGameCycle gc, CBitMemStream f);

        /// <summary>
        /// Return the value of a property (the update flag is set to false)
        /// </summary>
        /// <params id="id">is the text id of the property/grp</params>
        /// <params id="name">is the name of the property</params>
        /// <returns>the value of the property</returns>
        //long getProp(CTextId id);

        /// <summary>
        /// Set the value of a property (the update flag is set to true)
        /// </summary>
        /// <params id="id">is the text id of the property/grp</params>
        /// <params id="name">is the name of the property</params>
        /// <params id="value">is the value of the property</params>
        /// <returns>bool : 'true' if property found.</returns>
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

        /// <summary>the parent node for a branch (null by default)</summary>
        public virtual void setParent(CDBNodeBranch parent) { _Parent = parent; }

        public virtual CDBNodeBranch getParent()
        {
            return _Parent;
        }

        /// <summary>get the number of nodes</summary>
        public ushort getNbNodes()
        {
            return (ushort)_Nodes.Count;
        }

        /// <summary>Count the leaves</summary>
        public virtual uint countLeaves() { return 0; }

        /// <summary>Find the leaf which count is specified (if found, the returned value is non-null and count is 0)</summary>
        public virtual CDBNodeLeaf findLeafAtCount(uint count) { return null; }

        public virtual void display(string prefix) { }

        //void removeNode(CTextId id);

        /// <summary>
        /// add an observer to a property
        /// </summary>
        /// <params id="observer">: pointer to an observer</params>
        /// <params id="id">text id identifying the property</params>
        /// <returns>false if the node doen t exist</returns>
        //virtual bool addObserver(IPropertyObserver observer, CTextId id);

        /// <summary> remove an obsever</summary>
        /// <params id="observer">: pointer to an observer</params>
        /// <returns>false if the node or observer doesn t exist</returns>
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

        // number of bits required to stock my children's ids
        byte _IdBits = 7;
        bool _Sorted = true;

        // observers for this node or branch
        //TObserverHandleList observerHandles;

        /// <summary>called by clear</summary>
        void removeAllBranchObserver() { }

        //#if NL_CDB_OPTIMIZE_PREDICT
        //		CRefPtr<ICDBNode>		_PredictNode;
        //#endif
        public override void SetParent(CDBNodeBranch parent)
        {
            //throw new NotImplementedException();
        }

        public override void SetAtomic(bool atomBranch)
        {
            //throw new NotImplementedException();
        }
    }
}