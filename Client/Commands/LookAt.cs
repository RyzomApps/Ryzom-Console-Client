using API;
using API.Commands;
using API.Entity;
using Client.Config;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Client.Commands
{
    /// <summary>
    /// Look at a target, a direction or a position.
    /// Only works when SendPosition is enabled: the new orientation is stored
    /// on the user entity and sent with the next position update.
    /// </summary>
    public class LookAt : CommandBase
    {

        public override CommandCategory CmdCategory => CommandCategory.Interaction;
        public override string CmdName => "lookat";

        public override string CmdUsage => "[<name> | <direction 0-359> | <x> <y>]";

        public override string CmdDesc => "Look at the target, an entity, a direction (0 north, 360 degrees clockwise) or a position.";

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            var userEntity = ryzomClient.GetApiNetworkManager()?.GetApiEntityManager()?.GetApiUserEntity();

            if (userEntity == null)
            {
                responseMsg = "User entity missing.";
                return true;
            }

            if (!ClientConfig.SendPosition)
            {
                responseMsg = "LookAt requires SendPosition to be enabled (see sendposition in the config).";
                return true;
            }

            var args = GetArgs(command);
            float ori;

            if (args.Length == 0)
            {
                // Look at the current target.
                var slot = userEntity.TargetSlot();

                if (slot == Constants.InvalidSlot || slot == userEntity.Slot())
                {
                    responseMsg = "No target. Give a name, a direction or a position.";
                    return true;
                }

                var target = ryzomClient.GetApiNetworkManager().GetApiEntityManager().GetApiEntities()[slot];

                if (target == null || target.Slot() == Constants.InvalidSlot)
                {
                    responseMsg = $"Invalid target slot {slot}.";
                    return true;
                }

                ori = OriFromPoint(userEntity, target.Pos);
            }
            else if (args.Length == 1 && int.TryParse(args[0], out var dir) && dir >= 0 && dir < 360)
            {
                // Direction: 0 is north, increasing clockwise (user convention).
                ori = (float)(Math.PI / 2.0 - dir * Math.PI / 180.0);
            }
            else if (args.Length == 1)
            {
                // One argument that is not a direction: an entity name.
                var entity = ryzomClient.GetNetworkManager().GetEntityManager().GetEntityByName(string.Join(" ", args), false, true);

                if (entity == null)
                {
                    responseMsg = $"Could not find '{args[0]}'.";
                    return true;
                }

                ori = OriFromPoint(userEntity, entity.Pos);

                // Also target the entity like the 'target' command does.
                ryzomClient.GetNetworkManager().GetEntityManager().UserEntity.SetSelection(entity.Slot());
                ryzomClient.GetNetworkManager().GetEntityManager().UserEntity.SetTargetSlot(entity.Slot());
            }
            else if (args.Length == 2 && float.TryParse(args[0], out var x) && float.TryParse(args[1], out var y))
            {
                // Position in x y.
                ori = OriFromPoint(userEntity, new Vector3(x, y, userEntity.Pos.Z));
            }
            else
            {
                responseMsg = $"Usage: {CmdUsage}";
                return true;
            }

            // Store the orientation - it is sent with the next position update.
            userEntity.Front = new Vector3((float)Math.Cos(ori), (float)Math.Sin(ori), 0.0f);
            userEntity.Dir = userEntity.Front;

            return true;
        }

        /// <summary>
        /// Orientation in radians pointing from the user towards a point.
        /// Server convention: 0 is east, counter clockwise.
        /// </summary>
        private static float OriFromPoint(IEntity userEntity, Vector3 point)
        {
            return (float)Math.Atan2(point.Y - userEntity.Pos.Y, point.X - userEntity.Pos.X);
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return ["look"];
        }
    }
}
