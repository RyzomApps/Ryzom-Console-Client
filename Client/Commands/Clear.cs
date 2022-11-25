using System;
using System.Collections.Generic;
using API;
using API.Commands;
using Client.Helper;

namespace Client.Commands
{
    /// <summary>
    /// Clear content of current char window
    /// </summary>
    public class Clear : CommandBase
    {
        public override string CmdName => "clear";

        public override string CmdUsage => "";

        public override string CmdDesc => "Clears the content of the console";

        public override string Run(IClient handler, string command, Dictionary<string, object> localVars)
        {
            ConsoleIO.Reset();
            Console.Clear();

            return "";
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return new[] {"cls"};
        }
    }
}