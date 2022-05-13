using System;
using System.Collections.Generic;
using API;
using API.Commands;
using Client.Network;

namespace Client.Commands
{
    public class AddFriend : CommandBase
    {
        public override string CmdName => "AddFriend";
        public override string CmdUsage => "<contactName>";
        public override string CmdDesc => "Adds a friend to the contact list";

        public override string Run(IClient handler, string command, Dictionary<string, object> localVars)
        {
            if (!(handler is RyzomClient ryzomClient))
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            if (args.Length != 1)
            {
                handler.GetLogger().Warn($"Please specify a player name to add.");
                return "";
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

                //Debug.Print(out2.ToString());

                ryzomClient.GetNetworkManager().Push(out2);
            }
            else
                handler.GetLogger().Warn($"Unknown message named '{msgName}'.");

            return "";
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return new string[] { };
        }
    }
}