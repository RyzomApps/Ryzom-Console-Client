using RCC.Config;
using RCC.Messages;
using RCC.Network;

namespace RCC.Client
{
    /// <summary>
    ///     Launch the game given a slot (slot is reference to the character summaries)
    /// </summary>
    public static class ActionHandlerLaunchGame //: IActionHandler
    {
        /// <summary>
        ///     Gets the player selected slot and sends it to the server
        /// </summary>
        public static void Execute(string sSlot)
        {
            // Get the player selected slot
            if (sSlot != "ingame_auto")
            {
                Connection.PlayerSelectedSlot = (byte) int.Parse(sSlot); //result.getInteger());
                if (Connection.PlayerSelectedSlot >= Connection.CharacterSummaries.Count)
                    return;

                ClientConfig.SelectCharacter = Connection.PlayerSelectedSlot;
            }

            // Select the right sheet to create the user character.
            ClientConfig.UserSheet = Connection.CharacterSummaries[Connection.PlayerSelectedSlot].SheetId.ToString();

            // Send CONNECTION:SELECT_CHAR
            var out2 = new BitMemoryStream(false, 2);
            GenericMessageHeaderManager.PushNameToStream("CONNECTION:SELECT_CHAR", out2);

            var c = Connection.PlayerSelectedSlot;
            out2.Serial(ref c);
            NetworkManager.Push(out2);

            RyzomClient.Log?.Info("impulseCallBack : CONNECTION:SELECT_CHAR '" + Connection.PlayerSelectedSlot +
                                  "' sent.");

            Connection.WaitServerAnswer = true;
        }
    }
}