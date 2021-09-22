﻿using System.Collections.Generic;
using RCC.Commands.Internal;
using RCC.Messages;
using RCC.Network;

namespace RCC.Commands
{
    // TODO: em command is not working yet
    public class Emote : CommandBase
    {
        public override string CmdName => "em";
        public override string CmdUsage => "<custom emote text>";
        public override string CmdDesc => "Creates an emote without using an animation.";

        public override string Run(RyzomClient handler, string command, Dictionary<string, object> localVars)
        {
            var args = getArgs(command);

            if (args.Length < 1)
                return "";

            var emotePhrase = "";
            byte behavToSend = 1; //MBEHAV::IDLE;

            if (args.Length != 0)
            {
                emotePhrase = string.Join(" ", args);
            }

            // Create the message and send.
            const string msgName = "COMMAND:CUSTOM_EMOTE";
            var out2 = new BitMemoryStream();

            if (GenericMessageHeaderManager.PushNameToStream(msgName, out2))
            {
                emotePhrase = "&EMT&" + Connection.PlayerSelectedHomeShardName + " " + emotePhrase;

                out2.Serial(ref behavToSend);
                out2.Serial(ref emotePhrase);
                NetworkManager.Push(out2);
            }
            else
                RyzomClient.Log?.Warn($"Unknown message named '{msgName}'.");

            return "";
        }

        public override IEnumerable<string> getCMDAliases()
        {
            return new string[] { "emote" };
        }
    }
}