using System;
using System.Collections.Generic;
using API;
using API.Commands;
using Client.Network;

namespace Client.Commands
{
    /// <summary>
    /// Engage target in combat
    /// </summary>
    public class Engage : CommandBase
    {
        public override string CmdName => "engage";

        public override string CmdUsage => "";

        public override string CmdDesc => "Engage target in combat";

        public override string Run(IClient handler, string command, Dictionary<string, object> localVars)
        {
            if (!(handler is RyzomClient ryzomClient))
                throw new Exception("Command handler is not a Ryzom client.");

            // Create the message for the server to execute a phrase.
            const string msgName = "COMBAT:ENGAGE";

            var @out = new BitMemoryStream();

            if (ryzomClient.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(msgName, @out))
                ryzomClient.GetNetworkManager().Push(@out);
            else
                return $"unknown message name : '{msgName}'.";

            return "";
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return new string[] { };
        }
    }
}