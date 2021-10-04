///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;
using RCC.Network;

namespace RCC.Database
{
    /// <summary>
    /// Database DatabaseNode which contains a set of properties
    /// </summary>
    /// <author>Stephane Coutelas</author>
    /// <author>Nevrax France</author>
    /// <date>2002</date>
    public class DatabaseNodeBranch : DatabaseNodeBase
    {
        /// <summary>
        /// database subnodes not sorted
        /// </summary>
        private readonly List<DatabaseNodeBase> _nodes = new List<DatabaseNodeBase>();

        /// <summary>
        /// subnodes sorted by name
        /// </summary>
        private readonly List<DatabaseNodeBase> _nodesByName = new List<DatabaseNodeBase>();

        DatabaseNodeBase _predictDatabaseNode;

        /// <summary>number of bits required to stock my children's ids</summary>
        private byte _idBits;

        private bool _sorted;

        private const bool VerboseDatabase = false;

        /// <summary>
        /// default constructor
        /// </summary>
        public DatabaseNodeBranch(string name)
        {
            Name = name;
            Parent = null;
            _idBits = 0;
            _sorted = false;
        }

        /// <summary>
        /// Build the structure of the database from a file
        /// </summary>
        /// <params id="f">is the stream</params>
        internal override void Init(XmlElement node, Action progressCallBack, bool mapBanks = false, BankHandler bankHandler = null)
        {
            _sorted = false;
            var countNode = 0;

            // look for other branches within this branch
            foreach (var subNode in node.SelectNodes("branch"))
            {
                if (!(subNode is XmlElement child))
                    continue;

                countNode++;

                var sBank = child.GetAttribute("bank");
                var sAtom = child.GetAttribute("atom");
                var sClientonly = child.GetAttribute("clientonly");
                var sCount = child.GetAttribute("count");
                var sName = child.GetAttribute("name");

                if (sCount != string.Empty)
                {
                    // dealing with an array of entries
                    var countAsInt = int.Parse(sCount);

                    for (uint i = 0; i < countAsInt; i++)
                    {
                        var newName = sName + i;
                        AddNode(new DatabaseNodeBranch(newName), newName, this, _nodes, _nodesByName, child, sBank, sAtom == "1", sClientonly == "1", progressCallBack, mapBanks, bankHandler);
                    }
                }
                else
                {
                    // dealing with a single entry
                    var newName = sName;
                    AddNode(new DatabaseNodeBranch(newName), newName, this, _nodes, _nodesByName, child, sBank, sAtom == "1", sClientonly == "1", progressCallBack, mapBanks, bankHandler);
                }
            }

            // look for leaves of this branch
            foreach (var subNode in node.SelectNodes("leaf"))
            {
                if (!(subNode is XmlElement child))
                    continue;

                countNode++;

                var sBank = child.GetAttribute("bank");
                var sCount = child.GetAttribute("count");
                var sName = child.GetAttribute("name");

                if (sCount != string.Empty)
                {
                    // dealing with an array of entries
                    var countAsInt = int.Parse(sCount);

                    for (uint i = 0; i < countAsInt; i++)
                    {
                        var newName = sName + i;
                        AddNode(new DatabaseNodeLeaf(newName), newName, this, _nodes, _nodesByName, child, sBank, false, false, progressCallBack, mapBanks, bankHandler);
                    }
                }
                else
                {
                    // dealing with a single entry
                    var newName = sName;
                    AddNode(new DatabaseNodeLeaf(newName), newName, this, _nodes, _nodesByName, child, sBank, false, false, progressCallBack, mapBanks, bankHandler);
                }
            }

            // count number of bits required to store the id
            if (mapBanks && GetParent() == null)
            {
                Debug.Assert(bankHandler != null);
                Debug.Assert(bankHandler.GetUnifiedIndexToBankSize() == countNode);

                bankHandler.CalcIdBitsByBank();
                _idBits = 0;
            }
            else
            {
                if (_nodes.Count != 0)
                {
                    for (_idBits = 1; _nodes.Count > (uint)(1 << _idBits); _idBits++) { }
                }
                else
                {
                    _idBits = 0;
                }
            }

            Find(""); // Sort !
        }

        /// <summary>
        /// Adds a new node to the branch
        /// </summary>
        private static void AddNode(DatabaseNodeBase newDatabaseNode, string newName, DatabaseNodeBranch parent, List<DatabaseNodeBase> nodes, List<DatabaseNodeBase> nodesSorted, XmlElement child, string bankName, bool atomBranch, bool clientOnly, Action progressCallBack, bool mapBanks, BankHandler bankHandler = null)
        {
            nodesSorted.Add(newDatabaseNode);
            nodes.Add(newDatabaseNode);
            nodes[^1].SetParent(parent);
            nodes[^1].SetAtomic(parent.IsAtomic() || atomBranch);
            nodes[^1].Init(child, progressCallBack);

            // Setup bank mapping for first-level databaseNode
            if (!mapBanks || parent.GetParent() != null) return;

            if (!string.IsNullOrEmpty(bankName))
            {
                bankHandler?.MapNodeByBank(bankName);
                //nldebug( "CDB: Mapping %s for %s (databaseNode %u)", newName.c_str(), bankName.c_str(), nodes.size()-1 );
            }
            else
            {
                RyzomClient.GetInstance().GetLogger().Error("Missing bank for first-level databaseNode " + newName);
            }
        }

