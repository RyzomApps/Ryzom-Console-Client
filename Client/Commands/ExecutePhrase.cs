using System;
using System.Collections.Generic;
using API;
using API.Commands;
using Client.Network;

namespace Client.Commands
{
    public class ExecutePhrase : CommandBase
    {
        public override string CmdName => "executePhrase";

        public override string CmdUsage => "<[memoryId] [slotId]>";

        public override string CmdDesc => "Command to send the execution message for a phrase to the server.";

        public override string Run(IClient handler, string command, Dictionary<string, object> localVars)
        {
            if (!(handler is RyzomClient ryzomClient))
                throw new Exception("Command handler is not a Ryzom client.");

            const bool cyclic = false;

            var args = GetArgs(command);

            if (args.Length < 1)
                return "";

            byte memoryId = 0;
            byte slotId = 0;

            if (args.Length == 2)
            {
                var worked = byte.TryParse(args[0], out memoryId);
                worked &= byte.TryParse(args[1], out slotId);

                if (!worked)
                {
                    return "One of the arguments could not be parsed.";
                }
            }
            else if (args.Length == 1 || args.Length > 2)
            {
                return "Please specify zero or two arguments.";
            }

            // before, append the execution counter to the list of ACK to wait
            //appendCurrentToAckExecute(cyclic);

            // send msg
            var out2 = new BitMemoryStream();
            const string msgName = cyclic ? "PHRASE:EXECUTE_CYCLIC" : "PHRASE:EXECUTE";

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