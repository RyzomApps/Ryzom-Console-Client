using System.Collections.Generic;
using RCC.Commands.Internal;
using RCC.Network;

namespace RCC.Commands
{
    public class Quit : CommandBase
    {
        public override string CmdName => "quit";
        public override string CmdUsage => "";
        public override string CmdDesc => "request to quit the game";

        public override string Run(RyzomClient handler, string command, Dictionary<string, object> localVars)
        {
            // If we are not connected, quit now
            if (!handler.IsInGame())
            {
                Connection.GameExit = true;
                RyzomClient.Log.Info("User Request to Quit ryzom");
            }
            else
            {
                // Don't quit but wait for server Quit
                const string msgName = "CONNECTION:CLIENT_QUIT_REQUEST";
                var out2 = new BitMemoryStream();
                handler.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(msgName, out2);
                var bypassDisconnectionTimer = false; // no need on a ring shard, as it's very short
                out2.Serial(ref bypassDisconnectionTimer);

                //FarTP.writeSecurityCodeForDisconnection(out2); // must always be written because of msg.xml (or could have a special handler in the FS)
                uint sessionId = 0;
                out2.Serial(ref sessionId);
                uint asNum = 0; // this has to be short, but thats not implemented
                out2.Serial(ref asNum);

                handler.GetNetworkManager().Push(out2);
                //nlinfo("impulseCallBack : %s sent", msgName.c_str());

                RyzomClient.Log.Info("Initiating quit sequence... Please wait 30s for the logout.");
            }

            //Program.Exit();
            return "";
        }

        public override IEnumerable<string> getCMDAliases()
        {
            return new[] {"exit", "disconnect"};
        }
    }
}