using System;
using System.Collections.Generic;
using API;
using API.Commands;
using Client.Stream;

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

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            // Create the message for the server
            const string msgName = "COMBAT:ENGAGE";

            var @out = new BitMemoryStream();

            if (ryzomClient.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(msgName, @out))
            {
                ryzomClient.GetNetworkManager().Push(@out);
            }
            else
            {
                responseMsg = $"unknown message name : '{msgName}'.";
                return false;
            }

            return true;
        }
    }
}