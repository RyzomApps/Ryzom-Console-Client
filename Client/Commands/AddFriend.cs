using System;
using System.Collections.Generic;
using API;
using API.Commands;
using Client.Stream;

namespace Client.Commands
{
    public class AddFriend : CommandBase
    {
        public override string CmdName => "AddFriend";

        public override string CmdUsage => "<contactName>";

        public override string CmdDesc => "Adds a friend to the contact list";

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            if (args.Length != 1)
            {
                responseMsg = "Please specify a player name to add.";
                return false;
            }

            // add into server (NB: will be added by the server response later)
            const string msgName = "TEAM:CONTACT_ADD";
            var out2 = new BitMemoryStream();

            if (ryzomClient.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(msgName, out2))
            {
                byte list = 0; // friendslist
                var temp = args[0];

                out2.Serial(ref temp);
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