///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using API;
using Client.Stream;

namespace Client.ActionHandler;

/// <summary>
/// Ask the server to delete a character
/// </summary>
public class ActionHandlerDeleteChar : ActionHandlerBase
{
    /// <summary>
    /// Constructor
    /// </summary>
    public ActionHandlerDeleteChar(IClient client) : base(client) { }

    /// <summary>
    /// Execute the request to delete a character
    /// </summary>
    public override void Execute(object caller, string parameters)
    {
        // Extract parameters
        var slot = byte.Parse(GetParam(parameters, "slot")); // u8

        var ryzomClient = (RyzomClient)_client;
        var networkManager = ryzomClient.GetNetworkManager();
        var @out = new BitMemoryStream();

        if (!networkManager.GetMessageHeaderManager().PushNameToStream("CONNECTION:DELETE_CHAR", @out))
        {
            ryzomClient.GetLogger().Error("Don't know message name CONNECTION:DELETE_CHAR");
            return;
        }

        // Create the message to send to the server for deleting the character
        @out.Serial(ref slot);

        networkManager.Push(@out);
        networkManager.Send(networkManager.GetNetworkConnection().GetCurrentServerTick());

        networkManager.WaitServerAnswer = true;

        ryzomClient.GetLogger().Debug("impulseCallBack : CONNECTION:DELETE_CHAR sent");
    }
}