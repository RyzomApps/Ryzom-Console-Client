using RCC.Client;
using RCC.Commands.Internal;
using RCC.Network;
using System.Collections.Generic;

namespace RCC.Commands
{
    // TODO: em command seems to be broken (still)
    public class Emote : CommandBase
    {
        public override string CmdName => "em";
        public override string CmdUsage => "<custom emote text>";
        public override string CmdDesc => "Creates an emote without using an animation.";

        public override string Run(RyzomClient handler, string command, Dictionary<string, object> localVars)
        {
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

            if (handler.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(msgName, out2))
            {
                var displayName = $"{Entity.Entity.RemoveTitleAndShardFromName(handler.GetNetworkManager().PlayerSelectedHomeShardName)}";
                emotePhrase = $"&EMT&{displayName} {emotePhrase}";

                out2.Serial(ref behavToSend);
                out2.Serial(ref emotePhrase); // ucstring
                handler.GetNetworkManager().Push(out2);
            }
            else
                handler.GetLogger().Warn($"Unknown message named '{msgName}'.");

            return "";
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return new[] { "emote" };
        }
    }
}