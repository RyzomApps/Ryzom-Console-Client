// This code is a modified version of a file from the 'Ryzom - MMORPG Framework'
// <http://dev.ryzom.com/projects/ryzom/>,
// which is released under GNU Affero General Public License.
// <http://www.gnu.org/licenses/>
// Original Copyright 2010 by Winch Gate Property Limited

using System;
using System.Xml;
using RCC.Helper;
using RCC.Network;

namespace RCC.Messages
{
    internal static class GenericMessageHeaderManager
    {
        public static Node Root;

        /// <summary>
        ///     init
        /// </summary>
        public static void Init(string filename)
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
                ConsoleIO.WriteLine($"§cCannot open xml file '{filename}', unable to initialize generic messages");
                return;
            }

            // create root node from root xml node
            Root = new Node(file.DocumentElement, 0);

            ConsoleIO.WriteLine("Loaded " + Root.Nodes.Count + " messages nodes.");
        }

        /// <summary>
        ///     set callback
        /// </summary>
        public static bool SetCallback(string msgName, Action<BitMemoryStream> callback)
        {
            // check root
            if (Root == null)
            {
                ConsoleIO.WriteLine($"Can't set callback for message '{msgName}', Root not properly initialized.");
                return false;
            }

            // search for msg node
            var node = Root.Select(msgName);

            // check node
            if (node == null)
            {
                ConsoleIO.WriteLine($"§cCan't set callback for message '{msgName}', message not found.");
                return false;
            }

            // set callback
            node.Callback = callback;

            return true;
        }

        /// <summary>
        ///     execute
        /// </summary>
        public static void Execute(BitMemoryStream strm)
        {
            // check root
            if (Root == null)
            {
                ConsoleIO.WriteLine("§cCan't execute message , Root not properly initialized.");
                return;
            }

            var node = Root.Select(strm);

            // check node
            if (node == null)
            {
                ConsoleIO.WriteLine("§cCan't execute stream, no valid sequence found");
            }
            // check callback
            else if (node.Callback == null)
            {
                ConsoleIO.WriteLineFormatted($"§cCan't execute msg '{node.Name}', no callback set");
            }
            // execute callback
            else
            {
                node.Callback(strm);
            }
        }

        public static bool PushNameToStream(string msgName, BitMemoryStream strm)
        {
            var res = Root.Select(msgName, strm) != null;

            if (!res)
            {
                ConsoleIO.WriteLineFormatted($"§epushNameToStream failed: Unknown message name '{msgName}'");
            }

            return res;
        }
    }
}