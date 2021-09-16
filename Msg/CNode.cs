using System;
using System.Collections.Generic;
using System.Xml;
using RCC.Helper;
using RCC.Network;

namespace RCC.Msg
{
    /// <summary>
    /// CGenericXmlMsgHeaderManager::CNode
    /// </summary>
    class CNode
    {
        public List<CNode> Nodes;
        public Dictionary<string, CNode> NodesByName;

        public uint Value;
        public string Name;
        public string Description;
        public string SendTo;
        public bool UseCycle;
        public uint[] UserData = new uint[4];
        public List<CMessageField> Format = new List<CMessageField>(); // TMessageFormat
        public uint NbBits;
        //public delegate void TMsgHeaderCallback(object[] arguments); // TMsgHeaderCallback -> Event Structure
        public Action<CBitMemStream> Callback;

        /// <summary>
        /// Constructor
        /// </summary>
        public CNode(XmlElement xmlNode, uint value)
        {
            Nodes = new List<CNode>();
            NodesByName = new Dictionary<string, CNode>();

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
                                    Format.Add(new CMessageField(TFieldType.String, 0));
                                else
                                {
                                    // here consider s as sint
                                    byte numBits = (byte)int.Parse(scan.Substring(1));

                                    if (numBits == 8)
                                        Format.Add(new CMessageField(TFieldType.Sint8, numBits));
                                    else if (numBits == 16)
                                        Format.Add(new CMessageField(TFieldType.Sint16, numBits));
                                    else if (numBits == 32)
                                        Format.Add(new CMessageField(TFieldType.Sint32, numBits));
                                    else if (numBits == 64)
                                        Format.Add(new CMessageField(TFieldType.Sint64, numBits));
                                    else
                                        ConsoleIO.WriteLine("Can't use sint in format with other size than 8, 16, 32 or 64");
                                }

                                break;

                            case 'u':
                                if (scan == "uc")
                                    // here consider s as string
                                    Format.Add(new CMessageField(TFieldType.UCString, 0));
                                else
                                {
                                    // here consider s as sint
                                    byte numBits = (byte)int.Parse(scan.Substring(1));

                                    if (numBits == 8)
                                        Format.Add(new CMessageField(TFieldType.Uint8, numBits));
                                    else if (numBits == 16)
                                        Format.Add(new CMessageField(TFieldType.Uint16, numBits));
                                    else if (numBits == 32)
                                        Format.Add(new CMessageField(TFieldType.Uint32, numBits));
                                    else if (numBits == 64)
                                        Format.Add(new CMessageField(TFieldType.Uint64, numBits));
                                    else
                                        Format.Add(new CMessageField(TFieldType.BitSizedUint, numBits));
                                }

                                break;

                            case 'f':
                                // here consider f as float
                                Format.Add(new CMessageField(TFieldType.Float, 32));
                                break;

                            case 'd':
                                // here consider d as double
                                Format.Add(new CMessageField(TFieldType.Double, 64));
                                break;

                            case 'e':
                                // here consider e as CEntityId
                                Format.Add(new CMessageField(TFieldType.EntityId, 64));
                                break;

                            case 'b':
                                // here consider b as bool
                                Format.Add(new CMessageField(TFieldType.Bool, 1));
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
                    if (xmlChild.Name == "branch" || xmlChild.Name == "leaf")
                    {
                        if (xmlChild.NodeType != XmlNodeType.Element) break;

                        // create a node from the child xml node
                        var child = new CNode((XmlElement)xmlChild, childValue);

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
                            ConsoleIO.WriteLine("Child '" + child.Name + "' in node '" + Name + "' already exists, unable to add it");
                            // delete child;
                        }
                    }
                }
            }

            // compute number of bits from the number of children
            NbBits = (childValue == 0) ? 0 : getPowerOf2(childValue);
        }

        /// <summary>
        /// select node using name, no other action performed
        /// </summary>
        internal CNode select(string msgName)
        {
            var node = this;

            string[] subSplitted = msgName.Split(":");

            for (int i = 0; i < subSplitted.Length; i++)
            {
                var sub = subSplitted[i];

                if (!node.NodesByName.ContainsKey(sub))
                {
                    ConsoleIO.WriteLineFormatted($"§eCouldn't select node '{sub}', not found in parent '{node.Name}'");
                    return null;
                }

                node = node.NodesByName[sub];

                if (i == subSplitted.Length - 1)
                    return node;
            }

            return null;
        }


        /// <summary>
        /// select node using name, and write bits in stream
        /// </summary>
        internal CNode select(string name, CBitMemStream strm)
        {
            var node = this;

            var subSplitted = name.Split(":");

            for (var i = 0; i < subSplitted.Length; i++)
            {
                var sub = subSplitted[i];

                if (node.NbBits == 0)
                {
                    ConsoleIO.WriteLineFormatted($"§eCouldn't select node '{sub}', parent '{node.Name}' has no bit per child");
                    return null;
                }

                if (!node.NodesByName.ContainsKey(sub))
                {
                    ConsoleIO.WriteLineFormatted($"§eCouldn't select node '{sub}', not found in parent '{node.Name}'");
                    return null;
                }

                var nodeOld = node;
                node = node.NodesByName[sub];

                var index = node.Value;
                strm.serialAndLog2(ref index, nodeOld.NbBits);

                if (i == subSplitted.Length - 1)
                    return node;
            }

            return null;
        }


        /// <summary>
        /// select node using bits stream
        /// </summary>
        public CNode select(CBitMemStream strm)
        {
            CNode node = this;

            while (node != null && node.NbBits != 0)
            {
                uint index = 0;
                strm.serialAndLog2(ref index, node.NbBits);

                if (index >= node.Nodes.Count)
                {
                    ConsoleIO.WriteLine("Couldn't select node from stream, invalid index " + index + " in parent '" + node.Name + "'");
                    return null;
                }

                node = node.Nodes[(int)index];
            }

            return node;
        }

        /// <summary>
        /// Destructor
        /// </summary>
        ~CNode()
        {
            uint i;
            for (i = 0; i < Nodes.Count; ++i)
            {
                //    delete Nodes[i];
                Nodes[(int)i] = null;
            }
        }

        /// <summary>
        /// Return the power of 2 of v.
        /// </summary>
        /// <example>
        /// getPowerOf2(8) is 3
        /// getPowerOf2(5) is 3
        /// </example>>
        private static uint getPowerOf2(uint v)
        {
            uint res = 1;
            uint ret = 0;
            while (res < v)
            {
                ret++;
                res <<= 1;
            }

            return ret;
        }
    }
}
