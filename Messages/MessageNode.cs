﻿///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Xml;
using RCC.Helper;
using RCC.Network;

namespace RCC.Messages
{
    /// <summary>
    ///     Node Leafs in a tree storing server message information (callbacks, bit sizes, names, ...)
    ///     from CGenericXmlMsgHeaderManager::CNode
    /// </summary>
    internal class MessageNode
    {
        //public delegate void TMsgHeaderCallback(object[] arguments); // TMsgHeaderCallback -> Event Structure
        public Action<BitMemoryStream> Callback;
        public string Description;
        public List<MessageField> Format = new List<MessageField>(); // TMessageFormat
        public string Name;
        public uint NbBits;
        public List<MessageNode> Nodes;
        public Dictionary<string, MessageNode> NodesByName;
        public string SendTo;
        public bool UseCycle;
        public uint[] UserData = new uint[4];

        public uint Value;

        /// <summary>
        ///     Constructor that copies its settings from a xml Node
        /// </summary>
        public MessageNode(XmlElement xmlNode, uint value)
        {
            Nodes = new List<MessageNode>();
            NodesByName = new Dictionary<string, MessageNode>();

            Value = value;
            UseCycle = false;
            NbBits = 0;
            Callback = null;
            //Callback = delegate { };

            UserData[0] = 0;
            UserData[1] = 0;
            UserData[2] = 0;
            UserData[3] = 0;

            // setup node name
            //CXMLAutoPtr name(xmlGetProp (xmlNode, (xmlChar*)"name"));
            if (xmlNode.GetAttribute("name") != "")
            {
                Name = xmlNode.GetAttribute("name");
            }

            uint childValue = 0;

            if (xmlNode.Name == "leaf")
            {
                // only setup description and format if leaf

                // setup node description
                //CXMLAutoPtr description(xmlGetProp (xmlNode, (xmlChar*)"description"));
                if (xmlNode.GetAttribute("description") != "")
                {
                    //    Description = (const char*)description;
                    Description = xmlNode.GetAttribute("description");
                }

                //
                //// setup node description
                //CXMLAutoPtr sendto(xmlGetProp (xmlNode, (xmlChar*)"sendto"));
                if (xmlNode.GetAttribute("sendto") != "")
                {
                    //    SendTo = (const char*)sendto;
                    SendTo = xmlNode.GetAttribute("sendto");
                }

                //
                //// setup node description
                //CXMLAutoPtr usecycle(xmlGetProp (xmlNode, (xmlChar*)"usecycle"));
                //
                //if (bool(usecycle) && !strcmp((const char*)usecycle, "yes"))
                if (xmlNode.GetAttribute("usecycle") != "")
                {
                    UseCycle = xmlNode.GetAttribute("usecycle") == "yes";
                }

                //
                //// setup node format
                //CXMLAutoPtr format(xmlGetProp (xmlNode, (xmlChar*)"format"));
                char[] buf = new char[256];

                if (xmlNode.GetAttribute("format") != "")
                {
                    //    char* scan = &buf[0];
                    //    nlassert(strlen((const char*)format) < 256 );
                    //    strcpy(scan, (const char*)format );

                    var formats = xmlNode.GetAttribute("format").Split(" ");

                    //    while (*scan != '\0')
                    foreach (var scan in formats)
                    {
                        switch (scan.ToLower()[0])
                        {
                            case 's':
                                if (scan.Length == 1)
                                    // here consider s as string
                                    Format.Add(new MessageField(FieldType.String, 0));
                                else
                                {
                                    // here consider s as sint
                                    byte numBits = (byte) int.Parse(scan.Substring(1));

                                    if (numBits == 8)
                                        Format.Add(new MessageField(FieldType.Sint8, numBits));
                                    else if (numBits == 16)
                                        Format.Add(new MessageField(FieldType.Sint16, numBits));
                                    else if (numBits == 32)
                                        Format.Add(new MessageField(FieldType.Sint32, numBits));
                                    else if (numBits == 64)
                                        Format.Add(new MessageField(FieldType.Sint64, numBits));
                                    else
                                        RyzomClient.Log?.Warn(
                                            "Can't use sint in format with other size than 8, 16, 32 or 64");
                                }

                                break;

                            case 'u':
                                if (scan == "uc")
                                    // here consider s as string
                                    Format.Add(new MessageField(FieldType.UcString, 0));
                                else
                                {
                                    // here consider s as sint
                                    byte numBits = (byte) int.Parse(scan.Substring(1));

                                    if (numBits == 8)
                                        Format.Add(new MessageField(FieldType.Uint8, numBits));
                                    else if (numBits == 16)
                                        Format.Add(new MessageField(FieldType.Uint16, numBits));
                                    else if (numBits == 32)
                                        Format.Add(new MessageField(FieldType.Uint32, numBits));
                                    else if (numBits == 64)
                                        Format.Add(new MessageField(FieldType.Uint64, numBits));
                                    else
                                        Format.Add(new MessageField(FieldType.BitSizedUint, numBits));
                                }

                                break;

                            case 'f':
                                // here consider f as float
                                Format.Add(new MessageField(FieldType.Float, 32));
                                break;

                            case 'd':
                                // here consider d as double
                                Format.Add(new MessageField(FieldType.Double, 64));
                                break;

                            case 'e':
                                // here consider e as CEntityId
                                Format.Add(new MessageField(FieldType.EntityId, 64));
                                break;

                            case 'b':
                                // here consider b as bool
                                Format.Add(new MessageField(FieldType.Bool, 1));
                                break;
                        }
                    }
                }
            }
            else
            {
                //// only parse children if not leaf
                foreach (XmlNode xmlChild in xmlNode.ChildNodes)
                {
                    // check node is leaf or branch
                    if (xmlChild.Name != "branch" && xmlChild.Name != "leaf") continue;

                    if (xmlChild.NodeType != XmlNodeType.Element) break;

                    // create a node from the child xml node
                    var child = new MessageNode((XmlElement) xmlChild, childValue);

                    // check node doesn't exist yet in parent
                    if (!NodesByName.ContainsKey(child.Name))
                    {
                        // add it to parent's children
                        NodesByName.Add(child.Name, child);
                        Nodes.Add(child);
                        ++childValue;
                    }
                    else
                    {
                        RyzomClient.Log?.Debug(
                            $"Child '{child.Name}' in node '{Name}' already exists, unable to add it");
                        // delete child;
                    }
                }
            }

            // compute number of bits from the number of children
            NbBits = childValue == 0 ? 0 : Misc.GetPowerOf2(childValue);
        }

