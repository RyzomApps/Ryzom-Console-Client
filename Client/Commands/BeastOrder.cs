using System;
using System.Collections.Generic;
using API;
using API.Commands;
using Client.Stream;

namespace Client.Commands
{
    public enum AnimalsOrders
    {
        FOLLOW = 0,
        STOP,
        FREE,
        CALL,
        ENTER_STABLE,
        LEAVE_STABLE,
        GRAZE, //must be added later
        ATTACK,
        MOUNT, // For animal of type : Mount
        UNMOUNT, // For animal of type : Mount

        // the number of size existing
        BEAST_ORDERS_SIZE,
        UNKNOWN_BEAST_ORDER = BEAST_ORDERS_SIZE
    }


    /// <summary>
    /// Pack animal orders - give an order to the beast
    /// </summary>
    public class BeastOrder : CommandBase
    {
        public override string CmdName => "beastOrder";

        public override string CmdUsage => "<order> <index>";

        public override string CmdDesc => "Give an order to the beast";

        public override string Run(IClient handler, string command, Dictionary<string, object> localVars)
        {
            if (!(handler is RyzomClient ryzomClient))
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            if (args.Length != 2)
                return "Please specify two arguments.";

            if (!Enum.TryParse(args[0], out AnimalsOrders order))
            {
                return $"invalid beast order: {args[0]}.";
            }

            if (!long.TryParse(args[1], out var beastIndex))
            {
                return $"Can't read beast index: {args[1]}.";
            }

            if (order == AnimalsOrders.FREE)
            {
                return "Can't free a beast with the console client.";
            }

            // launch the command
            var @out = new BitMemoryStream();
            var msgName = "ANIMALS:BEAST";

            if (ryzomClient.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(msgName, @out))
            {
                byte u8BeastIndex = (byte)beastIndex;
                @out.Serial(ref u8BeastIndex); // to activate on server side
                                               // 0 -> all beasts, otherwise, the index of the beast

                byte u8Order = (byte)order;
                @out.Serial(ref u8Order);
                ryzomClient.GetNetworkManager().Push(@out);
            }
            else
            {
                return $"Unknown message named '{msgName}'.";
            }

            return "";
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return new[] { "" };
        }
    }
}