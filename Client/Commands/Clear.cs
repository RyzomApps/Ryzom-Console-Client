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

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            ConsoleIO.Reset();
            Console.Clear();

            return true;
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return ["cls"];
        }
    }
}