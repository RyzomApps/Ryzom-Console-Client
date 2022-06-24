using System;
using System.Collections.Generic;
using API;
using API.Commands;
using Client.Network;
using Client.Stream;

namespace Client.Commands
{
    public class Tell : CommandBase
    {
        public override string CmdName => "tell";

        public override string CmdUsage => "<receiver> <text>";

        public override string CmdDesc => "Transmit a chat message to the receiver.";

        /// <summary>
        /// Transmit a chat message to the receiver
        /// arg[0]      receiver is the name of the listening char (truncated to 255 char max)
        /// arg[1..]    str is the chat content(truncated to 255 char max)
        /// </summary>
        public override string Run(IClient handler, string command, Dictionary<string, object> localVars)
        {
            if (!(handler is RyzomClient ryzomClient))
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            if (args.Length < 2)
                return "";

            var receiver = args[0];
            var str = string.Join(" ", args[1..]);

            if (receiver.Length > 255) receiver = receiver.Substring(0, 255);
            if (str.Length > 255) str = str.Substring(0, 255);

            // Create the message and send.
            const string msgName = "STRING:TELL";
            var bms = new BitMemoryStream();

            if (ryzomClient.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(msgName, bms))
            {
                bms.Serial(ref receiver, false); // string
                bms.Serial(ref str); // ucstring
                ryzomClient.GetNetworkManager().Push(bms);
            }
            else
                return $"Unknown message named '{msgName}'.";

            return "";
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return new string[0];
        }
    }
}