using System;
using System.Collections.Generic;
using API;
using API.Commands;

namespace Client.Commands
{
    public class ChatLog : CommandBase
    {
        public override string CmdName => "ChatLog";

        public override string CmdUsage => "";

        public override string CmdDesc => "Log all current chats in the file log_playername.txt saved in save directory.";

        public override string Run(IClient handler, string command, Dictionary<string, object> localVars)
        {
            if (!(handler is RyzomClient ryzomClient))
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            if (args.Length != 0)
                return "This command has no arguments.";

            if (ryzomClient.LogState)
            {
                ryzomClient.GetLogger().Info("Log turned off");
            }

            ryzomClient.LogState = !ryzomClient.LogState;

            if (ryzomClient.LogState)
            {
                ryzomClient.GetLogger().Info("Log turned on");
            }

            var node = ryzomClient.GetDatabaseManager().GetDbProp("UI:SAVE:CHATLOG_STATE", false);
            node?.SetValue32(ryzomClient.LogState ? 1 : 0);

            return "";
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return new[] { "" };
        }
    }
}