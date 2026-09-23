using API;
using API.Commands;
using Client.Stream;
using System.Collections.Generic;

namespace Client.Commands
{
    public class DynchatClickOption : CommandBase
    {
        public override string CmdName => "DynchatClickOption";

        public override string CmdUsage => "<index>";

        public override string CmdDesc => "Execution of dynamic chat option clicks by sending the selected option to the bot and closing the chat bubble.";

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
            {
                responseMsg = "Command handler is not a Ryzom client.";
                return false;
            }

            var args = GetArgs(command);

            if (args.Length != 1 || !byte.TryParse(args[0], out var nOpt))
            {
                responseMsg = "Wrong argument count or argument could not be parsed.";
                return false;
            }

            // Get the bot UID
            var entityManager = ryzomClient.GetNetworkManager().GetEntityManager();

            if (entityManager == null)
            {
                responseMsg = "Entity manager is null.";
                return false;
            }

            var user = entityManager.UserEntity;

            if (user == null)
            {
                responseMsg = "User entity is null.";
                return false;
            }

            var entity = entityManager.GetEntity(user.TargetSlot());

            if (entity == null)
            {
                responseMsg = "Target entity is null.";
                return true;
            }

            var nBotUid = entity.DataSetId();

            // Create the message for the server
            const string sMsg = "BOTCHAT:DYNCHAT_SEND";
            var outStream = new BitMemoryStream();
            if (ryzomClient.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(sMsg, outStream))
            {
                outStream.Serial(ref nBotUid);
                outStream.Serial(ref nOpt);
                ryzomClient.GetNetworkManager().Push(outStream);

                responseMsg = "Dynamic chat option sent.";
                return true;
            }

            responseMsg = $"Warning: unknown message name '{sMsg}'.";
            return false;
        }
    }
}