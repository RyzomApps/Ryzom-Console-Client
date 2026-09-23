using API;
using API.Commands;
using System;
using System.Collections.Generic;

namespace Client.Commands
{
    /// <summary>
    /// Display a dyn string value
    /// </summary>
    public class DynStr : CommandBase
    {
        public override string CmdName => "dynStr";

        public override string CmdUsage => "<string_id>";

        public override string CmdDesc => "Display the value of a dynamic string";

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

            var dynId = Convert.ToUInt32(args[0]);
            var networkManager = ryzomClient.GetNetworkManager();

            ryzomClient.GetStringManager().GetDynString(dynId, out responseMsg, networkManager);
            return true;
        }
    }
}