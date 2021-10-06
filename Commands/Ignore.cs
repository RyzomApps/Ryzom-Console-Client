using System.Collections.Generic;
using RCC.Commands.Internal;
using RCC.Network;

namespace RCC.Commands
{
    public class Ignore : CommandBase
    {
        public override string CmdName => "ignore";

        public override string CmdUsage => "<player name>";

        public override string CmdDesc => "add or remove a player from the ignore list";

        public override string Run(RyzomClient handler, string command, Dictionary<string, object> localVars)
        {
            var args = GetArgs(command);

            // Check parameters.
            if (args.Length < 1) return "";

            // NB: playernames cannot have special characters
            var playerName = new string(args[0]);

            // add to the ignore list
            // add into server (NB: will be added by the server response later)
            const string msgName = "TEAM:CONTACT_ADD";
            var out2 = new BitMemoryStream();

            if (handler.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(msgName, out2))
            {
                byte list = 1; // IgnoreList

                out2.Serial(ref playerName);
                out2.Serial(ref list);

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