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

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            // Disengage MSG.
            const string msgName = "COMBAT:DISENGAGE";
            var @out = new BitMemoryStream();

            if (ryzomClient.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(msgName, @out))
            {
                ryzomClient.GetNetworkManager().Push(@out);
            }
            else
            {
                responseMsg = $"Unknown message named '{msgName}'.";
                return false;
            }

            // Well Done.
            return true;
        }
    }
}