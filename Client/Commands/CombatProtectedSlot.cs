using System;
using System.Collections.Generic;
using API;
using API.Commands;
using Client.Stream;

namespace Client.Commands
{
    public class CombatProtectedSlot : CommandBase
    {
        public override string CmdName => "CombatProtectedSlot";

        public override string CmdUsage => "<slot>";

        public override string CmdDesc => "Select protected slot";

        public override string Run(IClient handler, string command, Dictionary<string, object> localVars)
        {
            if (!(handler is RyzomClient ryzomClient))
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            if (args.Length != 1)
                return "Please specify one argument.";

            var worked = byte.TryParse(args[0], out var slot);

            if (!worked)
                return "One of the arguments could not be parsed.";

            // send msg
            var out2 = new BitMemoryStream();
            var msgName = "COMBAT:PROTECTED_SLOT";

            if (ryzomClient.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(msgName, out2))
            {
                //serial the sentence memorized index
                out2.Serial(ref slot);
                ryzomClient.GetNetworkManager().Push(out2);
            }
            else
            {
                return $"Unknown message named '{msgName}'.";
            }

            return "";
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return new string[] { };
        }
    }
}