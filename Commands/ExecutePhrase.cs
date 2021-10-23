using RCC.Commands.Internal;
using RCC.Network;
using System.Collections.Generic;

namespace RCC.Commands
{
    public class ExecutePhrase : CommandBase
    {
        public override string CmdName => "executePhrase";
        public override string CmdUsage => "<[memoryId] [slotId]>";
        public override string CmdDesc => "Command to send the execution msg for a phrase to the server";

        public override string Run(RyzomClient handler, string command, Dictionary<string, object> localVars)
        {
            var args = GetArgs(command);

            if (args.Length > 0)
                return "";

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
                byte memoryId = 0;
                byte slotId = 0;
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