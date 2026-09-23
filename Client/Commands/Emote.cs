using API;
using API.Commands;
using Client.ActionHandler;
using System;
using System.Collections.Generic;

namespace Client.Commands
{
    public class Emote : CommandBase
    {
        public override string CmdName => "em";

        public override string CmdUsage => "<emote phrase>";

        public override string CmdDesc => "Emote command";

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            var args = GetArgs(command);

            if (args.Length < 1)
            {
                responseMsg = "Usage: em <emote phrase>";
                return false;
            }

            // Build the emote phrase from all arguments
            var emotePhrase = string.Join(" ", args);

            // Run the action handler
            try
            {
                var actionHandler = new ActionHandlerEmote(handler);
                actionHandler.Execute(null, $"nb=0|behav=255|custom_phrase={emotePhrase}");
                return true;
            }
            catch (Exception ex)
            {
                responseMsg = $"Error executing emote: {ex.Message}";
                return false;
            }
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return ["emote"];
        }
    }
}