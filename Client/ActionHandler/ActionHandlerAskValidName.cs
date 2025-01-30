///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using API;
using Client.Stream;

namespace Client.ActionHandler
{
    /// <summary>
    /// Ask the server if the name is not already used
    /// </summary>
    public class ActionHandlerAskValidName : ActionHandlerBase
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ActionHandlerAskValidName(IClient client) : base(client) { }

        /// <summary>
        /// Execute the request to check if the character name is valid
        /// </summary>
        public override void Execute(object caller, string parameters)
        {
            var sName = GetParam(parameters, "name");

            var ryzomClient = (RyzomClient)_client;

            if (string.IsNullOrEmpty(sName))
                return;

            // Ask the server
            ryzomClient.GetNetworkManager().CharNameValid = true;

            // Check for space issue
            if (sName.Contains(' '))
                ryzomClient.GetNetworkManager().CharNameValid = false;

            if (ryzomClient.GetNetworkManager().CharNameValid)
            {
                var @out = new BitMemoryStream();

                if (!ryzomClient.GetNetworkManager().GetMessageHeaderManager().PushNameToStream("CONNECTION:ASK_NAME", @out))
                {
                    ryzomClient.GetLogger().Error("Don't know message name CONNECTION:ASK_NAME");
                    return;
                }

                @out.Serial(ref sName, false);
                var homeSessionId = (uint)ryzomClient.GetNetworkManager().MainlandSelected;
                @out.Serial(ref homeSessionId);

                ryzomClient.GetNetworkManager().Push(@out);
                ryzomClient.GetNetworkManager().Send(ryzomClient.GetNetworkManager().GetCurrentServerTick());

                ryzomClient.GetNetworkManager().CharNameValidArrived = false;
            }
            else
            {
                ryzomClient.GetNetworkManager().CharNameValidArrived = true;
            }

            ryzomClient.GetNetworkManager().WaitServerAnswer = true;
        }
    }
}