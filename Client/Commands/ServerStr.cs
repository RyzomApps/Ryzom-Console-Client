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

        public override string Run(IClient handler, string command, Dictionary<string, object> localVars)
        {
            if (!(handler is RyzomClient ryzomClient))
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            if (args.Length != 1) 
                return "Usage: " + CmdUsage;

            var dynId = (uint)Convert.ToInt32(args[0]);

            var networkManager = ryzomClient.GetNetworkManager();
            ryzomClient.GetStringManager().GetString(dynId, out var result, networkManager);

            handler.GetLogger().Info(result);

            return "";
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return new string[] { };
        }
    }
}