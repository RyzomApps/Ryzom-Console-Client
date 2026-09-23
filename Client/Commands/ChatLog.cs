using API;
using API.Commands;
using System;
using System.Collections.Generic;

namespace Client.Commands
{
    public class ChatLog : CommandBase
    {
        public override string CmdName => "ChatLog";

        public override string CmdUsage => "";

        public override string CmdDesc => "Log all current chats in the file log_playername.txt saved in save directory.";

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            if (args.Length != 0)
            {
                responseMsg = "This command has no arguments.";
                return false;
            }

            ryzomClient.LogState = !ryzomClient.LogState;
            responseMsg = ryzomClient.LogState ? "Log turned on" : "Log turned off";

            var node = ryzomClient.GetDatabaseManager().GetServerNode("UI:SAVE:CHATLOG_STATE", false);
            node?.SetValue32(ryzomClient.LogState ? 1 : 0);

            return true;
        }
    }
}