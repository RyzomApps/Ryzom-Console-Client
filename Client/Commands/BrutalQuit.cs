using API;
using API.Commands;
using System.Collections.Generic;

namespace Client.Commands
{
    public class BrutalQuit : CommandBase
    {
        public override string CmdName => "brutalQuit";
        public override string CmdUsage => "";
        public override string CmdDesc => "Instantaneously quits the game client";

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            Program.Exit();
            return true;
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return ["ragequit", "forcequit"];
        }
    }
}