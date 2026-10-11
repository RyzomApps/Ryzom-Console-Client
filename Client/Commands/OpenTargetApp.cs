using API;
using API.Commands;
using System;
using System.Collections.Generic;

namespace Client.Commands
{
    public class OpenTargetApp : CommandBase
    {

        public override CommandCategory CmdCategory => CommandCategory.Npc;
        public override string CmdName => "OpenTargetApp";

        public override string CmdUsage => "";

        public override string CmdDesc => "Open the app page of the currently targeted NPC (requires the NPC to have the web page bot chat flag). Unlike BrowseNpcWebPage the server sends its own hardcoded app URL, so no URL can be injected.";

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            // Check parameters and perform admin command
            // The server requires at least one argument, but ignores it.
            if (args.Length == 0)
                return ryzomClient.PerformInternalCommand("a openTargetApp 0", out responseMsg);

            responseMsg = "Wrong argument count in the command.";
            return false;
        }
    }
}
