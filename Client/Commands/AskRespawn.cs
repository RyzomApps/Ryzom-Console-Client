using API;
using API.Commands;
using Client.Stream;
using System;
using System.Collections.Generic;

namespace Client.Commands
{
    public class AskRespawn : CommandBase
    {
        public override string CmdName => "AskRespawn";

        public override string CmdUsage => "<index>";

        public override string CmdDesc => "Client wants to re-spawn somewhere (index of the re-spawn location wanted)";

        public override IEnumerable<string> GetCmdAliases()
        {
            return ["respawn"];
        }

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            if (args.Length != 1)
            {
                responseMsg = "Please specify a parameter.";
                return false;
            }

            // send command
            const string msgName = "DEATH:ASK_RESPAWN";

            var index = ushort.Parse(args[0]);
            var out2 = new BitMemoryStream();

            if (ryzomClient.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(msgName, out2))
            {
                out2.Serial(ref index);
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