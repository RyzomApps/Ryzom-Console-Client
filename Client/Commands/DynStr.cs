using System;
using System.Collections.Generic;
using API;
using API.Commands;

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

        public override string Run(IClient handler, string command, Dictionary<string, object> localVars)
        {
            if (!(handler is RyzomClient ryzomClient))
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            if (args.Length != 1)
                return $"Usage: {CmdUsage}";

            var dynId = Convert.ToUInt32(args[0]);

            var networkManager = ryzomClient.GetNetworkManager();
            ryzomClient.GetStringManager().GetDynString(dynId, out var result, networkManager);

            return result;
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return new string[] { };
        }
    }
}