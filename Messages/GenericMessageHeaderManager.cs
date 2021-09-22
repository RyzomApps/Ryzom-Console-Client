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

namespace RCC.Messages
{
    public class GenericMessageHeaderManager
    {
        public MessageNode Root;

        /// <summary>
        ///     init the massages from a xml file
        /// </summary>
        public void Init(string filename)
        {
            // open xml file
            var file = new XmlDocument();

            try
            {
                // Init an xml stream
                file.Load(filename);
            }
            catch
            {
                RyzomClient.GetInstance().GetLogger()?.Warn($"Cannot open xml file '{filename}', unable to initialize generic messages");
                return;
            }

            // create root node from root xml node
            Root = new MessageNode(file.DocumentElement, 0);

            RyzomClient.GetInstance().GetLogger()?.Debug($"Loaded {Root.Nodes.Count} server messages nodes.");
        }

        /// <summary>
        ///     set callback that is executed when a specific message arrived
        /// </summary>
        public bool SetCallback(string msgName, Action<BitMemoryStream> callback)
        {
            // check root
            if (Root == null)
            {
                RyzomClient.GetInstance().GetLogger()?.Warn($"Can't set callback for message '{msgName}', Root not properly initialized.");
                return false;
            }

            // search for msg node
            var node = Root.Select(msgName);

            // check node
            if (node == null)
            {
                RyzomClient.GetInstance().GetLogger()?.Warn($"Can't set callback for message '{msgName}', message not found.");
                return false;
            }

            // set callback
            node.Callback = callback;

            return true;
        }

        /// <summary>
        ///     execute a message based on the interpretation of the stream
        /// </summary>
        public void Execute(BitMemoryStream strm)
        {
            // check root
            if (Root == null)
            {
                RyzomClient.GetInstance().GetLogger()?.Warn("Can't execute message , Root not properly initialized.");
                return;
            }

            var node = Root.Select(strm);

            // check node
            if (node == null)
            {
                RyzomClient.GetInstance().GetLogger()?.Warn("Can't execute stream, no valid sequence found");
            }
            // check callback
            else if (node.Callback == null)
            {
                RyzomClient.GetInstance().GetLogger()?.Warn($"Can't execute msg '{node.Name}', no callback set");
            }
            // execute callback
            else
            {
                node.Callback(strm);
            }
        }

        /// <summary>
        ///     selects the message by its name and writes it to the stream
        /// </summary>
        public bool PushNameToStream(string msgName, BitMemoryStream strm)
        {
            var res = Root.Select(msgName, strm) != null;

            if (!res)
            {
                RyzomClient.GetInstance().GetLogger()?.Warn($"pushNameToStream failed: Unknown message name '{msgName}'");
            }

            return res;
        }
    }
}