        /// <summary>
        ///     select node using name, no other action performed
        /// </summary>
        internal MessageNode Select(string msgName)
        {
            var node = this;

            var subSplitted = msgName.Split(":");

            for (var i = 0; i < subSplitted.Length; i++)
            {
                var sub = subSplitted[i];

                if (!node.NodesByName.ContainsKey(sub))
                {
                    RyzomClient.Log?.Warn($"Couldn't select node '{sub}', not found in parent '{node.Name}'");
                    return null;
                }

                node = node.NodesByName[sub];

                if (i == subSplitted.Length - 1)
                    return node;
            }

            return null;
        }


        /// <summary>
        ///     select node using name, and write bits in stream
        /// </summary>
        internal MessageNode Select(string name, BitMemoryStream strm)
        {
            var node = this;

            var subSplitted = name.Split(":");

            for (var i = 0; i < subSplitted.Length; i++)
            {
                var sub = subSplitted[i];

                if (node.NbBits == 0)
                {
                    RyzomClient.Log?.Warn($"Couldn't select node '{sub}', parent '{node.Name}' has no bit per child");
                    return null;
                }

                if (!node.NodesByName.ContainsKey(sub))
                {
                    RyzomClient.Log?.Warn($"Couldn't select node '{sub}', not found in parent '{node.Name}'");
                    return null;
                }

                var nodeOld = node;
                node = node.NodesByName[sub];

                var index = node.Value;
                strm.SerialAndLog2(ref index, nodeOld.NbBits);

                if (i == subSplitted.Length - 1)
                    return node;
            }

            return null;
        }


        /// <summary>
        ///     select node using bits stream
        /// </summary>
        public MessageNode Select(BitMemoryStream strm)
        {
            var node = this;

            while (node != null && node.NbBits != 0)
            {
                uint index = 0;
                strm.SerialAndLog2(ref index, node.NbBits);

                if (index >= node.Nodes.Count)
                {
                    RyzomClient.Log?.Debug(
                        $"Couldn't select node from stream, invalid index {index} in parent '{node.Name}'");
                    return null;
                }

                node = node.Nodes[(int) index];
            }

            return node;
        }

        /// <summary>
        ///     Destructor
        /// </summary>
        ~MessageNode()
        {
            uint i;
            for (i = 0; i < Nodes.Count; ++i)
            {
                //    delete Nodes[i];
                Nodes[(int) i] = null;
            }
        }
    }
}