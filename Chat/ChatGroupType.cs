namespace RCC.Chat
{
    /// <summary>
    ///     Type of the chat channel (e.g. around, region, dynamic).
    /// </summary>
    public enum ChatGroupType : byte
    {
        Say = 0,
        Shout,
        Team,
        Guild,
        Civilization,
        Territory,
        Universe,
        Tell,
        Player,
        Around,
        System,
        Region,
        DynChat,

        NbChatMode
        // Following mode are client side only. Thus, after 'nbChatMode'
    };
}