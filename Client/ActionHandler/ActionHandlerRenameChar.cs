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
/// Ask the server to rename a character
/// </summary>
public class ActionHandlerRenameChar : ActionHandlerBase
{
    /// <summary>
    /// Constructor
    /// </summary>
    public ActionHandlerRenameChar(IClient client) : base(client) { }

    /// <summary>
    /// Execute the answer to the action
    /// </summary>
    public override void Execute(object caller, string parameters)
    {
        // Extract parameters
        var slot = byte.Parse(GetParam(parameters, "slot")); // u8
        var name = GetParam(parameters, "name"); // s
        var surname = GetParam(parameters, "surname"); // s


        var ryzomClient = (RyzomClient)_client;
        var networkManager = ryzomClient.GetNetworkManager();
        var @out = new BitMemoryStream();

        if (!networkManager.GetMessageHeaderManager().PushNameToStream("CONNECTION:RENAME_CHAR", @out))
        {
            ryzomClient.GetLogger().Error("Don't know message name CONNECTION:RENAME_CHAR");
            return;
        }

        // Create the message to send to the server for renaming
        @out.Serial(ref slot);
        @out.Serial(ref name, false);
        @out.Serial(ref surname, false);

        networkManager.Push(@out);
        networkManager.Send(networkManager.GetNetworkConnection().GetCurrentServerTick());

        networkManager.WaitServerAnswer = true;

        ryzomClient.GetLogger().Debug("impulseCallBack : CONNECTION:RENAME_CHAR sent");
    }
}