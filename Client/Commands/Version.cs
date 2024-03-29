﻿using System.Collections.Generic;
using API;
using API.Commands;

namespace Client.Commands
{
    public class Version : CommandBase
    {
        public override string CmdName => "version";

        public override string CmdUsage => "";

        public override string CmdDesc => "Display client version";

        public override string Run(IClient handler, string command, Dictionary<string, object> localVars)
        {
            return $"RCC {Program.Version} ({Resources.BuildDate})".Replace("\r\n", "").Replace("  ", " ");
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return new string[] { };
        }
    }
}