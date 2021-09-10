using System.Xml;
using RCC.Msg;

namespace RCC
{
    internal static class GenericMsgHeaderMngr
    {
        public static CNode _Root;

        // init
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
                ConsoleIO.WriteLine("Cannot open xml file '"+ filename + "', unable to initialize generic messages");
                return;
            }

            // create root node from root xml node
            _Root = new CNode(file.DocumentElement, 0);

            ConsoleIO.WriteLine("Loaded " + _Root.Nodes.Count + " messages nodes.");
        }
    }
}