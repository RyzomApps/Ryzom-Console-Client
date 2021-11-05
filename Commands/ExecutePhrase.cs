using RCC.Commands.Internal;
using RCC.Network;
using System.Collections.Generic;

namespace RCC.Commands
{
    public class ExecutePhrase : CommandBase
    {
        public override string CmdName => "executePhrase";
        public override string CmdUsage => "<[memoryId] [slotId]>";
        public override string CmdDesc => "Command to send the execution message for a phrase to the server.";

        public override string Run(RyzomClient handler, string command, Dictionary<string, object> localVars)
        {
            var args = GetArgs(command);

            if (args.Length < 1)
                return "";

            byte memoryId = 0;
            byte slotId = 0;

            if (args.Length == 2)
            {
                bool worked = byte.TryParse(args[0], out memoryId);
                worked &= byte.TryParse(args[1], out slotId);

                if (!worked)
                {
                    handler.GetLogger().Warn($"One of the arguments could not be parsed.");
                }
            }
            else if (args.Length == 1 || args.Length > 2)
            {
                handler.GetLogger().Warn($"Please specify zero or two arguments.");
                return "";
            }

            //TODO : ARGS
            var cyclic = true;

            // before, append the execution counter to the list of ACK to wait
            //appendCurrentToAckExecute(cyclic);

            // send msg
            BitMemoryStream out2 = new BitMemoryStream();
            string msgName;

            if (cyclic)
            {
                msgName = "PHRASE:EXECUTE_CYCLIC";
            }
            else
            {
                msgName = "PHRASE:EXECUTE";
            }
            if (handler.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(msgName, out2))
            {
                //serial the sentence memorized index
                out2.Serial(ref memoryId);
                out2.Serial(ref slotId);
                handler.GetNetworkManager().Push(out2);
            }
            else
            {
                handler.GetLogger().Warn($"Unknown message named '{msgName}'.");
            }

            return "";
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return new string[] { };
        }
    }
}