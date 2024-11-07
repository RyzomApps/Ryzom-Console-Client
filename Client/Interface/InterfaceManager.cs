///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using System;
using System.Xml;
using API.Network.Web;
using Client.Brick;
using Client.Config;
using Client.Database;
using Client.Network.Web;
using Client.Phrase;
using Client.Skill;

namespace Client.Interface
{
    /// <summary>
    /// class managing the interface
    /// </summary>
    /// <author>Matthieu 'TrapII' Besson</author>
    /// <author>Nevrax France</author>
    /// <date>2002</date>
    public class InterfaceManager
    {
        private readonly RyzomClient _client;
        private readonly DatabaseManager _databaseManager;
        private readonly SkillManager _skillManager;
        private readonly BrickManager _brickManager;
        private readonly PhraseManager _phraseManager;
        private readonly IWebTransfer _webTransfer;

        ServerToLocalAutoCopy ServerToLocalAutoCopyInventory;
        ServerToLocalAutoCopy ServerToLocalAutoCopyExchange;
        ServerToLocalAutoCopy ServerToLocalAutoCopyDMGift;
        ServerToLocalAutoCopy ServerToLocalAutoCopyContextMenu;
        ServerToLocalAutoCopy ServerToLocalAutoCopySkillPoints;

        /** This is the GLOBAL Action counter used to synchronize some systems (including INVENTORY) with the server.*/
        byte _LocalSyncActionCounter;

        /// This is the Mask (4bits)
        byte _LocalSyncActionCounterMask;

        /// <summary>
        /// Constructor
        /// </summary>
        public InterfaceManager(RyzomClient client)
        {
            _client = client;
            _databaseManager = client.GetDatabaseManager();
            _skillManager = client.GetSkillManager();
            _brickManager = client.GetBrickManager();
            _phraseManager = client.GetPhraseManager();
            _webTransfer = client.GetWebTransfer();

            ServerToLocalAutoCopyInventory = new ServerToLocalAutoCopy(this, _databaseManager);
            ServerToLocalAutoCopyExchange = new ServerToLocalAutoCopy(this, _databaseManager);
            ServerToLocalAutoCopyDMGift = new ServerToLocalAutoCopy(this, _databaseManager);
            ServerToLocalAutoCopyContextMenu = new ServerToLocalAutoCopy(this, _databaseManager);
            ServerToLocalAutoCopySkillPoints = new ServerToLocalAutoCopy(this, _databaseManager);
        }

        /// <summary>
        /// initialize the whole in game interface
        /// </summary>
        public void InitInGame()
        {
            // Skill Manager Init
            _skillManager.InitInGame();

            // SBrick Manager Init
            _brickManager.InitInGame();
            _brickManager.InitTitles();

            // SPhrase Manager DB Init (BEFORE loading). Must be init AFTER skill and brick init
            _phraseManager.InitInGame();

            // Start the WebIG Thread
            WebigThread.StartThread(_client, _webTransfer);

            // Start the Browser Proxy
            if (ClientConfig.BrowserProxyEnabled)
                WebBrowserProxyThread.StartThread(_client, _webTransfer);
        }

        public void CreateLocalBranch(string fileName)
        {
            try
            {
                // open xml file
                var file = new XmlDocument();

                // Init an xml stream
                file.Load(fileName);
                file.Load(fileName);

                // Parse the parser output!!!
                var localNode = new DatabaseNodeBranch("LOCAL");
                localNode.Init(file.DocumentElement, null);
                _databaseManager.GetDb().AttachChild(localNode, "LOCAL");

                // Create the observers for auto-copy SERVER->LOCAL of inventory
                ServerToLocalAutoCopyInventory.Init("INVENTORY");

                // Create the observers for auto-copy SERVER->LOCAL of exchange
                ServerToLocalAutoCopyExchange.Init("EXCHANGE");

                // Create the observers for auto-copy SERVER->LOCAL of dm (animator) gift
                ServerToLocalAutoCopyDMGift.Init("DM_GIFT");

                // Create the observers for auto-copy SERVER->LOCAL of context menu
                ServerToLocalAutoCopyContextMenu.Init("TARGET:CONTEXT_MENU");

                // Create the observers for auto-copy SERVER->LOCAL of Skill Points
                ServerToLocalAutoCopySkillPoints.Init("USER");
            }
            catch (Exception e)
            {
                // Output error
                _client.GetLogger().Error($"InterfaceManager: Error while loading the form {fileName}: {e.Message}");
            }
        }

        internal bool localActionCounterSynchronizedWith(DatabaseNodeLeaf leaf)
        {
            if (leaf == null)
            {
                return false;
            }

            var srvVal = leaf.GetValue32();
            int locVal = _LocalSyncActionCounter;
            srvVal &= _LocalSyncActionCounterMask;
            locVal &= _LocalSyncActionCounterMask;
            return srvVal == locVal;
        }
    }
}
