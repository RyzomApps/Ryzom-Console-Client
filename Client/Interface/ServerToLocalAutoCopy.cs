///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using Client.Database;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Client.Interface
{
    /// <summary>
    /// Observer interface to a database property
    /// </summary>
    /// <author>Nicolas Brigand</author>
    /// <author>Nevrax France</author>
    /// <date>2002</date>
    public abstract class IPropertyObserver : IDisposable
    {
        public virtual void Dispose() { }
        public abstract void Update(DatabaseNode node);
    }


    /// <summary>
    /// Observer for copying db branch changes
    /// </summary>
    public class ServerToLocalAutoCopy : IDisposable
    {
        // Counter Node
        private DatabaseNodeLeaf _ServerCounter;
        // updaters
        private LocalDBObserver _LocalObserver;
        private ServerDBObserver _ServerObserver;
        // avoid reentrance
        private bool _LocalUpdating;

        /// <summary>Array of Nodes that have to be synchronized</summary>
        private List<Node> _Nodes = new List<Node>();
        /// <summary>Sorting of Nodes, by Server Node</summary>
        List<NodeServerComp> _ServerNodeMap = new List<NodeServerComp>();
        /// <summary>Sorting of Nodes, by Local Node</summary>
        List<NodeLocalComp> _LocalNodeMap = new List<NodeLocalComp>();
        /// <summary>List of nodes to update until next synchonized client-server counter</summary>
        private List<Node> _UpdateList = new List<Node>();

        private readonly InterfaceManager _interfaceManger;
        private readonly DatabaseManager _databaseManager;

        public ServerToLocalAutoCopy(InterfaceManager interfaceManager, DatabaseManager databaseManager)
        {
            _interfaceManger = interfaceManager;
            _databaseManager = databaseManager;
        }

        public void Dispose()
        {
            Release();
        }

        // init the AutoCopy
        public void Init(string dbPath)
        {
            var pIM = _interfaceManger;

            // Get the synchronisation Counter in Server DB
            _ServerCounter = _databaseManager.GetDbProp("SERVER:" + dbPath + ":COUNTER", false);

            // if found
            if (_ServerCounter != null)
            {
                TextId textId;

                // **** Add Observers on all nodes
                // add the observers when server node change
                textId = new TextId("SERVER:" + dbPath);
                _databaseManager.GetDb().AddObserver(_ServerObserver, textId);

                // add the observers when local node change
                textId = new TextId("LOCAL:" + dbPath);
                _databaseManager.GetDb().AddObserver(_LocalObserver, textId);

                // **** Init the Nodes shortcut
                // Parse all Local Nodes
                DatabaseNodeBranch localBranch = _databaseManager.GetDbBranch("LOCAL:" + dbPath);

                if (localBranch != null)
                {
                    int i;
                    List<DatabaseNodeLeaf> leaves = new List<DatabaseNodeLeaf>();
                    BuildRecursLocalLeaves(localBranch, leaves);

                    // --- build _Nodes
                    _Nodes = new List<Node>(leaves.Count);

                    for (i = 0; i < leaves.Count; i++)
                    {
                        DatabaseNodeLeaf localLeaf = leaves[i];

                        // get the SERVER associated node name
                        string serverLeafStr = localLeaf.GetName();
                        DatabaseNodeBranch parent = localLeaf.GetParent();
                        while (parent.GetName() != "LOCAL")
                        {
                            serverLeafStr = parent.GetName() + ":" + serverLeafStr;
                            parent = parent.GetParent();
                        }
                        serverLeafStr = "SERVER:" + serverLeafStr;

                        // try then to get this server node
                        DatabaseNodeLeaf serverLeaf = _databaseManager.GetDbProp(serverLeafStr, false);
                        if (serverLeaf != null)
                        {
                            // Both server and local leaves exist, ok, append to _Nodes
                            Node node = new Node
                            {
                                ServerNode = serverLeaf,
                                LocalNode = localLeaf
                            };

                            _Nodes.Add(node);
                        }
                    }

                    // --- Init the maps
                    _ServerNodeMap = new List<NodeServerComp>(leaves.Count);
                    _LocalNodeMap = new List<NodeLocalComp>(leaves.Count);

                    // For all valid _Nodes, insert in "map"
                    for (i = 0; i < _Nodes.Count; i++)
                    {
                        NodeLocalComp lc = new NodeLocalComp();
                        NodeServerComp sc = new NodeServerComp();
                        lc.Node = _Nodes[i];
                        sc.Node = _Nodes[i];
                        _LocalNodeMap.Add(lc);
                        _ServerNodeMap.Add(sc);
                    }

                    // then sort
                    _LocalNodeMap.Sort();
                    _ServerNodeMap.Sort();
                }
            }
        }

        // unhook from everything we are tangled up in
        void Release() { }

        // When something in the SERVER DB changes
        void OnServerChange(DatabaseNode serverNode)
        {
            if (_Nodes.Count == 0)
            {
                return;
            }

            var serverLeaf = (DatabaseNodeLeaf)serverNode;
            var pIM = _interfaceManger;

            // Add the leaf to the update list. only if not the counter
            if (serverLeaf != _ServerCounter)
            {
                // build the map key
                var nodeComp = new Node();
                var sc = new NodeServerComp();
                nodeComp.ServerNode = serverLeaf;
                sc.Node = nodeComp;
                // try to find the node associated to this server leaf
                var index = _ServerNodeMap.IndexOf(sc);

                // if found
                if (index > 0 || _ServerNodeMap[0].Node.ServerNode == serverLeaf)
                {
                    var node = _ServerNodeMap[index].Node;
                    // if this node is not already inserted
                    if (!node.InsertedInUpdateList)
                    {
                        // insert
                        node.InsertedInUpdateList = true;
                        _UpdateList.Add(node);
                    }
                }
            }

            // if the client and server are synchonized.
            if (pIM.localActionCounterSynchronizedWith(_ServerCounter))
            {
                // update all leaves
                for (var i = 0; i < _UpdateList.Count; i++)
                {
                    var node = _UpdateList[i];

                    _LocalUpdating = true;
                    node.LocalNode.SetValue64(node.ServerNode.GetValue64());
                    _LocalUpdating = false;

                    // reset inserted flag
                    node.InsertedInUpdateList = false;
                }

                // clear update list
                _UpdateList.Clear();
            }
        }

        // When something in the LOCAL DB changes
        void OnLocalChange(DatabaseNode localNode) { }

        private class LocalDBObserver : IPropertyObserver
        {
            public ServerToLocalAutoCopy _Owner;

            public LocalDBObserver(ServerToLocalAutoCopy owner)
            {
                _Owner = owner;
            }

            public override void Update(DatabaseNode node)
            {
                _Owner.OnLocalChange(node);
            }
        }

        private class ServerDBObserver : IPropertyObserver
        {
            public ServerToLocalAutoCopy _Owner;
            public ServerDBObserver(ServerToLocalAutoCopy owner)
            {
                _Owner = owner;
            }
            public override void Update(DatabaseNode node)
            {
                _Owner.OnServerChange(node);
            }
        }

        /// <summary>A node here is a pair Server<->Local</summary>
        private class Node
        {
            public DatabaseNodeLeaf ServerNode;
            public DatabaseNodeLeaf LocalNode;
            public bool InsertedInUpdateList;

            public Node()
            {
                ServerNode = null;
                LocalNode = null;
                InsertedInUpdateList = false;
            }
        }

        /// <summary>Struct for comparing nodes, by either Local or Server pointer</summary>
        private class NodeLocalComp : IComparable<NodeLocalComp>
        {
            public Node Node { get; internal set; }

            public int CompareTo([AllowNull] NodeLocalComp other)
            {
                return Node.LocalNode.GetValue32().CompareTo(other.Node.LocalNode.GetValue32());
            }
        }

        private class NodeServerComp
        {
            public Node Node { get; internal set; }

            public int CompareTo([AllowNull] NodeLocalComp other)
            {
                return Node.ServerNode.GetValue32().CompareTo(other.Node.ServerNode.GetValue32());
            }
        }

        void BuildRecursLocalLeaves(DatabaseNodeBranch branch, List<DatabaseNodeLeaf> leaves)
        {
            for (ushort i = 0; i < branch.NodeCount(); i++)
            {
                var node = branch.GetNode(i);
                if (node != null)
                {
                    if (node is DatabaseNodeLeaf leaf)
                    {
                        // just append to list
                        leaves.Add(leaf);
                    }
                    else
                    {
                        // recurs if a branch (should be...)
                        if (node is DatabaseNodeBranch sonBranch)
                        {
                            BuildRecursLocalLeaves(sonBranch, leaves);
                        }
                    }
                }
            }

        }
    }
}
