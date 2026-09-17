using System;
using System.Collections.Generic;
using API;
using API.Commands;

namespace Client.Commands
{
    /// <summary>
    /// Display a server string value
    /// </summary>
    public class ServerStr : CommandBase
    {
        public override string CmdName => "serverStr";

        public override string CmdUsage => "<dynId>";

        public override string CmdDesc => "Display a server string value";

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            if (args.Length != 1)
            {
                responseMsg = $"Usage: {CmdUsage}";
                return false;
            }

            var dynId = (uint)Convert.ToInt32(args[0]);

            var networkManager = ryzomClient.GetNetworkManager();
            ryzomClient.GetStringManager().GetString(dynId, out responseMsg, networkManager);

            return true;
        }
    }
}