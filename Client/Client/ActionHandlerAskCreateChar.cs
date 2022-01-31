///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using Client.Network;

namespace Client.Client
{
    /// <summary>
    /// Ask the server to create a character
    /// </summary>
    public static class ActionHandlerAskCreateChar
    {
        /// <summary>
        /// Execute the answer to the action
        /// </summary>
        public static void Execute(string sSlot, NetworkManager networkManager)
        {
            //CInterfaceManager pIM = CInterfaceManager.getInstance();

            // Create the message for the server to create the character.
            var out2 = new BitMemoryStream(false);

            if (!networkManager.GetMessageHeaderManager().PushNameToStream("CONNECTION:CREATE_CHAR", out2))
            {
                RyzomClient.GetInstance().GetLogger().Warn("don't know message name CONNECTION:CREATE_CHAR");
                return;
            }

            // Setup the name
            //string sEditBoxPath = getParam(Params, "name");
            string sFirstName = "NotSet";
            string sSurName = "NotSet";

            //CGroupEditBox pGEB = CWidgetManager.getInstance().getElementFromId(sEditBoxPath) as CGroupEditBox;
            //if (pGEB != null)
            //{
            //    sFirstName = pGEB.getInputString();
            //}
            //else
            //{
            //    nlwarning("can't get edit box name : %s", sEditBoxPath);
            //}

            // Build the character summary from the database branch ui:temp:char3d
            CharacterSummary CS = new CharacterSummary();

            //string sCharSumPath = getParam(Params, "charsum");
            //SCharacter3DSetup.setupCharacterSummaryFromDB(CS, sCharSumPath);
            //CS.Mainland = MainlandSelected;
            //CS.Name = ucstring.makeFromUtf8(sFirstName); // FIXME: UTF-8 (serial)
                                                           // CS.Surname = sSurName;

            // Create the message to send to the server from the character summary
            //CreateCharMsg CreateCharMsg = new CreateCharMsg();
            //
            //CreateCharMsg.setupFromCharacterSummary(CS);
            //
            //{
            //    // Slot
            //    string sSlot = getParam(Params, "slot");
            //
            //    CInterfaceExprValue result = new CInterfaceExprValue();
            //    if (!CInterfaceExpr.eval(sSlot, result))
            //    {
            //        return;
            //    }
            //
            //    CreateCharMsg.Slot = (byte)result.getInteger();
            //
            //    NLGUI.CDBManager.getInstance().getDbProp("UI:SELECTED_SLOT").setValue32(PlayerSelectedSlot);
            //}
            //
            //// Setup the new career
            //string sCaracBasePath = getParam(Params, "caracs");
            //CreateCharMsg.NbPointFighter = (byte)NLGUI.CDBManager.getInstance().getDbProp(sCaracBasePath + "FIGHT").getValue32();
            //CreateCharMsg.NbPointCaster = (byte)NLGUI.CDBManager.getInstance().getDbProp(sCaracBasePath + "MAGIC").getValue32();
            //CreateCharMsg.NbPointCrafter = (byte)NLGUI.CDBManager.getInstance().getDbProp(sCaracBasePath + "CRAFT").getValue32();
            //CreateCharMsg.NbPointHarvester = (byte)NLGUI.CDBManager.getInstance().getDbProp(sCaracBasePath + "FORAGE").getValue32();
            //
            //// Setup starting point
            //string sLocationPath = getParam(Params, "loc");
            //{
            //    CreateCharMsg.StartPoint = RYZOM_STARTING_POINT.borea;
            //
            //    CCDBNodeLeaf pNL = NLGUI.CDBManager.getInstance().getDbProp(sLocationPath, false);
            //    if (pNL != null)
            //    {
            //        CreateCharMsg.StartPoint = (RYZOM_STARTING_POINT.TStartPoint)(pNL.getValue64());
            //    }
            //    else
            //    {
            //        nlwarning(("Can't read starting point from the database : " + sLocationPath).c_str());
            //    }
            //
            //    if (CS.People == EGSPD.CPeople.Fyros)
            //    {
            //        CreateCharMsg.StartPoint = (RYZOM_STARTING_POINT.TStartPoint)(((byte)CreateCharMsg.StartPoint) + ((byte)RYZOM_STARTING_POINT.fyros_start));
            //    }
            //    else if (CS.People == EGSPD.CPeople.Matis)
            //    {
            //        CreateCharMsg.StartPoint = (RYZOM_STARTING_POINT.TStartPoint)(((byte)CreateCharMsg.StartPoint) + ((byte)RYZOM_STARTING_POINT.matis_start));
            //    }
            //    else if (CS.People == EGSPD.CPeople.Tryker)
            //    {
            //        CreateCharMsg.StartPoint = (RYZOM_STARTING_POINT.TStartPoint)(((byte)CreateCharMsg.StartPoint) + ((byte)RYZOM_STARTING_POINT.tryker_start));
            //    }
            //    else // if (CS.People == EGSPD::CPeople::Zorai)
            //    {
            //        CreateCharMsg.StartPoint = (RYZOM_STARTING_POINT.TStartPoint)(((byte)CreateCharMsg.StartPoint) + ((byte)RYZOM_STARTING_POINT.zorai_start));
            //    }
            //
            //}
            //
            //// Send the message to the server
            //CreateCharMsg.serialBitMemStream(@out);
            //if (!ClientCfg.Local)
            //{
            //    noUserChar = userChar = false;
            //
            //    NetMngr.push(@out);
            //    NetMngr.send(NetMngr.getCurrentServerTick());
            //
            //    //nlinfo("impulseCallBack : CONNECTION:CREATE_CHAR sent");
            //    CreateCharMsg.dump();
            //}
            //else
            //{
            //    userChar = true;
            //    if (CharacterSummaries.size() < 5)
            //    {
            //        CharacterSummaries.push_back(CS);
            //    }
            //}
            //
            //WaitServerAnswer = true;
        }
    }
}