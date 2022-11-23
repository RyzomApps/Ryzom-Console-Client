using System;
using System.Collections.Generic;
using API;
using API.Commands;
using Client.Stream;

namespace Client.Commands
{
    public class ExecutePhrase : CommandBase
    {
        public override string CmdName => "executePhrase";

        public override string CmdUsage => "<memoryId> <slotId> [cyclic]";

        public override string CmdDesc => "Command to send the execution message for a phrase to the server.";

        public override string Run(IClient handler, string command, Dictionary<string, object> localVars)
        {
            if (!(handler is RyzomClient ryzomClient))
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            if (args.Length != 2 && args.Length != 3)
                return "Please specify two or three arguments.";

            var cyclic = false;

            var worked = byte.TryParse(args[0], out var memoryId);
            worked &= byte.TryParse(args[1], out var slotId);

            if (args.Length == 3)
                worked &= bool.TryParse(args[2], out cyclic);

            if (!worked)
                return "One of the arguments could not be parsed.";

            // send msg
            var out2 = new BitMemoryStream();
            var msgName = cyclic ? "PHRASE:EXECUTE_CYCLIC" : "PHRASE:EXECUTE";

            if (ryzomClient.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(msgName, out2))
            {
                //serial the sentence memorized index
                out2.Serial(ref memoryId);
                out2.Serial(ref slotId);
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