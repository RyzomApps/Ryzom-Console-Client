using API;
using API.Commands;
using Client.Stream;
using System;
using System.Collections.Generic;

namespace Client.Commands
{
    public class Ignore : CommandBase
    {
        public override string CmdName => "ignore";

        public override string CmdUsage => "<playerName>";

        public override string CmdDesc => "Add or remove a player from the ignore list";

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            // Check parameters.
            if (args.Length != 1)
            {
                responseMsg = "Please specify a name.";
                return false;
            }

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
                responseMsg = $"Unknown message named '{msgName}'.";
                return false;
            }

            return true;
        }
    }
}