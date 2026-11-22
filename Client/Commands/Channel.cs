using API;
using API.Chat;
using API.Commands;
using System;
using System.Collections.Generic;

namespace Client.Commands
{
    public class Channel : CommandBase
    {

        public override CommandCategory CmdCategory => CommandCategory.Chat;
        public override string CmdName => "channel";

        public override string CmdUsage => "<name> [text] | dyn <name|index|id> [text ...]";

        public override string CmdDesc => "Change the chat channel. Use 'dyn <name|index|id>' for a player " +
            "chat channel; run 'ChatShare dyn' to list them.";

        public override bool CmdAsync => true;
        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            if (args.Length == 0)
            {
                responseMsg = $"Please specify a channel name: {string.Join(", ", Enum.GetNames(typeof(ChatGroupType)))}.";
                return false;
            }

            if (ChatShare.IsDynChatPrefix(args[0]))
            {
                if (args.Length < 2)
                {
                    responseMsg = "Usage: channel dyn <name|index|id> [text ...]. " +
                                  "Run 'ChatShare dyn' to list player chat channels.";
                    return false;
                }

                if (!ChatShare.TryResolveDynChannel(ryzomClient, args[1],
                        out var dynId, out var dynName, out var dynError))
                {
                    responseMsg = dynError;
                    return false;
                }

                ryzomClient.Channel = ChatGroupType.DynChat;
                ryzomClient.DynamicChannelId = dynId; // full 64-bit CEntityId

                // Set the server mode now; typed text then uses this channel even
                // before the queued first message is flushed.
                ryzomClient.GetNetworkManager().GetChatManager().SetChatMode(ChatGroupType.DynChat, dynId);

                if (args.Length > 2)
                {
                    var dynText = string.Join(" ", args[2..]);
                    ryzomClient.SendText(dynText);
                }

                responseMsg = $"Chat channel changed to player chat channel '{dynName}'.";
                return true;
            }

            if (!Enum.TryParse<ChatGroupType>(args[0], true, out var type))
            {
                responseMsg = $"Cannot find channel. Valid channels are: {string.Join(", ", Enum.GetNames(typeof(ChatGroupType)))}.";
                return false;
            }

            ryzomClient.Channel = type;

            if (args.Length <= 1)
                return true;

            // got some text to send
            var text = string.Join(" ", args[1..]);

            ryzomClient.SendText(text);

            return true;
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return ["sh", "y", "yell"];
        }
    }
}