using System.Collections.Generic;
using System.Diagnostics;
using RCC.Commands.Internal;
using RCC.Network;

namespace RCC.Commands
{
    public class AddFriend : CommandBase
    {
        public override string CmdName => "AddFriend";
        public override string CmdUsage => "<contactName>";
        public override string CmdDesc => "";

        public override string Run(RyzomClient handler, string command, Dictionary<string, object> localVars)
        {
            var args = GetArgs(command);

            if (args.Length != 1)
            {
                handler.GetLogger().Warn($"Please specify a player name to add.");
                return "";
            }

            // add into server (NB: will be added by the server response later)
            const string msgName = "TEAM:CONTACT_ADD";
            var out2 = new BitMemoryStream();

            if (handler.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(msgName, out2))
            {
                byte list = 0; // friendslist
                string temp = args[0];

                out2.Serial(ref temp);
                out2.Serial(ref list);

                Debug.Print(out2.ToString());

                handler.GetNetworkManager().Push(out2);
            }
            else
                handler.GetLogger().Warn($"Unknown message named '{msgName}'.");

            return "";
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return new string[] { };
        }
    }
}