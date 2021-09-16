using System.Diagnostics;
using RCC.Config;
using RCC.Helper;
using RCC.Msg;
using RCC.Network;

namespace RCC.Client
{
    /// <summary>
    /// Launch the game given a slot (slot is reference to the character summaries
    /// </summary>
    public static class CAHLaunchGame //: IActionHandler
    {
        public static void execute(string sSlot)
        {
            //// Get the edit/play mode
            //string sEditMode = getParam(Params, "edit_mode");
            //bool wantsEditMode = false;
            //CInterfaceExprValue result;
            //bool wantsNewScenario = false;
            //
            //if (CInterfaceExpr::eval(sEditMode, result))
            //{
            //	wantsEditMode = (result.getInteger() == 1) || (result.getInteger() == 2);
            //	wantsNewScenario = (result.getInteger() == 2);
            //}
            //
            //CInterfaceManager* im = CInterfaceManager::getInstance();
            //if (wantsEditMode)
            //{
            //	// full patch needed for edition, warn the client
            //	if (AvailablePatchs != 0)
            //	{
            //		if (im->isInGame())
            //		{
            //			inGamePatchUncompleteWarning();
            //		}
            //		else
            //		{
            //			im->messageBoxWithHelp(CI18N::get("uiBGD_FullPatchNeeded"), "ui:outgame");
            //		}
            //		return;
            //	}
            //}

            // Get the player selected slot
            //string sSlot = getParam(Params, "slot");
            if (sSlot != "ingame_auto")
            {
                //CInterfaceExprValue result;
                //if (!CInterfaceExpr::eval(sSlot, result))
                //    return;
                Connection.PlayerSelectedSlot = (byte)int.Parse(sSlot); //result.getInteger());
                if (Connection.PlayerSelectedSlot >= Connection.CharacterSummaries.Count)
                    return;

                //ClientCfg.writeInt("SelectedSlot", PlayerSelectedSlot);
                //if (ClientCfg.SaveConfig)
                //    ClientCfg.ConfigFile.save();
                ClientCfg.SelectCharacter = Connection.PlayerSelectedSlot;
            }


            /*
            static volatile bool isMainlandCharacter = false; // TMP until we can get this info
            if (isMainlandCharacter)
            {
                nlassert(0); // use id="message_box" !!!
                if (AvailablePatchs != 0)
                {
                    im->messageBoxWithHelp(CI18N::get("uiBGD_MainlandCharFullPatchNeeded"), "ui:outgame");
                }
                return;
            }
            */


            // Select the right sheet to create the user character.
            ClientCfg.UserSheet = Connection.CharacterSummaries[Connection.PlayerSelectedSlot].SheetId.ToString();

            // If the user wants to enter its editing session, get the ring server to Far TP to.
            //if (wantsEditMode)
            //{
            //
            //    if (wantsNewScenario)
            //    {
            //        CSessionBrowserImpl & sb = CSessionBrowserImpl::getInstance();
            //        sb.init(NULL);
            //        sb.closeEditSession(sb.getCharId());
            //        sb.waitOneMessage(CSessionBrowserImpl::getMessageName("on_invokeResult"));
            //    }
            //    if (FarTP.requestFarTPToSession((TSessionId)0, PlayerSelectedSlot, CFarTP::LaunchEditor, false))
            //    {
            //        WaitServerAnswer = true; // prepare to receive the character messages
            //    }
            //
            //    //			// If the player clicked 'Launch Editor', there was no CONNECTION:SELECT_CHAR sent yet,
            //    //			// so don't wait for the EGS to acknowledge our quit message as he does not know our character
            //    //			LoginSM.pushEvent(CLoginStateMachine::ev_ingame_return);
            //
            //    return;
            //}

            // Send CONNECTION:SELECT_CHAR
            var out2 = new CBitMemStream(false, 2);
            GenericMsgHeaderMngr.pushNameToStream("CONNECTION:SELECT_CHAR", out2);


            //CSelectCharMsg SelectCharMsg;
            var c = Connection.PlayerSelectedSlot;
            out2.serial(ref c);
            //if (!ClientCfg.Local/*ace!ClientCfg.Light*/)
            //{

            Debug.WriteLine(out2);

            NetworkManager.push(out2);

            ConsoleIO.WriteLineFormatted("§aimpulseCallBack : CONNECTION:SELECT_CHAR '" + Connection.PlayerSelectedSlot + "' sent.");

            //NetworkManager.send(NetworkConnection.getCurrentServerTick());
            //}

            // PlayerWantToGoInGame = true;

            //		CBitMemStream out2;
            //		if(GenericMsgHeaderMngr.pushNameToStream("CONNECTION:ENTER", out2))
            //		{
            //			NetMngr.push(out2);
            //			nlinfo("impulseCallBack : CONNECTION:ENTER sent");
            //		}
            //		else
            //			nlwarning("unknown message name : 'CONNECTION:ENTER'.");

            Connection.WaitServerAnswer = true;
            //if (ClientCfg.Local)
            //    serverReceivedReady = true;

            //NetworkManager.serverReceivedReady = true;
        }
    };
}