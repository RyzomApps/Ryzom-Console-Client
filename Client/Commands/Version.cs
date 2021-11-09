using System.Collections.Generic;
using Client.Commands.Internal;

namespace Client.Commands
{
    public class Version : CommandBase
    {
        public override string CmdName => "version";

        public override string CmdUsage => "";

        public override string CmdDesc => "Display client version";

        public override string Run(RyzomClient handler, string command, Dictionary<string, object> localVars)
        {
            var sVersion = $"RCC {Program.Version} ({Resources.BuildDate})";

            handler.GetLogger().Info(sVersion.Replace("\r\n", "").Replace("  ", " "));

            return "";
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return new string[] { };
        }
    }
}