        /// <summary>
        /// Get a database node. Create it if it does not exist yet
        /// </summary>
        /// <params id="id">the TextId identifying the database node</params>
        internal override DatabaseNodeBase GetNode(TextId id, bool bCreate = true)
        {
            // lookup next element from textid in my index => idx
            var str = id.ReadNext();

            var pNode = Find(str);

            // If the node do not exists
            if (pNode != null) return !id.HasElements() ? pNode : pNode.GetNode(id, bCreate);

            if (bCreate)
            {
                // Yoyo: must not be SERVER or LOCAL, cause definied through xml.
                // This may cause some important crash error
                //nlassert(!id.empty());
                //nlassert(id.getElement(0)!="SERVER");
                //nlassert(id.getElement(0)!="LOCAL");
                DatabaseNodeBase newNode;
                if (id.GetCurrentIndex() == id.Size())
                {
                    newNode = new DatabaseNodeLeaf(str);
                }
                else
                {
                    newNode = new DatabaseNodeBranch(str);
                }

                _nodes.Add(newNode);
                _nodesByName.Add(newNode);
                _sorted = false;
                newNode.SetParent(this);
                pNode = newNode;
            }
            else
            {
                return null;
            }

            // get property from child
            return !id.HasElements() ? pNode : pNode.GetNode(id, bCreate);
        }

        /// <summary>
        /// Add a new sub database node
        /// </summary>
        /// <params id="databaseNode">is the new subnode</params>
        /// <params id="nodeName">is the name of the database node</params>
        internal override void AttachChild(DatabaseNodeBase node, string nodeName)
        {
            Debug.Assert(Parent == null);

            if (node == null) return;

            node.SetParent(this);
            _nodes.Add(node);
            //nldebug ( "CDB: Attaching node" );
            _nodesByName.Add(node);
            _sorted = false;

            _predictDatabaseNode = node;
        }

        /// <summary>
        /// Get a database node. Return null if out of bounds (no warning)
        /// </summary>
        /// <params id="idx">is the database node index</params>
        internal override DatabaseNodeBase GetNode(ushort idx)
        {
            return idx < _nodes.Count ? _nodes[idx] : null;
        }

        /// <summary>
        /// Get a database node index
        /// </summary>
        /// <params id="databaseNode">is a pointer to the database node</params>
        internal override bool GetNodeIndex(DatabaseNodeBase databaseNode, ref uint index)
        {
            foreach (var it in _nodes)
            {
                if (it == databaseNode)
                    return true;
                index++;
            }

            return false;
        }

        /// <summary>
        /// Update the database from the delta, but map the first level with the bank mapping (see _CDBBankToUnifiedIndexMapping)
        /// </summary>
        internal void ReadAndMapDelta(uint gc, BitMemoryStream s, uint bank, BankHandler bankHandler)
        {
            Debug.Assert(!IsAtomic()); // root databaseNode mustn't be atomic

            // Read index
            uint idx = 0;
            s.Serial(ref idx, (int)bankHandler.GetFirstLevelIdBits((int)bank));

            // Translate bank index . unified index
            idx = bankHandler.GetServerToClientUidMapping((int)bank, (int)idx);

            if (idx >= _nodes.Count)
            {
                throw new Exception($"idx {idx} > _Nodes.size() {_nodes.Count} ");
            }

            // Apply delta to children nodes
            _nodes[(int)idx].ReadDelta(gc, s);
        }

        /// <summary>
        /// Update the database from a stream coming from the FE
        /// </summary>
        internal override void ReadDelta(uint gc, BitMemoryStream f)
        {
            if (IsAtomic())
            {
                // Read the atom bitfield
                var nbAtomElements = CountLeaves();

                var bitfield = new bool[nbAtomElements];

                f.Serial(ref bitfield);

                // Set each modified property
                for (uint i = 0; i != bitfield.Length; ++i)
                {
                    if (!bitfield[i]) continue;

                    var atomIndex = i;
                    var leaf = FindLeafAtCount(ref atomIndex);

                    if (leaf != null)
                    {
                        leaf.ReadDelta(gc, f);
                    }
                    else
                    {
                        RyzomClient.GetInstance().GetLogger().Warn($"CDB: Can't find leaf with index {i} in atom branch {(GetParent() == null ? GetName() : "(root)")}");
                    }
                }
            }
            else
            {
                uint idx = 0;

                f.Serial(ref idx, _idBits);

                if (idx >= _nodes.Count)
                {
                    throw new Exception($"idx {idx} > _Nodes.size() {_nodes.Count} ");
                }

                _nodes[(int)idx].ReadDelta(gc, f);
            }
        }


