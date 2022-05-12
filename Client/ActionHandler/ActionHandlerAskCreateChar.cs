///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using Client.Client;
using Client.Messages;
using Client.Network;

namespace Client.ActionHandler
{
    /// <summary>
    /// Ask the server to create a character
    /// </summary>
    public static class ActionHandlerAskCreateChar
    {
        /// <summary>
        /// Execute the answer to the action
        /// </summary>
        public static void Execute(string name, byte slot, NetworkManager networkManager)
        {
            //CInterfaceManager pIM = CInterfaceManager.getInstance();

            // Create the message for the server to create the character.
            var out2 = new BitMemoryStream();

            if (!networkManager.GetMessageHeaderManager().PushNameToStream("CONNECTION:CREATE_CHAR", out2))
            {
                RyzomClient.GetInstance().GetLogger().Warn("don't know message name CONNECTION:CREATE_CHAR");
                return;
            }

            // Setup the name
            //string sEditBoxPath = getParam(Params, "name");
            var sFirstName = name; //"NotSet";
            //string sSurName = "NotSet";

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
            var cs = new CharacterSummary {Mainland = networkManager.MainlandSelected, Name = sFirstName};

            //string sCharSumPath = getParam(Params, "charsum");
            //SCharacter3DSetup.setupCharacterSummaryFromDB(CS, sCharSumPath);

            // FIXME: UTF-8 (serial)
            // CS.Surname = sSurName;

            // Create the message to send to the server from the character summary
            var createCharMsg = new CreateCharMsg();

            createCharMsg.SetupFromCharacterSummary(cs);

            // Slot
            //{
            //    
            //    string sSlot = getParam(Params, "slot");
            //
            //    CInterfaceExprValue result = new CInterfaceExprValue();
            //    if (!CInterfaceExpr.eval(sSlot, result))
            //    {
            //        return;
            //    }
            //
            //createCharMsg.Slot = slot;
            //
            //    NLGUI.CDBManager.getInstance().getDbProp("UI:SELECTED_SLOT").setValue32(PlayerSelectedSlot);
            //}

            //// Setup the new career
            //string sCaracBasePath = getParam(Params, "caracs");
            createCharMsg.NbPointFighter = 2; //(byte)NLGUI.CDBManager.getInstance().getDbProp(sCaracBasePath + "FIGHT").getValue32();
            createCharMsg.NbPointCaster = 1;  //(byte)NLGUI.CDBManager.getInstance().getDbProp(sCaracBasePath + "MAGIC").getValue32();
            createCharMsg.NbPointCrafter = 1;  //(byte)NLGUI.CDBManager.getInstance().getDbProp(sCaracBasePath + "CRAFT").getValue32();
            createCharMsg.NbPointHarvester = 1;  //(byte)NLGUI.CDBManager.getInstance().getDbProp(sCaracBasePath + "FORAGE").getValue32();

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

            //// Send the message to the server
            createCharMsg.SerialBitMemStream(out2);

            //if (!ClientCfg.Local)
            //{
            //    noUserChar = userChar = false;
            //
            networkManager.Push(out2);
            //networkManager.Send(networkManager.GetNetworkConnection().GetCurrentServerTick());

            RyzomClient.GetInstance().GetLogger().Info("impulseCallBack : CONNECTION:CREATE_CHAR sent");

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