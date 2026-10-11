using API;
using API.Commands;
using Client.Stream;
using System;
using System.Collections.Generic;

namespace Client.Commands
{
    /// <summary>
    /// Respawn at one of the available respawn points while dead.
    /// The <index> selects an entry from the respawn-point list the server sends on death
    /// (client caches it; in Ryzom Borea/terminal-style lists, 0 is usually the current spot).
    /// </summary>
    public class AskRespawn : CommandBase
    {

        public override CommandCategory CmdCategory => CommandCategory.Interaction;
        public override string CmdName => "AskRespawn";

        public override string CmdUsage => "<index>";

        public override string CmdDesc => "Respawn at one of the available respawn points while dead. The index selects an entry from the respawn-point list the server sends on death (usually 0 = respawn at current spot, 1..n = bind spots).";

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
