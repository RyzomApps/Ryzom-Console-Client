using System;
using System.Collections.Generic;
using API;
using API.Commands;
using Client.Stream;

namespace Client.Commands
{
    /// <summary>
    /// Method to disengage the target.
    /// </summary>
    public class Disengage : CommandBase
    {
        public override string CmdName => "disengage";

        public override string CmdUsage => "";

        public override string CmdDesc => "Disengage from combat";

        public override string Run(IClient handler, string command, Dictionary<string, object> localVars)
        {
            if (!(handler is RyzomClient ryzomClient))
                throw new Exception("Command handler is not a Ryzom client.");

            // Disengage MSG.
            const string msgName = "COMBAT:DISENGAGE";
            var @out = new BitMemoryStream();

            if (ryzomClient.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(msgName, @out))
                ryzomClient.GetNetworkManager().Push(@out);
            else
                return $"Unknown message named '{msgName}'.";

            // Well Done.
            return "";
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return new string[] { };
        }
    }
}