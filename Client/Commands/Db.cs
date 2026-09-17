using System;
using System.Collections.Generic;
using System.Globalization;
using API;
using API.Commands;

namespace Client.Commands
{
    /// <summary>
    /// Modify Database
    /// </summary>
    public class Db : CommandBase
    {
        public override string CmdName => "db";

        public override string CmdUsage => "<Property> <Value>";

        public override string CmdDesc => "Modify Database";

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            var size = args.Length;

            switch (size)
            {
                case 1:
                    var prop1 = ryzomClient.GetDatabaseManager().GetNode(args[0], false);

                    if (prop1 == null)
                    {
                        responseMsg = $"{args[0]} was not found in the database.";
                        return false;
                    }

                    responseMsg = prop1.GetValue64().ToString(CultureInfo.InvariantCulture);

                    break;

                case 2:
                    // Convert the string into an Int64.
                    var value = long.Parse(args[1], CultureInfo.InvariantCulture);

                    // Set the property.
                    var prop2 = ryzomClient.GetDatabaseManager().GetNode(args[0], true);

                    if (prop2 == null)
                    {
                        responseMsg = $"{args[0]} was not found in the database.";
                        return false;
                    }

                    prop2.SetValue64(value);

                    break;

                default:
                    responseMsg = $"Usage: {CmdUsage}";
                    return false;
            }

            return true;
        }
    }
}