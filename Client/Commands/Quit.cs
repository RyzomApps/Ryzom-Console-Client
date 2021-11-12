﻿using System;
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
        public override string CmdDesc => "request to quit the game";

        public override string Run(IClient handler, string command, Dictionary<string, object> localVars)
        {
            if (!(handler is RyzomClient ryzomClient))
                throw new Exception("Command handler is not a Ryzom client.");

            // If we are not connected, quit now
            if (!handler.IsInGame())
            {
                ryzomClient.GetNetworkManager().GameExit = true;
                handler.GetLogger().Info("User Request to Quit ryzom");
            }
            else
            {
                // Don't quit but wait for server Quit
                const string msgName = "CONNECTION:CLIENT_QUIT_REQUEST";
                var out2 = new BitMemoryStream();
                ryzomClient.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(msgName, out2);
                var bypassDisconnectionTimer = false; // no need on a ring shard, as it's very short
                out2.Serial(ref bypassDisconnectionTimer);

                //FarTP.writeSecurityCodeForDisconnection(out2); // must always be written because of msg.xml (or could have a special handler in the FS)
                uint sessionId = 0;
                out2.Serial(ref sessionId);
                uint asNum = 0; // this has to be short, but thats not implemented
                out2.Serial(ref asNum);

                ryzomClient.GetNetworkManager().Push(out2);
                //nlinfo("impulseCallBack : %s sent", msgName.c_str());

                handler.GetLogger().Info("Initiating quit sequence... Please wait 30s for the logout.");
            }

            //Program.Exit();
            return "";
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return new[] {"exit", "disconnect"};
        }
    }
}