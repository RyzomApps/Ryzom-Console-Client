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

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            if (args.Length != 1)
            {
                responseMsg = "Please specify one argument.";
                return false;
            }

            var worked = byte.TryParse(args[0], out var slot);

            if (!worked)
            {
                responseMsg = "One of the arguments could not be parsed.";
                return false;
            }

            // send msg
            var out2 = new BitMemoryStream();
            const string msgName = "COMBAT:PROTECTED_SLOT";

            if (ryzomClient.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(msgName, out2))
            {
                //serial the sentence memorized index
                out2.Serial(ref slot);
                ryzomClient.GetNetworkManager().Push(out2);
            }
            else
            {
                responseMsg = $"Unknown message named '{msgName}'.";
                return false;
            }

            return true;
        }
    }
}