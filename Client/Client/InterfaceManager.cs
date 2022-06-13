///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using System.Diagnostics;
using Client.Brick;
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
        private readonly SkillManager _skillManager;
        private readonly BrickManager _brickManager;
        private readonly PhraseManager _phraseManager;

        /// <summary>
        /// Constructor
        /// </summary>
        public InterfaceManager(SkillManager skillManager, BrickManager brickManager, PhraseManager phraseManager)
        {
            _skillManager = skillManager;
            _brickManager = brickManager;
            _phraseManager = phraseManager;
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
        }

        public void CreateLocalBranch(string fileName)
        {
            Debug.Print("TODO");

            //try
            //{
            //    CIFile file = new CIFile();
            //
            //    if (file.open(fileName))
            //    {
            //        // Init an xml stream
            //        CIXml read = new CIXml();
            //        read.init(file);
            //
            //        //Parse the parser output!!!
            //        CCDBNodeBranch localNode = new CCDBNodeBranch("LOCAL");
            //        localNode.init(read.getRootNode(), progressCallBack);
            //        NLGUI.CDBManager.getInstance().getDB().attachChild(localNode, "LOCAL");
            //
            //        // Create the observers for auto-copy SERVER->LOCAL of inventory
            //        ServerToLocalAutoCopyInventory.init("INVENTORY");
            //
            //        // Create the observers for auto-copy SERVER->LOCAL of exchange
            //        ServerToLocalAutoCopyExchange.init("EXCHANGE");
            //
            //        // Create the observers for auto-copy SERVER->LOCAL of dm (animator) gift
            //        ServerToLocalAutoCopyDMGift.init("DM_GIFT");
            //
            //        // Create the observers for auto-copy SERVER->LOCAL of context menu
            //        ServerToLocalAutoCopyContextMenu.init("TARGET:CONTEXT_MENU");
            //
            //        // Create the observers for auto-copy SERVER->LOCAL of Skill Points
            //        ServerToLocalAutoCopySkillPoints.init("USER");
            //    }
            //}
            //catch (Exception e)
            //{
            //    // Output error
            //    nlwarning("CFormLoader: Error while loading the form %s: %s", fileName, e.what());
            //}
        }
    }
}
