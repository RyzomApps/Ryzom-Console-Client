using System;
using System.Collections.Generic;
using API;
using API.Commands;
using Client.Stream;

namespace Client.Commands
{
    public class DynchatClickOption : CommandBase
    {
        public override string CmdName => "DynchatClickOption";

        public override string CmdUsage => "<index>";

        public override string CmdDesc => "Execution of dynamic chat option clicks by sending the selected option to the bot and closing the chat bubble.";

        public override string Run(IClient handler, string command, Dictionary<string, object> localVars)
        {
            if (!(handler is RyzomClient ryzomClient))
                return "Command handler is not a Ryzom client.";

            var args = GetArgs(command);

            if (args.Length != 1 || !byte.TryParse(args[0], out var nOpt))
            {
                return "Wrong argument count or argument could not be parsed.";
            }

            // Get the bot UID
            var entityManager = ryzomClient.GetNetworkManager().GetEntityManager();

            if (entityManager == null)
                return "Entity manager is null.";

            var user = entityManager.UserEntity;

            if (user == null)
                return "User entity is null.";

            var entity = entityManager.GetEntity(user.TargetSlot());

            if (entity == null)
                return "Target entity is null.";

            var nBotUID = entity.DataSetId();

            // Create the message for the server
            const string sMsg = "BOTCHAT:DYNCHAT_SEND";
            var outStream = new BitMemoryStream();
            if (ryzomClient.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(sMsg, outStream))
            {
                outStream.Serial(ref nBotUID);
                outStream.Serial(ref nOpt);
                ryzomClient.GetNetworkManager().Push(outStream);
                return "Dynamic chat option sent.";
            }
            else
            {
                return $"Warning: unknown message name '{sMsg}'.";
            }
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return Array.Empty<string>();
        }
    }
}