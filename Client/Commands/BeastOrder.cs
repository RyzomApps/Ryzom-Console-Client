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

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            if (args.Length != 2)
            {
                responseMsg = "Please specify two arguments.";
                return false;
            }

            if (!Enum.TryParse(args[0], out AnimalsOrders order))
            {
                responseMsg = $"invalid beast order: {args[0]}.";
                return false;
            }


            if (!long.TryParse(args[1], out var beastIndex))
            {
                responseMsg = $"Can't read beast index: {args[1]}.";
                return false;
            }

            if (order == AnimalsOrders.FREE)
            {
                responseMsg = "Can't free a beast with the console client.";
                return false;
            }

            // launch the command
            var @out = new BitMemoryStream();
            const string msgName = "ANIMALS:BEAST";

            if (ryzomClient.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(msgName, @out))
            {
                var u8BeastIndex = (byte)beastIndex;
                @out.Serial(ref u8BeastIndex); // to activate on server side
                // 0 -> all beasts, otherwise, the index of the beast

                var u8Order = (byte)order;
                @out.Serial(ref u8Order);
                ryzomClient.GetNetworkManager().Push(@out);
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