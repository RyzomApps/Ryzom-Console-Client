﻿using System;
using System.Collections.Generic;
using API;
using API.Commands;
using Client.Config;

namespace Client.Commands
{
    /// <summary>
    /// Create a file with the current state of the client (good to report a bug)
    /// </summary>
    public class Dump : CommandBase
    {
        public override string CmdName => "dump";
        public override string CmdUsage => "[name]";
        public override string CmdDesc => "Command to create a file with the current state of the client";

        public override string Run(IClient handler, string command, Dictionary<string, object> localVars)
        {
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            if (args.Length > 1)
                return "Please specify less parameters.";

            var dumpName = args.Length == 1 ? args[0] : "default";

            // TODO: Write information to start as the version (RecordVersion, currentPos)

            // Write the DB
            if (ClientConfig.UseDatabase)
                ryzomClient.GetDatabaseManager().Write($"{dumpName}_db.rec");

            // TODO: Dump Client CFG.

            // TODO: Dump entities.

            ryzomClient.GetPhraseManager().Write($"{dumpName}_phrases.rec");

            if (ClientConfig.UseInventory)
                ryzomClient.GetInventoryManager().Write($"{dumpName}_inventory.rec");

            return "";
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return new string[] { };
        }
    }
}