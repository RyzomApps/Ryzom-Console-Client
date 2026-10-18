using API;
using API.Commands;
using Client.ActionHandler;
using System;
using System.Collections.Generic;

namespace Client.Commands
{
    public class Emote : CommandBase
    {

        public override CommandCategory CmdCategory => CommandCategory.Chat;
        public override string CmdName => "em";
        public override bool CmdAsync => true;

        public override string CmdUsage => "<emote phrase>";

        public override string CmdDesc => "Perform an emote (only visible in the around chat). With a custom phrase: '/em says Hello' shows as 'Takopi says hello'.";

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
