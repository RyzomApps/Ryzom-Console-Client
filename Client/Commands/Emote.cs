using System;
using System.Collections.Generic;
using API;
using API.Commands;
using API.Entity;
using Client.Network;
using Client.Stream;

namespace Client.Commands
{
    // TODO: em command is broken - need to test on the real client for the phrase
    public class Emote : CommandBase
    {
        public override string CmdName => "em";

        public override string CmdUsage => "<custom emote text>";

        public override string CmdDesc => "Creates an emote without using an animation.";

        public override string Run(IClient handler, string command, Dictionary<string, object> localVars)
        {
            if (!(handler is RyzomClient ryzomClient))
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            if (args.Length < 1)
                return "";

            var emotePhrase = "";
            byte behavToSend = 60; //EMOTE_BEGIN <- first emote

            if (args.Length != 0)
            {
                emotePhrase = string.Join(" ", args);
            }

            // Create the message and send.
            const string msgName = "COMMAND:CUSTOM_EMOTE";
            var out2 = new BitMemoryStream();

            if (ryzomClient.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(msgName, out2))
            {
                var displayName = $"{EntityHelper.RemoveTitleAndShardFromName(ryzomClient.GetNetworkManager().PlayerSelectedHomeShardName)}";
                emotePhrase = $"&EMT&{displayName} {emotePhrase}";

                out2.Serial(ref behavToSend);
                out2.Serial(ref emotePhrase); // ucstring
                ryzomClient.GetNetworkManager().Push(out2);
            }
            else
                return $"Unknown message named '{msgName}'.";

            return "";
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return new[] { "emote" };
        }
    }
}