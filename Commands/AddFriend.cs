using System.Collections.Generic;
using System.Diagnostics;
using RCC.Messages;
using RCC.Network;

namespace RCC.Commands
{
    public class AddFriend : Command
    {
        public override string CmdName => "AddFriend";
        public override string CmdUsage => "<contactName>";
        public override string CmdDesc => "";

        public override string Run(RyzomClient handler, string command, Dictionary<string, object> localVars)
        {
            var args = getArgs(command);

            if (args.Length != 1)
            {
                RyzomClient.Log?.Warn($"Please specify a player name to add.");
                return "";
            }

            // add into server (NB: will be added by the server response later)
            string msgName = "TEAM:CONTACT_ADD";
            BitMemoryStream out2 = new BitMemoryStream();

            if (GenericMessageHeaderManager.PushNameToStream(msgName, out2))
            {
                byte list = 0; // friendslist
                string temp = args[0];

                out2.Serial(ref temp);
                out2.Serial(ref list);

                Debug.Print(out2.ToString());

                NetworkManager.Push(out2);
            }
            else
                RyzomClient.Log?.Warn($"Unknown message named '{msgName}'.");

            return "";
        }

        public override IEnumerable<string> getCMDAliases()
        {
            return new string[] { };
        }
    }
}