        /// <summary>
        /// Return the value of a property (the update flag is set to false)
        /// </summary>
        /// <param name="id">is the text id of the property/grp</param>
        /// <returns>the value of the property</returns>
        internal override long GetProp(TextId id)
        {
            // lookup next element from textid in my index => idx
            var str = id.ReadNext();
            var pDatabaseNode = Find(str);
            Debug.Assert(pDatabaseNode != null);

            // get property from child
            return pDatabaseNode.GetProp(id);
        }

        /// <summary>Clear the databaseNode and his children</summary>
        public void Clear()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Resets the data of the specific node in the branch
        /// </summary>
        public void ResetNode(uint gc, int node)
        {
            if (node > _nodes.Count)
                return;

            _nodes[node].ResetData(gc);
        }

        /// <summary>
        /// Reset all leaf data from this point
        /// </summary>
        internal override void ResetData(uint gc, bool forceReset = false)
        {
            for (var i = 0; i != _nodes.Count; ++i)
            {
                _nodes[i].ResetData(gc, forceReset);
            }
        }

        /// <summary>
        ///	Destructor
        /// </summary>
        ~DatabaseNodeBranch() { Clear(); }

        /// <summary>
        /// Count the leaves
        /// </summary>
        internal override uint CountLeaves()
        {
            uint n = 0;

            foreach (var it in _nodes)
            {
                n += it.CountLeaves();
            }

            return n;
        }

        /// <summary>
        /// Find the leaf which count is specified (if found, the returned value is non-null and count is 0)
        /// </summary>
        internal override DatabaseNodeLeaf FindLeafAtCount(ref uint count)
        {
            foreach (var itNode in _nodes)
            {
                var leaf = itNode.FindLeafAtCount(ref count);

                if (leaf != null)
                {
                    return leaf;
                }
            }

            return null;
        }

        /// <summary>
        /// Removed the node with the id
        /// </summary>
        public void RemoveNode(TextId id)
        {
            // Look for the node
            if (!(GetNode(id, false) is DatabaseNodeBranch pNode))
            {
                RyzomClient.GetInstance().GetLogger().Warn("node " + id + " not found");
                return;
            }

            var pParent = pNode.GetParent();

            if (pParent == null)
            {
                RyzomClient.GetInstance().GetLogger().Warn("parent node not found");
                return;
            }

            // search index node unsorted
            int indexNode;
            for (indexNode = 0; indexNode < pParent._nodes.Count; ++indexNode)
            {
                if (pParent._nodes[indexNode] == pNode)
                {
                    break;
                }
            }

            if (indexNode == pParent._nodes.Count)
            {
                RyzomClient.GetInstance().GetLogger().Warn("node not found");
                return;
            }

            // search index node sorted
            int indexSorted;

            for (indexSorted = 0; indexSorted < pParent._nodesByName.Count; ++indexSorted)
            {
                if (pParent._nodesByName[indexSorted] == pNode)
                {
                    break;
                }
            }

            if (indexSorted == pParent._nodesByName.Count)
            {
                RyzomClient.GetInstance().GetLogger().Warn("node not found");
                return;
            }

            // Remove node from parent
            pParent._nodes.RemoveAt(indexNode);
            pParent._nodesByName.RemoveAt(indexSorted);
            pParent._sorted = false;

            // Delete the node
            pNode.Clear();
        }


        /// <summary>
        /// Find a subnode at this level
        /// </summary>
        public DatabaseNodeBase Find(string nodeName)
        {
            var predictNode = _predictDatabaseNode;

            if (predictNode != null)
            {
                if (predictNode.GetParent() == this && predictNode.GetName() == nodeName)
                {
                    return predictNode;
                }
            }

            if (!_sorted)
            {
                _sorted = true;
                //sort(_NodesByName.begin(), _NodesByName.end(), CCDBNodeBranchComp());
                _nodesByName.Sort((a, b) => string.Compare(a.GetName(), b.GetName(), StringComparison.Ordinal));
            }

            foreach (var it in _nodesByName)
            {
                if (it.GetName() != nodeName) continue;

                var node = it;
                _predictDatabaseNode = node;
                return node;
            }

            return null;
        }

        internal override void Write(string id, StreamWriter f)
        {
            foreach (var node in _nodes)
            {
                node.Write($"{id}:{node.GetName()}", f);
            }
        }
    }
}
