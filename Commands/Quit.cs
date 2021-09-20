using System.Collections.Generic;
using RCC.Messages;
using RCC.Network;

namespace RCC.Commands
{
    public class Quit : Command
    {
        public override string CmdName => "quit";
        public override string CmdUsage => "";
        public override string CmdDesc => "request to quit the game";

        public override string Run(RyzomClient handler, string command, Dictionary<string, object> localVars)
        {
            // If we are not connected, quit now TODO
            if (false /*!RyzomClient.ConnectionReadySent*/)
            {
                Connection.GameExit = true;
                RyzomClient.Log.Info("User Request to Quit ryzom");
            }
            else
            {
                // Don't quit but wait for server Quit
                const string msgName = "CONNECTION:CLIENT_QUIT_REQUEST";
                var out2 = new BitMemoryStream();
                GenericMessageHeaderManager.PushNameToStream(msgName, out2);
                var bypassDisconnectionTimer = false; // no need on a ring shard, as it's very short
                out2.Serial(ref bypassDisconnectionTimer);

                //FarTP.writeSecurityCodeForDisconnection(out2); // must always be written because of msg.xml (or could have a special handler in the FS)
                uint sessionId = 0;
                out2.Serial(ref sessionId);
                uint asNum = 0; // this has to be short, but thats not implemented
                out2.Serial(ref asNum);

                NetworkManager.Push(out2);
                //nlinfo("impulseCallBack : %s sent", msgName.c_str());
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