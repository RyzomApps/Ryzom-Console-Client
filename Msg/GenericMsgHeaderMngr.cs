using System;
using System.Xml;
using RCC.Helper;
using RCC.Network;

namespace RCC.Msg
{
    internal static class GenericMsgHeaderMngr
    {
        public static CNode _Root;

        /// <summary>
        /// init
        /// </summary>
        public static void init(string filename)
        {
            // open xml file
            XmlDocument file = new XmlDocument();

            try
            {
                // Init an xml stream
                file.Load(filename);
            }
            catch
            {
                ConsoleIO.WriteLine("Cannot open xml file '" + filename + "', unable to initialize generic messages");
                return;
            }

            // create root node from root xml node
            _Root = new CNode(file.DocumentElement, 0);

            ConsoleIO.WriteLine("Loaded " + _Root.Nodes.Count + " messages nodes.");
        }

        /// <summary>
        /// set callback
        /// </summary>
        public static bool setCallback(string msgName, Action<CBitMemStream> callback)
        {
            // check root
            if (_Root == null)
            {
                ConsoleIO.WriteLine("Can't set callback for message '" + msgName + "', Root not properly initialized.");
                return false;
            }

            // search for msg node
            CNode node = _Root.select(msgName);

            // check node
            if (node == null)
            {
                ConsoleIO.WriteLine("Can't set callback for message '" + msgName + "', message not found.");
                return false;
            }

            // set callback
            node.Callback = callback;

            return true;
        }

        /// <summary>
        /// execute
        /// </summary>
        public static void execute(CBitMemStream strm)
        {
            // check root
            if (_Root == null)
            {
                ConsoleIO.WriteLine("Can't execute message , Root not properly initialized.");
                return;
            }

            CNode node = _Root.select(strm);

            // check node
            if (node == null)
            {
                ConsoleIO.WriteLine("Can't execute stream, no valid sequence found");
            }
            // check callback
            else if (node.Callback == null)
            {
                ConsoleIO.WriteLine("Can't execute msg '" + node.Name + "', no callback set");
            }
            // execute callback
            else
            {
                node.Callback(strm);
            }
        }
    }
}