using System.Collections.Generic;
using RCC.Client;
using RCC.Helper;
using RCC.Msg;
using RCC.Network;

namespace RCC.Commands
{
    // TODO: say command is not working yet
    public partial class Say : Command
    {
        public override string CmdName => "say";
        public override string CmdUsage => "<custom text>";
        public override string CmdDesc => "Messages sent normally in the around channel have a 25m range. ";

        public override IEnumerable<string> getCMDAliases()
        {
            return new[] { "s" };
        }

        public override string Run(RyzomClient handler, string command, Dictionary<string, object> localVars)
        {
            var args = getArgs(command);

            if (args.Length == 0)
            {
                ConsoleIO.WriteLineFormatted("§cPlease enter a text.");
                return "";
            }

            string text = string.Join(" ", args);

            if (text.Length > 255)
                text = text.Substring(0, 255);

            CBitMemStream bms = new CBitMemStream();
            string msgType = "STRING:CHAT_MODE";
            byte mode = (byte)TGroupType.arround;
            uint dynamicChannelId = 0;
            if (GenericMsgHeaderMngr.pushNameToStream(msgType, bms))
            {
                bms.serial(ref mode);
                bms.serial(ref dynamicChannelId);
                NetworkManager.push(bms);
                //nlinfo("impulseCallBack : %s %d sent", msgType.c_str(), mode);
            }
            else
            {
                ConsoleIO.WriteLineFormatted($"§cUnknown message named '{msgType}'.");
            }

            // send str to IOS
            msgType = "STRING:CHAT";

            CBitMemStream out2 = new CBitMemStream();
            if (GenericMsgHeaderMngr.pushNameToStream(msgType, out2))
            {
                out2.serial(ref text);
                NetworkManager.push(out2);
            }
            else
                ConsoleIO.WriteLineFormatted($"§cUnknown message named '{msgType}'.");

            return "";
        }
    }
}