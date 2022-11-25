using System;
using System.Collections.Generic;
using API;
using API.Commands;
using Client.Stream;

namespace Client.Commands
{
    public class Ignore : CommandBase
    {
        public override string CmdName => "ignore";

        public override string CmdUsage => "<playerName>";

        public override string CmdDesc => "Add or remove a player from the ignore list";

        public override string Run(IClient handler, string command, Dictionary<string, object> localVars)
        {
            if (!(handler is RyzomClient ryzomClient))
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            // Check parameters.
            if (args.Length != 1)
                return "Please specify a name.";

            // NB: player names cannot have special characters
            var playerName = new string(args[0]);

            // add to the ignore list
            // add into server (NB: will be added by the server response later)
            const string msgName = "TEAM:CONTACT_ADD";
            var out2 = new BitMemoryStream();

            if (ryzomClient.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(msgName, out2))
            {
                byte list = 1; // IgnoreList

                out2.Serial(ref playerName);
                out2.Serial(ref list);

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