using System;
using System.Collections.Generic;
using API;
using API.Commands;
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
        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            if (args.Length < 2)
            {
                responseMsg = "Please specify more parameters.";
                return false;
            }

            var receiver = args[0];
            var str = string.Join(" ", args[1..]);

            if (receiver.Length > 255) receiver = receiver[..255];
            if (str.Length > 255) str = str[..255];

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
            {
                responseMsg = $"Unknown message named '{msgName}'.";
                return false;
            }

            return true;
        }
    }
}