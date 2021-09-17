using System.Collections.Generic;
using RCC.Client;
using RCC.Messages;
using RCC.Network;

namespace RCC.Commands
{
    // TODO: say command is not working yet
    public class Say : Command
    {
        public override string CmdName => "say";
        public override string CmdUsage => "<custom text>";
        public override string CmdDesc => "Messages sent normally in the around channel have a 25m range. ";

        public override IEnumerable<string> getCMDAliases()
        {
            return new[] {"s"};
        }

        public override string Run(RyzomClient handler, string command, Dictionary<string, object> localVars)
        {
            var args = getArgs(command);

            if (args.Length == 0)
            {
                RyzomClient.Log?.Warn("Please enter a text.");
                return "";
            }

            string text = string.Join(" ", args);

            if (text.Length > 255)
                text = text.Substring(0, 255);

            BitMemoryStream bms = new BitMemoryStream();
            string msgType = "STRING:CHAT_MODE";
            byte mode = (byte) ChatGroupType.Around;
            uint dynamicChannelId = 0;
            if (GenericMessageHeaderManager.PushNameToStream(msgType, bms))
            {
                bms.Serial(ref mode);
                bms.Serial(ref dynamicChannelId);
                NetworkManager.Push(bms);
                //nlinfo("impulseCallBack : %s %d sent", msgType.c_str(), mode);
            }
            else
            {
                RyzomClient.Log?.Warn($"Unknown message named '{msgType}'.");
            }

            // send str to IOS
            msgType = "STRING:CHAT";

            var out2 = new BitMemoryStream();
            if (GenericMessageHeaderManager.PushNameToStream(msgType, out2))
            {
                out2.Serial(ref text);
                NetworkManager.Push(out2);
            }
            else
                RyzomClient.Log?.Warn($"Unknown message named '{msgType}'.");

            return "";
        }
    }
}