using System.Collections.Generic;

namespace RCC.Client
{
    /// <summary>
    ///     Stores information about the connection to the ryzom server.
    /// </summary>
    public static class Connection
    {
        public static byte PlayerSelectedSlot = 0;
        public static byte ServerPeopleActive = 255;
        public static byte ServerCareerActive = 255;

        public static List<CharacterSummary> CharacterSummaries = new List<CharacterSummary>();
        public static bool WaitServerAnswer;

        public static bool GameExit = false;

        public static bool UserChar;
        public static bool NoUserChar;
        public static bool ConnectInterf;
        public static bool CreateInterf;
        public static bool CharacterInterf;

        // non ryzom variables (for workarounds)
        public static bool AutoSendCharSelection = false;
    }
}