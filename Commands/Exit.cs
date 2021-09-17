using System.Collections.Generic;
using RCC.Client;

namespace RCC.Commands
{
    public class Exit : Command
    {
        public override string CmdName => "exit";
        public override string CmdUsage => "exit";
        public override string CmdDesc => "cmd.exit.desc";

        public override string Run(RyzomClient handler, string command, Dictionary<string, object> localVars)
        {
            Connection.GameExit = true;
            //Program.Exit();
            return "";
        }

        public override IEnumerable<string> getCMDAliases()
        {
            return new[] {"quit", "disconnect"};
        }
    }
}