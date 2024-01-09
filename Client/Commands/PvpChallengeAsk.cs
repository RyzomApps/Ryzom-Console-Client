﻿using System;
using System.Collections.Generic;
using API;
using API.Commands;
using Client.Stream;

namespace Client.Commands
{
    public class PvpChallengeAsk : CommandBase
    {
        public override string CmdName => "PvpChallengeAsk";

        public override string CmdUsage => "";

        public override string CmdDesc => "Gcm Pvp Challenge";

        public override string Run(IClient handler, string command, Dictionary<string, object> localVars)
        {
            if (!(handler is RyzomClient ryzomClient))
                throw new Exception("Command handler is not a Ryzom client.");

            // Create the message for the server
            const string msgName = "PVP_CHALLENGE:ASK";
            var out2 = new BitMemoryStream();

            if (ryzomClient.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(msgName, out2))
                ryzomClient.GetNetworkManager().Push(out2);
            else
                return $"Unknown message named '{msgName}'.";

            return "";
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return new string[] { };
        }
    }
}