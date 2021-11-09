using System.Collections.Generic;
using Client.Commands.Internal;
using Client.Network;

namespace Client.Commands
{
    /// <summary>
    /// This command is use to do all admin execution commands on you
    ///
    /// For example: "/a God 1" will set you in god mode
    /// </summary>
    /// TODO not sure if a command is working right
    public class A : CommandBase
    {
        public override string CmdName => "a";
        public override string CmdUsage => "<cmd> <arg>";
        public override string CmdDesc => "Execute an admin command on you";

        public override string Run(RyzomClient handler, string command, Dictionary<string, object> localVars)
        {
            var args = GetArgs(command);

            if (args.Length == 0) { return ""; }

            // generate the command
            var onTarget = false;
            var cmd = args[0];
            var arg = args.Length == 1 ? "" : string.Join(" ", args[1..]);
            const string msgName = "COMMAND:ADMIN";

            var out2 = new BitMemoryStream();
            if (handler.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(msgName, out2))
            {
                out2.Serial(ref onTarget);
                out2.Serial(ref cmd);
                out2.Serial(ref arg);

                handler.GetNetworkManager().Push(out2);
            }
            else
                handler.GetLogger().Warn($"Unknown message named '{msgName}'.");

            return "";
        }

        public override IEnumerable<string> GetCmdAliases() { return new string[] { }; }
    }
}