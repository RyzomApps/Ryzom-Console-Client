using API;
using API.Commands;
using System.Collections.Generic;

namespace Client.Commands
{
    public class Version : CommandBase
    {
        public override string CmdName => "version";

        public override string CmdUsage => "";

        public override string CmdDesc => "Display client version";

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = $"RCC {Program.Version} ({Resources.BuildDate})".Replace("\r\n", "").Replace("  ", " ");
            return true;
        }
    }
}