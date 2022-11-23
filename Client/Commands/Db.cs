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

        public override string Run(IClient handler, string command, Dictionary<string, object> localVars)
        {
            if (!(handler is RyzomClient ryzomClient))
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            var size = args.Length;

            switch (size)
            {
                case 2:
                    {
                        // Convert the string into an Int64.
                        var value = long.Parse(args[1], CultureInfo.InvariantCulture);

                        // Set the property.
                        var prop = ryzomClient.GetDatabaseManager().GetDbProp(args[0]);

                        if (prop == null)
                            return $"{args[0]} was not found in the database.";

                        prop.SetValue64(value);

                        break;
                    }
                case 1:
                    {
                        var prop = ryzomClient.GetDatabaseManager().GetDbProp(args[0]);

                        if (prop == null)
                            return $"{args[0]} was not found in the database.";


                        var str = prop.GetValue64().ToString(CultureInfo.InvariantCulture);

                        return str;
                    }
                default:
                    return "Usage: " + CmdUsage;
            }

            return "";
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return new string[] { };
        }
    }
}