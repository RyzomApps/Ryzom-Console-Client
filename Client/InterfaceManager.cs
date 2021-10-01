///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

namespace RCC.Client
{
    internal static class InterfaceManager
    {
        public static void CreateLocalBranch(string fileName)
        {
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
