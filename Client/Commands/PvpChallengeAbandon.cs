﻿using System;
using System.Collections.Generic;
using API;
using API.Commands;
using Client.Stream;

namespace Client.Commands
{
    public class PvpChallengeAbandon : CommandBase
    {
        public override string CmdName => "PvpChallengeAbandon";

        public override string CmdUsage => "";

        public override string CmdDesc => "Gcm Un_pvp_challenge";

        public override string Run(IClient handler, string command, Dictionary<string, object> localVars)
        {
            if (!(handler is RyzomClient ryzomClient))
                throw new Exception("Command handler is not a Ryzom client.");

            // Create the message for the server to execute a phrase.
            const string msgName = "PVP_CHALLENGE:ABANDON";
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