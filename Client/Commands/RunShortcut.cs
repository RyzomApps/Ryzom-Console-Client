using System.Collections.Generic;
using Client.Commands.Internal;

namespace Client.Commands
{
    /// <summary>
    /// Called when user hit the 1.2.3.4.5..... key
    /// </summary>
    public class RunShortcut : CommandBase
    {

        public override string CmdName => "RunShortcut";
        public override string CmdUsage => "<shortcut>";
        public override string CmdDesc => "Emulates the user hitting the <shortcut> key.";

        const string PhraseMemoryCtrlBase = "ui:interface:gestionsets:shortcuts:s";
        const string PhraseMemoryAltCtrlBase = "ui:interface:gestionsets2:header_closed:shortcuts:s";

        public override string Run(RyzomClient handler, string command, Dictionary<string, object> localVars)
        {
            var args = GetArgs(command);

            if (args.Length != 1 || !int.TryParse(args[0], out int shortcut))
            {
                return "";
            }

            if (shortcut < 0 || shortcut > 2 * Constants.RyzomMaxShortcut) return "";

            // get the control
            //CInterfaceElement	*elm;
            //if (shortcut < Constants.RyzomMaxShortcut)
                //elm = CWidgetManager::getInstance()->getElementFromId(PhraseMemoryCtrlBase + toString(shortcut) );
            //else
                //elm = CWidgetManager::getInstance()->getElementFromId(PhraseMemoryAltCtrlBase + toString(shortcut-RYZOM_MAX_SHORTCUT) );

            //CDBCtrlSheet		*ctrl= dynamic_cast<CDBCtrlSheet*>(elm);
            //if(ctrl)
            //{
            //    // run the standard cast case.
            //    if(ctrl->isMacro())
            //        CAHManager::getInstance()->runActionHandler("cast_macro", ctrl);
            //    else
            //        CAHManager::getInstance()->runActionHandler("cast_phrase", ctrl);
            //}

            //string msgName = "PHRASE:EXECUTE";
            //
            //BitMemoryStream out2 = new BitMemoryStream();
            //if (handler.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(msgName, out2))
            //{
            //serial the sentence memorized index
            //    uint8	memoryId= (uint8)memoryLine;
            //    uint8	slotId= (uint8)memorySlot;
            //    out.serial( memoryId );
            //    out.serial( slotId );
            //    handler.GetNetworkManager().Push(out2);
            //}
            //else
            //    handler.GetLogger().Warn($"Unknown message named '{msgName}'.");

            return "NOT IMPLEMENTED YET";
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return new string[] { };
        }
    }
}