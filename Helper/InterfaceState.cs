// This code is a modified version of a file from the 'Minecraft Console Client'
// <https://github.com/ORelio/Minecraft-Console-Client>,
// which is released under CDDL-1.0 License.
// <http://opensource.org/licenses/CDDL-1.0>
// Original Copyright 2021 by ORelio and Contributers

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