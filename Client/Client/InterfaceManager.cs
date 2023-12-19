///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using System;
using System.Xml;
using API.Chat;
using Client.Brick;
using Client.Database;
using Client.Helper;
using Client.Network.WebIG;
using Client.Phrase;
using Client.Skill;

namespace Client.Client
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
            // TODO: WebigNotificationThread.StartWebIgNotificationThread(_client);
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

                //// Create the observers for auto-copy SERVER->LOCAL of inventory
                //ServerToLocalAutoCopyInventory.init("INVENTORY");

                //// Create the observers for auto-copy SERVER->LOCAL of exchange
                //ServerToLocalAutoCopyExchange.init("EXCHANGE");

                //// Create the observers for auto-copy SERVER->LOCAL of dm (animator) gift
                //ServerToLocalAutoCopyDMGift.init("DM_GIFT");

                //// Create the observers for auto-copy SERVER->LOCAL of context menu
                //ServerToLocalAutoCopyContextMenu.init("TARGET:CONTEXT_MENU");

                //// Create the observers for auto-copy SERVER->LOCAL of Skill Points
                //ServerToLocalAutoCopySkillPoints.init("USER");
            }
            catch (Exception e)
            {
                // Output error
                _client.GetLogger().Error($"InterfaceManager: Error while loading the form {fileName}: {e.Message}");
            }
        }
    }
}
