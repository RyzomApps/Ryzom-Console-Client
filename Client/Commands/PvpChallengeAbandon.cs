using API;
using API.Commands;
using Client.Stream;
using System;
using System.Collections.Generic;

namespace Client.Commands
{
    public class PvpChallengeAbandon : CommandBase
    {

        public override CommandCategory CmdCategory => CommandCategory.Pvp;
        public override string CmdName => "PvpChallengeAbandon";

        public override string CmdUsage => "";

        public override string CmdDesc => "Legacy team PVP command: only the team leader may abandon the current group PVP challenge (server rejects non-leaders with CHALLENGE_ABANDON_NOT_LEADER). Nothing to do with 1v1 duels - see DuelAbandon; for League management see League / LeagueQuit.";

        public override bool CmdAsync => true;
        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            // Create the message for the server
            const string msgName = "PVP_CHALLENGE:ABANDON";
            var out2 = new BitMemoryStream();

            if (ryzomClient.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(msgName, out2))
            {
                ryzomClient.GetNetworkManager().Push(out2);
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
