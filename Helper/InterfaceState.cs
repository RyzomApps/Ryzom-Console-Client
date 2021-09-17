namespace RCC.Helper
{
    public enum InterfaceState
    {
        AutoLogin, // -> GLOBAL_MENU, QUIT (if connection errors)
        GlobalMenu, // -> SELECT_CHARACTER, QUIT (if connection errors)
        GoInTheGame, // -> launch the game
        QuitTheGame // -> quit the game
    };
}