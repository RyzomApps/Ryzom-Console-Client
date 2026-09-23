using API;
using API.Commands;
using Client.Database;
using System.Collections.Generic;

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

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            var args = GetArgs(command);

            // Check parameters.
            if (args.Length != 0)
            {
                responseMsg = $"Usage: {CmdUsage}";
                return false;
            }

            DatabaseManager.VerboseDatabase = !DatabaseManager.VerboseDatabase;

            responseMsg = DatabaseManager.VerboseDatabase ? "enabled" : "disabled";
            return true;
        }
    }
}