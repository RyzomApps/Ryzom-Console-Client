using System;
using System.Collections.Generic;
using API;
using API.Commands;
using Client.Network;
using Client.Stream;

namespace Client.Commands
{
    /// <summary>
    /// This command is use to do all admin execution commands on you<br />
    /// For example: "/a God 1" will set you in god mode
    /// </summary>
    /// TODO check if the command is working right on the test server
    public class A : CommandBase
    {
        public override string CmdName => "a";

        public override string CmdUsage => "<cmd> <arg>";

        public override string CmdDesc => "Execute an admin command on you";

        public override string Run(IClient handler, string command, Dictionary<string, object> localVars)
        {
            if (!(handler is RyzomClient ryzomClient))
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            if (args.Length == 0)
            {
                return "Please specify an admin command.";
            }

            // generate the command
            var onTarget = false;
            var cmd = args[0];
            var arg = args.Length == 1 ? "" : string.Join(" ", args[1..]);
            const string msgName = "COMMAND:ADMIN";

            var out2 = new BitMemoryStream();

            if (ryzomClient.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(msgName, out2))
            {
                out2.Serial(ref onTarget);
                out2.Serial(ref cmd);
                out2.Serial(ref arg);

                ryzomClient.GetNetworkManager().Push(out2);
            }
            else
                return $"Unknown message named '{msgName}'.";

            return "";
        }

        public override IEnumerable<string> GetCmdAliases() { return new string[] { }; }
    }
}