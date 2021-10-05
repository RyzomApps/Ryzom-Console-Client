using System.Collections.Generic;
using RCC.Commands.Internal;
using RCC.Network;

namespace RCC.Commands
{
    public class BrutalQuit : CommandBase
    {
        public override string CmdName => "brutalQuit";
        public override string CmdUsage => "";
        public override string CmdDesc => "Instantaneously quits the game client";

        public override string Run(RyzomClient handler, string command, Dictionary<string, object> localVars)
        {
            Program.Exit();
            return "";
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return new string[] { };
        }
    }
}