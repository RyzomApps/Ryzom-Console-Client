using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using API;
using API.Commands;
using Client.Network;
using Client.Stream;

namespace Client.Commands
{
    /// <summary>
    /// Change the position of the user.
    /// </summary>
    public class Pos : CommandBase
    {
        public override string CmdName => "pos";

        public override string CmdUsage => "/pos <x, y, (z)> OR 1 name of 'tp.teleport_list'. or a bot name";

        public override string CmdDesc => "Change the position of the user.";

        public override string Run(IClient handler, string command, Dictionary<string, object> localVars)
        {
            if (!(handler is RyzomClient ryzomClient))
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            var newPos = new Vector3();

            switch (args.Length)
            {
                case 0:

                    // Display the position
                    var user = ryzomClient?.GetApiNetworkManager()?.GetApiEntityManager()?.GetApiUserEntity();
                    return user != null ? user.Pos.ToString() : "User entity missing.";

                case 1:
                    // Named destination.

                    // TODO: pos command get teleport position for name
                    //var dest = args[0];
                    //newPos = Teleport.getPos(NLMISC.strlwr(dest));
                    //if (newPos == Teleport.Unknown)
                    //{
                    //here we try to teleport to a bot destination
                    const string msgName = "TP:BOT";
                    var @out = new BitMemoryStream();
                    if (ryzomClient.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(msgName, @out))
                    {
                        var str = args[0];
                        @out.Serial(ref str);
                        handler.GetLogger().Debug("/pos: TP:BOT sent");
                        ryzomClient.GetNetworkManager().Push(@out);
                    }
                    else
                    {
                        return $"Unknown message named '{msgName}'.";
                    }

                    return "";
                //}
                case 2:
                case 3:
                    // Teleport to anywhere.
                    newPos.X = float.Parse(args[0]);
                    newPos.Y = float.Parse(args[1]);
                    newPos.Z = args.Length == 3 ? float.Parse(args[2]) : 0.0f;
                    break;
                default:
                    // Bad argument number.
                    return "Usage: " + CmdUsage;
            }

            // Teleport to the right destination.
            //Teleport(newPos);
            var userEntity = ryzomClient.GetApiNetworkManager().GetApiEntityManager().GetApiUserEntity();

            if (userEntity != null)
                userEntity.Pos = newPos;
            else
                return "User entity missing.";


            // Command well done.
            return "";
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return new string[] { };
        }
    }
}