using API;
using API.Commands;
using Client.Stream;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Client.Commands
{
    /// <summary>
    /// Change the position of the user.
    /// </summary>
    public class Pos : CommandBase
    {
        public override string CmdName => "pos";

        public override string CmdUsage => "[x|teleportListName|botName] [y] [z]";

        public override string CmdDesc => "Retrieve or change the position of the user.";

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            var newPos = new Vector3();

            switch (args.Length)
            {
                case 0:
                    // Display the position
                    var user = ryzomClient?.GetApiNetworkManager()?.GetApiEntityManager()?.GetApiUserEntity();

                    responseMsg = user != null ? $"Your position is <{user.Pos.X:0} {user.Pos.Y:0}>." : "User entity missing.";
                    return true;

                case 1:
                    // Named destination.

                    // TODO: pos command get teleport position for name  Teleport.getPos(NLMISC.strlwr(dest))
                    // Here we try to teleport to a bot destination
                    const string msgName = "TP:BOT";
                    var @out = new BitMemoryStream();
                    if (ryzomClient.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(msgName, @out))
                    {
                        var str = args[0];
                        @out.Serial(ref str);
                        responseMsg = "TP:BOT sent";
                        ryzomClient.GetNetworkManager().Push(@out);
                    }
                    else
                    {
                        responseMsg = $"Unknown message named '{msgName}'.";
                        return true;
                    }

                    responseMsg = "";
                    return true;

                case 2:
                case 3:
                    // Teleport to anywhere.
                    newPos.X = float.Parse(args[0]);
                    newPos.Y = float.Parse(args[1]);
                    newPos.Z = args.Length == 3 ? float.Parse(args[2]) : 0.0f;
                    break;

                default:
                    // Bad argument number.
                    responseMsg = $"Usage: {CmdUsage}";
                    return true;
            }

            // Teleport to the right destination.
            var userEntity = ryzomClient.GetApiNetworkManager().GetApiEntityManager().GetApiUserEntity();

            if (userEntity != null)
            {
                // Set the user position here - TODO: Do a real teleportation
                userEntity.Pos = newPos;
            }
            else
            {
                responseMsg = "User entity missing.";
                return false;
            }

            // Command well done.
            return true;
        }
    }
}