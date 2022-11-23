using System.Collections.Generic;
using API;
using API.Commands;
using Client.Database;

namespace Client.Commands
{
    /// <summary>
    /// Enable/Disable the log for the database.
    /// </summary>
    public class VerboseDatabase : CommandBase
    {
        public override string CmdName => "verboseDatabase";

        public override string CmdUsage => "";

        public override string CmdDesc => "Enable/Disable the log for the database.";

        public override string Run(IClient handler, string command, Dictionary<string, object> localVars)
        {
            var args = GetArgs(command);

            // Check parameters.
            if (args.Length != 0)
                return "Usage: " + CmdUsage;

            DatabaseManager.VerboseDatabase = !DatabaseManager.VerboseDatabase;

            return DatabaseManager.VerboseDatabase ? "enabled" : "disabled";
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return new string[] { };
        }
    }
}