///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////
using API;
using API.Commands;
using Client.Stream;
using System;
using System.Collections.Generic;

namespace Client.Commands
{
    /// <summary>
    /// Cancel the top phrase being executed
    /// </summary>
    public class CancelTopPhrase : CommandBase
    {

        public override CommandCategory CmdCategory => CommandCategory.Combat;
        public override string CmdName => "cancelTopPhrase";

        public override string CmdUsage => "";

        public override string CmdDesc => "Cancel the top phrase being executed";

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            // no parameter needed
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            // Create the message for the server to cancel the top phrase being executed
            const string msgName = "SENTENCE:CANCEL_TOP";

            var @out = new BitMemoryStream();

            if (ryzomClient.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(msgName, @out))
            {
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
