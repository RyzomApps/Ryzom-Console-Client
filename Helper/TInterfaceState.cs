namespace RCC
{
    public enum TInterfaceState
    {
        AUTO_LOGIN,         // -> GLOBAL_MENU, QUIT (if connection errors)
        GLOBAL_MENU,        // -> SELECT_CHARACTER, QUIT (if connection errors)
        GOGOGO_IN_THE_GAME, // -> launch the game
        QUIT_THE_GAME       // -> quit the game
    };
}