using System;
using System.Collections.Generic;
using API;
using API.Commands;
using Client.Network;

namespace Client.Commands
{
    public class Quit : CommandBase
    {
        public override string CmdName => "quit";
        public override string CmdUsage => "";
        public override string CmdDesc => "Request to quit the game. The logout will usually take 30s.";

        public override string Run(IClient handler, string command, Dictionary<string, object> localVars)
        {
            if (!(handler is RyzomClient ryzomClient))
                throw new Exception("Command handler is not a Ryzom client.");

            // If we are not connected, quit now
            if (!handler.IsInGame())
            {
                ryzomClient.GetNetworkManager().GameExit = true;
                return "User Request to Quit ryzom";
            }

            // Don't quit but wait for server Quit
            const string msgName = "CONNECTION:CLIENT_QUIT_REQUEST";
            var out2 = new BitMemoryStream();
            ryzomClient.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(msgName, out2);
            var bypassDisconnectionTimer = false; // no need on a ring shard, as it's very short
            out2.Serial(ref bypassDisconnectionTimer);

            //FarTP.writeSecurityCodeForDisconnection(out2); // must always be written because of msg.xml (or could have a special handler in the FS)
            uint sessionId = 0;
            out2.Serial(ref sessionId);
            short asNum = 0;
            out2.Serial(ref asNum);

            ryzomClient.GetNetworkManager().Push(out2);
            //nlinfo("impulseCallBack : %s sent", msgName.c_str());

            return "Initiating quit sequence... Please wait 30s for the logout.";
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return new[] {"exit", "disconnect"};
        }
    }
}