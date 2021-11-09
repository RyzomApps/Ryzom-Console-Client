﻿using System.Collections.Generic;
using Client.Commands.Internal;

namespace Client.Commands
{
    /// <summary>
    /// Create a file with the current state of the client (good to report a bug)
    /// </summary>
    public class Dump : CommandBase
    {
        public override string CmdName => "dump";
        public override string CmdUsage => "<dump name>";
        public override string CmdDesc => "Command to create a file with the current state of the client";

        public override string Run(RyzomClient handler, string command, Dictionary<string, object> localVars)
        {
            var args = GetArgs(command);

            if (args.Length > 1)
                return "";

            var dumpName = args.Length == 1 ? args[0] : "default";

            // Write the DB
            handler.GetDatabaseManager().Write(dumpName + "_db.rec");

            // TODO: Other dump files

            return "";
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return new string[] { };
        }
    }
}