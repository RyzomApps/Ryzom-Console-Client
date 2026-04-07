///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using API;
using API.Commands;
using Client.ActionHandler;

namespace Client.Commands
{
    public class Emote : CommandBase
    {
        public override string CmdName => "em";

        public override string CmdUsage => "<emote phrase>";

        public override string CmdDesc => "Emote command";

        public override string Run(IClient handler, string command, Dictionary<string, object> localVars)
        {
            var args = GetArgs(command);

            if (args.Length < 1)
                return "Usage: em <emote phrase>";

            // Build the emote phrase from all arguments
            string emotePhrase = string.Join(" ", args);

            // Run the action handler
            try
            {
                var actionHandler = new ActionHandlerEmote(handler);
                actionHandler.Execute(null, $"nb=0|behav=255|custom_phrase={emotePhrase}");
                return "";
            }
            catch (Exception ex)
            {
                return $"Error executing emote: {ex.Message}";
            }
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return ["emote"];
        }
    }
}